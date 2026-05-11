using Assets.Scripts.Data;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.MapScene
{
    /// <summary>
    /// MonoBehaviour component for ship GameObject - handles Unity-specific logic
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class ShipObject : MonoBehaviour, IClickable
    {
        private static WaitForSecondsRealtime _waitForSecondsRealtime1 = new WaitForSecondsRealtime(1f);
        private Ship ship;
        private NavMeshAgent navAgent;
        private LineRenderer destinationLine;
        private Material destinationLineMaterial;
        private static Texture2D dashedLineTexture;
        private Coroutine initializeRoutine;

        [SerializeField] private float destinationLineWidth = 0.2f;
        [SerializeField] private float destinationDashLength = 1.25f;
        [SerializeField, Range(0.1f, 0.9f)] private float destinationDashFill = 0.55f;

        [SerializeField] private Color destinationLineColor = new(1f, 0.85f, 0.2f, 0.9f);

        public void Initialize(Ship ship)
        {
            this.ship = ship;
            ship.ShipObject = this;
            ResolveSelectionVisuals();
            ShipManager.instance.OnSelectionChanged += HandleSelectionChanged;

            // Initialize NavMeshAgent
            navAgent = GetComponent<NavMeshAgent>();
            navAgent.speed = ship.speed;
            destinationLine = GetOrCreateDestinationLine();
            
            // Restore state from Ship data
            transform.SetPositionAndRotation(ship.SavedPosition, ship.SavedRotation);

            // Restore destination if there was one
            if (ship.Destination.HasValue && navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.SetDestination(ship.Destination.Value);
            }

            RefreshDestinationLine();
            HandleSelectionChanged(ShipManager.instance.SelectedShip);
        }

        private void OnDestroy()
        {
            if (initializeRoutine != null)
            {
                StopCoroutine(initializeRoutine);
                initializeRoutine = null;
            }

            // Save state when leaving MapScene
            if (ship != null)
            {
                ship.SavedPosition = transform.position;
                ship.SavedRotation = transform.rotation;
                ship.ShipObject = null; // Clear reference
            }

            if (destinationLineMaterial != null)
            {
                Destroy(destinationLineMaterial);
        }

            ShipManager.instance.OnSelectionChanged -= HandleSelectionChanged;
        }

        public void OnClicked(RaycastHit hit, int mouseButton)
        {
            ship?.OnClicked(mouseButton);
        }

        private bool IsSelected => ship == ShipManager.instance.SelectedShip;

        [SerializeField] private MeshRenderer selectionCylinderRenderer;
        [SerializeField] private SpriteRenderer selectionSpriteRenderer;

        private void Update()
        {
            UpdatePosition();
            RefreshDestinationLine();
        }

        private bool firstUpdate = true;

        private void UpdatePosition()
        {
            if (navAgent == null || ship == null) return;

            // Check if we've reached the destination
            if (ship.Destination.HasValue && !navAgent.pathPending)
            {
                if (firstUpdate)
                {
                    Debug.Log($"Updating position for ship {ship.Name} to ({transform.position.x}, {transform.position.z})");
                    firstUpdate = false;
                }
                if (navAgent.remainingDistance <= navAgent.stoppingDistance)
                {
                    if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                    {
                        ship.OnArriveToDestination();
                    }
                }
            }
        }

        public void UpdateDestination(Vector3 destination)
        {
            gameObject.SetActive(true);

            Vector3 destInPlane = new(destination.x, transform.position.y, destination.z);

            // Set NavMeshAgent destination
            if (navAgent != null)
            {
                navAgent.destination = destInPlane;
            }

            RefreshDestinationLine();
        }

        private LineRenderer GetOrCreateDestinationLine()
        {
            if (!ship.userOwned)
            {
                return null;
            }

            if (destinationLine != null)
            {
                return destinationLine;
            }

            if (!TryGetComponent(out destinationLine))
            {
                destinationLine = gameObject.AddComponent<LineRenderer>();
            }

            destinationLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            destinationLine.receiveShadows = false;
            destinationLine.textureMode = LineTextureMode.Tile;
            destinationLine.alignment = LineAlignment.View;
            destinationLine.widthMultiplier = destinationLineWidth;
            destinationLine.positionCount = 2;
            destinationLine.enabled = false;
            destinationLine.loop = false;
            destinationLine.useWorldSpace = true;
            destinationLine.startColor = destinationLineColor;
            destinationLine.endColor = destinationLineColor;
            destinationLineMaterial = CreateDestinationLineMaterial();
            destinationLine.sharedMaterial = destinationLineMaterial;

            return destinationLine;
        }

        private Material CreateDestinationLineMaterial()
        {
            if (destinationLineMaterial != null)
            {
                return destinationLineMaterial;
            }

            Shader shader = Shader.Find("Sprites/Default");
            destinationLineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave,
                mainTexture = GetDashedLineTexture()
            };
            destinationLineMaterial.color = Color.white;

            return destinationLineMaterial;
        }

        private Texture2D GetDashedLineTexture()
        {
            if (dashedLineTexture != null)
            {
                return dashedLineTexture;
            }

            const int textureWidth = 16;
            dashedLineTexture = new Texture2D(textureWidth, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point,
                hideFlags = HideFlags.HideAndDontSave
            };

            int filledPixels = Mathf.Clamp(Mathf.RoundToInt(textureWidth * destinationDashFill), 1, textureWidth - 1);
            for (int i = 0; i < textureWidth; i++)
            {
                dashedLineTexture.SetPixel(i, 0, i < filledPixels ? Color.white : Color.clear);
            }

            dashedLineTexture.Apply(false, true);
            return dashedLineTexture;
        }

        private void RefreshDestinationLine()
        {
            if (ship == null || navAgent == null || !ship.userOwned)
            {
                return;
            }

            LineRenderer line = GetOrCreateDestinationLine();
            bool hasDestination = ship.Destination.HasValue;
            line.enabled = hasDestination;

            if (!hasDestination)
            {
                return;
            }

            line.widthMultiplier = destinationLineWidth;
            line.startColor = destinationLineColor;
            line.endColor = destinationLineColor;
            CreateDestinationLineMaterial();

            Vector3 heightOffset = Vector3.up * 0.1f;
            Vector3[] corners = navAgent.hasPath ? navAgent.path.corners : null;

            if (corners != null && corners.Length > 1)
            {
                line.positionCount = corners.Length;
                for (int i = 0; i < corners.Length; i++)
                {
                    line.SetPosition(i, corners[i] + heightOffset);
                }

                UpdateDestinationLineTiling(line, corners, corners.Length);

                return;
            }

            line.positionCount = 2;
            Vector3 start = transform.position + heightOffset;
            Vector3 end = ship.Destination.Value + heightOffset;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            UpdateDestinationLineTiling(line, new[] { start, end }, 2);
        }

        private void UpdateDestinationLineTiling(LineRenderer line, Vector3[] points, int pointCount)
        {
            if (destinationLineMaterial == null)
            {
                return;
            }

            float pathLength = 0f;
            for (int i = 0; i < pointCount - 1; i++)
            {
                pathLength += Vector3.Distance(points[i], points[i + 1]);
            }

            float repeatCount = Mathf.Max(1f, pathLength / Mathf.Max(0.05f, destinationDashLength));
            destinationLineMaterial.mainTextureScale = new Vector2(repeatCount, 1f);
        }

        private void ResolveSelectionVisuals()
        {
            if (selectionCylinderRenderer == null)
            {
                Transform cylinderTransform = transform.Find("Cylinder");
                if (cylinderTransform != null)
                {
                    selectionCylinderRenderer = cylinderTransform.GetComponent<MeshRenderer>();
                }
            }

            if (selectionSpriteRenderer == null)
            {
                Transform spriteTransform = transform.Find("Square");
                if (spriteTransform != null)
                {
                    selectionSpriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
                }
            }
        }

        private void HandleSelectionChanged(Ship selectedShip)
        {
            if (selectionCylinderRenderer != null)
            {
                selectionCylinderRenderer.enabled = IsSelected;
            }

            if (selectionSpriteRenderer != null)
            {
                selectionSpriteRenderer.enabled = ship.userOwned;
            }

            RefreshDestinationLine();
        }
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Scripts.CityScene
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class CityCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Camera targetCamera;

        [Header("Default View")]
        [SerializeField] Vector3 defaultFocusPoint = new Vector3(-70f, 22f, 150f);
        [SerializeField] float defaultYaw = 0f;
        [SerializeField] float defaultPitch = 28f;
        [SerializeField] float defaultDistance = 235f;
        [SerializeField] float defaultFov = 42f;
        [SerializeField] bool previewDefaultViewInEditMode;

        [Header("Default View Interaction")]
        [SerializeField] float sidePanRange = 22f;
        [SerializeField] float dragPanSpeed = 0.065f;
        [SerializeField] float tiltRange = 3f;
        [SerializeField] float dragTiltSpeed = 0.018f;
        [SerializeField] bool enableEdgePush = true;
        [SerializeField] float edgePushZonePixels = 64f;
        [SerializeField] float edgePanRange = 8f;
        [SerializeField] float edgeTiltRange = 1.8f;
        [SerializeField] float edgePushSmoothTime = 0.18f;
        [SerializeField] float zoomRange = 0.12f;
        [SerializeField] float zoomSpeed = 0.12f;

        [Header("Building Focus")]
        [SerializeField] float selectedPitch = 24f;
        [SerializeField] float selectedFov = 34f;
        [SerializeField] float selectedDistanceMultiplier = 1.35f;
        [SerializeField] float selectedMinDistance = 34f;
        [SerializeField] float selectedMaxDistance = 120f;
        [SerializeField] float focusYOffset = 1.5f;

        [Header("Motion")]
        [SerializeField] float positionSmoothTime = 0.28f;
        [SerializeField] float rotationSmoothTime = 0.22f;
        [SerializeField] float fovSmoothTime = 0.18f;

        Vector3 desiredFocus;
        Vector3 currentPositionVelocity;
        float desiredYaw;
        float desiredPitch;
        float desiredDistance;
        float desiredFov;
        float currentYaw;
        float currentPitch;
        float currentFov;
        float yawVelocity;
        float pitchVelocity;
        float fovVelocity;
        Vector3 desiredCameraPosition;
        Quaternion desiredCameraRotation = Quaternion.identity;
        float sidePanOffset;
        float tiltOffset;
        float edgePanOffset;
        float edgeTiltOffset;
        float edgePanVelocity;
        float edgeTiltVelocity;
        float zoomOffset;
        Vector2 lastDragPosition;
        CityBuilding focusedBuilding;
        bool isDragging;
        bool isBuildingFocused;
        bool useExplicitCameraPose;

        public Camera Camera => targetCamera;
        public bool IsBuildingFocused => isBuildingFocused;

        void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            if (previewDefaultViewInEditMode)
            {
                ApplyDefaultViewImmediate();
            }
        }

        void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            if (!Application.isPlaying)
            {
                if (previewDefaultViewInEditMode)
                {
                    ApplyDefaultViewImmediate();
                }

                return;
            }

            desiredFocus = defaultFocusPoint;
            desiredYaw = defaultYaw;
            desiredPitch = defaultPitch;
            desiredDistance = defaultDistance;
            desiredFov = defaultFov;
            currentYaw = desiredYaw;
            currentPitch = desiredPitch;
            currentFov = desiredFov;

            PlaceCameraImmediate();
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            HandleDefaultViewInput();
            UpdateDesiredDefaultView();
            SmoothToDesiredView();
        }

        public void ConfigureDefaultView(Vector3 focusPoint, float yaw, float pitch, float distance, float fov)
        {
            defaultFocusPoint = focusPoint;
            defaultYaw = yaw;
            defaultPitch = pitch;
            defaultDistance = distance;
            defaultFov = fov;
            ReturnToCity(true);
        }

        public void FocusBuilding(CityBuilding building)
        {
            if (building == null)
            {
                ReturnToCity();
                return;
            }

            if (building.HasCameraView)
            {
                focusedBuilding = building;
                desiredCameraPosition = building.CameraView.position;
                desiredCameraRotation = building.CameraView.rotation;
                desiredFov = building.CameraViewFov;
                useExplicitCameraPose = true;
                isBuildingFocused = true;
                isDragging = false;
                return;
            }

            Bounds bounds = building.GetWorldBounds();
            float radius = bounds.extents.magnitude;
            float fitDistance = radius / Mathf.Sin(selectedFov * Mathf.Deg2Rad * 0.5f);
            fitDistance = Mathf.Clamp(fitDistance * selectedDistanceMultiplier, selectedMinDistance, selectedMaxDistance);

            desiredFocus = building.GetFocusPoint() + Vector3.up * focusYOffset;
            desiredYaw = defaultYaw;
            desiredPitch = selectedPitch;
            desiredDistance = fitDistance;
            desiredFov = selectedFov;
            focusedBuilding = building;
            useExplicitCameraPose = false;
            isBuildingFocused = true;
            isDragging = false;
        }

        public void ReturnToCity(bool immediate = false)
        {
            isBuildingFocused = false;
            focusedBuilding = null;
            useExplicitCameraPose = false;
            sidePanOffset = 0f;
            tiltOffset = 0f;
            edgePanOffset = 0f;
            edgeTiltOffset = 0f;
            zoomOffset = 0f;
            UpdateDesiredDefaultView();

            if (immediate)
            {
                PlaceCameraImmediate();
            }
        }

        void HandleDefaultViewInput()
        {
            if (isBuildingFocused)
            {
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                SmoothEdgePushOffset(0f, 0f);
                return;
            }

            Vector2 pointer = mouse.position.ReadValue();
            bool pointerOverUi = IsPointerOverUi();
            UpdateEdgePushOffset(pointer, pointerOverUi);

            if (!pointerOverUi)
            {
                float scroll = mouse.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    zoomOffset = Mathf.Clamp(zoomOffset - Mathf.Sign(scroll) * zoomSpeed, -zoomRange, zoomRange);
                }
            }

            if (mouse.middleButton.wasPressedThisFrame && !pointerOverUi)
            {
                isDragging = true;
                lastDragPosition = pointer;
            }
            else if (mouse.middleButton.wasReleasedThisFrame)
            {
                isDragging = false;
            }

            if (isDragging && mouse.middleButton.isPressed)
            {
                Vector2 delta = pointer - lastDragPosition;
                sidePanOffset = Mathf.Clamp(sidePanOffset - delta.x * dragPanSpeed, -sidePanRange, sidePanRange);
                tiltOffset = Mathf.Clamp(tiltOffset + delta.y * dragTiltSpeed, -tiltRange, tiltRange);
                lastDragPosition = pointer;
            }
        }

        void UpdateDesiredDefaultView()
        {
            if (isBuildingFocused)
            {
                return;
            }

            Quaternion yawRotation = Quaternion.Euler(0f, defaultYaw, 0f);
            float totalPanOffset = sidePanOffset + edgePanOffset;
            float totalTiltOffset = tiltOffset + edgeTiltOffset;

            desiredFocus = defaultFocusPoint + yawRotation * Vector3.right * totalPanOffset;
            desiredYaw = defaultYaw;
            desiredPitch = Mathf.Clamp(defaultPitch + totalTiltOffset, -25f, 80f);
            desiredDistance = defaultDistance * (1f + zoomOffset);
            desiredFov = defaultFov;
        }

        void SmoothToDesiredView()
        {
            if (targetCamera == null)
            {
                return;
            }

            if (useExplicitCameraPose)
            {
                if (focusedBuilding != null && focusedBuilding.HasCameraView)
                {
                    desiredCameraPosition = focusedBuilding.CameraView.position;
                    desiredCameraRotation = focusedBuilding.CameraView.rotation;
                    desiredFov = focusedBuilding.CameraViewFov;
                }

                currentFov = Mathf.SmoothDamp(currentFov, desiredFov, ref fovVelocity, fovSmoothTime);
                transform.position = Vector3.SmoothDamp(transform.position, desiredCameraPosition, ref currentPositionVelocity, positionSmoothTime);

                float rotationT = rotationSmoothTime <= 0f ? 1f : 1f - Mathf.Exp(-Time.deltaTime / rotationSmoothTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredCameraRotation, rotationT);
                targetCamera.fieldOfView = currentFov;
                return;
            }

            currentYaw = Mathf.SmoothDampAngle(currentYaw, desiredYaw, ref yawVelocity, rotationSmoothTime);
            currentPitch = Mathf.SmoothDampAngle(currentPitch, desiredPitch, ref pitchVelocity, rotationSmoothTime);
            currentFov = Mathf.SmoothDamp(currentFov, desiredFov, ref fovVelocity, fovSmoothTime);

            Vector3 targetPosition = CalculateCameraPosition(desiredFocus, currentYaw, currentPitch, desiredDistance);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentPositionVelocity, positionSmoothTime);
            transform.rotation = Quaternion.LookRotation(desiredFocus - transform.position, Vector3.up);
            targetCamera.fieldOfView = currentFov;
        }

        void PlaceCameraImmediate()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            if (targetCamera == null)
            {
                return;
            }

            currentYaw = desiredYaw;
            currentPitch = desiredPitch;
            currentFov = desiredFov;
            transform.position = CalculateCameraPosition(desiredFocus, currentYaw, currentPitch, desiredDistance);
            transform.rotation = Quaternion.LookRotation(desiredFocus - transform.position, Vector3.up);
            targetCamera.fieldOfView = currentFov;
        }

        [ContextMenu("Apply Default View Now")]
        public void ApplyDefaultViewImmediate()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            isBuildingFocused = false;
            focusedBuilding = null;
            useExplicitCameraPose = false;
            desiredFocus = defaultFocusPoint;
            desiredYaw = defaultYaw;
            desiredPitch = defaultPitch;
            desiredDistance = defaultDistance;
            desiredFov = defaultFov;
            sidePanOffset = 0f;
            tiltOffset = 0f;
            edgePanOffset = 0f;
            edgeTiltOffset = 0f;
            zoomOffset = 0f;
            currentPositionVelocity = Vector3.zero;
            yawVelocity = 0f;
            pitchVelocity = 0f;
            fovVelocity = 0f;
            PlaceCameraImmediate();

#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        [ContextMenu("Capture Current Camera As Default View")]
        public void CaptureCurrentCameraAsDefaultView()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            Vector3 offset = transform.position - defaultFocusPoint;
            if (offset.sqrMagnitude < 0.001f)
            {
                return;
            }

            Quaternion lookRotation = Quaternion.LookRotation(defaultFocusPoint - transform.position, Vector3.up);
            Vector3 euler = lookRotation.eulerAngles;
            defaultYaw = NormalizeAngle(euler.y);
            defaultPitch = NormalizeAngle(euler.x);
            defaultDistance = offset.magnitude;
            if (targetCamera != null)
            {
                defaultFov = targetCamera.fieldOfView;
            }

            desiredFocus = defaultFocusPoint;
            desiredYaw = defaultYaw;
            desiredPitch = defaultPitch;
            desiredDistance = defaultDistance;
            desiredFov = defaultFov;
            currentYaw = defaultYaw;
            currentPitch = defaultPitch;
            currentFov = defaultFov;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            if (targetCamera != null)
            {
                UnityEditor.EditorUtility.SetDirty(targetCamera);
            }

            UnityEditor.SceneView.RepaintAll();
#endif
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/City/Capture Main Camera As Default City View")]
        static void CaptureMainCameraAsDefaultCityView()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                UnityEngine.Debug.LogError("No Main Camera found to capture as the default city view.");
                return;
            }

            CityCameraController controller = camera.GetComponent<CityCameraController>();
            if (controller == null)
            {
                UnityEngine.Debug.LogError("Main Camera does not have a CityCameraController.");
                return;
            }

            UnityEditor.Undo.RecordObject(controller, "Capture Main Camera As Default City View");
            controller.CaptureCurrentCameraAsDefaultView();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            UnityEngine.Debug.Log("Captured the current Main Camera pose/FOV as the default city view.");
        }
#endif

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.78f, 0.28f, 0.9f);
            Gizmos.DrawWireSphere(defaultFocusPoint, 2.5f);
            Gizmos.DrawLine(transform.position, defaultFocusPoint);
        }

        void UpdateEdgePushOffset(Vector2 pointer, bool pointerOverUi)
        {
            if (!enableEdgePush || pointerOverUi || Screen.width <= 0 || Screen.height <= 0)
            {
                SmoothEdgePushOffset(0f, 0f);
                return;
            }

            float zone = Mathf.Max(1f, edgePushZonePixels);
            float targetPan = 0f;
            float targetTilt = 0f;

            if (pointer.x <= zone)
            {
                targetPan = -Mathf.Clamp01((zone - pointer.x) / zone) * edgePanRange;
            }
            else if (pointer.x >= Screen.width - zone)
            {
                targetPan = Mathf.Clamp01((pointer.x - (Screen.width - zone)) / zone) * edgePanRange;
            }

            if (pointer.y <= zone)
            {
                targetTilt = Mathf.Clamp01((zone - pointer.y) / zone) * edgeTiltRange;
            }
            else if (pointer.y >= Screen.height - zone)
            {
                targetTilt = -Mathf.Clamp01((pointer.y - (Screen.height - zone)) / zone) * edgeTiltRange;
            }

            SmoothEdgePushOffset(targetPan, targetTilt);
        }

        void SmoothEdgePushOffset(float targetPan, float targetTilt)
        {
            float smoothTime = Mathf.Max(0.001f, edgePushSmoothTime);
            edgePanOffset = Mathf.SmoothDamp(edgePanOffset, targetPan, ref edgePanVelocity, smoothTime);
            edgeTiltOffset = Mathf.SmoothDamp(edgeTiltOffset, targetTilt, ref edgeTiltVelocity, smoothTime);
        }

        static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        static Vector3 CalculateCameraPosition(Vector3 focus, float yaw, float pitch, float distance)
        {
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            return focus + rotation * new Vector3(0f, 0f, -distance);
        }

        static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f)
            {
                angle -= 360f;
            }
            else if (angle < -180f)
            {
                angle += 360f;
            }

            return angle;
        }
    }
}

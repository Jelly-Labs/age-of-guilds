using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.MapScene
{
    public class MapSceneManager : MonoBehaviour
    {
        [Header("Prefab References")]
        public GameObject shipPrefab;

        [Header("Town Label Overlay")]
        [SerializeField] Vector3 townLabelWorldOffset = new Vector3(0f, 4.25f, 0f);

        TownWorldLabelOverlay townWorldLabelOverlay;
        MapShipCargoPanel shipCargoPanel;

        private void Start()
        {
            TownObject[] townObjects = InitializeTowns();
            InitializeTownLabels(townObjects);
            InitializeShipCargoPanel();
            RestoreShips();
        }

        private TownObject[] InitializeTowns()
        {
            // Find all TownObjects in the scene
            TownObject[] townObjects = FindObjectsByType<TownObject>();

            foreach (var townObject in townObjects)
            {
                // Match TownObject to Town by ID
                if (townObject.townID >= 0 && townObject.townID < GameData.instance.towns.Count)
                {
                    Town town = GameData.instance.towns[townObject.townID];
                    townObject.Initialize(town);
                }
                else
                {
                    Debug.LogWarning($"TownObject has invalid townID: {townObject.townID}. Valid range: 0-{GameData.instance.towns.Count - 1}");
                }
            }

            return townObjects;
        }

        private void InitializeTownLabels(TownObject[] townObjects)
        {
            if (townWorldLabelOverlay == null)
            {
                townWorldLabelOverlay = GetComponent<TownWorldLabelOverlay>();
            }

            if (townWorldLabelOverlay == null)
            {
                townWorldLabelOverlay = gameObject.AddComponent<TownWorldLabelOverlay>();
            }

            townWorldLabelOverlay.Initialize(
                townLabelWorldOffset,
                townObjects);
        }

        private void InitializeShipCargoPanel()
        {
            if (shipCargoPanel == null)
            {
                shipCargoPanel = GetComponent<MapShipCargoPanel>();
            }

            if (shipCargoPanel == null)
            {
                shipCargoPanel = gameObject.AddComponent<MapShipCargoPanel>();
            }

            shipCargoPanel.Initialize();
        }

        private void RestoreShips()
        {
            if (shipPrefab == null)
            {
                Debug.LogWarning("MapSceneManager: Ship prefab not assigned!");
                return;
            }

            // Restore all ships from saved state
            Debug.Log($"Restoring {GameData.instance.ships.Count} ships in MapScene");
            foreach (var ship in GameData.instance.ships)
            {
                var result = NavMesh.SamplePosition(ship.SavedPosition, out var hit, 100f, NavMesh.AllAreas);
                if (result)
                {
                    ship.SavedPosition = new Vector3(hit.position.x, ship.SavedPosition.y, hit.position.z);
                }
                else
                {
                    while(true)
                    {
                        ship.SavedPosition = new Vector3(UnityEngine.Random.Range(0, 100), 0.3f, UnityEngine.Random.Range(0, 100));
                        if (NavMesh.SamplePosition(ship.SavedPosition, out hit, 100f, NavMesh.AllAreas))
                        {
                            ship.SavedPosition = new Vector3(hit.position.x, ship.SavedPosition.y, hit.position.z);
                            break;
                        }
                    }
                }
                GameObject shipGameObject = Instantiate(shipPrefab, ship.SavedPosition, ship.SavedRotation);
                
                if (shipGameObject.TryGetComponent<ShipObject>(out var shipObject))
                {
                    shipObject.Initialize(ship);
                }
            }
        }

        private void Update()
        {
            GameData.instance.UpdateTime(Time.deltaTime);
            ShipManager.instance.UpdateDestinations();
        }
    }
}

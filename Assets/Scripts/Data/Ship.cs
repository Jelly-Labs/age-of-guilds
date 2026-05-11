using Assets.Scripts.Data.market;
using Assets.Scripts.MapScene;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Pure data class representing a ship's state - persists across scenes
    /// </summary>
    public class Ship
    {
        public static int NextID = 0;
        public readonly int ID = NextID++;

        public Vector3? Destination { get; set; }
        public Town DestinationTown { get; set; }

        // Saved state for scene transitions
        public Vector3 SavedPosition { get; set; }
       
        public Quaternion SavedRotation { get; set; }

        // Reference to the GameObject (null when not in MapScene)
        public ShipObject ShipObject { get; set; }

        public float speed = 1.5f;

        public FullLoad load = new();

        public bool userOwned;

        public string Name { get; set; }

        public Nullable<DateTime> timeToLeave = null;

        public Ship(string name = null, bool userOwned = false)
        {
            this.Name = name ?? $"Ship {ID}";
            this.userOwned = userOwned;
            ShipManager.instance.OnSelectionChanged += OnSelectionChanged;
        }

        ~Ship()
        {
            ShipManager.instance.OnSelectionChanged -= OnSelectionChanged;
        }

        public void UpdateDestination(Vector3 destination)
        {
            DestinationTown?.Undock(this);
            DestinationTown = null;

            Vector3 destInPlane = new(destination.x, SavedPosition.y, destination.z);
            Destination = destInPlane;

            // Delegate to ShipObject if it exists (in MapScene)
            (ShipObject ? ShipObject : null)?.UpdateDestination(destination);
            timeToLeave = null; // Clear any scheduled departure time when setting a new destination
        }

        public void UpdateDestination(Town town)
        {
            Debug.Log($"Setting new destination for ship {ID} to town {town.Name}");
            if (town?.TownObject == null)
            {
                Debug.LogWarning("Cannot set destination to town - TownObject not available");
                return;
            }

            UpdateDestination(town.TownObject.PortPosition);
            DestinationTown = town;
        }

        public void OnClicked(int mouseButton)
        {
            if (mouseButton == 0)
            {
                ShipManager.instance.SelectedShip = this;
            }
        }

        public void OnSelectionChanged(Ship selectedShip)
        {
            // Handle selection logic here if needed
            // Visual feedback is handled by ShipObject when it exists
        }

        public void OnArriveToDestination()
        {
            Destination = null;
            if (DestinationTown != null)
            {
                ShipObject.gameObject.SetActive(false);

                if (userOwned)
                {
                    DestinationTown.Dock(this);
                }
                else
                {
                    timeToLeave = GameData.instance.currentDate.AddHours(UnityEngine.Random.Range(4, 24));
                    load = new FullLoad();
                    load.RandomizeLoad();
                }
            }
        }
    }
}

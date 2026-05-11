using System;
using UnityEngine;

namespace Assets.Scripts.Data
{
	public class ShipManager
	{

		public static readonly ShipManager instance = new();

		private ShipManager() { }
		
		private Ship _selectedShip;

		public Ship SelectedShip
		{
			get { return _selectedShip; }
            set
			{
                if (_selectedShip == value)
                {
                    return;
                }

                _selectedShip = value;
                OnSelectionChanged?.Invoke(value);
            }
		}

		public void UpdateDestinations()
		{
			foreach (Ship ship in GameData.instance.ships)
			{
				if (ship.userOwned)
				{
					continue;
				}

				if (ship.ShipObject == null)
				{
                    Debug.Log($"Skipping ship {ship.Name} destination update, not initialized");
                    continue;
				}

                // If the ship has a time to leave and it's in the past, or if it doesn't have a destination town, we should update its destination.
                bool shouldUpdate = (ship.timeToLeave != null && ship.timeToLeave.Value < GameData.instance.currentDate) || ship.DestinationTown == null;
                if (!shouldUpdate)
				{
					continue;
                }

                Town town = GameData.instance.GetRandomTown();
                Debug.Log($"Updating ship {ship.Name} destination to {town.Name}");
				ship.UpdateDestination(town);
            }
        }

		public event Action<Ship> OnSelectionChanged;
	}
}
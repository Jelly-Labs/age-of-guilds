using Assets.Scripts.MapScene;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Pure data class representing a town's state - persists across scenes
    /// </summary>
    public class Town
    {
        public static int NextID = 0;
        public readonly int ID = NextID++;

        public string name;

        public Sprite banner;

        public string Name => name;

        public Market Market { get; private set; } = new ();

        public List<Ship> DockedShips { get; private set; } = new ();

        // Reference to the GameObject (null when not in MapScene)
        public TownObject TownObject { get; set; }

        public Town(string initialName = null)
        {
            name = string.IsNullOrWhiteSpace(initialName) ? $"Town {ID + 1}" : initialName;
        }

        public void Dock(Ship ship)
        {
            DockedShips.Add(ship);
        }

        public void Undock(Ship ship)
        {
            DockedShips.Remove(ship);
        }

        public void OnClicked(int mouseButton)
        {
            if (ShipManager.instance.SelectedShip != null && mouseButton == 1)
            {
                ShipManager.instance.SelectedShip.UpdateDestination(this);
            }
            else if (mouseButton == 0)
            {
                // Store the town data and load CityScene
                GameData.instance.CurrentTown = this;
                UnityEngine.SceneManagement.SceneManager.LoadScene("CityScene");
            }
        }

        public void ExecuteTrades(MarketTradePreview preview)
        {
            Market = preview.market;
        }
    }
}

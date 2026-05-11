using UnityEngine;
using System.Collections.Generic;
using System;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Singleton to hold global game data across scenes
    /// </summary>
    public class GameData
    {
        private static readonly string[] npcShipNames = new string[]
        {
            "Zuzana",
            "Liza",
            "Marija",
            "Jeca",
            "Anci",
            "Bojana",
            "Oksana",
            "Jelisaveta",
            "Mina",
            "Joanna",
            "Katarina",
            "Svetlana",
            "Marta",
            "Darija",
            "Aleksandra",
        };

        public static readonly GameData instance = new();

        private Town selectedTown;

        public event Action<Town> selectedTownChanged;

        public Town CurrentTown
        {
            get => selectedTown;
            set
            {
                if (ReferenceEquals(selectedTown, value))
                {
                    return;
                }

                selectedTown = value;
                selectedTownChanged?.Invoke(selectedTown);
            }
        }

        public readonly List<Ship> ships = new();

        public readonly List<Town> towns = new();

        public double gold = 10000;

        public int NumberOfNpcShips => 15;

        private static readonly DateTime startDate = new(1227, 8, 13);

        public DateTime currentDate = new(1227, 8, 13);

        public DateTime lastMarketUpdate = new(1227, 8, 13);

        public const double marketUpdateFrequencyHours = 12;

        private GameData()
        {
            // Initialize two towns
            towns.Add(new("Beograd")); // ID 0
            towns.Add(new("Zagreb")); // ID 1
            towns.Add(new("Ljubljana")); // ID 2
            towns.Add(new("Sarajevo")); // ID 3
            towns.Add(new("Atina")); // ID 4
            towns.Add(new("Podgorica")); // ID 5
            towns.Add(new("Skopje")); // ID 6

            Ship ship = new("Zivana", true)
            {
                SavedPosition = new Vector3(60, 0.3f, 20)
            };
            ships.Add(ship);

            for(int i = 0; i < NumberOfNpcShips; i++)
            {
                string name = npcShipNames[i % npcShipNames.Length];
                Ship npcShip = new(name, false)
                {
                    SavedPosition = new Vector3(UnityEngine.Random.Range(0, 100), 0.3f, UnityEngine.Random.Range(0, 100)),
                    speed = UnityEngine.Random.Range(1f, 3f)
                };
                npcShip.load.RandomizeLoad();
                ships.Add(npcShip);
            }
        }

        public Town GetRandomTown()
        {
            if (towns.Count == 0)
            {
                return null;
            }
            int index = UnityEngine.Random.Range(0, towns.Count);
            return towns[index];
        }

        public void UpdateTime(float deltaTime)
        {
            currentDate = currentDate.AddHours(24 * deltaTime / 60); // Time passes faster than real time
            //Debug.Log($"Current in-game date: {currentDate}");

            double timeSinceLastMarketUpdate = (currentDate - lastMarketUpdate).TotalHours;
            if (timeSinceLastMarketUpdate > marketUpdateFrequencyHours)
            {
                UpdateMarkets();
            }
        }

        private void UpdateMarkets()
        {
            foreach (var town in towns)
            {
                town.Market.Update();
            }

            lastMarketUpdate = currentDate;
        }
    }
}

using Assets.Scripts.Data;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.MapScene
{
    /// <summary>
    /// MonoBehaviour component for town GameObject - handles Unity-specific logic
    /// </summary>
    public class TownObject : MonoBehaviour, IClickable
    {
        [Header("Town Identification")]
        [Tooltip("Unique ID to match with Town data (0 or 1)")]
        public int townID = 0;

        [Header("Port Settings")]
        public Vector3 portOffset = Vector3.zero;

        public Vector3 PortPosition
        {
            get { return transform.position + portOffset; }
        }

        private Town town;

        public Town Town => town;

        public void Initialize(Town town)
        {
            this.town = town;
            town.TownObject = this;
        }

        private void OnDestroy()
        {
            if (town != null)
            {
                town.TownObject = null;
            }
        }

        public void OnClicked(RaycastHit hit, int mouseButton)
        {
            town?.OnClicked(mouseButton);
        }
    }
}

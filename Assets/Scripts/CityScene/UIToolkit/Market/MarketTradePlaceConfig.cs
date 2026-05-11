using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.CityScene
{
    [CreateAssetMenu(fileName = "MarketTradePlaceConfig", menuName = "Age of Guilds/City/Market Trade Places")]
    public class MarketTradePlaceConfig : ScriptableObject
    {
        [SerializeField] List<MarketTradePlace> places = new();

        public IReadOnlyList<MarketTradePlace> Places => places;
        public bool HasPlaces => places.Count > 0;

        public MarketTradePlace GetPlace(int index)
        {
            if (places.Count == 0)
            {
                return null;
            }

            return places[Mathf.Clamp(index, 0, places.Count - 1)];
        }

        public int IndexOf(string id)
        {
            for (int i = 0; i < places.Count; i++)
            {
                if (places[i] != null && places[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }

#if UNITY_EDITOR
        public void AddDefaultPlaces(params MarketTradePlace[] defaultPlaces)
        {
            if (places.Count > 0 || defaultPlaces == null)
            {
                return;
            }

            places.AddRange(defaultPlaces);
        }
#endif
    }

    [Serializable]
    public class MarketTradePlace
    {
        [SerializeField] string id = "place";
        [SerializeField] string displayLabel = "Place";
        [SerializeField] Sprite icon;
        [SerializeField] MarketGoodsCatalog goodsCatalog;

        public string Id => id;
        public string DisplayLabel => displayLabel;
        public Sprite Icon => icon;
        public MarketGoodsCatalog GoodsCatalog => goodsCatalog;

        public MarketTradePlace()
        {
        }

        public MarketTradePlace(string id, string displayLabel, Sprite icon, MarketGoodsCatalog goodsCatalog)
        {
            this.id = id;
            this.displayLabel = displayLabel;
            this.icon = icon;
            this.goodsCatalog = goodsCatalog;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.CityScene
{
    [CreateAssetMenu(fileName = "MarketGoodsCatalog", menuName = "Age of Guilds/City/Market Goods Catalog")]
    public class MarketGoodsCatalog : ScriptableObject
    {
        [SerializeField] List<MarketGoodEntry> goods = new();

        public IReadOnlyList<MarketGoodEntry> Goods => goods;
        public bool HasGoods => goods.Count > 0;

#if UNITY_EDITOR
        public void AddDefaultGoods(params string[] goodIds)
        {
            if (goods.Count > 0 || goodIds == null)
            {
                return;
            }

            for (int i = 0; i < goodIds.Length; i++)
            {
                goods.Add(new MarketGoodEntry(goodIds[i], null));
            }
        }
#endif
    }

    [Serializable]
    public class MarketGoodEntry
    {
        [SerializeField] string id = "good";
        [SerializeField] Sprite icon;

        public string Id => id;
        public Sprite Icon => icon;

        public MarketGoodEntry()
        {
        }

        public MarketGoodEntry(string id, Sprite icon)
        {
            this.id = id;
            this.icon = icon;
        }
    }
}

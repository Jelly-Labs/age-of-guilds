using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

namespace Assets.Scripts.Data.market
{
	public class FullLoad
	{

		public readonly Dictionary<CommodityType, CommodityLoad> load = new();

		public FullLoad Clone()
		{
			FullLoad clone = new FullLoad();
			foreach (var kvp in load)
			{
				clone.load[kvp.Key] = kvp.Value.Clone();
			}
			return clone;
        }
		
		public void AddOne(CommodityType _commodity, double _averagePrice)
		{
			if (load.TryGetValue(_commodity, out CommodityLoad existingLoad))
			{
				existingLoad.Add(1, _averagePrice);
			}
			else
			{
				load[_commodity] = new CommodityLoad(_commodity, 1, _averagePrice);
			}
		}

        public void RemoveOne(CommodityType _commodity)
        {
            if (load.TryGetValue(_commodity, out CommodityLoad existingLoad))
            {
                existingLoad.Remove(1);
            }
        }

		public void UndoAddOne(CommodityType _commodity, double _averagePrice)
		{
			if (load.TryGetValue(_commodity, out CommodityLoad existingLoad))
			{
				existingLoad.UndoAdd(1, _averagePrice);
			}
        }

		public int GetAmount(CommodityType _commodity)
		{
			if (load.TryGetValue(_commodity, out CommodityLoad existingLoad))
			{
				return existingLoad.Amount;
			}
			return 0;
        }

        public double GetAveragePrice(CommodityType _commodity)
        {
            if (load.TryGetValue(_commodity, out CommodityLoad existingLoad))
            {
                return existingLoad.AveragePrice;
            }
            return 0;
        }

		public void RandomizeLoad()
		{
			int commodityCount = UnityEngine.Random.Range(0, 4);
			for (int i = 0; i < commodityCount; i++)
			{
				CommodityType randomCommodity = (CommodityType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(CommodityType)).Length);
				int randomAmount = UnityEngine.Random.Range(1, 101);
				double randomPrice = UnityEngine.Random.Range(1, 100);
				load[randomCommodity] = new CommodityLoad(randomCommodity, randomAmount, randomPrice);
			}
		}
    }
}

using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using System;

namespace Assets.Scripts.Data
{
    public class Market
    {
        private readonly List<CommodityTradable> tradables = new()
        {
            new CommodityTradable(CommodityType.Fish, 100, new CommodityData(
                minPrice: 5,
                slopeStartPrice: 10,
                slopeEndPrice: 20,
                maxPrice: 30,
                slopeStartAmount: 20,
                slopeEndAmount: 80)
            ),
            new CommodityTradable(CommodityType.Wood, 50, new CommodityData(
                minPrice: 10,
                slopeStartPrice: 20,
                slopeEndPrice: 40,
                maxPrice: 60,
                slopeStartAmount: 20,
                slopeEndAmount: 80)
            ),
            new CommodityTradable(CommodityType.Metal, 20, new CommodityData(
                minPrice: 15,
                slopeStartPrice: 30,
                slopeEndPrice: 60,
                maxPrice: 90,
                slopeStartAmount: 20,
                slopeEndAmount: 80)
            )
        };

        public Market()
        {

        }

        public IReadOnlyList<CommodityTradable> Tradables => tradables;

        public Market(List<CommodityTradable> _tradables)
        {
            tradables = _tradables;
        }

        public Market Clone()
        {
            List<CommodityTradable> clonedTradables = new();
            foreach (var tradable in tradables)
            {
                clonedTradables.Add(tradable.Clone());
            }
            return new Market(clonedTradables);
        }

        public Nullable<double> BuyOne(CommodityType _commodity)
        {
            var tradable = tradables.Find(t => t.commodity == _commodity);
            if (tradable == null)
            {
                return null;
            }
            return tradable.BuyOne();
        }

        public Nullable<double> SellOne(CommodityType _commodity)
        {
            var tradable = tradables.Find(t => t.commodity == _commodity);
            if (tradable == null)
            {
                return null;
            }
            return tradable.SellOne();
        }

        public double UndoBuyOne(CommodityType _commodity)
        {
            var tradable = tradables.Find(t => t.commodity == _commodity)!;
            return tradable.UndoBuyOne();
        }

        public double UndoSellOne(CommodityType _commodity)
        {
            var tradable = tradables.Find(t => t.commodity == _commodity)!;
            return tradable.UndoSellOne();
        }

        public double GetPrice(CommodityType _commodity)
        {
            var tradable = tradables.Find(t => t.commodity == _commodity);
            if (tradable == null)
            {
                return 0;
            }
            return tradable.GetPrice();
        }

        public void Update()
        {
            foreach(var tradable in tradables)
            {
                tradable.Update(UnityEngine.Random.Range(-50, 50));
            }
        }
    }
}

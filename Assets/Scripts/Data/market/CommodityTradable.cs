using UnityEngine;
using System.Collections;
using System;

namespace Assets.Scripts.Data
{
    public struct CommodityData
    {
        public double minPrice;
        public double slopeStartPrice;
        public double slopeEndPrice;
        public double maxPrice;

        public double slopeStartAmount;
        public double slopeEndAmount;

        public CommodityData(int minPrice, int slopeStartPrice, int slopeEndPrice, int maxPrice, int slopeStartAmount, int slopeEndAmount)
        {
            this.minPrice = minPrice;
            this.slopeStartPrice = slopeStartPrice;
            this.slopeEndPrice = slopeEndPrice;
            this.maxPrice = maxPrice;
            this.slopeStartAmount = slopeStartAmount;
            this.slopeEndAmount = slopeEndAmount;
        }
    }

    public class CommodityTradable
	{
		public readonly CommodityType commodity;
		public double Price { get; private set; } = 0;
		public int Stock { get; private set; } = 0;

        public readonly CommodityData data;

        public CommodityTradable(CommodityType _commodity, int _stock, CommodityData _data)
		{
			commodity = _commodity;
			Stock = _stock;

            data = _data;
			
			UpdatePrice();

        }
		public CommodityTradable(CommodityType _commodity)
		{
			commodity = _commodity;
		}

		public CommodityTradable Clone()
		{
            return new CommodityTradable(commodity, Stock, data);
        }

        private void UpdatePrice()
        {
            if (Stock < data.slopeStartAmount)
            {
                Price = data.minPrice;

            }
            else if (Stock > data.slopeEndAmount)
            {
                Price = data.maxPrice;
            }
            else
            {
                Price = data.slopeStartPrice + (data.slopeEndPrice - data.slopeStartPrice) * ((Stock - data.slopeStartAmount) / (data.slopeEndAmount - data.slopeStartAmount));
            }

            Price = Math.Round(Price);
        }

        public double GetPrice()
        {
            return Price;
        }

        public Nullable<double> BuyOne()
		{
            double price = Price;
            if (Stock <= 0)
            {
                return null;
            }
            Stock -= 1;
            UpdatePrice();
            return price;
        }

		public double SellOne()
        {
            double price = Price;
            Stock += 1;
            UpdatePrice();
            return price;
        }

        public double UndoBuyOne()
        {
            Stock += 1;
            UpdatePrice();
            return Price;
        }

        public double UndoSellOne()
        {
            Stock -= 1;
            UpdatePrice();
            return Price;
        }

        public void Update(int amount)
        {
            Stock += amount;
            if(Stock < 0)
            {
                Stock = 0;
            }
            if(Stock > 500)
            {
                Stock = 500;
            }
            UpdatePrice();
        }
    }
}
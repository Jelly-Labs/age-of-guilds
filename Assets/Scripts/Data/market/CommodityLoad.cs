using UnityEngine;
using System.Collections;

namespace Assets.Scripts.Data
{
	public class CommodityLoad	{
		public readonly CommodityType commodity;
		public double AveragePrice { get; private set; } = 0;
		
		private int _amount = 0;
        public int Amount
		{
			get => _amount;
			private set
			{
				_amount = value;
			}
		}

        public void Add(int amount, double averagePrice)
		{
			AveragePrice = (Amount * AveragePrice + amount * averagePrice) / (Amount + amount);
			Amount += amount;
        }

		public void Remove(int amount)
		{
			Amount -= amount;
		}

		public void UndoAdd(int amount, double averagePrice)
		{
			if (Amount - amount == 0)
			{
				AveragePrice = 0;
			}
			else
			{
				AveragePrice = (Amount * AveragePrice - amount * averagePrice) / (Amount - amount);
			}
			Amount -= amount;
		}

		public CommodityLoad(CommodityType _commodity, int _amount, double _averagePrice)
		{
			commodity = _commodity;
			Amount = _amount;
			AveragePrice = _averagePrice;
        }

		public CommodityLoad(CommodityType _commodity)
		{
			commodity = _commodity;
		}

		public CommodityLoad Clone()
		{
			return new CommodityLoad(commodity, Amount, AveragePrice);
        }
    }
}
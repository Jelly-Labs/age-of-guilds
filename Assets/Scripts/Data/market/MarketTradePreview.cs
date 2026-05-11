using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using Assets.Scripts.Data.market;

namespace Assets.Scripts.Data
{
    public readonly struct TradeResult
    {
        public enum Status
        {
            Buy,
            Sell,
            NotEnoughStock
        }

        public readonly Status status;
        public readonly CommodityType commodity;
        public readonly int amount;
        public readonly double averagePrice;
        public readonly double priceBefore;
        public readonly double priceAfter;

        public TradeResult(Status _status, CommodityType _commodity, int _amount, double _averagePrice, double _priceBefore, double _priceAfter)
        {
            status = _status;
            commodity = _commodity;
            amount = _amount;
            averagePrice = _averagePrice;
            priceBefore = _priceBefore;
            priceAfter = _priceAfter;
        }
    }

    public class MarketTradePreview
	{
		public Market market;

        public FullLoad load;

        public double gold = GameData.instance.gold;

        public MarketTradePreview(Market _market, Ship ship)
        {
            market = _market.Clone();
            if (ship == null)
            {
                load = new FullLoad();
            }
            else
            {
                load = ship.load.Clone();
            }
        }

        public Dictionary<CommodityType, TradeResult> Preview { get; private set; } = new();

        public bool Buyable(CommodityType _commodity)
        {
            return gold >= market.GetPrice(_commodity);
        }

        public void BuyOne(CommodityType _commodity)
		{
            TradeResult old = Preview.ContainsKey(_commodity) ? Preview[_commodity] : new TradeResult(TradeResult.Status.Buy, _commodity, 0, 0, market.GetPrice(_commodity), 0);
            // If some selling was done before, this is undoing it
            // Otherwise, it's a new buy
            double price;
            TradeResult.Status newStatus;
            if (old.status == TradeResult.Status.Sell)
            {
                price = market.UndoSellOne(_commodity);
                newStatus = TradeResult.Status.Sell;
            }
            else
            {
                if (gold < market.GetPrice(_commodity))
                {
                    return;
                }

                var currentPrice = market.BuyOne(_commodity);
                if (currentPrice == null)
                {
                    return;
                }
                price = currentPrice.Value;
                newStatus = TradeResult.Status.Buy;
            }

            // Update the load for this commodity
            load.AddOne(_commodity, price);
            gold -= price;

            // Update the total amount in the preview
            // If it is now 0, remove it from the preview, otherwise update the average price
            int newAmount = old.amount + 1;
            if (newAmount == 0)
            {
                Preview.Remove(_commodity);
                return;
            }
            else
            {
                double averagePrice = (old.amount * old.averagePrice + price) / (newAmount);
                TradeResult result = new(newStatus, _commodity, newAmount, averagePrice, old.priceBefore, price);
                Preview[_commodity] = result;
            }

            Debug.Log($"New trade data: {Preview[_commodity].status} amount: {Preview[_commodity].amount}, avg_price: {Preview[_commodity].averagePrice}");
            Debug.Log($"Gold left: {gold}");
            Debug.Log($"Load for {_commodity}: {load.GetAmount(_commodity)}");
        }

		public void SellOne(CommodityType _commodity)
		{
            TradeResult old = Preview.ContainsKey(_commodity) ? Preview[_commodity] : new TradeResult(TradeResult.Status.Sell, _commodity, 0, 0, market.GetPrice(_commodity), 0); ;
            // If some selling was done before, this is undoing it
            // Otherwise, it's a new buy
            double price = 0;
            TradeResult.Status newStatus;
            if (old.status == TradeResult.Status.Buy)
            {
                price = market.UndoBuyOne(_commodity);
                load.UndoAddOne(_commodity, price);
                newStatus = TradeResult.Status.Buy;
            }
            else
            {
                var currentPrice = market.SellOne(_commodity);
                if (currentPrice == null)
                {
                    return;
                }
                price = currentPrice.Value;
                load.RemoveOne(_commodity);
                newStatus = TradeResult.Status.Sell;
            }
            gold += price;

            // Update the total amount in the preview
            // If it is now 0, remove it from the preview, otherwise update the average price
            int newAmount = old.amount - 1;
            if (newAmount == 0)
            {
                Preview.Remove(_commodity);
                return;
            }
            else
            {
                double averagePrice = (old.amount * old.averagePrice - price) / (newAmount);
                TradeResult result = new(newStatus, _commodity, newAmount, averagePrice, old.priceBefore, price);
                Preview[_commodity] = result;
            }

            Debug.Log($"New trade data: {Preview[_commodity].status} amount: {Preview[_commodity].amount}, avg_price: {Preview[_commodity].averagePrice}");
            Debug.Log($"Gold left: {gold}");
            Debug.Log($"Load for {_commodity}: {load.GetAmount(_commodity)}");
        }
    }
}

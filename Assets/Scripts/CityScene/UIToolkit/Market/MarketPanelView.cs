using System.Collections.Generic;
using Assets.Scripts.Data;
using UnityEngine.UIElements;

namespace Assets.Scripts.CityScene.UIToolkit
{
    public sealed class MarketPanelView : CityMenuPanelView
    {
        const string ContentLabelName = "MarketPanelContentLabel";
        const string ContentLabelClass = "city-menu-market-panel__content-label";
        const string HeaderName = "MarketPanelHeader";
        const string HeaderClass = "city-menu-market-panel__header";
        const string ShipSelectorName = "MarketPanelShipSelector";
        const string ShipSelectorClass = "city-menu-market-panel__ship-selector";
        const string BodyName = "MarketPanelBody";
        const string BodyClass = "city-menu-market-panel__body";

        readonly DropdownSelectorView<Ship> shipSelector;
        readonly MarketTradeTableView tradeTableView;
        readonly MarketTradeSummaryView tradeSummaryView;

        private MarketTradePreview tradePreview;

        Ship selectedShip;

        public MarketPanelView(string menuItemId, string title)
            : base(menuItemId, title)
        {

            shipSelector = new DropdownSelectorView<Ship>
            {
                name = ShipSelectorName,
                pickingMode = PickingMode.Position
            };
            shipSelector.AddToClassList(ShipSelectorClass);
            shipSelector.SetItems(GameData.instance.CurrentTown.DockedShips, HandleSelectedShipChanged, FormatShip);
            selectedShip = shipSelector.SelectedItem;

            VisualElement body = new VisualElement
            {
                name = BodyName,
                pickingMode = PickingMode.Position
            };
            body.AddToClassList(BodyClass);

            tradeTableView = new MarketTradeTableView(shipSelector);
            tradeTableView.TradeValueChanged += HandleRowTradeValueChanged;
            body.Add(tradeTableView);

            tradeSummaryView = new MarketTradeSummaryView();
            tradeSummaryView.ResetRequested += HandleResetClicked;
            tradeSummaryView.ConfirmRequested += HandleConfirmClicked;
            body.Add(tradeSummaryView);

            ContentRoot.Add(body);

            BuildMarketRows();
        }

        public Ship SelectedShip => selectedShip;

        public override void Dispose()
        {
            tradeTableView.TradeValueChanged -= HandleRowTradeValueChanged;
            tradeTableView.Dispose();
            tradeSummaryView.ResetRequested -= HandleResetClicked;
            tradeSummaryView.ConfirmRequested -= HandleConfirmClicked;
            tradeSummaryView.Dispose();
            shipSelector.Dispose();
            base.Dispose();
        }

        void HandleSelectedShipChanged(Ship ship)
        {
            selectedShip = ship;
            BuildMarketRows();
        }

        static string FormatShip(Ship ship)
        {
            return ship != null ? $"{ship.Name}" : string.Empty;
        }

        void BuildMarketRows()
        {
            if (selectedShip == null)
            {
                tradePreview = null;
                tradeSummaryView.Refresh(null);
                tradeTableView.Rebuild(null, null);
                return;
            }

            tradePreview = new MarketTradePreview(GameData.instance.CurrentTown.Market, selectedShip);

            IReadOnlyList<CommodityTradable> tradables = tradePreview.market.Tradables;
            tradeTableView.Rebuild(tradables, GetCommodityDisplayData);
            tradeSummaryView.Refresh(tradePreview);
        }

        void RefreshMarketRows()
        {
            if (tradePreview == null)
            {
                return;
            }

            IReadOnlyList<CommodityTradable> tradables = tradePreview.market.Tradables;
            tradeTableView.Refresh(tradables, GetCommodityDisplayData);
            tradeSummaryView.Refresh(tradePreview);
        }

        void HandleRowTradeValueChanged(CommodityType commodity, int previousValue, int nextValue)
        {
            int delta = nextValue - previousValue;
            if (delta > 0)
            {
                for (int i = 0; i < delta; i++)
                {
                    if (!tradePreview.Buyable(commodity))
                    {
                        break;
                    }
                    tradePreview.BuyOne(commodity);
                }
            }
            else if (delta < 0)
            {
                for (int i = 0; i < -delta; i++)
                {
                    tradePreview.SellOne(commodity);
                }
            }

            RefreshMarketRows();
        }

        MarketCommodityDisplayData GetCommodityDisplayData(CommodityType commodity)
        {
            int amount = 0;
            double averagePrice = 0;

            if (tradePreview == null)
            {
                return new MarketCommodityDisplayData(amount, averagePrice, false);
            }

            amount = tradePreview.load.GetAmount(commodity);
            averagePrice = tradePreview.load.GetAveragePrice(commodity);
            return new MarketCommodityDisplayData(amount, averagePrice, tradePreview.Buyable(commodity));
        }

        void HandleResetClicked()
        {
            BuildMarketRows();
        }

        void HandleConfirmClicked()
        {
            if (tradePreview == null || selectedShip == null || tradePreview.Preview.Count == 0)
            {
                return;
            }

            GameData.instance.CurrentTown.ExecuteTrades(tradePreview);
            selectedShip.load = tradePreview.load;
            GameData.instance.gold = tradePreview.gold;

            BuildMarketRows();
        }
    }
}

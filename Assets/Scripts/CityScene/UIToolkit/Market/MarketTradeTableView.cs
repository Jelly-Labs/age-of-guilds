using System;
using System.Collections.Generic;
using Assets.Scripts.Data;
using UnityEngine.UIElements;

namespace Assets.Scripts.CityScene.UIToolkit
{
    public readonly struct MarketCommodityDisplayData
    {
        public MarketCommodityDisplayData(int shipAmount, double shipAveragePrice, bool isBuyable)
        {
            ShipAmount = shipAmount;
            ShipAveragePrice = shipAveragePrice;
            IsBuyable = isBuyable;
        }

        public int ShipAmount { get; }
        public double ShipAveragePrice { get; }
        public bool IsBuyable { get; }
    }

    public sealed class MarketTradeTableView : VisualElement, IDisposable
    {
        const string TradeSectionName = "MarketPanelTradeSection";
        const string TradeSectionClass = "city-menu-market-panel__trade-section";
        const string ToolbarName = "MarketPanelTradeToolbar";
        const string ToolbarClass = "city-menu-market-panel__trade-toolbar";
        const string RowListName = "MarketPanelCommodityRows";
        const string RowListClass = "city-menu-market-panel__rows";
        const string RowsHeaderName = "MarketPanelRowsHeader";
        const string RowsHeaderClass = "city-menu-market-panel__rows-header";
        const string RowsHeaderMarketSectionClass = "city-menu-market-panel__rows-header-market-section";
        const string RowsHeaderMiddleSectionClass = "city-menu-market-panel__rows-header-middle-section";
        const string RowsHeaderShipSectionClass = "city-menu-market-panel__rows-header-ship-section";
        const string RowsHeaderCommodityClass = "city-menu-market-panel__rows-header-commodity";
        const string RowsHeaderValueClass = "city-menu-market-panel__rows-header-value";
        const string RowsHeaderTradeActionClass = "city-menu-market-panel__rows-header-trade-action";
        const string RowsHeaderTradeArrowClass = "city-menu-market-panel__rows-header-trade-arrow";
        const string RowsHeaderTradeArrowRightClass = "city-menu-market-panel__rows-header-trade-arrow--right";

        readonly VisualElement rowList;
        readonly List<MarketRowView> marketRows = new();

        public event Action<CommodityType, int, int> TradeValueChanged;

        public MarketTradeTableView(VisualElement shipSelector)
        {
            name = TradeSectionName;
            pickingMode = PickingMode.Position;
            AddToClassList(TradeSectionClass);

            VisualElement toolbar = new VisualElement
            {
                name = ToolbarName,
                pickingMode = PickingMode.Position
            };
            toolbar.AddToClassList(ToolbarClass);

            if (shipSelector != null)
            {
                toolbar.Add(shipSelector);
            }

            Add(toolbar);

            rowList = new VisualElement
            {
                name = RowListName,
                pickingMode = PickingMode.Position
            };
            rowList.AddToClassList(RowListClass);

            Add(rowList);

            toolbar.BringToFront();
        }

        public void Rebuild(IReadOnlyList<CommodityTradable> tradables, Func<CommodityType, MarketCommodityDisplayData> getDisplayData)
        {
            ClearRows();
            rowList.Clear();
            AddRowsHeader();

            if (tradables == null)
            {
                return;
            }

            for (int i = 0; i < tradables.Count; i++)
            {
                MarketCommodityDisplayData displayData = getDisplayData(tradables[i].commodity);

                MarketRowView row = new MarketRowView();
                row.TradeValueChanged += HandleRowTradeValueChanged;
                row.Bind(
                    tradables[i],
                    displayData.ShipAmount,
                    displayData.ShipAveragePrice,
                    -displayData.ShipAmount,
                    tradables[i].Stock,
                    displayData.IsBuyable);

                rowList.Add(row);
                marketRows.Add(row);
            }
        }

        public void Refresh(IReadOnlyList<CommodityTradable> tradables, Func<CommodityType, MarketCommodityDisplayData> getDisplayData)
        {
            if (tradables == null)
            {
                return;
            }

            int rowCount = Math.Min(marketRows.Count, tradables.Count);
            for (int i = 0; i < rowCount; i++)
            {
                MarketCommodityDisplayData displayData = getDisplayData(tradables[i].commodity);
                marketRows[i].UpdateValues(tradables[i], displayData.ShipAmount, displayData.ShipAveragePrice);
                marketRows[i].UpdateLimits(displayData.ShipAmount, tradables[i].Stock, displayData.IsBuyable);
            }
        }

        public void Dispose()
        {
            ClearRows();
            TradeValueChanged = null;
        }

        void ClearRows()
        {
            for (int i = 0; i < marketRows.Count; i++)
            {
                marketRows[i].TradeValueChanged -= HandleRowTradeValueChanged;
                marketRows[i].Dispose();
            }

            marketRows.Clear();
        }

        void HandleRowTradeValueChanged(CommodityType commodity, int previousValue, int nextValue)
        {
            TradeValueChanged?.Invoke(commodity, previousValue, nextValue);
        }

        void AddRowsHeader()
        {
            VisualElement header = new VisualElement
            {
                name = RowsHeaderName,
                pickingMode = PickingMode.Ignore
            };
            header.AddToClassList(RowsHeaderClass);

            VisualElement marketSection = new VisualElement
            {
                name = "MarketPanelRowsHeaderMarketSection",
                pickingMode = PickingMode.Ignore
            };
            marketSection.AddToClassList(RowsHeaderMarketSectionClass);

            VisualElement middleSection = new VisualElement
            {
                name = "MarketPanelRowsHeaderMiddleSection",
                pickingMode = PickingMode.Ignore
            };
            middleSection.AddToClassList(RowsHeaderMiddleSectionClass);

            VisualElement shipSection = new VisualElement
            {
                name = "MarketPanelRowsHeaderShipSection",
                pickingMode = PickingMode.Ignore
            };
            shipSection.AddToClassList(RowsHeaderShipSectionClass);

            Label commodityLabel = new Label("Goods")
            {
                pickingMode = PickingMode.Ignore
            };
            commodityLabel.AddToClassList(RowsHeaderCommodityClass);

            Label stockLabel = new Label("Stock")
            {
                pickingMode = PickingMode.Ignore
            };
            stockLabel.AddToClassList(RowsHeaderValueClass);

            Label priceLabel = new Label("Price")
            {
                pickingMode = PickingMode.Ignore
            };
            priceLabel.AddToClassList(RowsHeaderValueClass);

            Label sellLabel = new Label("Sell")
            {
                pickingMode = PickingMode.Ignore
            };
            sellLabel.AddToClassList(RowsHeaderTradeActionClass);

            Label buyLabel = new Label("Buy")
            {
                pickingMode = PickingMode.Ignore
            };
            buyLabel.AddToClassList(RowsHeaderTradeActionClass);

            VisualElement leftArrow = new VisualElement
            {
                name = "MarketPanelRowsHeaderSellArrow",
                pickingMode = PickingMode.Ignore
            };
            leftArrow.AddToClassList(RowsHeaderTradeArrowClass);

            VisualElement rightArrow = new VisualElement
            {
                name = "MarketPanelRowsHeaderBuyArrow",
                pickingMode = PickingMode.Ignore
            };
            rightArrow.AddToClassList(RowsHeaderTradeArrowClass);
            rightArrow.AddToClassList(RowsHeaderTradeArrowRightClass);

            Label shipAmountLabel = new Label("Cargo")
            {
                pickingMode = PickingMode.Ignore
            };
            shipAmountLabel.AddToClassList(RowsHeaderValueClass);

            Label shipAveragePriceLabel = new Label("Bought for")
            {
                pickingMode = PickingMode.Ignore
            };
            shipAveragePriceLabel.AddToClassList(RowsHeaderValueClass);

            marketSection.Add(commodityLabel);
            marketSection.Add(stockLabel);
            marketSection.Add(priceLabel);
            middleSection.Add(leftArrow);
            middleSection.Add(sellLabel);
            middleSection.Add(buyLabel);
            middleSection.Add(rightArrow);
            shipSection.Add(shipAmountLabel);
            shipSection.Add(shipAveragePriceLabel);

            header.Add(marketSection);
            header.Add(middleSection);
            header.Add(shipSection);
            rowList.Add(header);
        }
    }
}
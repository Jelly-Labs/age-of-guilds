using System;
using System.Globalization;
using Assets.Scripts.Data;
using UnityEngine.UIElements;

namespace Assets.Scripts.CityScene.UIToolkit
{
    public sealed class MarketRowView : VisualElement
    {
        const string RootClass = "city-menu-market-row";
        const string MarketSectionClass = "city-menu-market-row__market-section";
        const string MiddleSectionClass = "city-menu-market-row__middle-section";
        const string ShipSectionClass = "city-menu-market-row__ship-section";
        const string CommodityGroupClass = "city-menu-market-row__commodity-group";
        const string CommodityIconClass = "city-menu-market-row__commodity-icon";
        const string CommodityIconFishClass = "city-menu-market-row__commodity-icon--fish";
        const string CommodityIconWoodClass = "city-menu-market-row__commodity-icon--wood";
        const string CommodityIconMetalClass = "city-menu-market-row__commodity-icon--metal";
        const string IconValueGroupClass = "city-menu-market-row__icon-value-group";
        const string ValueIconClass = "city-menu-market-row__value-icon";
        const string ValueIconGoldClass = "city-menu-market-row__value-icon--gold";
        const string ValueIconCargoClass = "city-menu-market-row__value-icon--cargo";
        const string CommodityClass = "city-menu-market-row__commodity";
        const string ValueClass = "city-menu-market-row__value";
        const string ValueCompactClass = "city-menu-market-row__value--compact";
        const string TradeSliderName = "MarketRowTradeSlider";
        const string TradeSliderClass = "city-menu-market-row__trade-slider";
        const string StockName = "MarketRowStock";
        const string PriceName = "MarketRowPrice";
        const string ShipAmountName = "MarketRowShipAmount";
        const string ShipAveragePriceName = "MarketRowShipAveragePrice";

        readonly VisualElement commodityIcon;
        readonly VisualElement shipAmountGroup;
        readonly VisualElement shipAveragePriceGroup;
        readonly Label commodityLabel;
        readonly Label stockLabel;
        readonly Label priceLabel;
        readonly Label shipAmountLabel;
        readonly Label shipAveragePriceLabel;
        readonly IntegerSliderView tradeSlider;

        CommodityType commodity;
        int tradeValue;

        public event Action<CommodityType, int, int> TradeValueChanged;

        public MarketRowView()
        {
            AddToClassList(RootClass);

            VisualElement marketSection = new VisualElement
            {
                name = "MarketRowMarketSection",
                pickingMode = PickingMode.Ignore
            };
            marketSection.AddToClassList(MarketSectionClass);

            VisualElement middleSection = new VisualElement
            {
                name = "MarketRowMiddleSection",
                pickingMode = PickingMode.Position
            };
            middleSection.AddToClassList(MiddleSectionClass);

            VisualElement shipSection = new VisualElement
            {
                name = "MarketRowShipSection",
                pickingMode = PickingMode.Ignore
            };
            shipSection.AddToClassList(ShipSectionClass);

            VisualElement commodityGroup = new VisualElement
            {
                name = "MarketRowCommodityGroup",
                pickingMode = PickingMode.Ignore
            };
            commodityGroup.AddToClassList(CommodityGroupClass);

            commodityIcon = new VisualElement
            {
                name = "MarketRowCommodityIcon",
                pickingMode = PickingMode.Position
            };
            commodityIcon.AddToClassList(CommodityIconClass);

            commodityLabel = new Label
            {
                name = "MarketRowCommodity",
                pickingMode = PickingMode.Ignore
            };
            commodityLabel.AddToClassList(CommodityClass);

            commodityGroup.Add(commodityIcon);
            commodityGroup.Add(commodityLabel);

            stockLabel = new Label
            {
                name = StockName,
                pickingMode = PickingMode.Ignore
            };
            stockLabel.AddToClassList(ValueClass);

            VisualElement priceGroup = new VisualElement
            {
                name = "MarketRowPriceGroup",
                pickingMode = PickingMode.Ignore
            };
            priceGroup.AddToClassList(IconValueGroupClass);

            VisualElement priceIcon = new VisualElement
            {
                name = "MarketRowPriceIcon",
                pickingMode = PickingMode.Ignore
            };
            priceIcon.AddToClassList(ValueIconClass);
            priceIcon.AddToClassList(ValueIconGoldClass);

            priceLabel = new Label
            {
                name = PriceName,
                pickingMode = PickingMode.Ignore
            };
            priceLabel.AddToClassList(ValueClass);
            priceLabel.AddToClassList(ValueCompactClass);
            priceGroup.Add(priceIcon);
            priceGroup.Add(priceLabel);

            shipAmountGroup = new VisualElement
            {
                name = "MarketRowShipAmountGroup",
                pickingMode = PickingMode.Ignore
            };
            shipAmountGroup.AddToClassList(IconValueGroupClass);

            VisualElement shipAmountIcon = new VisualElement
            {
                name = "MarketRowShipAmountIcon",
                pickingMode = PickingMode.Ignore
            };
            shipAmountIcon.AddToClassList(ValueIconClass);
            shipAmountIcon.AddToClassList(ValueIconCargoClass);

            shipAmountLabel = new Label
            {
                name = ShipAmountName,
                pickingMode = PickingMode.Ignore
            };
            shipAmountLabel.AddToClassList(ValueClass);
            shipAmountLabel.AddToClassList(ValueCompactClass);
            shipAmountGroup.Add(shipAmountIcon);
            shipAmountGroup.Add(shipAmountLabel);

            shipAveragePriceGroup = new VisualElement
            {
                name = "MarketRowShipAveragePriceGroup",
                pickingMode = PickingMode.Ignore
            };
            shipAveragePriceGroup.AddToClassList(IconValueGroupClass);

            VisualElement shipAveragePriceIcon = new VisualElement
            {
                name = "MarketRowShipAveragePriceIcon",
                pickingMode = PickingMode.Ignore
            };
            shipAveragePriceIcon.AddToClassList(ValueIconClass);
            shipAveragePriceIcon.AddToClassList(ValueIconGoldClass);

            shipAveragePriceLabel = new Label
            {
                name = ShipAveragePriceName,
                pickingMode = PickingMode.Ignore
            };
            shipAveragePriceLabel.AddToClassList(ValueClass);
            shipAveragePriceLabel.AddToClassList(ValueCompactClass);
            shipAveragePriceGroup.Add(shipAveragePriceIcon);
            shipAveragePriceGroup.Add(shipAveragePriceLabel);

            tradeSlider = new IntegerSliderView
            {
                name = TradeSliderName,
                pickingMode = PickingMode.Position
            };
            tradeSlider.AddToClassList(TradeSliderClass);
            tradeSlider.ValueChanged += HandleTradeSliderValueChanged;

            marketSection.Add(commodityGroup);
            marketSection.Add(stockLabel);
            marketSection.Add(priceGroup);
            middleSection.Add(tradeSlider);
            shipSection.Add(shipAmountGroup);
            shipSection.Add(shipAveragePriceGroup);

            Add(marketSection);
            Add(middleSection);
            Add(shipSection);
        }

        public void Bind(CommodityTradable tradable, int shipAmount, double shipAveragePrice, int lowerLimit, int upperLimit, bool isBuyable)
        {
            if (tradable != null)
            {
                commodity = tradable.commodity;
            }

            if (!isBuyable)
            {
                upperLimit = 0;
            }

            tradeValue = 0;
            tradeSlider.SetRange(lowerLimit, upperLimit);
            tradeSlider.SetLimits(lowerLimit, upperLimit);
            UpdateValues(tradable, shipAmount, shipAveragePrice);
        }

        public void UpdateValues(CommodityTradable tradable, int shipAmount, double shipAveragePrice)
        {
            if (tradable == null)
            {
                ApplyCommodityIcon(default);
                commodityIcon.tooltip = string.Empty;
                commodityLabel.text = string.Empty;
                stockLabel.text = string.Empty;
                priceLabel.text = string.Empty;
                shipAmountLabel.text = string.Empty;
                shipAveragePriceLabel.text = string.Empty;
                shipAmountGroup.style.display = DisplayStyle.None;
                shipAveragePriceGroup.style.display = DisplayStyle.None;
                return;
            }

            ApplyCommodityIcon(tradable.commodity);
            commodityIcon.tooltip = FormatCommodity(tradable.commodity);
            commodityLabel.text = FormatCommodity(tradable.commodity);
            stockLabel.text = tradable.Stock.ToString(CultureInfo.InvariantCulture);
            priceLabel.text = tradable.Price.ToString("0.##", CultureInfo.InvariantCulture);
            shipAmountLabel.text = shipAmount.ToString(CultureInfo.InvariantCulture);
            shipAveragePriceLabel.text = shipAmount > 0
                ? shipAveragePrice.ToString("0.##", CultureInfo.InvariantCulture)
                : "-";
            DisplayStyle cargoDisplay = shipAmount > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            shipAmountGroup.style.display = cargoDisplay;
            shipAveragePriceGroup.style.display = cargoDisplay;
        }

        public void UpdateLimits(int shipAmount, int marketStock, bool isBuyable)
        {
            // horible hack -- lower limit should be always the ship amount at the beggining, but since ship amount changes we have to recalculate it every time
            int lowerLimit = tradeValue < 0 ? tradeValue - shipAmount : -shipAmount;
            int upperLimit = isBuyable ? marketStock + tradeValue : tradeValue;
            tradeSlider.SetLimits(lowerLimit, upperLimit);
        }

        public void Dispose()
        {
            tradeSlider.ValueChanged -= HandleTradeSliderValueChanged;
            tradeSlider.Dispose();
            TradeValueChanged = null;
        }

        static string FormatCommodity(CommodityType commodity)
        {
            return commodity.ToString();
        }

        void ApplyCommodityIcon(CommodityType commodityType)
        {
            commodityIcon.EnableInClassList(CommodityIconFishClass, commodityType == CommodityType.Fish);
            commodityIcon.EnableInClassList(CommodityIconWoodClass, commodityType == CommodityType.Wood);
            commodityIcon.EnableInClassList(CommodityIconMetalClass, commodityType == CommodityType.Metal);
        }

        void HandleTradeSliderValueChanged(int nextValue)
        {
            if (nextValue == tradeValue)
            {
                return;
            }

            int previousValue = tradeValue;
            tradeValue = nextValue;
            TradeValueChanged?.Invoke(commodity, previousValue, tradeValue);
        }
    }
}

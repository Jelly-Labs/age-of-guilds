using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Data;
using UnityEngine.UIElements;

namespace Assets.Scripts.CityScene.UIToolkit
{
    public sealed class MarketTradeSummaryView : VisualElement, IDisposable
    {
        const string SummaryName = "MarketPanelSummary";
        const string SummaryClass = "city-menu-market-panel__summary";
        const string SummaryTitleClass = "city-menu-market-panel__summary-title";
        const string SummaryHeadersClass = "city-menu-market-panel__summary-headers";
        const string SummaryHeaderGoodsClass = "city-menu-market-panel__summary-header-goods";
        const string SummaryHeaderAmountClass = "city-menu-market-panel__summary-header-amount";
        const string SummaryHeaderAverageClass = "city-menu-market-panel__summary-header-average";
        const string SummaryHeaderPriceClass = "city-menu-market-panel__summary-header-price";
        const string SummaryContentClass = "city-menu-market-panel__summary-content";
        const string SummarySectionClass = "city-menu-market-panel__summary-section";
        const string SummarySectionTitleClass = "city-menu-market-panel__summary-section-title";
        const string SummaryRowClass = "city-menu-market-panel__summary-row";
        const string SummaryGoodsCellClass = "city-menu-market-panel__summary-goods-cell";
        const string SummaryGoodsIconFrameClass = "city-menu-market-panel__summary-goods-icon-frame";
        const string SummaryGoodsClass = "city-menu-market-panel__summary-goods";
        const string SummaryAmountClass = "city-menu-market-panel__summary-amount";
        const string SummaryValueGroupClass = "city-menu-market-panel__summary-value-group";
        const string SummaryValueGroupAverageClass = "city-menu-market-panel__summary-value-group--average";
        const string SummaryValueGroupPriceClass = "city-menu-market-panel__summary-value-group--price";
        const string SummaryValueGroupTotalClass = "city-menu-market-panel__summary-value-group--total";
        const string SummaryValueIconClass = "city-menu-market-panel__summary-value-icon";
        const string SummaryValueIconGoldClass = "city-menu-market-panel__summary-value-icon--gold";
        const string SummaryAverageClass = "city-menu-market-panel__summary-average";
        const string SummaryPriceClass = "city-menu-market-panel__summary-price";
        const string SummaryEmptyClass = "city-menu-market-panel__summary-empty";
        const string SummaryFooterClass = "city-menu-market-panel__summary-footer";
        const string SummaryTotalRowClass = "city-menu-market-panel__summary-total-row";
        const string SummaryTotalLabelClass = "city-menu-market-panel__summary-total-label";
        const string SummaryTotalValueClass = "city-menu-market-panel__summary-total-value";
        const string SummaryButtonsClass = "city-menu-market-panel__summary-buttons";
        const string SummaryResetButtonClass = "city-menu-market-panel__summary-reset-button";
        const string SummaryResetIconClass = "city-menu-market-panel__summary-reset-icon";
        const string SummaryConfirmButtonClass = "city-menu-market-panel__summary-confirm-button";
        const string CommodityIconClass = "city-menu-market-row__commodity-icon";
        const string CommodityIconFishClass = "city-menu-market-row__commodity-icon--fish";
        const string CommodityIconWoodClass = "city-menu-market-row__commodity-icon--wood";
        const string CommodityIconMetalClass = "city-menu-market-row__commodity-icon--metal";

        readonly VisualElement summaryContent;
        readonly Label totalValueLabel;
        readonly Button resetButton;
        readonly Button confirmButton;

        public event Action ResetRequested;
        public event Action ConfirmRequested;

        public MarketTradeSummaryView()
        {
            name = SummaryName;
            pickingMode = PickingMode.Position;
            AddToClassList(SummaryClass);

            Label summaryTitle = new Label("TRADE")
            {
                pickingMode = PickingMode.Ignore
            };
            summaryTitle.AddToClassList(SummaryTitleClass);
            Add(summaryTitle);

            VisualElement summaryHeaders = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            summaryHeaders.AddToClassList(SummaryHeadersClass);

            Label goodsHeader = new Label("Goods") { pickingMode = PickingMode.Ignore };
            goodsHeader.AddToClassList(SummaryHeaderGoodsClass);
            Label amountHeader = new Label("Amount") { pickingMode = PickingMode.Ignore };
            amountHeader.AddToClassList(SummaryHeaderAmountClass);
            Label averageHeader = new Label("Avg. price/unit") { pickingMode = PickingMode.Ignore };
            averageHeader.AddToClassList(SummaryHeaderAverageClass);
            Label priceHeader = new Label("Price") { pickingMode = PickingMode.Ignore };
            priceHeader.AddToClassList(SummaryHeaderPriceClass);

            summaryHeaders.Add(goodsHeader);
            summaryHeaders.Add(amountHeader);
            summaryHeaders.Add(averageHeader);
            summaryHeaders.Add(priceHeader);
            Add(summaryHeaders);

            summaryContent = new VisualElement
            {
                pickingMode = PickingMode.Position
            };
            summaryContent.AddToClassList(SummaryContentClass);
            Add(summaryContent);

            VisualElement summaryFooter = new VisualElement
            {
                pickingMode = PickingMode.Position
            };
            summaryFooter.AddToClassList(SummaryFooterClass);

            VisualElement totalRow = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            totalRow.AddToClassList(SummaryTotalRowClass);

            Label totalLabel = new Label("Total")
            {
                pickingMode = PickingMode.Ignore
            };
            totalLabel.AddToClassList(SummaryTotalLabelClass);

            VisualElement totalValueGroup = CreateGoldValueGroup(SummaryValueGroupTotalClass, SummaryTotalValueClass, out totalValueLabel);

            totalRow.Add(totalLabel);
            totalRow.Add(totalValueGroup);

            VisualElement summaryButtons = new VisualElement
            {
                pickingMode = PickingMode.Position
            };
            summaryButtons.AddToClassList(SummaryButtonsClass);

            resetButton = new Button(HandleResetClicked)
            {
                pickingMode = PickingMode.Position
            };
            resetButton.tooltip = "Reset trade";
            resetButton.AddToClassList(SummaryResetButtonClass);

            VisualElement resetIcon = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            resetIcon.AddToClassList(SummaryResetIconClass);
            resetButton.Add(resetIcon);

            confirmButton = new Button(HandleConfirmClicked)
            {
                text = "Confirm deal",
                pickingMode = PickingMode.Position
            };
            confirmButton.AddToClassList(SummaryConfirmButtonClass);

            summaryButtons.Add(resetButton);
            summaryButtons.Add(confirmButton);
            summaryFooter.Add(totalRow);
            summaryFooter.Add(summaryButtons);
            Add(summaryFooter);
        }

        public void Refresh(MarketTradePreview tradePreview)
        {
            summaryContent.Clear();

            if (tradePreview == null || tradePreview.Preview.Count == 0)
            {
                Label emptyLabel = new Label("No pending trade")
                {
                    pickingMode = PickingMode.Ignore
                };
                emptyLabel.AddToClassList(SummaryEmptyClass);
                summaryContent.Add(emptyLabel);
                totalValueLabel.text = FormatSignedValue(0d);
                confirmButton.SetEnabled(false);
                resetButton.SetEnabled(false);
                return;
            }

            AddSummarySection(tradePreview, "Buying", TradeResult.Status.Buy);
            AddSummarySection(tradePreview, "Selling", TradeResult.Status.Sell);

            double total = 0;
            foreach (TradeResult result in tradePreview.Preview.Values)
            {
                total += -result.amount * result.averagePrice;
            }

            totalValueLabel.text = FormatSignedValue(total);
            confirmButton.SetEnabled(true);
            resetButton.SetEnabled(true);
        }

        public void Dispose()
        {
            ResetRequested = null;
            ConfirmRequested = null;
        }

        void AddSummarySection(MarketTradePreview tradePreview, string title, TradeResult.Status status)
        {
            List<TradeResult> sectionResults = tradePreview.Preview.Values
                .Where(result => result.status == status)
                .OrderBy(result => result.commodity.ToString())
                .ToList();

            if (sectionResults.Count == 0)
            {
                return;
            }

            VisualElement section = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            section.AddToClassList(SummarySectionClass);

            Label sectionTitle = new Label(title)
            {
                pickingMode = PickingMode.Ignore
            };
            sectionTitle.AddToClassList(SummarySectionTitleClass);
            section.Add(sectionTitle);

            for (int i = 0; i < sectionResults.Count; i++)
            {
                TradeResult result = sectionResults[i];
                VisualElement row = new VisualElement
                {
                    pickingMode = PickingMode.Ignore
                };
                row.AddToClassList(SummaryRowClass);

                VisualElement goodsCell = new VisualElement
                {
                    pickingMode = PickingMode.Ignore
                };
                goodsCell.AddToClassList(SummaryGoodsCellClass);

                VisualElement goodsIconFrame = new VisualElement
                {
                    pickingMode = PickingMode.Ignore,
                    tooltip = result.commodity.ToString()
                };
                goodsIconFrame.AddToClassList(SummaryGoodsIconFrameClass);

                VisualElement goodsIcon = new VisualElement
                {
                    pickingMode = PickingMode.Ignore,
                    tooltip = result.commodity.ToString()
                };
                goodsIcon.AddToClassList(CommodityIconClass);
                goodsIcon.AddToClassList(SummaryGoodsClass);
                ApplyCommodityIcon(goodsIcon, result.commodity);
                goodsIconFrame.Add(goodsIcon);
                goodsCell.Add(goodsIconFrame);

                int absAmount = Math.Abs(result.amount);

                Label amountLabel = new Label(absAmount.ToString(CultureInfo.InvariantCulture))
                {
                    pickingMode = PickingMode.Ignore
                };
                amountLabel.AddToClassList(SummaryAmountClass);

                VisualElement averageGroup = CreateGoldValueGroup(
                    SummaryValueGroupAverageClass,
                    SummaryAverageClass,
                    out Label averageLabel);
                averageLabel.text = result.averagePrice.ToString("0.##", CultureInfo.InvariantCulture);

                VisualElement priceGroup = CreateGoldValueGroup(
                    SummaryValueGroupPriceClass,
                    SummaryPriceClass,
                    out Label priceLabel);
                priceLabel.text = (absAmount * result.averagePrice).ToString("0.##", CultureInfo.InvariantCulture);

                row.Add(goodsCell);
                row.Add(amountLabel);
                row.Add(averageGroup);
                row.Add(priceGroup);
                section.Add(row);
            }

            summaryContent.Add(section);
        }

        static VisualElement CreateGoldValueGroup(string modifierClass, string labelClass, out Label valueLabel)
        {
            VisualElement valueGroup = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            valueGroup.AddToClassList(SummaryValueGroupClass);
            valueGroup.AddToClassList(modifierClass);

            VisualElement valueIcon = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            valueIcon.AddToClassList(SummaryValueIconClass);
            valueIcon.AddToClassList(SummaryValueIconGoldClass);

            valueLabel = new Label
            {
                pickingMode = PickingMode.Ignore
            };
            valueLabel.AddToClassList(labelClass);

            valueGroup.Add(valueIcon);
            valueGroup.Add(valueLabel);
            return valueGroup;
        }

        static void ApplyCommodityIcon(VisualElement commodityIcon, CommodityType commodityType)
        {
            commodityIcon.EnableInClassList(CommodityIconFishClass, commodityType == CommodityType.Fish);
            commodityIcon.EnableInClassList(CommodityIconWoodClass, commodityType == CommodityType.Wood);
            commodityIcon.EnableInClassList(CommodityIconMetalClass, commodityType == CommodityType.Metal);
        }

        void HandleResetClicked()
        {
            ResetRequested?.Invoke();
        }

        void HandleConfirmClicked()
        {
            ConfirmRequested?.Invoke();
        }

        static string FormatSignedValue(double value)
        {
            string formattedValue = value.ToString("0.##", CultureInfo.InvariantCulture);
            if (value > 0)
            {
                return "+" + formattedValue;
            }

            return formattedValue;
        }
    }
}
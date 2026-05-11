using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CityScene
{
    [DisallowMultipleComponent]
    public class MarketPopupView : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] MarketTradePlaceConfig tradePlaceConfig;
        [SerializeField] string defaultLeftPlaceId = "city_market";
        [SerializeField] string defaultRightPlaceId = "ship_name";

        [Header("Selectors")]
        [SerializeField] MarketTradeSelector leftSelector;
        [SerializeField] MarketTradeSelector rightSelector;

        [Header("Goods")]
        [SerializeField] RectTransform goodsRowsRoot;
        [SerializeField] MarketGoodsRow goodsRowTemplate;

        [Header("Trade Details")]
        [SerializeField] Text tradePlaceholderText;
        [SerializeField] Text totalText;
        [SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;

        readonly List<MarketGoodsRow> visibleRows = new();
        bool isInitialized;

        public IReadOnlyList<MarketGoodsRow> VisibleRows => visibleRows;
        public Text TotalText => totalText;
        public Button ConfirmButton => confirmButton;
        public Button CancelButton => cancelButton;

        void Awake()
        {
            Initialize();
        }

        public void SetConfig(MarketTradePlaceConfig config)
        {
            tradePlaceConfig = config;
            isInitialized = false;
            Initialize();
        }

        public void SetReferences(
            MarketTradeSelector newLeftSelector,
            MarketTradeSelector newRightSelector,
            RectTransform newGoodsRowsRoot,
            MarketGoodsRow newGoodsRowTemplate,
            Text newTradePlaceholderText,
            Text newTotalText,
            Button newConfirmButton,
            Button newCancelButton)
        {
            leftSelector = newLeftSelector;
            rightSelector = newRightSelector;
            goodsRowsRoot = newGoodsRowsRoot;
            goodsRowTemplate = newGoodsRowTemplate;
            tradePlaceholderText = newTradePlaceholderText;
            totalText = newTotalText;
            confirmButton = newConfirmButton;
            cancelButton = newCancelButton;
            isInitialized = false;
            Initialize();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Initialize();
        }

        public void Hide()
        {
            CloseSelectors();
            gameObject.SetActive(false);
        }

        void Initialize()
        {
            if (isInitialized || tradePlaceConfig == null)
            {
                return;
            }

            IReadOnlyList<MarketTradePlace> places = tradePlaceConfig.Places;
            int leftIndex = Mathf.Max(0, tradePlaceConfig.IndexOf(defaultLeftPlaceId));
            int rightIndex = Mathf.Max(0, tradePlaceConfig.IndexOf(defaultRightPlaceId));

            if (leftSelector != null)
            {
                leftSelector.Bind(places, leftIndex, HandleSelectorChanged);
            }

            if (rightSelector != null)
            {
                rightSelector.Bind(places, rightIndex, HandleSelectorChanged);
            }

            if (goodsRowTemplate != null)
            {
                goodsRowTemplate.gameObject.SetActive(false);
            }

            SetTotalPlaceholder();
            RebuildRows(tradePlaceConfig.GetPlace(leftIndex));
            isInitialized = true;
        }

        void HandleSelectorChanged(MarketTradeSelector selector, int selectedIndex)
        {
            CloseInactiveSelector(selector);
            RebuildRows(tradePlaceConfig != null ? tradePlaceConfig.GetPlace(selectedIndex) : null);
        }

        void RebuildRows(MarketTradePlace place)
        {
            if (goodsRowsRoot != null)
            {
                for (int i = goodsRowsRoot.childCount - 1; i >= 0; i--)
                {
                    Transform child = goodsRowsRoot.GetChild(i);
                    if (goodsRowTemplate == null || child != goodsRowTemplate.transform)
                    {
                        DestroyObject(child.gameObject);
                    }
                }
            }

            visibleRows.Clear();

            if (goodsRowsRoot == null || goodsRowTemplate == null || place == null || place.GoodsCatalog == null)
            {
                SetTradePlaceholderVisible(true);
                return;
            }

            IReadOnlyList<MarketGoodEntry> goods = place.GoodsCatalog.Goods;
            SetTradePlaceholderVisible(goods.Count == 0);

            for (int i = 0; i < goods.Count; i++)
            {
                MarketGoodsRow row = Instantiate(goodsRowTemplate, goodsRowsRoot);
                row.name = $"GoodRow_{goods[i].Id}";
                row.gameObject.SetActive(true);
                row.Bind(goods[i]);
                visibleRows.Add(row);
            }
        }

        void CloseInactiveSelector(MarketTradeSelector changedSelector)
        {
            if (leftSelector != null && leftSelector != changedSelector)
            {
                leftSelector.Close();
            }

            if (rightSelector != null && rightSelector != changedSelector)
            {
                rightSelector.Close();
            }
        }

        void CloseSelectors()
        {
            leftSelector?.Close();
            rightSelector?.Close();
        }

        void SetTradePlaceholderVisible(bool visible)
        {
            if (tradePlaceholderText != null)
            {
                tradePlaceholderText.gameObject.SetActive(visible);
            }
        }

        void SetTotalPlaceholder()
        {
            if (totalText != null)
            {
                totalText.text = "--";
            }
        }

        static void DestroyObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}

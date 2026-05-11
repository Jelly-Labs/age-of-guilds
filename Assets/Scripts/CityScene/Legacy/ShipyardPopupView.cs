using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CityScene
{
    [DisallowMultipleComponent]
    public class ShipyardPopupView : MonoBehaviour
    {
        enum ShipyardTab
        {
            Order,
            Queue,
            Buy
        }

        [Header("Data")]
        [SerializeField] ShipyardConfig config;

        [Header("Tabs")]
        [SerializeField] Button orderTabButton;
        [SerializeField] Button queueTabButton;
        [SerializeField] Button buyTabButton;
        [SerializeField] Image orderTabBackplate;
        [SerializeField] Image queueTabBackplate;
        [SerializeField] Image buyTabBackplate;
        [SerializeField] RectTransform orderPanel;
        [SerializeField] RectTransform queuePanel;
        [SerializeField] RectTransform buyPanel;

        [Header("Order Ship")]
        [SerializeField] Button previousShipButton;
        [SerializeField] Button nextShipButton;
        [SerializeField] Image orderShipImage;
        [SerializeField] Text orderImageFallbackText;
        [SerializeField] Text orderShipNameText;
        [SerializeField] Text orderClassTagText;
        [SerializeField] Text orderSpeedText;
        [SerializeField] Text orderHealthText;
        [SerializeField] Text orderCrewText;
        [SerializeField] Text orderStorageText;
        [SerializeField] Text orderOperationalCostText;
        [SerializeField] Text orderPriceText;
        [SerializeField] Text orderEstimatedBuildTimeText;
        [SerializeField] RectTransform orderMaterialsRoot;
        [SerializeField] ShipyardMaterialRow orderMaterialTemplate;
        [SerializeField] Button orderButton;

        [Header("Construction Queue")]
        [SerializeField] ShipyardConstructionRow underConstructionRow;
        [SerializeField] RectTransform constructionQueueRoot;
        [SerializeField] ShipyardConstructionRow constructionQueueTemplate;
        [SerializeField] Text constructionEmptyText;

        [Header("Buy Ships")]
        [SerializeField] Button previousBuyOfferButton;
        [SerializeField] Button nextBuyOfferButton;
        [SerializeField] Text buyCarouselIndexText;
        [SerializeField] RectTransform buyCardsRoot;
        [SerializeField] ShipyardBuyCard buyCardTemplate;
        [SerializeField] Text buyEmptyText;
        [SerializeField, Min(1)] int maxVisibleBuyCards = 4;

        readonly List<ShipyardMaterialRow> orderMaterialRows = new();
        readonly List<ShipyardConstructionRow> constructionRows = new();
        readonly List<ShipyardBuyCard> buyCards = new();
        int selectedShipIndex;
        int buyOfferPageStartIndex;
        bool isInitialized;

        static readonly Color TabActiveColor = new(0.12f, 0.115f, 0.1f, 1f);
        static readonly Color TabInactiveColor = new(0.045f, 0.043f, 0.04f, 1f);

        public IReadOnlyList<ShipyardMaterialRow> OrderMaterialRows => orderMaterialRows;
        public IReadOnlyList<ShipyardConstructionRow> ConstructionRows => constructionRows;
        public IReadOnlyList<ShipyardBuyCard> BuyCards => buyCards;
        public Button OrderButton => orderButton;

        void Awake()
        {
            BindControlListeners();
            Initialize();
        }

        public void SetConfig(ShipyardConfig newConfig)
        {
            config = newConfig;
            isInitialized = false;
            Initialize();
        }

        public void SetReferences(
            Button newOrderTabButton,
            Button newQueueTabButton,
            Button newBuyTabButton,
            Image newOrderTabBackplate,
            Image newQueueTabBackplate,
            Image newBuyTabBackplate,
            RectTransform newOrderPanel,
            RectTransform newQueuePanel,
            RectTransform newBuyPanel,
            Button newPreviousShipButton,
            Button newNextShipButton,
            Image newOrderShipImage,
            Text newOrderImageFallbackText,
            Text newOrderShipNameText,
            Text newOrderClassTagText,
            Text newOrderSpeedText,
            Text newOrderHealthText,
            Text newOrderCrewText,
            Text newOrderStorageText,
            Text newOrderOperationalCostText,
            Text newOrderPriceText,
            Text newOrderEstimatedBuildTimeText,
            RectTransform newOrderMaterialsRoot,
            ShipyardMaterialRow newOrderMaterialTemplate,
            Button newOrderButton,
            ShipyardConstructionRow newUnderConstructionRow,
            RectTransform newConstructionQueueRoot,
            ShipyardConstructionRow newConstructionQueueTemplate,
            Text newConstructionEmptyText,
            Button newPreviousBuyOfferButton,
            Button newNextBuyOfferButton,
            Text newBuyCarouselIndexText,
            RectTransform newBuyCardsRoot,
            ShipyardBuyCard newBuyCardTemplate,
            Text newBuyEmptyText)
        {
            orderTabButton = newOrderTabButton;
            queueTabButton = newQueueTabButton;
            buyTabButton = newBuyTabButton;
            orderTabBackplate = newOrderTabBackplate;
            queueTabBackplate = newQueueTabBackplate;
            buyTabBackplate = newBuyTabBackplate;
            orderPanel = newOrderPanel;
            queuePanel = newQueuePanel;
            buyPanel = newBuyPanel;
            previousShipButton = newPreviousShipButton;
            nextShipButton = newNextShipButton;
            orderShipImage = newOrderShipImage;
            orderImageFallbackText = newOrderImageFallbackText;
            orderShipNameText = newOrderShipNameText;
            orderClassTagText = newOrderClassTagText;
            orderSpeedText = newOrderSpeedText;
            orderHealthText = newOrderHealthText;
            orderCrewText = newOrderCrewText;
            orderStorageText = newOrderStorageText;
            orderOperationalCostText = newOrderOperationalCostText;
            orderPriceText = newOrderPriceText;
            orderEstimatedBuildTimeText = newOrderEstimatedBuildTimeText;
            orderMaterialsRoot = newOrderMaterialsRoot;
            orderMaterialTemplate = newOrderMaterialTemplate;
            orderButton = newOrderButton;
            underConstructionRow = newUnderConstructionRow;
            constructionQueueRoot = newConstructionQueueRoot;
            constructionQueueTemplate = newConstructionQueueTemplate;
            constructionEmptyText = newConstructionEmptyText;
            previousBuyOfferButton = newPreviousBuyOfferButton;
            nextBuyOfferButton = newNextBuyOfferButton;
            buyCarouselIndexText = newBuyCarouselIndexText;
            buyCardsRoot = newBuyCardsRoot;
            buyCardTemplate = newBuyCardTemplate;
            buyEmptyText = newBuyEmptyText;

            BindControlListeners();
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
            gameObject.SetActive(false);
        }

        void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            if (orderMaterialTemplate != null)
            {
                orderMaterialTemplate.gameObject.SetActive(false);
            }

            if (constructionQueueTemplate != null)
            {
                constructionQueueTemplate.gameObject.SetActive(false);
            }

            if (buyCardTemplate != null)
            {
                buyCardTemplate.gameObject.SetActive(false);
            }

            selectedShipIndex = 0;
            buyOfferPageStartIndex = 0;
            ShowSelectedOrderShip();
            RebuildConstructionQueue();
            RebuildVisibleBuyOffers();
            ShowTab(ShipyardTab.Order);
            isInitialized = true;
        }

        void BindControlListeners()
        {
            BindButton(orderTabButton, ShowOrderTab);
            BindButton(queueTabButton, ShowQueueTab);
            BindButton(buyTabButton, ShowBuyTab);
            BindButton(previousShipButton, ShowPreviousShip);
            BindButton(nextShipButton, ShowNextShip);
            BindButton(previousBuyOfferButton, ShowPreviousBuyOffer);
            BindButton(nextBuyOfferButton, ShowNextBuyOffer);
        }

        static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        void ShowOrderTab()
        {
            ShowTab(ShipyardTab.Order);
        }

        void ShowQueueTab()
        {
            ShowTab(ShipyardTab.Queue);
        }

        void ShowBuyTab()
        {
            ShowTab(ShipyardTab.Buy);
        }

        void ShowTab(ShipyardTab tab)
        {
            SetPanelActive(orderPanel, tab == ShipyardTab.Order);
            SetPanelActive(queuePanel, tab == ShipyardTab.Queue);
            SetPanelActive(buyPanel, tab == ShipyardTab.Buy);

            SetTabState(orderTabButton, orderTabBackplate, tab == ShipyardTab.Order);
            SetTabState(queueTabButton, queueTabBackplate, tab == ShipyardTab.Queue);
            SetTabState(buyTabButton, buyTabBackplate, tab == ShipyardTab.Buy);
        }

        void SetTabState(Button button, Image backplate, bool selected)
        {
            if (button != null)
            {
                button.interactable = !selected;
            }

            if (backplate != null)
            {
                backplate.color = selected ? TabActiveColor : TabInactiveColor;
            }
        }

        void ShowPreviousShip()
        {
            int shipCount = config != null ? config.BuildableShips.Count : 0;
            if (shipCount == 0)
            {
                return;
            }

            selectedShipIndex = (selectedShipIndex - 1 + shipCount) % shipCount;
            ShowSelectedOrderShip();
        }

        void ShowNextShip()
        {
            int shipCount = config != null ? config.BuildableShips.Count : 0;
            if (shipCount == 0)
            {
                return;
            }

            selectedShipIndex = (selectedShipIndex + 1) % shipCount;
            ShowSelectedOrderShip();
        }

        void ShowPreviousBuyOffer()
        {
            int offerCount = config != null ? config.BuyOffers.Count : 0;
            if (offerCount == 0)
            {
                return;
            }

            int visibleCount = GetVisibleBuyCardCount(offerCount);
            buyOfferPageStartIndex -= visibleCount;
            if (buyOfferPageStartIndex < 0)
            {
                buyOfferPageStartIndex = GetLastBuyOfferPageStartIndex(offerCount, visibleCount);
            }

            RebuildVisibleBuyOffers();
        }

        void ShowNextBuyOffer()
        {
            int offerCount = config != null ? config.BuyOffers.Count : 0;
            if (offerCount == 0)
            {
                return;
            }

            int visibleCount = GetVisibleBuyCardCount(offerCount);
            buyOfferPageStartIndex += visibleCount;
            if (buyOfferPageStartIndex >= offerCount)
            {
                buyOfferPageStartIndex = 0;
            }

            RebuildVisibleBuyOffers();
        }

        void ShowSelectedOrderShip()
        {
            ShipyardShipDefinition ship = GetSelectedShip();
            Sprite sprite = ship != null ? ship.Image : null;
            if (orderShipImage != null)
            {
                orderShipImage.sprite = sprite;
                orderShipImage.enabled = sprite != null;
            }

            if (orderImageFallbackText != null)
            {
                orderImageFallbackText.gameObject.SetActive(sprite == null);
            }

            SetText(orderShipNameText, ship != null ? ship.DisplayName : "--");
            SetText(orderClassTagText, ship != null ? ship.ClassTag : "--");
            SetText(orderSpeedText, FormatStat("Speed", ship != null ? ship.SpeedText : "--"));
            SetText(orderHealthText, FormatStat("Health", ship != null ? ship.HealthText : "--"));
            SetText(orderCrewText, FormatStat("Crew", ship != null ? ship.CrewText : "--"));
            SetText(orderStorageText, FormatStat("Storage", ship != null ? ship.StorageText : "--"));
            SetText(orderOperationalCostText, FormatStat("Operational cost", ship != null ? ship.OperationalCostText : "--"));
            SetText(orderPriceText, ship != null ? ship.PriceText : "--");
            SetText(orderEstimatedBuildTimeText, ship != null ? ship.EstimatedBuildTimeText : "--");

            RebuildOrderMaterials(ship);
        }

        ShipyardShipDefinition GetSelectedShip()
        {
            if (config == null || config.BuildableShips.Count == 0)
            {
                return null;
            }

            selectedShipIndex = Mathf.Clamp(selectedShipIndex, 0, config.BuildableShips.Count - 1);
            return config.BuildableShips[selectedShipIndex];
        }

        void RebuildOrderMaterials(ShipyardShipDefinition ship)
        {
            if (orderMaterialsRoot != null)
            {
                for (int i = orderMaterialsRoot.childCount - 1; i >= 0; i--)
                {
                    Transform child = orderMaterialsRoot.GetChild(i);
                    if (orderMaterialTemplate == null || child != orderMaterialTemplate.transform)
                    {
                        DestroyObject(child.gameObject);
                    }
                }
            }

            orderMaterialRows.Clear();

            if (orderMaterialsRoot == null || orderMaterialTemplate == null || ship == null)
            {
                return;
            }

            IReadOnlyList<ShipyardMaterialRequirement> materials = ship.RequiredMaterials;
            for (int i = 0; i < materials.Count; i++)
            {
                ShipyardMaterialRow row = Instantiate(orderMaterialTemplate, orderMaterialsRoot);
                row.name = $"RequiredMaterial_{materials[i].Id}";
                row.gameObject.SetActive(true);
                row.Bind(materials[i], true);
                orderMaterialRows.Add(row);
            }
        }

        void RebuildConstructionQueue()
        {
            if (constructionQueueRoot != null)
            {
                for (int i = constructionQueueRoot.childCount - 1; i >= 0; i--)
                {
                    Transform child = constructionQueueRoot.GetChild(i);
                    if (constructionQueueTemplate == null || child != constructionQueueTemplate.transform)
                    {
                        DestroyObject(child.gameObject);
                    }
                }
            }

            constructionRows.Clear();
            int count = config != null ? config.ConstructionQueue.Count : 0;
            if (constructionEmptyText != null)
            {
                constructionEmptyText.gameObject.SetActive(count == 0);
            }

            if (underConstructionRow != null)
            {
                bool hasCurrent = count > 0;
                underConstructionRow.gameObject.SetActive(hasCurrent);
                if (hasCurrent)
                {
                    underConstructionRow.Bind(config.ConstructionQueue[0]);
                }
            }

            if (constructionQueueRoot == null || constructionQueueTemplate == null || config == null)
            {
                return;
            }

            for (int i = 1; i < config.ConstructionQueue.Count; i++)
            {
                ShipyardConstructionRow row = Instantiate(constructionQueueTemplate, constructionQueueRoot);
                row.name = $"ConstructionRow_{config.ConstructionQueue[i].Id}";
                row.gameObject.SetActive(true);
                row.Bind(config.ConstructionQueue[i]);
                constructionRows.Add(row);
            }
        }

        void RebuildVisibleBuyOffers()
        {
            if (buyCardsRoot != null)
            {
                for (int i = buyCardsRoot.childCount - 1; i >= 0; i--)
                {
                    Transform child = buyCardsRoot.GetChild(i);
                    if (buyCardTemplate == null || child != buyCardTemplate.transform)
                    {
                        DestroyObject(child.gameObject);
                    }
                }
            }

            buyCards.Clear();
            int count = config != null ? config.BuyOffers.Count : 0;
            bool hasOffers = count > 0;
            int visibleCount = GetVisibleBuyCardCount(count);
            if (hasOffers)
            {
                buyOfferPageStartIndex = Mathf.Clamp(buyOfferPageStartIndex, 0, GetLastBuyOfferPageStartIndex(count, visibleCount));
            }

            if (buyEmptyText != null)
            {
                buyEmptyText.gameObject.SetActive(!hasOffers);
            }

            if (buyCardsRoot != null)
            {
                buyCardsRoot.gameObject.SetActive(hasOffers);
            }

            bool hasOverflow = count > visibleCount;
            if (previousBuyOfferButton != null)
            {
                previousBuyOfferButton.gameObject.SetActive(hasOverflow);
                previousBuyOfferButton.interactable = hasOverflow;
            }

            if (nextBuyOfferButton != null)
            {
                nextBuyOfferButton.gameObject.SetActive(hasOverflow);
                nextBuyOfferButton.interactable = hasOverflow;
            }

            if (buyCarouselIndexText != null)
            {
                buyCarouselIndexText.gameObject.SetActive(hasOffers);
                int firstVisible = hasOffers ? buyOfferPageStartIndex + 1 : 0;
                int lastVisible = hasOffers ? Mathf.Min(count, buyOfferPageStartIndex + visibleCount) : 0;
                buyCarouselIndexText.text = hasOffers ? $"{firstVisible}-{lastVisible} / {count}" : string.Empty;
            }

            if (!hasOffers || buyCardsRoot == null || buyCardTemplate == null || config == null)
            {
                return;
            }

            int endIndex = Mathf.Min(count, buyOfferPageStartIndex + visibleCount);
            for (int i = buyOfferPageStartIndex; i < endIndex; i++)
            {
                ShipyardBuyOffer offer = config.BuyOffers[i];
                ShipyardBuyCard card = Instantiate(buyCardTemplate, buyCardsRoot);
                card.name = $"BuyShipCard_{offer.Id}";
                card.gameObject.SetActive(true);
                card.Bind(offer);
                buyCards.Add(card);
            }
        }

        int GetVisibleBuyCardCount(int offerCount)
        {
            if (offerCount <= 0)
            {
                return 0;
            }

            return Mathf.Min(offerCount, Mathf.Max(1, maxVisibleBuyCards));
        }

        static int GetLastBuyOfferPageStartIndex(int offerCount, int visibleCount)
        {
            if (offerCount <= 0 || visibleCount <= 0)
            {
                return 0;
            }

            int remainder = offerCount % visibleCount;
            if (remainder == 0)
            {
                return Mathf.Max(0, offerCount - visibleCount);
            }

            return offerCount - remainder;
        }

        static void SetPanelActive(RectTransform panel, bool active)
        {
            if (panel != null)
            {
                panel.gameObject.SetActive(active);
            }
        }

        static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        static string FormatStat(string label, string value)
        {
            return $"{label}     {value}";
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

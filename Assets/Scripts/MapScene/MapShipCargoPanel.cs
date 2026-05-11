using System;
using System.Collections.Generic;
using System.Globalization;
using Assets.Scripts.Data;
using Assets.Scripts.Data.market;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MapScene
{
    public sealed class MapShipCargoPanel : MonoBehaviour
    {
        const int SortingOrder = 50;
        const string UiAssetRoot = "Assets/Materials/UI/image_assets/";
        const string ShipTypeLabel = "Trade Cog";

        static readonly Color PanelBackgroundColor = new Color32(5, 24, 29, 246);
        static readonly Color DividerColor = new Color32(35, 61, 67, 210);
        static readonly Color CreamTextColor = new Color32(247, 231, 203, 255);
        static readonly Color MutedTextColor = new Color32(157, 173, 178, 255);
        static readonly Color RowTextColor = new Color32(246, 248, 248, 255);
        static readonly Color IconFrameColor = new Color32(6, 20, 25, 248);

        readonly Vector2 panelSize = new Vector2(270f, 500f);
        readonly Vector2 panelOffset = new Vector2(-36f, -112f);
        readonly List<CommodityLoad> visibleLoads = new List<CommodityLoad>();
        readonly Dictionary<CommodityType, Sprite> commodityIcons = new Dictionary<CommodityType, Sprite>();

        [SerializeField] Font regularFont;
        [SerializeField] Font boldFont;
        [SerializeField] Sprite topRailSprite;
        [SerializeField] Sprite bottomRailSprite;
        [SerializeField] Sprite cornerTopSprite;
        [SerializeField] Sprite cornerBottomSprite;
        [SerializeField] Sprite sideEdgeSprite;
        [SerializeField] Sprite ornamentPatternSprite;
        [SerializeField] Sprite emblemSprite;
        [SerializeField] Sprite iconFrameSprite;
        [SerializeField] Sprite defaultCommodityIcon;
        [SerializeField] Sprite woodCommodityIcon;
        [SerializeField] Sprite metalCommodityIcon;

        RectTransform root;
        RectTransform cargoRowsRoot;
        Text nameLabel;
        Text typeLabel;

        Ship displayedShip;
        int displayedSignature = int.MinValue;

        public void Initialize()
        {
            Build();
            Refresh(force: true);
        }

        void OnEnable()
        {
            ShipManager.instance.OnSelectionChanged += HandleSelectionChanged;
            Initialize();
        }

        void OnDisable()
        {
            ShipManager.instance.OnSelectionChanged -= HandleSelectionChanged;
        }

        void Update()
        {
            Refresh(force: false);
        }

        void HandleSelectionChanged(Ship ship)
        {
            Refresh(force: true);
        }

        void Build()
        {
            if (root != null)
            {
                return;
            }

            LoadAssets();

            GameObject canvasObject = new GameObject(
                "MapSelectedShipCargoCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            CanvasGroup canvasGroup = canvasObject.GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            GameObject panelObject = CreateChild(
                "SelectedShipCargoPanel",
                canvasObject.transform,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            root = (RectTransform)panelObject.transform;
            root.anchorMin = new Vector2(1f, 1f);
            root.anchorMax = new Vector2(1f, 1f);
            root.pivot = new Vector2(1f, 1f);
            root.anchoredPosition = panelOffset;
            root.sizeDelta = panelSize;

            Image panelBackground = panelObject.GetComponent<Image>();
            panelBackground.color = PanelBackgroundColor;
            panelBackground.raycastTarget = false;

            BuildFrame();
            BuildHeader();
            BuildCargoList();

            panelObject.SetActive(false);
        }

        void LoadAssets()
        {
            regularFont = regularFont != null ? regularFont : LoadFont("Assets/Fonts/Cardo/Cardo-Regular.ttf") ?? GetDefaultFont();
            boldFont = boldFont != null ? boldFont : LoadFont("Assets/Fonts/Cardo/Cardo-Bold.ttf") ?? regularFont ?? GetDefaultFont();

            topRailSprite = topRailSprite != null ? topRailSprite : LoadSprite(UiAssetRoot + "ui_container_middle_up.png");
            bottomRailSprite = bottomRailSprite != null ? bottomRailSprite : LoadSprite(UiAssetRoot + "ui_container_middle_down.png");
            cornerTopSprite = cornerTopSprite != null ? cornerTopSprite : LoadSprite(UiAssetRoot + "ui_container_corner_up.png");
            cornerBottomSprite = cornerBottomSprite != null ? cornerBottomSprite : LoadSprite(UiAssetRoot + "ui_container_corner_down.png");
            sideEdgeSprite = sideEdgeSprite != null ? sideEdgeSprite : LoadSprite(UiAssetRoot + "ui_container_edge_stretch.png");
            ornamentPatternSprite = ornamentPatternSprite != null ? ornamentPatternSprite : LoadSprite(UiAssetRoot + "ui_ornament_pattern.png");
            emblemSprite = emblemSprite != null ? emblemSprite : LoadSprite(UiAssetRoot + "guild_emblem_01.png");
            iconFrameSprite = iconFrameSprite != null ? iconFrameSprite : LoadSprite(UiAssetRoot + "container_input.png");
            defaultCommodityIcon = defaultCommodityIcon != null ? defaultCommodityIcon : LoadSprite(UiAssetRoot + "icon_box.png");
            woodCommodityIcon = woodCommodityIcon != null ? woodCommodityIcon : LoadSprite(UiAssetRoot + "icon_goods_active_wood.png");
            metalCommodityIcon = metalCommodityIcon != null ? metalCommodityIcon : LoadSprite(UiAssetRoot + "icon_goods_active_iron.png");

            if (woodCommodityIcon != null)
            {
                commodityIcons[CommodityType.Wood] = woodCommodityIcon;
            }

            if (metalCommodityIcon != null)
            {
                commodityIcons[CommodityType.Metal] = metalCommodityIcon;
            }
        }

        void BuildFrame()
        {
            AddImage("TopRail", root, topRailSprite, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(panelSize.x - 34f, 40f), new Vector2(0f, -10f));
            AddImage("BottomRail", root, bottomRailSprite, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(panelSize.x - 34f, 38f), new Vector2(0f, 10f));

            AddImage("TopLeftCorner", root, cornerTopSprite, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(35f, 46f), new Vector2(10f, -12f));
            AddImage("TopRightCorner", root, cornerTopSprite, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(35f, 46f), new Vector2(-10f, -12f), flipX: true);
            AddImage("BottomLeftCorner", root, cornerBottomSprite, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(35f, 44f), new Vector2(10f, 12f));
            AddImage("BottomRightCorner", root, cornerBottomSprite, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0.5f), new Vector2(35f, 44f), new Vector2(-10f, 12f), flipX: true);

            AddImage("LeftEdge", root, sideEdgeSprite, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(16f, panelSize.y - 54f), new Vector2(8f, 0f));
            AddImage("RightEdge", root, sideEdgeSprite, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(16f, panelSize.y - 54f), new Vector2(-8f, 0f), flipX: true);

            Image pattern = AddImage("HeaderOrnamentPattern", root, ornamentPatternSprite, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, -28f));
            RectTransform patternRect = (RectTransform)pattern.transform;
            patternRect.offsetMin = new Vector2(28f, 0f);
            patternRect.offsetMax = new Vector2(-28f, -92f);
            pattern.color = new Color32(255, 255, 255, 36);
        }

        void BuildHeader()
        {
            nameLabel = CreateText(
                "ShipName",
                root,
                boldFont,
                37,
                FontStyle.Bold,
                CreamTextColor,
                TextAnchor.MiddleLeft);
            SetAnchored(nameLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -62f), new Vector2(158f, 54f));
            nameLabel.resizeTextForBestFit = true;
            nameLabel.resizeTextMinSize = 16;
            nameLabel.resizeTextMaxSize = 32;

            typeLabel = CreateText(
                "ShipType",
                root,
                regularFont,
                22,
                FontStyle.Normal,
                MutedTextColor,
                TextAnchor.MiddleLeft);
            SetAnchored(typeLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -111f), new Vector2(150f, 34f));

            AddImage("GuildEmblem", root, emblemSprite, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(50f, 51f), new Vector2(-50f, -88f));

            Image divider = AddSolidImage("HeaderDivider", root, DividerColor);
            SetAnchored((RectTransform)divider.transform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            RectTransform dividerRect = (RectTransform)divider.transform;
            dividerRect.offsetMin = new Vector2(24f, -166f);
            dividerRect.offsetMax = new Vector2(-24f, -162f);
        }

        void BuildCargoList()
        {
            GameObject scrollObject = CreateChild("CargoList", root, typeof(RectTransform), typeof(ScrollRect));
            RectTransform scrollRectTransform = (RectTransform)scrollObject.transform;
            scrollRectTransform.anchorMin = Vector2.zero;
            scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = new Vector2(28f, 42f);
            scrollRectTransform.offsetMax = new Vector2(-28f, -190f);

            GameObject viewportObject = CreateChild(
                "Viewport",
                scrollObject.transform,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(RectMask2D));
            RectTransform viewport = (RectTransform)viewportObject.transform;
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;

            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = false;

            GameObject contentObject = CreateChild(
                "CargoRows",
                viewportObject.transform,
                typeof(RectTransform),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            cargoRowsRoot = (RectTransform)contentObject.transform;
            cargoRowsRoot.anchorMin = new Vector2(0f, 1f);
            cargoRowsRoot.anchorMax = new Vector2(1f, 1f);
            cargoRowsRoot.pivot = new Vector2(0.5f, 1f);
            cargoRowsRoot.anchoredPosition = Vector2.zero;
            cargoRowsRoot.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 14f;

            ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
            scrollRect.viewport = viewport;
            scrollRect.content = cargoRowsRoot;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = false;
            scrollRect.scrollSensitivity = 24f;
        }

        void Refresh(bool force)
        {
            if (root == null)
            {
                Build();
            }

            Ship selectedShip = ShipManager.instance.SelectedShip;
            bool hasSelectedShip = selectedShip != null;
            if (root.gameObject.activeSelf != hasSelectedShip)
            {
                root.gameObject.SetActive(hasSelectedShip);
            }

            if (!hasSelectedShip)
            {
                displayedShip = null;
                displayedSignature = int.MinValue;
                return;
            }

            int signature = CalculateSignature(selectedShip);
            if (!force && ReferenceEquals(displayedShip, selectedShip) && displayedSignature == signature)
            {
                return;
            }

            displayedShip = selectedShip;
            displayedSignature = signature;

            nameLabel.text = selectedShip.Name ?? string.Empty;
            typeLabel.text = ShipTypeLabel;
            RebuildCargoRows(selectedShip.load);
        }

        void RebuildCargoRows(FullLoad load)
        {
            if (cargoRowsRoot == null)
            {
                return;
            }

            for (int i = cargoRowsRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(cargoRowsRoot.GetChild(i).gameObject);
            }

            visibleLoads.Clear();
            if (load != null)
            {
                foreach (var pair in load.load)
                {
                    CommodityLoad commodityLoad = pair.Value;
                    if (commodityLoad != null && commodityLoad.Amount > 0)
                    {
                        visibleLoads.Add(commodityLoad);
                    }
                }
            }

            visibleLoads.Sort((left, right) => left.commodity.CompareTo(right.commodity));

            if (visibleLoads.Count == 0)
            {
                CreateEmptyCargoRow();
                return;
            }

            for (int i = 0; i < visibleLoads.Count; i++)
            {
                CreateCargoRow(visibleLoads[i]);
            }
        }

        void CreateCargoRow(CommodityLoad commodityLoad)
        {
            GameObject rowObject = CreateChild("CargoRow", cargoRowsRoot, typeof(RectTransform), typeof(LayoutElement));
            RectTransform rowRect = (RectTransform)rowObject.transform;
            rowRect.sizeDelta = new Vector2(0f, 53f);

            LayoutElement layoutElement = rowObject.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 53f;
            layoutElement.minHeight = 53f;

            Image iconFrame = AddImage(
                "IconFrame",
                rowRect,
                iconFrameSprite,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(43f, 43f),
                new Vector2(0f, 0f));
            iconFrame.color = IconFrameColor;

            Sprite iconSprite = GetCommodityIcon(commodityLoad.commodity);
            AddImage(
                "CommodityIcon",
                iconFrame.transform,
                iconSprite,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(32f, 32f),
                Vector2.zero);

            Text commodityLabel = CreateText(
                "CommodityName",
                rowRect,
                boldFont,
                20,
                FontStyle.Bold,
                RowTextColor,
                TextAnchor.MiddleLeft);
            RectTransform commodityLabelRect = commodityLabel.rectTransform;
            commodityLabelRect.anchorMin = Vector2.zero;
            commodityLabelRect.anchorMax = Vector2.one;
            commodityLabelRect.offsetMin = new Vector2(58f, 0f);
            commodityLabelRect.offsetMax = new Vector2(-66f, 0f);
            commodityLabel.text = FormatCommodityName(commodityLoad.commodity);

            Text amountLabel = CreateText(
                "CommodityAmount",
                rowRect,
                boldFont,
                24,
                FontStyle.Bold,
                RowTextColor,
                TextAnchor.MiddleRight);
            SetAnchored(amountLabel.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(61f, 0f));
            amountLabel.text = commodityLoad.Amount.ToString(CultureInfo.InvariantCulture);
        }

        void CreateEmptyCargoRow()
        {
            GameObject rowObject = CreateChild("EmptyCargoRow", cargoRowsRoot, typeof(RectTransform), typeof(LayoutElement));
            LayoutElement layoutElement = rowObject.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = 62f;
            layoutElement.minHeight = 62f;

            Text emptyLabel = CreateText(
                "EmptyCargoLabel",
                rowObject.transform,
                regularFont,
                19,
                FontStyle.Normal,
                MutedTextColor,
                TextAnchor.MiddleCenter);
            SetAnchored(emptyLabel.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            emptyLabel.rectTransform.offsetMin = Vector2.zero;
            emptyLabel.rectTransform.offsetMax = Vector2.zero;
            emptyLabel.text = "No cargo";
        }

        int CalculateSignature(Ship ship)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ship.ID;
                hash = hash * 31 + (ship.Name != null ? StringComparer.Ordinal.GetHashCode(ship.Name) : 0);

                Array commodities = Enum.GetValues(typeof(CommodityType));
                for (int i = 0; i < commodities.Length; i++)
                {
                    CommodityType commodity = (CommodityType)commodities.GetValue(i);
                    int amount = ship.load != null ? ship.load.GetAmount(commodity) : 0;
                    hash = hash * 31 + (int)commodity;
                    hash = hash * 31 + amount;
                }

                return hash;
            }
        }

        Sprite GetCommodityIcon(CommodityType commodity)
        {
            if (commodityIcons.TryGetValue(commodity, out Sprite sprite) && sprite != null)
            {
                return sprite;
            }

            return defaultCommodityIcon;
        }

        static string FormatCommodityName(CommodityType commodity)
        {
            return commodity == CommodityType.Metal ? "Iron" : commodity.ToString();
        }

        static GameObject CreateChild(string name, Transform parent, params Type[] components)
        {
            GameObject child = new GameObject(name, components);
            child.transform.SetParent(parent, false);
            return child;
        }

        Image AddImage(
            string name,
            Transform parent,
            Sprite sprite,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta,
            Vector2 anchoredPosition,
            bool flipX = false)
        {
            GameObject imageObject = CreateChild(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = (RectTransform)imageObject.transform;
            SetAnchored(rect, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta);
            if (flipX)
            {
                rect.localScale = new Vector3(-1f, 1f, 1f);
            }

            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            if (sprite == null)
            {
                image.color = Color.clear;
            }

            image.preserveAspect = false;
            image.raycastTarget = false;
            return image;
        }

        static Image AddSolidImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = CreateChild(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        Text CreateText(
            string name,
            Transform parent,
            Font font,
            int fontSize,
            FontStyle fontStyle,
            Color color,
            TextAnchor alignment)
        {
            GameObject textObject = CreateChild(name, parent, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = fontStyle;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        static void SetAnchored(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        static Font GetDefaultFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        static Sprite LoadSprite(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
#else
            return null;
#endif
        }

        static Font LoadFont(string assetPath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(assetPath);
#else
            return null;
#endif
        }
    }
}

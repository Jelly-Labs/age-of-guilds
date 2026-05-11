using System.Globalization;
using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public sealed class PersistentHeaderUI : MonoBehaviour
    {
        private const string GuildBannerResourcePath = "PersistentHeader/PersistentHeaderGuildBanner";
        private const string MiddleBannerResourcePath = "PersistentHeader/PersistentHeaderTownBanner";
        private const string PlayerBannerResourcePath = "PersistentHeader/PersistentHeaderPlayerBanner";
        private const int HeaderSortingOrder = 100;

        private static PersistentHeaderUI instance;

        private Text dateLabel;
        private Text goldAmountLabel;
        private Text townNameLabel;
        private Image townBannerImage;
        private GameObject townBannerRoot;

        private string lastDateText;
        private double lastGoldValue = double.MinValue;
        private string lastTownName;
        private Sprite lastTownBanner;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (instance != null)
            {
                return;
            }

            GameObject root = new("PersistentHeaderUI");
            instance = root.AddComponent<PersistentHeaderUI>();
            DontDestroyOnLoad(root);
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            BuildCanvas();
            GameData.instance.selectedTownChanged += HandleSelectedTownChanged;
            RefreshDate(force: true);
            RefreshTown(GameData.instance.CurrentTown, force: true);
            RefreshGold(force: true);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                GameData.instance.selectedTownChanged -= HandleSelectedTownChanged;
                instance = null;
            }
        }

        private void Update()
        {
            RefreshDate(force: false);
            RefreshGold(force: false);
        }

        private void BuildCanvas()
        {
            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = HeaderSortingOrder;

            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            gameObject.AddComponent<GraphicRaycaster>();

            CanvasGroup canvasGroup = gameObject.AddComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            GameObject barObject = CreateChild("HeaderBar", transform);
            RectTransform barRect = barObject.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(0.5f, 1f);
            barRect.offsetMin = new Vector2(0f, -84f);
            barRect.offsetMax = Vector2.zero;

            GameObject middleBanner = InstantiateHeaderElement(MiddleBannerResourcePath, barRect);
            if (middleBanner != null)
            {
                dateLabel = FindChildText(middleBanner.transform, "DateText");
                townBannerRoot = middleBanner.transform.Find("TownBanner")?.gameObject;
                townNameLabel = FindChildText(middleBanner.transform, "TownNameText");
                townBannerImage = middleBanner.transform.Find("TownBannerBackground")?.GetComponent<Image>();
            }

            InstantiateHeaderElement(GuildBannerResourcePath, barRect);

            GameObject playerBanner = InstantiateHeaderElement(PlayerBannerResourcePath, barRect);
            if (playerBanner != null)
            {
                goldAmountLabel = FindChildText(playerBanner.transform, "GoldAmount");
            }
        }

        private static GameObject CreateChild(string name, Transform parent)
        {
            GameObject child = new(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static GameObject InstantiateHeaderElement(string resourcePath, Transform parent)
        {
            GameObject prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                return null;
            }

            GameObject instanceObject = Instantiate(prefab, parent, false);
            instanceObject.name = prefab.name;
            return instanceObject;
        }

        private static Text FindChildText(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            Transform target = root.Find(objectName);
            if (target != null)
            {
                return target.GetComponent<Text>();
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Text nestedText = FindChildText(root.GetChild(i), objectName);
                if (nestedText != null)
                {
                    return nestedText;
                }
            }

            return null;
        }

        private void HandleSelectedTownChanged(Town town)
        {
            RefreshTown(town, force: true);
        }

        private void RefreshDate(bool force)
        {
            if (dateLabel == null)
            {
                return;
            }
         
            string dateText = GameData.instance.currentDate.ToString("MMMM d, yyyy");
            bool shouldUpdate = force || dateText != lastDateText;
            if (shouldUpdate)
            {
                lastDateText = dateText;
                dateLabel.text = dateText;
            }
        }

        private void RefreshTown(Town currentTown, bool force)
        {
            string townName = currentTown?.name ?? string.Empty;

            bool showTown = currentTown != null;
            if (townBannerRoot != null)
            {
                townBannerRoot.SetActive(showTown);
            }

            if (townNameLabel != null && (force || townName != lastTownName))
            {
                lastTownName = townName;
                townNameLabel.text = townName;
            }

            if (townBannerImage == null)
            {
                return;
            }

            if (currentTown == null)
            {
                lastTownBanner = null;
                townBannerImage.sprite = null;
                return;
            }

            if (currentTown.banner == null)
            {
                lastTownBanner = null;
                townBannerImage.sprite = null;
                return;
            }

            if (force || currentTown.banner != lastTownBanner)
            {
                lastTownBanner = currentTown.banner;
                townBannerImage.sprite = currentTown.banner;
            }
        }

        private void RefreshGold(bool force)
        {
            if (goldAmountLabel == null)
            {
                return;
            }

            double goldValue = GameData.instance.gold;
            if (!force && Mathf.Approximately((float)(goldValue - lastGoldValue), 0f))
            {
                return;
            }

            lastGoldValue = goldValue;
            goldAmountLabel.text = goldValue.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
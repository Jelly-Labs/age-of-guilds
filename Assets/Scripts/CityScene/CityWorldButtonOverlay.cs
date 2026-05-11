using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.CityScene
{
    internal static class CityWorldButtonUvUtility
    {
        public static Rect CalculateUvRect(Sprite sprite, Sprite referenceSprite)
        {
            if (sprite == null || referenceSprite == null)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            Texture texture = sprite.texture;
            if (texture == null)
            {
                return new Rect(0f, 0f, 1f, 1f);
            }

            Rect textureRect = sprite.textureRect;
            float normalizedX = textureRect.x / texture.width;
            float normalizedY = textureRect.y / texture.height;
            float normalizedWidth = textureRect.width / texture.width;
            float normalizedHeight = textureRect.height / texture.height;

            float targetHeight = Mathf.Min(referenceSprite.rect.height, sprite.rect.height);
            float croppedHeight = (targetHeight / sprite.rect.height) * normalizedHeight;
            float cropOffset = (normalizedHeight - croppedHeight) * 0.5f;

            return new Rect(normalizedX, normalizedY + cropOffset, normalizedWidth, croppedHeight);
        }
    }

    public sealed class CityWorldButtonOverlay : MonoBehaviour
    {
        private const string ResourcePath = "CityScene/CityWorldButtonOverlay";
        private const string TargetSceneName = "CityScene";
        private const int SortingOrder = 120;

        private static CityWorldButtonOverlay instance;

        [SerializeField] private Sprite normalSprite;
        [SerializeField] private Sprite hoverSprite;

        private GameObject overlayRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (instance != null)
            {
                return;
            }

            GameObject prefab = Resources.Load<GameObject>(ResourcePath);
            if (prefab == null)
            {
                return;
            }

            GameObject root = Instantiate(prefab);
            root.name = prefab.name;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += HandleSceneLoaded;
                HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (instance != this)
            {
                return;
            }

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            instance = null;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == TargetSceneName)
            {
                EnsureOverlay();
                return;
            }

            DestroyOverlay();
        }

        private void EnsureOverlay()
        {
            if (overlayRoot != null)
            {
                return;
            }

            overlayRoot = new GameObject("CityWorldButtonCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            overlayRoot.transform.SetParent(transform, false);

            Canvas canvas = overlayRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            CanvasScaler scaler = overlayRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1366f, 768f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = (RectTransform)overlayRoot.transform;
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            CreateButton(canvasRect);
        }

        private void DestroyOverlay()
        {
            if (overlayRoot == null)
            {
                return;
            }

            Destroy(overlayRoot);
            overlayRoot = null;
        }

        private void CreateButton(RectTransform parent)
        {
            GameObject buttonObject = new GameObject("ToWorldButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CityWorldButtonView));
            buttonObject.transform.SetParent(parent, false);

            RectTransform buttonRect = (RectTransform)buttonObject.transform;
            buttonRect.anchorMin = new Vector2(1f, 0f);
            buttonRect.anchorMax = new Vector2(1f, 0f);
            buttonRect.pivot = new Vector2(1f, 0f);
            buttonRect.anchoredPosition = new Vector2(-26f, 24f);

            if (normalSprite != null)
            {
                buttonRect.sizeDelta = new Vector2(normalSprite.rect.width, normalSprite.rect.height);
            }

            Image buttonBackground = buttonObject.GetComponent<Image>();
            buttonBackground.color = new Color(1f, 1f, 1f, 0f);
            buttonBackground.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;

            GameObject artObject = new GameObject("Art", typeof(RectTransform), typeof(RawImage));
            artObject.transform.SetParent(buttonObject.transform, false);
            RectTransform artRect = (RectTransform)artObject.transform;
            artRect.anchorMin = Vector2.zero;
            artRect.anchorMax = Vector2.one;
            artRect.offsetMin = Vector2.zero;
            artRect.offsetMax = Vector2.zero;

            RawImage art = artObject.GetComponent<RawImage>();
            art.texture = normalSprite != null ? normalSprite.texture : null;
            art.color = Color.white;
            art.raycastTarget = false;
            art.uvRect = CityWorldButtonUvUtility.CalculateUvRect(normalSprite, normalSprite);

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = new Vector2(0.1f, 0.5f);
            labelRect.anchorMax = new Vector2(0.56f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.sizeDelta = new Vector2(0f, 22f);
            labelRect.anchoredPosition = new Vector2(0f, -2f);

            Text label = labelObject.GetComponent<Text>();
            label.text = "To world";
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 16;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.9f, 0.76f, 0.5f, 1f);
            label.raycastTarget = false;

            CityWorldButtonView view = buttonObject.GetComponent<CityWorldButtonView>();
            view.Initialize(button, art, label, normalSprite, hoverSprite);
        }
    }

    public sealed class CityWorldButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Button button;
        private RawImage art;
        private Text label;
        private Sprite normalSprite;
        private Sprite hoverSprite;

        public void Initialize(Button targetButton, RawImage targetArt, Text targetLabel, Sprite defaultIcon, Sprite highlightedIcon)
        {
            button = targetButton;
            art = targetArt;
            label = targetLabel;
            normalSprite = defaultIcon;
            hoverSprite = highlightedIcon != null ? highlightedIcon : defaultIcon;

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
                button.onClick.AddListener(HandleClick);
            }

            ApplyState(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ApplyState(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ApplyState(false);
        }

        private void HandleClick()
        {
            GameData.instance.CurrentTown = null;
            SceneManager.LoadScene("MapScene");
        }

        private void ApplyState(bool hovered)
        {
            if (art != null)
            {
                Sprite activeSprite = hovered ? hoverSprite : normalSprite;
                art.texture = activeSprite != null ? activeSprite.texture : null;
                // art.uvRect = CityWorldButtonUvUtility.CalculateUvRect(activeSprite, normalSprite);
            }

            if (label != null)
            {
                label.color = hovered
                    ? new Color(1f, 0.84f, 0.56f, 1f)
                    : new Color(0.9f, 0.76f, 0.5f, 1f);
            }
        }

    }
}

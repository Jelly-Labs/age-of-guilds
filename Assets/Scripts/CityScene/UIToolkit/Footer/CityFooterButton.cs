using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.CityScene
{
    [DisallowMultipleComponent]
    public class CityFooterButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        static Font cachedFont;

        [Header("Entry")]
        [SerializeField] string entryId;
        [SerializeField] string displayName;
        [SerializeField] string targetBuildingId;
        [SerializeField] bool isAvailable = true;

        [Header("References")]
        [SerializeField] Button button;
        [SerializeField] Image backplateImage;
        [SerializeField] Image iconImage;
        [SerializeField] Text glyphText;
        [SerializeField] Text labelText;

        [Header("Exchangeable Sprites")]
        [SerializeField] Sprite defaultSprite;
        [SerializeField] Sprite highlightedSprite;
        [SerializeField] Sprite selectedSprite;
        [SerializeField] Sprite disabledSprite;

        [Header("State Tints")]
        [SerializeField] Color defaultTint = new Color(0.86f, 0.75f, 0.55f, 1f);
        [SerializeField] Color highlightedTint = new Color(1f, 0.86f, 0.58f, 1f);
        [SerializeField] Color selectedTint = new Color(1f, 0.78f, 0.38f, 1f);
        [SerializeField] Color disabledTint = new Color(0.32f, 0.32f, 0.34f, 1f);
        [SerializeField] Color defaultBackplate = new Color(0.025f, 0.024f, 0.022f, 0.94f);
        [SerializeField] Color highlightedBackplate = new Color(0.065f, 0.06f, 0.052f, 0.98f);
        [SerializeField] Color selectedBackplate = new Color(0.09f, 0.08f, 0.064f, 1f);
        [SerializeField] Color disabledBackplate = new Color(0.018f, 0.018f, 0.018f, 0.86f);

        CityUIView owner;
        CityBuilding targetBuilding;
        bool isSelected;
        bool isHovered;

        public string EntryId => entryId;
        public string DisplayName => displayName;
        public string TargetBuildingId => targetBuildingId;
        public bool IsAvailable => isAvailable;
        public CityBuilding TargetBuilding => targetBuilding;
        public Button Button => button;

        void Awake()
        {
            EnsureReferences();
            if (button != null)
            {
                button.onClick.RemoveListener(Click);
                button.onClick.AddListener(Click);
            }

            ApplyVisualState();
        }

        public void Configure(string newEntryId, string newDisplayName, string newTargetBuildingId, bool newIsAvailable)
        {
            entryId = newEntryId;
            displayName = newDisplayName;
            targetBuildingId = newTargetBuildingId;
            isAvailable = newIsAvailable;
            EnsureReferences();
            ApplyText();
            ApplyVisualState();
        }

        public void SetReferences(Button newButton, Image newBackplateImage, Image newIconImage, Text newGlyphText, Text newLabelText)
        {
            button = newButton;
            backplateImage = newBackplateImage;
            iconImage = newIconImage;
            glyphText = newGlyphText;
            labelText = newLabelText;
            EnsureReferences();
            ApplyText();
            ApplyVisualState();
        }

        public void Bind(CityUIView newOwner, CityBuilding newTargetBuilding)
        {
            owner = newOwner;
            targetBuilding = newTargetBuilding;
            EnsureReferences();
            ApplyText();
            ApplyVisualState();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            ApplyVisualState();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            ApplyVisualState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            ApplyVisualState();
        }

        protected virtual void Click()
        {
            owner?.HandleFooterButtonClicked(this);
        }

        void EnsureReferences()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (backplateImage == null)
            {
                backplateImage = GetComponent<Image>();
            }
        }

        void ApplyText()
        {
            if (labelText != null)
            {
                labelText.text = displayName;
                labelText.font = GetDefaultFont();
            }

            if (glyphText != null)
            {
                glyphText.text = string.IsNullOrWhiteSpace(displayName)
                    ? "?"
                    : displayName.Substring(0, Mathf.Min(displayName.Length, 2)).ToUpperInvariant();
                glyphText.font = GetDefaultFont();
            }
        }

        void ApplyVisualState()
        {
            bool active = isAvailable && isSelected;
            bool highlighted = isAvailable && isHovered;

            Sprite sprite = defaultSprite;
            Color iconTint = defaultTint;
            Color backplateTint = defaultBackplate;

            if (!isAvailable)
            {
                sprite = disabledSprite != null ? disabledSprite : defaultSprite;
                iconTint = disabledTint;
                backplateTint = disabledBackplate;
            }
            else if (active)
            {
                sprite = selectedSprite != null ? selectedSprite : highlightedSprite != null ? highlightedSprite : defaultSprite;
                iconTint = selectedTint;
                backplateTint = selectedBackplate;
            }
            else if (highlighted)
            {
                sprite = highlightedSprite != null ? highlightedSprite : defaultSprite;
                iconTint = highlightedTint;
                backplateTint = highlightedBackplate;
            }

            if (button != null)
            {
                button.interactable = isAvailable;
            }

            if (backplateImage != null)
            {
                backplateImage.color = backplateTint;
            }

            if (iconImage != null)
            {
                iconImage.sprite = sprite;
                iconImage.enabled = sprite != null;
                iconImage.color = iconTint;
            }

            if (glyphText != null)
            {
                glyphText.enabled = sprite == null;
                glyphText.color = iconTint;
            }

            if (labelText != null)
            {
                labelText.color = iconTint;
            }
        }

        static Font GetDefaultFont()
        {
            if (cachedFont != null)
            {
                return cachedFont;
            }

            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (cachedFont == null)
            {
                cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return cachedFont;
        }
    }
}

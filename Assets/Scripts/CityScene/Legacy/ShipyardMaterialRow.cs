using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CityScene
{
    [DisallowMultipleComponent]
    public class ShipyardMaterialRow : MonoBehaviour
    {
        static Font cachedFont;

        [SerializeField] Image iconImage;
        [SerializeField] Text fallbackGlyphText;
        [SerializeField] Text labelText;
        [SerializeField] Text amountText;
        [SerializeField] Color normalTextColor = new Color(0.92f, 0.92f, 0.9f, 1f);
        [SerializeField] Color warningTextColor = new Color(1f, 0.28f, 0.28f, 1f);

        public Image IconImage => iconImage;
        public Text FallbackGlyphText => fallbackGlyphText;
        public Text LabelText => labelText;
        public Text AmountText => amountText;

        public void SetReferences(Image icon, Text fallbackGlyph, Text label, Text amount)
        {
            iconImage = icon;
            fallbackGlyphText = fallbackGlyph;
            labelText = label;
            amountText = amount;
        }

        public void Bind(ShipyardMaterialRequirement material, bool showLabel)
        {
            Sprite sprite = material != null ? material.Icon : null;
            if (iconImage != null)
            {
                iconImage.sprite = sprite;
                iconImage.enabled = sprite != null;
            }

            if (fallbackGlyphText != null)
            {
                fallbackGlyphText.enabled = sprite == null;
                fallbackGlyphText.font = GetDefaultFont();
                fallbackGlyphText.text = CreateFallbackGlyph(material != null ? material.Id : string.Empty);
            }

            if (labelText != null)
            {
                labelText.font = GetDefaultFont();
                labelText.text = material != null ? material.DisplayLabel : "--";
                labelText.gameObject.SetActive(showLabel);
            }

            if (amountText != null)
            {
                amountText.font = GetDefaultFont();
                amountText.text = material != null ? material.AmountText : "--";
                amountText.color = material != null && material.Warning ? warningTextColor : normalTextColor;
            }
        }

        static string CreateFallbackGlyph(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return "?";
            }

            return id.Substring(0, Mathf.Min(2, id.Length)).ToUpperInvariant();
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

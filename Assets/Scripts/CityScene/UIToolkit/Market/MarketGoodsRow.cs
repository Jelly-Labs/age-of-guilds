using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CityScene
{
    [DisallowMultipleComponent]
    public class MarketGoodsRow : MonoBehaviour
    {
        static Font cachedFont;

        [Header("Good")]
        [SerializeField] Image iconImage;
        [SerializeField] Text fallbackGlyphText;

        [Header("Numbers")]
        [SerializeField] Text stockText;
        [SerializeField] Text priceText;
        [SerializeField] Text amountText;
        [SerializeField] Text cargoText;
        [SerializeField] Text boughtForText;

        [Header("Controls")]
        [SerializeField] Button decreaseButton;
        [SerializeField] Button increaseButton;
        [SerializeField] Slider amountSlider;

        public Text StockText => stockText;
        public Text PriceText => priceText;
        public Text AmountText => amountText;
        public Text CargoText => cargoText;
        public Text BoughtForText => boughtForText;
        public Button DecreaseButton => decreaseButton;
        public Button IncreaseButton => increaseButton;
        public Slider AmountSlider => amountSlider;

        public void SetReferences(
            Image icon,
            Text fallbackGlyph,
            Text stock,
            Text price,
            Text amount,
            Text cargo,
            Text boughtFor,
            Button decrease,
            Button increase,
            Slider slider)
        {
            iconImage = icon;
            fallbackGlyphText = fallbackGlyph;
            stockText = stock;
            priceText = price;
            amountText = amount;
            cargoText = cargo;
            boughtForText = boughtFor;
            decreaseButton = decrease;
            increaseButton = increase;
            amountSlider = slider;
        }

        public void Bind(MarketGoodEntry good)
        {
            Sprite sprite = good != null ? good.Icon : null;
            if (iconImage != null)
            {
                iconImage.sprite = sprite;
                iconImage.enabled = sprite != null;
            }

            if (fallbackGlyphText != null)
            {
                fallbackGlyphText.enabled = sprite == null;
                fallbackGlyphText.font = GetDefaultFont();
                fallbackGlyphText.text = CreateFallbackGlyph(good != null ? good.Id : string.Empty);
            }

            ApplyPlaceholderNumbers();
        }

        public void ApplyPlaceholderNumbers()
        {
            SetText(stockText, "--");
            SetText(priceText, "--");
            SetText(amountText, "0");
            SetText(cargoText, "--");
            SetText(boughtForText, "--");

            if (amountSlider != null)
            {
                amountSlider.minValue = -100f;
                amountSlider.maxValue = 100f;
                amountSlider.wholeNumbers = true;
                amountSlider.value = 0f;
            }
        }

        static void SetText(Text target, string value)
        {
            if (target != null)
            {
                target.text = value;
                target.font = GetDefaultFont();
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

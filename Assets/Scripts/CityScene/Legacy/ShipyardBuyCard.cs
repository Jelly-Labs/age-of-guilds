using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CityScene
{
    [DisallowMultipleComponent]
    public class ShipyardBuyCard : MonoBehaviour
    {
        static Font cachedFont;

        [SerializeField] Image shipImage;
        [SerializeField] Text imageFallbackText;
        [SerializeField] Text nameText;
        [SerializeField] Text classTagText;
        [SerializeField] Text speedText;
        [SerializeField] Text healthText;
        [SerializeField] Text crewText;
        [SerializeField] Text storageText;
        [SerializeField] Text operationalCostText;
        [SerializeField] Text priceText;
        [SerializeField] Text ownedCountText;
        [SerializeField] Button buyButton;

        public Image ShipImage => shipImage;
        public Text ImageFallbackText => imageFallbackText;
        public Text NameText => nameText;
        public Text ClassTagText => classTagText;
        public Text SpeedText => speedText;
        public Text HealthText => healthText;
        public Text CrewText => crewText;
        public Text StorageText => storageText;
        public Text OperationalCostText => operationalCostText;
        public Text PriceText => priceText;
        public Text OwnedCountText => ownedCountText;
        public Button BuyButton => buyButton;

        public void SetReferences(
            Image image,
            Text imageFallback,
            Text name,
            Text classTag,
            Text speed,
            Text health,
            Text crew,
            Text storage,
            Text operationalCost,
            Text price,
            Text ownedCount,
            Button buy)
        {
            shipImage = image;
            imageFallbackText = imageFallback;
            nameText = name;
            classTagText = classTag;
            speedText = speed;
            healthText = health;
            crewText = crew;
            storageText = storage;
            operationalCostText = operationalCost;
            priceText = price;
            ownedCountText = ownedCount;
            buyButton = buy;
        }

        public void Bind(ShipyardBuyOffer offer)
        {
            Sprite sprite = offer != null ? offer.Image : null;
            if (shipImage != null)
            {
                shipImage.sprite = sprite;
                shipImage.enabled = sprite != null;
            }

            if (imageFallbackText != null)
            {
                imageFallbackText.font = GetDefaultFont();
                imageFallbackText.gameObject.SetActive(sprite == null);
            }

            SetText(nameText, offer != null ? offer.DisplayName : "--");
            SetText(classTagText, offer != null ? offer.ClassTag : "--");
            SetText(speedText, FormatStat("Speed", offer != null ? offer.SpeedText : "--"));
            SetText(healthText, FormatStat("Health", offer != null ? offer.HealthText : "--"));
            SetText(crewText, FormatStat("Crew", offer != null ? offer.CrewText : "--"));
            SetText(storageText, FormatStat("Storage", offer != null ? offer.StorageText : "--"));
            SetText(operationalCostText, FormatStat("Operational cost", offer != null ? offer.OperationalCostText : "--"));
            SetText(priceText, $"Buy  {(offer != null ? offer.PriceText : "--")}");
            SetText(ownedCountText, $"You have: {(offer != null ? offer.OwnedCountText : "--")}");
        }

        static void SetText(Text target, string value)
        {
            if (target == null)
            {
                return;
            }

            target.font = GetDefaultFont();
            target.text = value;
        }

        static string FormatStat(string label, string value)
        {
            return $"{label}     {value}";
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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.CityScene
{
    [CreateAssetMenu(fileName = "ShipyardConfig", menuName = "Age of Guilds/City/Shipyard Config")]
    public class ShipyardConfig : ScriptableObject
    {
        [SerializeField] List<ShipyardShipDefinition> buildableShips = new();
        [SerializeField] List<ShipyardConstructionEntry> constructionQueue = new();
        [SerializeField] List<ShipyardBuyOffer> buyOffers = new();

        public IReadOnlyList<ShipyardShipDefinition> BuildableShips => buildableShips;
        public IReadOnlyList<ShipyardConstructionEntry> ConstructionQueue => constructionQueue;
        public IReadOnlyList<ShipyardBuyOffer> BuyOffers => buyOffers;

#if UNITY_EDITOR
        public void AddDefaultData(
            ShipyardShipDefinition[] defaultShips,
            ShipyardConstructionEntry[] defaultConstructionQueue,
            ShipyardBuyOffer[] defaultBuyOffers)
        {
            if (buildableShips.Count == 0 && defaultShips != null)
            {
                buildableShips.AddRange(defaultShips);
            }

            if (constructionQueue.Count == 0 && defaultConstructionQueue != null)
            {
                constructionQueue.AddRange(defaultConstructionQueue);
            }

            if (buyOffers.Count == 0 && defaultBuyOffers != null)
            {
                buyOffers.AddRange(defaultBuyOffers);
            }
        }
#endif
    }

    [Serializable]
    public class ShipyardShipDefinition
    {
        [SerializeField] string id = "ship";
        [SerializeField] string displayName = "Ship";
        [SerializeField] string classTag = "Trade";
        [SerializeField] Sprite image;
        [SerializeField] string speedText = "--";
        [SerializeField] string healthText = "--";
        [SerializeField] string crewText = "--";
        [SerializeField] string storageText = "--";
        [SerializeField] string operationalCostText = "--";
        [SerializeField] string priceText = "--";
        [SerializeField] string estimatedBuildTimeText = "--";
        [SerializeField] List<ShipyardMaterialRequirement> requiredMaterials = new();

        public string Id => id;
        public string DisplayName => displayName;
        public string ClassTag => classTag;
        public Sprite Image => image;
        public string SpeedText => speedText;
        public string HealthText => healthText;
        public string CrewText => crewText;
        public string StorageText => storageText;
        public string OperationalCostText => operationalCostText;
        public string PriceText => priceText;
        public string EstimatedBuildTimeText => estimatedBuildTimeText;
        public IReadOnlyList<ShipyardMaterialRequirement> RequiredMaterials => requiredMaterials;

        public ShipyardShipDefinition()
        {
        }

        public ShipyardShipDefinition(
            string id,
            string displayName,
            string classTag,
            Sprite image,
            string speedText,
            string healthText,
            string crewText,
            string storageText,
            string operationalCostText,
            string priceText,
            string estimatedBuildTimeText,
            IEnumerable<ShipyardMaterialRequirement> requiredMaterials)
        {
            this.id = id;
            this.displayName = displayName;
            this.classTag = classTag;
            this.image = image;
            this.speedText = speedText;
            this.healthText = healthText;
            this.crewText = crewText;
            this.storageText = storageText;
            this.operationalCostText = operationalCostText;
            this.priceText = priceText;
            this.estimatedBuildTimeText = estimatedBuildTimeText;
            this.requiredMaterials = requiredMaterials != null ? new List<ShipyardMaterialRequirement>(requiredMaterials) : new List<ShipyardMaterialRequirement>();
        }
    }

    [Serializable]
    public class ShipyardMaterialRequirement
    {
        [SerializeField] string id = "material";
        [SerializeField] string displayLabel = "Material";
        [SerializeField] Sprite icon;
        [SerializeField] string amountText = "--";
        [SerializeField] bool warning;

        public string Id => id;
        public string DisplayLabel => displayLabel;
        public Sprite Icon => icon;
        public string AmountText => amountText;
        public bool Warning => warning;

        public ShipyardMaterialRequirement()
        {
        }

        public ShipyardMaterialRequirement(string id, string displayLabel, Sprite icon, string amountText, bool warning = false)
        {
            this.id = id;
            this.displayLabel = displayLabel;
            this.icon = icon;
            this.amountText = amountText;
            this.warning = warning;
        }
    }

    [Serializable]
    public class ShipyardConstructionEntry
    {
        [SerializeField] string id = "construction";
        [SerializeField] string shipName = "Ship";
        [SerializeField] string ownerName = "--";
        [SerializeField] string estimatedBuildTimeText = "--";
        [SerializeField] string statusMessage = string.Empty;
        [SerializeField, Range(0f, 1f)] float progress = 0f;
        [SerializeField] List<ShipyardMaterialRequirement> materialStatus = new();

        public string Id => id;
        public string ShipName => shipName;
        public string OwnerName => ownerName;
        public string EstimatedBuildTimeText => estimatedBuildTimeText;
        public string StatusMessage => statusMessage;
        public float Progress => progress;
        public IReadOnlyList<ShipyardMaterialRequirement> MaterialStatus => materialStatus;

        public ShipyardConstructionEntry()
        {
        }

        public ShipyardConstructionEntry(
            string id,
            string shipName,
            string ownerName,
            string estimatedBuildTimeText,
            string statusMessage,
            float progress,
            IEnumerable<ShipyardMaterialRequirement> materialStatus)
        {
            this.id = id;
            this.shipName = shipName;
            this.ownerName = ownerName;
            this.estimatedBuildTimeText = estimatedBuildTimeText;
            this.statusMessage = statusMessage;
            this.progress = Mathf.Clamp01(progress);
            this.materialStatus = materialStatus != null ? new List<ShipyardMaterialRequirement>(materialStatus) : new List<ShipyardMaterialRequirement>();
        }
    }

    [Serializable]
    public class ShipyardBuyOffer
    {
        [SerializeField] string id = "offer";
        [SerializeField] string displayName = "Ship";
        [SerializeField] string classTag = "Trade";
        [SerializeField] Sprite image;
        [SerializeField] string speedText = "--";
        [SerializeField] string healthText = "--";
        [SerializeField] string crewText = "--";
        [SerializeField] string storageText = "--";
        [SerializeField] string operationalCostText = "--";
        [SerializeField] string priceText = "--";
        [SerializeField] string ownedCountText = "--";

        public string Id => id;
        public string DisplayName => displayName;
        public string ClassTag => classTag;
        public Sprite Image => image;
        public string SpeedText => speedText;
        public string HealthText => healthText;
        public string CrewText => crewText;
        public string StorageText => storageText;
        public string OperationalCostText => operationalCostText;
        public string PriceText => priceText;
        public string OwnedCountText => ownedCountText;

        public ShipyardBuyOffer()
        {
        }

        public ShipyardBuyOffer(
            string id,
            string displayName,
            string classTag,
            Sprite image,
            string speedText,
            string healthText,
            string crewText,
            string storageText,
            string operationalCostText,
            string priceText,
            string ownedCountText)
        {
            this.id = id;
            this.displayName = displayName;
            this.classTag = classTag;
            this.image = image;
            this.speedText = speedText;
            this.healthText = healthText;
            this.crewText = crewText;
            this.storageText = storageText;
            this.operationalCostText = operationalCostText;
            this.priceText = priceText;
            this.ownedCountText = ownedCountText;
        }
    }
}

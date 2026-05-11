using UnityEngine;

namespace Assets.Scripts.CityScene
{
    public enum CityBuildingKind
    {
        Shipyard,
        GuildHouse,
        CountingHouse,
        Bank,
        Market,
        Tavern,
        Church,
        Custom
    }

    [CreateAssetMenu(fileName = "CityBuildingDefinition", menuName = "Age of Guilds/City/Building Definition")]
    public class CityBuildingDefinition : ScriptableObject
    {
        [SerializeField] string id = "building";
        [SerializeField] string displayName = "Building";
        [SerializeField] CityBuildingKind kind = CityBuildingKind.Custom;
        [SerializeField] Sprite icon;
        [SerializeField] string fallbackGlyph = "B";
        [SerializeField] Color accentColor = new Color(0.93f, 0.76f, 0.45f, 1f);
        [SerializeField] string panelTitle = "Building";
        [TextArea(2, 4)]
        [SerializeField] string panelSummary = "City operations will be wired here.";

        public string Id => id;
        public string DisplayName => displayName;
        public CityBuildingKind Kind => kind;
        public Sprite Icon => icon;
        public string FallbackGlyph => fallbackGlyph;
        public Color AccentColor => accentColor;
        public string PanelTitle => string.IsNullOrWhiteSpace(panelTitle) ? displayName : panelTitle;
        public string PanelSummary => panelSummary;

#if UNITY_EDITOR
        public void Configure(
            string newId,
            string newDisplayName,
            CityBuildingKind newKind,
            string newFallbackGlyph,
            Color newAccentColor,
            string newPanelTitle,
            string newPanelSummary)
        {
            id = newId;
            displayName = newDisplayName;
            kind = newKind;
            fallbackGlyph = newFallbackGlyph;
            accentColor = newAccentColor;
            panelTitle = newPanelTitle;
            panelSummary = newPanelSummary;
        }
#endif
    }
}

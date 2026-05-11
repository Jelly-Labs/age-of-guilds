using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CityScene
{
    [DisallowMultipleComponent]
    public class ShipyardConstructionRow : MonoBehaviour
    {
        static Font cachedFont;

        [SerializeField] Text shipNameText;
        [SerializeField] Text ownerText;
        [SerializeField] Text estimatedBuildTimeText;
        [SerializeField] Text statusText;
        [SerializeField] Image progressFillImage;
        [SerializeField] RectTransform materialStatusRoot;
        [SerializeField] ShipyardMaterialRow materialStatusTemplate;
        [SerializeField] Button cancelButton;

        readonly List<ShipyardMaterialRow> materialRows = new();

        public Text ShipNameText => shipNameText;
        public Text OwnerText => ownerText;
        public Text EstimatedBuildTimeText => estimatedBuildTimeText;
        public Text StatusText => statusText;
        public Image ProgressFillImage => progressFillImage;
        public Button CancelButton => cancelButton;
        public IReadOnlyList<ShipyardMaterialRow> MaterialRows => materialRows;

        public void SetReferences(
            Text shipName,
            Text owner,
            Text estimatedBuildTime,
            Text status,
            Image progressFill,
            RectTransform materialRoot,
            ShipyardMaterialRow materialTemplate,
            Button cancel)
        {
            shipNameText = shipName;
            ownerText = owner;
            estimatedBuildTimeText = estimatedBuildTime;
            statusText = status;
            progressFillImage = progressFill;
            materialStatusRoot = materialRoot;
            materialStatusTemplate = materialTemplate;
            cancelButton = cancel;
        }

        public void Bind(ShipyardConstructionEntry entry)
        {
            SetText(shipNameText, entry != null ? entry.ShipName : "--");
            SetText(ownerText, entry != null ? entry.OwnerName : "--");
            SetText(estimatedBuildTimeText, entry != null ? entry.EstimatedBuildTimeText : "--");
            SetText(statusText, entry != null ? entry.StatusMessage : string.Empty);

            if (statusText != null)
            {
                statusText.gameObject.SetActive(entry != null && !string.IsNullOrWhiteSpace(entry.StatusMessage));
            }

            if (progressFillImage != null)
            {
                RectTransform fillRect = (RectTransform)progressFillImage.transform;
                Vector2 anchorMax = fillRect.anchorMax;
                anchorMax.x = entry != null ? Mathf.Clamp01(entry.Progress) : 0f;
                fillRect.anchorMax = anchorMax;
                fillRect.offsetMax = Vector2.zero;
            }

            RebuildMaterialRows(entry);
        }

        void RebuildMaterialRows(ShipyardConstructionEntry entry)
        {
            if (materialStatusRoot != null)
            {
                for (int i = materialStatusRoot.childCount - 1; i >= 0; i--)
                {
                    Transform child = materialStatusRoot.GetChild(i);
                    if (materialStatusTemplate == null || child != materialStatusTemplate.transform)
                    {
                        DestroyObject(child.gameObject);
                    }
                }
            }

            materialRows.Clear();

            if (materialStatusRoot == null || materialStatusTemplate == null || entry == null)
            {
                return;
            }

            materialStatusTemplate.gameObject.SetActive(false);
            IReadOnlyList<ShipyardMaterialRequirement> materials = entry.MaterialStatus;
            for (int i = 0; i < materials.Count; i++)
            {
                ShipyardMaterialRow row = Instantiate(materialStatusTemplate, materialStatusRoot);
                row.name = $"MaterialStatus_{materials[i].Id}";
                row.gameObject.SetActive(true);
                row.Bind(materials[i], false);
                materialRows.Add(row);
            }
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

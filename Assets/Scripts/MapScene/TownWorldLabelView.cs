using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MapScene
{
    public sealed class TownWorldLabelView : MonoBehaviour
    {
        const float DropdownTopOffset = 14f;
        const float DropdownWidth = 252f;
        const float DropdownRowHeight = 54f;

        static TownWorldLabelView openDropdownView;

        [SerializeField] RectTransform root;
        [SerializeField] Text nameLabel;
        [SerializeField] Text dockedShipsLabel;

        RectTransform dockedShipsBadgeRoot;
        Button dockedShipsButton;
        RectTransform dropdownRoot;
        Font dropdownFont;
        TownObject townObject;
        string displayedTownName;
        int displayedDockedShipCount = int.MinValue;

        public void Bind(TownObject targetTownObject)
        {
            townObject = targetTownObject;
            EnsureInteractiveElements();
            ApplyText();
        }

        public void Refresh(Camera worldCamera, RectTransform canvasRect, Vector3 worldOffset, float screenBoundsMargin)
        {
            if (root == null)
            {
                root = (RectTransform)transform;
            }

            Town town = townObject != null ? townObject.Town : null;
            if (townObject == null || town == null || worldCamera == null || canvasRect == null)
            {
                CloseDropdown();
                SetVisible(false);
                return;
            }

            Vector3 screenPoint = worldCamera.WorldToScreenPoint(townObject.transform.position + worldOffset);
            bool isVisible = screenPoint.z > 0f
                && screenPoint.x >= -screenBoundsMargin
                && screenPoint.x <= Screen.width + screenBoundsMargin
                && screenPoint.y >= -screenBoundsMargin
                && screenPoint.y <= Screen.height + screenBoundsMargin;

            if (!isVisible)
            {
                CloseDropdown();
                SetVisible(false);
                return;
            }

            root.position = new Vector3(
                Mathf.Round(screenPoint.x),
                Mathf.Round(screenPoint.y),
                0f);
            root.localScale = Vector3.one / 2;

            SetVisible(true);
            ApplyText();
        }

        void OnDisable()
        {
            CloseDropdown();
        }

        void EnsureInteractiveElements()
        {
            if (root == null)
            {
                root = (RectTransform)transform;
            }

            if (dockedShipsLabel == null)
            {
                return;
            }

            dockedShipsBadgeRoot = dockedShipsLabel.transform.parent as RectTransform;
            if (dockedShipsBadgeRoot == null)
            {
                return;
            }

            Image badgeImage = dockedShipsBadgeRoot.GetComponent<Image>();
            if (badgeImage != null)
            {
                badgeImage.raycastTarget = true;
            }

            dockedShipsButton = dockedShipsBadgeRoot.GetComponent<Button>();
            if (dockedShipsButton == null)
            {
                dockedShipsButton = dockedShipsBadgeRoot.gameObject.AddComponent<Button>();
            }

            dockedShipsButton.transition = Selectable.Transition.None;
            dockedShipsButton.targetGraphic = badgeImage;
            dockedShipsButton.onClick.RemoveListener(HandleDockedShipsBadgeClicked);
            dockedShipsButton.onClick.AddListener(HandleDockedShipsBadgeClicked);

            dockedShipsButton.gameObject.SetActive(townObject.Town.DockedShips.Count > 0);

            EnsureDropdownRoot();
        }

        void EnsureDropdownRoot()
        {
            if (dropdownRoot != null || root == null)
            {
                return;
            }

            dropdownFont ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject dropdownObject = new GameObject(
                "DockedShipsDropdown",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(VerticalLayoutGroup),
                typeof(ContentSizeFitter));
            dropdownObject.transform.SetParent(root, false);

            dropdownRoot = (RectTransform)dropdownObject.transform;
            dropdownRoot.anchorMin = new Vector2(0.5f, 0f);
            dropdownRoot.anchorMax = new Vector2(0.5f, 0f);
            dropdownRoot.pivot = new Vector2(0.5f, 1f);
            dropdownRoot.anchoredPosition = new Vector2(0f, -DropdownTopOffset);
            dropdownRoot.sizeDelta = new Vector2(DropdownWidth, 0f);

            Image background = dropdownObject.GetComponent<Image>();
            background.color = new Color32(12, 29, 38, 244);
            background.raycastTarget = true;

            VerticalLayoutGroup layout = dropdownObject.GetComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 0f;
            layout.padding = new RectOffset(10, 10, 10, 10);

            ContentSizeFitter fitter = dropdownObject.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            dropdownObject.SetActive(false);
        }

        void HandleDockedShipsBadgeClicked()
        {
            if (dropdownRoot == null)
            {
                EnsureDropdownRoot();
            }

            if (dropdownRoot == null)
            {
                return;
            }

            if (openDropdownView != null && openDropdownView != this)
            {
                openDropdownView.CloseDropdown();
            }

            bool shouldOpen = !dropdownRoot.gameObject.activeSelf;
            if (!shouldOpen)
            {
                CloseDropdown();
                return;
            }

            RebuildDropdownOptions();
            dropdownRoot.gameObject.SetActive(true);
            openDropdownView = this;
        }

        void RebuildDropdownOptions()
        {
            if (dropdownRoot == null)
            {
                return;
            }

            for (int i = dropdownRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(dropdownRoot.GetChild(i).gameObject);
            }

            Town town = townObject != null ? townObject.Town : null;
            if (town == null || town.DockedShips == null || town.DockedShips.Count == 0)
            {
                CreateDropdownLabel("No docked ships");
                return;
            }

            for (int i = 0; i < town.DockedShips.Count; i++)
            {
                Ship ship = town.DockedShips[i];
                CreateDropdownOption(ship, FormatShipLabel(ship));
            }
        }

        void CreateDropdownOption(Ship ship, string label)
        {
            GameObject optionObject = new GameObject(
                $"DockedShip_{ship.ID}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            optionObject.transform.SetParent(dropdownRoot, false);

            RectTransform optionRect = (RectTransform)optionObject.transform;
            optionRect.sizeDelta = new Vector2(0f, DropdownRowHeight);

            Image optionImage = optionObject.GetComponent<Image>();
            optionImage.color = ship == ShipManager.instance.SelectedShip
                ? new Color32(30, 54, 66, 255)
                : new Color32(15, 36, 47, 230);
            optionImage.raycastTarget = true;

            Button button = optionObject.GetComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = optionImage;
            ColorBlock colors = button.colors;
            colors.normalColor = optionImage.color;
            colors.highlightedColor = new Color32(36, 64, 78, 255);
            colors.pressedColor = new Color32(45, 76, 90, 255);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color32(15, 36, 47, 160);
            button.colors = colors;
            button.onClick.AddListener(() => HandleShipSelected(ship));

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(optionObject.transform, false);

            RectTransform labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(18f, 8f);
            labelRect.offsetMax = new Vector2(-18f, -8f);

            Text optionLabel = labelObject.GetComponent<Text>();
            optionLabel.font = dropdownFont;
            optionLabel.fontSize = 12;
            optionLabel.fontStyle = FontStyle.Bold;
            optionLabel.alignment = TextAnchor.MiddleLeft;
            optionLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            optionLabel.verticalOverflow = VerticalWrapMode.Overflow;
            optionLabel.raycastTarget = false;
            optionLabel.color = ship == ShipManager.instance.SelectedShip
                ? new Color32(247, 223, 183, 255)
                : new Color32(239, 236, 226, 255);
            optionLabel.text = label;
        }

        void CreateDropdownLabel(string label)
        {
            GameObject labelObject = new GameObject("EmptyState", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(dropdownRoot, false);

            RectTransform labelRect = (RectTransform)labelObject.transform;
            labelRect.sizeDelta = new Vector2(0f, DropdownRowHeight - 8f);

            Text emptyLabel = labelObject.GetComponent<Text>();
            emptyLabel.font = dropdownFont;
            emptyLabel.fontSize = 11;
            emptyLabel.fontStyle = FontStyle.Bold;
            emptyLabel.alignment = TextAnchor.MiddleCenter;
            emptyLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            emptyLabel.verticalOverflow = VerticalWrapMode.Overflow;
            emptyLabel.raycastTarget = false;
            emptyLabel.color = new Color32(176, 186, 191, 255);
            emptyLabel.text = label;
        }

        string FormatShipLabel(Ship ship)
        {
            if (ship == null)
            {
                return "Unknown ship";
            }

            return ship.Name;
        }

        void HandleShipSelected(Ship ship)
        {
            ShipManager.instance.SelectedShip = ship;
            CloseDropdown();
        }

        void ApplyText()
        {
            Town town = townObject != null ? townObject.Town : null;
            string townName = town?.Name ?? string.Empty;
            int dockedShipCount = town?.DockedShips?.Count ?? 0;

            if (nameLabel != null && !string.Equals(displayedTownName, townName))
            {
                displayedTownName = townName;
                nameLabel.text = displayedTownName;
            }

            if (dockedShipsLabel != null && displayedDockedShipCount != dockedShipCount)
            {
                displayedDockedShipCount = dockedShipCount;
                dockedShipsLabel.text = dockedShipCount.ToString();

                if (dropdownRoot != null && dropdownRoot.gameObject.activeSelf)
                {
                    RebuildDropdownOptions();
                }

                dockedShipsButton.gameObject.SetActive(dockedShipCount > 0);
            }
        }

        void SetVisible(bool visible)
        {
            if (root != null && root.gameObject.activeSelf != visible)
            {
                root.gameObject.SetActive(visible);
            }
        }

        void CloseDropdown()
        {
            if (dropdownRoot != null)
            {
                dropdownRoot.gameObject.SetActive(false);
            }

            if (openDropdownView == this)
            {
                openDropdownView = null;
            }
        }
    }
}

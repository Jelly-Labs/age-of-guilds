using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.CityScene
{
    [DisallowMultipleComponent]
    public class CityUIView : MonoBehaviour
    {
        [Header("Footer")]
        [SerializeField] RectTransform footerRoot;

        [Header("Popup")]
        [SerializeField] RectTransform popupRoot;
        [SerializeField] RectTransform placeholderRoot;
        [SerializeField] MarketPopupView marketPopupView;
        [SerializeField] ShipyardPopupView shipyardPopupView;
        [SerializeField] CanvasGroup popupGroup;
        [SerializeField] Text popupTitleText;
        [SerializeField] Button closeButton;

        [Header("Motion")]
        [SerializeField] float popupSmoothTime = 0.12f;
        [SerializeField] Vector2 hiddenPopupOffset = new Vector2(0f, -24f);
        [SerializeField] Vector2 defaultPopupSize = new Vector2(520f, 280f);
        [SerializeField] Vector2 marketPopupSize = new Vector2(1320f, 600f);
        [SerializeField] Vector2 shipyardPopupSize = new Vector2(1040f, 520f);

        readonly List<CityFooterButton> footerButtons = new();
        readonly Dictionary<string, CityBuilding> buildingsById = new();
        CityInteractionController interactionController;
        Vector2 popupShownPosition;
        Vector2 popupVelocity;
        float popupAlphaVelocity;
        float targetPopupAlpha;
        bool popupVisible;
        string activeEntryId;

        void Awake()
        {
            if (popupRoot != null)
            {
                popupShownPosition = popupRoot.anchoredPosition;
                if (defaultPopupSize == Vector2.zero)
                {
                    defaultPopupSize = popupRoot.sizeDelta;
                }
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(ClosePopup);
            }

            RefreshFooterButtons();
            HideBuilding(true);
        }

        void Update()
        {
            UpdatePopupMotion();
        }

        public void Bind(CityInteractionController controller)
        {
            interactionController = controller;
        }

        public void SetCityName(string cityName)
        {
            // Kept for setup compatibility. City name UI is intentionally out of scope for this footer-only pass.
        }

        public void SetBuildings(IReadOnlyList<CityBuilding> buildings)
        {
            buildingsById.Clear();
            if (buildings != null)
            {
                for (int i = 0; i < buildings.Count; i++)
                {
                    CityBuilding building = buildings[i];
                    if (building == null || building.Definition == null)
                    {
                        continue;
                    }

                    buildingsById[building.Definition.Id] = building;
                }
            }

            RefreshFooterButtons();
            BindFooterButtons();
        }

        public void ShowBuilding(CityBuilding building)
        {
            if (building == null)
            {
                HideBuilding();
                return;
            }

            string entryId = building.Definition != null ? building.Definition.Id : string.Empty;
            ShowPopup(building.DisplayName, entryId);
        }

        public void ShowPlaceholder(string title, string entryId)
        {
            interactionController?.ClearSelectionWithoutCameraReturn();
            ShowPopup(title, entryId);
        }

        public void ShowFooterEntry(string title, string entryId)
        {
            ShowPopup(title, entryId);
        }

        public void HideBuilding(bool immediate = false)
        {
            popupVisible = false;
            targetPopupAlpha = 0f;
            activeEntryId = string.Empty;
            SetFooterSelection(activeEntryId);
            SetPopupContentMode(false);

            if (immediate && popupGroup != null)
            {
                popupGroup.alpha = 0f;
                popupGroup.interactable = false;
                popupGroup.blocksRaycasts = false;
            }

            if (immediate && popupRoot != null)
            {
                popupRoot.anchoredPosition = popupShownPosition + hiddenPopupOffset;
            }
        }

        void RefreshFooterButtons()
        {
            footerButtons.Clear();

            if (footerRoot == null)
            {
                return;
            }

            footerRoot.GetComponentsInChildren(true, footerButtons);
        }

        void BindFooterButtons()
        {
            for (int i = 0; i < footerButtons.Count; i++)
            {
                CityFooterButton footerButton = footerButtons[i];
                if (footerButton == null)
                {
                    continue;
                }

                CityBuilding targetBuilding = null;
                if (!string.IsNullOrWhiteSpace(footerButton.TargetBuildingId))
                {
                    buildingsById.TryGetValue(footerButton.TargetBuildingId, out targetBuilding);
                }

                footerButton.Bind(this, targetBuilding);
                footerButton.SetSelected(footerButton.EntryId == activeEntryId);
            }
        }

        public void HandleFooterButtonClicked(CityFooterButton footerButton)
        {
            if (footerButton == null || !footerButton.IsAvailable)
            {
                return;
            }

            if (footerButton.EntryId == "back")
            {
                SceneManager.LoadScene("MapScene");
                return;
            }

            CityBuilding targetBuilding = footerButton.TargetBuilding;
            if (targetBuilding != null)
            {
                interactionController?.SelectBuilding(targetBuilding, footerButton.DisplayName, footerButton.EntryId);
                return;
            }

            ShowPlaceholder(footerButton.DisplayName, footerButton.EntryId);
        }

        public bool TryGetAvailableFooterEntryForBuilding(CityBuilding building, out string displayName, out string entryId)
        {
            displayName = string.Empty;
            entryId = string.Empty;

            if (building == null || building.Definition == null)
            {
                return false;
            }

            string buildingId = building.Definition.Id;
            for (int i = 0; i < footerButtons.Count; i++)
            {
                CityFooterButton footerButton = footerButtons[i];
                if (footerButton == null || !footerButton.IsAvailable)
                {
                    continue;
                }

                if (footerButton.TargetBuildingId != buildingId)
                {
                    continue;
                }

                displayName = footerButton.DisplayName;
                entryId = footerButton.EntryId;
                return true;
            }

            return false;
        }

        void ShowPopup(string title, string entryId)
        {
            bool showMarket = entryId == "market" && marketPopupView != null;
            bool showShipyard = entryId == "shipyard" && shipyardPopupView != null;
            SetPopupContentMode(showMarket, showShipyard);

            if (popupTitleText != null)
            {
                popupTitleText.text = title;
            }

            popupVisible = true;
            targetPopupAlpha = 1f;
            activeEntryId = entryId;
            SetFooterSelection(activeEntryId);
        }

        void SetPopupContentMode(bool showMarket)
        {
            SetPopupContentMode(showMarket, false);
        }

        void SetPopupContentMode(bool showMarket, bool showShipyard)
        {
            if (popupRoot != null)
            {
                if (showMarket)
                {
                    popupRoot.sizeDelta = marketPopupSize;
                }
                else if (showShipyard)
                {
                    popupRoot.sizeDelta = shipyardPopupSize;
                }
                else
                {
                    popupRoot.sizeDelta = defaultPopupSize;
                }
            }

            if (placeholderRoot != null)
            {
                placeholderRoot.gameObject.SetActive(!showMarket && !showShipyard);
            }

            if (popupTitleText != null)
            {
                popupTitleText.gameObject.SetActive(!showMarket && !showShipyard);
            }

            if (showMarket)
            {
                marketPopupView?.Show();
            }
            else
            {
                marketPopupView?.Hide();
            }

            if (showShipyard)
            {
                shipyardPopupView?.Show();
            }
            else
            {
                shipyardPopupView?.Hide();
            }
        }

        void ClosePopup()
        {
            interactionController?.ClearSelection();
            HideBuilding();
        }

        void SetFooterSelection(string entryId)
        {
            for (int i = 0; i < footerButtons.Count; i++)
            {
                CityFooterButton footerButton = footerButtons[i];
                if (footerButton != null)
                {
                    footerButton.SetSelected(!string.IsNullOrEmpty(entryId) && footerButton.EntryId == entryId);
                }
            }
        }

        void UpdatePopupMotion()
        {
            if (popupRoot != null)
            {
                Vector2 target = popupVisible ? popupShownPosition : popupShownPosition + hiddenPopupOffset;
                popupRoot.anchoredPosition = Vector2.SmoothDamp(popupRoot.anchoredPosition, target, ref popupVelocity, popupSmoothTime);
            }

            if (popupGroup != null)
            {
                popupGroup.alpha = Mathf.SmoothDamp(popupGroup.alpha, targetPopupAlpha, ref popupAlphaVelocity, popupSmoothTime);
                bool interactive = popupGroup.alpha > 0.4f;
                popupGroup.interactable = interactive;
                popupGroup.blocksRaycasts = interactive;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Scripts.CityScene
{
    [DisallowMultipleComponent]
    public class CityInteractionController : MonoBehaviour
    {
        [SerializeField] Camera sceneCamera;
        [SerializeField] CityCameraController cameraController;
        [SerializeField] CityUIView cityView;
        [SerializeField] LayerMask raycastMask = ~0;
        [SerializeField] float maxRaycastDistance = 1000f;

        readonly List<CityBuilding> buildings = new();
        CityBuilding hoveredBuilding;
        CityBuilding selectedBuilding;

        public IReadOnlyList<CityBuilding> Buildings => buildings;
        public CityBuilding SelectedBuilding => selectedBuilding;
        public event Action<CityBuilding> SelectionChanged;
        public event Action<CityBuilding> HoverChanged;

        void Awake()
        {
            if (sceneCamera == null)
            {
                sceneCamera = Camera.main;
            }

            if (cameraController == null && sceneCamera != null)
            {
                cameraController = sceneCamera.GetComponent<CityCameraController>();
            }

            RefreshBuildings();

            if (cityView != null)
            {
                cityView.Bind(this);
                cityView.SetBuildings(buildings);
            }
        }

        void Update()
        {
            UpdateHover();
            UpdateSelectionInput();
        }

        public void Configure(Camera camera, CityCameraController cityCamera, CityUIView view)
        {
            sceneCamera = camera;
            cameraController = cityCamera;
            cityView = view;

            RefreshBuildings();
            if (cityView != null)
            {
                cityView.Bind(this);
                cityView.SetBuildings(buildings);
            }
        }

        public void RefreshBuildings()
        {
            buildings.Clear();
            buildings.AddRange(FindObjectsByType<CityBuilding>(FindObjectsInactive.Exclude));
            buildings.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.Ordinal));
        }

        public void SelectBuilding(CityBuilding building)
        {
            SelectBuilding(building, string.Empty, string.Empty);
        }

        public void SelectBuilding(CityBuilding building, string popupTitle, string footerEntryId)
        {
            if (selectedBuilding == building && building != null)
            {
                cameraController?.FocusBuilding(selectedBuilding);
                ShowSelectedBuildingUi(selectedBuilding, popupTitle, footerEntryId);
                return;
            }

            if (selectedBuilding != null)
            {
                selectedBuilding.SetSelected(false);
            }

            selectedBuilding = building;

            if (selectedBuilding != null)
            {
                selectedBuilding.SetSelected(true);
                cameraController?.FocusBuilding(selectedBuilding);
                ShowSelectedBuildingUi(selectedBuilding, popupTitle, footerEntryId);
            }
            else
            {
                cameraController?.ReturnToCity();
                cityView?.HideBuilding();
            }

            SelectionChanged?.Invoke(selectedBuilding);
        }

        public void ClearSelection()
        {
            SelectBuilding(null);
        }

        public void ClearSelectionWithoutCameraReturn()
        {
            if (selectedBuilding == null)
            {
                return;
            }

            selectedBuilding.SetSelected(false);
            selectedBuilding = null;
            SelectionChanged?.Invoke(null);
        }

        void UpdateHover()
        {
            CityBuilding nextHover = IsPointerOverUi() ? null : RaycastBuilding();
            if (nextHover == hoveredBuilding)
            {
                return;
            }

            if (hoveredBuilding != null)
            {
                hoveredBuilding.SetHovered(false);
            }

            hoveredBuilding = nextHover;

            if (hoveredBuilding != null)
            {
                hoveredBuilding.SetHovered(true);
            }

            HoverChanged?.Invoke(hoveredBuilding);
        }

        void UpdateSelectionInput()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasReleasedThisFrame || IsPointerOverUi())
            {
                return;
            }

            if (hoveredBuilding == null)
            {
                return;
            }

            if (cityView != null && cityView.TryGetAvailableFooterEntryForBuilding(hoveredBuilding, out string popupTitle, out string footerEntryId))
            {
                SelectBuilding(hoveredBuilding, popupTitle, footerEntryId);
            }
        }

        CityBuilding RaycastBuilding()
        {
            if (sceneCamera == null || Mouse.current == null)
            {
                return null;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Ray ray = sceneCamera.ScreenPointToRay(screenPosition);

            if (!Physics.Raycast(ray, out RaycastHit hit, maxRaycastDistance, raycastMask, QueryTriggerInteraction.Ignore))
            {
                return null;
            }

            return hit.collider.GetComponentInParent<CityBuilding>();
        }

        void ShowSelectedBuildingUi(CityBuilding building, string popupTitle, string footerEntryId)
        {
            if (!string.IsNullOrWhiteSpace(footerEntryId))
            {
                string title = string.IsNullOrWhiteSpace(popupTitle) ? building.DisplayName : popupTitle;
                cityView?.ShowFooterEntry(title, footerEntryId);
                return;
            }

            cityView?.ShowBuilding(building);
        }

        static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}

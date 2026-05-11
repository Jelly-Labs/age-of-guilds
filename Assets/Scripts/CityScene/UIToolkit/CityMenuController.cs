using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#pragma warning disable 0649

namespace Assets.Scripts.CityScene.UIToolkit
{
    [DisallowMultipleComponent]
    public sealed class CityMenuController : MonoBehaviour
    {
        const string RootName = "CityMenuRoot";
        const string PanelLayerName = "CityMenuPanelLayer";
        const string FooterBarName = "CityMenuFooterBar";

        [SerializeField] UIDocument document;
        [SerializeField] VisualTreeAsset buttonTemplate;
        [SerializeField] List<CityMenuItemConfig> menuItems = new List<CityMenuItemConfig>();

        CityFooterBarView footerBarView;
        CityMenuPanelLayerView panelLayerView;
        VisualElement rootElement;
        VisualElement panelLayer;
        VisualElement footerBarElement;

        void OnEnable()
        {
            Build();
        }

        void OnDisable()
        {
            Teardown();
        }

        public void HandleFooterMenuItemClicked(string menuItemId)
        {
            CityMenuItemConfig item = FindItem(menuItemId);
            if (item == null || !item.IsEnabled)
            {
                return;
            }

            ActivatePanel(menuItemId);
        }

        public void CloseActivePanel()
        {
            HideActivePanel();
        }

        void Build()
        {
            Teardown();

            if (document == null)
            {
                document = GetComponent<UIDocument>();
            }

            if (document == null || document.rootVisualElement == null)
            {
                return;
            }

            VisualElement documentRoot = document.rootVisualElement;
            documentRoot.pickingMode = PickingMode.Ignore;

            rootElement = documentRoot.Q<VisualElement>(RootName);
            panelLayer = documentRoot.Q<VisualElement>(PanelLayerName);
            footerBarElement = documentRoot.Q<VisualElement>(FooterBarName);

            if (rootElement != null)
            {
                rootElement.pickingMode = PickingMode.Ignore;
            }

            if (panelLayer != null)
            {
                panelLayer.pickingMode = PickingMode.Ignore;
                panelLayerView = new CityMenuPanelLayerView(panelLayer);
                panelLayerView.CloseRequested += CloseActivePanel;
                panelLayerView.SetItems(menuItems);
            }

            if (footerBarElement != null)
            {
                footerBarElement.pickingMode = PickingMode.Position;
                footerBarView = new CityFooterBarView(footerBarElement, buttonTemplate);
                footerBarView.MenuItemClicked += HandleFooterMenuItemClicked;
                footerBarView.SetItems(menuItems);
            }

            HideActivePanel();
        }

        void Teardown()
        {
            if (footerBarView != null)
            {
                footerBarView.MenuItemClicked -= HandleFooterMenuItemClicked;
                footerBarView.Dispose();
                footerBarView = null;
            }

            if (panelLayerView != null)
            {
                panelLayerView.CloseRequested -= CloseActivePanel;
                panelLayerView.Dispose();
                panelLayerView = null;
            }

            rootElement = null;
            panelLayer = null;
            footerBarElement = null;
        }

        void ActivatePanel(string menuItemId)
        {
            if (panelLayerView == null || !panelLayerView.ActivatePanel(menuItemId))
            {
                footerBarView?.SetSelected(string.Empty);
                return;
            }

            footerBarView?.SetSelected(menuItemId);
        }

        void HideActivePanel()
        {
            panelLayerView?.HideActivePanel();
            footerBarView?.SetSelected(string.Empty);
        }

        CityMenuItemConfig FindItem(string menuItemId)
        {
            if (string.IsNullOrWhiteSpace(menuItemId))
            {
                return null;
            }

            for (int i = 0; i < menuItems.Count; i++)
            {
                CityMenuItemConfig item = menuItems[i];
                if (item != null && item.MenuItemId == menuItemId)
                {
                    return item;
                }
            }

            return null;
        }
    }

    [Serializable]
    public sealed class CityMenuItemConfig
    {
        [SerializeField] string menuItemId;
        [SerializeField] string label;
        [SerializeField] Sprite enabledImage;
        [SerializeField] Sprite hoveredImage;
        [SerializeField] Sprite selectedImage;
        [SerializeField] Sprite disabledImage;
        [SerializeField] bool isEnabled = true;

        public string MenuItemId => menuItemId;
        public string Label => label;
        public Sprite EnabledImage => enabledImage;
        public Sprite HoveredImage => hoveredImage;
        public Sprite SelectedImage => selectedImage;
        public Sprite DisabledImage => disabledImage;
        public bool IsEnabled => isEnabled;
    }
}

#pragma warning restore 0649

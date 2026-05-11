using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Assets.Scripts.CityScene.UIToolkit
{
    public sealed class CityMenuPanelLayerView : IDisposable
    {
        const string MarketMenuItemId = "market";

        readonly VisualElement root;
        readonly Dictionary<string, CityMenuPanelView> panelsById = new Dictionary<string, CityMenuPanelView>();

        CityMenuPanelView activePanel;

        public event Action CloseRequested;

        public CityMenuPanelLayerView(VisualElement root)
        {
            this.root = root;
        }

        public string ActiveMenuItemId { get; private set; }

        public void SetItems(IReadOnlyList<CityMenuItemConfig> items)
        {
            Clear();

            if (root == null || items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                CityMenuItemConfig item = items[i];
                if (item == null || string.IsNullOrWhiteSpace(item.MenuItemId) || panelsById.ContainsKey(item.MenuItemId))
                {
                    continue;
                }

                CityMenuPanelView panel = CreatePanel(item);
                panel.CloseRequested += HandlePanelCloseRequested;

                root.Add(panel.Root);
                panelsById[item.MenuItemId] = panel;
            }

            HideActivePanel();
        }

        public bool ActivatePanel(string menuItemId)
        {
            HideAllPanels();

            if (!panelsById.TryGetValue(menuItemId, out activePanel))
            {
                activePanel = null;
                ActiveMenuItemId = string.Empty;
                return false;
            }

            activePanel.Show();
            ActiveMenuItemId = menuItemId;
            return true;
        }

        public void HideActivePanel()
        {
            HideAllPanels();
            activePanel = null;
            ActiveMenuItemId = string.Empty;
        }

        public void Dispose()
        {
            CloseRequested = null;
            Clear();
        }

        void Clear()
        {
            foreach (CityMenuPanelView panel in panelsById.Values)
            {
                panel.CloseRequested -= HandlePanelCloseRequested;
                panel.Dispose();
            }

            panelsById.Clear();
            root?.Clear();
            activePanel = null;
            ActiveMenuItemId = string.Empty;
        }

        void HideAllPanels()
        {
            foreach (CityMenuPanelView panel in panelsById.Values)
            {
                panel.Hide();
            }
        }

        void HandlePanelCloseRequested()
        {
            CloseRequested?.Invoke();
        }

        static CityMenuPanelView CreatePanel(CityMenuItemConfig item)
        {
            if (item.MenuItemId == MarketMenuItemId)
            {
                return new MarketPanelView(item.MenuItemId, item.Label);
            }

            return new CityMenuPanelView(item.MenuItemId, item.Label);
        }
    }
}

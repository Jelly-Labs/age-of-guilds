using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Assets.Scripts.CityScene.UIToolkit
{
    public sealed class CityFooterBarView : IDisposable
    {
        readonly VisualElement root;
        readonly VisualTreeAsset buttonTemplate;
        readonly List<CityMenuButtonView> buttons = new List<CityMenuButtonView>();

        public event Action<string> MenuItemClicked;

        public CityFooterBarView(VisualElement root, VisualTreeAsset buttonTemplate)
        {
            this.root = root;
            this.buttonTemplate = buttonTemplate;
        }

        public void SetItems(IReadOnlyList<CityMenuItemConfig> items)
        {
            Clear();

            if (root == null || buttonTemplate == null || items == null)
            {
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                CityMenuItemConfig item = items[i];
                if (item == null || string.IsNullOrWhiteSpace(item.MenuItemId))
                {
                    continue;
                }

                TemplateContainer templateRoot = buttonTemplate.Instantiate();
                templateRoot.AddToClassList("city-menu-button-template");
                templateRoot.pickingMode = PickingMode.Ignore;
                root.Add(templateRoot);

                CityMenuButtonView buttonView = new(templateRoot);
                buttonView.Configure(item);
                buttonView.Clicked += HandleButtonClicked;
                buttons.Add(buttonView);
            }
        }

        public void SetSelected(string menuItemId)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                CityMenuButtonView button = buttons[i];
                button.SetSelected(!string.IsNullOrWhiteSpace(menuItemId) && button.MenuItemId == menuItemId);
            }
        }

        public void Dispose()
        {
            Clear();
            MenuItemClicked = null;
        }

        void Clear()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Clicked -= HandleButtonClicked;
                buttons[i].Dispose();
            }

            buttons.Clear();
            root?.Clear();
        }

        void HandleButtonClicked(string menuItemId)
        {
            MenuItemClicked?.Invoke(menuItemId);
        }
    }
}

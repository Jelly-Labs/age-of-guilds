using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.CityScene.UIToolkit
{
    public sealed class CityMenuButtonView : IDisposable
    {
        static readonly Color EnabledIconTint = Color.white;

        readonly VisualElement root;
        readonly Button button;
        readonly Image imageElement;
        readonly Label labelElement;

        bool isSelected;
        bool isHovered;

        public event Action<string> Clicked;

        public string MenuItemId { get; private set; }
        public string Label { get; private set; }
        public Sprite EnabledImage { get; private set; }
        public Sprite HoveredImage { get; private set; }
        public Sprite SelectedImage { get; private set; }
        public Sprite DisabledImage { get; private set; }
        public bool IsEnabled { get; private set; }

        public CityMenuButtonView(VisualElement root)
        {
            this.root = root;
            button = root?.Q<Button>("CityMenuButton");
            imageElement = root?.Q<Image>("CityMenuButtonImage");
            labelElement = root?.Q<Label>("CityMenuButtonLabel");

            if (button != null)
            {
                button.clicked += HandleClicked;
                button.RegisterCallback<PointerEnterEvent>(HandlePointerEnter);
                button.RegisterCallback<PointerLeaveEvent>(HandlePointerLeave);
                button.text = string.Empty;
            }
        }

        public void Configure(CityMenuItemConfig config)
        {
            MenuItemId = config.MenuItemId;
            Label = config.Label;
            EnabledImage = config.EnabledImage;
            HoveredImage = config.HoveredImage;
            SelectedImage = config.SelectedImage;
            DisabledImage = config.DisabledImage;
            IsEnabled = config.IsEnabled;
            ApplyContent();
            ApplyVisualState();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected && IsEnabled;
            ApplyVisualState();
        }

        public void Dispose()
        {
            if (button != null)
            {
                button.clicked -= HandleClicked;
                button.UnregisterCallback<PointerEnterEvent>(HandlePointerEnter);
                button.UnregisterCallback<PointerLeaveEvent>(HandlePointerLeave);
            }

            Clicked = null;
        }

        void ApplyContent()
        {
            if (labelElement != null)
            {
                labelElement.text = Label;
            }

            if (imageElement != null)
            {
                imageElement.scaleMode = ScaleMode.ScaleToFit;
            }
        }

        void ApplyVisualState()
        {
            if (button != null)
            {
                button.SetEnabled(IsEnabled);
                button.EnableInClassList("city-menu-button--enabled", IsEnabled);
                button.EnableInClassList("city-menu-button--disabled", !IsEnabled);
                button.EnableInClassList("city-menu-button--selected", isSelected);
                button.pickingMode = IsEnabled ? PickingMode.Position : PickingMode.Ignore;
            }

            if (imageElement != null)
            {
                imageElement.sprite = ResolveStateImage();
                imageElement.tintColor = EnabledIconTint;
            }

            root?.EnableInClassList("city-menu-button-template--disabled", !IsEnabled);
        }

        Sprite ResolveStateImage()
        {
            if (isSelected)
            {
                return SelectedImage;
            }
            else if (isHovered)
            {
                return HoveredImage;
            }
            else if (IsEnabled)
            {
                return EnabledImage;
            }
            else
            {
                return DisabledImage;
            }
        }

        void HandleClicked()
        {
            if (!IsEnabled)
            {
                return;
            }

            Clicked?.Invoke(MenuItemId);
        }

        void HandlePointerEnter(PointerEnterEvent evt)
        {
            isHovered = IsEnabled && !isSelected;
            ApplyVisualState();
        }

        void HandlePointerLeave(PointerLeaveEvent evt)
        {
            isHovered = false;
            ApplyVisualState();
        }
    }
}

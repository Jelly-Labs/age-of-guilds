using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Assets.Scripts.CityScene.UIToolkit
{
    public sealed class DropdownSelectorView<T> : VisualElement, IDisposable
    {
        const string RootClass = "dropdown-selector";
        const string RootOpenClass = "dropdown-selector--open";
        const string ToggleClass = "dropdown-selector__toggle";
        const string IconClass = "dropdown-selector__icon";
        const string SelectedLabelClass = "dropdown-selector__selected-label";
        const string ArrowClass = "dropdown-selector__arrow";
        const string OptionsClass = "dropdown-selector__options";
        const string OptionClass = "dropdown-selector__option";
        const string OptionSelectedClass = "dropdown-selector__option--selected";

        readonly List<T> items = new List<T>();
        readonly List<Button> optionButtons = new List<Button>();
        readonly Button toggleButton;
        readonly VisualElement iconElement;
        readonly Label selectedLabel;
        readonly Label arrowLabel;
        readonly VisualElement optionsRoot;

        Func<T, string> itemLabelSelector;
        Action<T> selectedItemChanged;
        int selectedIndex = -1;
        bool isOpen;

        public DropdownSelectorView()
        {
            AddToClassList(RootClass);

            toggleButton = new Button(Toggle)
            {
                name = "DropdownSelectorToggle",
                text = string.Empty,
                pickingMode = PickingMode.Position
            };
            toggleButton.AddToClassList(ToggleClass);

            iconElement = new VisualElement
            {
                name = "DropdownSelectorIcon",
                pickingMode = PickingMode.Ignore
            };
            iconElement.AddToClassList(IconClass);
            toggleButton.Add(iconElement);

            selectedLabel = new Label
            {
                name = "DropdownSelectorSelectedLabel",
                pickingMode = PickingMode.Ignore
            };
            selectedLabel.AddToClassList(SelectedLabelClass);
            toggleButton.Add(selectedLabel);

            arrowLabel = new Label
            {
                name = "DropdownSelectorArrow",
                text = "v",
                pickingMode = PickingMode.Ignore
            };
            arrowLabel.AddToClassList(ArrowClass);
            toggleButton.Add(arrowLabel);

            optionsRoot = new VisualElement
            {
                name = "DropdownSelectorOptions",
                pickingMode = PickingMode.Position
            };
            optionsRoot.AddToClassList(OptionsClass);

            BringToFront();

            Add(toggleButton);
            Add(optionsRoot);
            Close();
            ApplySelectedItem();
        }

        public int SelectedIndex => selectedIndex;
        public T SelectedItem => HasSelection ? items[selectedIndex] : default;
        public bool HasSelection => selectedIndex >= 0 && selectedIndex < items.Count;
        public bool IsOpen => isOpen;

        public void SetItems(IReadOnlyList<T> newItems, Action<T> onSelectedItemChanged)
        {
            SetItems(newItems, 0, onSelectedItemChanged, null);
        }

        public void SetItems(
            IReadOnlyList<T> newItems,
            Action<T> onSelectedItemChanged,
            Func<T, string> newItemLabelSelector)
        {
            SetItems(newItems, 0, onSelectedItemChanged, newItemLabelSelector);
        }

        public void SetItems(
            IReadOnlyList<T> newItems,
            int newSelectedIndex,
            Action<T> onSelectedItemChanged,
            Func<T, string> newItemLabelSelector = null)
        {
            items.Clear();
            if (newItems != null)
            {
                for (int i = 0; i < newItems.Count; i++)
                {
                    items.Add(newItems[i]);
                }
            }

            selectedItemChanged = onSelectedItemChanged;
            itemLabelSelector = newItemLabelSelector;
            selectedIndex = ClampIndex(newSelectedIndex);

            RebuildOptions();
            ApplySelectedItem();
            Close();
        }

        void Open()
        {
            isOpen = true;
            optionsRoot.style.display = DisplayStyle.Flex;
            EnableInClassList(RootOpenClass, true);
        }

        void Close()
        {
            isOpen = false;
            optionsRoot.style.display = DisplayStyle.None;
            EnableInClassList(RootOpenClass, false);
        }

        public void Dispose()
        {
            toggleButton.clicked -= Toggle;
            selectedItemChanged = null;
            itemLabelSelector = null;
            optionButtons.Clear();
            optionsRoot.Clear();
        }

        void Toggle()
        {
            if (isOpen)
            {
                Close();
                return;
            }

            Open();
        }

        void SelectOption(int optionIndex)
        {
            int nextIndex = ClampIndex(optionIndex);
            bool changed = selectedIndex != nextIndex;

            selectedIndex = nextIndex;
            ApplySelectedItem();
            ApplyOptionSelection();
            Close();

            if (changed && HasSelection)
            {
                selectedItemChanged?.Invoke(SelectedItem);
            }
        }

        void RebuildOptions()
        {
            optionButtons.Clear();
            optionsRoot.Clear();

            for (int i = 0; i < items.Count; i++)
            {
                int optionIndex = i;
                Button optionButton = new Button(() => SelectOption(optionIndex))
                {
                    name = "DropdownSelectorOption",
                    text = FormatItem(items[i]),
                    pickingMode = PickingMode.Position
                };
                optionButton.AddToClassList(OptionClass);

                optionsRoot.Add(optionButton);
                optionButtons.Add(optionButton);
            }

            ApplyOptionSelection();
        }

        void ApplySelectedItem()
        {
            selectedLabel.text = HasSelection ? FormatItem(SelectedItem) : string.Empty;
        }

        void ApplyOptionSelection()
        {
            for (int i = 0; i < optionButtons.Count; i++)
            {
                optionButtons[i].EnableInClassList(OptionSelectedClass, i == selectedIndex);
            }
        }

        int ClampIndex(int index)
        {
            if (items.Count == 0)
            {
                return -1;
            }

            if (index < 0)
            {
                return 0;
            }

            if (index >= items.Count)
            {
                return items.Count - 1;
            }

            return index;
        }

        string FormatItem(T item)
        {
            if (itemLabelSelector != null)
            {
                return itemLabelSelector(item);
            }

            return item != null ? item.ToString() : string.Empty;
        }
    }
}

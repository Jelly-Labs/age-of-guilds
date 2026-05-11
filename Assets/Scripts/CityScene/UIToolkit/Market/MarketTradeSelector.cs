using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CityScene
{
    [DisallowMultipleComponent]
    public class MarketTradeSelector : MonoBehaviour
    {
        static Font cachedFont;

        [SerializeField] Button toggleButton;
        [SerializeField] Image iconImage;
        [SerializeField] Text selectedLabelText;
        [SerializeField] RectTransform optionsRoot;
        [SerializeField] Button optionButtonTemplate;

        readonly List<Button> optionButtons = new();
        IReadOnlyList<MarketTradePlace> places;
        Action<MarketTradeSelector, int> selectionChanged;
        int selectedIndex;
        bool isOpen;

        public int SelectedIndex => selectedIndex;

        void Awake()
        {
            EnsureReferences();
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(ToggleOptions);
                toggleButton.onClick.AddListener(ToggleOptions);
            }

            SetOptionsVisible(false);
        }

        public void SetReferences(Button button, Image icon, Text label, RectTransform options, Button optionTemplate)
        {
            toggleButton = button;
            iconImage = icon;
            selectedLabelText = label;
            optionsRoot = options;
            optionButtonTemplate = optionTemplate;
            EnsureReferences();
            SetOptionsVisible(false);
        }

        public void Bind(IReadOnlyList<MarketTradePlace> newPlaces, int newSelectedIndex, Action<MarketTradeSelector, int> onSelectionChanged)
        {
            places = newPlaces;
            selectionChanged = onSelectionChanged;
            selectedIndex = Mathf.Max(0, newSelectedIndex);
            RebuildOptions();
            ApplySelectedPlace();
        }

        public void SetSelectedIndex(int index)
        {
            selectedIndex = Mathf.Max(0, index);
            ApplySelectedPlace();
        }

        public void Close()
        {
            isOpen = false;
            SetOptionsVisible(false);
        }

        void ToggleOptions()
        {
            isOpen = !isOpen;
            if (isOpen)
            {
                transform.SetAsLastSibling();
            }

            SetOptionsVisible(isOpen);
        }

        void SelectOption(int index)
        {
            selectedIndex = index;
            ApplySelectedPlace();
            Close();
            selectionChanged?.Invoke(this, selectedIndex);
        }

        void RebuildOptions()
        {
            if (optionsRoot != null)
            {
                for (int i = optionsRoot.childCount - 1; i >= 0; i--)
                {
                    Transform child = optionsRoot.GetChild(i);
                    if (optionButtonTemplate == null || child != optionButtonTemplate.transform)
                    {
                        DestroyObject(child.gameObject);
                    }
                }
            }

            optionButtons.Clear();

            if (optionButtonTemplate == null || optionsRoot == null || places == null)
            {
                return;
            }

            optionButtonTemplate.gameObject.SetActive(false);

            for (int i = 0; i < places.Count; i++)
            {
                int optionIndex = i;
                Button optionButton = Instantiate(optionButtonTemplate, optionsRoot);
                optionButton.name = $"Option_{places[i].Id}";
                optionButton.gameObject.SetActive(true);
                optionButton.onClick.RemoveAllListeners();
                optionButton.onClick.AddListener(() => SelectOption(optionIndex));

                Text label = optionButton.GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.text = places[i].DisplayLabel;
                    label.font = GetDefaultFont();
                }

                optionButtons.Add(optionButton);
            }
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

        void ApplySelectedPlace()
        {
            MarketTradePlace place = places != null && places.Count > 0
                ? places[Mathf.Clamp(selectedIndex, 0, places.Count - 1)]
                : null;

            if (selectedLabelText != null)
            {
                selectedLabelText.text = place != null ? place.DisplayLabel : "Select";
                selectedLabelText.font = GetDefaultFont();
            }

            if (iconImage != null)
            {
                Sprite sprite = place != null ? place.Icon : null;
                iconImage.sprite = sprite;
                iconImage.enabled = sprite != null;
            }
        }

        void SetOptionsVisible(bool visible)
        {
            if (optionsRoot != null)
            {
                optionsRoot.gameObject.SetActive(visible);
            }
        }

        void EnsureReferences()
        {
            if (toggleButton == null)
            {
                toggleButton = GetComponent<Button>();
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

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.CityScene
{
    [DisallowMultipleComponent]
    public class CityWorldIcon : MonoBehaviour
    {
        [SerializeField] Canvas canvas;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image backplate;
        [SerializeField] Image iconImage;
        [SerializeField] Text fallbackText;
        [SerializeField] float baseWorldScale = 0.045f;
        [SerializeField] float defaultScale = 0.82f;
        [SerializeField] float hoverScale = 1.06f;
        [SerializeField] float selectedScale = 1.18f;
        [SerializeField] float defaultAlpha = 0.62f;
        [SerializeField] float hoverAlpha = 0.94f;
        [SerializeField] float selectedAlpha = 1f;
        [SerializeField] float smoothTime = 0.1f;

        Camera targetCamera;
        RectTransform rectTransform;
        Vector3 scaleVelocity;
        float alphaVelocity;
        float targetScale;
        float targetAlpha;

        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                {
                    rectTransform = transform as RectTransform;
                }

                return rectTransform;
            }
        }

        void Awake()
        {
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            rectTransform = transform as RectTransform;
            targetScale = defaultScale;
            targetAlpha = defaultAlpha;
        }

        void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                transform.rotation = Quaternion.LookRotation(targetCamera.transform.forward, targetCamera.transform.up);
            }

            Vector3 target = Vector3.one * baseWorldScale * targetScale;
            transform.localScale = Vector3.SmoothDamp(transform.localScale, target, ref scaleVelocity, smoothTime);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha, targetAlpha, ref alphaVelocity, smoothTime);
            }
        }

        public void Bind(CityBuilding building, Camera cameraOverride)
        {
            targetCamera = cameraOverride != null ? cameraOverride : Camera.main;

            if (building == null || building.Definition == null)
            {
                return;
            }

            CityBuildingDefinition definition = building.Definition;
            if (iconImage != null)
            {
                iconImage.sprite = definition.Icon;
                iconImage.enabled = definition.Icon != null;
                iconImage.color = definition.AccentColor;
            }

            if (fallbackText != null)
            {
                fallbackText.text = string.IsNullOrWhiteSpace(definition.FallbackGlyph)
                    ? definition.DisplayName.Substring(0, Mathf.Min(definition.DisplayName.Length, 2)).ToUpperInvariant()
                    : definition.FallbackGlyph;
                fallbackText.color = definition.AccentColor;
            }

            if (backplate != null)
            {
                backplate.color = new Color(0.025f, 0.024f, 0.022f, 0.88f);
            }
        }

        public void SetState(bool hovered, bool selected)
        {
            if (selected)
            {
                targetScale = selectedScale;
                targetAlpha = selectedAlpha;
            }
            else if (hovered)
            {
                targetScale = hoverScale;
                targetAlpha = hoverAlpha;
            }
            else
            {
                targetScale = defaultScale;
                targetAlpha = defaultAlpha;
            }
        }
    }
}

using System.Collections.Generic;
using System;
using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.MapScene
{
    public sealed class TownWorldLabelOverlay : MonoBehaviour
    {
        const string ResourcePath = "MapScene/TownWorldLabel";
        const int SortingOrder = 20;
        const float ScreenBoundsMargin = 96f;

        readonly List<global::Assets.Scripts.MapScene.TownWorldLabelView> labels = new List<global::Assets.Scripts.MapScene.TownWorldLabelView>();

        RectTransform canvasRect;
        Vector3 worldOffset;
        GameObject labelPrefab;

        public void Initialize(
            Vector3 townLabelWorldOffset,
            IReadOnlyList<TownObject> townObjects)
        {
            worldOffset = townLabelWorldOffset;
            labelPrefab ??= Resources.Load<GameObject>(ResourcePath);

            EnsureCanvas();
            RebuildLabels(townObjects);
        }

        void LateUpdate()
        {
            if (canvasRect == null || labels.Count == 0)
            {
                return;
            }

            Camera worldCamera = Camera.main;
            for (int i = 0; i < labels.Count; i++)
            {
                labels[i].Refresh(worldCamera, canvasRect, worldOffset, ScreenBoundsMargin);
            }
        }

        void OnDestroy()
        {
            ClearLabels();
        }

        void EnsureCanvas()
        {
            if (canvasRect != null)
            {
                return;
            }

            EnsureEventSystem();

            GameObject canvasObject = new GameObject(
                "TownWorldLabelCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            CanvasGroup canvasGroup = canvasObject.GetComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            canvasRect = (RectTransform)canvasObject.transform;
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;
        }

        static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            Type inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                eventSystemObject.AddComponent(inputSystemModuleType);
                return;
            }

            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        void RebuildLabels(
            IReadOnlyList<TownObject> townObjects)
        {
            ClearLabels();

            if (canvasRect == null || townObjects == null || labelPrefab == null)
            {
                return;
            }

            for (int i = 0; i < townObjects.Count; i++)
            {
                TownObject townObject = townObjects[i];
                Town town = townObject != null ? townObject.Town : null;
                if (townObject == null || town == null)
                {
                    continue;
                }

                GameObject instance = Instantiate(labelPrefab, canvasRect, false);
                instance.name = $"TownWorldLabel_{town.Name}";

                global::Assets.Scripts.MapScene.TownWorldLabelView view = instance.GetComponent<global::Assets.Scripts.MapScene.TownWorldLabelView>();
                if (view == null)
                {
                    Destroy(instance);
                    continue;
                }

                view.Bind(townObject);
                labels.Add(view);
            }
        }

        void ClearLabels()
        {
            for (int i = 0; i < labels.Count; i++)
            {
                if (labels[i] != null)
                {
                    Destroy(labels[i].gameObject);
                }
            }

            labels.Clear();
        }
    }
}

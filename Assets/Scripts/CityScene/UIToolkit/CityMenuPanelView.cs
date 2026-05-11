using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts.CityScene.UIToolkit
{
    public class CityMenuPanelView : IDisposable
    {
        const string PanelNamePrefix = "CityMenuPanel_";
        const string PanelClass = "city-menu-panel";
        const string FrameClass = "city-menu-panel__frame";
        const string FrameTopRowClass = "city-menu-panel__frame-row";
        const string FrameTopRowTopClass = "city-menu-panel__frame-row--top";
        const string FrameTopRowBottomClass = "city-menu-panel__frame-row--bottom";
        const string FrameMiddleRowClass = "city-menu-panel__frame-middle";
        const string FrameCornerClass = "city-menu-panel__frame-corner";
        const string FrameCornerTopClass = "city-menu-panel__frame-corner--top";
        const string FrameCornerBottomClass = "city-menu-panel__frame-corner--bottom";
        const string FrameStretchClass = "city-menu-panel__frame-stretch";
        const string FrameStretchTopClass = "city-menu-panel__frame-stretch--top";
        const string FrameStretchBottomClass = "city-menu-panel__frame-stretch--bottom";
        const string FrameMiddleClass = "city-menu-panel__frame-middle-cap";
        const string FrameMiddleTopClass = "city-menu-panel__frame-middle-cap--top";
        const string FrameMiddleBottomClass = "city-menu-panel__frame-middle-cap--bottom";
        const string FrameEdgeClass = "city-menu-panel__frame-edge";
        const string FrameEdgeLeftClass = "city-menu-panel__frame-edge--left";
        const string FrameEdgeRightClass = "city-menu-panel__frame-edge--right";
        const string FrameFillClass = "city-menu-panel__frame-fill";
        const string HeaderBackgroundClass = "city-menu-panel__header-background";
        const string TitleName = "CityMenuPanelTitle";
        const string TitleClass = "city-menu-panel__title";
        const string ContentName = "CityMenuPanelContent";
        const string ContentClass = "city-menu-panel__content";
        const string CloseButtonName = "CityMenuPanelCloseButton";
        const string CloseButtonClass = "city-menu-panel__close";

        readonly Button closeButton;

        public event Action CloseRequested;

        public CityMenuPanelView(string menuItemId, string title)
        {
            MenuItemId = menuItemId;

            Root = new VisualElement
            {
                name = PanelNamePrefix + SanitizeId(menuItemId),
                pickingMode = PickingMode.Position
            };
            Root.AddToClassList(PanelClass);

            Root.Add(CreateFrame());
            Root.Add(CreateElement(HeaderBackgroundClass));

            Label titleLabel = new Label(title.ToUpper())
            {
                name = TitleName,
                pickingMode = PickingMode.Ignore
            };
            titleLabel.AddToClassList(TitleClass);
            Root.Add(titleLabel);

            ContentRoot = new VisualElement
            {
                name = ContentName,
                pickingMode = PickingMode.Position
            };
            ContentRoot.AddToClassList(ContentClass);
            Root.Add(ContentRoot);

            closeButton = new Button(HandleCloseClicked)
            {
                name = CloseButtonName,
                pickingMode = PickingMode.Position
            };
            closeButton.AddToClassList(CloseButtonClass);
            Root.Add(closeButton);

            Hide();
        }

        public string MenuItemId { get; }
        public VisualElement Root { get; }
        protected VisualElement ContentRoot { get; }

        public void Show()
        {
            Root.style.display = DisplayStyle.Flex;
            Root.BringToFront();
        }

        public void Hide()
        {
            Root.style.display = DisplayStyle.None;
        }

        public virtual void Dispose()
        {
            closeButton.clicked -= HandleCloseClicked;
            CloseRequested = null;
        }

        void HandleCloseClicked()
        {
            CloseRequested?.Invoke();
        }

        static string SanitizeId(string menuItemId)
        {
            return menuItemId.Replace(' ', '_').Replace('-', '_');
        }

        static VisualElement CreateFrame()
        {
            VisualElement frame = CreateElement(FrameClass);
            frame.Add(CreateFrameRow(isTop: true));
            frame.Add(CreateFrameMiddle());
            frame.Add(CreateFrameRow(isTop: false));
            return frame;
        }

        static VisualElement CreateFrameRow(bool isTop)
        {
            VisualElement row = CreateElement(FrameTopRowClass, isTop ? FrameTopRowTopClass : FrameTopRowBottomClass);
            row.Add(CreateFrameCorner(isTop, mirrorHorizontally: false));
            row.Add(CreateElement(FrameStretchClass, isTop ? FrameStretchTopClass : FrameStretchBottomClass));
            row.Add(CreateElement(FrameMiddleClass, isTop ? FrameMiddleTopClass : FrameMiddleBottomClass));
            row.Add(CreateElement(FrameStretchClass, isTop ? FrameStretchTopClass : FrameStretchBottomClass));
            row.Add(CreateFrameCorner(isTop, mirrorHorizontally: true));
            return row;
        }

        static VisualElement CreateFrameMiddle()
        {
            VisualElement middle = CreateElement(FrameMiddleRowClass);
            middle.Add(CreateFrameEdge(mirrorHorizontally: false));
            middle.Add(CreateElement(FrameFillClass));
            middle.Add(CreateFrameEdge(mirrorHorizontally: true));
            return middle;
        }

        static VisualElement CreateFrameCorner(bool isTop, bool mirrorHorizontally)
        {
            VisualElement corner = CreateElement(FrameCornerClass, isTop ? FrameCornerTopClass : FrameCornerBottomClass);
            if (mirrorHorizontally)
            {
                corner.transform.scale = new Vector3(-1f, 1f, 1f);
            }

            return corner;
        }

        static VisualElement CreateFrameEdge(bool mirrorHorizontally)
        {
            VisualElement edge = CreateElement(FrameEdgeClass, mirrorHorizontally ? FrameEdgeRightClass : FrameEdgeLeftClass);
            if (mirrorHorizontally)
            {
                edge.transform.scale = new Vector3(-1f, 1f, 1f);
            }

            return edge;
        }

        static VisualElement CreateElement(params string[] classes)
        {
            VisualElement element = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };

            for (int i = 0; i < classes.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(classes[i]))
                {
                    element.AddToClassList(classes[i]);
                }
            }

            return element;
        }
    }
}

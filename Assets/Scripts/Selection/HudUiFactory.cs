using UnityEngine;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public static class HudUiFactory
    {
        public const float Margin = 12f;
        public const float ResourcePanelWidth = 210f;
        public const float IdleHudGap = 8f;

        static Font defaultFont;

        public static Font DefaultFont
        {
            get
            {
                if (defaultFont == null)
                    defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return defaultFont;
            }
        }

        public static Transform GetHudRoot()
        {
            GameObject canvasObject = GameObject.Find("GameplayCanvas");
            if (canvasObject == null)
                return null;

            return canvasObject.transform.Find("HudRoot");
        }

        public static RectTransform GetOrCreateHudChild(Transform hudRoot, string name)
        {
            Transform existing = hudRoot.Find(name);
            if (existing != null)
                return EnsureRectTransform(existing.gameObject);

            GameObject childObject = new GameObject(name, typeof(RectTransform));
            childObject.transform.SetParent(hudRoot, false);
            return childObject.GetComponent<RectTransform>();
        }

        public static RectTransform EnsureRectTransform(GameObject target)
        {
            RectTransform rectTransform = target.GetComponent<RectTransform>();
            if (rectTransform == null)
                rectTransform = target.AddComponent<RectTransform>();
            return rectTransform;
        }

        public static RectTransform SetupScreenPanel(
            Transform hudRoot,
            string name,
            Color backgroundColor,
            float x,
            float y,
            float width,
            float height,
            bool topLeftAnchor)
        {
            RectTransform panel = GetOrCreateHudChild(hudRoot, name);
            ClearLegacyPanelWrapper(panel);
            if (topLeftAnchor)
                SetAnchoredTopLeft(panel, x, y, width, height);
            else
                SetAnchoredTopCenter(panel, y, width, height);

            EnsurePanelBackground(panel, backgroundColor);
            return panel;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color backgroundColor)
        {
            GameObject panelObject = new GameObject(name, typeof(RectTransform));
            panelObject.transform.SetParent(parent, false);
            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            EnsurePanelBackground(rectTransform, backgroundColor);
            return rectTransform;
        }

        public static void ClearLegacyPanelWrapper(RectTransform panel)
        {
            Transform legacy = panel.Find("Panel");
            if (legacy != null && legacy.parent == panel)
                Object.Destroy(legacy.gameObject);
        }

        public static void EnsurePanelBackground(RectTransform panel, Color backgroundColor)
        {
            Image image = panel.GetComponent<Image>();
            if (image == null)
                image = panel.gameObject.AddComponent<Image>();
            image.color = backgroundColor;
            image.raycastTarget = true;
        }

        public static void SetAnchoredTopLeft(RectTransform rectTransform, float x, float y, float width, float height)
        {
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(x, -y);
            rectTransform.sizeDelta = new Vector2(width, height);
        }

        public static void SetAnchoredTopCenter(RectTransform rectTransform, float y, float width, float height)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -y);
            rectTransform.sizeDelta = new Vector2(width, height);
        }

        public static void SetStretchFull(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        public static void SetAnchoredBottomLeft(RectTransform rectTransform, float x, float y, float width, float height)
        {
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 0f);
            rectTransform.pivot = new Vector2(0f, 0f);
            rectTransform.anchoredPosition = new Vector2(x, y);
            rectTransform.sizeDelta = new Vector2(width, height);
        }

        public static VerticalLayoutGroup AddVerticalLayout(
            RectTransform rectTransform,
            float spacing,
            bool reverseArrangement,
            bool autoHeight = false,
            bool expandWidth = true)
        {
            VerticalLayoutGroup layout = rectTransform.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
                layout = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = spacing;
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = expandWidth;
            layout.childForceExpandHeight = false;
            layout.reverseArrangement = reverseArrangement;

            ContentSizeFitter fitter = rectTransform.GetComponent<ContentSizeFitter>();
            if (autoHeight)
            {
                if (fitter == null)
                    fitter = rectTransform.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else if (fitter != null)
                Object.Destroy(fitter);

            return layout;
        }

        public static Text CreateLabel(Transform parent, string name, float preferredHeight, bool bold = false)
        {
            GameObject labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.transform.SetParent(parent, false);
            LayoutElement layoutElement = labelObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.minHeight = preferredHeight;
            Text text = labelObject.AddComponent<Text>();
            text.font = DefaultFont;
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.supportRichText = false;
            if (bold)
                text.fontStyle = FontStyle.Bold;
            return text;
        }

        public static Button CreateButton(Transform parent, string name, float preferredHeight)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);
            LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.minHeight = preferredHeight;
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.22f, 0.22f, 0.22f, 0.95f);
            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.22f, 0.22f, 0.22f, 0.95f);
            colors.highlightedColor = new Color(0.32f, 0.32f, 0.32f, 0.95f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
            colors.disabledColor = new Color(0.12f, 0.12f, 0.12f, 0.55f);
            button.colors = colors;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            SetStretchFull(labelRect);
            Text label = labelObject.AddComponent<Text>();
            label.font = DefaultFont;
            label.fontSize = 13;
            label.color = Color.white;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            label.supportRichText = false;
            return button;
        }

        public static Slider CreateSlider(Transform parent, string name, float preferredHeight)
        {
            GameObject sliderObject = new GameObject(name, typeof(RectTransform));
            sliderObject.transform.SetParent(parent, false);
            LayoutElement layoutElement = sliderObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = preferredHeight;
            layoutElement.minHeight = preferredHeight;
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.interactable = false;
            slider.minValue = 0f;
            slider.maxValue = 1f;

            GameObject backgroundObject = new GameObject("Background", typeof(RectTransform));
            backgroundObject.transform.SetParent(sliderObject.transform, false);
            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            SetStretchFull(backgroundRect);
            Image backgroundImage = backgroundObject.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            GameObject fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaObject.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillAreaObject.GetComponent<RectTransform>();
            SetStretchFull(fillAreaRect);

            GameObject fillObject = new GameObject("Fill", typeof(RectTransform));
            fillObject.transform.SetParent(fillAreaObject.transform, false);
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            SetStretchFull(fillRect);
            Image fillImage = fillObject.AddComponent<Image>();
            fillImage.color = new Color(0.25f, 0.55f, 0.25f, 0.95f);

            slider.fillRect = fillRect;
            slider.targetGraphic = backgroundImage;
            return slider;
        }

        public static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
                return;

            Text text = button.GetComponentInChildren<Text>();
            if (text != null)
                text.text = label;
        }

        public static void SetText(Text textComponent, string value)
        {
            if (textComponent != null)
                textComponent.text = value;
        }

        public static Color PanelBackgroundColor => new Color(0f, 0f, 0f, 0.55f);
        public static Color OverlayBackgroundColor => new Color(0f, 0f, 0f, 0.72f);
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace AoE.RTS.Selection
{
    /// <summary>
    /// HUD ヒット判定。uGUI RectTransform を主に使い、OnGUI 残存パネル用の Rect フォールバックも保持する。
    /// </summary>
    public static class GameUiInput
    {
        static readonly List<RectTransform> hudPanelRects = new List<RectTransform>();
        static Rect hudHintScreenRect;
        static Rect legacyHudPanelScreenRect;
        static bool hasLegacyHudPanelRect;
        static bool hasHudHintRect;

        public static void RegisterHudPanel(RectTransform panelRect)
        {
            if (panelRect == null || hudPanelRects.Contains(panelRect))
                return;

            hudPanelRects.Add(panelRect);
        }

        public static void UnregisterHudPanel(RectTransform panelRect)
        {
            if (panelRect == null)
                return;

            hudPanelRects.Remove(panelRect);
        }

        public static void SetHudHintScreenRect(Rect screenRect)
        {
            hudHintScreenRect = screenRect;
            hasHudHintRect = screenRect.width > 0f && screenRect.height > 0f;
        }

        public static void ClearHudHintScreenRect()
        {
            hudHintScreenRect = Rect.zero;
            hasHudHintRect = false;
        }

        public static void SetHudPanelScreenRect(Rect screenRect)
        {
            legacyHudPanelScreenRect = screenRect;
            hasLegacyHudPanelRect = true;
        }

        public static void ExpandHudPanelScreenRect(Rect guiRect)
        {
            Rect screenRect = GuiRectToScreenRect(guiRect);
            if (!hasLegacyHudPanelRect)
            {
                legacyHudPanelScreenRect = screenRect;
                hasLegacyHudPanelRect = true;
                return;
            }

            legacyHudPanelScreenRect = UnionRects(legacyHudPanelScreenRect, screenRect);
        }

        public static void BeginHudLayoutFrame()
        {
            hasLegacyHudPanelRect = false;
        }

        public static void ResetHudPanelRects()
        {
            hasLegacyHudPanelRect = false;
            hasHudHintRect = false;
        }

        public static bool IsPointerOverHud(Vector2 screenPosition)
        {
            for (int i = 0; i < hudPanelRects.Count; i++)
            {
                RectTransform panelRect = hudPanelRects[i];
                if (panelRect == null || !panelRect.gameObject.activeInHierarchy)
                    continue;

                if (RectTransformContainsScreenPoint(panelRect, screenPosition))
                    return true;
            }

            if (hasLegacyHudPanelRect && legacyHudPanelScreenRect.Contains(screenPosition))
                return true;

            if (hasHudHintRect && hudHintScreenRect.Contains(screenPosition))
                return true;

            return false;
        }

        public static bool RectTransformContainsScreenPoint(RectTransform rectTransform, Vector2 screenPosition)
        {
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            UnityEngine.Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition, eventCamera);
        }

        public static Rect RectTransformToScreenRect(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            float xMin = corners[0].x;
            float yMin = corners[0].y;
            float xMax = corners[2].x;
            float yMax = corners[2].y;
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        public static void RegisterHintFromRectTransform(RectTransform hintRect)
        {
            if (hintRect == null || !hintRect.gameObject.activeInHierarchy)
            {
                ClearHudHintScreenRect();
                return;
            }

            SetHudHintScreenRect(RectTransformToScreenRect(hintRect));
        }

        static Rect UnionRects(Rect a, Rect b)
        {
            float xMin = Mathf.Min(a.xMin, b.xMin);
            float yMin = Mathf.Min(a.yMin, b.yMin);
            float xMax = Mathf.Max(a.xMax, b.xMax);
            float yMax = Mathf.Max(a.yMax, b.yMax);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        public static Rect GuiRectToScreenRect(Rect guiRect)
        {
            float screenY = Screen.height - guiRect.y - guiRect.height;
            return new Rect(guiRect.x, screenY, guiRect.width, guiRect.height);
        }
    }
}

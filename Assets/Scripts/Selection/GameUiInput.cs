using UnityEngine;

namespace AoE.RTS.Selection
{
    /// <summary>
    /// OnGUI は左上原点、Input System の Pointer は左下原点。HUD ヒット判定用に画面座標 Rect を保持する。
    /// </summary>
    public static class GameUiInput
    {
        static Rect hudPanelScreenRect;
        static Rect hudHintScreenRect;
        static bool hasHudPanelRect;

        public static void SetHudPanelScreenRect(Rect screenRect)
        {
            hudPanelScreenRect = screenRect;
            hasHudPanelRect = true;
        }

        public static void SetHudHintScreenRect(Rect screenRect)
        {
            hudHintScreenRect = screenRect;
        }

        public static void ClearHudHintScreenRect()
        {
            hudHintScreenRect = Rect.zero;
        }

        public static bool IsPointerOverHud(Vector2 screenPosition)
        {
            EnsureFallbackHudRect();

            if (hasHudPanelRect && hudPanelScreenRect.Contains(screenPosition))
                return true;

            if (hudHintScreenRect.width > 0f && hudHintScreenRect.Contains(screenPosition))
                return true;

            return false;
        }

        static void EnsureFallbackHudRect()
        {
            if (hasHudPanelRect)
                return;

            const float margin = 12f;
            const float panelWidth = 210f;
            const float panelHeight = 132f;
            SetHudPanelScreenRect(GuiRectToScreenRect(new Rect(margin, margin, panelWidth, panelHeight)));
        }

        public static Rect GuiRectToScreenRect(Rect guiRect)
        {
            float screenY = Screen.height - guiRect.y - guiRect.height;
            return new Rect(guiRect.x, screenY, guiRect.width, guiRect.height);
        }
    }
}

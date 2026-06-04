using UnityEngine;

namespace AoE.RTS.Selection
{
    public class SelectionBoxView : MonoBehaviour
    {
        static readonly Color BoxColor = new Color(0.2f, 0.85f, 0.3f, 0.25f);

        bool isVisible;
        Vector2 startScreen;
        Vector2 endScreen;

        public void Show(Vector2 screenStart, Vector2 screenEnd)
        {
            isVisible = true;
            startScreen = screenStart;
            endScreen = screenEnd;
        }

        public void Hide()
        {
            isVisible = false;
        }

        void OnGUI()
        {
            if (!isVisible)
                return;

            Rect rect = ScreenRectToGuiRect(startScreen, endScreen);
            Color previous = GUI.color;
            GUI.color = BoxColor;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        static Rect ScreenRectToGuiRect(Vector2 a, Vector2 b)
        {
            float xMin = Mathf.Min(a.x, b.x);
            float xMax = Mathf.Max(a.x, b.x);
            float yMinScreen = Mathf.Min(a.y, b.y);
            float yMaxScreen = Mathf.Max(a.y, b.y);
            float guiYMin = Screen.height - yMaxScreen;
            float guiYMax = Screen.height - yMinScreen;
            return Rect.MinMaxRect(xMin, guiYMin, xMax, guiYMax);
        }
    }
}

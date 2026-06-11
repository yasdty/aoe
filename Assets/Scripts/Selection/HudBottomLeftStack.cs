using UnityEngine;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public static class HudBottomLeftStack
    {
        public static Transform GetOrCreate()
        {
            Transform hudRoot = HudUiFactory.GetHudRoot();
            if (hudRoot == null)
                return null;

            RectTransform stackRect = HudUiFactory.GetOrCreateHudChild(hudRoot, "BottomLeftStack");
            HudUiFactory.SetAnchoredBottomLeft(stackRect, HudUiFactory.Margin, HudUiFactory.Margin, 280f, 600f);

            HudUiFactory.AddVerticalLayout(stackRect, 4f, reverseArrangement: true, autoHeight: true, expandWidth: false);

            GameUiInput.RegisterHudPanel(stackRect);
            return stackRect;
        }
    }
}

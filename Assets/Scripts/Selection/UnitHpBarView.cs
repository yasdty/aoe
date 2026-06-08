using System.Collections.Generic;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class UnitHpBarView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;

        const float PanelWidth = 260f;
        const float LabelHeight = 16f;
        const float BarHeight = 10f;
        const float RowGap = 4f;
        const float RowHeight = LabelHeight + RowGap + BarHeight + RowGap;
        const float Margin = 12f;
        const float Padding = 8f;

        void Awake()
        {
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();
        }

        void OnGUI()
        {
            if (selectionManager == null)
                return;

            IReadOnlyList<Unit> units = selectionManager.SelectedUnits;
            if (units == null || units.Count <= 1)
                return;

            int aliveCount = 0;
            for (int i = 0; i < units.Count; i++)
            {
                if (units[i] != null && units[i].IsAlive)
                    aliveCount++;
            }

            if (aliveCount == 0)
                return;

            float panelHeight = Padding * 2f + aliveCount * RowHeight;
            float panelX = Screen.width * 0.5f - PanelWidth * 0.5f;
            float panelY = Screen.height - panelHeight - Margin - 96f;
            Rect panelRect = new Rect(panelX, panelY, PanelWidth, panelHeight);

            GUI.Box(panelRect, GUIContent.none);

            float y = panelY + Padding;
            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || !unit.IsAlive)
                    continue;

                string label = unit.Data != null ? unit.Data.displayName : "Unit";
                float hpRatio = unit.MaxHp > 0f ? unit.CurrentHp / unit.MaxHp : 0f;

                Rect labelRect = new Rect(panelX + Padding, y, PanelWidth - Padding * 2f, LabelHeight);
                GUI.Label(labelRect, $"{label}  HP: {Mathf.CeilToInt(unit.CurrentHp)}/{Mathf.CeilToInt(unit.MaxHp)}");

                Rect barRect = new Rect(
                    panelX + Padding,
                    y + LabelHeight + RowGap,
                    PanelWidth - Padding * 2f,
                    BarHeight);
                GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.85f);
                GUI.DrawTexture(barRect, Texture2D.whiteTexture);
                Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * hpRatio, barRect.height);
                GUI.color = hpRatio > 0.35f
                    ? new Color(0.25f, 0.75f, 0.3f)
                    : new Color(0.85f, 0.25f, 0.2f);
                GUI.DrawTexture(fillRect, Texture2D.whiteTexture);
                GUI.color = Color.white;

                y += RowHeight;
            }
        }
    }
}

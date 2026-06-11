using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class IdleUnitHudView : MonoBehaviour
    {
        [SerializeField] IdleUnitSelectionController idleSelectionController;

        const float Margin = 12f;
        const float ResourceHudWidth = 210f;
        const float HudGap = 8f;
        const float PanelWidth = 200f;
        const float LineHeight = 22f;
        const float ButtonHeight = 24f;
        const float Padding = 8f;
        const float ButtonGap = 4f;

        void Awake()
        {
            if (idleSelectionController == null)
                idleSelectionController = GetComponent<IdleUnitSelectionController>();
        }

        void OnGUI()
        {
            float panelHeight = Padding * 2f + LineHeight + ButtonGap + LineHeight + ButtonGap + ButtonHeight;
            float panelX = Margin + ResourceHudWidth + HudGap;
            Rect panelRect = new Rect(panelX, Margin, PanelWidth, panelHeight);
            GameUiInput.ExpandHudPanelScreenRect(panelRect);

            GUI.Box(panelRect, GUIContent.none);

            int idleVillagers = UnitIdleTracker.CountIdleVillagers();
            int idleMilitary = UnitIdleTracker.CountIdleMilitary();

            GUILayout.BeginArea(panelRect);
            GUILayout.Label(Localization.Format("ui.idle_villagers", idleVillagers));
            GUILayout.Label(Localization.Format("ui.idle_military", idleMilitary));

            GUI.enabled = idleVillagers > 0 && !GameSessionManager.IsGameOver;
            if (GUILayout.Button(Localization.Get("ui.next_idle_villager")) && idleSelectionController != null)
                idleSelectionController.SelectNextIdleVillager();
            GUI.enabled = true;

            GUILayout.EndArea();
        }
    }
}

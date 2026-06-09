using System.Collections.Generic;
using AoE.RTS.Combat;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class UnitStancePanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;

        const float PanelWidth = 220f;
        const float PanelHeight = 88f;
        const float Margin = 12f;
        const float ProductionPanelReserveHeight = 96f;
        const float InfoPanelReserveHeight = 120f;

        readonly List<Unit> militaryBuffer = new List<Unit>(16);

        public static float ReserveHeight => PanelHeight + Margin;

        void Awake()
        {
            if (selectionManager == null)
                selectionManager = GetComponent<SelectionManager>();
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();
        }

        void OnGUI()
        {
            if (selectionManager == null || !TryCollectMilitaryUnits(militaryBuffer))
                return;

            float bottomOffset = Margin;
            if (selectionManager.SelectedTownCenter != null || selectionManager.SelectedBarracks != null
                || selectionManager.SelectedArcheryRange != null || selectionManager.SelectedStable != null)
                bottomOffset += ProductionPanelReserveHeight;

            if (selectionManager.ShouldShowSelectionInfoPanel)
                bottomOffset += InfoPanelReserveHeight;

            Rect panelRect = new Rect(Margin, Screen.height - PanelHeight - bottomOffset, PanelWidth, PanelHeight);
            GameUiInput.ExpandHudPanelScreenRect(panelRect);
            GUI.Box(panelRect, GUIContent.none);

            GUILayout.BeginArea(panelRect);
            GUILayout.Label("Stance");

            if (GUILayout.Button("Aggressive"))
                ApplyStance(UnitCombatStance.Aggressive);

            if (GUILayout.Button("Defensive"))
                ApplyStance(UnitCombatStance.Defensive);

            if (GUILayout.Button("Stand Ground"))
                ApplyStance(UnitCombatStance.StandGround);

            GUILayout.EndArea();
        }

        bool TryCollectMilitaryUnits(List<Unit> buffer)
        {
            buffer.Clear();
            if (selectionManager == null)
                return false;

            IReadOnlyList<Unit> selectedUnits = selectionManager.SelectedUnits;
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                Unit unit = selectedUnits[i];
                if (unit == null || !unit.IsAlive || !unit.CanAttack)
                    continue;

                buffer.Add(unit);
            }

            return buffer.Count > 0;
        }

        void ApplyStance(UnitCombatStance stance)
        {
            for (int i = 0; i < militaryBuffer.Count; i++)
                militaryBuffer[i].SetCombatStance(stance);
        }
    }
}

using System.Collections.Generic;
using AoE.RTS.Combat;
using AoE.RTS.Units;
using AoE.RTS.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public class UnitStancePanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;

        const float PanelWidth = 220f;
        const float LineHeight = 20f;
        const float ButtonHeight = 24f;

        readonly List<Unit> militaryBuffer = new List<Unit>(16);
        RectTransform panelRoot;
        Text headerText;
        Button aggressiveButton;
        Button defensiveButton;
        Button standGroundButton;
        bool uiBuilt;

        public static float ReserveHeight => 88f + HudUiFactory.Margin;

        void Awake()
        {
            if (selectionManager == null)
                selectionManager = GetComponent<SelectionManager>();
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();
            TryBuildUi();
        }

        void OnDestroy()
        {
            if (panelRoot != null)
                GameUiInput.UnregisterHudPanel(panelRoot);
        }

        void TryBuildUi()
        {
            if (uiBuilt)
                return;

            Transform stack = HudBottomLeftStack.GetOrCreate();
            if (stack == null)
                return;

            panelRoot = HudUiFactory.CreatePanel(stack, "UnitStancePanel", HudUiFactory.PanelBackgroundColor);
            panelRoot.SetAsLastSibling();
            HudUiFactory.AddVerticalLayout(panelRoot, 4f, reverseArrangement: false);
            panelRoot.gameObject.AddComponent<LayoutElement>().preferredWidth = PanelWidth;
            GameUiInput.RegisterHudPanel(panelRoot);

            headerText = HudUiFactory.CreateLabel(panelRoot, "Header", LineHeight, bold: true);
            aggressiveButton = HudUiFactory.CreateButton(panelRoot, "Aggressive", ButtonHeight);
            aggressiveButton.onClick.AddListener(() => ApplyStance(UnitCombatStance.Aggressive));
            defensiveButton = HudUiFactory.CreateButton(panelRoot, "Defensive", ButtonHeight);
            defensiveButton.onClick.AddListener(() => ApplyStance(UnitCombatStance.Defensive));
            standGroundButton = HudUiFactory.CreateButton(panelRoot, "StandGround", ButtonHeight);
            standGroundButton.onClick.AddListener(() => ApplyStance(UnitCombatStance.StandGround));

            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        void LateUpdate()
        {
            TryBuildUi();
            if (!uiBuilt || selectionManager == null)
                return;

            bool visible = TryCollectMilitaryUnits(militaryBuffer);
            panelRoot.gameObject.SetActive(visible);
            if (!visible)
                return;

            HudUiFactory.SetText(headerText, Localization.Get("ui.stance_panel"));
            HudUiFactory.SetButtonLabel(aggressiveButton, Localization.Get("stance.aggressive"));
            HudUiFactory.SetButtonLabel(defensiveButton, Localization.Get("stance.defensive"));
            HudUiFactory.SetButtonLabel(standGroundButton, Localization.Get("stance.stand_ground"));
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

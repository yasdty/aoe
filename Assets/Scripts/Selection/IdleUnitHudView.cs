using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public class IdleUnitHudView : MonoBehaviour
    {
        [SerializeField] IdleUnitSelectionController idleSelectionController;

        const float PanelWidth = 200f;
        const float LineHeight = 22f;
        const float ButtonHeight = 24f;

        RectTransform panelRoot;
        Text villagerText;
        Text militaryText;
        Button nextIdleButton;

        bool uiBuilt;

        void Awake()
        {
            if (idleSelectionController == null)
                idleSelectionController = GetComponent<IdleUnitSelectionController>();
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

            Transform hudRoot = HudUiFactory.GetHudRoot();
            if (hudRoot == null)
                return;

            float x = HudUiFactory.Margin + HudUiFactory.ResourcePanelWidth + HudUiFactory.IdleHudGap;
            panelRoot = HudUiFactory.SetupScreenPanel(
                hudRoot,
                "IdleHudPanel",
                HudUiFactory.PanelBackgroundColor,
                x,
                HudUiFactory.Margin,
                PanelWidth,
                120f,
                topLeftAnchor: true);
            GameUiInput.RegisterHudPanel(panelRoot);
            HudUiFactory.AddVerticalLayout(panelRoot, 4f, reverseArrangement: false);

            villagerText = HudUiFactory.CreateLabel(panelRoot, "IdleVillagers", LineHeight);
            militaryText = HudUiFactory.CreateLabel(panelRoot, "IdleMilitary", LineHeight);
            nextIdleButton = HudUiFactory.CreateButton(panelRoot, "NextIdle", ButtonHeight);
            nextIdleButton.onClick.AddListener(() =>
            {
                if (idleSelectionController != null)
                    idleSelectionController.SelectNextIdleVillager();
            });

            uiBuilt = true;
        }

        void LateUpdate()
        {
            TryBuildUi();
            if (!uiBuilt)
                return;

            HudUiFactory.SetText(
                villagerText,
                Localization.Format("ui.idle_villagers", UnitIdleTracker.CountIdleVillagers()));
            HudUiFactory.SetText(
                militaryText,
                Localization.Format("ui.idle_military", UnitIdleTracker.CountIdleMilitary()));
            HudUiFactory.SetButtonLabel(nextIdleButton, Localization.Get("ui.next_idle_villager"));

            int idleVillagers = UnitIdleTracker.CountIdleVillagers();
            nextIdleButton.interactable = idleVillagers > 0 && !GameSessionManager.IsGameOver;
        }
    }
}

using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class BlacksmithPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;
        [SerializeField] TechnologyData infantryUpgradeTech;

        const float PanelWidth = 240f;
        const float PanelHeight = 130f;
        const float Margin = 12f;

        void OnGUI()
        {
            if (selectionManager == null)
                return;

            Blacksmith blacksmith = selectionManager.SelectedBlacksmith;
            if (blacksmith == null)
                return;

            TechnologyData tech = TechnologyDataResolver.ResolveInfantryUpgrade(ref infantryUpgradeTech);
            if (tech == null)
                return;

            Rect panelRect = new Rect(Margin, Screen.height - PanelHeight - Margin, PanelWidth, PanelHeight);
            GUI.Box(panelRect, GUIContent.none);

            GUILayout.BeginArea(panelRect);
            GUILayout.Label("Blacksmith");

            bool alreadyResearched = TechnologyState.HasInfantryUpgrade(blacksmith.Team);
            bool isResearching = BlacksmithResearchManager.IsResearching(blacksmith);
            bool canAffordFood = ResourceManager.Food >= tech.ScaledFoodCost;
            bool canAffordGold = ResourceManager.Gold >= tech.ScaledGoldCost;
            bool canAfford = canAffordFood && canAffordGold;

            if (alreadyResearched)
            {
                GUILayout.Label($"{tech.displayName}: Complete");
            }
            else
            {
                GUI.enabled = !isResearching && canAfford && !GameSessionManager.IsGameOver;
                if (GUILayout.Button(
                        $"{tech.displayName} (Q) ({Mathf.CeilToInt(tech.ScaledFoodCost)} Food, "
                        + $"{Mathf.CeilToInt(tech.ScaledGoldCost)} Gold)"))
                    CommandQueue.Enqueue(new ResearchInfantryUpgradeCommand(blacksmith));
                GUI.enabled = true;

                if (!canAffordFood)
                    GUILayout.Label("Need more Food");
                else if (!canAffordGold)
                    GUILayout.Label("Need more Gold");
            }

            if (isResearching)
            {
                float total = BlacksmithResearchManager.GetTotalSeconds(blacksmith);
                float remaining = BlacksmithResearchManager.GetRemainingSeconds(blacksmith);
                float progress = total > 0f ? 1f - remaining / total : 0f;
                GUILayout.Label($"Researching... {remaining:0.0}s");
                Rect progressRect = GUILayoutUtility.GetRect(PanelWidth - 24f, 18f);
                GUI.HorizontalSlider(progressRect, progress, 0f, 1f);
            }

            GUILayout.EndArea();
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver || selectionManager == null || input == null)
                return;

            Blacksmith blacksmith = selectionManager.SelectedBlacksmith;
            if (blacksmith == null || TechnologyState.HasInfantryUpgrade(blacksmith.Team))
                return;

            if (input.WasTrainVillagerPressedThisFrame())
                CommandQueue.Enqueue(new ResearchInfantryUpgradeCommand(blacksmith));
        }
    }
}

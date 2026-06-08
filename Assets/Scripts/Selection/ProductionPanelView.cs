using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class ProductionPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;

        const float PanelWidth = 220f;
        const float PanelHeight = 88f;
        const float Margin = 12f;

        void OnGUI()
        {
            if (selectionManager == null)
                return;

            TownCenter townCenter = selectionManager.SelectedTownCenter;
            if (townCenter == null)
                return;

            Rect panelRect = new Rect(Margin, Screen.height - PanelHeight - Margin, PanelWidth, PanelHeight);
            GUI.Box(panelRect, GUIContent.none);

            GUILayout.BeginArea(panelRect);
            GUILayout.Label("Town Center");

            bool isProducing = ProductionManager.IsProducing(townCenter);
            bool populationFull = !PopulationManager.CanTrainUnit();
            float foodCost = townCenter.Data != null ? townCenter.Data.villagerFoodCost : 0f;
            bool canAffordFood = ResourceManager.GetFood(UnitTeam.Player) >= foodCost;
            GUI.enabled = !isProducing && !populationFull && canAffordFood && !GameSessionManager.IsGameOver;
            if (GUILayout.Button($"Create Villager (Q) ({foodCost} Food)"))
                CommandQueue.Enqueue(new TrainVillagerCommand(townCenter));
            GUI.enabled = true;

            if (populationFull && !isProducing)
                GUILayout.Label("Population full");
            else if (!canAffordFood && !isProducing)
                GUILayout.Label("Need more Food");

            if (isProducing)
            {
                float total = ProductionManager.GetTotalSeconds(townCenter);
                float remaining = ProductionManager.GetRemainingSeconds(townCenter);
                float progress = total > 0f ? 1f - remaining / total : 0f;
                GUILayout.Label($"Training... {remaining:0.0}s");
                Rect progressRect = GUILayoutUtility.GetRect(PanelWidth - 24f, 18f);
                GUI.HorizontalSlider(progressRect, progress, 0f, 1f);
            }

            GUILayout.EndArea();
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver || selectionManager == null || input == null)
                return;

            TownCenter townCenter = selectionManager.SelectedTownCenter;
            if (townCenter == null)
                return;

            if (input.WasTrainVillagerPressedThisFrame())
                CommandQueue.Enqueue(new TrainVillagerCommand(townCenter));
        }
    }
}

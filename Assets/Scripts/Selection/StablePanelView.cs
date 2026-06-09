using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class StablePanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;

        const float PanelWidth = 220f;
        const float PanelHeight = 148f;
        const float Margin = 12f;

        void OnGUI()
        {
            if (selectionManager == null)
                return;

            Stable stable = selectionManager.SelectedStable;
            if (stable == null || stable.Data == null)
                return;

            PlacedBuildingData data = stable.Data;
            Rect panelRect = new Rect(Margin, Screen.height - PanelHeight - Margin, PanelWidth, PanelHeight);
            GUI.Box(panelRect, GUIContent.none);

            GUILayout.BeginArea(panelRect);
            GUILayout.Label("Stable");

            int queueCount = StableProductionManager.GetQueueCount(stable);
            bool isProducing = queueCount > 0;
            bool queueFull = queueCount >= StableProductionManager.MaxQueueSize;
            bool populationFull = !PopulationManager.CanTrainUnit();
            bool canAffordCavalryWood = ResourceManager.Wood >= data.trainWoodCost;
            bool canAffordCavalryFood = ResourceManager.Food >= data.trainFoodCost;
            bool canAffordCavalry = canAffordCavalryWood && canAffordCavalryFood;
            bool canAffordScoutWood = ResourceManager.Wood >= data.secondaryTrainWoodCost;
            bool canAffordScoutFood = ResourceManager.Food >= data.secondaryTrainFoodCost;
            bool canAffordScout = canAffordScoutWood && canAffordScoutFood;

            GUI.enabled = !queueFull && !populationFull && canAffordCavalry && !GameSessionManager.IsGameOver;
            if (GUILayout.Button($"Create Cavalry (Q) ({data.trainWoodCost} Wood, {data.trainFoodCost} Food)"))
                CommandQueue.Enqueue(new TrainCavalryCommand(stable));

            GUI.enabled = !queueFull && !populationFull && canAffordScout && !GameSessionManager.IsGameOver;
            if (GUILayout.Button(
                    $"Create Scout (E) ({data.secondaryTrainWoodCost} Wood, {data.secondaryTrainFoodCost} Food)"))
                CommandQueue.Enqueue(new TrainScoutCommand(stable));
            GUI.enabled = true;

            if (queueCount > 0)
                GUILayout.Label($"Queue: {queueCount}");

            if (queueFull)
                GUILayout.Label("Queue full");
            else if (populationFull)
                GUILayout.Label("Population full");
            else if (!canAffordCavalry && !canAffordScout)
                GUILayout.Label("Need more resources");

            if (isProducing)
            {
                float total = StableProductionManager.GetTotalSeconds(stable);
                float remaining = StableProductionManager.GetRemainingSeconds(stable);
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

            Stable stable = selectionManager.SelectedStable;
            if (stable == null)
                return;

            if (input.WasTrainVillagerPressedThisFrame())
                CommandQueue.Enqueue(new TrainCavalryCommand(stable));

            if (input.WasTrainSecondaryPressedThisFrame())
                CommandQueue.Enqueue(new TrainScoutCommand(stable));
        }
    }
}

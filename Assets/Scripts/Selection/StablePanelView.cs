using System.Collections.Generic;
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
        const float PanelHeight = 220f;
        const float Margin = 12f;

        readonly List<ProductionQueueEntry> queueEntriesBuffer = new List<ProductionQueueEntry>();

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
            GUILayout.Label(Localization.BuildingName(PlacedBuildingKind.Stable));

            int queueCount = StableProductionManager.GetQueueCount(stable);
            bool isProducing = queueCount > 0;
            bool queueFull = queueCount >= StableProductionManager.MaxQueueSize;
            bool populationFull = !PopulationManager.CanTrainUnit();
            bool canAffordCavalryWood = ResourceManager.Wood >= data.ScaledTrainWoodCost;
            bool canAffordCavalryFood = ResourceManager.Food >= data.ScaledTrainFoodCost;
            bool canAffordCavalry = canAffordCavalryWood && canAffordCavalryFood;
            bool canAffordScoutWood = data.ScaledSecondaryTrainWoodCost <= 0f
                || ResourceManager.Wood >= data.ScaledSecondaryTrainWoodCost;
            bool canAffordScoutFood = ResourceManager.Food >= data.ScaledSecondaryTrainFoodCost;
            bool canAffordScout = canAffordScoutWood && canAffordScoutFood;

            GUI.enabled = !queueFull && !populationFull && canAffordCavalry && !GameSessionManager.IsGameOver;
            if (GUILayout.Button(
                    Localization.Format(
                        "ui.create_unit_dual",
                        Localization.Get("unit.cavalry"),
                        Mathf.CeilToInt(data.ScaledTrainWoodCost),
                        Mathf.CeilToInt(data.ScaledTrainFoodCost))))
                CommandQueue.Enqueue(new TrainCavalryCommand(stable));

            GUI.enabled = !queueFull && !populationFull && canAffordScout && !GameSessionManager.IsGameOver;
            if (GUILayout.Button(BuildScoutLabel(data)))
                CommandQueue.Enqueue(new TrainScoutCommand(stable));
            GUI.enabled = true;

            if (queueCount > 0)
            {
                StableProductionManager.GetQueueEntries(stable, queueEntriesBuffer);
                ProductionQueuePanelUi.DrawCancelableQueue(
                    queueEntriesBuffer,
                    index => StableProductionManager.TryCancelQueueItem(stable, index));
            }

            if (queueFull)
                GUILayout.Label(Localization.Get("ui.queue_full"));
            else if (populationFull)
                GUILayout.Label(Localization.Get("ui.population_full"));
            else if (!canAffordCavalry && !canAffordScout)
                GUILayout.Label(Localization.Get("ui.need_resources"));

            if (isProducing)
            {
                float total = StableProductionManager.GetTotalSeconds(stable);
                float remaining = StableProductionManager.GetRemainingSeconds(stable);
                float progress = total > 0f ? 1f - remaining / total : 0f;
                GUILayout.Label(Localization.Format("ui.training", remaining));
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

        static string BuildScoutLabel(PlacedBuildingData data)
        {
            if (data.ScaledSecondaryTrainWoodCost > 0f)
            {
                return Localization.Format(
                    "ui.create_unit_dual_e",
                    Localization.Get("unit.scout"),
                    Mathf.CeilToInt(data.ScaledSecondaryTrainWoodCost),
                    Mathf.CeilToInt(data.ScaledSecondaryTrainFoodCost));
            }

            return Localization.Format(
                "ui.create_unit_food_e",
                Localization.Get("unit.scout"),
                Mathf.CeilToInt(data.ScaledSecondaryTrainFoodCost));
        }
    }
}

using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class BarracksPanelView : MonoBehaviour
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

            Barracks barracks = selectionManager.SelectedBarracks;
            if (barracks == null || barracks.Data == null)
                return;

            PlacedBuildingData data = barracks.Data;
            Rect panelRect = new Rect(Margin, Screen.height - PanelHeight - Margin, PanelWidth, PanelHeight);
            GUI.Box(panelRect, GUIContent.none);

            GUILayout.BeginArea(panelRect);
            GUILayout.Label(Localization.BuildingName(PlacedBuildingKind.Barracks));

            int queueCount = BarracksProductionManager.GetQueueCount(barracks);
            bool isProducing = queueCount > 0;
            bool queueFull = queueCount >= BarracksProductionManager.MaxQueueSize;
            bool populationFull = !PopulationManager.CanTrainUnit();
            bool canAffordMilitiaFood = ResourceManager.Food >= data.ScaledTrainFoodCost;
            bool canAffordMilitiaWood = data.ScaledTrainWoodCost <= 0f
                || ResourceManager.Wood >= data.ScaledTrainWoodCost;
            bool canAffordMilitia = canAffordMilitiaFood && canAffordMilitiaWood;
            bool canAffordSpearmanWood = ResourceManager.Wood >= data.ScaledSecondaryTrainWoodCost;
            bool canAffordSpearmanFood = ResourceManager.Food >= data.ScaledSecondaryTrainFoodCost;
            bool canAffordSpearman = canAffordSpearmanWood && canAffordSpearmanFood;

            string primaryTrainName = Localization.UnitName(BarracksTraining.ResolvePrimaryTrainUnit(barracks));
            GUI.enabled = !queueFull && !populationFull && canAffordMilitia && !GameSessionManager.IsGameOver;
            if (GUILayout.Button(BuildPrimaryTrainLabel(primaryTrainName, data)))
                CommandQueue.Enqueue(new TrainMilitiaCommand(barracks));

            GUI.enabled = !queueFull && !populationFull && canAffordSpearman && !GameSessionManager.IsGameOver;
            if (GUILayout.Button(BuildSpearmanLabel(data)))
                CommandQueue.Enqueue(new TrainSpearmanCommand(barracks));
            GUI.enabled = true;

            if (queueCount > 0)
            {
                BarracksProductionManager.GetQueueEntries(barracks, queueEntriesBuffer);
                ProductionQueuePanelUi.DrawCancelableQueue(
                    queueEntriesBuffer,
                    index => BarracksProductionManager.TryCancelQueueItem(barracks, index));
            }

            if (queueFull)
                GUILayout.Label(Localization.Get("ui.queue_full"));
            else if (populationFull)
                GUILayout.Label(Localization.Get("ui.population_full"));
            else if (!canAffordMilitia && !canAffordSpearman)
                GUILayout.Label(Localization.Get("ui.need_resources"));
            else if (!canAffordSpearmanWood || !canAffordSpearmanFood)
            {
                if (!canAffordSpearmanWood)
                    GUILayout.Label(Localization.Get("ui.need_wood_spearman"));
                else
                    GUILayout.Label(Localization.Get("ui.need_food_spearman"));
            }

            if (isProducing)
            {
                float total = BarracksProductionManager.GetTotalSeconds(barracks);
                float remaining = BarracksProductionManager.GetRemainingSeconds(barracks);
                float progress = total > 0f ? 1f - remaining / total : 0f;
                GUILayout.Label(Localization.Format("ui.training", remaining));
                Rect progressRect = GUILayoutUtility.GetRect(PanelWidth - 24f, 18f);
                GUI.HorizontalSlider(progressRect, progress, 0f, 1f);
            }

            GUILayout.EndArea();
        }

        static string BuildPrimaryTrainLabel(string unitName, PlacedBuildingData data)
        {
            if (data.ScaledTrainWoodCost > 0f)
            {
                return Localization.Format(
                    "ui.create_unit_dual",
                    unitName,
                    Mathf.CeilToInt(data.ScaledTrainWoodCost),
                    Mathf.CeilToInt(data.ScaledTrainFoodCost));
            }

            return Localization.Format(
                "ui.create_unit_food",
                unitName,
                Mathf.CeilToInt(data.ScaledTrainFoodCost));
        }

        static string BuildSpearmanLabel(PlacedBuildingData data)
        {
            return Localization.Format(
                "ui.create_unit_dual_e",
                Localization.Get("unit.spearman"),
                Mathf.CeilToInt(data.ScaledSecondaryTrainWoodCost),
                Mathf.CeilToInt(data.ScaledSecondaryTrainFoodCost));
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver || selectionManager == null || input == null)
                return;

            Barracks barracks = selectionManager.SelectedBarracks;
            if (barracks == null)
                return;

            if (input.WasTrainVillagerPressedThisFrame())
            {
                int count = input.IsShiftHeld ? ProductionQueuePanelUi.ShiftBatchQueueCount : 1;
                for (int i = 0; i < count; i++)
                    CommandQueue.Enqueue(new TrainMilitiaCommand(barracks));
            }

            if (input.WasTrainSecondaryPressedThisFrame())
                CommandQueue.Enqueue(new TrainSpearmanCommand(barracks));
        }
    }
}

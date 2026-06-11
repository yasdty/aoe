using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class ArcheryRangePanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;

        const float PanelWidth = 220f;
        const float PanelHeight = 180f;
        const float Margin = 12f;

        readonly List<ProductionQueueEntry> queueEntriesBuffer = new List<ProductionQueueEntry>();

        void OnGUI()
        {
            if (selectionManager == null)
                return;

            ArcheryRange archeryRange = selectionManager.SelectedArcheryRange;
            if (archeryRange == null || archeryRange.Data == null)
                return;

            PlacedBuildingData data = archeryRange.Data;
            Rect panelRect = new Rect(Margin, Screen.height - PanelHeight - Margin, PanelWidth, PanelHeight);
            GUI.Box(panelRect, GUIContent.none);

            GUILayout.BeginArea(panelRect);
            GUILayout.Label(Localization.BuildingName(PlacedBuildingKind.ArcheryRange));

            int queueCount = ArcheryRangeProductionManager.GetQueueCount(archeryRange);
            bool isProducing = queueCount > 0;
            bool queueFull = queueCount >= ArcheryRangeProductionManager.MaxQueueSize;
            bool populationFull = !PopulationManager.CanTrainUnit();
            bool canAffordWood = ResourceManager.Wood >= data.ScaledTrainWoodCost;
            bool canAffordFood = ResourceManager.Food >= data.ScaledTrainFoodCost;
            bool canAfford = canAffordWood && canAffordFood;
            GUI.enabled = !queueFull && !populationFull && canAfford && !GameSessionManager.IsGameOver;
            if (GUILayout.Button(
                    Localization.Format(
                        "ui.create_unit_dual",
                        Localization.Get("unit.archer"),
                        Mathf.CeilToInt(data.ScaledTrainWoodCost),
                        Mathf.CeilToInt(data.ScaledTrainFoodCost))))
                CommandQueue.Enqueue(new TrainArcherCommand(archeryRange));
            GUI.enabled = true;

            if (queueCount > 0)
            {
                ArcheryRangeProductionManager.GetQueueEntries(archeryRange, queueEntriesBuffer);
                ProductionQueuePanelUi.DrawCancelableQueue(
                    queueEntriesBuffer,
                    index => ArcheryRangeProductionManager.TryCancelQueueItem(archeryRange, index));
            }

            if (queueFull)
                GUILayout.Label(Localization.Get("ui.queue_full"));
            else if (populationFull)
                GUILayout.Label(Localization.Get("ui.population_full"));
            else if (!canAffordWood)
                GUILayout.Label(Localization.Get("ui.need_wood"));
            else if (!canAffordFood)
                GUILayout.Label(Localization.Get("ui.need_food"));

            if (isProducing)
            {
                float total = ArcheryRangeProductionManager.GetTotalSeconds(archeryRange);
                float remaining = ArcheryRangeProductionManager.GetRemainingSeconds(archeryRange);
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

            ArcheryRange archeryRange = selectionManager.SelectedArcheryRange;
            if (archeryRange == null)
                return;

            if (input.WasTrainVillagerPressedThisFrame())
                CommandQueue.Enqueue(new TrainArcherCommand(archeryRange));
        }
    }
}

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
    public class ProductionPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;
        [SerializeField] AgeData feudalAgeData;

        const float PanelWidth = 220f;
        const float PanelHeight = 220f;
        const float Margin = 12f;

        readonly List<ProductionQueueEntry> queueEntriesBuffer = new List<ProductionQueueEntry>();

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
            GUILayout.Label(Localization.Get("ui.town_center"));
            GUILayout.Label(Localization.Format("ui.age_label", Localization.AgeName(GameSessionManager.GetAge(townCenter.Team))));

            int queueCount = ProductionManager.GetQueueCount(townCenter);
            bool isProducing = queueCount > 0;
            bool queueFull = queueCount >= ProductionManager.MaxQueueSize;
            bool populationFull = !PopulationManager.CanTrainUnit();
            float foodCost = townCenter.Data != null ? townCenter.Data.ScaledVillagerFoodCost : 0f;
            bool canAffordFood = ResourceManager.GetFood(UnitTeam.Player) >= foodCost;
            GUI.enabled = !queueFull && !populationFull && canAffordFood && !GameSessionManager.IsGameOver;
            if (GUILayout.Button(Localization.Format("ui.create_villager", Mathf.CeilToInt(foodCost))))
                CommandQueue.Enqueue(new TrainVillagerCommand(townCenter));
            GUI.enabled = true;

            DrawAgeUpButton(townCenter);

            if (queueCount > 0)
            {
                ProductionManager.GetQueueEntries(townCenter, queueEntriesBuffer);
                ProductionQueuePanelUi.DrawCancelableQueue(
                    queueEntriesBuffer,
                    index => ProductionManager.TryCancelQueueItem(townCenter, index));
            }

            if (queueFull)
                GUILayout.Label(Localization.Get("ui.queue_full"));
            else if (populationFull)
                GUILayout.Label(Localization.Get("ui.population_full"));
            else if (!canAffordFood)
                GUILayout.Label(Localization.Get("ui.need_food"));

            if (isProducing)
            {
                float total = ProductionManager.GetTotalSeconds(townCenter);
                float remaining = ProductionManager.GetRemainingSeconds(townCenter);
                float progress = total > 0f ? 1f - remaining / total : 0f;
                GUILayout.Label(Localization.Format("ui.training", remaining));
                Rect progressRect = GUILayoutUtility.GetRect(PanelWidth - 24f, 18f);
                GUI.HorizontalSlider(progressRect, progress, 0f, 1f);
            }

            GUILayout.EndArea();
        }

        void DrawAgeUpButton(TownCenter townCenter)
        {
            if (townCenter.Team != UnitTeam.Player)
                return;

            if (GameSessionManager.GetAge(townCenter.Team) >= GameAge.Feudal)
                return;

            AgeData ageData = feudalAgeData;
            if (ageData == null)
                return;

            float foodCost = GameplayBalance.ScaleResourceCost(ageData.upgradeFoodCost);
            float goldCost = GameplayBalance.ScaleResourceCost(ageData.upgradeGoldCost);
            bool canAfford = ResourceManager.Food >= foodCost && ResourceManager.Gold >= goldCost;
            GUI.enabled = canAfford && !GameSessionManager.IsGameOver;
            if (GUILayout.Button(
                    Localization.Format(
                        "ui.age_up_feudal",
                        Mathf.CeilToInt(foodCost),
                        Mathf.CeilToInt(goldCost))))
                CommandQueue.Enqueue(new AgeUpCommand(townCenter));
            GUI.enabled = true;

            if (!canAfford)
                GUILayout.Label(Localization.Get("ui.need_food_gold_feudal"));
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver || selectionManager == null || input == null)
                return;

            TownCenter townCenter = selectionManager.SelectedTownCenter;
            if (townCenter == null)
                return;

            if (input.WasTrainVillagerPressedThisFrame())
            {
                int count = input.IsShiftHeld ? ProductionQueuePanelUi.ShiftBatchQueueCount : 1;
                for (int i = 0; i < count; i++)
                    CommandQueue.Enqueue(new TrainVillagerCommand(townCenter));
            }
        }
    }
}

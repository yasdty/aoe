using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Commands;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using UnityEngine;
using UnityEngine.UI;

namespace AoE.RTS.Selection
{
    public class BarracksPanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;

        const float PanelWidth = 220f;
        const float LineHeight = 20f;
        const float ButtonHeight = 28f;

        RectTransform panelRoot;
        Text headerText;
        Button primaryTrainButton;
        Button spearmanButton;
        Text statusText;
        Text trainingText;
        Slider progressSlider;
        ProductionQueueListView queueListView;
        readonly List<ProductionQueueEntry> queueEntriesBuffer = new List<ProductionQueueEntry>();
        bool uiBuilt;

        void Awake()
        {
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();
            if (input == null)
                input = FindAnyObjectByType<RTSInputReader>();
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

            panelRoot = HudUiFactory.CreatePanel(stack, "BarracksPanel", HudUiFactory.PanelBackgroundColor);
            HudUiFactory.AddVerticalLayout(panelRoot, 4f, reverseArrangement: false);
            LayoutElement layoutElement = panelRoot.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = PanelWidth;
            GameUiInput.RegisterHudPanel(panelRoot);

            headerText = HudUiFactory.CreateLabel(panelRoot, "Header", LineHeight, bold: true);
            primaryTrainButton = HudUiFactory.CreateButton(panelRoot, "PrimaryTrain", ButtonHeight);
            primaryTrainButton.onClick.AddListener(OnPrimaryTrainClicked);
            spearmanButton = HudUiFactory.CreateButton(panelRoot, "SpearmanTrain", ButtonHeight);
            spearmanButton.onClick.AddListener(OnSpearmanClicked);

            GameObject queueHost = new GameObject("QueueList");
            queueHost.transform.SetParent(panelRoot, false);
            queueListView = queueHost.AddComponent<ProductionQueueListView>();
            queueListView.Initialize(queueHost.transform, 24f);

            statusText = HudUiFactory.CreateLabel(panelRoot, "Status", LineHeight);
            trainingText = HudUiFactory.CreateLabel(panelRoot, "Training", LineHeight);
            progressSlider = HudUiFactory.CreateSlider(panelRoot, "Progress", 18f);
            panelRoot.gameObject.SetActive(false);
            uiBuilt = true;
        }

        void OnPrimaryTrainClicked()
        {
            Barracks barracks = selectionManager != null ? selectionManager.SelectedBarracks : null;
            if (barracks != null)
                CommandQueue.Enqueue(new TrainMilitiaCommand(barracks));
        }

        void OnSpearmanClicked()
        {
            Barracks barracks = selectionManager != null ? selectionManager.SelectedBarracks : null;
            if (barracks != null)
                CommandQueue.Enqueue(new TrainSpearmanCommand(barracks));
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

        void LateUpdate()
        {
            TryBuildUi();
            if (!uiBuilt || selectionManager == null)
                return;

            Barracks barracks = selectionManager.SelectedBarracks;
            bool visible = barracks != null && barracks.Data != null;
            panelRoot.gameObject.SetActive(visible);
            if (!visible)
                return;

            PlacedBuildingData data = barracks.Data;
            HudUiFactory.SetText(headerText, Localization.BuildingName(PlacedBuildingKind.Barracks));

            int queueCount = BarracksProductionManager.GetQueueCount(barracks);
            bool queueFull = queueCount >= BarracksProductionManager.MaxQueueSize;
            bool populationFull = !PopulationManager.CanTrainUnit();
            bool canAffordMilitiaFood = ResourceManager.Food >= data.ScaledTrainFoodCost;
            bool canAffordMilitiaWood = data.ScaledTrainWoodCost <= 0f || ResourceManager.Wood >= data.ScaledTrainWoodCost;
            bool canAffordMilitia = canAffordMilitiaFood && canAffordMilitiaWood;
            bool canAffordSpearmanWood = ResourceManager.Wood >= data.ScaledSecondaryTrainWoodCost;
            bool canAffordSpearmanFood = ResourceManager.Food >= data.ScaledSecondaryTrainFoodCost;
            bool canAffordSpearman = canAffordSpearmanWood && canAffordSpearmanFood;

            string primaryTrainName = Localization.UnitName(BarracksTraining.ResolvePrimaryTrainUnit(barracks));
            primaryTrainButton.interactable = !queueFull && !populationFull && canAffordMilitia && !GameSessionManager.IsGameOver;
            HudUiFactory.SetButtonLabel(primaryTrainButton, BuildPrimaryTrainLabel(primaryTrainName, data));
            spearmanButton.interactable = !queueFull && !populationFull && canAffordSpearman && !GameSessionManager.IsGameOver;
            HudUiFactory.SetButtonLabel(spearmanButton, BuildSpearmanLabel(data));

            if (queueCount > 0)
            {
                BarracksProductionManager.GetQueueEntries(barracks, queueEntriesBuffer);
                queueListView.Refresh(queueEntriesBuffer, index => BarracksProductionManager.TryCancelQueueItem(barracks, index));
            }
            else
                queueListView.HideAll();

            string status = string.Empty;
            if (queueFull)
                status = Localization.Get("ui.queue_full");
            else if (populationFull)
                status = Localization.Get("ui.population_full");
            else if (!canAffordMilitia && !canAffordSpearman)
                status = Localization.Get("ui.need_resources");
            else if (!canAffordSpearmanWood || !canAffordSpearmanFood)
                status = !canAffordSpearmanWood
                    ? Localization.Get("ui.need_wood_spearman")
                    : Localization.Get("ui.need_food_spearman");
            statusText.gameObject.SetActive(!string.IsNullOrEmpty(status));
            HudUiFactory.SetText(statusText, status);

            bool isProducing = queueCount > 0;
            trainingText.gameObject.SetActive(isProducing);
            progressSlider.gameObject.SetActive(isProducing);
            if (isProducing)
            {
                float total = BarracksProductionManager.GetTotalSeconds(barracks);
                float remaining = BarracksProductionManager.GetRemainingSeconds(barracks);
                progressSlider.value = total > 0f ? 1f - remaining / total : 0f;
                HudUiFactory.SetText(trainingText, Localization.Format("ui.training", remaining));
            }
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
    }
}

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
    public class StablePanelView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;

        const float PanelWidth = 220f;
        const float LineHeight = 20f;
        const float ButtonHeight = 28f;

        RectTransform panelRoot;
        Text headerText;
        Button cavalryButton;
        Button scoutButton;
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

            panelRoot = HudUiFactory.CreatePanel(stack, "StablePanel", HudUiFactory.PanelBackgroundColor);
            HudUiFactory.AddVerticalLayout(panelRoot, 4f, reverseArrangement: false);
            panelRoot.gameObject.AddComponent<LayoutElement>().preferredWidth = PanelWidth;
            GameUiInput.RegisterHudPanel(panelRoot);

            headerText = HudUiFactory.CreateLabel(panelRoot, "Header", LineHeight, bold: true);
            cavalryButton = HudUiFactory.CreateButton(panelRoot, "TrainCavalry", ButtonHeight);
            cavalryButton.onClick.AddListener(OnCavalryClicked);
            scoutButton = HudUiFactory.CreateButton(panelRoot, "TrainScout", ButtonHeight);
            scoutButton.onClick.AddListener(OnScoutClicked);

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

        void OnCavalryClicked()
        {
            Stable stable = selectionManager != null ? selectionManager.SelectedStable : null;
            if (stable != null)
                CommandQueue.Enqueue(new TrainCavalryCommand(stable));
        }

        void OnScoutClicked()
        {
            Stable stable = selectionManager != null ? selectionManager.SelectedStable : null;
            if (stable != null)
                CommandQueue.Enqueue(new TrainScoutCommand(stable));
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

        void LateUpdate()
        {
            TryBuildUi();
            if (!uiBuilt || selectionManager == null)
                return;

            Stable stable = selectionManager.SelectedStable;
            bool visible = stable != null && stable.Data != null;
            panelRoot.gameObject.SetActive(visible);
            if (!visible)
                return;

            PlacedBuildingData data = stable.Data;
            HudUiFactory.SetText(headerText, Localization.BuildingName(PlacedBuildingKind.Stable));

            int queueCount = StableProductionManager.GetQueueCount(stable);
            bool queueFull = queueCount >= StableProductionManager.MaxQueueSize;
            bool populationFull = !PopulationManager.CanTrainUnit();
            bool canAffordCavalry = ResourceManager.Wood >= data.ScaledTrainWoodCost
                && ResourceManager.Food >= data.ScaledTrainFoodCost;
            bool canAffordScoutWood = data.ScaledSecondaryTrainWoodCost <= 0f
                || ResourceManager.Wood >= data.ScaledSecondaryTrainWoodCost;
            bool canAffordScoutFood = ResourceManager.Food >= data.ScaledSecondaryTrainFoodCost;
            bool canAffordScout = canAffordScoutWood && canAffordScoutFood;

            cavalryButton.interactable = !queueFull && !populationFull && canAffordCavalry && !GameSessionManager.IsGameOver;
            HudUiFactory.SetButtonLabel(
                cavalryButton,
                Localization.Format(
                    "ui.create_unit_dual",
                    Localization.Get("unit.cavalry"),
                    Mathf.CeilToInt(data.ScaledTrainWoodCost),
                    Mathf.CeilToInt(data.ScaledTrainFoodCost)));
            scoutButton.interactable = !queueFull && !populationFull && canAffordScout && !GameSessionManager.IsGameOver;
            HudUiFactory.SetButtonLabel(scoutButton, BuildScoutLabel(data));

            if (queueCount > 0)
            {
                StableProductionManager.GetQueueEntries(stable, queueEntriesBuffer);
                queueListView.Refresh(queueEntriesBuffer, index => StableProductionManager.TryCancelQueueItem(stable, index));
            }
            else
                queueListView.HideAll();

            string status = string.Empty;
            if (queueFull)
                status = Localization.Get("ui.queue_full");
            else if (populationFull)
                status = Localization.Get("ui.population_full");
            else if (!canAffordCavalry && !canAffordScout)
                status = Localization.Get("ui.need_resources");
            statusText.gameObject.SetActive(!string.IsNullOrEmpty(status));
            HudUiFactory.SetText(statusText, status);

            bool isProducing = queueCount > 0;
            trainingText.gameObject.SetActive(isProducing);
            progressSlider.gameObject.SetActive(isProducing);
            if (isProducing)
            {
                float total = StableProductionManager.GetTotalSeconds(stable);
                float remaining = StableProductionManager.GetRemainingSeconds(stable);
                progressSlider.value = total > 0f ? 1f - remaining / total : 0f;
                HudUiFactory.SetText(trainingText, Localization.Format("ui.training", remaining));
            }
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

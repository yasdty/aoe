using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class ResourceHudView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] PlacedBuildingData houseData;
        [SerializeField] PlacedBuildingData barracksData;
        [SerializeField] PlacedBuildingData archeryRangeData;
        [SerializeField] PlacedBuildingData stableData;
        [SerializeField] PlacedBuildingData farmData;
        [SerializeField] PlacedBuildingData lumberCampData;
        [SerializeField] PlacedBuildingData miningCampData;
        [SerializeField] PlacedBuildingData millData;

        const float Margin = 12f;
        const float PanelWidth = 210f;
        const float WoodLineHeight = 24f;
        const float FoodLineHeight = 24f;
        const float GoldLineHeight = 24f;
        const float StoneLineHeight = 24f;
        const float PopLineHeight = 24f;
        const float ButtonHeight = 28f;
        const float ButtonGap = 4f;
        const float Padding = 8f;

        static ResourceHudView instance;

        void Awake()
        {
            instance = this;
            houseData = PlacedBuildingDataResolver.ResolveHouse(ref houseData);
            barracksData = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
            archeryRangeData = PlacedBuildingDataResolver.ResolveArcheryRange(ref archeryRangeData);
            stableData = PlacedBuildingDataResolver.ResolveStable(ref stableData);
            farmData = PlacedBuildingDataResolver.ResolveFarm(ref farmData);
            lumberCampData = PlacedBuildingDataResolver.ResolveLumberCamp(ref lumberCampData);
            miningCampData = PlacedBuildingDataResolver.ResolveMiningCamp(ref miningCampData);
            millData = PlacedBuildingDataResolver.ResolveMill(ref millData);
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static bool IsPointerOverHud(Vector2 screenPosition)
        {
            return GameUiInput.IsPointerOverHud(screenPosition);
        }

        void OnGUI()
        {
            PlacedBuildingData house = PlacedBuildingDataResolver.ResolveHouse(ref houseData);
            PlacedBuildingData barracks = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
            PlacedBuildingData archeryRange = PlacedBuildingDataResolver.ResolveArcheryRange(ref archeryRangeData);
            PlacedBuildingData stable = PlacedBuildingDataResolver.ResolveStable(ref stableData);
            PlacedBuildingData farm = PlacedBuildingDataResolver.ResolveFarm(ref farmData);
            PlacedBuildingData lumberCamp = PlacedBuildingDataResolver.ResolveLumberCamp(ref lumberCampData);
            PlacedBuildingData miningCamp = PlacedBuildingDataResolver.ResolveMiningCamp(ref miningCampData);
            PlacedBuildingData mill = PlacedBuildingDataResolver.ResolveMill(ref millData);
            float panelHeight = Padding * 2f + WoodLineHeight + ButtonGap + FoodLineHeight + ButtonGap
                + GoldLineHeight + ButtonGap + StoneLineHeight + ButtonGap + PopLineHeight + ButtonGap
                + ButtonHeight + ButtonGap + ButtonHeight + ButtonGap + ButtonHeight + ButtonGap + ButtonHeight + ButtonGap + ButtonHeight + ButtonGap + ButtonHeight + ButtonGap + ButtonHeight + ButtonGap + ButtonHeight;
            Rect panelRect = new Rect(Margin, Margin, PanelWidth, panelHeight);
            GameUiInput.SetHudPanelScreenRect(GameUiInput.GuiRectToScreenRect(panelRect));

            GUI.Box(panelRect, GUIContent.none);

            float y = Margin + Padding;

            Rect woodRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, WoodLineHeight);
            GUI.Label(woodRect, $"Wood: {Mathf.FloorToInt(ResourceManager.Wood)}");
            y += WoodLineHeight + ButtonGap;

            Rect foodRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, FoodLineHeight);
            GUI.Label(foodRect, $"Food: {Mathf.FloorToInt(ResourceManager.Food)}");
            y += FoodLineHeight + ButtonGap;

            Rect goldRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, GoldLineHeight);
            GUI.Label(goldRect, $"Gold: {Mathf.FloorToInt(ResourceManager.Gold)}");
            y += GoldLineHeight + ButtonGap;

            Rect stoneRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, StoneLineHeight);
            GUI.Label(stoneRect, $"Stone: {Mathf.FloorToInt(ResourceManager.Stone)}");
            y += StoneLineHeight + ButtonGap;

            Rect popRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, PopLineHeight);
            GUI.Label(popRect, $"Pop: {PopulationManager.CurrentPopulation}/{PopulationManager.MaxPopulation}");
            y += PopLineHeight + ButtonGap;

            bool inPlacementMode = BuildingPlacementManager.IsPlacementModeActive;
            bool gameOver = GameSessionManager.IsGameOver;

            Rect houseButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canAffordHouse = ResourceManager.Wood >= house.woodCost;
            GUI.enabled = canAffordHouse && !inPlacementMode && !gameOver;
            if (GUI.Button(houseButtonRect, $"Build House ({house.woodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterHousePlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect barracksButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canAffordBarracks = ResourceManager.Wood >= barracks.woodCost;
            GUI.enabled = canAffordBarracks && !inPlacementMode && !gameOver;
            if (GUI.Button(barracksButtonRect, $"Build Barracks ({barracks.woodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterBarracksPlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect archeryRangeButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canAffordArcheryRange = ResourceManager.Wood >= archeryRange.woodCost;
            GUI.enabled = canAffordArcheryRange && !inPlacementMode && !gameOver;
            if (GUI.Button(archeryRangeButtonRect, $"Build Archery Range ({archeryRange.woodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterArcheryRangePlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect stableButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canAffordStable = ResourceManager.Wood >= stable.woodCost;
            GUI.enabled = canAffordStable && !inPlacementMode && !gameOver;
            if (GUI.Button(stableButtonRect, $"Build Stable ({stable.woodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterStablePlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect farmButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canAffordFarm = ResourceManager.Wood >= farm.woodCost;
            GUI.enabled = canAffordFarm && !inPlacementMode && !gameOver;
            if (GUI.Button(farmButtonRect, $"Build Farm ({farm.woodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterFarmPlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect lumberCampButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canAffordLumberCamp = ResourceManager.Wood >= lumberCamp.woodCost;
            GUI.enabled = canAffordLumberCamp && !inPlacementMode && !gameOver;
            if (GUI.Button(lumberCampButtonRect, $"Build Lumber Camp ({lumberCamp.woodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterLumberCampPlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect miningCampButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canAffordMiningCamp = ResourceManager.Wood >= miningCamp.woodCost;
            GUI.enabled = canAffordMiningCamp && !inPlacementMode && !gameOver;
            if (GUI.Button(miningCampButtonRect, $"Build Mining Camp ({miningCamp.woodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterMiningCampPlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect millButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canAffordMill = ResourceManager.Wood >= mill.woodCost;
            GUI.enabled = canAffordMill && !inPlacementMode && !gameOver;
            if (GUI.Button(millButtonRect, $"Build Mill ({mill.woodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterMillPlacementMode(builders);
            }
            GUI.enabled = true;

            if (inPlacementMode)
            {
                Rect hintRect = new Rect(Margin, panelRect.yMax + 4f, PanelWidth + 60f, 36f);
                GameUiInput.SetHudHintScreenRect(GameUiInput.GuiRectToScreenRect(hintRect));
                GUI.Label(hintRect, "Click ground to place. Esc / Right-click to cancel.");
            }
            else
            {
                GameUiInput.ClearHudHintScreenRect();
                if (!canAffordHouse && !canAffordBarracks && !canAffordArcheryRange && !canAffordStable && !canAffordFarm && !canAffordLumberCamp && !canAffordMiningCamp && !canAffordMill)
                {
                    Rect hintRect = new Rect(Margin + Padding, panelRect.yMax + 4f, PanelWidth, 20f);
                    GUI.Label(hintRect, "Need more Wood.");
                }
            }
        }
    }
}

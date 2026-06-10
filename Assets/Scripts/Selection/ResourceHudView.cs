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
            int houseWoodCost = Mathf.CeilToInt(house.ScaledWoodCost);
            bool canAffordHouse = ResourceManager.Wood >= house.ScaledWoodCost;
            GUI.enabled = canAffordHouse && !inPlacementMode && !gameOver;
            if (GUI.Button(houseButtonRect, $"Build House ({houseWoodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterHousePlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect barracksButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            int barracksWoodCost = Mathf.CeilToInt(barracks.ScaledWoodCost);
            bool canAffordBarracks = ResourceManager.Wood >= barracks.ScaledWoodCost;
            GUI.enabled = canAffordBarracks && !inPlacementMode && !gameOver;
            if (GUI.Button(barracksButtonRect, $"Build Barracks ({barracksWoodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterBarracksPlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect archeryRangeButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            int archeryWoodCost = Mathf.CeilToInt(archeryRange.ScaledWoodCost);
            bool canBuildArcheryRange = GameSessionManager.CanBuild(archeryRange, UnitTeam.Player);
            bool canAffordArcheryRange = ResourceManager.Wood >= archeryRange.ScaledWoodCost;
            GUI.enabled = canBuildArcheryRange && canAffordArcheryRange && !inPlacementMode && !gameOver;
            if (GUI.Button(
                    archeryRangeButtonRect,
                    canBuildArcheryRange
                        ? $"Build Archery Range ({archeryWoodCost} Wood)"
                        : "Archery Range (Feudal Age)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterArcheryRangePlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect stableButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            int stableWoodCost = Mathf.CeilToInt(stable.ScaledWoodCost);
            bool canBuildStable = GameSessionManager.CanBuild(stable, UnitTeam.Player);
            bool canAffordStable = ResourceManager.Wood >= stable.ScaledWoodCost;
            GUI.enabled = canBuildStable && canAffordStable && !inPlacementMode && !gameOver;
            if (GUI.Button(
                    stableButtonRect,
                    canBuildStable
                        ? $"Build Stable ({stableWoodCost} Wood)"
                        : "Stable (Feudal Age)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterStablePlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect farmButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            int farmWoodCost = Mathf.CeilToInt(farm.ScaledWoodCost);
            bool canAffordFarm = ResourceManager.Wood >= farm.ScaledWoodCost;
            GUI.enabled = canAffordFarm && !inPlacementMode && !gameOver;
            if (GUI.Button(farmButtonRect, $"Build Farm ({farmWoodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterFarmPlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect lumberCampButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            int lumberCampWoodCost = Mathf.CeilToInt(lumberCamp.ScaledWoodCost);
            bool canAffordLumberCamp = ResourceManager.Wood >= lumberCamp.ScaledWoodCost;
            GUI.enabled = canAffordLumberCamp && !inPlacementMode && !gameOver;
            if (GUI.Button(lumberCampButtonRect, $"Build Lumber Camp ({lumberCampWoodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterLumberCampPlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect miningCampButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            int miningCampWoodCost = Mathf.CeilToInt(miningCamp.ScaledWoodCost);
            bool canAffordMiningCamp = ResourceManager.Wood >= miningCamp.ScaledWoodCost;
            GUI.enabled = canAffordMiningCamp && !inPlacementMode && !gameOver;
            if (GUI.Button(miningCampButtonRect, $"Build Mining Camp ({miningCampWoodCost} Wood)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterMiningCampPlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect millButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            int millWoodCost = Mathf.CeilToInt(mill.ScaledWoodCost);
            bool canAffordMill = ResourceManager.Wood >= mill.ScaledWoodCost;
            GUI.enabled = canAffordMill && !inPlacementMode && !gameOver;
            if (GUI.Button(millButtonRect, $"Build Mill ({millWoodCost} Wood)"))
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

using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class ResourceHudView : MonoBehaviour
    {
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] RTSInputReader input;
        [SerializeField] PlacedBuildingData houseData;
        [SerializeField] PlacedBuildingData barracksData;
        [SerializeField] PlacedBuildingData archeryRangeData;
        [SerializeField] PlacedBuildingData stableData;
        [SerializeField] PlacedBuildingData blacksmithData;
        [SerializeField] PlacedBuildingData farmData;
        [SerializeField] PlacedBuildingData lumberCampData;
        [SerializeField] PlacedBuildingData miningCampData;
        [SerializeField] PlacedBuildingData millData;
        [SerializeField] PlacedBuildingData palisadeWallData;
        [SerializeField] PlacedBuildingData stoneWallData;
        [SerializeField] PlacedBuildingData gateData;
        [SerializeField] PlacedBuildingData watchTowerData;
        [SerializeField] PlacedBuildingData marketData;
        [SerializeField] PlacedBuildingData townCenterPlacementData;

        const float Margin = 12f;
        const float PanelWidth = 210f;
        const float WoodLineHeight = 24f;
        const float FoodLineHeight = 24f;
        const float GoldLineHeight = 24f;
        const float StoneLineHeight = 24f;
        const float PopLineHeight = 24f;
        const float CivLineHeight = 20f;
        const float ButtonHeight = 28f;
        const float ButtonGap = 4f;
        const float Padding = 8f;
        const int BuildButtonCount = 15;

        static ResourceHudView instance;

        static float ResourceStripHeight =>
            Padding * 2f
            + WoodLineHeight + ButtonGap
            + FoodLineHeight + ButtonGap
            + GoldLineHeight + ButtonGap
            + StoneLineHeight + ButtonGap
            + PopLineHeight + ButtonGap
            + CivLineHeight + ButtonGap;

        static float BuildMenuHeight =>
            BuildButtonCount * ButtonHeight + (BuildButtonCount - 1) * ButtonGap;

        void Awake()
        {
            instance = this;
            houseData = PlacedBuildingDataResolver.ResolveHouse(ref houseData);
            barracksData = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
            archeryRangeData = PlacedBuildingDataResolver.ResolveArcheryRange(ref archeryRangeData);
            stableData = PlacedBuildingDataResolver.ResolveStable(ref stableData);
            blacksmithData = PlacedBuildingDataResolver.ResolveBlacksmith(ref blacksmithData);
            farmData = PlacedBuildingDataResolver.ResolveFarm(ref farmData);
            lumberCampData = PlacedBuildingDataResolver.ResolveLumberCamp(ref lumberCampData);
            miningCampData = PlacedBuildingDataResolver.ResolveMiningCamp(ref miningCampData);
            millData = PlacedBuildingDataResolver.ResolveMill(ref millData);
            palisadeWallData = PlacedBuildingDataResolver.ResolvePalisadeWall(ref palisadeWallData);
            stoneWallData = PlacedBuildingDataResolver.ResolveStoneWall(ref stoneWallData);
            gateData = PlacedBuildingDataResolver.ResolveGate(ref gateData);
            watchTowerData = PlacedBuildingDataResolver.ResolveWatchTower(ref watchTowerData);
            marketData = PlacedBuildingDataResolver.ResolveMarket(ref marketData);
            townCenterPlacementData = PlacedBuildingDataResolver.ResolveTownCenterPlacement(ref townCenterPlacementData);
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();
            if (input == null)
                input = FindAnyObjectByType<RTSInputReader>();
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver || input == null || selectionManager == null)
                return;

            if (BuildingPlacementManager.IsPlacementModeActive)
                return;

            if (!selectionManager.HasSelectedPlayerVillagers())
                return;

            PlacedBuildingData house = PlacedBuildingDataResolver.ResolveHouse(ref houseData);
            PlacedBuildingData barracks = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);

            if (input.WasBuildHousePressedThisFrame()
                && house != null
                && ResourceManager.Wood >= house.ScaledWoodCost)
            {
                BuildingPlacementManager.EnterHousePlacementMode(selectionManager.SelectedUnits);
                return;
            }

            if (input.WasBuildBarracksPressedThisFrame()
                && barracks != null
                && ResourceManager.Wood >= barracks.ScaledWoodCost)
            {
                BuildingPlacementManager.EnterBarracksPlacementMode(selectionManager.SelectedUnits);
            }
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
            GameUiInput.BeginHudLayoutFrame();

            PlacedBuildingData house = PlacedBuildingDataResolver.ResolveHouse(ref houseData);
            PlacedBuildingData barracks = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
            PlacedBuildingData archeryRange = PlacedBuildingDataResolver.ResolveArcheryRange(ref archeryRangeData);
            PlacedBuildingData stable = PlacedBuildingDataResolver.ResolveStable(ref stableData);
            PlacedBuildingData blacksmith = PlacedBuildingDataResolver.ResolveBlacksmith(ref blacksmithData);
            PlacedBuildingData farm = PlacedBuildingDataResolver.ResolveFarm(ref farmData);
            PlacedBuildingData lumberCamp = PlacedBuildingDataResolver.ResolveLumberCamp(ref lumberCampData);
            PlacedBuildingData miningCamp = PlacedBuildingDataResolver.ResolveMiningCamp(ref miningCampData);
            PlacedBuildingData mill = PlacedBuildingDataResolver.ResolveMill(ref millData);
            PlacedBuildingData palisadeWall = PlacedBuildingDataResolver.ResolvePalisadeWall(ref palisadeWallData);
            PlacedBuildingData stoneWall = PlacedBuildingDataResolver.ResolveStoneWall(ref stoneWallData);
            PlacedBuildingData gate = PlacedBuildingDataResolver.ResolveGate(ref gateData);
            PlacedBuildingData watchTower = PlacedBuildingDataResolver.ResolveWatchTower(ref watchTowerData);
            PlacedBuildingData market = PlacedBuildingDataResolver.ResolveMarket(ref marketData);
            PlacedBuildingData townCenterPlacement = PlacedBuildingDataResolver.ResolveTownCenterPlacement(
                ref townCenterPlacementData);
            bool inPlacementMode = BuildingPlacementManager.IsPlacementModeActive;
            bool showBuildMenu = !inPlacementMode
                && selectionManager != null
                && selectionManager.HasSelectedPlayerVillagers();
            float panelHeight = ResourceStripHeight + (showBuildMenu ? BuildMenuHeight : 0f);
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

            string civLabel = CivilizationBonusUtility.GetHudLabel(UnitTeam.Player);
            if (!string.IsNullOrEmpty(civLabel))
            {
                Rect civRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, CivLineHeight);
                GUI.Label(civRect, civLabel);
                y += CivLineHeight + ButtonGap;
            }

            if (!showBuildMenu)
            {
                DrawPlacementHints(panelRect, inPlacementMode, canAffordAnyBuild: false);
                return;
            }

            bool gameOver = GameSessionManager.IsGameOver;

            Rect houseButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            int houseWoodCost = Mathf.CeilToInt(house.ScaledWoodCost);
            bool canAffordHouse = ResourceManager.Wood >= house.ScaledWoodCost;
            GUI.enabled = canAffordHouse && !inPlacementMode && !gameOver;
            if (GUI.Button(houseButtonRect, $"Build House (H) ({houseWoodCost} Wood)"))
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
            if (GUI.Button(barracksButtonRect, $"Build Barracks (B) ({barracksWoodCost} Wood)"))
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

            Rect blacksmithButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            int blacksmithWoodCost = Mathf.CeilToInt(blacksmith.ScaledWoodCost);
            bool canBuildBlacksmith = GameSessionManager.CanBuild(blacksmith, UnitTeam.Player);
            bool canAffordBlacksmith = ResourceManager.Wood >= blacksmith.ScaledWoodCost;
            GUI.enabled = canBuildBlacksmith && canAffordBlacksmith && !inPlacementMode && !gameOver;
            if (GUI.Button(
                    blacksmithButtonRect,
                    canBuildBlacksmith
                        ? $"Build Blacksmith ({blacksmithWoodCost} Wood)"
                        : "Blacksmith (Feudal Age)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterBlacksmithPlacementMode(builders);
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
            y += ButtonHeight + ButtonGap;

            Rect palisadeButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canAffordPalisade = PlacementCostUtility.CanAfford(UnitTeam.Player, palisadeWall);
            GUI.enabled = canAffordPalisade && !inPlacementMode && !gameOver;
            if (GUI.Button(palisadeButtonRect, $"Build Palisade ({FormatPlacementCost(palisadeWall)})"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterPalisadeWallPlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect stoneWallButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canBuildStoneWall = GameSessionManager.CanBuild(stoneWall, UnitTeam.Player);
            bool canAffordStoneWall = PlacementCostUtility.CanAfford(UnitTeam.Player, stoneWall);
            GUI.enabled = canBuildStoneWall && canAffordStoneWall && !inPlacementMode && !gameOver;
            if (GUI.Button(
                    stoneWallButtonRect,
                    canBuildStoneWall
                        ? $"Build Stone Wall ({FormatPlacementCost(stoneWall)})"
                        : "Stone Wall (Feudal Age)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterStoneWallPlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect gateButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canBuildGate = GameSessionManager.CanBuild(gate, UnitTeam.Player);
            bool canAffordGate = PlacementCostUtility.CanAfford(UnitTeam.Player, gate);
            GUI.enabled = canBuildGate && canAffordGate && !inPlacementMode && !gameOver;
            if (GUI.Button(
                    gateButtonRect,
                    canBuildGate
                        ? $"Build Gate ({FormatPlacementCost(gate)})"
                        : "Gate (Feudal Age + wall)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterGatePlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect watchTowerButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            bool canBuildWatchTower = GameSessionManager.CanBuild(watchTower, UnitTeam.Player);
            bool canAffordWatchTower = PlacementCostUtility.CanAfford(UnitTeam.Player, watchTower);
            GUI.enabled = canBuildWatchTower && canAffordWatchTower && !inPlacementMode && !gameOver;
            if (GUI.Button(
                    watchTowerButtonRect,
                    canBuildWatchTower
                        ? $"Build Watch Tower ({FormatPlacementCost(watchTower)})"
                        : "Watch Tower (Feudal Age)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterWatchTowerPlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            Rect marketButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            int marketWoodCost = Mathf.CeilToInt(market.ScaledWoodCost);
            bool canBuildMarket = GameSessionManager.CanBuild(market, UnitTeam.Player);
            bool canAffordMarket = ResourceManager.Wood >= market.ScaledWoodCost;
            GUI.enabled = canBuildMarket && canAffordMarket && !inPlacementMode && !gameOver;
            if (GUI.Button(
                    marketButtonRect,
                    canBuildMarket
                        ? $"Build Market ({marketWoodCost} Wood)"
                        : "Market (Feudal Age)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterMarketPlacementMode(builders);
            }
            y += ButtonHeight + ButtonGap;

            bool feudalUnlocked = GameSessionManager.CanBuild(townCenterPlacement, UnitTeam.Player);
            bool canPlaceTownCenter = feudalUnlocked
                && BuildingPlacementManager.CanPlaceAdditionalTownCenter(UnitTeam.Player);
            bool canAffordTownCenter = PlacementCostUtility.CanAfford(UnitTeam.Player, townCenterPlacement);
            Rect townCenterButtonRect = new Rect(Margin + Padding, y, PanelWidth - Padding * 2f, ButtonHeight);
            GUI.enabled = canPlaceTownCenter && canAffordTownCenter && !inPlacementMode && !gameOver;
            if (GUI.Button(
                    townCenterButtonRect,
                    canPlaceTownCenter
                        ? $"Build Town Center ({FormatPlacementCost(townCenterPlacement)})"
                        : feudalUnlocked
                            ? "Town Center (Max 2)"
                            : "Town Center (Feudal Age)"))
            {
                IReadOnlyList<Unit> builders = selectionManager != null
                    ? selectionManager.SelectedUnits
                    : null;
                BuildingPlacementManager.EnterTownCenterPlacementMode(builders);
            }
            GUI.enabled = true;

            bool canAffordAnyBuild = canAffordHouse || canAffordBarracks || canAffordArcheryRange || canAffordStable
                || canAffordBlacksmith || canAffordFarm || canAffordLumberCamp || canAffordMiningCamp || canAffordMill
                || canAffordPalisade || canAffordStoneWall || canAffordGate || canAffordWatchTower || canAffordMarket
                || canAffordTownCenter;
            DrawPlacementHints(panelRect, inPlacementMode, canAffordAnyBuild);
        }

        void DrawPlacementHints(Rect panelRect, bool inPlacementMode, bool canAffordAnyBuild)
        {
            if (inPlacementMode)
            {
                Rect hintRect = new Rect(Margin, panelRect.yMax + 4f, PanelWidth + 60f, 36f);
                GameUiInput.SetHudHintScreenRect(GameUiInput.GuiRectToScreenRect(hintRect));
                GUI.Label(hintRect, "Click to place. Walls: drag a line. Esc / Right-click to cancel.");
                return;
            }

            GameUiInput.ClearHudHintScreenRect();
            if (!canAffordAnyBuild)
            {
                Rect hintRect = new Rect(Margin + Padding, panelRect.yMax + 4f, PanelWidth, 20f);
                GUI.Label(hintRect, "Need more Wood.");
            }
        }

        static string FormatPlacementCost(PlacedBuildingData data)
        {
            if (data == null)
                return "0";

            int wood = Mathf.CeilToInt(data.ScaledWoodCost);
            int stone = Mathf.CeilToInt(data.ScaledStoneCost);
            if (wood > 0 && stone > 0)
                return $"{wood} Wood, {stone} Stone";

            if (stone > 0)
                return $"{stone} Stone";

            return $"{wood} Wood";
        }
    }
}

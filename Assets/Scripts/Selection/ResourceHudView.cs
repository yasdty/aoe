using System;
using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using AoE.RTS.Units;
using UnityEngine;
using UnityEngine.UI;

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

        const float PanelWidth = 210f;
        const float LineHeight = 24f;
        const float CivLineHeight = 20f;
        const float ButtonHeight = 28f;
        const float Padding = 8f;
        const float ButtonGap = 4f;

        static ResourceHudView instance;

        RectTransform panelRoot;
        RectTransform hintRoot;
        Text woodText;
        Text foodText;
        Text goldText;
        Text stoneText;
        Text popText;
        Text civText;
        Text hintText;
        RectTransform buildMenuRoot;
        readonly List<BuildButtonEntry> buildButtons = new List<BuildButtonEntry>();
        bool uiBuilt;

        struct BuildButtonEntry
        {
            public Button button;
            public Func<string> getLabel;
            public Func<bool> getInteractable;
            public Action onClick;
        }

        void Awake()
        {
            instance = this;
            ResolveBuildingData();
            if (selectionManager == null)
                selectionManager = FindAnyObjectByType<SelectionManager>();
            if (input == null)
                input = FindAnyObjectByType<RTSInputReader>();
            TryBuildUi();
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
            if (panelRoot != null)
                GameUiInput.UnregisterHudPanel(panelRoot);
            if (hintRoot != null)
                GameUiInput.UnregisterHudPanel(hintRoot);
        }

        void ResolveBuildingData()
        {
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
        }

        void TryBuildUi()
        {
            if (uiBuilt)
                return;

            Transform hudRoot = HudUiFactory.GetHudRoot();
            if (hudRoot == null)
                return;

            panelRoot = HudUiFactory.SetupScreenPanel(
                hudRoot,
                "ResourceHudPanel",
                HudUiFactory.PanelBackgroundColor,
                HudUiFactory.Margin,
                HudUiFactory.Margin,
                PanelWidth,
                800f,
                topLeftAnchor: true);
            GameUiInput.RegisterHudPanel(panelRoot);
            HudUiFactory.AddVerticalLayout(panelRoot, ButtonGap, reverseArrangement: false);

            woodText = HudUiFactory.CreateLabel(panelRoot, "Wood", LineHeight);
            foodText = HudUiFactory.CreateLabel(panelRoot, "Food", LineHeight);
            goldText = HudUiFactory.CreateLabel(panelRoot, "Gold", LineHeight);
            stoneText = HudUiFactory.CreateLabel(panelRoot, "Stone", LineHeight);
            popText = HudUiFactory.CreateLabel(panelRoot, "Pop", LineHeight);
            civText = HudUiFactory.CreateLabel(panelRoot, "Civ", CivLineHeight);

            GameObject buildMenuObject = new GameObject("BuildMenu", typeof(RectTransform));
            buildMenuObject.transform.SetParent(panelRoot, false);
            buildMenuRoot = buildMenuObject.GetComponent<RectTransform>();
            HudUiFactory.AddVerticalLayout(buildMenuRoot, ButtonGap, reverseArrangement: false);

            RegisterBuildButtons();

            hintRoot = HudUiFactory.SetupScreenPanel(
                hudRoot,
                "PlacementHintPanel",
                Color.clear,
                HudUiFactory.Margin,
                HudUiFactory.Margin + 200f,
                PanelWidth + 60f,
                36f,
                topLeftAnchor: true);
            hintRoot.GetComponent<Image>().raycastTarget = false;
            GameUiInput.RegisterHudPanel(hintRoot);
            hintText = HudUiFactory.CreateLabel(hintRoot, "HintText", 36f);
            HudUiFactory.SetStretchFull(hintText.rectTransform);
            hintText.alignment = TextAnchor.UpperLeft;
            hintRoot.gameObject.SetActive(false);

            uiBuilt = true;
        }

        void RegisterBuildButtons()
        {
            buildButtons.Clear();
            AddBuildButton(
                () => Localization.Format(
                    "ui.build_hotkey_wood",
                    Localization.BuildingName(PlacedBuildingKind.House),
                    "H",
                    Mathf.CeilToInt(houseData.ScaledWoodCost),
                    Localization.Get("resource.wood")),
                () => CanUseBuildButton(ResourceManager.Wood >= houseData.ScaledWoodCost),
                () => BuildingPlacementManager.EnterHousePlacementMode(GetBuilders()));

            AddBuildButton(
                () => Localization.Format(
                    "ui.build_hotkey_wood",
                    Localization.BuildingName(PlacedBuildingKind.Barracks),
                    "B",
                    Mathf.CeilToInt(barracksData.ScaledWoodCost),
                    Localization.Get("resource.wood")),
                () => CanUseBuildButton(ResourceManager.Wood >= barracksData.ScaledWoodCost),
                () => BuildingPlacementManager.EnterBarracksPlacementMode(GetBuilders()));

            AddBuildButton(
                () => BuildAgeLockedLabel(archeryRangeData, PlacedBuildingKind.ArcheryRange, "age.feudal"),
                () => CanUseBuildButton(
                    GameSessionManager.CanBuild(archeryRangeData, UnitTeam.Player)
                    && ResourceManager.Wood >= archeryRangeData.ScaledWoodCost),
                () => BuildingPlacementManager.EnterArcheryRangePlacementMode(GetBuilders()));

            AddBuildButton(
                () => BuildAgeLockedLabel(stableData, PlacedBuildingKind.Stable, "age.feudal"),
                () => CanUseBuildButton(
                    GameSessionManager.CanBuild(stableData, UnitTeam.Player)
                    && ResourceManager.Wood >= stableData.ScaledWoodCost),
                () => BuildingPlacementManager.EnterStablePlacementMode(GetBuilders()));

            AddBuildButton(
                () => BuildAgeLockedLabel(blacksmithData, PlacedBuildingKind.Blacksmith, "age.feudal"),
                () => CanUseBuildButton(
                    GameSessionManager.CanBuild(blacksmithData, UnitTeam.Player)
                    && ResourceManager.Wood >= blacksmithData.ScaledWoodCost),
                () => BuildingPlacementManager.EnterBlacksmithPlacementMode(GetBuilders()));

            AddBuildButton(
                () => Localization.Format(
                    "ui.build_wood",
                    Localization.BuildingName(PlacedBuildingKind.Farm),
                    Mathf.CeilToInt(farmData.ScaledWoodCost),
                    Localization.Get("resource.wood")),
                () => CanUseBuildButton(ResourceManager.Wood >= farmData.ScaledWoodCost),
                () => BuildingPlacementManager.EnterFarmPlacementMode(GetBuilders()));

            AddBuildButton(
                () => Localization.Format(
                    "ui.build_wood",
                    Localization.BuildingName(PlacedBuildingKind.LumberCamp),
                    Mathf.CeilToInt(lumberCampData.ScaledWoodCost),
                    Localization.Get("resource.wood")),
                () => CanUseBuildButton(ResourceManager.Wood >= lumberCampData.ScaledWoodCost),
                () => BuildingPlacementManager.EnterLumberCampPlacementMode(GetBuilders()));

            AddBuildButton(
                () => Localization.Format(
                    "ui.build_wood",
                    Localization.BuildingName(PlacedBuildingKind.MiningCamp),
                    Mathf.CeilToInt(miningCampData.ScaledWoodCost),
                    Localization.Get("resource.wood")),
                () => CanUseBuildButton(ResourceManager.Wood >= miningCampData.ScaledWoodCost),
                () => BuildingPlacementManager.EnterMiningCampPlacementMode(GetBuilders()));

            AddBuildButton(
                () => Localization.Format(
                    "ui.build_wood",
                    Localization.BuildingName(PlacedBuildingKind.Mill),
                    Mathf.CeilToInt(millData.ScaledWoodCost),
                    Localization.Get("resource.wood")),
                () => CanUseBuildButton(ResourceManager.Wood >= millData.ScaledWoodCost),
                () => BuildingPlacementManager.EnterMillPlacementMode(GetBuilders()));

            AddBuildButton(
                () => BuildWallLabel(palisadeWallData, PlacedBuildingKind.PalisadeWall, "age.dark"),
                () => CanUseBuildButton(
                    GameSessionManager.CanBuild(palisadeWallData, UnitTeam.Player)
                    && PlacementCostUtility.CanAfford(UnitTeam.Player, palisadeWallData)),
                () => BuildingPlacementManager.EnterPalisadeWallPlacementMode(GetBuilders()));

            AddBuildButton(
                () => BuildWallLabel(stoneWallData, PlacedBuildingKind.StoneWall, "age.feudal"),
                () => CanUseBuildButton(
                    GameSessionManager.CanBuild(stoneWallData, UnitTeam.Player)
                    && PlacementCostUtility.CanAfford(UnitTeam.Player, stoneWallData)),
                () => BuildingPlacementManager.EnterStoneWallPlacementMode(GetBuilders()));

            AddBuildButton(
                () => BuildGateLabel(),
                () => CanUseBuildButton(
                    GameSessionManager.CanBuild(gateData, UnitTeam.Player)
                    && PlacementCostUtility.CanAfford(UnitTeam.Player, gateData)),
                () => BuildingPlacementManager.EnterGatePlacementMode(GetBuilders()));

            AddBuildButton(
                () => BuildWallLabel(watchTowerData, PlacedBuildingKind.WatchTower, "age.feudal"),
                () => CanUseBuildButton(
                    GameSessionManager.CanBuild(watchTowerData, UnitTeam.Player)
                    && PlacementCostUtility.CanAfford(UnitTeam.Player, watchTowerData)),
                () => BuildingPlacementManager.EnterWatchTowerPlacementMode(GetBuilders()));

            AddBuildButton(
                () => BuildAgeLockedLabel(marketData, PlacedBuildingKind.Market, "age.feudal"),
                () => CanUseBuildButton(
                    GameSessionManager.CanBuild(marketData, UnitTeam.Player)
                    && ResourceManager.Wood >= marketData.ScaledWoodCost),
                () => BuildingPlacementManager.EnterMarketPlacementMode(GetBuilders()));

            AddBuildButton(
                () => BuildTownCenterLabel(),
                () => CanUseBuildButton(CanPlaceTownCenter() && PlacementCostUtility.CanAfford(UnitTeam.Player, townCenterPlacementData)),
                () => BuildingPlacementManager.EnterTownCenterPlacementMode(GetBuilders()));
        }

        void AddBuildButton(Func<string> getLabel, Func<bool> getInteractable, Action onClick)
        {
            Button button = HudUiFactory.CreateButton(buildMenuRoot, $"Build{buildButtons.Count}", ButtonHeight);
            button.onClick.AddListener(() => onClick());
            buildButtons.Add(new BuildButtonEntry
            {
                button = button,
                getLabel = getLabel,
                getInteractable = getInteractable,
                onClick = onClick
            });
        }

        IReadOnlyList<Unit> GetBuilders()
        {
            return selectionManager != null ? selectionManager.SelectedUnits : null;
        }

        bool CanUseBuildButton(bool canAfford)
        {
            return canAfford
                && !BuildingPlacementManager.IsPlacementModeActive
                && !GameSessionManager.IsGameOver;
        }

        bool CanPlaceTownCenter()
        {
            bool feudalUnlocked = GameSessionManager.CanBuild(townCenterPlacementData, UnitTeam.Player);
            return feudalUnlocked && BuildingPlacementManager.CanPlaceAdditionalTownCenter(UnitTeam.Player);
        }

        string BuildAgeLockedLabel(PlacedBuildingData data, PlacedBuildingKind kind, string ageKey)
        {
            if (GameSessionManager.CanBuild(data, UnitTeam.Player))
            {
                return Localization.Format(
                    "ui.build_wood",
                    Localization.BuildingName(kind),
                    Mathf.CeilToInt(data.ScaledWoodCost),
                    Localization.Get("resource.wood"));
            }

            return Localization.Format(
                "ui.locked_age",
                Localization.BuildingName(kind),
                Localization.Get(ageKey));
        }

        string BuildWallLabel(PlacedBuildingData data, PlacedBuildingKind kind, string ageKey)
        {
            if (GameSessionManager.CanBuild(data, UnitTeam.Player))
            {
                return Localization.Format(
                    "ui.build_cost",
                    Localization.BuildingName(kind),
                    Localization.FormatPlacementCost(data));
            }

            return Localization.Format(
                "ui.locked_age",
                Localization.BuildingName(kind),
                Localization.Get(ageKey));
        }

        string BuildGateLabel()
        {
            if (GameSessionManager.CanBuild(gateData, UnitTeam.Player))
            {
                return Localization.Format(
                    "ui.build_cost",
                    Localization.BuildingName(PlacedBuildingKind.Gate),
                    Localization.FormatPlacementCost(gateData));
            }

            return Localization.Format(
                "ui.locked_gate",
                Localization.BuildingName(PlacedBuildingKind.Gate),
                Localization.Get("age.feudal"));
        }

        string BuildTownCenterLabel()
        {
            if (CanPlaceTownCenter())
            {
                return Localization.Format(
                    "ui.build_cost",
                    Localization.BuildingName(PlacedBuildingKind.TownCenter),
                    Localization.FormatPlacementCost(townCenterPlacementData));
            }

            bool feudalUnlocked = GameSessionManager.CanBuild(townCenterPlacementData, UnitTeam.Player);
            if (feudalUnlocked)
            {
                return Localization.Format(
                    "ui.town_center_max",
                    Localization.BuildingName(PlacedBuildingKind.TownCenter));
            }

            return Localization.Format(
                "ui.locked_age",
                Localization.BuildingName(PlacedBuildingKind.TownCenter),
                Localization.Get("age.feudal"));
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver || input == null || selectionManager == null)
                return;

            if (BuildingPlacementManager.IsPlacementModeActive)
                return;

            if (!selectionManager.HasSelectedPlayerVillagers())
                return;

            if (input.WasBuildHousePressedThisFrame()
                && houseData != null
                && ResourceManager.Wood >= houseData.ScaledWoodCost)
            {
                BuildingPlacementManager.EnterHousePlacementMode(selectionManager.SelectedUnits);
                return;
            }

            if (input.WasBuildBarracksPressedThisFrame()
                && barracksData != null
                && ResourceManager.Wood >= barracksData.ScaledWoodCost)
                BuildingPlacementManager.EnterBarracksPlacementMode(selectionManager.SelectedUnits);
        }

        void LateUpdate()
        {
            TryBuildUi();
            if (!uiBuilt)
                return;

            HudUiFactory.SetText(
                woodText,
                Localization.Format("ui.resource_amount", Localization.Get("resource.wood"), Mathf.FloorToInt(ResourceManager.Wood)));
            HudUiFactory.SetText(
                foodText,
                Localization.Format("ui.resource_amount", Localization.Get("resource.food"), Mathf.FloorToInt(ResourceManager.Food)));
            HudUiFactory.SetText(
                goldText,
                Localization.Format("ui.resource_amount", Localization.Get("resource.gold"), Mathf.FloorToInt(ResourceManager.Gold)));
            HudUiFactory.SetText(
                stoneText,
                Localization.Format("ui.resource_amount", Localization.Get("resource.stone"), Mathf.FloorToInt(ResourceManager.Stone)));
            HudUiFactory.SetText(
                popText,
                Localization.Format(
                    "ui.resource_amount",
                    Localization.Get("ui.pop"),
                    $"{PopulationManager.CurrentPopulation}/{PopulationManager.MaxPopulation}"));

            string civLabel = CivilizationBonusUtility.GetHudLabel(UnitTeam.Player);
            civText.gameObject.SetActive(!string.IsNullOrEmpty(civLabel));
            HudUiFactory.SetText(civText, civLabel);

            bool inPlacementMode = BuildingPlacementManager.IsPlacementModeActive;
            bool showBuildMenu = !inPlacementMode
                && selectionManager != null
                && selectionManager.HasSelectedPlayerVillagers();
            buildMenuRoot.gameObject.SetActive(showBuildMenu);

            bool canAffordAnyBuild = false;
            if (showBuildMenu)
            {
                for (int i = 0; i < buildButtons.Count; i++)
                {
                    BuildButtonEntry entry = buildButtons[i];
                    HudUiFactory.SetButtonLabel(entry.button, entry.getLabel());
                    bool interactable = entry.getInteractable();
                    entry.button.interactable = interactable;
                    if (interactable)
                        canAffordAnyBuild = true;
                }
            }

            RefreshHint(inPlacementMode, showBuildMenu && canAffordAnyBuild);
            ResizePanel(showBuildMenu);
        }

        void RefreshHint(bool inPlacementMode, bool canAffordAnyBuild)
        {
            if (hintRoot == null)
                return;

            if (inPlacementMode)
            {
                hintRoot.gameObject.SetActive(true);
                HudUiFactory.SetText(hintText, Localization.Get("ui.placement_hint"));
                float panelBottom = HudUiFactory.Margin + GetPanelHeight(showBuildMenu: false);
                HudUiFactory.SetAnchoredTopLeft(hintRoot, HudUiFactory.Margin, panelBottom + 4f, PanelWidth + 60f, 36f);
                GameUiInput.RegisterHintFromRectTransform(hintRoot);
                return;
            }

            if (!canAffordAnyBuild && selectionManager != null && selectionManager.HasSelectedPlayerVillagers())
            {
                hintRoot.gameObject.SetActive(true);
                HudUiFactory.SetText(hintText, Localization.Get("ui.need_wood"));
                float panelBottom = HudUiFactory.Margin + GetPanelHeight(showBuildMenu: true);
                HudUiFactory.SetAnchoredTopLeft(hintRoot, HudUiFactory.Margin + Padding, panelBottom + 4f, PanelWidth, 20f);
                GameUiInput.RegisterHintFromRectTransform(hintRoot);
                return;
            }

            hintRoot.gameObject.SetActive(false);
            GameUiInput.ClearHudHintScreenRect();
        }

        float GetPanelHeight(bool showBuildMenu)
        {
            float height = Padding * 2f + LineHeight * 5f + CivLineHeight + ButtonGap * 5f;
            if (showBuildMenu)
                height += buildButtons.Count * ButtonHeight + (buildButtons.Count - 1) * ButtonGap;
            return height;
        }

        void ResizePanel(bool showBuildMenu)
        {
            panelRoot.sizeDelta = new Vector2(PanelWidth, GetPanelHeight(showBuildMenu));
        }

        public static bool IsPointerOverHud(Vector2 screenPosition)
        {
            return GameUiInput.IsPointerOverHud(screenPosition);
        }
    }
}

using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Input;
using AoE.RTS.Selection;
using AoE.RTS.Units;
using AoE.RTS.View;
using AoE.RTS.Visuals;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AoE.RTS.Buildings
{
    public class BuildingPlacementManager : MonoBehaviour, ISimulationTickable
    {
        struct ConstructionSite
        {
            public PlacedBuildingData data;
            public Unit builder;
            public Unit queuedBuilder;
            public Vector3 position;
            public float remainingTime;
            public bool builderArrived;
            public GameObject siteVisual;
            public float wallOrientationY;
        }

        const float BuilderReachPadding = 1.5f;
        const float ApproachReachDistance = 2.5f;
        const float GroundRayDistance = 1000f;
        const float MinSiteSeparation = 5f;
        const float WallDragThresholdPixels = 8f;

        static BuildingPlacementManager instance;

        [SerializeField] UnityEngine.Camera mainCamera;
        [SerializeField] RTSInputReader input;
        [SerializeField] PlacedBuildingData houseData;
        [SerializeField] PlacedBuildingData barracksData;
        [SerializeField] PlacedBuildingData archeryRangeData;
        [SerializeField] PlacedBuildingData stableData;
        [SerializeField] PlacedBuildingData blacksmithData;
        [SerializeField] PlacedBuildingData palisadeWallData;
        [SerializeField] PlacedBuildingData stoneWallData;
        [SerializeField] PlacedBuildingData gateData;
        [SerializeField] PlacedBuildingData watchTowerData;
        [SerializeField] PlacedBuildingData marketData;
        [SerializeField] PlacedBuildingData townCenterPlacementData;
        [SerializeField] PlacedBuildingData farmData;
        [SerializeField] PlacedBuildingData lumberCampData;
        [SerializeField] PlacedBuildingData miningCampData;
        [SerializeField] PlacedBuildingData millData;
        [SerializeField] MonoBehaviour placementPreviewViewHost;

        PlacedBuildingData activePlacementData;

        readonly List<ConstructionSite> sites = new List<ConstructionSite>();
        readonly List<Unit> stashedBuilders = new List<Unit>();
        readonly List<Unit> builderLookupBuffer = new List<Unit>();
        readonly List<Vector3> wallLineBuffer = new List<Vector3>();
        readonly List<PlacementPreviewState> wallPreviewBuffer = new List<PlacementPreviewState>();

        Vector3 ghostPosition;
        bool isPlacementModeActive;
        int placementOpenedFrame = -1;
        int wallLineBuilderCursor;
        bool wallDragTracking;
        Vector2 wallDragStartScreen;

        public static bool IsPlacementModeActive => instance != null && instance.isPlacementModeActive;

        public static bool IsActiveWallPlacement()
        {
            return instance != null
                && instance.isPlacementModeActive
                && IsWallKind(instance.activePlacementData);
        }

        public static bool IsUnitBuilding(Unit unit)
        {
            if (instance == null || unit == null)
                return false;

            for (int i = 0; i < instance.sites.Count; i++)
            {
                if (instance.sites[i].builder == unit)
                    return true;
            }

            return false;
        }

        public static bool HasActiveConstructionForTeam(UnitTeam team)
        {
            if (instance == null)
                return false;

            for (int i = 0; i < instance.sites.Count; i++)
            {
                Unit builder = instance.sites[i].builder;
                if (builder != null && builder.Team == team)
                    return true;
            }

            return false;
        }

        public static bool HasActiveBarracksConstructionForTeam(UnitTeam team)
        {
            if (instance == null)
                return false;

            for (int i = 0; i < instance.sites.Count; i++)
            {
                ConstructionSite site = instance.sites[i];
                if (site.builder == null || site.builder.Team != team)
                    continue;

                if (site.data != null && site.data.kind == PlacedBuildingKind.Barracks)
                    return true;
            }

            return false;
        }

        public static bool HasActiveArcheryRangeConstructionForTeam(UnitTeam team)
        {
            if (instance == null)
                return false;

            for (int i = 0; i < instance.sites.Count; i++)
            {
                ConstructionSite site = instance.sites[i];
                if (site.builder == null || site.builder.Team != team)
                    continue;

                if (site.data != null && site.data.kind == PlacedBuildingKind.ArcheryRange)
                    return true;
            }

            return false;
        }

        public static bool HasActiveStableConstructionForTeam(UnitTeam team)
        {
            if (instance == null)
                return false;

            for (int i = 0; i < instance.sites.Count; i++)
            {
                ConstructionSite site = instance.sites[i];
                if (site.builder == null || site.builder.Team != team)
                    continue;

                if (site.data != null && site.data.kind == PlacedBuildingKind.Stable)
                    return true;
            }

            return false;
        }

        void Awake()
        {
            instance = this;
            houseData = PlacedBuildingDataResolver.ResolveHouse(ref houseData);
            barracksData = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
            archeryRangeData = PlacedBuildingDataResolver.ResolveArcheryRange(ref archeryRangeData);
            stableData = PlacedBuildingDataResolver.ResolveStable(ref stableData);
            blacksmithData = PlacedBuildingDataResolver.ResolveBlacksmith(ref blacksmithData);
            palisadeWallData = PlacedBuildingDataResolver.ResolvePalisadeWall(ref palisadeWallData);
            stoneWallData = PlacedBuildingDataResolver.ResolveStoneWall(ref stoneWallData);
            gateData = PlacedBuildingDataResolver.ResolveGate(ref gateData);
            watchTowerData = PlacedBuildingDataResolver.ResolveWatchTower(ref watchTowerData);
            marketData = PlacedBuildingDataResolver.ResolveMarket(ref marketData);
            townCenterPlacementData = PlacedBuildingDataResolver.ResolveTownCenterPlacement(ref townCenterPlacementData);
            farmData = PlacedBuildingDataResolver.ResolveFarm(ref farmData);
            lumberCampData = PlacedBuildingDataResolver.ResolveLumberCamp(ref lumberCampData);
            miningCampData = PlacedBuildingDataResolver.ResolveMiningCamp(ref miningCampData);
            millData = PlacedBuildingDataResolver.ResolveMill(ref millData);
            if (mainCamera == null)
                mainCamera = UnityEngine.Camera.main;
            if (input == null)
                input = FindAnyObjectByType<RTSInputReader>();
        }

        IPlacementPreviewView GetPreviewView()
        {
            if (placementPreviewViewHost != null && placementPreviewViewHost is IPlacementPreviewView hostedView)
                return hostedView;

            return PlacementPreviewViewRegistry.Current;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;

            SimulationTick.Unregister(this);
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver)
                return;

            if (isPlacementModeActive)
                UpdatePlacementMode();
        }

        void Start()
        {
            SimulationTick.Register(this);
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            TickConstructionSites(fixedDeltaTime);
        }

        public static void EnterHousePlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.houseData = PlacedBuildingDataResolver.ResolveHouse(ref instance.houseData);
            if (instance.houseData == null)
                return;

            instance.activePlacementData = instance.houseData;
            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.wallLineBuilderCursor = 0;
            instance.RefreshPlacementPreview();
        }

        public static void EnterHousePlacementMode()
        {
            EnterHousePlacementMode(null);
        }

        public static void EnterBarracksPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.barracksData = PlacedBuildingDataResolver.ResolveBarracks(ref instance.barracksData);
            if (instance.barracksData == null || !GameSessionManager.CanBuild(instance.barracksData, UnitTeam.Player))
                return;

            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.activePlacementData = instance.barracksData;
            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.wallLineBuilderCursor = 0;
            instance.RefreshPlacementPreview();
        }

        public static void EnterArcheryRangePlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.archeryRangeData = PlacedBuildingDataResolver.ResolveArcheryRange(ref instance.archeryRangeData);
            if (instance.archeryRangeData == null || !GameSessionManager.CanBuild(instance.archeryRangeData, UnitTeam.Player))
                return;

            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.activePlacementData = instance.archeryRangeData;
            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.wallLineBuilderCursor = 0;
            instance.RefreshPlacementPreview();
        }

        public static void EnterStablePlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.stableData = PlacedBuildingDataResolver.ResolveStable(ref instance.stableData);
            if (instance.stableData == null || !GameSessionManager.CanBuild(instance.stableData, UnitTeam.Player))
                return;

            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.activePlacementData = instance.stableData;
            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.wallLineBuilderCursor = 0;
            instance.RefreshPlacementPreview();
        }

        public static void EnterBlacksmithPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.blacksmithData = PlacedBuildingDataResolver.ResolveBlacksmith(ref instance.blacksmithData);
            if (instance.blacksmithData == null || !GameSessionManager.CanBuild(instance.blacksmithData, UnitTeam.Player))
                return;

            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.activePlacementData = instance.blacksmithData;
            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.wallLineBuilderCursor = 0;
            instance.RefreshPlacementPreview();
        }

        public static void EnterPalisadeWallPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.palisadeWallData = PlacedBuildingDataResolver.ResolvePalisadeWall(ref instance.palisadeWallData);
            if (instance.palisadeWallData == null || !GameSessionManager.CanBuild(instance.palisadeWallData, UnitTeam.Player))
                return;

            BeginPlacementMode(builders, instance.palisadeWallData);
        }

        public static void EnterStoneWallPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.stoneWallData = PlacedBuildingDataResolver.ResolveStoneWall(ref instance.stoneWallData);
            if (instance.stoneWallData == null || !GameSessionManager.CanBuild(instance.stoneWallData, UnitTeam.Player))
                return;

            BeginPlacementMode(builders, instance.stoneWallData);
        }

        public static void EnterGatePlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.gateData = PlacedBuildingDataResolver.ResolveGate(ref instance.gateData);
            if (instance.gateData == null || !GameSessionManager.CanBuild(instance.gateData, UnitTeam.Player))
                return;

            BeginPlacementMode(builders, instance.gateData);
        }

        public static void EnterWatchTowerPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.watchTowerData = PlacedBuildingDataResolver.ResolveWatchTower(ref instance.watchTowerData);
            if (instance.watchTowerData == null || !GameSessionManager.CanBuild(instance.watchTowerData, UnitTeam.Player))
                return;

            BeginPlacementMode(builders, instance.watchTowerData);
        }

        public static void EnterMarketPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.marketData = PlacedBuildingDataResolver.ResolveMarket(ref instance.marketData);
            if (instance.marketData == null || !GameSessionManager.CanBuild(instance.marketData, UnitTeam.Player))
                return;

            BeginPlacementMode(builders, instance.marketData);
        }

        public static void EnterTownCenterPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.townCenterPlacementData = PlacedBuildingDataResolver.ResolveTownCenterPlacement(
                ref instance.townCenterPlacementData);
            if (instance.townCenterPlacementData == null
                || !GameSessionManager.CanBuild(instance.townCenterPlacementData, UnitTeam.Player)
                || !CanPlaceAdditionalTownCenter(UnitTeam.Player))
                return;

            BeginPlacementMode(builders, instance.townCenterPlacementData);
        }

        public static bool CanPlaceAdditionalTownCenter(UnitTeam team)
        {
            return GetTownCenterSlotsUsedForTeam(team) < ProductionManager.MaxTownCentersPerTeam;
        }

        public static int GetTownCenterSlotsUsedForTeam(UnitTeam team)
        {
            int count = ProductionManager.GetTownCenterCountForTeam(team);
            if (instance == null)
                return count;

            for (int i = 0; i < instance.sites.Count; i++)
            {
                ConstructionSite site = instance.sites[i];
                if (site.data == null || site.data.kind != PlacedBuildingKind.TownCenter)
                    continue;

                Unit builder = site.builder;
                if (builder != null && builder.Team == team)
                    count++;
            }

            return count;
        }

        static void BeginPlacementMode(IReadOnlyList<Unit> builders, PlacedBuildingData placementData)
        {
            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.activePlacementData = placementData;
            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.wallLineBuilderCursor = 0;
            instance.wallDragTracking = false;
            instance.RefreshPlacementPreview();
        }

        public static void EnterFarmPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.farmData = PlacedBuildingDataResolver.ResolveFarm(ref instance.farmData);
            if (instance.farmData == null)
                return;

            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.activePlacementData = instance.farmData;
            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.wallLineBuilderCursor = 0;
            instance.RefreshPlacementPreview();
        }

        public static void EnterLumberCampPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.lumberCampData = PlacedBuildingDataResolver.ResolveLumberCamp(ref instance.lumberCampData);
            if (instance.lumberCampData == null)
                return;

            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.activePlacementData = instance.lumberCampData;
            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.wallLineBuilderCursor = 0;
            instance.RefreshPlacementPreview();
        }

        public static void EnterMiningCampPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.miningCampData = PlacedBuildingDataResolver.ResolveMiningCamp(ref instance.miningCampData);
            if (instance.miningCampData == null)
                return;

            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.activePlacementData = instance.miningCampData;
            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.wallLineBuilderCursor = 0;
            instance.RefreshPlacementPreview();
        }

        public static void EnterMillPlacementMode(IReadOnlyList<Unit> builders)
        {
            if (instance == null || GameSessionManager.IsGameOver)
                return;

            instance.millData = PlacedBuildingDataResolver.ResolveMill(ref instance.millData);
            if (instance.millData == null)
                return;

            instance.stashedBuilders.Clear();
            if (builders != null)
            {
                for (int i = 0; i < builders.Count; i++)
                {
                    Unit unit = builders[i];
                    if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player)
                        instance.stashedBuilders.Add(unit);
                }
            }

            instance.activePlacementData = instance.millData;
            instance.isPlacementModeActive = true;
            instance.placementOpenedFrame = Time.frameCount;
            instance.wallLineBuilderCursor = 0;
            instance.RefreshPlacementPreview();
        }

        public static void CancelPlacementMode()
        {
            if (instance == null)
                return;

            instance.isPlacementModeActive = false;
            instance.activePlacementData = null;
            instance.wallDragTracking = false;
            instance.GetPreviewView()?.HidePreview();
            instance.stashedBuilders.Clear();
            instance.placementOpenedFrame = -1;
            instance.wallLineBuilderCursor = 0;
        }

        public static bool TryConfirmPlacement(IReadOnlyList<Unit> builders)
        {
            if (instance == null || !instance.isPlacementModeActive || instance.activePlacementData == null)
                return false;

            if (Time.frameCount <= instance.placementOpenedFrame)
                return false;

            return instance.TryConfirmAtGhostPosition(builders);
        }

        public static bool TryConfirmWallDragLine(
            Vector2 startScreen,
            Vector2 endScreen)
        {
            if (instance == null || !instance.isPlacementModeActive || !IsWallKind(instance.activePlacementData))
                return false;

            if (Time.frameCount <= instance.placementOpenedFrame)
                return false;

            IReadOnlyList<Unit> builders = instance.stashedBuilders.Count > 0
                ? instance.stashedBuilders
                : null;

            if (!instance.TryScreenToPlacementPosition(startScreen, out Vector3 startWorld)
                || !instance.TryScreenToPlacementPosition(endScreen, out Vector3 endWorld))
                return false;

            WallPlacementUtility.GetWallLinePositions(
                startWorld,
                endWorld,
                instance.activePlacementData,
                instance.wallLineBuffer,
                out float orientationY);

            Unit builder = instance.PickNextWallLineBuilder(builders);
            if (builder == null)
                return false;

            int placedCount = 0;
            for (int i = 0; i < instance.wallLineBuffer.Count; i++)
            {
                if (!instance.TryQueueWallSegmentAt(
                        instance.wallLineBuffer[i],
                        instance.activePlacementData,
                        builder,
                        orientationY,
                        assignBuilderNow: i == 0))
                    break;

                placedCount++;
            }

            if (placedCount > 0)
                CancelPlacementMode();

            return placedCount > 0;
        }

        bool TryQueueWallSegmentAt(
            Vector3 placementPosition,
            PlacedBuildingData placementData,
            Unit builder,
            float wallOrientationY,
            bool assignBuilderNow)
        {
            if (builder == null || placementData == null)
                return false;

            if (!GameSessionManager.CanBuild(placementData, builder.Team))
                return false;

            Vector3 snapped = SnapToFootprint(placementPosition);
            if (!CanPlaceBuildingAt(snapped, placementData))
                return false;

            if (!PlacementCostUtility.TrySpend(UnitTeam.Player, placementData))
                return false;

            if (assignBuilderNow)
            {
                builderLookupBuffer.Clear();
                builderLookupBuffer.Add(builder);
                GatherManager.CancelForUnits(builderLookupBuffer);
                FoodGatherManager.CancelForUnits(builderLookupBuffer);
                MineralGatherManager.CancelForUnits(builderLookupBuffer);
                RemoveIncompleteSitesForBuilder(builder);

                Vector3 approach = GetBuildApproachPosition(snapped, placementData, builder);
                builder.SetMoveTarget(approach);
            }

            sites.Add(new ConstructionSite
            {
                data = placementData,
                builder = assignBuilderNow ? builder : null,
                queuedBuilder = assignBuilderNow ? null : builder,
                position = snapped,
                remainingTime = placementData.ScaledBuildTime,
                builderArrived = false,
                siteVisual = CreateConstructionVisual(placementData, snapped),
                wallOrientationY = wallOrientationY
            });

            return true;
        }

        bool TryConfirmAtGhostPosition(IReadOnlyList<Unit> builders)
        {
            IReadOnlyList<Unit> resolvedBuilders = stashedBuilders.Count > 0 ? stashedBuilders : builders;
            if (!TryGetPointerPlacementPosition(out Vector3 placementPosition))
                return false;

            float orientationY = ResolveWallOrientationForPosition(placementPosition, activePlacementData);
            if (!TryQueueConstructionAt(placementPosition, activePlacementData, resolvedBuilders, orientationY))
                return false;

            CancelPlacementMode();
            return true;
        }

        bool TryQueueConstructionAt(
            Vector3 placementPosition,
            PlacedBuildingData placementData,
            IReadOnlyList<Unit> builders,
            float wallOrientationY)
        {
            Unit builder = PickNextWallLineBuilder(builders);
            if (builder == null)
                return false;

            if (!GameSessionManager.CanBuild(placementData, builder.Team))
                return false;

            if (placementData.kind == PlacedBuildingKind.TownCenter && !CanPlaceAdditionalTownCenter(builder.Team))
                return false;

            ghostPosition = SnapToFootprint(placementPosition);
            if (!CanPlaceBuildingAt(ghostPosition, placementData))
                return false;

            if (!PlacementCostUtility.TrySpend(UnitTeam.Player, placementData))
                return false;

            builderLookupBuffer.Clear();
            builderLookupBuffer.Add(builder);
            GatherManager.CancelForUnits(builderLookupBuffer);
            FoodGatherManager.CancelForUnits(builderLookupBuffer);
            MineralGatherManager.CancelForUnits(builderLookupBuffer);
            RemoveIncompleteSitesForBuilder(builder);

            Vector3 approach = GetBuildApproachPosition(ghostPosition, placementData, builder);
            builder.SetMoveTarget(approach);

            sites.Add(new ConstructionSite
            {
                data = placementData,
                builder = builder,
                position = ghostPosition,
                remainingTime = placementData.ScaledBuildTime,
                builderArrived = false,
                siteVisual = CreateConstructionVisual(placementData, ghostPosition),
                wallOrientationY = wallOrientationY
            });

            return true;
        }

        Unit PickNextWallLineBuilder(IReadOnlyList<Unit> builders)
        {
            if (stashedBuilders.Count > 0)
            {
                int attempts = stashedBuilders.Count;
                for (int i = 0; i < attempts; i++)
                {
                    int index = (wallLineBuilderCursor + i) % stashedBuilders.Count;
                    Unit candidate = stashedBuilders[index];
                    if (candidate != null && candidate.IsAlive && !IsBuilderBusy(candidate))
                    {
                        wallLineBuilderCursor = index + 1;
                        return candidate;
                    }
                }
            }

            return ResolveIdleBuilder(builders);
        }

        static float ResolveWallOrientationForPosition(Vector3 position, PlacedBuildingData data)
        {
            if (data == null || data.kind == PlacedBuildingKind.Gate)
                return ResolveGateOrientation(position);

            if (!IsWallKind(data))
                return 0f;

            return 0f;
        }

        static float ResolveGateOrientation(Vector3 position)
        {
            // Default gate faces north-south; extend later with nearest wall heading.
            return 0f;
        }

        bool TryScreenToPlacementPosition(Vector2 screenPosition, out Vector3 placementPosition)
        {
            placementPosition = ghostPosition;
            if (mainCamera == null)
                return false;

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, GroundRayDistance, GameLayers.GroundMask))
                return false;

            placementPosition = SnapToFootprint(hit.point);
            return true;
        }

        bool CanPlaceBuildingAt(Vector3 position, PlacedBuildingData data)
        {
            if (data == null)
                return false;

            if (!GameSessionManager.CanBuild(data, UnitTeam.Player))
                return false;

            if (data.kind == PlacedBuildingKind.Gate)
                return CanPlaceGateAt(position, data);

            return CanPlaceAt(position, data);
        }

        bool CanPlaceGateAt(Vector3 position, PlacedBuildingData data)
        {
            if (!CanPlaceAt(position, data))
                return false;

            return WallOccupancyRegistry.IsAdjacentToWall(position, 6f);
        }

        static bool IsWallKind(PlacedBuildingData data)
        {
            return data != null
                && (data.kind == PlacedBuildingKind.PalisadeWall || data.kind == PlacedBuildingKind.StoneWall);
        }

        public static bool TryStartTeamConstruction(PlacedBuildingData data, Vector3 position, Unit builder)
        {
            if (instance == null || data == null || builder == null || !builder.IsAlive)
                return false;

            if (!GameSessionManager.CanBuild(data, builder.Team))
                return false;

            position = instance.SnapToFootprint(position);
            if (!instance.CanPlaceAt(position, data))
                return false;

            if (!PlacementCostUtility.TrySpend(builder.Team, data))
                return false;

            instance.builderLookupBuffer.Clear();
            instance.builderLookupBuffer.Add(builder);
            GatherManager.CancelForUnits(instance.builderLookupBuffer);
            FoodGatherManager.CancelForUnits(instance.builderLookupBuffer);
            MineralGatherManager.CancelForUnits(instance.builderLookupBuffer);
            instance.RemoveIncompleteSitesForBuilder(builder);

            Vector3 approach = instance.GetBuildApproachPosition(position, data, builder);
            builder.SetMoveTarget(approach);

            instance.sites.Add(new ConstructionSite
            {
                data = data,
                builder = builder,
                position = position,
                remainingTime = data.ScaledBuildTime,
                builderArrived = false,
                siteVisual = instance.CreateConstructionVisual(data, position)
            });

            return true;
        }

        public static bool TryFindPlacementNear(
            Vector3 center,
            float minRadius,
            float maxRadius,
            PlacedBuildingData data,
            out Vector3 placementPosition)
        {
            placementPosition = center;
            if (instance == null || data == null)
                return false;

            const int angleSteps = 16;
            for (float radius = minRadius; radius <= maxRadius; radius += 2f)
            {
                for (int step = 0; step < angleSteps; step++)
                {
                    float angle = step * (Mathf.PI * 2f / angleSteps);
                    Vector3 candidate = new Vector3(
                        center.x + Mathf.Cos(angle) * radius,
                        0f,
                        center.z + Mathf.Sin(angle) * radius);

                    candidate = instance.SnapToFootprint(candidate);
                    if (instance.CanPlaceAt(candidate, data))
                    {
                        placementPosition = candidate;
                        return true;
                    }
                }
            }

            return false;
        }

        Unit ResolveIdleBuilder(IReadOnlyList<Unit> builders)
        {
            Unit builder = PickIdlePlayerBuilder(stashedBuilders);
            if (builder != null)
                return builder;

            return PickIdlePlayerBuilder(builders);
        }

        Unit PickIdlePlayerBuilder(IReadOnlyList<Unit> units)
        {
            if (units == null)
                return null;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit != null && unit.IsAlive && unit.Team == UnitTeam.Player && !IsBuilderBusy(unit))
                    return unit;
            }

            return null;
        }

        bool IsBuilderBusy(Unit builder)
        {
            for (int i = 0; i < sites.Count; i++)
            {
                if (sites[i].builder == builder)
                    return true;
            }

            return false;
        }

        void UpdatePlacementMode()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelPlacementMode();
                return;
            }

            UpdateWallDragInput();
            RefreshPlacementPreview();
        }

        void UpdateWallDragInput()
        {
            if (input == null)
                return;

            Vector2 pointer = input.PointerScreenPosition;
            if (ResourceHudView.IsPointerOverHud(pointer))
                return;

            if (input.WasSelectPressedThisFrame())
            {
                wallDragTracking = true;
                wallDragStartScreen = pointer;
            }

            if (!input.WasSelectReleasedThisFrame() || !wallDragTracking)
                return;

            wallDragTracking = false;
            if (Time.frameCount <= placementOpenedFrame)
                return;

            float thresholdSq = WallDragThresholdPixels * WallDragThresholdPixels;
            if ((pointer - wallDragStartScreen).sqrMagnitude >= thresholdSq)
                TryConfirmWallDragLine(wallDragStartScreen, pointer);
            else
                TryConfirmAtGhostPosition(stashedBuilders);
        }

        void RefreshPlacementPreview()
        {
            if (activePlacementData == null)
                return;

            if (IsWallKind(activePlacementData) && wallDragTracking)
            {
                RefreshWallLinePreviewFromDrag();
                return;
            }

            RefreshGhostFromPointer();
        }

        void RefreshWallLinePreviewFromDrag()
        {
            IPlacementPreviewView previewView = GetPreviewView();
            if (previewView == null || activePlacementData == null)
                return;

            if (!TryScreenToPlacementPosition(wallDragStartScreen, out Vector3 startWorld)
                || !TryGetPointerPlacementPosition(out Vector3 endWorld))
            {
                previewView.HidePreview();
                return;
            }

            WallPlacementUtility.GetWallLinePositions(
                startWorld,
                endWorld,
                activePlacementData,
                wallLineBuffer,
                out float orientationY);

            BuildWallPreviewStates(orientationY);
            if (wallPreviewBuffer.Count == 0)
            {
                previewView.HidePreview();
                return;
            }

            previewView.ShowWallLinePreview(wallPreviewBuffer);
        }

        void BuildWallPreviewStates(float orientationY)
        {
            wallPreviewBuffer.Clear();
            if (activePlacementData == null)
                return;

            float simulatedWood = ResourceManager.Wood;
            float simulatedStone = ResourceManager.Stone;
            bool chainActive = true;

            for (int i = 0; i < wallLineBuffer.Count; i++)
            {
                Vector3 snapped = SnapToFootprint(wallLineBuffer[i]);
                bool valid = false;
                if (chainActive)
                {
                    valid = WouldQueueWallSegmentAt(
                        snapped,
                        activePlacementData,
                        ref simulatedWood,
                        ref simulatedStone);
                    if (!valid)
                        chainActive = false;
                }

                wallPreviewBuffer.Add(new PlacementPreviewState(
                    activePlacementData,
                    snapped,
                    orientationY,
                    valid));
            }
        }

        bool WouldQueueWallSegmentAt(
            Vector3 snapped,
            PlacedBuildingData placementData,
            ref float simulatedWood,
            ref float simulatedStone)
        {
            if (!GameSessionManager.CanBuild(placementData, UnitTeam.Player))
                return false;

            if (!CanPlaceBuildingAt(snapped, placementData))
                return false;

            float woodCost = placementData.ScaledWoodCost;
            float stoneCost = placementData.ScaledStoneCost;
            if (woodCost > simulatedWood || stoneCost > simulatedStone)
                return false;

            simulatedWood -= woodCost;
            simulatedStone -= stoneCost;
            return true;
        }

        void RefreshGhostFromPointer()
        {
            IPlacementPreviewView previewView = GetPreviewView();
            if (previewView == null || mainCamera == null || input == null || activePlacementData == null)
                return;

            if (!TryGetPointerPlacementPosition(out Vector3 placementPosition))
                return;

            ghostPosition = placementPosition;
            float orientationY = ResolveWallOrientationForPosition(ghostPosition, activePlacementData);
            bool valid = CanPlaceBuildingAt(ghostPosition, activePlacementData);
            previewView.ShowSinglePreview(new PlacementPreviewState(
                activePlacementData,
                ghostPosition,
                orientationY,
                valid));
        }

        bool TryGetPointerPlacementPosition(out Vector3 placementPosition)
        {
            placementPosition = ghostPosition;
            if (mainCamera == null || input == null)
                return false;

            Ray ray = mainCamera.ScreenPointToRay(input.PointerScreenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, GroundRayDistance, GameLayers.GroundMask))
                return false;

            placementPosition = SnapToFootprint(hit.point);
            return true;
        }

        void TickConstructionSites(float deltaTime)
        {
            for (int i = sites.Count - 1; i >= 0; i--)
            {
                ConstructionSite site = sites[i];
                if (site.builder == null)
                {
                    if (site.queuedBuilder != null && site.queuedBuilder.IsAlive)
                        continue;

                    DestroySiteVisual(site.siteVisual);
                    sites.RemoveAt(i);
                    continue;
                }

                if (!site.builderArrived)
                {
                    PlacedBuildingData siteData = site.data ?? activePlacementData ?? houseData;
                    if (HasBuilderArrived(site.builder, site.position, siteData))
                    {
                        site.builder.ClearMoveTarget();
                        site.builderArrived = true;
                        sites[i] = site;
                    }
                    else if (!site.builder.HasMoveTarget)
                    {
                        site.builder.SetMoveTarget(GetBuildApproachPosition(site.position, siteData, site.builder));
                    }

                    continue;
                }

                PlacedBuildingData buildData = site.data ?? activePlacementData ?? houseData;
                if (!HasBuilderArrived(site.builder, site.position, buildData))
                    continue;

                site.remainingTime -= deltaTime;
                if (site.remainingTime > 0f)
                {
                    sites[i] = site;
                    continue;
                }

                CompleteConstruction(ref site);
                AssignBuilderToNextQueuedWallSite(site.builder);
                sites.RemoveAt(i);
            }
        }

        void AssignBuilderToNextQueuedWallSite(Unit completedBuilder)
        {
            if (completedBuilder == null || !completedBuilder.IsAlive)
                return;

            for (int i = 0; i < sites.Count; i++)
            {
                ConstructionSite site = sites[i];
                if (site.builder != null || site.queuedBuilder != completedBuilder)
                    continue;

                site.builder = completedBuilder;
                site.queuedBuilder = null;
                site.builderArrived = false;

                PlacedBuildingData siteData = site.data ?? activePlacementData ?? houseData;
                Vector3 approach = GetBuildApproachPosition(site.position, siteData, completedBuilder);
                completedBuilder.SetMoveTarget(approach);
                sites[i] = site;
                return;
            }
        }

        public static void AbortConstructionForUnits(IReadOnlyList<Unit> units)
        {
            if (instance == null || units == null || units.Count == 0)
                return;

            for (int i = instance.sites.Count - 1; i >= 0; i--)
            {
                Unit siteBuilder = instance.sites[i].builder;
                if (siteBuilder == null)
                    continue;

                for (int u = 0; u < units.Count; u++)
                {
                    if (units[u] != siteBuilder)
                        continue;

                    DestroySiteVisual(instance.sites[i].siteVisual);
                    instance.sites.RemoveAt(i);
                    break;
                }
            }
        }

        static readonly List<Unit> singleUnitAbortBuffer = new List<Unit>(1);

        public static void AbortConstructionForUnit(Unit unit)
        {
            if (unit == null)
                return;

            singleUnitAbortBuffer.Clear();
            singleUnitAbortBuffer.Add(unit);
            AbortConstructionForUnits(singleUnitAbortBuffer);
        }

        void CompleteConstruction(ref ConstructionSite site)
        {
            DestroySiteVisual(site.siteVisual);
            site.builder.ClearMoveTarget();

            if (site.data == null)
                return;

            if (site.data.kind == PlacedBuildingKind.Barracks)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                Barracks barracks = RuntimeBuildingFactory.CreateBarracks(site.data, site.position, team);
                if (barracks != null && site.builder != null)
                    barracks.SetTeam(site.builder.Team);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.ArcheryRange)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                ArcheryRange archeryRange = RuntimeBuildingFactory.CreateArcheryRange(site.data, site.position, team);
                if (archeryRange != null && site.builder != null)
                    archeryRange.SetTeam(site.builder.Team);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.Stable)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                Stable stable = RuntimeBuildingFactory.CreateStable(site.data, site.position, team);
                if (stable != null && site.builder != null)
                    stable.SetTeam(site.builder.Team);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.Blacksmith)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                Blacksmith blacksmith = RuntimeBuildingFactory.CreateBlacksmith(site.data, site.position, team);
                if (blacksmith != null && site.builder != null)
                    blacksmith.SetTeam(site.builder.Team);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.PalisadeWall)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                RuntimeBuildingFactory.CreatePalisadeWall(site.data, site.position, team, site.wallOrientationY);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.StoneWall)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                RuntimeBuildingFactory.CreateStoneWall(site.data, site.position, team, site.wallOrientationY);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.Gate)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                RuntimeBuildingFactory.CreateGate(site.data, site.position, team, site.wallOrientationY);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.WatchTower)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                RuntimeBuildingFactory.CreateWatchTower(site.data, site.position, team);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.Market)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                Market market = RuntimeBuildingFactory.CreateMarket(site.data, site.position, team);
                if (market != null && site.builder != null)
                    market.SetTeam(site.builder.Team);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.TownCenter)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                TownCenter townCenter = RuntimeBuildingFactory.CreatePlacedTownCenter(site.position, team);
                if (townCenter != null && site.builder != null)
                    townCenter.SetTeam(site.builder.Team);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.Farm)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                RuntimeBuildingFactory.CreateFarm(site.data, site.position, team);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.LumberCamp)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                RuntimeBuildingFactory.CreateLumberCamp(site.data, site.position, team);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.MiningCamp)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                RuntimeBuildingFactory.CreateMiningCamp(site.data, site.position, team);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.Mill)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                RuntimeBuildingFactory.CreateMill(site.data, site.position, team);
                return;
            }

            if (site.data.kind == PlacedBuildingKind.House)
            {
                UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
                RuntimeBuildingFactory.CreateHouse(site.data, site.position, team);
                if (site.data.housingProvided > 0)
                    PopulationManager.AddHousing(team, site.data.housingProvided);
            }
        }

        Vector3 SnapToFootprint(Vector3 worldPoint)
        {
            return new Vector3(worldPoint.x, 0f, worldPoint.z);
        }

        Vector3 GetBuildApproachPosition(Vector3 sitePosition, PlacedBuildingData data, Unit builder)
        {
            const float buildStandRadius = 2.5f;
            float offset = data != null ? data.footprintDepth * 0.5f + 2f : 4f;
            Vector3 approach = new Vector3(sitePosition.x, 1f, sitePosition.z + offset);
            return UnitPositionOffsets.ApplyRingOffset(approach, builder, buildStandRadius);
        }

        bool HasBuilderArrived(Unit builder, Vector3 sitePosition, PlacedBuildingData data)
        {
            if (builder == null || data == null)
                return false;

            Vector3 approach = GetBuildApproachPosition(sitePosition, data, builder);
            if (builder.IsNear(approach, ApproachReachDistance))
                return true;

            float reach = Mathf.Max(data.footprintWidth, data.footprintDepth) * 0.5f + BuilderReachPadding;
            Vector3 siteCenter = new Vector3(sitePosition.x, 1f, sitePosition.z);
            return builder.IsNear(siteCenter, reach);
        }

        void RemoveIncompleteSitesForBuilder(Unit builder)
        {
            for (int i = sites.Count - 1; i >= 0; i--)
            {
                if (sites[i].builder != builder)
                    continue;

                DestroySiteVisual(sites[i].siteVisual);
                sites.RemoveAt(i);
            }
        }

        bool IsSitePositionOccupied(Vector3 position, PlacedBuildingData data)
        {
            float minSeparation = GetMinSiteSeparation(data);
            float minSeparationSq = minSeparation * minSeparation;
            for (int i = 0; i < sites.Count; i++)
            {
                Vector3 delta = sites[i].position - position;
                delta.y = 0f;
                if (delta.sqrMagnitude < minSeparationSq)
                    return true;
            }

            return false;
        }

        static float GetMinSiteSeparation(PlacedBuildingData data)
        {
            if (IsWallKind(data))
                return Mathf.Max(0.5f, WallPlacementUtility.GetSegmentLength(data) * 0.85f);

            return MinSiteSeparation;
        }

        bool CanPlaceAt(Vector3 position, PlacedBuildingData data)
        {
            if (data == null)
                return false;

            Vector3 halfExtents = new Vector3(
                data.footprintWidth * 0.5f,
                data.buildingHeight * 0.5f,
                data.footprintDepth * 0.5f);

            Vector3 center = new Vector3(
                position.x,
                data.buildingHeight * 0.5f + 0.05f,
                position.z);

            int mask = GameLayers.BuildingMask | GameLayers.ResourceMask;
            Collider[] overlaps = Physics.OverlapBox(center, halfExtents, Quaternion.identity, mask);
            if (overlaps.Length > 0 && !CanIgnoreWallBuildingOverlap(data, overlaps))
                return false;

            return !IsSitePositionOccupied(position, data);
        }

        static bool CanIgnoreWallBuildingOverlap(PlacedBuildingData data, Collider[] overlaps)
        {
            if (!IsWallKind(data))
                return false;

            for (int i = 0; i < overlaps.Length; i++)
            {
                if (!IsWallRelatedCollider(overlaps[i]))
                    return false;
            }

            return overlaps.Length > 0;
        }

        static bool IsWallRelatedCollider(Collider overlap)
        {
            if (overlap == null)
                return false;

            if (overlap.GetComponentInParent<PalisadeWall>() != null
                || overlap.GetComponentInParent<StoneWall>() != null
                || overlap.GetComponentInParent<Gate>() != null)
                return true;

            Transform current = overlap.transform;
            while (current != null)
            {
                if (current.name == "ConstructionSite")
                    return true;

                current = current.parent;
            }

            return false;
        }

        GameObject CreateConstructionVisual(PlacedBuildingData data, Vector3 position)
        {
            GameObject siteRoot = new GameObject("ConstructionSite");
            siteRoot.layer = LayerMask.NameToLayer("Building");
            siteRoot.transform.position = new Vector3(
                position.x,
                data.buildingHeight * 0.5f + 0.05f,
                position.z);

            Vector3 fallbackScale = new Vector3(data.footprintWidth, data.buildingHeight, data.footprintDepth);
            PlaceholderVisualKind kind = EntityVisualBuilder.GetBuildingVisualKind(data);
            GameObject visual = EntityVisualBuilder.CreateGhostVisual(kind, fallbackScale);
            visual.transform.SetParent(siteRoot.transform, false);
            visual.transform.localScale = Vector3.one;
            visual.transform.localPosition = Vector3.zero;

            Renderer renderer = visual.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", data.constructionColor);
                block.SetColor("_Color", data.constructionColor);
                renderer.SetPropertyBlock(block);
            }

            return siteRoot;
        }

        static void DestroySiteVisual(GameObject visual)
        {
            if (visual != null)
                Destroy(visual);
        }
    }
}

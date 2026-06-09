using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Spatial;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.AI
{
    public class CpuEconomyAiManager : MonoBehaviour, ISimulationTickable
    {
        const float EvaluateInterval = 2f;
        const int TargetVillagerCount = 6;
        const float HouseMinRadius = 8f;
        const float HouseMaxRadius = 24f;
        const float EconomyBuildMinRadius = 8f;
        const float EconomyBuildMaxRadius = 24f;
        const float FoodNeedThreshold = 200f;
        const float GoldNeedThreshold = 100f;
        const float StoneNeedThreshold = 100f;

        [SerializeField] PlacedBuildingData houseData;
        [SerializeField] PlacedBuildingData millData;
        [SerializeField] PlacedBuildingData miningCampData;
        [SerializeField] PlacedBuildingData farmData;

        float evaluateTimer;
        TownCenter cpuTownCenter;
        readonly List<Unit> unitBuffer = new List<Unit>(16);
        readonly List<Unit> gatherCommandBuffer = new List<Unit>(1);
        static readonly List<Unit> farmCheckBuffer = new List<Unit>(1);

        void Awake()
        {
            houseData = PlacedBuildingDataResolver.ResolveHouse(ref houseData);
            millData = PlacedBuildingDataResolver.ResolveMill(ref millData);
            miningCampData = PlacedBuildingDataResolver.ResolveMiningCamp(ref miningCampData);
            farmData = PlacedBuildingDataResolver.ResolveFarm(ref farmData);
        }

        void Start()
        {
            RefreshCpuTownCenter();
            evaluateTimer = 0.5f;
            SimulationTick.Register(this);
        }

        void OnDestroy()
        {
            SimulationTick.Unregister(this);
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            evaluateTimer -= fixedDeltaTime;
            if (evaluateTimer > 0f)
                return;

            evaluateTimer = EvaluateInterval;

            if (cpuTownCenter == null)
                RefreshCpuTownCenter();

            if (cpuTownCenter == null)
                return;

            TryBuildHouse();
            TryTrainVillager();
            TryBuildEconomyBuildings();
            AssignIdleVillagers();
        }

        void RefreshCpuTownCenter()
        {
            cpuTownCenter = ProductionManager.GetTownCenterForTeam(CpuAiCoordination.CpuTeam);
        }

        void TryBuildEconomyBuildings()
        {
            if (CpuAiCoordination.ShouldDeferEconomyBuildings())
                return;

            if (CpuAiCoordination.HasActiveCpuConstruction())
                return;

            if (ShouldReserveBuilderForHouse())
                return;

            if (TryBuildMiningCamp())
                return;

            if (TryBuildMill())
                return;

            TryBuildFarm();
        }

        bool TryBuildMiningCamp()
        {
            if (miningCampData == null || HasTeamMiningCamp())
                return false;

            float gold = ResourceManager.GetGold(CpuAiCoordination.CpuTeam);
            float stone = ResourceManager.GetStone(CpuAiCoordination.CpuTeam);
            if (gold >= GoldNeedThreshold && stone >= StoneNeedThreshold)
                return false;

            return TryStartEconomyConstruction(miningCampData, "Mining Camp");
        }

        bool TryBuildMill()
        {
            if (millData == null || HasTeamMill())
                return false;

            if (ResourceManager.GetFood(CpuAiCoordination.CpuTeam) >= FoodNeedThreshold)
                return false;

            return TryStartEconomyConstruction(millData, "Mill");
        }

        bool TryBuildFarm()
        {
            if (farmData == null)
                return false;

            if (ResourceManager.GetFood(CpuAiCoordination.CpuTeam) >= FoodNeedThreshold)
                return false;

            return TryStartEconomyConstruction(farmData, "Farm");
        }

        bool TryStartEconomyConstruction(PlacedBuildingData data, string logName)
        {
            if (data == null || cpuTownCenter == null)
                return false;

            if (!CpuAiCoordination.HasWoodReserveForBarracks(data.woodCost))
                return false;

            if (ResourceManager.GetWood(CpuAiCoordination.CpuTeam) < data.woodCost)
                return false;

            Unit builder = FindBuilderForHouse();
            if (builder == null)
                return false;

            Vector3 center = cpuTownCenter.transform.position;
            if (!BuildingPlacementManager.TryFindPlacementNear(
                    center,
                    EconomyBuildMinRadius,
                    EconomyBuildMaxRadius,
                    data,
                    out Vector3 placement))
                return false;

            if (!BuildingPlacementManager.TryStartTeamConstruction(data, placement, builder))
                return false;

            Debug.Log(
                $"[CPU Economy] {logName} construction started at {FormatTime(Time.timeSinceLevelLoad)} "
                + $"(Wood={ResourceManager.GetWood(CpuAiCoordination.CpuTeam):0})");
            return true;
        }

        void AssignIdleVillagers()
        {
            if (ShouldReserveBuilderForHouse())
                return;

            int woodSlot = 0;
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (!IsCpuVillager(unit) || !IsIdleForEconomy(unit))
                    continue;

                if (TryAssignFoodGather(unit))
                    continue;

                if (TryAssignGoldGather(unit))
                    continue;

                if (TryAssignStoneGather(unit))
                    continue;

                if (TryAssignFarmGather(unit))
                    continue;

                TreeResource tree = TreeSpatialIndex.FindRankedAvailable(unit.transform.position, woodSlot);
                woodSlot++;
                if (tree == null)
                    continue;

                gatherCommandBuffer.Clear();
                gatherCommandBuffer.Add(unit);
                GatherManager.IssueGatherCommand(gatherCommandBuffer, tree);
            }
        }

        bool TryAssignFoodGather(Unit unit)
        {
            if (ResourceManager.GetFood(CpuAiCoordination.CpuTeam) >= FoodNeedThreshold)
                return false;

            BerryBushResource bush = BerryBushSpatialIndex.FindNearestAvailable(unit.transform.position);
            if (bush != null)
            {
                gatherCommandBuffer.Clear();
                gatherCommandBuffer.Add(unit);
                FoodGatherManager.IssueGatherCommand(gatherCommandBuffer, bush);
                Debug.Log(
                    $"[CPU Economy] villager → Berry Bush (Food={ResourceManager.GetFood(CpuAiCoordination.CpuTeam):0})");
                return true;
            }

            if (TryAssignHunt(unit))
                return true;

            return false;
        }

        bool TryAssignHunt(Unit unit)
        {
            if (TryIssueHunt(unit, FindNearestDeer(unit.transform.position)))
                return true;

            return TryIssueHunt(unit, FindNearestOwnedSheep(unit.transform.position));
        }

        bool TryIssueHunt(Unit unit, MonoBehaviour animal)
        {
            if (animal == null)
                return false;

            if (animal is SheepResource sheep && !sheep.CanBeHuntedBy(CpuAiCoordination.CpuTeam))
                return false;

            if (animal is IHuntableFoodResource huntable && huntable.IsDepleted)
                return false;

            gatherCommandBuffer.Clear();
            gatherCommandBuffer.Add(unit);
            FoodGatherManager.IssueHuntCommand(gatherCommandBuffer, (IHuntableFoodResource)animal);
            Debug.Log(
                $"[CPU Economy] villager → Hunt {animal.name} (Food={ResourceManager.GetFood(CpuAiCoordination.CpuTeam):0})");
            return true;
        }

        bool TryAssignGoldGather(Unit unit)
        {
            if (!HasTeamMiningCamp() || ResourceManager.GetGold(CpuAiCoordination.CpuTeam) >= GoldNeedThreshold)
                return false;

            GoldMineResource mine = FindNearestGoldMine(unit.transform.position);
            if (mine == null)
                return false;

            gatherCommandBuffer.Clear();
            gatherCommandBuffer.Add(unit);
            MineralGatherManager.IssueGatherGoldCommand(gatherCommandBuffer, mine);
            Debug.Log(
                $"[CPU Economy] villager → Gold Mine (Gold={ResourceManager.GetGold(CpuAiCoordination.CpuTeam):0})");
            return true;
        }

        bool TryAssignStoneGather(Unit unit)
        {
            if (!HasTeamMiningCamp() || ResourceManager.GetStone(CpuAiCoordination.CpuTeam) >= StoneNeedThreshold)
                return false;

            StoneMineResource mine = FindNearestStoneMine(unit.transform.position);
            if (mine == null)
                return false;

            gatherCommandBuffer.Clear();
            gatherCommandBuffer.Add(unit);
            MineralGatherManager.IssueGatherStoneCommand(gatherCommandBuffer, mine);
            Debug.Log(
                $"[CPU Economy] villager → Stone Mine (Stone={ResourceManager.GetStone(CpuAiCoordination.CpuTeam):0})");
            return true;
        }

        bool TryAssignFarmGather(Unit unit)
        {
            if (ResourceManager.GetFood(CpuAiCoordination.CpuTeam) >= FoodNeedThreshold)
                return false;

            Farm farm = FindAssignableFarm(unit);
            if (farm == null)
                return false;

            gatherCommandBuffer.Clear();
            gatherCommandBuffer.Add(unit);
            FoodGatherManager.IssueGatherFarmCommand(gatherCommandBuffer, farm);
            Debug.Log(
                $"[CPU Economy] villager → Farm (Food={ResourceManager.GetFood(CpuAiCoordination.CpuTeam):0})");
            return true;
        }

        bool ShouldReserveBuilderForHouse()
        {
            if (houseData == null)
                return false;

            if (PopulationManager.GetCurrentPopulation(CpuAiCoordination.CpuTeam)
                < PopulationManager.GetMaxPopulation(CpuAiCoordination.CpuTeam))
                return false;

            if (ResourceManager.GetWood(CpuAiCoordination.CpuTeam) < houseData.woodCost)
                return false;

            return !BuildingPlacementManager.HasActiveConstructionForTeam(CpuAiCoordination.CpuTeam)
                && FindIdleVillagerNearestTownCenter() == null;
        }

        void TryBuildHouse()
        {
            if (houseData == null)
                return;

            if (PopulationManager.GetCurrentPopulation(CpuAiCoordination.CpuTeam)
                < PopulationManager.GetMaxPopulation(CpuAiCoordination.CpuTeam))
                return;

            if (ResourceManager.GetWood(CpuAiCoordination.CpuTeam) < houseData.woodCost)
                return;

            if (BuildingPlacementManager.HasActiveConstructionForTeam(CpuAiCoordination.CpuTeam))
                return;

            Unit builder = FindBuilderForHouse();
            if (builder == null)
                return;

            Vector3 center = cpuTownCenter.transform.position;
            if (!BuildingPlacementManager.TryFindPlacementNear(
                    center,
                    HouseMinRadius,
                    HouseMaxRadius,
                    houseData,
                    out Vector3 placement))
                return;

            BuildingPlacementManager.TryStartTeamConstruction(houseData, placement, builder);
        }

        void TryTrainVillager()
        {
            if (PopulationManager.GetCurrentPopulation(CpuAiCoordination.CpuTeam) >= TargetVillagerCount)
                return;

            if (!PopulationManager.CanTrainUnit(CpuAiCoordination.CpuTeam))
                return;

            cpuTownCenter.TryQueueVillagerProduction();
        }

        static bool IsCpuVillager(Unit unit)
        {
            return unit != null
                && unit.IsAlive
                && unit.Team == CpuAiCoordination.CpuTeam
                && !unit.CanAttack;
        }

        static bool IsIdleForEconomy(Unit unit)
        {
            if (unit.HasMoveTarget)
                return false;

            if (GatherManager.IsUnitGathering(unit))
                return false;

            if (FoodGatherManager.IsUnitGathering(unit))
                return false;

            if (MineralGatherManager.IsUnitGathering(unit))
                return false;

            if (BuildingPlacementManager.IsUnitBuilding(unit))
                return false;

            return true;
        }

        static bool HasTeamMiningCamp()
        {
            MiningCamp[] camps = Object.FindObjectsByType<MiningCamp>();
            for (int i = 0; i < camps.Length; i++)
            {
                if (IsActiveTeamBuilding(camps[i].gameObject, CpuAiCoordination.CpuTeam))
                    return true;
            }

            return false;
        }

        static bool HasTeamMill()
        {
            Mill[] mills = Object.FindObjectsByType<Mill>();
            for (int i = 0; i < mills.Length; i++)
            {
                if (IsActiveTeamBuilding(mills[i].gameObject, CpuAiCoordination.CpuTeam))
                    return true;
            }

            return false;
        }

        static bool IsActiveTeamBuilding(GameObject buildingObject, UnitTeam team)
        {
            if (buildingObject == null || !buildingObject.activeInHierarchy)
                return false;

            BuildingHealth health = buildingObject.GetComponent<BuildingHealth>();
            if (health == null)
                return false;

            return health.Team == team && health.IsAlive;
        }

        static DeerResource FindNearestDeer(Vector3 origin)
        {
            DeerResource best = null;
            float bestDistanceSq = float.MaxValue;
            IReadOnlyList<DeerResource> deerList = DeerRegistry.All;
            for (int i = 0; i < deerList.Count; i++)
            {
                DeerResource deer = deerList[i];
                if (deer == null || deer.IsDepleted)
                    continue;

                float distanceSq = HorizontalDistanceSq(origin, deer.transform.position);
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                best = deer;
            }

            return best;
        }

        static SheepResource FindNearestOwnedSheep(Vector3 origin)
        {
            SheepResource best = null;
            float bestDistanceSq = float.MaxValue;
            IReadOnlyList<SheepResource> sheepList = SheepRegistry.All;
            for (int i = 0; i < sheepList.Count; i++)
            {
                SheepResource sheep = sheepList[i];
                if (sheep == null || sheep.IsDepleted || !sheep.CanBeHuntedBy(CpuAiCoordination.CpuTeam))
                    continue;

                float distanceSq = HorizontalDistanceSq(origin, sheep.transform.position);
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                best = sheep;
            }

            return best;
        }

        static GoldMineResource FindNearestGoldMine(Vector3 origin)
        {
            GoldMineResource best = null;
            float bestDistanceSq = float.MaxValue;
            GoldMineResource[] mines = Object.FindObjectsByType<GoldMineResource>();
            for (int i = 0; i < mines.Length; i++)
            {
                GoldMineResource mine = mines[i];
                if (mine == null || mine.IsDepleted)
                    continue;

                float distanceSq = HorizontalDistanceSq(origin, mine.transform.position);
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                best = mine;
            }

            return best;
        }

        static StoneMineResource FindNearestStoneMine(Vector3 origin)
        {
            StoneMineResource best = null;
            float bestDistanceSq = float.MaxValue;
            StoneMineResource[] mines = Object.FindObjectsByType<StoneMineResource>();
            for (int i = 0; i < mines.Length; i++)
            {
                StoneMineResource mine = mines[i];
                if (mine == null || mine.IsDepleted)
                    continue;

                float distanceSq = HorizontalDistanceSq(origin, mine.transform.position);
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                best = mine;
            }

            return best;
        }

        static Farm FindAssignableFarm(Unit unit)
        {
            Farm[] farms = Object.FindObjectsByType<Farm>();
            Farm best = null;
            float bestDistanceSq = float.MaxValue;
            for (int i = 0; i < farms.Length; i++)
            {
                Farm farm = farms[i];
                if (farm == null || farm.IsDepleted || farm.Team != CpuAiCoordination.CpuTeam)
                    continue;

                farmCheckBuffer.Clear();
                farmCheckBuffer.Add(unit);
                if (!FoodGatherManager.HasAssignableFarmGatherers(farmCheckBuffer, farm))
                    continue;

                float distanceSq = HorizontalDistanceSq(unit.transform.position, farm.transform.position);
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                best = farm;
            }

            return best;
        }

        static float HorizontalDistanceSq(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return (b - a).sqrMagnitude;
        }

        Unit FindBuilderForHouse()
        {
            Unit idle = FindIdleVillagerNearestTownCenter();
            if (idle != null)
                return idle;

            return FindNearestCpuVillagerToTownCenter(requireIdle: false);
        }

        Unit FindIdleVillagerNearestTownCenter()
        {
            return FindNearestCpuVillagerToTownCenter(requireIdle: true);
        }

        Unit FindNearestCpuVillagerToTownCenter(bool requireIdle)
        {
            if (cpuTownCenter == null)
                return null;

            return UnitSpatialIndex.FindNearestUnit(
                cpuTownCenter.transform.position,
                CpuAiCoordination.CpuTeam,
                unit =>
                {
                    if (!IsCpuVillager(unit))
                        return false;

                    if (BuildingPlacementManager.IsUnitBuilding(unit))
                        return false;

                    if (requireIdle && !IsIdleForEconomy(unit))
                        return false;

                    return true;
                });
        }

        static string FormatTime(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int total = Mathf.FloorToInt(seconds);
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}

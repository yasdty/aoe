using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Commands;
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

        [SerializeField] PlayerId cpuPlayerId = PlayerId.Player1;
        [SerializeField] PlacedBuildingData houseData;
        [SerializeField] PlacedBuildingData millData;
        [SerializeField] PlacedBuildingData miningCampData;
        [SerializeField] PlacedBuildingData farmData;

        float evaluateTimer;
        TownCenter cpuTownCenter;
        readonly List<Unit> unitBuffer = new List<Unit>(16);
        readonly List<Unit> gatherCommandBuffer = new List<Unit>(1);
        static readonly List<Unit> farmCheckBuffer = new List<Unit>(1);

        UnitTeam Team => PlayerIdMapping.ToLegacyTeam(cpuPlayerId);

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

        void EnqueueCpu(IGameCommand command) => CpuAiCommandQueue.Enqueue(cpuPlayerId, command);

        void RefreshCpuTownCenter()
        {
            cpuTownCenter = ProductionManager.GetTownCenterForPlayer(cpuPlayerId);
        }

        void TryBuildEconomyBuildings()
        {
            if (CpuAiCoordination.ShouldDeferEconomyBuildings(cpuPlayerId))
                return;

            if (CpuAiCoordination.HasActiveCpuConstruction(cpuPlayerId))
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

            float gold = ResourceManager.GetGold(cpuPlayerId);
            float stone = ResourceManager.GetStone(cpuPlayerId);
            if (gold >= GoldNeedThreshold && stone >= StoneNeedThreshold)
                return false;

            return TryStartEconomyConstruction(miningCampData, "Mining Camp");
        }

        bool TryBuildMill()
        {
            if (millData == null || HasTeamMill())
                return false;

            if (ResourceManager.GetFood(cpuPlayerId) >= FoodNeedThreshold)
                return false;

            return TryStartEconomyConstruction(millData, "Mill");
        }

        bool TryBuildFarm()
        {
            if (farmData == null)
                return false;

            if (ResourceManager.GetFood(cpuPlayerId) >= FoodNeedThreshold)
                return false;

            return TryStartEconomyConstruction(farmData, "Farm");
        }

        bool TryStartEconomyConstruction(PlacedBuildingData data, string logName)
        {
            if (data == null || cpuTownCenter == null)
                return false;

            if (!CpuAiCoordination.HasWoodReserveForBarracks(cpuPlayerId, data.ScaledWoodCost))
                return false;

            if (ResourceManager.GetWood(cpuPlayerId) < data.ScaledWoodCost)
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

            EnqueueCpu(new CpuStartTeamConstructionCommand(data, placement, builder));

            Debug.Log(
                $"[CPU Economy] {logName} construction started at {FormatTime(Time.timeSinceLevelLoad)} "
                + $"(Wood={ResourceManager.GetWood(cpuPlayerId):0})");
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
                EnqueueCpu(new GatherCommand(gatherCommandBuffer, tree));
            }
        }

        bool TryAssignFoodGather(Unit unit)
        {
            if (ResourceManager.GetFood(cpuPlayerId) >= FoodNeedThreshold)
                return false;

            BerryBushResource bush = BerryBushSpatialIndex.FindNearestAvailable(unit.transform.position);
            if (bush != null)
            {
                gatherCommandBuffer.Clear();
                gatherCommandBuffer.Add(unit);
                EnqueueCpu(new GatherFoodCommand(gatherCommandBuffer, bush));
                Debug.Log(
                    $"[CPU Economy] villager → Berry Bush (Food={ResourceManager.GetFood(cpuPlayerId):0})");
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

            if (animal is SheepResource sheep && !sheep.CanBeHuntedBy(Team))
                return false;

            if (animal is IHuntableFoodResource huntable && huntable.IsDepleted)
                return false;

            gatherCommandBuffer.Clear();
            gatherCommandBuffer.Add(unit);
            if (animal is DeerResource deer)
                EnqueueCpu(new HuntFoodCommand(gatherCommandBuffer, deer));
            else if (animal is SheepResource ownedSheep)
                EnqueueCpu(new HuntFoodCommand(gatherCommandBuffer, ownedSheep));
            else if (animal is BoarResource boar)
                EnqueueCpu(new HuntFoodCommand(gatherCommandBuffer, boar));
            else
                return false;

            Debug.Log(
                $"[CPU Economy] villager → Hunt {animal.name} (Food={ResourceManager.GetFood(cpuPlayerId):0})");
            return true;
        }

        bool TryAssignGoldGather(Unit unit)
        {
            if (!HasTeamMiningCamp() || ResourceManager.GetGold(cpuPlayerId) >= GoldNeedThreshold)
                return false;

            GoldMineResource mine = FindNearestGoldMine(unit.transform.position);
            if (mine == null)
                return false;

            gatherCommandBuffer.Clear();
            gatherCommandBuffer.Add(unit);
            EnqueueCpu(new GatherGoldCommand(gatherCommandBuffer, mine));
            Debug.Log(
                $"[CPU Economy] villager → Gold Mine (Gold={ResourceManager.GetGold(cpuPlayerId):0})");
            return true;
        }

        bool TryAssignStoneGather(Unit unit)
        {
            if (!HasTeamMiningCamp() || ResourceManager.GetStone(cpuPlayerId) >= StoneNeedThreshold)
                return false;

            StoneMineResource mine = FindNearestStoneMine(unit.transform.position);
            if (mine == null)
                return false;

            gatherCommandBuffer.Clear();
            gatherCommandBuffer.Add(unit);
            EnqueueCpu(new GatherStoneCommand(gatherCommandBuffer, mine));
            Debug.Log(
                $"[CPU Economy] villager → Stone Mine (Stone={ResourceManager.GetStone(cpuPlayerId):0})");
            return true;
        }

        bool TryAssignFarmGather(Unit unit)
        {
            if (ResourceManager.GetFood(cpuPlayerId) >= FoodNeedThreshold)
                return false;

            Farm farm = FindAssignableFarm(unit);
            if (farm == null)
                return false;

            gatherCommandBuffer.Clear();
            gatherCommandBuffer.Add(unit);
            EnqueueCpu(new GatherFarmFoodCommand(gatherCommandBuffer, farm));
            Debug.Log(
                $"[CPU Economy] villager → Farm (Food={ResourceManager.GetFood(cpuPlayerId):0})");
            return true;
        }

        bool ShouldReserveBuilderForHouse()
        {
            if (houseData == null)
                return false;

            if (PopulationManager.GetCurrentPopulation(cpuPlayerId)
                < PopulationManager.GetMaxPopulation(cpuPlayerId))
                return false;

            if (ResourceManager.GetWood(cpuPlayerId) < houseData.ScaledWoodCost)
                return false;

            return !BuildingPlacementManager.HasActiveConstructionForPlayer(cpuPlayerId)
                && FindIdleVillagerNearestTownCenter() == null;
        }

        void TryBuildHouse()
        {
            if (houseData == null)
                return;

            if (PopulationManager.GetCurrentPopulation(cpuPlayerId)
                < PopulationManager.GetMaxPopulation(cpuPlayerId))
                return;

            if (ResourceManager.GetWood(cpuPlayerId) < houseData.ScaledWoodCost)
                return;

            if (BuildingPlacementManager.HasActiveConstructionForPlayer(cpuPlayerId))
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

            EnqueueCpu(new CpuStartTeamConstructionCommand(houseData, placement, builder));
        }

        void TryTrainVillager()
        {
            if (PopulationManager.GetCurrentPopulation(cpuPlayerId) >= TargetVillagerCount)
                return;

            if (!PopulationManager.CanTrainUnit(cpuPlayerId))
                return;

            EnqueueCpu(new TrainVillagerCommand(cpuTownCenter));
        }

        bool IsCpuVillager(Unit unit)
        {
            return unit != null
                && unit.IsAlive
                && unit.OwnerId == cpuPlayerId
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

        bool HasTeamMiningCamp()
        {
            MiningCamp[] camps = Object.FindObjectsByType<MiningCamp>();
            for (int i = 0; i < camps.Length; i++)
            {
                if (IsActivePlayerBuilding(camps[i].gameObject, cpuPlayerId))
                    return true;
            }

            return false;
        }

        bool HasTeamMill()
        {
            Mill[] mills = Object.FindObjectsByType<Mill>();
            for (int i = 0; i < mills.Length; i++)
            {
                if (IsActivePlayerBuilding(mills[i].gameObject, cpuPlayerId))
                    return true;
            }

            return false;
        }

        static bool IsActivePlayerBuilding(GameObject buildingObject, PlayerId playerId)
        {
            if (buildingObject == null || !buildingObject.activeInHierarchy)
                return false;

            BuildingHealth health = buildingObject.GetComponent<BuildingHealth>();
            if (health == null)
                return false;

            return health.OwnerId == playerId && health.IsAlive;
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

        SheepResource FindNearestOwnedSheep(Vector3 origin)
        {
            SheepResource best = null;
            float bestDistanceSq = float.MaxValue;
            IReadOnlyList<SheepResource> sheepList = SheepRegistry.All;
            for (int i = 0; i < sheepList.Count; i++)
            {
                SheepResource sheep = sheepList[i];
                if (sheep == null || sheep.IsDepleted || !sheep.CanBeHuntedBy(Team))
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

        Farm FindAssignableFarm(Unit unit)
        {
            Farm[] farms = Object.FindObjectsByType<Farm>();
            Farm best = null;
            float bestDistanceSq = float.MaxValue;
            for (int i = 0; i < farms.Length; i++)
            {
                Farm farm = farms[i];
                if (farm == null || farm.IsDepleted || !IsActivePlayerBuilding(farm.gameObject, cpuPlayerId))
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
                Team,
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

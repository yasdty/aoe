using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.AI
{
    public class CpuEconomyAiManager : MonoBehaviour
    {
        const float EvaluateInterval = 2f;
        const int TargetVillagerCount = 6;
        const float HouseMinRadius = 8f;
        const float HouseMaxRadius = 24f;
        const UnitTeam CpuTeam = UnitTeam.Enemy;

        [SerializeField] PlacedBuildingData houseData;

        float evaluateTimer;
        TownCenter cpuTownCenter;
        TreeResource[] cachedTrees;
        readonly List<Unit> unitBuffer = new List<Unit>(16);
        readonly List<Unit> gatherCommandBuffer = new List<Unit>(1);

        void Awake()
        {
            houseData = PlacedBuildingDataResolver.ResolveHouse(ref houseData);
        }

        void Start()
        {
            RefreshTrees();
            RefreshCpuTownCenter();
            evaluateTimer = 0.5f;
        }

        void Update()
        {
            if (GameSessionManager.IsGameOver)
                return;

            evaluateTimer -= Time.deltaTime;
            if (evaluateTimer > 0f)
                return;

            evaluateTimer = EvaluateInterval;

            if (cpuTownCenter == null)
                RefreshCpuTownCenter();

            if (cpuTownCenter == null)
                return;

            TryBuildHouse();
            TryTrainVillager();
            AssignIdleVillagersToTrees();
        }

        void RefreshCpuTownCenter()
        {
            cpuTownCenter = ProductionManager.GetTownCenterForTeam(CpuTeam);
        }

        void RefreshTrees()
        {
            cachedTrees = FindObjectsByType<TreeResource>();
        }

        void AssignIdleVillagersToTrees()
        {
            if (ShouldReserveBuilderForHouse())
                return;

            int gatherSlot = 0;
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (!IsCpuVillager(unit) || !IsIdleForEconomy(unit))
                    continue;

                TreeResource tree = FindRankedAvailableTree(unit.transform.position, gatherSlot);
                gatherSlot++;
                if (tree == null)
                    continue;

                gatherCommandBuffer.Clear();
                gatherCommandBuffer.Add(unit);
                GatherManager.IssueGatherCommand(gatherCommandBuffer, tree);
            }
        }

        bool ShouldReserveBuilderForHouse()
        {
            if (houseData == null)
                return false;

            if (PopulationManager.GetCurrentPopulation(CpuTeam) < PopulationManager.GetMaxPopulation(CpuTeam))
                return false;

            if (ResourceManager.GetWood(CpuTeam) < houseData.woodCost)
                return false;

            return !BuildingPlacementManager.HasActiveConstructionForTeam(CpuTeam)
                && FindIdleVillagerNearestTownCenter() == null;
        }

        void TryBuildHouse()
        {
            if (houseData == null)
                return;

            if (PopulationManager.GetCurrentPopulation(CpuTeam) < PopulationManager.GetMaxPopulation(CpuTeam))
                return;

            if (ResourceManager.GetWood(CpuTeam) < houseData.woodCost)
                return;

            if (BuildingPlacementManager.HasActiveConstructionForTeam(CpuTeam))
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
            if (PopulationManager.GetCurrentPopulation(CpuTeam) >= TargetVillagerCount)
                return;

            if (!PopulationManager.CanTrainUnit(CpuTeam))
                return;

            if (ProductionManager.IsProducing(cpuTownCenter))
                return;

            cpuTownCenter.TryQueueVillagerProduction();
        }

        static bool IsCpuVillager(Unit unit)
        {
            return unit != null
                && unit.IsAlive
                && unit.Team == CpuTeam
                && !unit.CanAttack;
        }

        static bool IsIdleForEconomy(Unit unit)
        {
            if (unit.HasMoveTarget)
                return false;

            if (GatherManager.IsUnitGathering(unit))
                return false;

            if (BuildingPlacementManager.IsUnitBuilding(unit))
                return false;

            return true;
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

            Vector3 center = cpuTownCenter.transform.position;
            Unit best = null;
            float bestDistanceSq = float.MaxValue;

            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (!IsCpuVillager(unit))
                    continue;

                if (BuildingPlacementManager.IsUnitBuilding(unit))
                    continue;

                if (requireIdle && !IsIdleForEconomy(unit))
                    continue;

                Vector3 delta = unit.transform.position - center;
                delta.y = 0f;
                float distanceSq = delta.sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                best = unit;
            }

            return best;
        }

        TreeResource FindRankedAvailableTree(Vector3 fromPosition, int rank)
        {
            if (cachedTrees == null || cachedTrees.Length == 0)
            {
                RefreshTrees();
                if (cachedTrees == null || cachedTrees.Length == 0)
                    return null;
            }

            TreeResource best = null;
            float bestDistanceSq = float.MaxValue;

            for (int i = 0; i < cachedTrees.Length; i++)
            {
                TreeResource tree = cachedTrees[i];
                if (tree == null || tree.IsDepleted)
                    continue;

                Vector3 delta = tree.transform.position - fromPosition;
                delta.y = 0f;
                float distanceSq = delta.sqrMagnitude;

                int betterCount = 0;
                for (int j = 0; j < cachedTrees.Length; j++)
                {
                    TreeResource other = cachedTrees[j];
                    if (other == null || other.IsDepleted || other == tree)
                        continue;

                    Vector3 otherDelta = other.transform.position - fromPosition;
                    otherDelta.y = 0f;
                    if (otherDelta.sqrMagnitude < distanceSq)
                        betterCount++;
                }

                if (betterCount != rank)
                    continue;

                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                best = tree;
            }

            return best ?? FindNearestAvailableTree(fromPosition);
        }

        TreeResource FindNearestAvailableTree(Vector3 fromPosition)
        {
            if (cachedTrees == null || cachedTrees.Length == 0)
            {
                RefreshTrees();
                if (cachedTrees == null || cachedTrees.Length == 0)
                    return null;
            }

            TreeResource best = null;
            float bestDistanceSq = float.MaxValue;

            for (int i = 0; i < cachedTrees.Length; i++)
            {
                TreeResource tree = cachedTrees[i];
                if (tree == null || tree.IsDepleted)
                    continue;

                Vector3 delta = tree.transform.position - fromPosition;
                delta.y = 0f;
                float distanceSq = delta.sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                best = tree;
            }

            return best;
        }
    }
}

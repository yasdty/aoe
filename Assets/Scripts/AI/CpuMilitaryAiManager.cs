using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Combat;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Selection;
using AoE.RTS.Spatial;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.AI
{
    public class CpuMilitaryAiManager : MonoBehaviour, ISimulationTickable
    {
        const float EvaluateInterval = 2f;
        const int TargetMilitiaCount = 8;
        const int TargetSpearmanCount = 4;
        const int TargetArcherCount = 4;
        const int TargetCavalryCount = 3;
        const int TargetScoutCount = 2;
        const int MinAttackUnitsForWave = 1;
        const float BarracksMinRadius = 10f;
        const float BarracksMaxRadius = 24f;
        const float ArcheryRangeMinRadius = 12f;
        const float ArcheryRangeMaxRadius = 28f;
        const float StableMinRadius = 14f;
        const float StableMaxRadius = 32f;
        const UnitTeam CpuTeam = UnitTeam.Enemy;
        const UnitTeam PlayerTeam = UnitTeam.Player;

        static CpuMilitaryAiManager instance;

        [SerializeField] PlacedBuildingData barracksData;
        [SerializeField] PlacedBuildingData archeryRangeData;
        [SerializeField] PlacedBuildingData stableData;
        [SerializeField] float barracksBuildDelaySeconds = 60f;
        [SerializeField] float attackWaveIntervalSeconds = 30f;

        float evaluateTimer;
        float waveTimer;
        TownCenter cpuTownCenter;
        readonly List<Unit> unitBuffer = new List<Unit>(24);
        readonly List<Unit> attackWaveBuffer = new List<Unit>(16);

        public static CpuMilitaryAiManager Instance => instance;
        public float WaveTimerRemaining => waveTimer;
        public float BarracksBuildDelayRemaining => Mathf.Max(0f, barracksBuildDelaySeconds - Time.timeSinceLevelLoad);
        public float BarracksWoodCost => barracksData != null ? barracksData.woodCost : 50f;
        public bool HasCpuBarracks => BarracksProductionManager.HasBarracksForTeam(CpuTeam);
        public bool IsBuildingCpuBarracks => BuildingPlacementManager.HasActiveBarracksConstructionForTeam(CpuTeam);
        public bool HasCpuArcheryRange => ArcheryRangeProductionManager.HasArcheryRangeForTeam(CpuTeam);
        public bool HasCpuStable => StableProductionManager.HasStableForTeam(CpuTeam);

        void Awake()
        {
            instance = this;
            barracksData = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
            archeryRangeData = PlacedBuildingDataResolver.ResolveArcheryRange(ref archeryRangeData);
            stableData = PlacedBuildingDataResolver.ResolveStable(ref stableData);
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;

            SimulationTick.Unregister(this);
        }

        void Start()
        {
            RefreshCpuTownCenter();
            evaluateTimer = 1f;
            waveTimer = attackWaveIntervalSeconds;
            SimulationTick.Register(this);
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            waveTimer -= fixedDeltaTime;
            if (waveTimer <= 0f)
            {
                waveTimer = attackWaveIntervalSeconds;
                LaunchAttackWave();
            }

            evaluateTimer -= fixedDeltaTime;
            if (evaluateTimer > 0f)
                return;

            evaluateTimer = EvaluateInterval;

            if (cpuTownCenter == null)
                RefreshCpuTownCenter();

            if (cpuTownCenter == null)
                return;

            TryBuildBarracks();
            TryBuildArcheryRange();
            TryBuildStable();
            TryTrainMilitia();
            TryTrainSpearman();
            TryTrainArcher();
            TryTrainCavalry();
            TryTrainScout();
        }

        void RefreshCpuTownCenter()
        {
            cpuTownCenter = ProductionManager.GetTownCenterForTeam(CpuTeam);
        }

        void TryBuildBarracks()
        {
            if (barracksData == null)
                return;

            if (Time.timeSinceLevelLoad < barracksBuildDelaySeconds)
                return;

            if (BarracksProductionManager.HasBarracksForTeam(CpuTeam))
                return;

            if (BuildingPlacementManager.HasActiveConstructionForTeam(CpuTeam))
                return;

            if (ResourceManager.GetWood(CpuTeam) < barracksData.woodCost)
                return;

            Unit builder = FindBuilderVillager();
            if (builder == null)
                return;

            Vector3 center = cpuTownCenter.transform.position;
            if (!BuildingPlacementManager.TryFindPlacementNear(
                    center,
                    BarracksMinRadius,
                    BarracksMaxRadius,
                    barracksData,
                    out Vector3 placement))
                return;

            if (BuildingPlacementManager.TryStartTeamConstruction(barracksData, placement, builder))
                Debug.Log($"[CPU Military] Barracks construction started at {FormatTime(Time.timeSinceLevelLoad)}");
        }

        void TryBuildArcheryRange()
        {
            if (archeryRangeData == null)
                return;

            if (!BarracksProductionManager.HasBarracksForTeam(CpuTeam))
                return;

            if (ArcheryRangeProductionManager.HasArcheryRangeForTeam(CpuTeam))
                return;

            if (BuildingPlacementManager.HasActiveConstructionForTeam(CpuTeam))
                return;

            if (ResourceManager.GetWood(CpuTeam) < archeryRangeData.woodCost)
                return;

            Unit builder = FindBuilderVillager();
            if (builder == null)
                return;

            Vector3 center = cpuTownCenter.transform.position;
            if (!BuildingPlacementManager.TryFindPlacementNear(
                    center,
                    ArcheryRangeMinRadius,
                    ArcheryRangeMaxRadius,
                    archeryRangeData,
                    out Vector3 placement))
                return;

            if (BuildingPlacementManager.TryStartTeamConstruction(archeryRangeData, placement, builder))
                Debug.Log($"[CPU Military] Archery Range construction started at {FormatTime(Time.timeSinceLevelLoad)}");
        }

        void TryBuildStable()
        {
            if (stableData == null)
                return;

            if (!ArcheryRangeProductionManager.HasArcheryRangeForTeam(CpuTeam))
                return;

            if (StableProductionManager.HasStableForTeam(CpuTeam))
                return;

            if (BuildingPlacementManager.HasActiveConstructionForTeam(CpuTeam))
                return;

            if (ResourceManager.GetWood(CpuTeam) < stableData.woodCost)
                return;

            Unit builder = FindBuilderVillager();
            if (builder == null)
                return;

            Vector3 center = cpuTownCenter.transform.position;
            if (!BuildingPlacementManager.TryFindPlacementNear(
                    center,
                    StableMinRadius,
                    StableMaxRadius,
                    stableData,
                    out Vector3 placement))
                return;

            if (BuildingPlacementManager.TryStartTeamConstruction(stableData, placement, builder))
                Debug.Log($"[CPU Military] Stable construction started at {FormatTime(Time.timeSinceLevelLoad)}");
        }

        void TryTrainMilitia()
        {
            if (CountCpuUnitsByName("Militia") >= TargetMilitiaCount)
                return;

            Barracks barracks = BarracksProductionManager.GetBarracksForTeam(CpuTeam);
            if (barracks == null)
                return;

            if (!PopulationManager.CanTrainUnit(CpuTeam))
                return;

            if (barracks.Data == null || barracks.Data.trainUnitData == null)
                return;

            if (ResourceManager.GetWood(CpuTeam) < barracks.Data.trainWoodCost)
                return;

            barracks.TryQueueMilitiaProduction();
        }

        void TryTrainSpearman()
        {
            if (CountCpuUnitsByName("Spearman") >= TargetSpearmanCount)
                return;

            Barracks barracks = BarracksProductionManager.GetBarracksForTeam(CpuTeam);
            if (barracks == null)
                return;

            if (!PopulationManager.CanTrainUnit(CpuTeam))
                return;

            if (barracks.Data == null || barracks.Data.secondaryTrainUnitData == null)
                return;

            if (ResourceManager.GetWood(CpuTeam) < barracks.Data.secondaryTrainWoodCost)
                return;

            if (ResourceManager.GetFood(CpuTeam) < barracks.Data.secondaryTrainFoodCost)
                return;

            barracks.TryQueueSpearmanProduction();
        }

        void TryTrainArcher()
        {
            if (CountCpuUnitsByName("Archer") >= TargetArcherCount)
                return;

            ArcheryRange archeryRange = ArcheryRangeProductionManager.GetArcheryRangeForTeam(CpuTeam);
            if (archeryRange == null)
                return;

            if (!PopulationManager.CanTrainUnit(CpuTeam))
                return;

            if (archeryRange.Data == null || archeryRange.Data.trainUnitData == null)
                return;

            if (ResourceManager.GetWood(CpuTeam) < archeryRange.Data.trainWoodCost)
                return;

            if (ResourceManager.GetFood(CpuTeam) < archeryRange.Data.trainFoodCost)
                return;

            archeryRange.TryQueueArcherProduction();
        }

        void TryTrainCavalry()
        {
            if (CountCpuUnitsByName("Cavalry") >= TargetCavalryCount)
                return;

            Stable stable = StableProductionManager.GetStableForTeam(CpuTeam);
            if (stable == null)
                return;

            if (!PopulationManager.CanTrainUnit(CpuTeam))
                return;

            if (stable.Data == null || stable.Data.trainUnitData == null)
                return;

            if (ResourceManager.GetWood(CpuTeam) < stable.Data.trainWoodCost)
                return;

            if (ResourceManager.GetFood(CpuTeam) < stable.Data.trainFoodCost)
                return;

            stable.TryQueueCavalryProduction();
        }

        void TryTrainScout()
        {
            if (CountCpuUnitsByName("Scout") >= TargetScoutCount)
                return;

            Stable stable = StableProductionManager.GetStableForTeam(CpuTeam);
            if (stable == null)
                return;

            if (!PopulationManager.CanTrainUnit(CpuTeam))
                return;

            if (stable.Data == null || stable.Data.secondaryTrainUnitData == null)
                return;

            if (ResourceManager.GetWood(CpuTeam) < stable.Data.secondaryTrainWoodCost)
                return;

            if (ResourceManager.GetFood(CpuTeam) < stable.Data.secondaryTrainFoodCost)
                return;

            stable.TryQueueScoutProduction();
        }

        void LaunchAttackWave()
        {
            CollectCpuAttackUnits(attackWaveBuffer);
            if (attackWaveBuffer.Count < MinAttackUnitsForWave)
            {
                if (BarracksProductionManager.HasBarracksForTeam(CpuTeam))
                    Debug.Log("[CPU Military] Attack wave skipped — no military units");
                return;
            }

            Vector3 rallyPoint = cpuTownCenter != null
                ? cpuTownCenter.transform.position
                : attackWaveBuffer[0].transform.position;

            Unit target = UnitSpatialIndex.FindNearestUnit(rallyPoint, PlayerTeam);
            if (target != null)
            {
                AttackManager.IssueAttack(attackWaveBuffer, target);
                Debug.Log(
                    $"[CPU Military] Attack wave: {FormatWaveComposition(attackWaveBuffer)} at {FormatTime(Time.timeSinceLevelLoad)}");
                return;
            }

            TownCenter playerTownCenter = ProductionManager.GetTownCenterForTeam(PlayerTeam);
            if (playerTownCenter != null)
            {
                BuildingHealth playerTownCenterHealth = playerTownCenter.GetComponent<BuildingHealth>();
                if (playerTownCenterHealth != null && playerTownCenterHealth.IsAlive)
                {
                    AttackManager.IssueAttack(attackWaveBuffer, playerTownCenterHealth);
                    Debug.Log(
                        $"[CPU Military] Attack wave: {FormatWaveComposition(attackWaveBuffer)} → Town Center at {FormatTime(Time.timeSinceLevelLoad)}");
                    return;
                }
            }

            Vector3 advanceTarget = playerTownCenter != null
                ? playerTownCenter.transform.position
                : attackWaveBuffer[0].transform.position;
            advanceTarget.y = 1f;
            FormationMoveManager.Register(attackWaveBuffer, advanceTarget, 2f);

            Debug.Log(
                $"[CPU Military] Attack wave: {FormatWaveComposition(attackWaveBuffer)} advancing at {FormatTime(Time.timeSinceLevelLoad)}");
        }

        static string FormatWaveComposition(IReadOnlyList<Unit> units)
        {
            if (units == null || units.Count == 0)
                return "0 units";

            int militia = 0;
            int spearman = 0;
            int archer = 0;
            int cavalry = 0;
            int scout = 0;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.Data == null)
                    continue;

                switch (unit.Data.displayName)
                {
                    case "Militia":
                        militia++;
                        break;
                    case "Spearman":
                        spearman++;
                        break;
                    case "Archer":
                        archer++;
                        break;
                    case "Cavalry":
                        cavalry++;
                        break;
                    case "Scout":
                        scout++;
                        break;
                }
            }

            List<string> parts = new List<string>(5);
            if (militia > 0)
                parts.Add($"Militia×{militia}");
            if (spearman > 0)
                parts.Add($"Spearman×{spearman}");
            if (archer > 0)
                parts.Add($"Archer×{archer}");
            if (cavalry > 0)
                parts.Add($"Cavalry×{cavalry}");
            if (scout > 0)
                parts.Add($"Scout×{scout}");

            string breakdown = parts.Count > 0 ? string.Join(", ", parts) : "mixed";
            return $"{units.Count} units ({breakdown})";
        }

        void CollectCpuAttackUnits(List<Unit> buffer)
        {
            buffer.Clear();
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (unit == null || !unit.IsAlive || unit.Team != CpuTeam || !unit.CanAttack)
                    continue;

                string name = unit.Data != null ? unit.Data.displayName : string.Empty;
                if (name == "Militia" || name == "Spearman" || name == "Archer" || name == "Cavalry" || name == "Scout")
                    buffer.Add(unit);
            }
        }

        int CountCpuUnitsByName(string displayName)
        {
            int count = 0;
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (unit == null || !unit.IsAlive || unit.Team != CpuTeam)
                    continue;

                if (unit.Data != null && unit.Data.displayName == displayName)
                    count++;
            }

            return count;
        }

        Unit FindBuilderVillager()
        {
            if (cpuTownCenter == null)
                return null;

            return UnitSpatialIndex.FindNearestUnit(
                cpuTownCenter.transform.position,
                CpuTeam,
                unit => IsCpuVillager(unit) && !BuildingPlacementManager.IsUnitBuilding(unit));
        }

        static bool IsCpuVillager(Unit unit)
        {
            return unit != null
                && unit.IsAlive
                && unit.Team == CpuTeam
                && !unit.CanAttack;
        }

        static string FormatTime(float seconds)
        {
            seconds = Mathf.Max(0f, seconds);
            int total = Mathf.FloorToInt(seconds);
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}

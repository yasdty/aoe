using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Combat;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Spatial;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.AI
{
    public class CpuMilitaryAiManager : MonoBehaviour, ISimulationTickable
    {
        const float EvaluateInterval = 2f;
        const int TargetMilitiaCount = 8;
        const int MinMilitiaForWave = 1;
        const float BarracksMinRadius = 10f;
        const float BarracksMaxRadius = 24f;
        const UnitTeam CpuTeam = UnitTeam.Enemy;
        const UnitTeam PlayerTeam = UnitTeam.Player;

        static CpuMilitaryAiManager instance;

        [SerializeField] PlacedBuildingData barracksData;
        [SerializeField] float barracksBuildDelaySeconds = 60f;
        [SerializeField] float attackWaveIntervalSeconds = 30f;

        float evaluateTimer;
        float waveTimer;
        TownCenter cpuTownCenter;
        readonly List<Unit> unitBuffer = new List<Unit>(24);
        readonly List<Unit> militiaAttackBuffer = new List<Unit>(16);

        public static CpuMilitaryAiManager Instance => instance;
        public float WaveTimerRemaining => waveTimer;
        public float BarracksBuildDelayRemaining => Mathf.Max(0f, barracksBuildDelaySeconds - Time.timeSinceLevelLoad);
        public float BarracksWoodCost => barracksData != null ? barracksData.woodCost : 50f;
        public bool HasCpuBarracks => BarracksProductionManager.HasBarracksForTeam(CpuTeam);
        public bool IsBuildingCpuBarracks => BuildingPlacementManager.HasActiveBarracksConstructionForTeam(CpuTeam);

        void Awake()
        {
            instance = this;
            barracksData = PlacedBuildingDataResolver.ResolveBarracks(ref barracksData);
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
            TryTrainMilitia();
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

        void TryTrainMilitia()
        {
            if (CountCpuMilitia() >= TargetMilitiaCount)
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

        void LaunchAttackWave()
        {
            CollectCpuMilitia(militiaAttackBuffer);
            if (militiaAttackBuffer.Count < MinMilitiaForWave)
            {
                if (BarracksProductionManager.HasBarracksForTeam(CpuTeam))
                    Debug.Log("[CPU Military] Attack wave skipped — no Militia");
                return;
            }

            Vector3 rallyPoint = cpuTownCenter != null
                ? cpuTownCenter.transform.position
                : militiaAttackBuffer[0].transform.position;

            Unit target = UnitSpatialIndex.FindNearestUnit(rallyPoint, PlayerTeam);
            if (target != null)
            {
                AttackManager.IssueAttack(militiaAttackBuffer, target);
                Debug.Log(
                    $"[CPU Military] Attack wave: {militiaAttackBuffer.Count} Militia at {FormatTime(Time.timeSinceLevelLoad)}");
                return;
            }

            TownCenter playerTownCenter = ProductionManager.GetTownCenterForTeam(PlayerTeam);
            if (playerTownCenter != null)
            {
                BuildingHealth playerTownCenterHealth = playerTownCenter.GetComponent<BuildingHealth>();
                if (playerTownCenterHealth != null && playerTownCenterHealth.IsAlive)
                {
                    AttackManager.IssueAttack(militiaAttackBuffer, playerTownCenterHealth);
                    Debug.Log(
                        $"[CPU Military] Attack wave: {militiaAttackBuffer.Count} Militia → Town Center at {FormatTime(Time.timeSinceLevelLoad)}");
                    return;
                }
            }

            Vector3 advanceTarget = playerTownCenter != null
                ? playerTownCenter.transform.position
                : militiaAttackBuffer[0].transform.position;
            advanceTarget.y = 1f;
            for (int i = 0; i < militiaAttackBuffer.Count; i++)
            {
                Unit militia = militiaAttackBuffer[i];
                Vector3 offsetTarget = UnitPositionOffsets.ApplyRingOffset(advanceTarget, militia, 4f);
                militia.SetMoveTarget(offsetTarget);
            }

            Debug.Log(
                $"[CPU Military] Attack wave: {militiaAttackBuffer.Count} Militia advancing at {FormatTime(Time.timeSinceLevelLoad)}");
        }

        void CollectCpuMilitia(List<Unit> buffer)
        {
            buffer.Clear();
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (unit != null && unit.IsAlive && unit.Team == CpuTeam && unit.CanAttack)
                    buffer.Add(unit);
            }
        }

        int CountCpuMilitia()
        {
            CollectCpuMilitia(militiaAttackBuffer);
            return militiaAttackBuffer.Count;
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

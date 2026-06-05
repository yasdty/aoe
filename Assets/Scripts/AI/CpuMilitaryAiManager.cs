using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Combat;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.AI
{
    public class CpuMilitaryAiManager : MonoBehaviour
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
        }

        void Start()
        {
            RefreshCpuTownCenter();
            evaluateTimer = 1f;
            waveTimer = attackWaveIntervalSeconds;
        }

        void Update()
        {
            float deltaTime = Time.deltaTime;
            waveTimer -= deltaTime;
            if (waveTimer <= 0f)
            {
                waveTimer = attackWaveIntervalSeconds;
                LaunchAttackWave();
            }

            evaluateTimer -= deltaTime;
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

            if (BuildingPlacementManager.HasActiveBarracksConstructionForTeam(CpuTeam))
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
                Debug.Log($"CPU Barracks construction started at {FormatTime(Time.timeSinceLevelLoad)}");
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

            if (BarracksProductionManager.IsProducing(barracks))
                return;

            barracks.TryQueueMilitiaProduction();
        }

        void LaunchAttackWave()
        {
            CollectCpuMilitia(militiaAttackBuffer);
            if (militiaAttackBuffer.Count < MinMilitiaForWave)
                return;

            Vector3 rallyPoint = cpuTownCenter != null
                ? cpuTownCenter.transform.position
                : militiaAttackBuffer[0].transform.position;

            Unit target = FindNearestPlayerUnit(rallyPoint);
            if (target != null)
            {
                AttackManager.IssueAttack(militiaAttackBuffer, target);
                Debug.Log(
                    $"CPU attack wave at {FormatTime(Time.timeSinceLevelLoad)}: {militiaAttackBuffer.Count} Militia");
                return;
            }

            TownCenter playerTownCenter = ProductionManager.GetTownCenterForTeam(PlayerTeam);
            if (playerTownCenter == null)
                return;

            Vector3 advanceTarget = playerTownCenter.transform.position;
            advanceTarget.y = 1f;
            for (int i = 0; i < militiaAttackBuffer.Count; i++)
            {
                Unit militia = militiaAttackBuffer[i];
                Vector3 offsetTarget = UnitPositionOffsets.ApplyRingOffset(advanceTarget, militia, 4f);
                militia.SetMoveTarget(offsetTarget);
            }

            Debug.Log(
                $"CPU attack wave at {FormatTime(Time.timeSinceLevelLoad)}: {militiaAttackBuffer.Count} Militia advancing");
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

        Unit FindNearestPlayerUnit(Vector3 fromPosition)
        {
            Unit best = null;
            float bestDistanceSq = float.MaxValue;

            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (unit == null || !unit.IsAlive || unit.Team != PlayerTeam)
                    continue;

                Vector3 delta = unit.transform.position - fromPosition;
                delta.y = 0f;
                float distanceSq = delta.sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                best = unit;
            }

            return best;
        }

        Unit FindBuilderVillager()
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

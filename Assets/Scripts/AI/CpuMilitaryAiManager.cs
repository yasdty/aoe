using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Commands;
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
        const int MinAttackUnitsForWaveAggressive = 1;
        const int MinAttackUnitsForWaveRelaxed = 1;

        static readonly string[] WaveUnitTypeOrder =
        {
            "Militia",
            "Spearman",
            "Archer",
            "Cavalry",
            "Scout"
        };
        const float BarracksMinRadius = 10f;
        const float BarracksMaxRadius = 24f;
        const float ArcheryRangeMinRadius = 12f;
        const float ArcheryRangeMaxRadius = 28f;
        const float StableMinRadius = 14f;
        const float StableMaxRadius = 32f;

        static CpuMilitaryAiManager instance;

        [SerializeField] PlayerId cpuPlayerId = PlayerId.Player1;
        [SerializeField] PlayerId opponentPlayerId = PlayerId.Player0;
        [SerializeField] PlacedBuildingData barracksData;
        [SerializeField] PlacedBuildingData archeryRangeData;
        [SerializeField] PlacedBuildingData stableData;
        [SerializeField] float barracksBuildDelaySeconds = 60f;
        [SerializeField] float attackWaveIntervalSeconds = 30f;
        [SerializeField] float relaxedFirstAttackGraceSeconds = 120f;
        [SerializeField] float relaxedBarracksBuildDelaySeconds = 90f;
        [SerializeField] float relaxedAttackWaveIntervalSeconds = 300f;
        [SerializeField] int relaxedMaxUnitsPerTypePerWave = 2;
        [SerializeField] int aggressiveMaxUnitsPerTypePerWave = 2;

        float evaluateTimer;
        float waveTimer;
        float relaxedGraceEndsAt;
        TownCenter cpuTownCenter;
        readonly List<Unit> unitBuffer = new List<Unit>(24);
        readonly List<Unit> attackWaveBuffer = new List<Unit>(16);

        UnitTeam Team => PlayerIdMapping.ToLegacyTeam(cpuPlayerId);
        UnitTeam OpponentTeam => PlayerIdMapping.ToLegacyTeam(opponentPlayerId);

        public static CpuMilitaryAiManager Instance => instance;
        public static bool IsCpuOffensiveActionsSuppressed =>
            instance != null && instance.IsAttackGraceActive;

        public float WaveTimerRemaining
        {
            get
            {
                float graceRemaining = AttackGraceRemainingSeconds;
                if (graceRemaining > 0f)
                    return graceRemaining;

                return waveTimer;
            }
        }

        public float BarracksBuildDelayRemaining =>
            Mathf.Max(0f, EffectiveBarracksBuildDelaySeconds - Time.timeSinceLevelLoad);

        public bool IsAttackGraceActive => AttackGraceRemainingSeconds > 0f;

        float AttackGraceRemainingSeconds =>
            IsRelaxedPace ? Mathf.Max(0f, relaxedGraceEndsAt - Time.timeSinceLevelLoad) : 0f;

        bool IsRelaxedPace => GameSessionManager.CpuAttackPace == CpuAttackPace.Relaxed;

        float EffectiveBarracksBuildDelaySeconds =>
            IsRelaxedPace
                ? relaxedBarracksBuildDelaySeconds
                : GameplayBalance.ScaleCpuDelaySeconds(barracksBuildDelaySeconds);

        float EffectiveAttackWaveIntervalSeconds =>
            IsRelaxedPace
                ? relaxedAttackWaveIntervalSeconds
                : GameplayBalance.ScaleCpuDelaySeconds(attackWaveIntervalSeconds);

        int EffectiveMinAttackUnitsForWave =>
            IsRelaxedPace ? MinAttackUnitsForWaveRelaxed : MinAttackUnitsForWaveAggressive;

        int EffectiveMaxUnitsPerTypePerWave =>
            IsRelaxedPace ? relaxedMaxUnitsPerTypePerWave : aggressiveMaxUnitsPerTypePerWave;
        public float BarracksWoodCost => barracksData != null ? barracksData.ScaledWoodCost : 0f;
        public bool HasCpuBarracks => BarracksProductionManager.HasBarracksForTeam(Team);
        public bool IsBuildingCpuBarracks => BuildingPlacementManager.HasActiveBarracksConstructionForTeam(Team);
        public bool HasCpuArcheryRange => ArcheryRangeProductionManager.HasArcheryRangeForTeam(Team);
        public bool HasCpuStable => StableProductionManager.HasStableForTeam(Team);

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

        float ScaledBarracksBuildDelaySeconds => EffectiveBarracksBuildDelaySeconds;

        float ScaledAttackWaveIntervalSeconds => EffectiveAttackWaveIntervalSeconds;

        void Start()
        {
            RefreshCpuTownCenter();
            evaluateTimer = 1f;
            waveTimer = ScaledAttackWaveIntervalSeconds;
            ApplyGraceForCurrentPace(resetFromNow: false);
            SimulationTick.Register(this);
        }

        public void NotifyCpuAttackPaceChanged(CpuAttackPace pace)
        {
            ApplyGraceForCurrentPace(resetFromNow: pace == CpuAttackPace.Relaxed);
            waveTimer = ScaledAttackWaveIntervalSeconds;
        }

        void ApplyGraceForCurrentPace(bool resetFromNow)
        {
            if (!IsRelaxedPace)
            {
                relaxedGraceEndsAt = 0f;
                return;
            }

            relaxedGraceEndsAt = resetFromNow
                ? Time.timeSinceLevelLoad + relaxedFirstAttackGraceSeconds
                : relaxedFirstAttackGraceSeconds;
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            if (IsAttackGraceActive)
            {
                waveTimer = ScaledAttackWaveIntervalSeconds;
            }
            else
            {
                waveTimer -= fixedDeltaTime;
                if (waveTimer <= 0f)
                {
                    waveTimer = ScaledAttackWaveIntervalSeconds;
                    LaunchAttackWave();
                }
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

        void EnqueueCpu(IGameCommand command) => CpuAiCommandQueue.Enqueue(cpuPlayerId, command);

        void RefreshCpuTownCenter()
        {
            cpuTownCenter = ProductionManager.GetTownCenterForTeam(Team);
        }

        void TryBuildBarracks()
        {
            if (barracksData == null)
                return;

            if (Time.timeSinceLevelLoad < EffectiveBarracksBuildDelaySeconds)
                return;

            if (BarracksProductionManager.HasBarracksForTeam(Team))
                return;

            if (BuildingPlacementManager.HasActiveConstructionForTeam(Team))
                return;

            if (ResourceManager.GetWood(Team) < barracksData.ScaledWoodCost)
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

            EnqueueCpu(new CpuStartTeamConstructionCommand(barracksData, placement, builder));
            Debug.Log($"[CPU Military] Barracks construction started at {FormatTime(Time.timeSinceLevelLoad)}");
        }

        void TryBuildArcheryRange()
        {
            if (archeryRangeData == null)
                return;

            if (!BarracksProductionManager.HasBarracksForTeam(Team))
                return;

            TryEnsureFeudalAge();

            if (!GameSessionManager.CanBuild(archeryRangeData, Team))
                return;

            if (ArcheryRangeProductionManager.HasArcheryRangeForTeam(Team))
                return;

            if (BuildingPlacementManager.HasActiveConstructionForTeam(Team))
                return;

            if (ResourceManager.GetWood(Team) < archeryRangeData.ScaledWoodCost)
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

            EnqueueCpu(new CpuStartTeamConstructionCommand(archeryRangeData, placement, builder));
            Debug.Log($"[CPU Military] Archery Range construction started at {FormatTime(Time.timeSinceLevelLoad)}");
        }

        void TryBuildStable()
        {
            if (stableData == null)
                return;

            if (!ArcheryRangeProductionManager.HasArcheryRangeForTeam(Team))
                return;

            TryEnsureFeudalAge();

            if (!GameSessionManager.CanBuild(stableData, Team))
                return;

            if (StableProductionManager.HasStableForTeam(Team))
                return;

            if (BuildingPlacementManager.HasActiveConstructionForTeam(Team))
                return;

            if (ResourceManager.GetWood(Team) < stableData.ScaledWoodCost)
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

            EnqueueCpu(new CpuStartTeamConstructionCommand(stableData, placement, builder));
            Debug.Log($"[CPU Military] Stable construction started at {FormatTime(Time.timeSinceLevelLoad)}");
        }

        void TryTrainMilitia()
        {
            if (CountCpuUnitsByName("Militia") >= TargetMilitiaCount)
                return;

            Barracks barracks = BarracksProductionManager.GetBarracksForTeam(Team);
            if (barracks == null)
                return;

            if (!PopulationManager.CanTrainUnit(Team))
                return;

            if (barracks.Data == null || barracks.Data.trainUnitData == null)
                return;

            if (ResourceManager.GetFood(Team) < barracks.Data.ScaledTrainFoodCost)
                return;

            if (barracks.Data.ScaledTrainWoodCost > 0f
                && ResourceManager.GetWood(Team) < barracks.Data.ScaledTrainWoodCost)
                return;

            EnqueueCpu(new TrainMilitiaCommand(barracks));
        }

        void TryTrainSpearman()
        {
            if (CountCpuUnitsByName("Spearman") >= TargetSpearmanCount)
                return;

            Barracks barracks = BarracksProductionManager.GetBarracksForTeam(Team);
            if (barracks == null)
                return;

            if (!PopulationManager.CanTrainUnit(Team))
                return;

            if (barracks.Data == null || barracks.Data.secondaryTrainUnitData == null)
                return;

            if (ResourceManager.GetWood(Team) < barracks.Data.ScaledSecondaryTrainWoodCost)
                return;

            if (ResourceManager.GetFood(Team) < barracks.Data.ScaledSecondaryTrainFoodCost)
                return;

            EnqueueCpu(new TrainSpearmanCommand(barracks));
        }

        void TryTrainArcher()
        {
            if (CountCpuUnitsByName("Archer") >= TargetArcherCount)
                return;

            ArcheryRange archeryRange = ArcheryRangeProductionManager.GetArcheryRangeForTeam(Team);
            if (archeryRange == null)
                return;

            if (!PopulationManager.CanTrainUnit(Team))
                return;

            if (archeryRange.Data == null || archeryRange.Data.trainUnitData == null)
                return;

            if (ResourceManager.GetWood(Team) < archeryRange.Data.ScaledTrainWoodCost)
                return;

            if (ResourceManager.GetFood(Team) < archeryRange.Data.ScaledTrainFoodCost)
                return;

            EnqueueCpu(new TrainArcherCommand(archeryRange));
        }

        void TryTrainCavalry()
        {
            if (CountCpuUnitsByName("Cavalry") >= TargetCavalryCount)
                return;

            Stable stable = StableProductionManager.GetStableForTeam(Team);
            if (stable == null)
                return;

            if (!PopulationManager.CanTrainUnit(Team))
                return;

            if (stable.Data == null || stable.Data.trainUnitData == null)
                return;

            if (ResourceManager.GetWood(Team) < stable.Data.ScaledTrainWoodCost)
                return;

            if (ResourceManager.GetFood(Team) < stable.Data.ScaledTrainFoodCost)
                return;

            EnqueueCpu(new TrainCavalryCommand(stable));
        }

        void TryTrainScout()
        {
            if (CountCpuUnitsByName("Scout") >= TargetScoutCount)
                return;

            Stable stable = StableProductionManager.GetStableForTeam(Team);
            if (stable == null)
                return;

            if (!PopulationManager.CanTrainUnit(Team))
                return;

            if (stable.Data == null || stable.Data.secondaryTrainUnitData == null)
                return;

            if (ResourceManager.GetWood(Team) < stable.Data.ScaledSecondaryTrainWoodCost)
                return;

            if (ResourceManager.GetFood(Team) < stable.Data.ScaledSecondaryTrainFoodCost)
                return;

            EnqueueCpu(new TrainScoutCommand(stable));
        }

        void TryEnsureFeudalAge()
        {
            if (GameSessionManager.GetAge(Team) >= GameAge.Feudal)
                return;

            EnqueueCpu(new CpuAgeUpCommand(Team));
        }

        void LaunchAttackWave()
        {
            LaunchAttackWaveInternal(respectAttackGrace: true);
        }

        public void ForceDebugAttackWave()
        {
            if (GameplayBalance.Mode != GameplayBalanceMode.Debug)
                return;

            LaunchAttackWaveInternal(respectAttackGrace: false);
        }

        void LaunchAttackWaveInternal(bool respectAttackGrace)
        {
            if (respectAttackGrace && IsAttackGraceActive)
                return;

            CollectCpuAttackWaveUnits(attackWaveBuffer);
            if (attackWaveBuffer.Count < EffectiveMinAttackUnitsForWave)
            {
                if (BarracksProductionManager.HasBarracksForTeam(Team))
                    Debug.Log("[CPU Military] Attack wave skipped — no military units");
                return;
            }

            Vector3 rallyPoint = cpuTownCenter != null
                ? cpuTownCenter.transform.position
                : attackWaveBuffer[0].transform.position;

            Unit target = UnitSpatialIndex.FindNearestUnit(rallyPoint, OpponentTeam);
            if (target != null)
            {
                EnqueueCpu(new AttackUnitCommand(attackWaveBuffer, target));
                Debug.Log(
                    $"[CPU Military] Attack wave: {FormatWaveComposition(attackWaveBuffer)} at {FormatTime(Time.timeSinceLevelLoad)}");
                return;
            }

            TownCenter playerTownCenter = ProductionManager.GetTownCenterForTeam(OpponentTeam);
            if (playerTownCenter != null)
            {
                BuildingHealth playerTownCenterHealth = playerTownCenter.GetComponent<BuildingHealth>();
                if (playerTownCenterHealth != null && playerTownCenterHealth.IsAlive)
                {
                    EnqueueCpu(new AttackBuildingCommand(attackWaveBuffer, playerTownCenterHealth));
                    Debug.Log(
                        $"[CPU Military] Attack wave: {FormatWaveComposition(attackWaveBuffer)} → Town Center at {FormatTime(Time.timeSinceLevelLoad)}");
                    return;
                }
            }

            Vector3 advanceTarget = playerTownCenter != null
                ? playerTownCenter.transform.position
                : attackWaveBuffer[0].transform.position;
            advanceTarget.y = 1f;
            EnqueueCpu(new MoveCommand(attackWaveBuffer, advanceTarget, 2f));

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

        void CollectCpuAttackWaveUnits(List<Unit> buffer)
        {
            buffer.Clear();
            int maxPerType = Mathf.Max(1, EffectiveMaxUnitsPerTypePerWave);
            UnitManager.CopyUnitsTo(unitBuffer);

            for (int typeIndex = 0; typeIndex < WaveUnitTypeOrder.Length; typeIndex++)
            {
                string unitType = WaveUnitTypeOrder[typeIndex];
                int taken = 0;
                for (int i = 0; i < unitBuffer.Count; i++)
                {
                    if (taken >= maxPerType)
                        break;

                    Unit unit = unitBuffer[i];
                    if (unit == null || !unit.IsAlive || unit.Team != Team || !unit.CanAttack)
                        continue;

                    if (unit.Data == null || unit.Data.displayName != unitType)
                        continue;

                    buffer.Add(unit);
                    taken++;
                }
            }
        }

        int CountCpuUnitsByName(string displayName)
        {
            int count = 0;
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (unit == null || !unit.IsAlive || unit.Team != Team)
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
                Team,
                unit => IsCpuVillager(unit) && !BuildingPlacementManager.IsUnitBuilding(unit));
        }

        bool IsCpuVillager(Unit unit)
        {
            return unit != null
                && unit.IsAlive
                && unit.Team == Team
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

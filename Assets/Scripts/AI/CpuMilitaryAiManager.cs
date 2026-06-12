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
        const float AttackReevaluateCooldown = 4f;

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
        float evaluateTimer;
        float attackEvaluateTimer;
        TownCenter cpuTownCenter;
        readonly List<Unit> unitBuffer = new List<Unit>(24);
        readonly List<Unit> attackWaveBuffer = new List<Unit>(16);

        UnitTeam Team => PlayerIdMapping.ToLegacyTeam(cpuPlayerId);
        UnitTeam OpponentTeam => PlayerIdMapping.ToLegacyTeam(opponentPlayerId);

        public static CpuMilitaryAiManager Instance => instance;
        public static bool IsCpuOffensiveActionsSuppressed => false;

        public int CpuArmyCount => CountArmyUnits();

        public int AttackThreshold =>
            CpuDifficultySettings.Current.ResolveAttackThreshold(CountOpponentArmyUnits());

        public float BarracksBuildDelayRemaining =>
            Mathf.Max(0f, CpuDifficultySettings.Current.BarracksUnlockSeconds - Time.timeSinceLevelLoad);
        public float BarracksWoodCost => barracksData != null ? barracksData.ScaledWoodCost : 0f;
        public bool HasCpuBarracks => PlayerBuildingQueries.HasBarracksForPlayer(cpuPlayerId);
        public bool IsBuildingCpuBarracks => BuildingPlacementManager.HasActiveBarracksConstructionForPlayer(cpuPlayerId);
        public bool HasCpuArcheryRange => PlayerBuildingQueries.HasArcheryRangeForPlayer(cpuPlayerId);
        public bool HasCpuStable => PlayerBuildingQueries.HasStableForPlayer(cpuPlayerId);

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
            attackEvaluateTimer = 2f;
            SimulationTick.Register(this);
        }

        public void NotifyCpuDifficultyChanged()
        {
            attackEvaluateTimer = 2f;
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            evaluateTimer -= fixedDeltaTime;
            attackEvaluateTimer -= fixedDeltaTime;

            if (cpuTownCenter == null)
                RefreshCpuTownCenter();

            if (cpuTownCenter == null)
                return;

            if (evaluateTimer <= 0f)
            {
                CpuDifficultyProfile profile = CpuDifficultySettings.Current;
                evaluateTimer = profile.DecisionInterval;
                CpuAiActionQueue.BeginCycle(cpuPlayerId, profile.MaxActionsPerCycle);

                TryBuildBarracks();
                TryBuildArcheryRange();
                TryBuildStable();
                TryTrainMilitia();
                TryTrainSpearman();
                TryTrainArcher();
                TryTrainCavalry();
                TryTrainScout();
            }

            if (attackEvaluateTimer <= 0f)
            {
                attackEvaluateTimer = AttackReevaluateCooldown;
                TryLaunchArmyAttack();
            }
        }

        bool TryScheduleCpu(CpuAiActionKind kind, IGameCommand command) =>
            CpuAiActionQueue.TrySchedule(cpuPlayerId, kind, command);

        void RefreshCpuTownCenter()
        {
            cpuTownCenter = ProductionManager.GetTownCenterForPlayer(cpuPlayerId);
        }

        void TryBuildBarracks()
        {
            if (barracksData == null)
                return;

            if (Time.timeSinceLevelLoad < CpuDifficultySettings.Current.BarracksUnlockSeconds)
                return;

            if (PlayerBuildingQueries.HasBarracksForPlayer(cpuPlayerId))
                return;

            if (BuildingPlacementManager.HasActiveConstructionForPlayer(cpuPlayerId))
                return;

            if (ResourceManager.GetWood(cpuPlayerId) < barracksData.ScaledWoodCost)
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

            TryScheduleCpu(CpuAiActionKind.Build, new CpuStartTeamConstructionCommand(barracksData, placement, builder));
            Debug.Log($"[CPU Military] Barracks construction started at {FormatTime(Time.timeSinceLevelLoad)}");
        }

        void TryBuildArcheryRange()
        {
            if (archeryRangeData == null)
                return;

            if (!PlayerBuildingQueries.HasBarracksForPlayer(cpuPlayerId))
                return;

            TryEnsureFeudalAge();

            if (!GameSessionManager.CanBuild(archeryRangeData, cpuPlayerId))
                return;

            if (PlayerBuildingQueries.HasArcheryRangeForPlayer(cpuPlayerId))
                return;

            if (BuildingPlacementManager.HasActiveConstructionForPlayer(cpuPlayerId))
                return;

            if (ResourceManager.GetWood(cpuPlayerId) < archeryRangeData.ScaledWoodCost)
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

            TryScheduleCpu(CpuAiActionKind.Build, new CpuStartTeamConstructionCommand(archeryRangeData, placement, builder));
            Debug.Log($"[CPU Military] Archery Range construction started at {FormatTime(Time.timeSinceLevelLoad)}");
        }

        void TryBuildStable()
        {
            if (stableData == null)
                return;

            if (!PlayerBuildingQueries.HasArcheryRangeForPlayer(cpuPlayerId))
                return;

            TryEnsureFeudalAge();

            if (!GameSessionManager.CanBuild(stableData, cpuPlayerId))
                return;

            if (PlayerBuildingQueries.HasStableForPlayer(cpuPlayerId))
                return;

            if (BuildingPlacementManager.HasActiveConstructionForPlayer(cpuPlayerId))
                return;

            if (ResourceManager.GetWood(cpuPlayerId) < stableData.ScaledWoodCost)
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

            TryScheduleCpu(CpuAiActionKind.Build, new CpuStartTeamConstructionCommand(stableData, placement, builder));
            Debug.Log($"[CPU Military] Stable construction started at {FormatTime(Time.timeSinceLevelLoad)}");
        }

        void TryTrainMilitia()
        {
            if (CountCpuUnitsByName("Militia") >= GetTargetCountForUnitType("Militia"))
                return;

            Barracks barracks = PlayerBuildingQueries.GetBarracksForPlayer(cpuPlayerId);
            if (barracks == null)
                return;

            if (!PopulationManager.CanTrainUnit(cpuPlayerId))
                return;

            if (barracks.Data == null || barracks.Data.trainUnitData == null)
                return;

            if (ResourceManager.GetFood(cpuPlayerId) < barracks.Data.ScaledTrainFoodCost)
                return;

            if (barracks.Data.ScaledTrainWoodCost > 0f
                && ResourceManager.GetWood(cpuPlayerId) < barracks.Data.ScaledTrainWoodCost)
                return;

            TryScheduleCpu(CpuAiActionKind.Train, new TrainMilitiaCommand(barracks));
        }

        void TryTrainSpearman()
        {
            if (CountCpuUnitsByName("Spearman") >= GetTargetCountForUnitType("Spearman"))
                return;

            Barracks barracks = PlayerBuildingQueries.GetBarracksForPlayer(cpuPlayerId);
            if (barracks == null)
                return;

            if (!PopulationManager.CanTrainUnit(cpuPlayerId))
                return;

            if (barracks.Data == null || barracks.Data.secondaryTrainUnitData == null)
                return;

            if (ResourceManager.GetWood(cpuPlayerId) < barracks.Data.ScaledSecondaryTrainWoodCost)
                return;

            if (ResourceManager.GetFood(cpuPlayerId) < barracks.Data.ScaledSecondaryTrainFoodCost)
                return;

            TryScheduleCpu(CpuAiActionKind.Train, new TrainSpearmanCommand(barracks));
        }

        void TryTrainArcher()
        {
            if (CountCpuUnitsByName("Archer") >= GetTargetCountForUnitType("Archer"))
                return;

            ArcheryRange archeryRange = PlayerBuildingQueries.GetArcheryRangeForPlayer(cpuPlayerId);
            if (archeryRange == null)
                return;

            if (!PopulationManager.CanTrainUnit(cpuPlayerId))
                return;

            if (archeryRange.Data == null || archeryRange.Data.trainUnitData == null)
                return;

            if (ResourceManager.GetWood(cpuPlayerId) < archeryRange.Data.ScaledTrainWoodCost)
                return;

            if (ResourceManager.GetFood(cpuPlayerId) < archeryRange.Data.ScaledTrainFoodCost)
                return;

            TryScheduleCpu(CpuAiActionKind.Train, new TrainArcherCommand(archeryRange));
        }

        void TryTrainCavalry()
        {
            if (CountCpuUnitsByName("Cavalry") >= GetTargetCountForUnitType("Cavalry"))
                return;

            Stable stable = PlayerBuildingQueries.GetStableForPlayer(cpuPlayerId);
            if (stable == null)
                return;

            if (!PopulationManager.CanTrainUnit(cpuPlayerId))
                return;

            if (stable.Data == null || stable.Data.trainUnitData == null)
                return;

            if (ResourceManager.GetWood(cpuPlayerId) < stable.Data.ScaledTrainWoodCost)
                return;

            if (ResourceManager.GetFood(cpuPlayerId) < stable.Data.ScaledTrainFoodCost)
                return;

            TryScheduleCpu(CpuAiActionKind.Train, new TrainCavalryCommand(stable));
        }

        void TryTrainScout()
        {
            if (CountCpuUnitsByName("Scout") >= GetTargetCountForUnitType("Scout"))
                return;

            Stable stable = PlayerBuildingQueries.GetStableForPlayer(cpuPlayerId);
            if (stable == null)
                return;

            if (!PopulationManager.CanTrainUnit(cpuPlayerId))
                return;

            if (stable.Data == null || stable.Data.secondaryTrainUnitData == null)
                return;

            if (ResourceManager.GetWood(cpuPlayerId) < stable.Data.ScaledSecondaryTrainWoodCost)
                return;

            if (ResourceManager.GetFood(cpuPlayerId) < stable.Data.ScaledSecondaryTrainFoodCost)
                return;

            TryScheduleCpu(CpuAiActionKind.Train, new TrainScoutCommand(stable));
        }

        void TryEnsureFeudalAge()
        {
            if (GameSessionManager.GetAge(cpuPlayerId) >= GameAge.Feudal)
                return;

            TryScheduleCpu(CpuAiActionKind.Train, new CpuAgeUpCommand(cpuPlayerId));
        }

        public void ForceDebugAttackWave()
        {
            if (GameplayBalance.Mode != GameplayBalanceMode.Debug)
                return;

            LaunchArmyAttack(force: true);
        }

        void TryLaunchArmyAttack()
        {
            LaunchArmyAttack(force: false);
        }

        void LaunchArmyAttack(bool force)
        {
            CpuDifficultyProfile profile = CpuDifficultySettings.Current;
            int armyCount = CountArmyUnits();
            int opponentArmy = CountOpponentArmyUnits();
            int threshold = profile.ResolveAttackThreshold(opponentArmy);
            if (!force && armyCount < threshold)
                return;

            float armyPower = ComputeArmyPower(cpuPlayerId);
            float enemyPower = ComputeArmyPower(opponentPlayerId);
            if (!force && armyPower < enemyPower * profile.AttackConfidence)
                return;

            CollectCpuAttackWaveUnits(attackWaveBuffer, armyCount);
            if (attackWaveBuffer.Count == 0)
                return;

            Vector3 rallyPoint = cpuTownCenter != null
                ? cpuTownCenter.transform.position
                : attackWaveBuffer[0].transform.position;

            Unit target = UnitSpatialIndex.FindNearestUnit(rallyPoint, OpponentTeam);
            if (target != null)
            {
                TryScheduleCpu(CpuAiActionKind.Attack, new AttackUnitCommand(attackWaveBuffer, target));
                Debug.Log(
                    $"[CPU Military] Army attack: {FormatWaveComposition(attackWaveBuffer)} at {FormatTime(Time.timeSinceLevelLoad)}");
                return;
            }

            TownCenter playerTownCenter = ProductionManager.GetTownCenterForPlayer(opponentPlayerId);
            if (playerTownCenter != null)
            {
                BuildingHealth playerTownCenterHealth = playerTownCenter.GetComponent<BuildingHealth>();
                if (playerTownCenterHealth != null && playerTownCenterHealth.IsAlive)
                {
                    TryScheduleCpu(CpuAiActionKind.Attack, new AttackBuildingCommand(attackWaveBuffer, playerTownCenterHealth));
                    Debug.Log(
                        $"[CPU Military] Army attack: {FormatWaveComposition(attackWaveBuffer)} → Town Center at {FormatTime(Time.timeSinceLevelLoad)}");
                    return;
                }
            }

            Vector3 advanceTarget = playerTownCenter != null
                ? playerTownCenter.transform.position
                : attackWaveBuffer[0].transform.position;
            advanceTarget.y = 1f;
            TryScheduleCpu(CpuAiActionKind.Attack, new MoveCommand(attackWaveBuffer, advanceTarget, 2f));

            Debug.Log(
                $"[CPU Military] Army advance: {FormatWaveComposition(attackWaveBuffer)} at {FormatTime(Time.timeSinceLevelLoad)}");
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

        void CollectCpuAttackWaveUnits(List<Unit> buffer, int maxUnits)
        {
            buffer.Clear();
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                if (buffer.Count >= maxUnits)
                    break;

                Unit unit = unitBuffer[i];
                if (unit == null || !unit.IsAlive || unit.OwnerId != cpuPlayerId || !unit.CanAttack)
                    continue;

                buffer.Add(unit);
            }
        }

        int GetTargetCountForUnitType(string displayName)
        {
            CpuDifficultyProfile profile = CpuDifficultySettings.Current;
            int villagerCount = CountVillagers();
            int totalArmy = profile.ResolveTargetArmyCount(villagerCount);
            return displayName switch
            {
                "Militia" => Mathf.Max(2, Mathf.RoundToInt(totalArmy * 0.35f)),
                "Spearman" => Mathf.Max(1, Mathf.RoundToInt(totalArmy * 0.15f)),
                "Archer" => Mathf.Max(2, Mathf.RoundToInt(totalArmy * 0.25f)),
                "Cavalry" => Mathf.Max(1, Mathf.RoundToInt(totalArmy * 0.15f)),
                "Scout" => Mathf.Max(1, Mathf.RoundToInt(totalArmy * 0.10f)),
                _ => 1
            };
        }

        int CountVillagers()
        {
            int count = 0;
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (unit != null && unit.IsAlive && unit.OwnerId == cpuPlayerId && !unit.CanAttack)
                    count++;
            }

            return count;
        }

        int CountArmyUnits()
        {
            int count = 0;
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (unit != null && unit.IsAlive && unit.OwnerId == cpuPlayerId && unit.CanAttack)
                    count++;
            }

            return count;
        }

        int CountOpponentArmyUnits()
        {
            int count = 0;
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (unit != null && unit.IsAlive && unit.OwnerId == opponentPlayerId && unit.CanAttack)
                    count++;
            }

            return count;
        }

        float ComputeArmyPower(PlayerId playerId)
        {
            float power = 0f;
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (unit == null || !unit.IsAlive || unit.OwnerId != playerId || !unit.CanAttack)
                    continue;

                power += unit.AttackPower * unit.MaxHp;
            }

            return power;
        }

        int CountCpuUnitsByName(string displayName)
        {
            int count = 0;
            UnitManager.CopyUnitsTo(unitBuffer);
            for (int i = 0; i < unitBuffer.Count; i++)
            {
                Unit unit = unitBuffer[i];
                if (unit == null || !unit.IsAlive || unit.OwnerId != cpuPlayerId)
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
                && unit.OwnerId == cpuPlayerId
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

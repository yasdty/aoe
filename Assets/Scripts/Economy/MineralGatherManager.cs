using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class MineralGatherManager : MonoBehaviour, ISimulationTickable
    {
        enum GatherState
        {
            MoveToMine,
            Gather,
            MoveToDeposit
        }

        struct GoldGatherJob
        {
            public Unit unit;
            public GoldMineResource mine;
            public GatherState state;
            public float carriedAmount;
        }

        struct StoneGatherJob
        {
            public Unit unit;
            public StoneMineResource mine;
            public GatherState state;
            public float carriedAmount;
        }

        const float CarryCapacity = 10f;
        const float GatherRate = 2.5f;
        const float GatherReachDistance = 2.5f;
        const float DepositReachDistance = 5f;
        const float GatherStandRadius = 2f;
        const float DepositStandRadius = 3.5f;

        static MineralGatherManager instance;
        readonly List<GoldGatherJob> goldJobs = new List<GoldGatherJob>();
        readonly List<StoneGatherJob> stoneJobs = new List<StoneGatherJob>();

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;

            SimulationTick.Unregister(this);
        }

        void Start()
        {
            SimulationTick.Register(this);
        }

        public void TickSimulation(float fixedDeltaTime)
        {
            TickGoldJobs(fixedDeltaTime);
            TickStoneJobs(fixedDeltaTime);
        }

        public static void IssueGatherGoldCommand(IReadOnlyList<Unit> units, GoldMineResource mine)
        {
            if (instance == null || mine == null || mine.IsDepleted)
                return;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.CanAttack)
                    continue;

                if (ProductionManager.GetTownCenterForTeam(unit.Team) == null)
                    continue;

                instance.RemoveJobForUnit(unit);
                instance.goldJobs.Add(new GoldGatherJob
                {
                    unit = unit,
                    mine = mine,
                    state = GatherState.MoveToMine,
                    carriedAmount = 0f
                });
                unit.SetMoveTarget(GetGatherPosition(mine, unit));
            }
        }

        public static void IssueGatherStoneCommand(IReadOnlyList<Unit> units, StoneMineResource mine)
        {
            if (instance == null || mine == null || mine.IsDepleted)
                return;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || unit.CanAttack)
                    continue;

                if (ProductionManager.GetTownCenterForTeam(unit.Team) == null)
                    continue;

                instance.RemoveJobForUnit(unit);
                instance.stoneJobs.Add(new StoneGatherJob
                {
                    unit = unit,
                    mine = mine,
                    state = GatherState.MoveToMine,
                    carriedAmount = 0f
                });
                unit.SetMoveTarget(GetGatherPosition(mine, unit));
            }
        }

        public static void CancelForUnits(IReadOnlyList<Unit> units)
        {
            if (instance == null || units == null)
                return;

            for (int i = 0; i < units.Count; i++)
                instance.RemoveJobForUnit(units[i]);
        }

        static readonly List<Unit> singleUnitCancelBuffer = new List<Unit>(1);

        public static void CancelForUnit(Unit unit)
        {
            if (unit == null)
                return;

            singleUnitCancelBuffer.Clear();
            singleUnitCancelBuffer.Add(unit);
            CancelForUnits(singleUnitCancelBuffer);
        }

        void RemoveJobForUnit(Unit unit)
        {
            for (int i = goldJobs.Count - 1; i >= 0; i--)
            {
                if (goldJobs[i].unit == unit)
                    goldJobs.RemoveAt(i);
            }

            for (int i = stoneJobs.Count - 1; i >= 0; i--)
            {
                if (stoneJobs[i].unit == unit)
                    stoneJobs.RemoveAt(i);
            }
        }

        void TickGoldJobs(float fixedDeltaTime)
        {
            if (goldJobs.Count == 0)
                return;

            for (int i = goldJobs.Count - 1; i >= 0; i--)
            {
                GoldGatherJob job = goldJobs[i];
                if (job.unit == null || !job.unit.IsAlive)
                {
                    goldJobs.RemoveAt(i);
                    continue;
                }

                if (job.mine == null || job.mine.IsDepleted && job.carriedAmount <= 0f)
                {
                    job.unit.ClearMoveTarget();
                    goldJobs.RemoveAt(i);
                    continue;
                }

                switch (job.state)
                {
                    case GatherState.MoveToMine:
                        TickMoveToGoldMine(ref job, i);
                        break;
                    case GatherState.Gather:
                        TickGatherGold(ref job, i, fixedDeltaTime);
                        break;
                    case GatherState.MoveToDeposit:
                        TickMoveGoldToDeposit(ref job, i);
                        break;
                }
            }
        }

        void TickStoneJobs(float fixedDeltaTime)
        {
            if (stoneJobs.Count == 0)
                return;

            for (int i = stoneJobs.Count - 1; i >= 0; i--)
            {
                StoneGatherJob job = stoneJobs[i];
                if (job.unit == null || !job.unit.IsAlive)
                {
                    stoneJobs.RemoveAt(i);
                    continue;
                }

                if (job.mine == null || job.mine.IsDepleted && job.carriedAmount <= 0f)
                {
                    job.unit.ClearMoveTarget();
                    stoneJobs.RemoveAt(i);
                    continue;
                }

                switch (job.state)
                {
                    case GatherState.MoveToMine:
                        TickMoveToStoneMine(ref job, i);
                        break;
                    case GatherState.Gather:
                        TickGatherStone(ref job, i, fixedDeltaTime);
                        break;
                    case GatherState.MoveToDeposit:
                        TickMoveStoneToDeposit(ref job, i);
                        break;
                }
            }
        }

        void TickMoveToGoldMine(ref GoldGatherJob job, int index)
        {
            if (job.mine == null || job.mine.IsDepleted)
            {
                BeginGoldMoveToDeposit(ref job, index);
                return;
            }

            Vector3 gatherPosition = GetGatherPosition(job.mine, job.unit);
            if (job.unit.IsNear(gatherPosition, GatherReachDistance))
            {
                job.unit.ClearMoveTarget();
                job.state = GatherState.Gather;
                goldJobs[index] = job;
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(gatherPosition);
        }

        void TickGatherGold(ref GoldGatherJob job, int index, float deltaTime)
        {
            if (job.mine == null || job.mine.IsDepleted)
            {
                BeginGoldMoveToDeposit(ref job, index);
                return;
            }

            float request = GatherRate * deltaTime;
            float room = CarryCapacity - job.carriedAmount;
            float taken = job.mine.TakeMineral(Mathf.Min(request, room));
            job.carriedAmount += taken;

            if (job.carriedAmount >= CarryCapacity || job.mine.IsDepleted)
                BeginGoldMoveToDeposit(ref job, index);
            else
                goldJobs[index] = job;
        }

        void BeginGoldMoveToDeposit(ref GoldGatherJob job, int index)
        {
            if (job.carriedAmount <= 0f || ProductionManager.GetTownCenterForTeam(job.unit.Team) == null)
            {
                job.unit.ClearMoveTarget();
                goldJobs.RemoveAt(index);
                return;
            }

            job.state = GatherState.MoveToDeposit;
            job.unit.SetMoveTarget(GetDepositPosition(job.unit));
            goldJobs[index] = job;
        }

        void TickMoveGoldToDeposit(ref GoldGatherJob job, int index)
        {
            Vector3 depositPosition = GetDepositPosition(job.unit);
            if (depositPosition == Vector3.zero)
            {
                goldJobs.RemoveAt(index);
                return;
            }

            if (job.unit.IsNear(depositPosition, DepositReachDistance))
            {
                ResourceManager.AddGold(job.unit.Team, job.carriedAmount);
                job.carriedAmount = 0f;

                if (job.mine != null && !job.mine.IsDepleted)
                {
                    job.state = GatherState.MoveToMine;
                    job.unit.SetMoveTarget(GetGatherPosition(job.mine, job.unit));
                    goldJobs[index] = job;
                    return;
                }

                job.unit.ClearMoveTarget();
                goldJobs.RemoveAt(index);
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(depositPosition);
        }

        void TickMoveToStoneMine(ref StoneGatherJob job, int index)
        {
            if (job.mine == null || job.mine.IsDepleted)
            {
                BeginStoneMoveToDeposit(ref job, index);
                return;
            }

            Vector3 gatherPosition = GetGatherPosition(job.mine, job.unit);
            if (job.unit.IsNear(gatherPosition, GatherReachDistance))
            {
                job.unit.ClearMoveTarget();
                job.state = GatherState.Gather;
                stoneJobs[index] = job;
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(gatherPosition);
        }

        void TickGatherStone(ref StoneGatherJob job, int index, float deltaTime)
        {
            if (job.mine == null || job.mine.IsDepleted)
            {
                BeginStoneMoveToDeposit(ref job, index);
                return;
            }

            float request = GatherRate * deltaTime;
            float room = CarryCapacity - job.carriedAmount;
            float taken = job.mine.TakeMineral(Mathf.Min(request, room));
            job.carriedAmount += taken;

            if (job.carriedAmount >= CarryCapacity || job.mine.IsDepleted)
                BeginStoneMoveToDeposit(ref job, index);
            else
                stoneJobs[index] = job;
        }

        void BeginStoneMoveToDeposit(ref StoneGatherJob job, int index)
        {
            if (job.carriedAmount <= 0f || ProductionManager.GetTownCenterForTeam(job.unit.Team) == null)
            {
                job.unit.ClearMoveTarget();
                stoneJobs.RemoveAt(index);
                return;
            }

            job.state = GatherState.MoveToDeposit;
            job.unit.SetMoveTarget(GetDepositPosition(job.unit));
            stoneJobs[index] = job;
        }

        void TickMoveStoneToDeposit(ref StoneGatherJob job, int index)
        {
            Vector3 depositPosition = GetDepositPosition(job.unit);
            if (depositPosition == Vector3.zero)
            {
                stoneJobs.RemoveAt(index);
                return;
            }

            if (job.unit.IsNear(depositPosition, DepositReachDistance))
            {
                ResourceManager.AddStone(job.unit.Team, job.carriedAmount);
                job.carriedAmount = 0f;

                if (job.mine != null && !job.mine.IsDepleted)
                {
                    job.state = GatherState.MoveToMine;
                    job.unit.SetMoveTarget(GetGatherPosition(job.mine, job.unit));
                    stoneJobs[index] = job;
                    return;
                }

                job.unit.ClearMoveTarget();
                stoneJobs.RemoveAt(index);
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(depositPosition);
        }

        static Vector3 GetGatherPosition(GoldMineResource mine, Unit unit)
        {
            if (mine == null)
                return Vector3.zero;

            Vector3 center = mine.GetGatherPosition();
            return UnitPositionOffsets.ApplyRingOffset(center, unit, GatherStandRadius);
        }

        static Vector3 GetGatherPosition(StoneMineResource mine, Unit unit)
        {
            if (mine == null)
                return Vector3.zero;

            Vector3 center = mine.GetGatherPosition();
            return UnitPositionOffsets.ApplyRingOffset(center, unit, GatherStandRadius);
        }

        static Vector3 GetDepositPosition(Unit unit)
        {
            if (unit == null)
                return Vector3.zero;

            TownCenter townCenter = ProductionManager.GetTownCenterForTeam(unit.Team);
            if (townCenter == null)
                return Vector3.zero;

            Vector3 center = townCenter.transform.position;
            center.y = 1f;
            return UnitPositionOffsets.ApplyRingOffset(center, unit, DepositStandRadius);
        }
    }
}

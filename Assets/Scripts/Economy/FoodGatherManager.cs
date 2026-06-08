using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class FoodGatherManager : MonoBehaviour, ISimulationTickable
    {
        enum GatherState
        {
            MoveToBush,
            Gather,
            MoveToDeposit
        }

        struct FoodGatherJob
        {
            public Unit unit;
            public BerryBushResource bush;
            public GatherState state;
            public float carriedFood;
        }

        const float CarryCapacity = 10f;
        const float GatherRate = 2.5f;
        const float GatherReachDistance = 2.5f;
        const float DepositReachDistance = 5f;
        const float GatherStandRadius = 2f;
        const float DepositStandRadius = 3.5f;

        static FoodGatherManager instance;
        readonly List<FoodGatherJob> jobs = new List<FoodGatherJob>();

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
            if (jobs.Count == 0)
                return;

            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                FoodGatherJob job = jobs[i];
                if (job.unit == null || !job.unit.IsAlive)
                {
                    jobs.RemoveAt(i);
                    continue;
                }

                if (job.bush == null || job.bush.IsDepleted && job.carriedFood <= 0f)
                {
                    job.unit.ClearMoveTarget();
                    jobs.RemoveAt(i);
                    continue;
                }

                switch (job.state)
                {
                    case GatherState.MoveToBush:
                        TickMoveToBush(ref job, i);
                        break;
                    case GatherState.Gather:
                        TickGather(ref job, i, fixedDeltaTime);
                        break;
                    case GatherState.MoveToDeposit:
                        TickMoveToDeposit(ref job, i);
                        break;
                }
            }
        }

        public static void IssueGatherCommand(IReadOnlyList<Unit> units, BerryBushResource bush)
        {
            if (instance == null || bush == null || bush.IsDepleted)
                return;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || ProductionManager.GetTownCenterForTeam(unit.Team) == null)
                    continue;

                instance.RemoveJobForUnit(unit);
                instance.jobs.Add(new FoodGatherJob
                {
                    unit = unit,
                    bush = bush,
                    state = GatherState.MoveToBush,
                    carriedFood = 0f
                });
                unit.SetMoveTarget(GetGatherPosition(bush, unit));
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
            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                if (jobs[i].unit == unit)
                    jobs.RemoveAt(i);
            }
        }

        void TickMoveToBush(ref FoodGatherJob job, int index)
        {
            if (job.bush.IsDepleted)
            {
                BeginMoveToDeposit(ref job, index);
                return;
            }

            Vector3 gatherPosition = GetGatherPosition(job.bush, job.unit);
            if (job.unit.IsNear(gatherPosition, GatherReachDistance))
            {
                job.unit.ClearMoveTarget();
                job.state = GatherState.Gather;
                jobs[index] = job;
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(gatherPosition);
        }

        void TickGather(ref FoodGatherJob job, int index, float deltaTime)
        {
            if (job.bush.IsDepleted)
            {
                BeginMoveToDeposit(ref job, index);
                return;
            }

            float request = GatherRate * deltaTime;
            float room = CarryCapacity - job.carriedFood;
            float taken = job.bush.TakeFood(Mathf.Min(request, room));
            job.carriedFood += taken;

            if (job.carriedFood >= CarryCapacity || job.bush.IsDepleted)
                BeginMoveToDeposit(ref job, index);
            else
                jobs[index] = job;
        }

        void BeginMoveToDeposit(ref FoodGatherJob job, int index)
        {
            if (job.carriedFood <= 0f || ProductionManager.GetTownCenterForTeam(job.unit.Team) == null)
            {
                job.unit.ClearMoveTarget();
                jobs.RemoveAt(index);
                return;
            }

            job.state = GatherState.MoveToDeposit;
            job.unit.SetMoveTarget(GetDepositPosition(job.unit));
            jobs[index] = job;
        }

        void TickMoveToDeposit(ref FoodGatherJob job, int index)
        {
            Vector3 depositPosition = GetDepositPosition(job.unit);
            if (depositPosition == Vector3.zero)
            {
                jobs.RemoveAt(index);
                return;
            }

            if (job.unit.IsNear(depositPosition, DepositReachDistance))
            {
                ResourceManager.AddFood(job.unit.Team, job.carriedFood);
                job.unit.ClearMoveTarget();
                jobs.RemoveAt(index);
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(depositPosition);
        }

        static Vector3 GetGatherPosition(BerryBushResource bush, Unit unit)
        {
            if (bush == null)
                return Vector3.zero;

            Vector3 center = bush.GetGatherPosition();
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

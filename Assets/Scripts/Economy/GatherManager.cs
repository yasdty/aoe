using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class GatherManager : MonoBehaviour
    {
        enum GatherState
        {
            MoveToTree,
            Gather,
            MoveToDeposit
        }

        struct GatherJob
        {
            public Unit unit;
            public TreeResource tree;
            public GatherState state;
            public float carriedWood;
        }

        const float CarryCapacity = 10f;
        const float GatherRate = 2.5f;
        const float GatherReachDistance = 2.5f;
        const float DepositReachDistance = 5f;
        const float GatherStandRadius = 2f;
        const float DepositStandRadius = 3.5f;

        static GatherManager instance;
        readonly List<GatherJob> jobs = new List<GatherJob>();

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static bool IsUnitGathering(Unit unit)
        {
            if (instance == null || unit == null)
                return false;

            for (int i = 0; i < instance.jobs.Count; i++)
            {
                if (instance.jobs[i].unit == unit)
                    return true;
            }

            return false;
        }

        public static void IssueGatherCommand(IReadOnlyList<Unit> units, TreeResource tree)
        {
            if (instance == null || tree == null || tree.IsDepleted)
                return;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (unit == null || ProductionManager.GetTownCenterForTeam(unit.Team) == null)
                    continue;

                instance.RemoveJobForUnit(unit);
                instance.jobs.Add(new GatherJob
                {
                    unit = unit,
                    tree = tree,
                    state = GatherState.MoveToTree,
                    carriedWood = 0f
                });
                unit.SetMoveTarget(GetGatherPosition(tree, unit));
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

        void LateUpdate()
        {
            if (jobs.Count == 0)
                return;

            float deltaTime = Time.deltaTime;
            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                GatherJob job = jobs[i];
                if (job.unit == null || !job.unit.IsAlive)
                {
                    jobs.RemoveAt(i);
                    continue;
                }

                if (job.tree == null || job.tree.IsDepleted && job.carriedWood <= 0f)
                {
                    job.unit.ClearMoveTarget();
                    jobs.RemoveAt(i);
                    continue;
                }

                switch (job.state)
                {
                    case GatherState.MoveToTree:
                        TickMoveToTree(ref job, i);
                        break;
                    case GatherState.Gather:
                        TickGather(ref job, i, deltaTime);
                        break;
                    case GatherState.MoveToDeposit:
                        TickMoveToDeposit(ref job, i);
                        break;
                }
            }
        }

        void TickMoveToTree(ref GatherJob job, int index)
        {
            if (job.tree.IsDepleted)
            {
                BeginMoveToDeposit(ref job, index);
                return;
            }

            Vector3 gatherPosition = GetGatherPosition(job.tree, job.unit);
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

        void TickGather(ref GatherJob job, int index, float deltaTime)
        {
            if (job.tree.IsDepleted)
            {
                BeginMoveToDeposit(ref job, index);
                return;
            }

            float request = GatherRate * deltaTime;
            float room = CarryCapacity - job.carriedWood;
            float taken = job.tree.TakeWood(Mathf.Min(request, room));
            job.carriedWood += taken;

            if (job.carriedWood >= CarryCapacity || job.tree.IsDepleted)
                BeginMoveToDeposit(ref job, index);
            else
                jobs[index] = job;
        }

        void BeginMoveToDeposit(ref GatherJob job, int index)
        {
            if (job.carriedWood <= 0f || ProductionManager.GetTownCenterForTeam(job.unit.Team) == null)
            {
                job.unit.ClearMoveTarget();
                jobs.RemoveAt(index);
                return;
            }

            job.state = GatherState.MoveToDeposit;
            job.unit.SetMoveTarget(GetDepositPosition(job.unit));
            jobs[index] = job;
        }

        void TickMoveToDeposit(ref GatherJob job, int index)
        {
            Vector3 depositPosition = GetDepositPosition(job.unit);
            if (depositPosition == Vector3.zero)
            {
                jobs.RemoveAt(index);
                return;
            }

            if (job.unit.IsNear(depositPosition, DepositReachDistance))
            {
                ResourceManager.AddWood(job.unit.Team, job.carriedWood);
                job.unit.ClearMoveTarget();
                jobs.RemoveAt(index);
                return;
            }

            if (!job.unit.HasMoveTarget)
                job.unit.SetMoveTarget(depositPosition);
        }

        static Vector3 GetGatherPosition(TreeResource tree, Unit unit)
        {
            if (tree == null)
                return Vector3.zero;

            Vector3 center = tree.GetGatherPosition();
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

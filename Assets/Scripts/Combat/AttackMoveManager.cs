using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Selection;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Combat
{
    public class AttackMoveManager : MonoBehaviour, ISimulationTickable
    {
        struct AttackMoveJob
        {
            public Unit unit;
            public Vector3 destination;
        }

        const float ArrivalRadius = 0.75f;

        static AttackMoveManager instance;
        readonly List<AttackMoveJob> activeJobs = new List<AttackMoveJob>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureExistsInPlayMode()
        {
            if (!Application.isPlaying || Object.FindAnyObjectByType<AttackMoveManager>() != null)
                return;

            GameObject systems = GameObject.Find("Systems");
            Transform parent = systems != null ? systems.transform : null;
            GameObject attackMoveObject = new GameObject("AttackMoveManager");
            if (parent != null)
                attackMoveObject.transform.SetParent(parent);

            attackMoveObject.AddComponent<AttackMoveManager>();
            Debug.Log("[AttackMove] AttackMoveManager was missing from the scene — created at runtime.");
        }

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
            if (activeJobs.Count == 0)
                return;

            for (int i = activeJobs.Count - 1; i >= 0; i--)
            {
                AttackMoveJob job = activeJobs[i];
                Unit unit = job.unit;
                if (!IsValidUnit(unit))
                {
                    activeJobs.RemoveAt(i);
                    continue;
                }

                if (unit.IsStandGround)
                    continue;

                if (AttackManager.IsUnitAttacking(unit) || BoarAttackManager.IsUnitAttackingBoar(unit))
                    continue;

                if (unit.IsNear(job.destination, ArrivalRadius))
                {
                    unit.ClearMoveTarget();
                    activeJobs.RemoveAt(i);
                    continue;
                }

                if (!unit.HasMoveTarget)
                    unit.SetMoveTarget(job.destination);
            }
        }

        public static bool IsUnitAttackMoving(Unit unit)
        {
            if (instance == null || unit == null)
                return false;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                if (instance.activeJobs[i].unit == unit)
                    return true;
            }

            return false;
        }

        public static void Register(IReadOnlyList<Unit> units, Vector3 center, float spacing)
        {
            if (instance == null || units == null || units.Count == 0)
                return;

            GroupMoveFormation.GetGridDimensions(units.Count, out int columns, out int rows);
            float centerColumn = (columns - 1) * 0.5f;
            float centerRow = (rows - 1) * 0.5f;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (!IsValidUnit(unit) || !unit.CanAttack)
                    continue;

                int column = i % columns;
                int row = i / columns;
                Vector3 offset = new Vector3(
                    (column - centerColumn) * spacing,
                    0f,
                    (row - centerRow) * spacing);
                Vector3 destination = center + offset;

                RemoveJobForUnit(unit);
                instance.activeJobs.Add(new AttackMoveJob
                {
                    unit = unit,
                    destination = destination
                });

                if (!unit.IsStandGround)
                    unit.SetMoveTarget(destination);
            }
        }

        public static void CancelForUnits(IReadOnlyList<Unit> units)
        {
            if (instance == null || units == null || units.Count == 0)
                return;

            for (int i = instance.activeJobs.Count - 1; i >= 0; i--)
            {
                Unit unit = instance.activeJobs[i].unit;
                if (unit == null)
                {
                    instance.activeJobs.RemoveAt(i);
                    continue;
                }

                for (int u = 0; u < units.Count; u++)
                {
                    if (units[u] != unit)
                        continue;

                    instance.activeJobs.RemoveAt(i);
                    break;
                }
            }
        }

        public static void CancelForUnit(Unit unit)
        {
            if (instance == null || unit == null)
                return;

            RemoveJobForUnit(unit);
        }

        static void RemoveJobForUnit(Unit unit)
        {
            for (int i = instance.activeJobs.Count - 1; i >= 0; i--)
            {
                if (instance.activeJobs[i].unit == unit)
                    instance.activeJobs.RemoveAt(i);
            }
        }

        static bool IsValidUnit(Unit unit)
        {
            return unit != null && unit.IsAlive;
        }
    }
}

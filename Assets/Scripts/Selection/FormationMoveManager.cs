using System.Collections.Generic;
using AoE.RTS.Combat;
using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public class FormationMoveManager : MonoBehaviour, ISimulationTickable
    {
        struct FormationSlot
        {
            public Unit unit;
            public Vector3 slotOffset;
        }

        sealed class FormationJob
        {
            public readonly List<FormationSlot> slots = new List<FormationSlot>(16);
            public Vector3 formationCenter;
            public Vector3 destinationCenter;
            public bool isAttackMove;
        }

        const float ArrivalRadius = 0.75f;
        const float CenterArrivalRadius = 0.5f;

        static FormationMoveManager instance;
        readonly List<FormationJob> activeJobs = new List<FormationJob>(4);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsureExistsInPlayMode()
        {
            if (!Application.isPlaying || Object.FindAnyObjectByType<FormationMoveManager>() != null)
                return;

            GameObject systems = GameObject.Find("Systems");
            Transform parent = systems != null ? systems.transform : null;
            GameObject formationObject = new GameObject("FormationMoveManager");
            if (parent != null)
                formationObject.transform.SetParent(parent);

            formationObject.AddComponent<FormationMoveManager>();
            Debug.Log("[Formation] FormationMoveManager was missing from the scene — created at runtime.");
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

            for (int jobIndex = activeJobs.Count - 1; jobIndex >= 0; jobIndex--)
            {
                FormationJob job = activeJobs[jobIndex];
                AdvanceFormationCenter(job, fixedDeltaTime);

                bool allArrived = true;
                for (int slotIndex = job.slots.Count - 1; slotIndex >= 0; slotIndex--)
                {
                    FormationSlot slot = job.slots[slotIndex];
                    Unit unit = slot.unit;
                    if (!IsValidUnit(unit))
                    {
                        job.slots.RemoveAt(slotIndex);
                        continue;
                    }

                    Vector3 finalTarget = FlattenY(job.destinationCenter + slot.slotOffset);
                    if (!unit.IsNear(finalTarget, ArrivalRadius))
                        allArrived = false;

                    if (ShouldSkipMoveUpdate(unit, job.isAttackMove))
                        continue;

                    Vector3 target = FlattenY(job.formationCenter + slot.slotOffset);
                    target.y = unit.transform.position.y;
                    unit.SetMoveTarget(target);
                }

                if (job.slots.Count == 0)
                {
                    activeJobs.RemoveAt(jobIndex);
                    continue;
                }

                if (allArrived && IsCenterNearDestination(job))
                    activeJobs.RemoveAt(jobIndex);
            }
        }

        public static void Register(IReadOnlyList<Unit> units, Vector3 destination, float spacing, bool isAttackMove = false)
        {
            if (instance == null || units == null || units.Count == 0)
                return;

            CancelForUnits(units);

            FormationJob job = new FormationJob
            {
                destinationCenter = FlattenY(destination),
                formationCenter = ComputeCentroid(units),
                isAttackMove = isAttackMove
            };

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (!IsValidUnit(unit))
                    continue;

                if (isAttackMove && !unit.CanAttack)
                    continue;

                GroupMoveFormation.TryGetSlotOffset(i, units.Count, spacing, out Vector3 offset);
                job.slots.Add(new FormationSlot
                {
                    unit = unit,
                    slotOffset = offset
                });

                if (!ShouldSkipMoveUpdate(unit, isAttackMove))
                {
                    Vector3 target = FlattenY(job.formationCenter + offset);
                    target.y = unit.transform.position.y;
                    unit.SetMoveTarget(target);
                }
            }

            if (job.slots.Count > 0)
                instance.activeJobs.Add(job);
        }

        public static bool IsUnitInFormation(Unit unit)
        {
            if (instance == null || unit == null)
                return false;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                FormationJob job = instance.activeJobs[i];
                for (int s = 0; s < job.slots.Count; s++)
                {
                    if (job.slots[s].unit == unit)
                        return true;
                }
            }

            return false;
        }

        public static bool IsUnitAttackMoving(Unit unit)
        {
            if (instance == null || unit == null)
                return false;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                FormationJob job = instance.activeJobs[i];
                if (!job.isAttackMove)
                    continue;

                for (int s = 0; s < job.slots.Count; s++)
                {
                    if (job.slots[s].unit == unit)
                        return true;
                }
            }

            return false;
        }

        public static void CancelForUnits(IReadOnlyList<Unit> units)
        {
            if (instance == null || units == null || units.Count == 0)
                return;

            for (int jobIndex = instance.activeJobs.Count - 1; jobIndex >= 0; jobIndex--)
            {
                FormationJob job = instance.activeJobs[jobIndex];
                for (int slotIndex = job.slots.Count - 1; slotIndex >= 0; slotIndex--)
                {
                    Unit slotUnit = job.slots[slotIndex].unit;
                    if (slotUnit == null)
                    {
                        job.slots.RemoveAt(slotIndex);
                        continue;
                    }

                    for (int u = 0; u < units.Count; u++)
                    {
                        if (units[u] != slotUnit)
                            continue;

                        job.slots.RemoveAt(slotIndex);
                        break;
                    }
                }

                if (job.slots.Count == 0)
                    instance.activeJobs.RemoveAt(jobIndex);
            }
        }

        public static void CancelForUnit(Unit unit)
        {
            if (instance == null || unit == null)
                return;

            for (int jobIndex = instance.activeJobs.Count - 1; jobIndex >= 0; jobIndex--)
            {
                FormationJob job = instance.activeJobs[jobIndex];
                for (int slotIndex = job.slots.Count - 1; slotIndex >= 0; slotIndex--)
                {
                    if (job.slots[slotIndex].unit != unit)
                        continue;

                    job.slots.RemoveAt(slotIndex);
                    break;
                }

                if (job.slots.Count == 0)
                    instance.activeJobs.RemoveAt(jobIndex);
            }
        }

        static void AdvanceFormationCenter(FormationJob job, float deltaTime)
        {
            if (IsCenterNearDestination(job))
            {
                job.formationCenter = job.destinationCenter;
                return;
            }

            float minSpeed = GetMinMoveSpeed(job);
            Vector3 center = job.formationCenter;
            Vector3 destination = job.destinationCenter;
            Vector3 toDestination = destination - center;
            toDestination.y = 0f;

            float distance = toDestination.magnitude;
            float step = minSpeed * deltaTime;
            if (distance <= step)
                job.formationCenter = destination;
            else
                job.formationCenter = center + toDestination / distance * step;
        }

        static float GetMinMoveSpeed(FormationJob job)
        {
            float minSpeed = float.MaxValue;
            for (int i = 0; i < job.slots.Count; i++)
            {
                Unit unit = job.slots[i].unit;
                if (!IsValidUnit(unit))
                    continue;

                if (ShouldSkipMoveUpdate(unit, job.isAttackMove))
                    continue;

                float speed = unit.Data != null ? unit.Data.moveSpeed : 5f;
                if (speed < minSpeed)
                    minSpeed = speed;
            }

            return minSpeed < float.MaxValue ? minSpeed : 5f;
        }

        static bool IsCenterNearDestination(FormationJob job)
        {
            Vector3 delta = job.destinationCenter - job.formationCenter;
            delta.y = 0f;
            return delta.sqrMagnitude <= CenterArrivalRadius * CenterArrivalRadius;
        }

        static bool ShouldSkipMoveUpdate(Unit unit, bool isAttackMove)
        {
            if (isAttackMove && unit.IsStandGround)
                return true;

            if (AttackManager.IsUnitAttacking(unit) || BoarAttackManager.IsUnitAttackingBoar(unit))
                return true;

            return false;
        }

        static Vector3 ComputeCentroid(IReadOnlyList<Unit> units)
        {
            Vector3 sum = Vector3.zero;
            int count = 0;
            float y = 1f;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unit = units[i];
                if (!IsValidUnit(unit))
                    continue;

                sum += unit.transform.position;
                y = unit.transform.position.y;
                count++;
            }

            if (count == 0)
                return new Vector3(0f, y, 0f);

            sum /= count;
            sum.y = y;
            return sum;
        }

        static Vector3 FlattenY(Vector3 position)
        {
            return new Vector3(position.x, 0f, position.z);
        }

        static bool IsValidUnit(Unit unit)
        {
            return unit != null && unit.IsAlive;
        }
    }
}

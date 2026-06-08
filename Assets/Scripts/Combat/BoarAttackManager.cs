using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Combat
{
    public class BoarAttackManager : MonoBehaviour, ISimulationTickable
    {
        struct BoarAttackJob
        {
            public Unit attacker;
            public BoarResource boar;
            public float cooldownRemaining;
        }

        static BoarAttackManager instance;
        static readonly List<Unit> gatherCancelBuffer = new List<Unit>(1);
        readonly List<BoarAttackJob> activeJobs = new List<BoarAttackJob>();

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
                if (i >= activeJobs.Count)
                    continue;

                BoarAttackJob job = activeJobs[i];
                if (!IsValidAttacker(job.attacker))
                {
                    Unit staleAttacker = job.attacker;
                    activeJobs.RemoveAt(i);
                    if (staleAttacker != null)
                        staleAttacker.NotifyStateChanged();
                    continue;
                }

                if (!IsValidBoarTarget(job.boar))
                {
                    Unit staleAttacker = job.attacker;
                    activeJobs.RemoveAt(i);
                    if (staleAttacker != null)
                        staleAttacker.NotifyStateChanged();
                    continue;
                }

                if (!ProcessBoarAttackJob(ref job, i, fixedDeltaTime))
                    continue;
            }

            RefreshAttackerVisuals();
        }

        bool ProcessBoarAttackJob(ref BoarAttackJob job, int index, float deltaTime)
        {
            Unit attacker = job.attacker;
            BoarResource boar = job.boar;
            Vector3 targetPosition = boar.GetGatherPosition();
            float attackRange = attacker.AttackRange;

            if (!attacker.IsNear(targetPosition, attackRange))
            {
                if (!JobStillActive(index, attacker))
                    return false;

                float standRadius = Mathf.Max(0.5f, attackRange * 0.85f);
                Vector3 approachPosition = UnitPositionOffsets.ApplyRingOffset(
                    targetPosition,
                    attacker,
                    standRadius);
                attacker.SetMoveTarget(approachPosition);
                job.attacker = attacker;
                job.boar = boar;
                activeJobs[index] = job;
                return true;
            }

            attacker.ClearMoveTarget();
            job.cooldownRemaining -= deltaTime;
            if (job.cooldownRemaining > 0f)
            {
                if (!JobStillActive(index, attacker))
                    return false;

                job.attacker = attacker;
                job.boar = boar;
                activeJobs[index] = job;
                return true;
            }

            float damage = attacker.AttackPower;
            boar.ApplyAttackDamage(damage, attacker);

            if (!JobStillActive(index, attacker))
                return false;

            if (!IsValidAttacker(attacker))
            {
                activeJobs.RemoveAt(index);
                return false;
            }

            if (!IsValidBoarTarget(boar))
            {
                activeJobs.RemoveAt(index);
                attacker.NotifyStateChanged();
                return false;
            }

            job.cooldownRemaining = attacker.AttackCooldownSeconds;
            job.attacker = attacker;
            job.boar = boar;
            activeJobs[index] = job;
            return true;
        }

        void RefreshAttackerVisuals()
        {
            for (int i = 0; i < activeJobs.Count; i++)
            {
                Unit attacker = activeJobs[i].attacker;
                if (attacker != null)
                    attacker.NotifyStateChanged();
            }
        }

        static bool IsValidAttacker(Unit unit)
        {
            return unit != null && unit.IsAlive && unit.CanAttack;
        }

        static bool IsValidBoarTarget(BoarResource boar)
        {
            return boar != null
                && boar.gameObject.activeInHierarchy
                && !boar.IsDead;
        }

        bool JobStillActive(int index, Unit attacker)
        {
            return index < activeJobs.Count && activeJobs[index].attacker == attacker;
        }

        public static bool IsUnitAttackingBoar(Unit unit)
        {
            if (instance == null || unit == null)
                return false;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                if (instance.activeJobs[i].attacker == unit)
                    return true;
            }

            return false;
        }

        public static void IssueAttack(IReadOnlyList<Unit> attackers, BoarResource boar)
        {
            if (instance == null || boar == null || attackers == null || boar.IsDead)
                return;

            for (int i = 0; i < attackers.Count; i++)
            {
                Unit attacker = attackers[i];
                if (!IsValidAttacker(attacker))
                    continue;

                gatherCancelBuffer.Clear();
                gatherCancelBuffer.Add(attacker);
                GatherManager.CancelForUnits(gatherCancelBuffer);
                FoodGatherManager.CancelForUnits(gatherCancelBuffer);
                MineralGatherManager.CancelForUnits(gatherCancelBuffer);
                AttackManager.CancelForUnits(gatherCancelBuffer);
                RemoveJobsForAttacker(attacker);
                instance.activeJobs.Add(new BoarAttackJob
                {
                    attacker = attacker,
                    boar = boar,
                    cooldownRemaining = 0f
                });
            }
        }

        public static void CancelJobsForUnit(Unit unit)
        {
            if (instance == null || unit == null)
                return;

            for (int i = instance.activeJobs.Count - 1; i >= 0; i--)
            {
                BoarAttackJob job = instance.activeJobs[i];
                if (job.attacker == unit)
                {
                    Unit staleAttacker = job.attacker;
                    instance.activeJobs.RemoveAt(i);
                    if (staleAttacker != null)
                        staleAttacker.NotifyStateChanged();
                }
            }
        }

        public static void CancelForUnits(IReadOnlyList<Unit> units)
        {
            if (instance == null || units == null || units.Count == 0)
                return;

            for (int i = instance.activeJobs.Count - 1; i >= 0; i--)
            {
                Unit attacker = instance.activeJobs[i].attacker;
                if (attacker == null)
                {
                    instance.activeJobs.RemoveAt(i);
                    continue;
                }

                for (int u = 0; u < units.Count; u++)
                {
                    if (units[u] != attacker)
                        continue;

                    instance.activeJobs.RemoveAt(i);
                    attacker.NotifyStateChanged();
                    break;
                }
            }
        }

        static void RemoveJobsForAttacker(Unit attacker)
        {
            for (int i = instance.activeJobs.Count - 1; i >= 0; i--)
            {
                if (instance.activeJobs[i].attacker == attacker)
                    instance.activeJobs.RemoveAt(i);
            }
        }
    }
}

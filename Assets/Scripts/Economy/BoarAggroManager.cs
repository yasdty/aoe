using System.Collections.Generic;
using AoE.RTS.Combat;
using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class BoarAggroManager : MonoBehaviour, ISimulationTickable
    {
        const float MaxChaseDistance = 18f;
        const float LoseAggroSeconds = 4f;

        struct AggroJob
        {
            public BoarResource boar;
            public Unit target;
            public float cooldownRemaining;
            public float outOfRangeTimer;
        }

        static BoarAggroManager instance;
        readonly List<AggroJob> activeJobs = new List<AggroJob>();

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

                AggroJob job = activeJobs[i];
                if (!IsValidBoar(job.boar))
                {
                    activeJobs.RemoveAt(i);
                    continue;
                }

                if (!IsValidTarget(job.target))
                {
                    activeJobs.RemoveAt(i);
                    continue;
                }

                Vector3 targetPosition = job.target.transform.position;
                float distance = HorizontalDistance(job.boar.transform.position, targetPosition);
                if (distance > MaxChaseDistance)
                {
                    job.outOfRangeTimer += fixedDeltaTime;
                    if (job.outOfRangeTimer >= LoseAggroSeconds)
                    {
                        activeJobs.RemoveAt(i);
                        continue;
                    }
                }
                else
                {
                    job.outOfRangeTimer = 0f;
                }

                float attackRange = job.boar.AttackRange;
                if (distance > attackRange)
                {
                    MoveBoarToward(job.boar, targetPosition, fixedDeltaTime);
                    job.cooldownRemaining = job.boar.AttackCooldownSeconds;
                    activeJobs[i] = job;
                    continue;
                }

                job.cooldownRemaining -= fixedDeltaTime;
                if (job.cooldownRemaining <= 0f)
                {
                    CombatDamageBreakdown breakdown =
                        CombatDamageResolver.ResolveMeleeAttack(job.boar.AttackPower, job.target);
                    job.target.TakeDamage(breakdown.totalDamage);
                    job.cooldownRemaining = job.boar.AttackCooldownSeconds;
                }

                activeJobs[i] = job;
            }
        }

        public static void NotifyHunted(BoarResource boar, Unit target)
        {
            UpsertAggroJob(boar, target);
        }

        public static void NotifyAttacked(BoarResource boar, Unit target)
        {
            UpsertAggroJob(boar, target);
        }

        public static void NotifyDepleted(BoarResource boar)
        {
            if (instance == null || boar == null)
                return;

            for (int i = instance.activeJobs.Count - 1; i >= 0; i--)
            {
                if (instance.activeJobs[i].boar == boar)
                    instance.activeJobs.RemoveAt(i);
            }
        }

        static void UpsertAggroJob(BoarResource boar, Unit target)
        {
            if (instance == null || !IsValidBoar(boar) || !IsValidTarget(target))
                return;

            for (int i = 0; i < instance.activeJobs.Count; i++)
            {
                AggroJob job = instance.activeJobs[i];
                if (job.boar != boar)
                    continue;

                job.target = target;
                job.outOfRangeTimer = 0f;
                instance.activeJobs[i] = job;
                return;
            }

            instance.activeJobs.Add(new AggroJob
            {
                boar = boar,
                target = target,
                cooldownRemaining = 0f,
                outOfRangeTimer = 0f
            });
        }

        static void MoveBoarToward(BoarResource boar, Vector3 targetPosition, float deltaTime)
        {
            Vector3 current = boar.transform.position;
            Vector3 flatTarget = new Vector3(targetPosition.x, current.y, targetPosition.z);
            Vector3 toTarget = flatTarget - current;
            float distance = toTarget.magnitude;
            if (distance <= 0.001f)
                return;

            float step = boar.MoveSpeed * deltaTime;
            boar.transform.position = step >= distance
                ? flatTarget
                : current + toTarget.normalized * step;
        }

        static bool IsValidBoar(BoarResource boar)
        {
            return boar != null
                && boar.gameObject.activeInHierarchy
                && !boar.IsDead;
        }

        static bool IsValidTarget(Unit target)
        {
            return target != null && target.IsAlive;
        }

        static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}

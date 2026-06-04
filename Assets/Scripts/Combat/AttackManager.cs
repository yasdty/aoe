using System.Collections.Generic;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Combat
{
    public class AttackManager : MonoBehaviour
    {
        struct AttackJob
        {
            public Unit attacker;
            public Unit target;
            public float cooldownRemaining;
        }

        static AttackManager instance;
        static readonly List<Unit> gatherCancelBuffer = new List<Unit>(1);
        readonly List<AttackJob> activeJobs = new List<AttackJob>();

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        void Update()
        {
            if (activeJobs.Count == 0)
                return;

            float deltaTime = Time.deltaTime;
            for (int i = activeJobs.Count - 1; i >= 0; i--)
            {
                AttackJob job = activeJobs[i];
                if (job.attacker == null || job.target == null)
                {
                    activeJobs.RemoveAt(i);
                    continue;
                }

                if (job.attacker.Team == job.target.Team)
                {
                    activeJobs.RemoveAt(i);
                    continue;
                }

                float attackRange = job.attacker.AttackRange;
                if (!job.attacker.IsNear(job.target.transform.position, attackRange))
                {
                    job.attacker.SetMoveTarget(job.target.transform.position);
                    activeJobs[i] = job;
                    continue;
                }

                job.attacker.ClearMoveTarget();
                job.cooldownRemaining -= deltaTime;
                if (job.cooldownRemaining > 0f)
                {
                    activeJobs[i] = job;
                    continue;
                }

                float damage = Mathf.Max(1f, job.attacker.AttackPower - job.target.Armor);
                job.target.TakeDamage(damage);
                job.cooldownRemaining = job.attacker.AttackCooldownSeconds;
                activeJobs[i] = job;

                Debug.Log(
                    $"{job.attacker.Data?.displayName ?? "Unit"} hit {job.target.Data?.displayName ?? "Unit"} for {damage:0} (HP {job.target.CurrentHp:0}/{job.target.MaxHp:0})");
            }
        }

        public static void IssueAttack(IReadOnlyList<Unit> attackers, Unit target)
        {
            if (instance == null || target == null || attackers == null)
                return;

            for (int i = 0; i < attackers.Count; i++)
            {
                Unit attacker = attackers[i];
                if (attacker == null || !attacker.CanAttack)
                    continue;

                if (attacker.Team == target.Team)
                    continue;

                gatherCancelBuffer.Clear();
                gatherCancelBuffer.Add(attacker);
                GatherManager.CancelForUnits(gatherCancelBuffer);
                RemoveJobsForAttacker(attacker);
                instance.activeJobs.Add(new AttackJob
                {
                    attacker = attacker,
                    target = target,
                    cooldownRemaining = 0f
                });
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

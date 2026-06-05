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
                if (!IsValidCombatUnit(job.attacker) || !IsValidCombatUnit(job.target))
                {
                    Unit staleAttacker = job.attacker;
                    activeJobs.RemoveAt(i);
                    if (staleAttacker != null)
                        staleAttacker.NotifyStateChanged();
                    continue;
                }

                if (job.attacker.Team == job.target.Team)
                {
                    Unit staleAttacker = job.attacker;
                    activeJobs.RemoveAt(i);
                    staleAttacker.NotifyStateChanged();
                    continue;
                }

                float attackRange = job.attacker.AttackRange;
                Vector3 targetPosition = job.target.transform.position;
                if (!job.attacker.IsNear(targetPosition, attackRange))
                {
                    float standRadius = Mathf.Max(0.5f, attackRange * 0.85f);
                    Vector3 approachPosition = UnitPositionOffsets.ApplyRingOffset(
                        targetPosition,
                        job.attacker,
                        standRadius);
                    job.attacker.SetMoveTarget(approachPosition);
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
                Unit attacker = job.attacker;
                Unit target = job.target;
                target.TakeDamage(damage);

                if (i >= activeJobs.Count || activeJobs[i].attacker != attacker)
                    continue;

                if (!IsValidCombatUnit(attacker))
                {
                    activeJobs.RemoveAt(i);
                    continue;
                }

                if (!IsValidCombatUnit(target))
                {
                    activeJobs.RemoveAt(i);
                    attacker.NotifyStateChanged();
                    continue;
                }

                job.cooldownRemaining = attacker.AttackCooldownSeconds;
                job.attacker = attacker;
                job.target = target;
                activeJobs[i] = job;

                Debug.Log(
                    $"[{FormatTeam(attacker.Team)}] {attacker.Data?.displayName ?? "Unit"} "
                    + $"→ [{FormatTeam(target.Team)}] {target.Data?.displayName ?? "Unit"}: "
                    + $"{damage:0} dmg (HP {target.CurrentHp:0}/{target.MaxHp:0})");
            }

            RefreshAttackerVisuals();
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

        static bool IsValidCombatUnit(Unit unit)
        {
            return unit != null && unit.IsAlive;
        }

        public static bool IsUnitAttacking(Unit unit)
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

        public static void IssueAttack(IReadOnlyList<Unit> attackers, Unit target)
        {
            if (instance == null || target == null || attackers == null || !target.IsAlive)
                return;

            for (int i = 0; i < attackers.Count; i++)
            {
                Unit attacker = attackers[i];
                if (!IsValidCombatUnit(attacker) || !attacker.CanAttack)
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

        public static void CancelJobsForUnit(Unit unit)
        {
            if (instance == null || unit == null)
                return;

            for (int i = instance.activeJobs.Count - 1; i >= 0; i--)
            {
                AttackJob job = instance.activeJobs[i];
                if (job.attacker == unit || job.target == unit)
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

        static string FormatTeam(UnitTeam team)
        {
            return team == UnitTeam.Player ? "Player" : "CPU";
        }
    }
}

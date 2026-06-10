using System.Collections.Generic;
using UnityEngine;

namespace AoE.RTS.Units
{
    public static class UnitSeparation
    {
        const float MinSeparation = 1.2f;
        const float MaxPushStep = 0.12f;

        public static void Apply(IReadOnlyList<Unit> units, float deltaTime)
        {
            if (units == null || units.Count < 2)
                return;

            float pushStep = MaxPushStep * Mathf.Max(deltaTime / 0.05f, 1f);
            float minSepSqr = MinSeparation * MinSeparation;

            for (int i = 0; i < units.Count; i++)
            {
                Unit unitA = units[i];
                if (!CanSeparate(unitA))
                    continue;

                Vector3 positionA = unitA.transform.position;

                for (int j = i + 1; j < units.Count; j++)
                {
                    Unit unitB = units[j];
                    if (!CanSeparate(unitB) || unitA.Team != unitB.Team)
                        continue;

                    Vector3 positionB = unitB.transform.position;
                    Vector3 delta = positionB - positionA;
                    delta.y = 0f;

                    float distanceSqr = delta.sqrMagnitude;
                    if (distanceSqr >= minSepSqr || distanceSqr <= 0.0001f)
                        continue;

                    float distance = Mathf.Sqrt(distanceSqr);
                    float overlap = MinSeparation - distance;
                    Vector3 push = delta / distance * (overlap * 0.5f);
                    push = Vector3.ClampMagnitude(push, pushStep);

                    float y = positionA.y;
                    unitA.transform.position = new Vector3(
                        positionA.x - push.x,
                        y,
                        positionA.z - push.z);
                    unitB.transform.position = new Vector3(
                        positionB.x + push.x,
                        y,
                        positionB.z + push.z);

                    positionA = unitA.transform.position;
                }
            }
        }

        static bool CanSeparate(Unit unit)
        {
            return unit != null && unit.IsAlive && unit.CanAttack && unit.HasMoveTarget;
        }
    }
}

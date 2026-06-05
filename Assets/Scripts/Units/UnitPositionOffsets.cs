using UnityEngine;

namespace AoE.RTS.Units
{
    public static class UnitPositionOffsets
    {
        public const int SlotCount = 8;

        public static Vector3 ApplyRingOffset(Vector3 center, Unit unit, float radius)
        {
            if (unit == null || radius <= 0f)
                return center;

            int slot = unit.StandSlot % SlotCount;
            float angle = slot * (Mathf.PI * 2f / SlotCount);
            return new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                center.y,
                center.z + Mathf.Sin(angle) * radius);
        }
    }
}

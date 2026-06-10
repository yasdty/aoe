using System.Collections.Generic;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Selection
{
    public static class GroupMoveFormation
    {
        public static void AssignMoveTargets(IReadOnlyList<Unit> units, Vector3 center, float spacing = 2f)
        {
            if (units == null || units.Count == 0)
                return;

            for (int i = 0; i < units.Count; i++)
            {
                TryGetSlotOffset(i, units.Count, spacing, out Vector3 offset);
                units[i].SetMoveTarget(center + offset);
            }
        }

        public static bool TryGetSlotOffset(int index, int count, float spacing, out Vector3 offset)
        {
            offset = Vector3.zero;
            if (count <= 0 || index < 0 || index >= count)
                return false;

            GetGridDimensions(count, out int columns, out int rows);

            float centerColumn = (columns - 1) * 0.5f;
            float centerRow = (rows - 1) * 0.5f;
            int column = index % columns;
            int row = index / columns;

            offset = new Vector3(
                (column - centerColumn) * spacing,
                0f,
                (row - centerRow) * spacing);
            return true;
        }

        public static void GetGridDimensions(int count, out int columns, out int rows)
        {
            columns = Mathf.CeilToInt(Mathf.Sqrt(count));
            rows = Mathf.CeilToInt(count / (float)columns);
        }
    }
}

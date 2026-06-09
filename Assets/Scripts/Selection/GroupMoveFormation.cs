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

            GetGridDimensions(units.Count, out int columns, out int rows);

            float centerColumn = (columns - 1) * 0.5f;
            float centerRow = (rows - 1) * 0.5f;

            for (int i = 0; i < units.Count; i++)
            {
                int column = i % columns;
                int row = i / columns;

                Vector3 offset = new Vector3(
                    (column - centerColumn) * spacing,
                    0f,
                    (row - centerRow) * spacing);

                units[i].SetMoveTarget(center + offset);
            }
        }

        public static void GetGridDimensions(int count, out int columns, out int rows)
        {
            columns = Mathf.CeilToInt(Mathf.Sqrt(count));
            rows = Mathf.CeilToInt(count / (float)columns);
        }
    }
}

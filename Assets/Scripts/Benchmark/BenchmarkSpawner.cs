using System.Collections.Generic;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Benchmark
{
    public class BenchmarkSpawner : MonoBehaviour
    {
        const float UnitGroundY = 1f;
        const float GridSpacing = 2.5f;

        [SerializeField] UnitData villagerData;

        readonly List<Unit> activeUnits = new List<Unit>();

        public int ActiveUnitCount => activeUnits.Count;

        public void SpawnCount(int count)
        {
            count = Mathf.Max(0, count);
            ClearAll();

            for (int i = 0; i < count; i++)
            {
                Vector3 position = ResolveGridPosition(i, count);
                Unit unit = UnitPool.Rent(villagerData, position, UnitTeam.Player);
                activeUnits.Add(unit);
            }
        }

        public void ClearAll()
        {
            for (int i = activeUnits.Count - 1; i >= 0; i--)
            {
                Unit unit = activeUnits[i];
                if (unit != null)
                    UnitPool.Return(unit);
            }

            activeUnits.Clear();
        }

        static Vector3 ResolveGridPosition(int index, int totalCount)
        {
            int columns = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(totalCount)));
            int rows = Mathf.Max(1, Mathf.CeilToInt(totalCount / (float)columns));
            int row = index / columns;
            int column = index % columns;

            float centerOffsetX = (columns - 1) * GridSpacing * 0.5f;
            float centerOffsetZ = (rows - 1) * GridSpacing * 0.5f;
            return new Vector3(
                column * GridSpacing - centerOffsetX,
                UnitGroundY,
                row * GridSpacing - centerOffsetZ);
        }
    }
}

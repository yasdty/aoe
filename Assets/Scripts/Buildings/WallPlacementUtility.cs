using System.Collections.Generic;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    public static class WallPlacementUtility
    {
        public static float GetSegmentLength(PlacedBuildingData data)
        {
            if (data == null)
                return 4f;

            return Mathf.Max(data.footprintWidth, data.footprintDepth);
        }

        public static void GetWallLinePositions(
            Vector3 startWorld,
            Vector3 endWorld,
            PlacedBuildingData data,
            List<Vector3> results,
            out float orientationY)
        {
            results.Clear();
            float segmentLength = GetSegmentLength(data);
            Vector3 start = SnapToSegmentGrid(startWorld, segmentLength);
            Vector3 end = SnapToSegmentGrid(endWorld, segmentLength);

            float dx = end.x - start.x;
            float dz = end.z - start.z;

            if (Mathf.Abs(dx) >= Mathf.Abs(dz))
            {
                orientationY = 90f;
                int stepSign = dx >= 0f ? 1 : -1;
                int segmentCount = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(dx) / segmentLength) + 1);
                for (int i = 0; i < segmentCount; i++)
                {
                    results.Add(new Vector3(
                        start.x + i * stepSign * segmentLength,
                        0f,
                        start.z));
                }
            }
            else
            {
                orientationY = 0f;
                int stepSign = dz >= 0f ? 1 : -1;
                int segmentCount = Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(dz) / segmentLength) + 1);
                for (int i = 0; i < segmentCount; i++)
                {
                    results.Add(new Vector3(
                        start.x,
                        0f,
                        start.z + i * stepSign * segmentLength));
                }
            }
        }

        public static Vector3 SnapToSegmentGrid(Vector3 worldPoint, float segmentLength)
        {
            float step = Mathf.Max(1f, segmentLength);
            return new Vector3(
                Mathf.Round(worldPoint.x / step) * step,
                0f,
                Mathf.Round(worldPoint.z / step) * step);
        }
    }
}

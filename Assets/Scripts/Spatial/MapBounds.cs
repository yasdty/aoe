using UnityEngine;

namespace AoE.RTS.Spatial
{
    /// <summary>
    /// Phase10 地面 AABB — ワールド XZ ↔ ミニマップ UV の単一真実源。
    /// Unity Plane 10×10、scale (18,1,18)、position (0,0,-30) → X:-90..90, Z:-120..60。
    /// </summary>
    public static class MapBounds
    {
        const float PlaneHalfExtent = 5f;

        public const float Phase10MinX = -90f;
        public const float Phase10MaxX = 90f;
        public const float Phase10MinZ = -120f;
        public const float Phase10MaxZ = 60f;

        static float minX = Phase10MinX;
        static float maxX = Phase10MaxX;
        static float minZ = Phase10MinZ;
        static float maxZ = Phase10MaxZ;

        public static Rect WorldXZRect => new Rect(minX, minZ, maxX - minX, maxZ - minZ);

        public static void ConfigureFromGroundTransform(Transform groundTransform)
        {
            if (groundTransform == null)
                return;

            Vector3 scale = groundTransform.lossyScale;
            Vector3 position = groundTransform.position;
            float halfX = PlaneHalfExtent * scale.x;
            float halfZ = PlaneHalfExtent * scale.z;
            minX = position.x - halfX;
            maxX = position.x + halfX;
            minZ = position.z - halfZ;
            maxZ = position.z + halfZ;
        }

        public static void ResetToPhase10Defaults()
        {
            minX = Phase10MinX;
            maxX = Phase10MaxX;
            minZ = Phase10MinZ;
            maxZ = Phase10MaxZ;
        }

        public static Vector2 WorldToNormalized01(Vector3 worldPosition)
        {
            float u = Mathf.InverseLerp(minX, maxX, worldPosition.x);
            float v = Mathf.InverseLerp(minZ, maxZ, worldPosition.z);
            return new Vector2(Mathf.Clamp01(u), Mathf.Clamp01(v));
        }

        public static Vector3 Normalized01ToWorld(Vector2 uv, float y = 0f)
        {
            float x = Mathf.Lerp(minX, maxX, Mathf.Clamp01(uv.x));
            float z = Mathf.Lerp(minZ, maxZ, Mathf.Clamp01(uv.y));
            return new Vector3(x, y, z);
        }
    }
}

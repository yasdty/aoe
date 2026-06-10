using AoE.RTS.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Buildings
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class PalisadeWall : OrientedWallSegment
    {
        protected override WallOccupancyKind GetOccupancyKind() => WallOccupancyKind.Wall;

        protected override void EnsureDataReference()
        {
            if (data != null)
                return;

#if UNITY_EDITOR
            data = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultPalisadeWallData);
#endif
        }
    }
}

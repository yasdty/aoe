using AoE.RTS.Buildings;
using UnityEngine;

namespace AoE.RTS.View
{
    public readonly struct PlacementPreviewState
    {
        public readonly PlacedBuildingData data;
        public readonly Vector3 groundPosition;
        public readonly float wallOrientationY;
        public readonly bool valid;

        public PlacementPreviewState(
            PlacedBuildingData data,
            Vector3 groundPosition,
            float wallOrientationY,
            bool valid)
        {
            this.data = data;
            this.groundPosition = groundPosition;
            this.wallOrientationY = wallOrientationY;
            this.valid = valid;
        }
    }
}

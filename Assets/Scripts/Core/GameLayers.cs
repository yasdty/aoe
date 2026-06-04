namespace AoE.RTS.Core
{
    public static class GameLayers
    {
        public const string GroundLayerName = "Ground";
        public const string UnitLayerName = "Unit";
        public const string BuildingLayerName = "Building";
        public const string ResourceLayerName = "Resource";

        public static int GroundMask => UnityEngine.LayerMask.GetMask(GroundLayerName);
        public static int UnitMask => UnityEngine.LayerMask.GetMask(UnitLayerName);
        public static int BuildingMask => UnityEngine.LayerMask.GetMask(BuildingLayerName);
        public static int ResourceMask => UnityEngine.LayerMask.GetMask(ResourceLayerName);
    }
}

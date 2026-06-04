namespace AoE.RTS.Core
{
    public static class GameLayers
    {
        public const string GroundLayerName = "Ground";
        public const string UnitLayerName = "Unit";

        public static int GroundMask => UnityEngine.LayerMask.GetMask(GroundLayerName);
        public static int UnitMask => UnityEngine.LayerMask.GetMask(UnitLayerName);
    }
}

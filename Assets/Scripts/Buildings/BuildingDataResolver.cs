using AoE.RTS.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Buildings
{
    public static class BuildingDataResolver
    {
        public static BuildingData ResolveTownCenter(ref BuildingData cached)
        {
            if (cached != null)
                return cached;

#if UNITY_EDITOR
            cached = AssetDatabase.LoadAssetAtPath<BuildingData>(GameAssetPaths.TownCenterData);
#endif
            return cached;
        }
    }
}

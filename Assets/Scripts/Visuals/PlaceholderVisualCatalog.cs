using UnityEngine;

namespace AoE.RTS.Visuals
{
    [CreateAssetMenu(fileName = "PlaceholderVisualCatalog", menuName = "AoE/Placeholder Visual Catalog")]
    public class PlaceholderVisualCatalog : ScriptableObject
    {
        [SerializeField] GameObject villagerPrefab;
        [SerializeField] GameObject militiaPrefab;
        [SerializeField] GameObject townCenterPrefab;
        [SerializeField] GameObject housePrefab;
        [SerializeField] GameObject barracksPrefab;
        [SerializeField] GameObject treePrefab;

        public GameObject GetPrefab(PlaceholderVisualKind kind)
        {
            GameObject prefab = GetSerializedPrefab(kind);
            if (prefab != null)
                return prefab;

            return Resources.Load<GameObject>(GetResourcePath(kind));
        }

        GameObject GetSerializedPrefab(PlaceholderVisualKind kind)
        {
            switch (kind)
            {
                case PlaceholderVisualKind.Villager:
                    return villagerPrefab;
                case PlaceholderVisualKind.Militia:
                    return militiaPrefab;
                case PlaceholderVisualKind.TownCenter:
                    return townCenterPrefab;
                case PlaceholderVisualKind.House:
                    return housePrefab;
                case PlaceholderVisualKind.Barracks:
                    return barracksPrefab;
                case PlaceholderVisualKind.Tree:
                    return treePrefab;
                default:
                    return null;
            }
        }

        public static string GetResourcePath(PlaceholderVisualKind kind)
        {
            switch (kind)
            {
                case PlaceholderVisualKind.Villager:
                    return PlaceholderVisualPaths.VillagerResource;
                case PlaceholderVisualKind.Militia:
                    return PlaceholderVisualPaths.MilitiaResource;
                case PlaceholderVisualKind.TownCenter:
                    return PlaceholderVisualPaths.TownCenterResource;
                case PlaceholderVisualKind.House:
                    return PlaceholderVisualPaths.HouseResource;
                case PlaceholderVisualKind.Barracks:
                    return PlaceholderVisualPaths.BarracksResource;
                case PlaceholderVisualKind.Tree:
                    return PlaceholderVisualPaths.TreeResource;
                default:
                    return null;
            }
        }
    }
}

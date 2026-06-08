using AoE.RTS.Economy;
using UnityEngine;

namespace AoE.RTS.Spatial
{
    public class BerryBushSpatialIndex : MonoBehaviour
    {
        static BerryBushSpatialIndex instance;

        [SerializeField] float cellSize = 12f;
        [SerializeField] float defaultMaxSearchRadius = 256f;

        SpatialHashGrid<BerryBushResource> grid;

        static System.Func<BerryBushResource, Vector3> BushPosition =>
            static bush => bush.transform.position;

        void Awake()
        {
            instance = this;
            grid = new SpatialHashGrid<BerryBushResource>(cellSize);
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static void Register(BerryBushResource bush)
        {
            if (instance == null || bush == null || bush.IsDepleted)
                return;

            instance.grid.Insert(bush, bush.transform.position);
        }

        public static void Unregister(BerryBushResource bush)
        {
            if (instance == null || bush == null)
                return;

            instance.grid.Remove(bush);
        }

        public static BerryBushResource FindNearestAvailable(Vector3 origin)
        {
            if (instance == null)
                return null;

            if (instance.grid.TryFindNearest(
                    origin,
                    instance.defaultMaxSearchRadius,
                    BushPosition,
                    IsAvailableBush,
                    out BerryBushResource nearest))
                return nearest;

            return null;
        }

        static bool IsAvailableBush(BerryBushResource bush)
        {
            return bush != null && !bush.IsDepleted;
        }
    }
}

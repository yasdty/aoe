using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Core
{
    /// <summary>
    /// Simulation-facing entity IDs. IDs are monotonic and are not reused after Unregister
    /// (pool respawn assigns a new ID on the next Register).
    /// </summary>
    public static class EntityRegistry
    {
        static int nextEntityId = 1;

        static readonly Dictionary<int, Unit> units = new Dictionary<int, Unit>();
        static readonly Dictionary<int, BuildingHealth> buildings = new Dictionary<int, BuildingHealth>();
        static readonly Dictionary<int, MonoBehaviour> resources = new Dictionary<int, MonoBehaviour>();
        static readonly Dictionary<MonoBehaviour, int> resourceToId = new Dictionary<MonoBehaviour, int>();

        public static int Register(Unit unit)
        {
            if (unit == null)
                return 0;

            if (unit.EntityId > 0)
                Unregister(unit.EntityId);

            int id = nextEntityId++;
            units[id] = unit;
            unit.SetEntityId(id);
            return id;
        }

        public static int Register(BuildingHealth building)
        {
            if (building == null)
                return 0;

            if (building.EntityId > 0)
                Unregister(building.EntityId);

            int id = nextEntityId++;
            buildings[id] = building;
            building.SetEntityId(id);
            return id;
        }

        public static int Register(MonoBehaviour resource)
        {
            if (resource == null || resource is Unit || resource is BuildingHealth)
                return 0;

            if (resourceToId.TryGetValue(resource, out int existingId))
                Unregister(existingId);

            int id = nextEntityId++;
            resources[id] = resource;
            resourceToId[resource] = id;
            return id;
        }

        public static void UnregisterResource(MonoBehaviour resource)
        {
            if (resource == null)
                return;

            if (!resourceToId.TryGetValue(resource, out int entityId))
                return;

            Unregister(entityId);
        }

        public static void Unregister(int entityId)
        {
            if (entityId <= 0)
                return;

            if (units.TryGetValue(entityId, out Unit unit))
            {
                units.Remove(entityId);
                unit.ClearEntityId();
                return;
            }

            if (buildings.TryGetValue(entityId, out BuildingHealth building))
            {
                buildings.Remove(entityId);
                building.ClearEntityId();
                return;
            }

            if (resources.TryGetValue(entityId, out MonoBehaviour resource))
            {
                resources.Remove(entityId);
                resourceToId.Remove(resource);
            }
        }

        public static bool TryGetUnit(int entityId, out Unit unit)
        {
            if (entityId > 0 && units.TryGetValue(entityId, out unit) && unit != null)
                return true;

            unit = null;
            return false;
        }

        public static bool TryGetBuilding(int entityId, out BuildingHealth building)
        {
            if (entityId > 0 && buildings.TryGetValue(entityId, out building) && building != null)
                return true;

            building = null;
            return false;
        }

        public static bool TryGet<T>(int entityId, out T entity) where T : class
        {
            entity = null;
            if (entityId <= 0)
                return false;

            if (typeof(T) == typeof(Unit) && TryGetUnit(entityId, out Unit unit))
            {
                entity = unit as T;
                return entity != null;
            }

            if (typeof(T) == typeof(BuildingHealth) && TryGetBuilding(entityId, out BuildingHealth building))
            {
                entity = building as T;
                return entity != null;
            }

            if (resources.TryGetValue(entityId, out MonoBehaviour resource) && resource is T typed)
            {
                entity = typed;
                return true;
            }

            return false;
        }
    }
}

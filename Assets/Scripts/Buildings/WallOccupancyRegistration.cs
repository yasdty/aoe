using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    [DisallowMultipleComponent]
    public class WallOccupancyRegistration : MonoBehaviour
    {
        PlacedBuildingData data;
        BuildingHealth health;
        float orientationY;
        Vector3 groundCenter;
        bool hasGroundCenter;
        WallOccupancyKind occupancyKind = WallOccupancyKind.Wall;
        int registrationId = -1;

        public void Configure(
            PlacedBuildingData buildingData,
            float segmentOrientationY,
            WallOccupancyKind kind,
            Vector3 placementCenter)
        {
            data = buildingData;
            orientationY = segmentOrientationY;
            occupancyKind = kind;
            groundCenter = new Vector3(placementCenter.x, 0f, placementCenter.z);
            hasGroundCenter = true;

            if (registrationId >= 0)
            {
                WallOccupancyRegistry.Unregister(registrationId);
                registrationId = -1;
            }

            TryRegister();
        }

        void Awake()
        {
            health = GetComponent<BuildingHealth>();
            if (GetComponent<Gate>() != null)
                occupancyKind = WallOccupancyKind.Gate;
        }

        void Start()
        {
            TryRegister();
        }

        void OnDestroy()
        {
            if (registrationId >= 0)
            {
                WallOccupancyRegistry.Unregister(registrationId);
                registrationId = -1;
            }
        }

        void TryRegister()
        {
            if (registrationId >= 0 || data == null)
                return;

            if (health == null)
                health = GetComponent<BuildingHealth>();

            Vector3 center = hasGroundCenter
                ? groundCenter
                : new Vector3(transform.position.x, 0f, transform.position.z);

            UnitTeam team = health != null ? health.Team : UnitTeam.Player;
            registrationId = WallOccupancyRegistry.Register(
                center,
                data.footprintWidth,
                data.footprintDepth,
                orientationY,
                team,
                occupancyKind);
        }
    }
}

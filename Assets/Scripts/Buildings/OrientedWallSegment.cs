using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Buildings
{
    public abstract class OrientedWallSegment : MonoBehaviour
    {
        [SerializeField] protected PlacedBuildingData data;
        [SerializeField] protected float segmentOrientationY;
        protected Vector3 groundCenter;

        protected Renderer cachedRenderer;
        protected MaterialPropertyBlock propertyBlock;
        protected BuildingHealth cachedHealth;

        public PlacedBuildingData Data => data;
        public float SegmentOrientationY => segmentOrientationY;

        public UnitTeam Team
        {
            get
            {
                if (cachedHealth == null)
                    cachedHealth = GetComponent<BuildingHealth>();
                return cachedHealth != null ? cachedHealth.Team : UnitTeam.Player;
            }
        }

        protected virtual void Start()
        {
            ConfigureOccupancy();
        }

        protected virtual void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            cachedHealth = GetComponent<BuildingHealth>();
            EnsureDataReference();
            ApplyOrientation();
            UpdateVisual();
        }

        public void SetData(PlacedBuildingData buildingData)
        {
            data = buildingData;
            UpdateVisual();
        }

        public void SetSegmentOrientation(float orientationY)
        {
            segmentOrientationY = orientationY;
            ApplyOrientation();
        }

        public void PrepareForReuse(
            PlacedBuildingData buildingData,
            Vector3 groundPosition,
            UnitTeam unitTeam,
            float orientationY = 0f)
        {
            SetData(buildingData);
            groundCenter = new Vector3(groundPosition.x, 0f, groundPosition.z);
            SetSegmentOrientation(orientationY);
            transform.position = RuntimeBuildingFactory.ResolveWorldPosition(buildingData, groundPosition);

            if (cachedHealth == null)
                cachedHealth = GetComponent<BuildingHealth>();
            if (cachedHealth != null && buildingData != null)
            {
                cachedHealth.Configure(
                    buildingData.maxHp,
                    buildingData.meleeArmor,
                    buildingData.pierceArmor,
                    unitTeam,
                    townCenter: false);
            }

            ConfigureOccupancy();
            UpdateVisual();
        }

        protected void ApplyOrientation()
        {
            transform.rotation = Quaternion.Euler(0f, segmentOrientationY, 0f);
        }

        protected void ConfigureOccupancy()
        {
            WallOccupancyRegistration registration = GetComponent<WallOccupancyRegistration>();
            if (registration == null)
                registration = gameObject.AddComponent<WallOccupancyRegistration>();
            registration.Configure(data, segmentOrientationY, GetOccupancyKind(), groundCenter);
        }

        protected abstract WallOccupancyKind GetOccupancyKind();
        protected abstract void EnsureDataReference();

        protected void UpdateVisual()
        {
            if (cachedRenderer == null)
                cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer == null || data == null)
                return;

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            cachedRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", data.defaultColor);
            propertyBlock.SetColor("_Color", data.defaultColor);
            cachedRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}

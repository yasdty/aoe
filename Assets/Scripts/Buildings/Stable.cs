using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Buildings
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class Stable : MonoBehaviour
    {
        [SerializeField] PlacedBuildingData data;
        [SerializeField] UnitTeam team = UnitTeam.Player;

        Renderer cachedRenderer;
        MaterialPropertyBlock propertyBlock;
        bool isSelected;
        int unitSpawnIndex;
        ProductionRallyPoint rally;

        public PlacedBuildingData Data => data;
        public UnitTeam Team => team;
        public bool IsSelected => isSelected;
        public ProductionRallyPoint Rally => rally;
        public bool HasRally => rally.kind != RallyTargetKind.None;

        void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            EnsureDataReference();
        }

        void OnEnable()
        {
            StableProductionManager.Register(this);
            UpdateVisual();
        }

        void Start()
        {
            StableProductionManager.Register(this);
        }

        void OnDisable()
        {
            StableProductionManager.Unregister(this);
        }

        public void SetData(PlacedBuildingData buildingData)
        {
            data = buildingData;
            UpdateVisual();
        }

        public void SetTeam(UnitTeam unitTeam)
        {
            team = unitTeam;
            UpdateVisual();
        }

        public void PrepareForReuse(PlacedBuildingData buildingData, Vector3 groundPosition, UnitTeam unitTeam)
        {
            isSelected = false;
            unitSpawnIndex = 0;
            rally = ProductionRallyPoint.None;
            SetData(buildingData);
            SetTeam(unitTeam);
            transform.position = RuntimeBuildingFactory.ResolveWorldPosition(buildingData, groundPosition);

            BuildingHealth health = GetComponent<BuildingHealth>();
            if (health != null && buildingData != null)
                health.Configure(buildingData.maxHp, buildingData.meleeArmor, buildingData.pierceArmor, unitTeam, townCenter: false);

            UpdateVisual();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisual();
        }

        public void SetRally(ProductionRallyPoint value)
        {
            rally = value;
        }

        public void ClearRally()
        {
            rally = ProductionRallyPoint.None;
        }

        public bool TryQueueCavalryProduction()
        {
            if (data == null || data.trainUnitData == null)
                return false;

            return StableProductionManager.TryQueueProduction(
                this,
                data.trainUnitData,
                data.ScaledTrainTime,
                data.ScaledTrainWoodCost,
                data.ScaledTrainFoodCost);
        }

        public bool TryQueueScoutProduction()
        {
            if (data == null || data.secondaryTrainUnitData == null)
                return false;

            return StableProductionManager.TryQueueProduction(
                this,
                data.secondaryTrainUnitData,
                data.ScaledSecondaryTrainTime,
                data.ScaledSecondaryTrainWoodCost,
                data.ScaledSecondaryTrainFoodCost);
        }

        public Vector3 GetUnitSpawnPosition()
        {
            const float unitGroundY = 1f;
            float clearance = data != null ? data.spawnClearance : 1.5f;

            Vector3 exitDirection = ResolveSpawnDirection();
            float halfExtent = BuildingSpawnFormation.GetHorizontalHalfExtentAlong(transform, exitDirection);
            Vector3 spawn = BuildingSpawnFormation.GetGridSlotPosition(
                transform.position,
                exitDirection,
                halfExtent,
                unitSpawnIndex,
                clearance: clearance,
                groundY: unitGroundY);

            unitSpawnIndex = (unitSpawnIndex + 1) % BuildingSpawnFormation.MaxSlots;
            return spawn;
        }

        Vector3 ResolveSpawnDirection()
        {
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                Vector3 toCamera = mainCamera.transform.position - transform.position;
                toCamera.y = 0f;
                if (toCamera.sqrMagnitude > 0.01f)
                    return toCamera.normalized;
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.0001f)
                return forward.normalized;

            return Vector3.forward;
        }

        void UpdateVisual()
        {
            if (cachedRenderer == null)
                cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer == null)
                return;

            if (!isSelected)
            {
                if (propertyBlock == null)
                    propertyBlock = new MaterialPropertyBlock();

                Color baseColor = team == UnitTeam.Enemy
                    ? new Color(0.5f, 0.42f, 0.28f)
                    : (data != null ? data.defaultColor : new Color(0.55f, 0.45f, 0.28f));

                cachedRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_BaseColor", baseColor);
                propertyBlock.SetColor("_Color", baseColor);
                cachedRenderer.SetPropertyBlock(propertyBlock);
                return;
            }

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            Color color = data != null
                ? data.selectedColor
                : new Color(0.9f, 0.75f, 0.35f);

            cachedRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            cachedRenderer.SetPropertyBlock(propertyBlock);
        }

        void EnsureDataReference()
        {
            if (data != null)
                return;

#if UNITY_EDITOR
            data = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultStableData);
#endif
        }
    }
}

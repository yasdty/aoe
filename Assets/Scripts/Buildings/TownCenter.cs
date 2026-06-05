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
    public class TownCenter : MonoBehaviour
    {
        [SerializeField] BuildingData data;
        [SerializeField] UnitTeam team = UnitTeam.Player;

        Renderer cachedRenderer;
        MaterialPropertyBlock propertyBlock;
        bool isSelected;

        public BuildingData Data => data;
        public UnitTeam Team => team;
        public bool IsSelected => isSelected;

        void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            EnsureDataReference();
            EnsureBuildingHealth();
        }

        void EnsureBuildingHealth()
        {
            BuildingHealth health = GetComponent<BuildingHealth>();
            if (health == null)
                health = gameObject.AddComponent<BuildingHealth>();

            float hp = data != null ? data.maxHp : 400f;
            float buildingArmor = data != null ? data.armor : 0f;
            health.Configure(hp, buildingArmor, team, townCenter: true);
        }

        void OnEnable()
        {
            ProductionManager.Register(this);
            UpdateVisual();
        }

        void Start()
        {
            ProductionManager.Register(this);
        }

        void OnDisable()
        {
            ProductionManager.Unregister(this);
        }

        public void SetData(BuildingData buildingData)
        {
            data = buildingData;
            EnsureBuildingHealth();
            UpdateVisual();
        }

        public void SetTeam(UnitTeam unitTeam)
        {
            team = unitTeam;
            BuildingHealth health = GetComponent<BuildingHealth>();
            if (health != null && data != null)
                health.Configure(data.maxHp, data.armor, team, townCenter: true);
            UpdateVisual();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisual();
        }

        public bool TryQueueVillagerProduction()
        {
            if (data == null || data.villagerUnitData == null)
                return false;

            return ProductionManager.TryQueueProduction(this, data.villagerUnitData, data.villagerTrainTime);
        }

        int villagerSpawnIndex;

        public Vector3 GetVillagerSpawnPosition()
        {
            const float villagerGroundY = 1f;
            float clearance = data != null ? data.spawnClearance : 4f;

            Vector3 exitDirection = ResolveSpawnDirection();
            float halfExtent = GetHorizontalHalfExtentAlong(exitDirection);
            Vector3 spawn = transform.position + exitDirection * (halfExtent + clearance);
            spawn.y = villagerGroundY;

            const int ringSlots = 8;
            const float ringRadius = 2.5f;
            float angle = villagerSpawnIndex * (Mathf.PI * 2f / ringSlots);
            villagerSpawnIndex = (villagerSpawnIndex + 1) % ringSlots;
            spawn += new Vector3(Mathf.Cos(angle) * ringRadius, 0f, Mathf.Sin(angle) * ringRadius);

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

        float GetHorizontalHalfExtentAlong(Vector3 worldDirection)
        {
            Vector3 localDirection = transform.InverseTransformDirection(worldDirection);
            localDirection.y = 0f;
            if (localDirection.sqrMagnitude < 0.0001f)
                localDirection = Vector3.forward;
            localDirection.Normalize();

            Vector3 halfSize = transform.lossyScale * 0.5f;
            return Mathf.Abs(localDirection.x) * halfSize.x + Mathf.Abs(localDirection.z) * halfSize.z;
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
                    ? new Color(0.45f, 0.55f, 0.85f)
                    : (data != null ? data.defaultColor : new Color(0.75f, 0.65f, 0.45f));

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
                : new Color(0.95f, 0.85f, 0.35f);

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
            data = AssetDatabase.LoadAssetAtPath<BuildingData>(GameAssetPaths.TownCenterData);
#endif
        }
    }
}

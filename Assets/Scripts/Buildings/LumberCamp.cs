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
    public class LumberCamp : MonoBehaviour
    {
        [SerializeField] PlacedBuildingData data;

        Renderer cachedRenderer;
        MaterialPropertyBlock propertyBlock;
        BuildingHealth cachedHealth;

        public PlacedBuildingData Data => data;

        public UnitTeam Team
        {
            get
            {
                if (cachedHealth == null)
                    cachedHealth = GetComponent<BuildingHealth>();
                return cachedHealth != null ? cachedHealth.Team : UnitTeam.Player;
            }
        }

        void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            cachedHealth = GetComponent<BuildingHealth>();
            EnsureDataReference();
        }

        void OnEnable()
        {
            LumberCampRegistry.Register(this);
        }

        void OnDisable()
        {
            LumberCampRegistry.Unregister(this);
        }

        public void SetData(PlacedBuildingData buildingData)
        {
            data = buildingData;
            UpdateVisual();
        }

        public void PrepareForReuse(PlacedBuildingData buildingData, Vector3 groundPosition, UnitTeam unitTeam)
        {
            SetData(buildingData);
            transform.position = RuntimeBuildingFactory.ResolveWorldPosition(buildingData, groundPosition);

            if (cachedHealth == null)
                cachedHealth = GetComponent<BuildingHealth>();
            if (cachedHealth != null && buildingData != null)
                cachedHealth.Configure(buildingData.maxHp, buildingData.meleeArmor, buildingData.pierceArmor, unitTeam, townCenter: false);

            UpdateVisual();
        }

        public Vector3 GetDepositPosition()
        {
            Vector3 position = transform.position;
            position.y = 1f;
            return position;
        }

        void UpdateVisual()
        {
            if (cachedRenderer == null)
                cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer == null)
                return;

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            Color color = data != null ? data.defaultColor : new Color(0.55f, 0.38f, 0.22f);
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
            data = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultLumberCampData);
#endif
        }
    }
}

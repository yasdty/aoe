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
    public class Farm : MonoBehaviour
    {
        [SerializeField] PlacedBuildingData data;

        float remainingFood;
        Renderer cachedRenderer;
        MaterialPropertyBlock propertyBlock;
        BuildingHealth cachedHealth;

        public PlacedBuildingData Data => data;
        public float RemainingFood => remainingFood;
        public bool IsDepleted => remainingFood <= 0f;

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
            remainingFood = data != null ? data.foodCapacity : 250f;
        }

        public void SetData(PlacedBuildingData buildingData)
        {
            data = buildingData;
            remainingFood = data != null ? data.foodCapacity : 250f;
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

        public Vector3 GetGatherPosition()
        {
            Vector3 position = transform.position;
            position.y = 1f;
            return position;
        }

        public float TakeFood(float amount)
        {
            if (amount <= 0f || IsDepleted)
                return 0f;

            float taken = Mathf.Min(amount, remainingFood);
            remainingFood -= taken;
            UpdateVisual();
            if (IsDepleted)
                OnDepleted();

            return taken;
        }

        void OnDepleted()
        {
            BuildingPool.ReturnFarm(this);
        }

        void UpdateVisual()
        {
            if (cachedRenderer == null)
                cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer == null)
                return;

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            Color color = data != null ? data.defaultColor : new Color(0.35f, 0.7f, 0.25f);
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
            data = AssetDatabase.LoadAssetAtPath<PlacedBuildingData>(GameAssetPaths.DefaultFarmData);
#endif
        }
    }
}

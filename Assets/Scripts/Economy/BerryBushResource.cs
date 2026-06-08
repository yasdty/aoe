using AoE.RTS.Core;
using AoE.RTS.Spatial;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Economy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class BerryBushResource : MonoBehaviour
    {
        [SerializeField] FoodNodeData data;

        float remainingFood;
        Renderer cachedRenderer;
        MaterialPropertyBlock propertyBlock;

        public float RemainingFood => remainingFood;
        public bool IsDepleted => remainingFood <= 0f;

        void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            EnsureDataReference();
            remainingFood = data != null ? data.initialFood : 250f;
        }

        void OnEnable()
        {
            BerryBushSpatialIndex.Register(this);
            UpdateVisual();
        }

        void OnDisable()
        {
            BerryBushSpatialIndex.Unregister(this);
        }

        void Start()
        {
            BerryBushSpatialIndex.Register(this);
        }

        public void SetData(FoodNodeData nodeData)
        {
            data = nodeData;
            remainingFood = data != null ? data.initialFood : 250f;
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
                BerryBushSpatialIndex.Unregister(this);

            return taken;
        }

        void UpdateVisual()
        {
            if (cachedRenderer == null)
                cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer == null)
                return;

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            Color color = IsDepleted
                ? (data != null ? data.depletedColor : Color.gray)
                : (data != null ? data.defaultColor : new Color(0.55f, 0.15f, 0.45f));

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
            data = AssetDatabase.LoadAssetAtPath<FoodNodeData>(GameAssetPaths.DefaultBerryBushData);
#endif
        }
    }
}

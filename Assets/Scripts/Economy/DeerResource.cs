using AoE.RTS.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Economy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class DeerResource : MonoBehaviour, IHuntableFoodResource
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
            remainingFood = data != null ? data.initialFood : 140f;
        }

        void OnEnable()
        {
            EntityRegistry.Register(this);
            DeerRegistry.Register(this);
            UpdateVisual();
        }

        void OnDisable()
        {
            EntityRegistry.UnregisterResource(this);
            DeerRegistry.Unregister(this);
        }

        public void SetData(FoodNodeData nodeData)
        {
            data = nodeData;
            remainingFood = data != null ? data.initialFood : 140f;
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
                : (data != null ? data.defaultColor : new Color(0.55f, 0.38f, 0.22f));

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
            data = AssetDatabase.LoadAssetAtPath<FoodNodeData>(GameAssetPaths.DefaultDeerData);
#endif
        }
    }
}

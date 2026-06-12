using AoE.RTS.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Economy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class StoneMineResource : MonoBehaviour
    {
        [SerializeField] MineralNodeData data;

        float remainingAmount;
        Renderer cachedRenderer;
        MaterialPropertyBlock propertyBlock;

        public float RemainingAmount => remainingAmount;
        public bool IsDepleted => remainingAmount <= 0f;

        void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            EnsureDataReference();
            remainingAmount = data != null ? data.initialAmount : 350f;
        }

        void OnEnable()
        {
            EntityRegistry.Register(this);
            UpdateVisual();
        }

        void OnDisable()
        {
            EntityRegistry.UnregisterResource(this);
        }

        public void SetData(MineralNodeData nodeData)
        {
            data = nodeData;
            remainingAmount = data != null ? data.initialAmount : 350f;
            UpdateVisual();
        }

        public Vector3 GetGatherPosition()
        {
            Vector3 position = transform.position;
            position.y = 1f;
            return position;
        }

        public float TakeMineral(float amount)
        {
            if (amount <= 0f || IsDepleted)
                return 0f;

            float taken = Mathf.Min(amount, remainingAmount);
            remainingAmount -= taken;
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
                : (data != null ? data.defaultColor : new Color(0.55f, 0.55f, 0.58f));

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
            data = AssetDatabase.LoadAssetAtPath<MineralNodeData>(GameAssetPaths.DefaultStoneMineData);
#endif
        }
    }
}

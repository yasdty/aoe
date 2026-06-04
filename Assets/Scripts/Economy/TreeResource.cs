using AoE.RTS.Core;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Economy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class TreeResource : MonoBehaviour
    {
        [SerializeField] ResourceNodeData data;

        float remainingWood;
        Renderer cachedRenderer;
        MaterialPropertyBlock propertyBlock;

        public float RemainingWood => remainingWood;
        public bool IsDepleted => remainingWood <= 0f;

        void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            EnsureDataReference();
            remainingWood = data != null ? data.initialWood : 100f;
        }

        void OnEnable()
        {
            UpdateVisual();
        }

        public void SetData(ResourceNodeData nodeData)
        {
            data = nodeData;
            remainingWood = data != null ? data.initialWood : 100f;
            UpdateVisual();
        }

        public Vector3 GetGatherPosition()
        {
            Vector3 position = transform.position;
            position.y = 1f;
            return position;
        }

        public float TakeWood(float amount)
        {
            if (amount <= 0f || IsDepleted)
                return 0f;

            float taken = Mathf.Min(amount, remainingWood);
            remainingWood -= taken;
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
                : (data != null ? data.defaultColor : new Color(0.2f, 0.5f, 0.22f));

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
            data = AssetDatabase.LoadAssetAtPath<ResourceNodeData>(GameAssetPaths.DefaultTreeData);
#endif
        }
    }
}

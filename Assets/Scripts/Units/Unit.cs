using UnityEngine;

namespace AoE.RTS.Units
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class Unit : MonoBehaviour
    {
        [SerializeField] UnitData data;

        float currentHp;
        Vector3? moveTarget;
        Renderer cachedRenderer;
        MaterialPropertyBlock propertyBlock;
        bool isSelected;

        public float CurrentHp => currentHp;
        public float MaxHp => data != null ? data.maxHp : 100f;
        public bool IsSelected => isSelected;
        public UnitData Data => data;
        public bool HasMoveTarget => moveTarget.HasValue;

        void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            currentHp = MaxHp;
        }

        void OnEnable()
        {
            UnitManager.Register(this);
            UpdateVisual();
        }

        void Start()
        {
            UnitManager.Register(this);
        }

        void OnDisable()
        {
            UnitManager.Unregister(this);
        }

        public void SetData(UnitData unitData)
        {
            data = unitData;
            currentHp = MaxHp;
            UpdateVisual();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisual();
        }

        public void SetMoveTarget(Vector3 worldPosition)
        {
            moveTarget = new Vector3(worldPosition.x, transform.position.y, worldPosition.z);
        }

        public void ClearMoveTarget()
        {
            moveTarget = null;
        }

        public bool IsNear(Vector3 worldPosition, float radius)
        {
            Vector3 delta = transform.position - worldPosition;
            delta.y = 0f;
            return delta.sqrMagnitude <= radius * radius;
        }

        public void TickMovement(float deltaTime)
        {
            if (!moveTarget.HasValue)
                return;

            Vector3 target = moveTarget.Value;
            Vector3 position = transform.position;
            Vector3 toTarget = target - position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;
            float speed = data != null ? data.moveSpeed : 5f;
            float step = speed * deltaTime;

            if (distance <= step)
            {
                transform.position = target;
                moveTarget = null;
                return;
            }

            transform.position = position + toTarget / distance * step;
        }

        void UpdateVisual()
        {
            if (cachedRenderer == null)
                cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer == null)
                return;

            if (propertyBlock == null)
                propertyBlock = new MaterialPropertyBlock();

            Color color = isSelected
                ? (data != null ? data.selectedColor : Color.green)
                : (data != null ? data.defaultColor : Color.white);

            cachedRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            cachedRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}

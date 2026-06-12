using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AoE.RTS.Economy
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class SheepResource : MonoBehaviour, IHuntableFoodResource
    {
        const float MoveSpeedValue = 2.25f;
        const float ArrivalDistance = 0.25f;

        [SerializeField] FoodNodeData data;

        float remainingFood;
        bool isDiscovered;
        UnitTeam ownerTeam;
        Vector3 moveTarget;
        bool hasMoveTarget;
        Renderer cachedRenderer;
        MaterialPropertyBlock propertyBlock;

        public float RemainingFood => remainingFood;
        public bool IsDepleted => remainingFood <= 0f;
        public bool IsNeutral => !isDiscovered;
        public bool IsDiscovered => isDiscovered;
        public UnitTeam OwnerTeam => ownerTeam;
        public float MoveSpeed => MoveSpeedValue;
        public bool HasMoveTarget => hasMoveTarget;

        void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            EnsureDataReference();
            remainingFood = data != null ? data.initialFood : 100f;
        }

        void OnEnable()
        {
            EntityRegistry.Register(this);
            SheepRegistry.Register(this);
            UpdateVisual();
        }

        void OnDisable()
        {
            EntityRegistry.UnregisterResource(this);
            SheepRegistry.Unregister(this);
        }

        public void SetData(FoodNodeData nodeData)
        {
            data = nodeData;
            remainingFood = data != null ? data.initialFood : 100f;
            UpdateVisual();
        }

        public void ResetToNeutral()
        {
            isDiscovered = false;
            ClearMoveTarget();
            UpdateVisual();
        }

        public void Discover(UnitTeam team)
        {
            if (isDiscovered)
                return;

            isDiscovered = true;
            ownerTeam = team;
            UpdateVisual();
        }

        public bool CanBeHuntedBy(UnitTeam team)
        {
            return isDiscovered && ownerTeam == team;
        }

        public void SetMoveTarget(Vector3 target)
        {
            moveTarget = target;
            moveTarget.y = transform.position.y;
            hasMoveTarget = true;
        }

        public void ClearMoveTarget()
        {
            hasMoveTarget = false;
        }

        public void TickMovement(float deltaTime)
        {
            if (!hasMoveTarget)
                return;

            Vector3 position = transform.position;
            Vector3 toTarget = moveTarget - position;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            float step = MoveSpeedValue * deltaTime;
            if (distance <= ArrivalDistance || distance <= step)
            {
                transform.position = new Vector3(moveTarget.x, position.y, moveTarget.z);
                ClearMoveTarget();
                return;
            }

            transform.position = position + toTarget / distance * step;
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

            Color color;
            if (IsDepleted)
            {
                color = data != null ? data.depletedColor : Color.gray;
            }
            else if (isDiscovered)
            {
                Color baseColor = data != null ? data.defaultColor : new Color(0.88f, 0.88f, 0.85f);
                Color teamTint = ownerTeam == UnitTeam.Player
                    ? new Color(0.75f, 0.9f, 1f)
                    : new Color(1f, 0.75f, 0.75f);
                color = Color.Lerp(baseColor, teamTint, 0.35f);
            }
            else
            {
                color = data != null ? data.defaultColor : new Color(0.88f, 0.88f, 0.85f);
            }

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
            data = AssetDatabase.LoadAssetAtPath<FoodNodeData>(GameAssetPaths.DefaultSheepData);
#endif
        }
    }
}

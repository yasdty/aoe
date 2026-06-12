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
    public class BoarResource : MonoBehaviour, IHuntableFoodResource
    {
        [SerializeField] FoodNodeData data;
        [SerializeField] float maxHp = 75f;
        [SerializeField] float attackPower = 7f;
        [SerializeField] float attackRange = 1.2f;
        [SerializeField] float attackCooldownSeconds = 1.5f;
        [SerializeField] float moveSpeed = 3.5f;

        float currentHp;
        float remainingFood;
        Renderer cachedRenderer;
        MaterialPropertyBlock propertyBlock;
        Unit activeHunter;

        public float MaxHp => maxHp;
        public float CurrentHp => currentHp;
        public bool IsDead => currentHp <= 0f;
        public float RemainingFood => remainingFood;
        public bool IsDepleted => IsDead && remainingFood <= 0f;
        public float AttackPower => attackPower;
        public float AttackRange => attackRange;
        public float AttackCooldownSeconds => attackCooldownSeconds;
        public float MoveSpeed => moveSpeed;

        void Awake()
        {
            cachedRenderer = GetComponentInChildren<Renderer>();
            EnsureDataReference();
            ResetToAliveState();
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

        public void SetData(FoodNodeData nodeData)
        {
            data = nodeData;
            ResetToAliveState();
            UpdateVisual();
        }

        void ResetToAliveState()
        {
            currentHp = maxHp;
            remainingFood = 0f;
        }

        public Vector3 GetGatherPosition()
        {
            Vector3 position = transform.position;
            position.y = 1f;
            return position;
        }

        public void SetActiveHunter(Unit hunter)
        {
            activeHunter = hunter;
        }

        public void ClearActiveHunter(Unit hunter)
        {
            if (activeHunter == hunter)
                activeHunter = null;
        }

        public float TakeFood(float amount)
        {
            if (!IsDead || amount <= 0f || remainingFood <= 0f)
                return 0f;

            float taken = Mathf.Min(amount, remainingFood);
            remainingFood -= taken;
            UpdateVisual();

            if (IsDepleted)
                BoarAggroManager.NotifyDepleted(this);

            return taken;
        }

        public float ApplyHuntDamage(float amount, Unit hunter)
        {
            if (IsDead || amount <= 0f)
                return 0f;

            float dealt = Mathf.Min(amount, currentHp);
            currentHp -= dealt;

            if (hunter != null)
                BoarAggroManager.NotifyHunted(this, hunter);
            else if (activeHunter != null)
                BoarAggroManager.NotifyHunted(this, activeHunter);

            if (IsDead)
                OnDeath();

            UpdateVisual();
            return dealt;
        }

        public void ApplyAttackDamage(float damage, Unit attacker)
        {
            if (IsDead || damage <= 0f)
                return;

            currentHp = Mathf.Max(0f, currentHp - damage);

            if (attacker != null)
                BoarAggroManager.NotifyAttacked(this, attacker);

            if (IsDead)
                OnDeath();

            UpdateVisual();
        }

        void OnDeath()
        {
            remainingFood = data != null ? data.initialFood : 340f;
            BoarAggroManager.NotifyDepleted(this);
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
            else if (IsDead)
            {
                Color baseColor = data != null ? data.defaultColor : new Color(0.35f, 0.35f, 0.38f);
                color = Color.Lerp(baseColor, data != null ? data.depletedColor : Color.gray, 0.45f);
            }
            else
            {
                color = data != null ? data.defaultColor : new Color(0.35f, 0.35f, 0.38f);
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
            data = AssetDatabase.LoadAssetAtPath<FoodNodeData>(GameAssetPaths.DefaultBoarData);
#endif
        }
    }
}

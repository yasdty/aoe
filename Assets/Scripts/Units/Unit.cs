using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Combat;
using AoE.RTS.Economy;
using AoE.RTS.Selection;
using UnityEngine;

namespace AoE.RTS.Units
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class Unit : MonoBehaviour
    {
        static int nextStandSlot;

        [SerializeField] UnitData data;
        [SerializeField] UnitTeam team = UnitTeam.Player;

        float currentHp;
        Vector3? moveTarget;
        Renderer cachedRenderer;
        MaterialPropertyBlock propertyBlock;
        bool isSelected;
        bool isDead;
        int standSlot;

        public float CurrentHp => currentHp;
        public float MaxHp => data != null ? data.maxHp : 100f;
        public bool IsAlive => !isDead;
        public bool IsSelected => isSelected;
        public UnitData Data => data;
        public UnitTeam Team => team;
        public bool HasMoveTarget => moveTarget.HasValue;
        public bool CanAttack => IsAlive && data != null && data.CanAttack;
        public float AttackPower => data != null ? data.attack : 0f;
        public float Armor => data != null ? data.armor : 0f;
        public float AttackRange => data != null ? data.attackRange : 1.5f;
        public float AttackCooldownSeconds => data != null ? data.attackCooldown : 1f;
        public int StandSlot => standSlot;

        public UnitState State
        {
            get
            {
                if (isDead)
                    return UnitState.Dead;

                if (AttackManager.IsUnitAttacking(this))
                    return UnitState.Attack;

                if (HasMoveTarget)
                    return UnitState.Move;

                return UnitState.Idle;
            }
        }

        void Awake()
        {
            standSlot = nextStandSlot % UnitPositionOffsets.SlotCount;
            nextStandSlot++;
            cachedRenderer = GetComponentInChildren<Renderer>();
            ApplyTeamFromData();
            currentHp = MaxHp;
        }

        void OnEnable()
        {
            if (!isDead)
                UnitManager.Register(this);
            UpdateVisual();
        }

        void Start()
        {
            if (!isDead)
                UnitManager.Register(this);

            UpdateVisual();
        }

        void OnDisable()
        {
            UnitManager.Unregister(this);
        }

        public void SetData(UnitData unitData)
        {
            data = unitData;
            ApplyTeamFromData();
            currentHp = MaxHp;
            UpdateVisual();
        }

        void ApplyTeamFromData()
        {
            if (data == null)
            {
                if (gameObject.name == "Enemy Dummy")
                    team = UnitTeam.Enemy;
                return;
            }

            if (data.displayName == "Enemy Dummy" || data.team == UnitTeam.Enemy && data.CanAttack)
                team = UnitTeam.Enemy;
        }

        public void SetTeam(UnitTeam unitTeam)
        {
            team = unitTeam;
            UpdateVisual();
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
                return;

            currentHp = Mathf.Max(0f, currentHp - amount);
            if (currentHp <= 0f)
                Die();
        }

        public void PrepareForSpawn(UnitData unitData, Vector3 position, UnitTeam unitTeam)
        {
            isDead = false;
            isSelected = false;
            moveTarget = null;

            if (unitData != null)
                data = unitData;

            ApplyTeamFromData();
            team = unitTeam;
            currentHp = MaxHp;
            transform.position = position;
            UpdateVisual();
        }

        public void Die()
        {
            if (isDead)
                return;

            isDead = true;
            ClearMoveTarget();

            GatherManager.CancelForUnit(this);
            FoodGatherManager.CancelForUnit(this);
            MineralGatherManager.CancelForUnit(this);
            BuildingPlacementManager.AbortConstructionForUnit(this);
            AttackManager.CancelJobsForUnit(this);
            SelectionManager.HandleUnitDied(this);
            UnitManager.Unregister(this);

            UnitPool.Return(this);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisual();
        }

        public void SetMoveTarget(Vector3 worldPosition)
        {
            if (!IsAlive)
                return;

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
            if (!IsAlive || !moveTarget.HasValue)
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

        public void NotifyStateChanged()
        {
            UpdateVisual();
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
            if (isSelected)
            {
                color = data != null ? data.selectedColor : Color.green;
            }
            else if (team == UnitTeam.Enemy)
            {
                color = new Color(0.45f, 0.55f, 0.85f);
            }
            else
            {
                color = data != null ? data.defaultColor : Color.white;
            }

            if (IsAlive && AttackManager.IsUnitAttacking(this))
            {
                Color attackTint = new Color(0.95f, 0.45f, 0.2f);
                color = Color.Lerp(color, attackTint, 0.55f);
            }

            cachedRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            cachedRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}

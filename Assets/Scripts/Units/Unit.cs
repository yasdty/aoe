using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Combat;
using AoE.RTS.Core;
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
        [SerializeField] PlayerId ownerId = PlayerId.Player0;

        float currentHp;
        float effectiveMaxHp;
        Vector3? moveTarget;
        Renderer cachedRenderer;
        MaterialPropertyBlock propertyBlock;
        bool isSelected;
        bool isDead;
        int standSlot;
        int entityId;
        UnitCombatStance combatStance = UnitCombatStance.Aggressive;

        public int EntityId => entityId;
        public float CurrentHp => currentHp;
        public float MaxHp => effectiveMaxHp > 0f ? effectiveMaxHp : (data != null ? data.maxHp : 100f);
        public bool IsAlive => !isDead;
        public bool IsSelected => isSelected;
        public UnitData Data => data;
        public UnitTeam Team => team;
        public PlayerId OwnerId => ownerId;
        public bool HasMoveTarget => moveTarget.HasValue;
        public bool CanAttack => IsAlive && data != null && data.CanAttack;
        public float AttackPower => data != null ? data.attack : 0f;
        public AttackDamageType AttackDamageType =>
            data != null ? data.attackDamageType : AttackDamageType.Melee;
        public float MeleeArmor => data != null ? data.meleeArmor : 0f;
        public float PierceArmor => data != null ? data.pierceArmor : 0f;
        public UnitArmorClass ArmorClass => data != null ? data.armorClass : UnitArmorClass.None;
        public float AttackRange => data != null ? data.attackRange : 1.5f;
        public float AttackCooldownSeconds => data != null ? data.attackCooldown : 1f;
        public int StandSlot => standSlot;
        public UnitCombatStance CombatStance => combatStance;
        public bool IsStandGround => combatStance == UnitCombatStance.StandGround;

        public UnitState State
        {
            get
            {
                if (isDead)
                    return UnitState.Dead;

                if (AttackManager.IsUnitAttacking(this) || BoarAttackManager.IsUnitAttackingBoar(this))
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
            UnitDataResolver.EnsureUnitHasData(this);
            ApplyTeamFromData();
            RefreshEffectiveMaxHp();
            currentHp = MaxHp;
        }

        void OnEnable()
        {
            if (!isDead)
            {
                UnitManager.Register(this);
                EntityRegistry.Register(this);
            }

            UpdateVisual();
        }

        void Start()
        {
            if (!isDead)
                UnitManager.Register(this);

            float baseMaxHp = data != null ? data.maxHp : 100f;
            bool wasAtFullHealth = Mathf.Approximately(currentHp, baseMaxHp);
            RefreshEffectiveMaxHp();
            if (wasAtFullHealth)
                currentHp = MaxHp;

            UpdateVisual();
        }

        void OnDisable()
        {
            UnitManager.Unregister(this);
            if (entityId > 0)
                EntityRegistry.Unregister(entityId);
        }

        internal void SetEntityId(int id) => entityId = id;

        internal void ClearEntityId() => entityId = 0;

        public void SetData(UnitData unitData)
        {
            data = unitData;
            if (data != null)
                gameObject.name = UnitDisplayNameUtility.GetDisplayName(data);

            ApplyTeamFromData();
            RefreshEffectiveMaxHp();
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
            ownerId = PlayerIdMapping.FromLegacyTeam(unitTeam);
            UpdateVisual();
        }

        public void SetOwner(PlayerId playerId)
        {
            ownerId = playerId;
            team = PlayerIdMapping.ToLegacyTeam(playerId);
            RefreshEffectiveMaxHp();
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
            combatStance = UnitCombatStance.Aggressive;

            if (unitData != null)
            {
                data = unitData;
                gameObject.name = UnitDisplayNameUtility.GetDisplayName(unitData);
            }

            ApplyTeamFromData();
            team = unitTeam;
            ownerId = PlayerIdMapping.FromLegacyTeam(unitTeam);
            RefreshEffectiveMaxHp();
            currentHp = MaxHp;
            transform.position = position;
            UpdateVisual();
        }

        void RefreshEffectiveMaxHp()
        {
            effectiveMaxHp = CivilizationBonusUtility.GetScaledMaxHp(data, team);
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
            BoarAttackManager.CancelJobsForUnit(this);
            AttackMoveManager.CancelForUnit(this);
            FormationMoveManager.CancelForUnit(this);
            SelectionManager.HandleUnitDied(this);
            UnitManager.Unregister(this);

            CombatDeathScheduler.ScheduleReturn(this);
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisual();
        }

        public void SetCombatStance(UnitCombatStance stance)
        {
            combatStance = stance;
            NotifyStateChanged();
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

        public bool TryGetMoveTargetPosition(out Vector3 worldPosition)
        {
            if (!moveTarget.HasValue)
            {
                worldPosition = default;
                return false;
            }

            worldPosition = moveTarget.Value;
            return true;
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

            Vector3 nextPosition = distance <= step
                ? target
                : position + toTarget / distance * step;

            if (WallOccupancyRegistry.IsMovementBlockedAlongPath(position, nextPosition, team))
            {
                moveTarget = null;
                return;
            }

            transform.position = nextPosition;
            if (distance <= step)
                moveTarget = null;
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

            if (IsAlive && (AttackManager.IsUnitAttacking(this) || BoarAttackManager.IsUnitAttackingBoar(this)))
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

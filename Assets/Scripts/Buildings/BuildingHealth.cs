using AoE.RTS.Core;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Buildings
{
    [DisallowMultipleComponent]
    public class BuildingHealth : MonoBehaviour
    {
        [SerializeField] float maxHp = 400f;
        [SerializeField] float meleeArmor;
        [SerializeField] float pierceArmor;
        [SerializeField] UnitTeam team = UnitTeam.Player;
        [SerializeField] PlayerId ownerId = PlayerId.Player0;
        [SerializeField] bool isTownCenter;

        float currentHp;
        bool isConfigured;
        int entityId;

        public int EntityId => entityId;
        public float MaxHp => maxHp;
        public float CurrentHp => currentHp;
        public float MeleeArmor => meleeArmor;
        public float PierceArmor => pierceArmor;
        public UnitTeam Team => team;
        public PlayerId OwnerId => ownerId;
        public bool IsTownCenter => isTownCenter;
        public bool IsAlive => currentHp > 0f;

        public void Configure(
            float hp,
            float buildingMeleeArmor,
            float buildingPierceArmor,
            UnitTeam buildingTeam,
            bool townCenter)
        {
            maxHp = Mathf.Max(1f, hp);
            meleeArmor = Mathf.Max(0f, buildingMeleeArmor);
            pierceArmor = Mathf.Max(0f, buildingPierceArmor);
            team = buildingTeam;
            ownerId = PlayerIdMapping.FromLegacyTeam(buildingTeam);
            isTownCenter = townCenter;
            currentHp = maxHp;
            isConfigured = true;
        }

        public void SetOwnerId(PlayerId playerId)
        {
            ownerId = playerId;
            team = PlayerIdMapping.ToLegacyTeam(playerId);
        }

        void Awake()
        {
            if (!isConfigured)
                currentHp = maxHp;
        }

        void OnEnable()
        {
            EntityRegistry.Register(this);
        }

        void OnDisable()
        {
            if (entityId > 0)
                EntityRegistry.Unregister(entityId);
        }

        internal void SetEntityId(int id) => entityId = id;

        internal void ClearEntityId() => entityId = 0;

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
                return;

            currentHp = Mathf.Max(0f, currentHp - amount);
            if (currentHp <= 0f)
                Die();
        }

        void Die()
        {
            bool countsAsTownCenter = isTownCenter || GetComponent<TownCenter>() != null;
            if (countsAsTownCenter)
            {
                PlayerId destroyedPlayer = ownerId;
                TownCenter townCenter = GetComponent<TownCenter>();
                if (townCenter != null)
                    ProductionManager.Unregister(townCenter);

                Destroy(gameObject);

                if (!ProductionManager.HasAnyTownCenterForPlayer(destroyedPlayer))
                    GameSessionManager.NotifyTownCenterDestroyed(destroyedPlayer);
                return;
            }

            Barracks barracks = GetComponent<Barracks>();
            if (barracks != null)
            {
                BuildingPool.ReturnBarracks(barracks);
                return;
            }

            House house = GetComponent<House>();
            if (house != null)
            {
                PlacedBuildingData houseData = house.Data;
                int housing = houseData != null ? houseData.housingProvided : 0;
                if (housing > 0)
                    PopulationManager.RemoveHousing(ownerId, housing);

                BuildingPool.ReturnHouse(house);
                return;
            }

            Farm farm = GetComponent<Farm>();
            if (farm != null)
            {
                FoodGatherManager.CancelJobsForFarm(farm);
                BuildingPool.ReturnFarm(farm);
                return;
            }

            LumberCamp lumberCamp = GetComponent<LumberCamp>();
            if (lumberCamp != null)
            {
                BuildingPool.ReturnLumberCamp(lumberCamp);
                return;
            }

            MiningCamp miningCamp = GetComponent<MiningCamp>();
            if (miningCamp != null)
            {
                BuildingPool.ReturnMiningCamp(miningCamp);
                return;
            }

            Mill mill = GetComponent<Mill>();
            if (mill != null)
            {
                BuildingPool.ReturnMill(mill);
                return;
            }

            Destroy(gameObject);
        }

        public Vector3 GetMeleeStandPosition(Vector3 attackerPosition, float standOff = 0.75f)
        {
            Vector3 center = transform.position;
            Vector3 direction = attackerPosition - center;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
                direction = Vector3.forward;
            direction.Normalize();

            float halfExtent = GetHorizontalHalfExtent(direction);
            Vector3 edge = center + direction * halfExtent;
            edge.y = attackerPosition.y;
            return edge + direction * standOff;
        }

        float GetHorizontalHalfExtent(Vector3 worldDirection)
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                Vector3 localDirection = transform.InverseTransformDirection(worldDirection);
                localDirection.y = 0f;
                if (localDirection.sqrMagnitude < 0.0001f)
                    localDirection = Vector3.forward;
                localDirection.Normalize();

                Vector3 halfSize = Vector3.Scale(box.size * 0.5f, transform.lossyScale);
                return Mathf.Abs(localDirection.x) * halfSize.x + Mathf.Abs(localDirection.z) * halfSize.z;
            }

            Collider collider = GetComponent<Collider>();
            if (collider != null)
            {
                Bounds bounds = collider.bounds;
                return Mathf.Max(bounds.extents.x, bounds.extents.z);
            }

            return 2f;
        }
    }
}

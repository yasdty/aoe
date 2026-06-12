using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class ResourceManager : MonoBehaviour
    {
        const int PlayerCount = 4;
        const float ClassicStartFood = 200f;
        const float ClassicStartWood = 200f;

        static ResourceManager instance;

        [SerializeField] float initialFoodPerPlayer = ClassicStartFood;
        [SerializeField] float initialWoodPerPlayer = ClassicStartWood;

        readonly float[] wood = new float[PlayerCount];
        readonly float[] food = new float[PlayerCount];
        readonly float[] gold = new float[PlayerCount];
        readonly float[] stone = new float[PlayerCount];

        public static float Wood => GetWood(UnitTeam.Player);
        public static float Food => GetFood(UnitTeam.Player);
        public static float Gold => GetGold(UnitTeam.Player);
        public static float Stone => GetStone(UnitTeam.Player);

        void Awake()
        {
            instance = this;
            for (int i = 0; i < PlayerCount; i++)
            {
                food[i] = initialFoodPerPlayer;
                wood[i] = initialWoodPerPlayer;
            }
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        static int ResolveIndex(PlayerId playerId) => PlayerIdMapping.ToIndex(playerId);

        static int ResolveIndex(UnitTeam team) => ResolveIndex(PlayerIdMapping.FromLegacyTeam(team));

        public static float GetWood(PlayerId playerId)
        {
            if (instance == null)
                return 0f;

            return instance.wood[ResolveIndex(playerId)];
        }

        public static float GetWood(UnitTeam team) => GetWood(PlayerIdMapping.FromLegacyTeam(team));

        public static float GetFood(PlayerId playerId)
        {
            if (instance == null)
                return 0f;

            return instance.food[ResolveIndex(playerId)];
        }

        public static float GetFood(UnitTeam team) => GetFood(PlayerIdMapping.FromLegacyTeam(team));

        public static float GetGold(PlayerId playerId)
        {
            if (instance == null)
                return 0f;

            return instance.gold[ResolveIndex(playerId)];
        }

        public static float GetGold(UnitTeam team) => GetGold(PlayerIdMapping.FromLegacyTeam(team));

        public static float GetStone(PlayerId playerId)
        {
            if (instance == null)
                return 0f;

            return instance.stone[ResolveIndex(playerId)];
        }

        public static float GetStone(UnitTeam team) => GetStone(PlayerIdMapping.FromLegacyTeam(team));

        public static void AddWood(float amount)
        {
            AddWood(UnitTeam.Player, amount);
        }

        public static void AddWood(PlayerId playerId, float amount)
        {
            if (instance == null || amount <= 0f)
                return;

            instance.wood[ResolveIndex(playerId)] += amount;
        }

        public static void AddWood(UnitTeam team, float amount) =>
            AddWood(PlayerIdMapping.FromLegacyTeam(team), amount);

        public static void AddFood(PlayerId playerId, float amount)
        {
            if (instance == null || amount <= 0f)
                return;

            instance.food[ResolveIndex(playerId)] += amount;
        }

        public static void AddFood(UnitTeam team, float amount) =>
            AddFood(PlayerIdMapping.FromLegacyTeam(team), amount);

        public static void AddGold(PlayerId playerId, float amount)
        {
            if (instance == null || amount <= 0f)
                return;

            instance.gold[ResolveIndex(playerId)] += amount;
        }

        public static void AddGold(UnitTeam team, float amount) =>
            AddGold(PlayerIdMapping.FromLegacyTeam(team), amount);

        public static void AddStone(PlayerId playerId, float amount)
        {
            if (instance == null || amount <= 0f)
                return;

            instance.stone[ResolveIndex(playerId)] += amount;
        }

        public static void AddStone(UnitTeam team, float amount) =>
            AddStone(PlayerIdMapping.FromLegacyTeam(team), amount);

        public static bool TrySpendWood(float amount)
        {
            return TrySpendWood(UnitTeam.Player, amount);
        }

        public static bool TrySpendWood(PlayerId playerId, float amount)
        {
            if (instance == null || amount <= 0f)
                return false;

            int index = ResolveIndex(playerId);
            if (instance.wood[index] < amount)
                return false;

            instance.wood[index] -= amount;
            return true;
        }

        public static bool TrySpendWood(UnitTeam team, float amount) =>
            TrySpendWood(PlayerIdMapping.FromLegacyTeam(team), amount);

        public static bool TrySpendFood(PlayerId playerId, float amount)
        {
            if (instance == null || amount <= 0f)
                return false;

            int index = ResolveIndex(playerId);
            if (instance.food[index] < amount)
                return false;

            instance.food[index] -= amount;
            return true;
        }

        public static bool TrySpendFood(UnitTeam team, float amount) =>
            TrySpendFood(PlayerIdMapping.FromLegacyTeam(team), amount);

        public static bool TrySpendGold(PlayerId playerId, float amount)
        {
            if (instance == null || amount <= 0f)
                return false;

            int index = ResolveIndex(playerId);
            if (instance.gold[index] < amount)
                return false;

            instance.gold[index] -= amount;
            return true;
        }

        public static bool TrySpendGold(UnitTeam team, float amount) =>
            TrySpendGold(PlayerIdMapping.FromLegacyTeam(team), amount);

        public static bool TrySpendStone(PlayerId playerId, float amount)
        {
            if (instance == null || amount <= 0f)
                return false;

            int index = ResolveIndex(playerId);
            if (instance.stone[index] < amount)
                return false;

            instance.stone[index] -= amount;
            return true;
        }

        public static bool TrySpendStone(UnitTeam team, float amount) =>
            TrySpendStone(PlayerIdMapping.FromLegacyTeam(team), amount);
    }
}

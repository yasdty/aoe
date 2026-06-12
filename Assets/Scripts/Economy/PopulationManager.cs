using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class PopulationManager : MonoBehaviour
    {
        const int PlayerCount = 4;

        static PopulationManager instance;

        [SerializeField] int initialHousingCap = 5;

        readonly int[] housingCap = new int[PlayerCount];

        public static int CurrentPopulation => GetCurrentPopulation(UnitTeam.Player);
        public static int MaxPopulation => GetMaxPopulation(UnitTeam.Player);

        void Awake()
        {
            instance = this;
            for (int i = 0; i < PlayerCount; i++)
                housingCap[i] = initialHousingCap;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        static int ResolveIndex(PlayerId playerId) => PlayerIdMapping.ToIndex(playerId);

        public static int GetCurrentPopulation(PlayerId playerId) =>
            UnitManager.GetUnitCountForPlayer(playerId);

        public static int GetCurrentPopulation(UnitTeam team) =>
            GetCurrentPopulation(PlayerIdMapping.FromLegacyTeam(team));

        public static int GetMaxPopulation(PlayerId playerId)
        {
            if (instance == null)
                return 0;

            return instance.housingCap[ResolveIndex(playerId)];
        }

        public static int GetMaxPopulation(UnitTeam team) =>
            GetMaxPopulation(PlayerIdMapping.FromLegacyTeam(team));

        public static bool CanTrainUnit()
        {
            return CanTrainUnit(UnitTeam.Player);
        }

        public static bool CanTrainUnit(PlayerId playerId) =>
            GetCurrentPopulation(playerId) < GetMaxPopulation(playerId);

        public static bool CanTrainUnit(UnitTeam team) =>
            CanTrainUnit(PlayerIdMapping.FromLegacyTeam(team));

        public static void AddHousing(int amount)
        {
            AddHousing(UnitTeam.Player, amount);
        }

        public static void AddHousing(PlayerId playerId, int amount)
        {
            if (instance == null || amount <= 0)
                return;

            instance.housingCap[ResolveIndex(playerId)] += amount;
        }

        public static void AddHousing(UnitTeam team, int amount) =>
            AddHousing(PlayerIdMapping.FromLegacyTeam(team), amount);

        public static void RemoveHousing(PlayerId playerId, int amount)
        {
            if (instance == null || amount <= 0)
                return;

            int index = ResolveIndex(playerId);
            instance.housingCap[index] = Mathf.Max(0, instance.housingCap[index] - amount);
        }

        public static void RemoveHousing(UnitTeam team, int amount) =>
            RemoveHousing(PlayerIdMapping.FromLegacyTeam(team), amount);
    }
}

using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class PopulationManager : MonoBehaviour
    {
        static PopulationManager instance;

        [SerializeField] int initialHousingCap = 5;

        int playerHousingCap;
        int enemyHousingCap;

        public static int CurrentPopulation => GetCurrentPopulation(UnitTeam.Player);
        public static int MaxPopulation => GetMaxPopulation(UnitTeam.Player);

        void Awake()
        {
            instance = this;
            playerHousingCap = initialHousingCap;
            enemyHousingCap = initialHousingCap;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static int GetCurrentPopulation(UnitTeam team)
        {
            return UnitManager.GetUnitCountForTeam(team);
        }

        public static int GetMaxPopulation(UnitTeam team)
        {
            if (instance == null)
                return 0;

            return team == UnitTeam.Enemy ? instance.enemyHousingCap : instance.playerHousingCap;
        }

        public static bool CanTrainUnit()
        {
            return CanTrainUnit(UnitTeam.Player);
        }

        public static bool CanTrainUnit(UnitTeam team)
        {
            return GetCurrentPopulation(team) < GetMaxPopulation(team);
        }

        public static void AddHousing(int amount)
        {
            AddHousing(UnitTeam.Player, amount);
        }

        public static void AddHousing(UnitTeam team, int amount)
        {
            if (instance == null || amount <= 0)
                return;

            if (team == UnitTeam.Enemy)
                instance.enemyHousingCap += amount;
            else
                instance.playerHousingCap += amount;
        }
    }
}

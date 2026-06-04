using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class PopulationManager : MonoBehaviour
    {
        static PopulationManager instance;

        [SerializeField] int initialHousingCap = 5;

        int housingCap;

        public static int CurrentPopulation => UnitManager.UnitCount;
        public static int MaxPopulation => instance != null ? instance.housingCap : 0;

        void Awake()
        {
            instance = this;
            housingCap = initialHousingCap;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static bool CanTrainUnit()
        {
            return CurrentPopulation < MaxPopulation;
        }

        public static void AddHousing(int amount)
        {
            if (instance == null || amount <= 0)
                return;

            instance.housingCap += amount;
        }
    }
}

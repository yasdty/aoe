using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class ResourceManager : MonoBehaviour
    {
        static ResourceManager instance;

        float playerWood;
        float enemyWood;

        public static float Wood => GetWood(UnitTeam.Player);

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static float GetWood(UnitTeam team)
        {
            if (instance == null)
                return 0f;

            return team == UnitTeam.Enemy ? instance.enemyWood : instance.playerWood;
        }

        public static void AddWood(float amount)
        {
            AddWood(UnitTeam.Player, amount);
        }

        public static void AddWood(UnitTeam team, float amount)
        {
            if (instance == null || amount <= 0f)
                return;

            if (team == UnitTeam.Enemy)
                instance.enemyWood += amount;
            else
                instance.playerWood += amount;
        }

        public static bool TrySpendWood(float amount)
        {
            return TrySpendWood(UnitTeam.Player, amount);
        }

        public static bool TrySpendWood(UnitTeam team, float amount)
        {
            if (instance == null || amount <= 0f)
                return false;

            if (team == UnitTeam.Enemy)
            {
                if (instance.enemyWood < amount)
                    return false;

                instance.enemyWood -= amount;
                return true;
            }

            if (instance.playerWood < amount)
                return false;

            instance.playerWood -= amount;
            return true;
        }
    }
}

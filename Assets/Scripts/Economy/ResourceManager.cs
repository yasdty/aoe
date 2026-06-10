using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Economy
{
    public class ResourceManager : MonoBehaviour
    {
        static ResourceManager instance;

        [SerializeField] float initialPlayerFood = 200f;
        [SerializeField] float initialEnemyFood;

        float playerWood;
        float enemyWood;
        float playerFood;
        float enemyFood;
        float playerGold;
        float enemyGold;
        float playerStone;
        float enemyStone;

        public static float Wood => GetWood(UnitTeam.Player);
        public static float Food => GetFood(UnitTeam.Player);
        public static float Gold => GetGold(UnitTeam.Player);
        public static float Stone => GetStone(UnitTeam.Player);

        void Awake()
        {
            instance = this;
            playerFood = initialPlayerFood;
            enemyFood = initialEnemyFood;
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

        public static float GetFood(UnitTeam team)
        {
            if (instance == null)
                return 0f;

            return team == UnitTeam.Enemy ? instance.enemyFood : instance.playerFood;
        }

        public static float GetGold(UnitTeam team)
        {
            if (instance == null)
                return 0f;

            return team == UnitTeam.Enemy ? instance.enemyGold : instance.playerGold;
        }

        public static float GetStone(UnitTeam team)
        {
            if (instance == null)
                return 0f;

            return team == UnitTeam.Enemy ? instance.enemyStone : instance.playerStone;
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

        public static void AddFood(UnitTeam team, float amount)
        {
            if (instance == null || amount <= 0f)
                return;

            if (team == UnitTeam.Enemy)
                instance.enemyFood += amount;
            else
                instance.playerFood += amount;
        }

        public static void AddGold(UnitTeam team, float amount)
        {
            if (instance == null || amount <= 0f)
                return;

            if (team == UnitTeam.Enemy)
                instance.enemyGold += amount;
            else
                instance.playerGold += amount;
        }

        public static void AddStone(UnitTeam team, float amount)
        {
            if (instance == null || amount <= 0f)
                return;

            if (team == UnitTeam.Enemy)
                instance.enemyStone += amount;
            else
                instance.playerStone += amount;
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

        public static bool TrySpendFood(UnitTeam team, float amount)
        {
            if (instance == null || amount <= 0f)
                return false;

            if (team == UnitTeam.Enemy)
            {
                if (instance.enemyFood < amount)
                    return false;

                instance.enemyFood -= amount;
                return true;
            }

            if (instance.playerFood < amount)
                return false;

            instance.playerFood -= amount;
            return true;
        }
    }
}

using UnityEngine;

namespace AoE.RTS.Economy
{
    public class ResourceManager : MonoBehaviour
    {
        static ResourceManager instance;

        float wood;

        public static float Wood => instance != null ? instance.wood : 0f;

        void Awake()
        {
            instance = this;
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static void AddWood(float amount)
        {
            if (instance == null || amount <= 0f)
                return;

            instance.wood += amount;
        }

        public static bool TrySpendWood(float amount)
        {
            if (instance == null || amount <= 0f || instance.wood < amount)
                return false;

            instance.wood -= amount;
            return true;
        }
    }
}

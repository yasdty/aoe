using UnityEngine;

namespace AoE.RTS.Core
{
    public static class GameplayBalance
    {
        const float DebugBuildTimeMultiplier = 0.1f;
        const float DebugCostMultiplier = 0.3f;
        const float DebugCpuDelayMultiplier = 0.1f;

        static GameplayBalanceMode mode = GameplayBalanceMode.Debug;

        public static GameplayBalanceMode Mode => mode;

        public static void SetMode(GameplayBalanceMode newMode)
        {
            mode = newMode;
        }

        public static float ScaleBuildTime(float baseSeconds)
        {
            if (baseSeconds <= 0f)
                return baseSeconds;

            return mode == GameplayBalanceMode.Debug
                ? baseSeconds * DebugBuildTimeMultiplier
                : baseSeconds;
        }

        public static float ScaleResourceCost(float baseCost)
        {
            if (baseCost <= 0f)
                return 0f;

            float scaled = mode == GameplayBalanceMode.Debug
                ? baseCost * DebugCostMultiplier
                : baseCost;

            return Mathf.Max(0f, Mathf.Ceil(scaled));
        }

        public static float ScaleCpuDelaySeconds(float baseSeconds)
        {
            if (baseSeconds <= 0f)
                return baseSeconds;

            return mode == GameplayBalanceMode.Debug
                ? baseSeconds * DebugCpuDelayMultiplier
                : baseSeconds;
        }
    }
}

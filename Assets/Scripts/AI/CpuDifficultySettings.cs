using AoE.RTS.Core;

namespace AoE.RTS.AI
{
    public static class CpuDifficultySettings
    {
        public static CpuDifficulty EffectiveDifficulty
        {
            get
            {
                if (GameplayBalance.Mode == GameplayBalanceMode.Debug)
                    return CpuDifficulty.Easy;

                return GameSessionManager.CpuDifficulty;
            }
        }

        public static CpuDifficultyProfile Current => CpuDifficultyProfile.For(EffectiveDifficulty);
    }
}

namespace AoE.RTS.Core
{
    public enum CpuDifficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Hardest = 3
    }

    public readonly struct CpuDifficultyProfile
    {
        public const int DynamicAttackThreshold = -1;

        public float ReactionDelay { get; }
        public float DecisionInterval { get; }
        public int MaxActionsPerCycle { get; }
        public int VillagerTarget { get; }
        public float ArmyRatio { get; }
        public int AttackThreshold { get; }
        public float AttackConfidence { get; }
        public float BarracksUnlockSeconds { get; }

        public CpuDifficultyProfile(
            float reactionDelay,
            float decisionInterval,
            int maxActionsPerCycle,
            int villagerTarget,
            float armyRatio,
            int attackThreshold,
            float attackConfidence,
            float barracksUnlockSeconds)
        {
            ReactionDelay = reactionDelay;
            DecisionInterval = decisionInterval;
            MaxActionsPerCycle = maxActionsPerCycle;
            VillagerTarget = villagerTarget;
            ArmyRatio = armyRatio;
            AttackThreshold = attackThreshold;
            AttackConfidence = attackConfidence;
            BarracksUnlockSeconds = barracksUnlockSeconds;
        }

        public int ResolveAttackThreshold(int opponentArmyCount)
        {
            if (AttackThreshold != DynamicAttackThreshold)
                return AttackThreshold;

            return UnityEngine.Mathf.Max(20, opponentArmyCount + 8);
        }

        public int ResolveTargetArmyCount(int villagerCount)
        {
            if (ArmyRatio <= 0f)
                return ResolveAttackThreshold(0);

            int fromRatio = UnityEngine.Mathf.RoundToInt(villagerCount * ArmyRatio);
            return UnityEngine.Mathf.Max(ResolveAttackThreshold(0), fromRatio);
        }

        public static CpuDifficultyProfile For(CpuDifficulty difficulty)
        {
            return difficulty switch
            {
                CpuDifficulty.Easy => new CpuDifficultyProfile(
                    reactionDelay: 3f,
                    decisionInterval: 5f,
                    maxActionsPerCycle: 1,
                    villagerTarget: 20,
                    armyRatio: 0.20f,
                    attackThreshold: 8,
                    attackConfidence: 1.5f,
                    barracksUnlockSeconds: 120f),
                CpuDifficulty.Normal => new CpuDifficultyProfile(
                    reactionDelay: 1.5f,
                    decisionInterval: 3f,
                    maxActionsPerCycle: 2,
                    villagerTarget: 40,
                    armyRatio: 0.30f,
                    attackThreshold: 15,
                    attackConfidence: 1.2f,
                    barracksUnlockSeconds: 60f),
                CpuDifficulty.Hard => new CpuDifficultyProfile(
                    reactionDelay: 0.75f,
                    decisionInterval: 2f,
                    maxActionsPerCycle: 3,
                    villagerTarget: 60,
                    armyRatio: 0.40f,
                    attackThreshold: 25,
                    attackConfidence: 1f,
                    barracksUnlockSeconds: 30f),
                CpuDifficulty.Hardest => new CpuDifficultyProfile(
                    reactionDelay: 0.25f,
                    decisionInterval: 1f,
                    maxActionsPerCycle: 5,
                    villagerTarget: 80,
                    armyRatio: 0.50f,
                    attackThreshold: DynamicAttackThreshold,
                    attackConfidence: 0.8f,
                    barracksUnlockSeconds: 15f),
                _ => For(CpuDifficulty.Normal)
            };
        }
    }
}

using UnityEngine;

namespace AoE.RTS.View
{
    public static class UnitAnimationParameters
    {
        public const string Speed = "Speed";
        public const string IsGathering = "IsGathering";
        public const string IsAttacking = "IsAttacking";
        public const string IsDead = "IsDead";

        public static readonly int SpeedHash = Animator.StringToHash(Speed);
        public static readonly int IsGatheringHash = Animator.StringToHash(IsGathering);
        public static readonly int IsAttackingHash = Animator.StringToHash(IsAttacking);
        public static readonly int IsDeadHash = Animator.StringToHash(IsDead);
    }
}

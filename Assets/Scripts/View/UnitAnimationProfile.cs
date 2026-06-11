using AoE.RTS.Combat;
using AoE.RTS.Units;

namespace AoE.RTS.View
{
    public enum UnitAnimationProfile
    {
        Villager = 0,
        Militia = 1,
        Archer = 2
    }

    public static class UnitAnimationProfileResolver
    {
        public static UnitAnimationProfile GetProfile(UnitData data)
        {
            if (data == null || !data.CanAttack)
                return UnitAnimationProfile.Villager;

            if (data.attackDamageType == AttackDamageType.Pierce && data.attackRange > 3f)
                return UnitAnimationProfile.Archer;

            return UnitAnimationProfile.Militia;
        }

        public static string GetControllerResourcePath(UnitAnimationProfile profile)
        {
            switch (profile)
            {
                case UnitAnimationProfile.Militia:
                    return "UnitAnimation/Militia";
                case UnitAnimationProfile.Archer:
                    return "UnitAnimation/Archer";
                default:
                    return "UnitAnimation/Villager";
            }
        }
    }
}

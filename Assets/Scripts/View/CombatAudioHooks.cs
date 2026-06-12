using UnityEngine;

namespace AoE.RTS.View
{
    public static class CombatAudioHooks
    {
        static CombatFeedbackView view;

        internal static void Bind(CombatFeedbackView boundView)
        {
            view = boundView;
        }

        public static void PlayMeleeHit(Vector3 worldPosition)
        {
            view?.PlayMeleeHit(worldPosition);
        }

        public static void PlayRangedHit(Vector3 worldPosition)
        {
            view?.PlayRangedHit(worldPosition);
        }

        public static void PlayUnitDeath(Vector3 worldPosition)
        {
            view?.PlayUnitDeath(worldPosition);
        }
    }
}

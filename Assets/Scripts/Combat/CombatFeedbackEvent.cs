using UnityEngine;

namespace AoE.RTS.Combat
{
    public struct CombatFeedbackEvent
    {
        public Vector3 sourceWorldPosition;
        public Vector3 targetWorldPosition;
        public CombatFeedbackKind kind;
        public bool targetWasKilled;
    }
}

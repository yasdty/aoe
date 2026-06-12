using System;

namespace AoE.RTS.Combat
{
    public static class CombatFeedbackBus
    {
        public static event Action<CombatFeedbackEvent> OnFeedback;

        public static void Raise(CombatFeedbackEvent feedbackEvent)
        {
            OnFeedback?.Invoke(feedbackEvent);
        }
    }
}

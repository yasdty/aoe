using UnityEngine;

namespace AoE.RTS.Economy
{
    public interface IHuntableFoodResource
    {
        bool IsDepleted { get; }
        Vector3 GetGatherPosition();
        float TakeFood(float amount);
    }
}

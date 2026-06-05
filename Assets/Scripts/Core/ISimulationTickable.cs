namespace AoE.RTS.Core
{
    public interface ISimulationTickable
    {
        void TickSimulation(float fixedDeltaTime);
    }
}

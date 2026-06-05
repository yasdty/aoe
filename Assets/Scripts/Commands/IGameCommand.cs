namespace AoE.RTS.Commands
{
    public interface IGameCommand
    {
        string DebugName { get; }

        void Execute();
    }
}

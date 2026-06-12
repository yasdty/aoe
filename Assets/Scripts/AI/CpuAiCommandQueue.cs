using AoE.RTS.Commands;
using AoE.RTS.Core;

namespace AoE.RTS.AI
{
    public static class CpuAiCommandQueue
    {
        public static void Enqueue(PlayerId playerId, IGameCommand command)
        {
            if (command == null)
                return;

            CommandQueue.Enqueue(new PlayerScopedCommand(playerId, command));
        }
    }
}

using AoE.RTS.Core;

namespace AoE.RTS.Commands
{
    public interface IPlayerCommand
    {
        PlayerId IssuingPlayerId { get; }
    }
}

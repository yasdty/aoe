using System.Collections.Generic;
using AoE.RTS.Core;

namespace AoE.RTS.Commands
{
    public sealed class PlayerScopedCommand : IGameCommand, IPlayerCommand, IEntityIdSource
    {
        readonly IGameCommand inner;

        public PlayerId IssuingPlayerId { get; }

        public string DebugName => inner != null ? inner.DebugName : "NullCommand";

        public PlayerScopedCommand(PlayerId issuingPlayerId, IGameCommand inner)
        {
            IssuingPlayerId = issuingPlayerId;
            this.inner = inner;
        }

        public void Execute()
        {
            inner?.Execute();
        }

        public void CollectEntityIds(List<int> entityIds)
        {
            if (inner is IEntityIdSource entityIdSource)
                entityIdSource.CollectEntityIds(entityIds);
        }
    }
}

using System.Collections.Generic;

namespace AoE.RTS.Commands
{
    public interface IEntityIdSource
    {
        void CollectEntityIds(List<int> entityIds);
    }
}

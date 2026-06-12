using System.Collections.Generic;
using AoE.RTS.Core;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Commands
{
    public static class CommandLog
    {
        public struct CommandRecord
        {
            public int tick;
            public string commandType;
            public UnitTeam team;
            public string entityIds;
        }

        static readonly List<CommandRecord> records = new List<CommandRecord>();
        static readonly List<int> entityIdScratch = new List<int>();

        public static IReadOnlyList<CommandRecord> Records => records;

        public static void Record(int tick, IGameCommand command)
        {
            if (command == null)
                return;

            entityIdScratch.Clear();
            if (command is IEntityIdSource entityIdSource)
                entityIdSource.CollectEntityIds(entityIdScratch);

            string entityIdSummary = FormatEntityIds(entityIdScratch);

            PlayerId issuingPlayerId = command is IPlayerCommand playerCommand
                ? playerCommand.IssuingPlayerId
                : PlayerId.Player0;
            UnitTeam team = PlayerIdMapping.ToLegacyTeam(issuingPlayerId);

            records.Add(new CommandRecord
            {
                tick = tick,
                commandType = command.DebugName,
                team = team,
                entityIds = entityIdSummary
            });

            if (entityIdSummary.Length > 0)
                Debug.Log($"CommandLog: tick={tick} {command.DebugName} player={issuingPlayerId} entities={entityIdSummary}");
            else
                Debug.Log($"CommandLog: tick={tick} {command.DebugName} player={issuingPlayerId}");
        }

        static string FormatEntityIds(List<int> entityIds)
        {
            if (entityIds == null || entityIds.Count == 0)
                return string.Empty;

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < entityIds.Count; i++)
            {
                if (i > 0)
                    builder.Append(',');

                builder.Append(entityIds[i]);
            }

            return builder.ToString();
        }

        public static void Clear()
        {
            records.Clear();
        }
    }
}

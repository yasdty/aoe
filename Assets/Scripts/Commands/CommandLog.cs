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
        }

        static readonly List<CommandRecord> records = new List<CommandRecord>();

        public static IReadOnlyList<CommandRecord> Records => records;

        public static void Record(int tick, IGameCommand command)
        {
            if (command == null)
                return;

            records.Add(new CommandRecord
            {
                tick = tick,
                commandType = command.DebugName,
                team = UnitTeam.Player
            });

            Debug.Log($"CommandLog: tick={tick} {command.DebugName}");
        }

        public static void Clear()
        {
            records.Clear();
        }
    }
}

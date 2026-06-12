using AoE.RTS.Units;

namespace AoE.RTS.Core
{
    public static class PlayerIdMapping
    {
        public static bool IsLocalHuman(PlayerId id) => id == PlayerId.Player0;

        public static UnitTeam ToLegacyTeam(PlayerId id)
        {
            return id == PlayerId.Player0 ? UnitTeam.Player : UnitTeam.Enemy;
        }

        public static PlayerId FromLegacyTeam(UnitTeam team)
        {
            return team == UnitTeam.Player ? PlayerId.Player0 : PlayerId.Player1;
        }

        public static int ToIndex(PlayerId id) => (int)id;

        public static bool IsHumanPlayer(PlayerId id) => id == PlayerId.Player0;
    }
}

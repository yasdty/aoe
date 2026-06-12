using System.Collections.Generic;

namespace AoE.RTS.Core
{
    public enum MatchMode
    {
        OneVsOneCpu = 0,
        FourPlayerFfa = 1
    }

    public static class MatchSettings
    {
        static readonly PlayerId[] OneVsOnePlayers = { PlayerId.Player0, PlayerId.Player1 };
        static readonly PlayerId[] FourPlayerPlayers =
        {
            PlayerId.Player0,
            PlayerId.Player1,
            PlayerId.Player2,
            PlayerId.Player3
        };

        public static MatchMode Mode =>
            GameSessionManager.Instance != null
                ? GameSessionManager.Instance.MatchMode
                : MatchMode.FourPlayerFfa;

        public static bool IsCpu(PlayerId id) => id != PlayerId.Player0;

        public static IReadOnlyList<PlayerId> ActivePlayers =>
            Mode == MatchMode.FourPlayerFfa ? FourPlayerPlayers : OneVsOnePlayers;

        public static bool IsActivePlayer(PlayerId id)
        {
            IReadOnlyList<PlayerId> active = ActivePlayers;
            for (int i = 0; i < active.Count; i++)
            {
                if (active[i] == id)
                    return true;
            }

            return false;
        }

        public static bool IsOpponentOfHuman(PlayerId id) =>
            id != PlayerId.Player0 && IsActivePlayer(id);
    }
}

using AoE.RTS.Buildings;
using AoE.RTS.Economy;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Core
{
    public enum MatchState
    {
        Playing = 0,
        Victory = 1,
        Defeat = 2
    }

    public class GameSessionManager : MonoBehaviour
    {
        static GameSessionManager instance;
        static MatchState staticState = MatchState.Playing;
        static GameAge playerAge = GameAge.Dark;
        static GameAge enemyAge = GameAge.Dark;

        [SerializeField] GameplayBalanceMode balanceMode = GameplayBalanceMode.Debug;
        [SerializeField] CpuAttackPace cpuAttackPace = CpuAttackPace.Relaxed;
        [SerializeField] AgeData feudalAgeData;

        MatchState state = MatchState.Playing;

        public static MatchState State => staticState;
        public static bool IsGameOver => staticState != MatchState.Playing;
        public static GameplayBalanceMode BalanceMode => GameplayBalance.Mode;
        public static CpuAttackPace CpuAttackPace =>
            instance != null ? instance.cpuAttackPace : CpuAttackPace.Relaxed;

        void Awake()
        {
            instance = this;
            staticState = MatchState.Playing;
            state = MatchState.Playing;
            playerAge = GameAge.Dark;
            enemyAge = GameAge.Dark;
            TechnologyState.Reset();
            GameplayBalance.SetMode(balanceMode);
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        public static GameAge GetAge(UnitTeam team)
        {
            return team == UnitTeam.Enemy ? enemyAge : playerAge;
        }

        public static bool CanBuild(PlacedBuildingData data, UnitTeam team)
        {
            if (data == null)
                return false;

            return GetAge(team) >= data.requiredAge;
        }

        public static bool CanAgeUpToFeudal(UnitTeam team)
        {
            return GetAge(team) == GameAge.Dark && instance != null && instance.feudalAgeData != null;
        }

        public static bool TryAgeUpToFeudal(TownCenter townCenter)
        {
            if (instance == null || townCenter == null || instance.feudalAgeData == null)
                return false;

            UnitTeam team = townCenter.Team;
            if (GetAge(team) >= GameAge.Feudal)
                return false;

            AgeData ageData = instance.feudalAgeData;
            float foodCost = GameplayBalance.ScaleResourceCost(ageData.upgradeFoodCost);
            float goldCost = GameplayBalance.ScaleResourceCost(ageData.upgradeGoldCost);

            if (foodCost > 0f && ResourceManager.GetFood(team) < foodCost)
                return false;

            if (goldCost > 0f && ResourceManager.GetGold(team) < goldCost)
                return false;

            if (foodCost > 0f && !ResourceManager.TrySpendFood(team, foodCost))
                return false;

            if (goldCost > 0f && !ResourceManager.TrySpendGold(team, goldCost))
            {
                if (foodCost > 0f)
                    ResourceManager.AddFood(team, foodCost);
                return false;
            }

            SetAge(team, GameAge.Feudal);
            Debug.Log($"GameSession: {team} advanced to Feudal Age");
            return true;
        }

        public static bool TryAgeUpForTeam(UnitTeam team)
        {
            TownCenter townCenter = ProductionManager.GetTownCenterForTeam(team);
            if (townCenter == null)
                return false;

            return TryAgeUpToFeudal(townCenter);
        }

        static void SetAge(UnitTeam team, GameAge age)
        {
            if (team == UnitTeam.Enemy)
                enemyAge = age;
            else
                playerAge = age;
        }

        public void ApplyBalanceModeFromInspector()
        {
            GameplayBalance.SetMode(balanceMode);
        }

        public static void NotifyTownCenterDestroyed(UnitTeam destroyedTeam)
        {
            if (IsGameOver)
                return;

            if (destroyedTeam == UnitTeam.Player)
                staticState = MatchState.Defeat;
            else if (destroyedTeam == UnitTeam.Enemy)
                staticState = MatchState.Victory;
            else
                return;

            if (instance != null)
                instance.state = staticState;

            Debug.Log(staticState == MatchState.Victory
                ? "GameSession: VICTORY"
                : "GameSession: DEFEAT");
        }
    }
}

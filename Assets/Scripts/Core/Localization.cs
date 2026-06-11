using System.Collections.Generic;
using AoE.RTS.Buildings;
using AoE.RTS.Units;
using UnityEngine;

namespace AoE.RTS.Core
{
    public static class Localization
    {
        const string PlayerPrefsKey = "AoE.GameLanguage";

        static readonly Dictionary<string, string> english = new Dictionary<string, string>();
        static readonly Dictionary<string, string> japanese = new Dictionary<string, string>();
        static GameLanguage currentLanguage = GameLanguage.English;
        static bool registered;

        static Localization()
        {
            EnsureRegistered();
            currentLanguage = LoadSavedLanguage();
        }

        public static GameLanguage CurrentLanguage
        {
            get => currentLanguage;
            set
            {
                if (currentLanguage == value)
                    return;

                currentLanguage = value;
                PlayerPrefs.SetInt(PlayerPrefsKey, (int)value);
                PlayerPrefs.Save();
            }
        }

        public static int EntryCount
        {
            get
            {
                EnsureRegistered();
                return english.Count;
            }
        }

        public static void EnsureRegistered()
        {
            if (registered)
                return;

            english.Clear();
            japanese.Clear();
            LanguageMapBootstrap.Register(english, japanese);
            registered = true;
        }

        public static void ToggleLanguage()
        {
            CurrentLanguage = CurrentLanguage == GameLanguage.English
                ? GameLanguage.Japanese
                : GameLanguage.English;
        }

        public static string Get(string key)
        {
            EnsureRegistered();
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            Dictionary<string, string> table = currentLanguage == GameLanguage.Japanese ? japanese : english;
            if (table.TryGetValue(key, out string value))
                return value;

            if (currentLanguage != GameLanguage.English && english.TryGetValue(key, out string fallback))
                return fallback;

            return key;
        }

        public static string Format(string key, params object[] args)
        {
            string template = Get(key);
            if (args == null || args.Length == 0)
                return template;

            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template;
            }
        }

        public static string BuildingName(PlacedBuildingKind kind)
        {
            return Get(LanguageMapBootstrap.BuildingKey(kind));
        }

        public static string BuildingName(PlacedBuildingData data)
        {
            return data != null ? BuildingName(data.kind) : Get("building.generic");
        }

        public static string UnitName(UnitData data)
        {
            if (data == null)
                return Get("unit.generic");

            string key = LanguageMapBootstrap.UnitKeyFromDisplayName(data.displayName);
            return key != null ? Get(key) : data.displayName;
        }

        public static string UnitName(Unit unit)
        {
            return UnitName(unit != null ? unit.Data : null);
        }

        public static string AgeName(GameAge age)
        {
            return age >= GameAge.Feudal ? Get("age.feudal") : Get("age.dark");
        }

        public static string FormatPlacementCost(PlacedBuildingData data)
        {
            if (data == null)
                return "0";

            int wood = Mathf.CeilToInt(data.ScaledWoodCost);
            int stone = Mathf.CeilToInt(data.ScaledStoneCost);
            if (wood > 0 && stone > 0)
                return Format("ui.cost_wood_stone", wood, stone);

            if (stone > 0)
                return Format("ui.cost_stone", stone);

            return Format("ui.cost_wood", wood);
        }

        public static string CurrentLanguageLabel()
        {
            return CurrentLanguage == GameLanguage.Japanese
                ? Get("ui.language_ja")
                : Get("ui.language_en");
        }

        public static string LocalizeDisplayName(string englishName)
        {
            string key = LanguageMapBootstrap.UnitKeyFromDisplayName(englishName);
            return key != null ? Get(key) : englishName;
        }

        static GameLanguage LoadSavedLanguage()
        {
            if (!PlayerPrefs.HasKey(PlayerPrefsKey))
                return GameLanguage.English;

            int value = PlayerPrefs.GetInt(PlayerPrefsKey, (int)GameLanguage.English);
            return value == (int)GameLanguage.Japanese ? GameLanguage.Japanese : GameLanguage.English;
        }
    }
}

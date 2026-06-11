using System.Collections.Generic;
using AoE.RTS.Buildings;

namespace AoE.RTS.Core
{
    static class LanguageMapBootstrap
    {
        public static void Register(Dictionary<string, string> english, Dictionary<string, string> japanese)
        {
            Add(english, japanese, "resource.wood", "Wood", "木材");
            Add(english, japanese, "resource.food", "Food", "食料");
            Add(english, japanese, "resource.gold", "Gold", "金");
            Add(english, japanese, "resource.stone", "Stone", "石");

            Add(english, japanese, "building.house", "House", "家");
            Add(english, japanese, "building.barracks", "Barracks", "兵舎");
            Add(english, japanese, "building.farm", "Farm", "農場");
            Add(english, japanese, "building.lumber_camp", "Lumber Camp", "伐採所");
            Add(english, japanese, "building.mining_camp", "Mining Camp", "採掘所");
            Add(english, japanese, "building.mill", "Mill", "風車");
            Add(english, japanese, "building.archery_range", "Archery Range", "弓工房");
            Add(english, japanese, "building.stable", "Stable", "厩舎");
            Add(english, japanese, "building.blacksmith", "Blacksmith", "鍛冶場");
            Add(english, japanese, "building.palisade", "Palisade", "フェンス");
            Add(english, japanese, "building.stone_wall", "Stone Wall", "石の城壁");
            Add(english, japanese, "building.gate", "Gate", "門");
            Add(english, japanese, "building.watch_tower", "Watch Tower", "望楼");
            Add(english, japanese, "building.market", "Market", "市場");
            Add(english, japanese, "building.town_center", "Town Center", "町の中心");
            Add(english, japanese, "building.generic", "Building", "建築物");

            Add(english, japanese, "unit.villager", "Villager", "村民");
            Add(english, japanese, "unit.militia", "Militia", "民兵");
            Add(english, japanese, "unit.man_at_arms", "Man-at-Arms", "長剣兵");
            Add(english, japanese, "unit.spearman", "Spearman", "槍兵");
            Add(english, japanese, "unit.archer", "Archer", "弓兵");
            Add(english, japanese, "unit.cavalry", "Cavalry", "騎士");
            Add(english, japanese, "unit.scout", "Scout", "スカウト");
            Add(english, japanese, "unit.generic", "Unit", "ユニット");

            Add(english, japanese, "age.dark", "Dark Age", "暗黒時代");
            Add(english, japanese, "age.feudal", "Feudal Age", "城主時代");

            Add(english, japanese, "tech.infantry_upgrade", "Infantry Upgrade", "歩兵強化");

            Add(english, japanese, "resource.node.tree", "Tree", "木");
            Add(english, japanese, "resource.node.berry_bush", "Berry Bush", "ベリー畑");
            Add(english, japanese, "resource.node.deer", "Deer", "鹿");
            Add(english, japanese, "resource.node.sheep", "Sheep", "羊");
            Add(english, japanese, "resource.node.boar", "Boar", "イノシシ");
            Add(english, japanese, "resource.node.gold_mine", "Gold Mine", "金鉱");
            Add(english, japanese, "resource.node.stone_mine", "Stone Mine", "石切り場");
            Add(english, japanese, "resource.node.generic", "Resource", "資源");

            Add(english, japanese, "stance.aggressive", "Aggressive", "攻撃的");
            Add(english, japanese, "stance.defensive", "Defensive", "防御的");
            Add(english, japanese, "stance.stand_ground", "Stand Ground", "静止");

            Add(english, japanese, "damage.melee", "Melee", "近接");
            Add(english, japanese, "damage.pierce", "Pierce", "貫通");

            Add(english, japanese, "ui.pop", "Pop", "人口");
            Add(english, japanese, "ui.resource_amount", "{0}: {1}", "{0}: {1}");
            Add(english, japanese, "ui.victory", "VICTORY", "勝利");
            Add(english, japanese, "ui.defeat", "DEFEAT", "敗北");
            Add(english, japanese, "ui.restart_hint", "Press R to restart", "R キーで再開");
            Add(english, japanese, "ui.language_label", "Language", "言語");
            Add(english, japanese, "ui.language_en", "EN", "EN");
            Add(english, japanese, "ui.language_ja", "JA", "JA");
            Add(english, japanese, "ui.language_toggle", "Language: {0} (L)", "言語: {0} (L)");

            Add(english, japanese, "ui.build_hotkey_wood", "Build {0} ({1}) ({2} {3})", "{0}を建てる ({1})（{2} {3}）");
            Add(english, japanese, "ui.build_wood", "Build {0} ({1} {2})", "{0}を建てる（{1} {2}）");
            Add(english, japanese, "ui.build_cost", "Build {0} ({1})", "{0}を建てる（{1}）");
            Add(english, japanese, "ui.locked_age", "{0} ({1})", "{0}（{1}）");
            Add(english, japanese, "ui.locked_gate", "{0} ({1} + wall)", "{0}（{1}・壁必要）");
            Add(english, japanese, "ui.town_center_max", "{0} (Max 2)", "{0}（最大2）");
            Add(english, japanese, "ui.placement_hint", "Click to place. Walls: drag a line. Esc / Right-click to cancel.",
                "クリックで配置。壁: ドラッグで列。Esc / 右クリックでキャンセル。");
            Add(english, japanese, "ui.need_wood", "Need more Wood.", "木材が足りません。");

            Add(english, japanese, "ui.cost_wood", "{0} Wood", "{0} 木材");
            Add(english, japanese, "ui.cost_stone", "{0} Stone", "{0} 石");
            Add(english, japanese, "ui.cost_wood_stone", "{0} Wood, {1} Stone", "{0} 木材, {1} 石");

            Add(english, japanese, "ui.town_center", "Town Center", "町の中心");
            Add(english, japanese, "ui.age_label", "Age: {0}", "時代: {0}");
            Add(english, japanese, "ui.create_villager", "Create Villager (Q) ({0} Food)", "村民を作る (Q)（{0} 食料）");
            Add(english, japanese, "ui.age_up_feudal", "Age Up to Feudal ({0} Food, {1} Gold)",
                "城主時代へ ({0} 食料, {1} 金)");
            Add(english, japanese, "ui.need_food_gold_feudal", "Need Food + Gold for Feudal Age",
                "城主時代には食料と金が必要");

            Add(english, japanese, "ui.queue_full", "Queue full", "キューが一杯");
            Add(english, japanese, "ui.population_full", "Population full", "人口上限");
            Add(english, japanese, "ui.need_food", "Need more Food", "食料が足りません");
            Add(english, japanese, "ui.need_gold", "Need more Gold", "金が足りません");
            Add(english, japanese, "ui.need_resources", "Need more resources", "資源が足りません");
            Add(english, japanese, "ui.need_wood_spearman", "Need more Wood (Spearman)", "木材が足りません（槍兵）");
            Add(english, japanese, "ui.need_food_spearman", "Need more Food (Spearman)", "食料が足りません（槍兵）");
            Add(english, japanese, "ui.training", "Training... {0:0.0}s", "訓練中... {0:0.0}秒");
            Add(english, japanese, "ui.researching", "Researching... {0:0.0}s", "研究中... {0:0.0}秒");
            Add(english, japanese, "ui.tech_complete", "{0}: Complete", "{0}: 完了");

            Add(english, japanese, "ui.create_unit_food", "Create {0} (Q) ({1} Food)", "{0}を作る (Q)（{1} 食料）");
            Add(english, japanese, "ui.create_unit_dual", "Create {0} (Q) ({1} Wood, {2} Food)",
                "{0}を作る (Q)（{1} 木材, {2} 食料）");
            Add(english, japanese, "ui.create_unit_dual_e", "Create {0} (E) ({1} Wood, {2} Food)",
                "{0}を作る (E)（{1} 木材, {2} 食料）");
            Add(english, japanese, "ui.create_unit_food_e", "Create {0} (E) ({1} Food)", "{0}を作る (E)（{1} 食料）");

            Add(english, japanese, "ui.cancel_queue", "Cancel #{0}: {1}", "取消 #{0}: {1}");

            Add(english, japanese, "ui.hp", "HP: {0} / {1}", "HP: {0} / {1}");
            Add(english, japanese, "ui.attack", "Attack: {0} ({1})", "攻撃: {0} ({1})");
            Add(english, japanese, "ui.melee_armor", "Melee Armor: {0}", "近接装甲: {0}");
            Add(english, japanese, "ui.pierce_armor", "Pierce Armor: {0}", "貫通装甲: {0}");
            Add(english, japanese, "ui.stance", "Stance: {0}", "スタンス: {0}");
            Add(english, japanese, "ui.rally_set", "Rally: Set", "集合地点: 設定済");
            Add(english, japanese, "ui.rally_none", "Rally: None", "集合地点: なし");
            Add(english, japanese, "ui.owner_neutral", "Owner: Neutral", "所有者: 中立");
            Add(english, japanese, "ui.owner_team", "Owner: {0}", "所有者: {0}");

            Add(english, japanese, "ui.idle_villagers", "Idle Villagers: {0}", "待機村民: {0}");
            Add(english, japanese, "ui.idle_military", "Idle Military: {0}", "待機軍: {0}");
            Add(english, japanese, "ui.next_idle_villager", "Next Idle Villager (.)", "次の待機村民 (.)");

            Add(english, japanese, "ui.research_button", "{0} (Q) ({1} Food, {2} Gold)", "{0} (Q)（{1} 食料, {2} 金）");

            Add(english, japanese, "trade.sell_food", "Sell Food ({0} → {1} Gold)", "食料を売る ({0} → {1} 金)");
            Add(english, japanese, "trade.buy_food", "Buy Food ({0} Gold → {1})", "食料を買う ({0} 金 → {1})");
            Add(english, japanese, "trade.sell_wood", "Sell Wood ({0} → {1} Gold)", "木材を売る ({0} → {1} 金)");
            Add(english, japanese, "trade.buy_wood", "Buy Wood ({0} Gold → {1})", "木材を買う ({0} 金 → {1})");
            Add(english, japanese, "trade.sell_stone", "Sell Stone ({0} → {1} Gold)", "石を売る ({0} → {1} 金)");
            Add(english, japanese, "trade.buy_stone", "Buy Stone ({0} Gold → {1})", "石を買う ({0} 金 → {1})");
        }

        static void Add(
            Dictionary<string, string> english,
            Dictionary<string, string> japanese,
            string key,
            string en,
            string ja)
        {
            english[key] = en;
            japanese[key] = ja;
        }

        public static string BuildingKey(PlacedBuildingKind kind)
        {
            switch (kind)
            {
                case PlacedBuildingKind.House: return "building.house";
                case PlacedBuildingKind.Barracks: return "building.barracks";
                case PlacedBuildingKind.Farm: return "building.farm";
                case PlacedBuildingKind.LumberCamp: return "building.lumber_camp";
                case PlacedBuildingKind.MiningCamp: return "building.mining_camp";
                case PlacedBuildingKind.Mill: return "building.mill";
                case PlacedBuildingKind.ArcheryRange: return "building.archery_range";
                case PlacedBuildingKind.Stable: return "building.stable";
                case PlacedBuildingKind.Blacksmith: return "building.blacksmith";
                case PlacedBuildingKind.PalisadeWall: return "building.palisade";
                case PlacedBuildingKind.StoneWall: return "building.stone_wall";
                case PlacedBuildingKind.Gate: return "building.gate";
                case PlacedBuildingKind.WatchTower: return "building.watch_tower";
                case PlacedBuildingKind.Market: return "building.market";
                case PlacedBuildingKind.TownCenter: return "building.town_center";
                default: return "building.generic";
            }
        }

        public static string UnitKeyFromDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return "unit.generic";

            switch (displayName)
            {
                case "Villager": return "unit.villager";
                case "Militia": return "unit.militia";
                case "Man-at-Arms": return "unit.man_at_arms";
                case "Spearman": return "unit.spearman";
                case "Archer": return "unit.archer";
                case "Cavalry": return "unit.cavalry";
                case "Scout": return "unit.scout";
                default: return null;
            }
        }
    }
}

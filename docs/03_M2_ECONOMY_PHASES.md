# AoE RTS Engine — Milestone 2: Economy Phase ロードマップ（Phase 17〜20）

> **Milestone:** M2 — Economy Expansion  
> **前提:** [02_M1_FOUNDATION_PHASES.md](02_M1_FOUNDATION_PHASES.md)（Phase 11〜16）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **前:** [01_M0_POC_PHASES.md](01_M0_POC_PHASES.md) / [02_M1_FOUNDATION_PHASES.md](02_M1_FOUNDATION_PHASES.md)

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 17 | Food | Berry Bush 採集 + Villager Food コスト | ✅ 実装済み |
| 18 | Farm | 建築 + 継続 Food 生産 | ✅ 実装済み |
| 19 | Lumber Camp | 木採集効率・Drop-off | ✅ 実装済み |
| 20 | Gold + Stone | 4 資源完成 | ✅ 実装済み |

**M2 完了条件:** Wood / Food / Gold / Stone の 4 資源がゲーム内で機能する。 ✅ **達成**

---

## Phase 17 — Food ✅

**目的:** Berry Bush から Food を採集し TownCenter に搬入。Villager 生産に Food コスト。

**実装:** `FoodNodeData` / `BerryBushResource` / `FoodGatherManager` / `GatherFoodCommand`

**プロンプト:** [prompts/phase17-prompt.md](prompts/phase17-prompt.md)

**セットアップ:** `AoE → Setup Phase10 Scene`

---

## Phase 18 — Farm ✅

**目的:** Wood で Farm を建築し、村民が継続的に Food を採集。枯渇時は Pool 返却。

**実装:** `Farm` / `FarmData` / `FoodGatherManager`（Farm ジョブ）/ `GatherFarmFoodCommand` / HUD Build Farm

**プロンプト:** [prompts/phase18-prompt.md](prompts/phase18-prompt.md)

---

## Phase 19 — Lumber Camp ✅

**目的:** Lumber Camp を Wood の Drop-off 拠点にし、森近くへの搬入で採集効率を改善。

**実装:** `LumberCamp` / `LumberCampData` / `LumberCampRegistry` / `GatherManager` Deposit 先拡張 / HUD Build Lumber Camp

**プロンプト:** [prompts/phase19-prompt.md](prompts/phase19-prompt.md)

---

## Phase 20 — Gold + Stone ✅

**目的:** Gold Mine / Stone Mine から採掘し TC に搬入。**M2 Economy 完了。**

**実装:** `MineralNodeData` / `GoldMineResource` / `StoneMineResource` / `MineralGatherManager` / `GatherGoldCommand` / `GatherStoneCommand`

**プロンプト:** [prompts/phase20-prompt.md](prompts/phase20-prompt.md)

---

## 進め方

1. [CONSTITUTION.md](../CONSTITUTION.md)
2. [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)
3. 該当 [prompts/phaseN-prompt.md](prompts/)
4. small diff → `Phase10.unity` Play 確認

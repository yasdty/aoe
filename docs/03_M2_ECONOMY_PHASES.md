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
| 18 | Farm | 建築 + 継続 Food 生産 | ⬜ 未着手 |
| 19 | Lumber Camp | 木採集効率・Drop-off | ⬜ 未着手 |
| 20 | Gold + Stone | 4 資源完成 | ⬜ 未着手 |

**M2 完了条件:** Wood / Food / Gold / Stone の 4 資源がゲーム内で機能する。

---

## Phase 17 — Food ✅

**目的:** Berry Bush から Food を採集し TownCenter に搬入。Villager 生産に Food コスト。

**実装:** `FoodNodeData` / `BerryBushResource` / `FoodGatherManager` / `GatherFoodCommand`

**プロンプト:** [prompts/phase17-prompt.md](prompts/phase17-prompt.md)

**セットアップ:** `AoE → Setup Phase10 Scene`

---

## Phase 18〜20（未着手）

| Phase | 概要 |
|-------|------|
| 18 Farm | 建築 + Villager による継続 Food 生産 |
| 19 Lumber Camp | 木の Drop-off 拠点 |
| 20 Gold + Stone | 採掘ノード + 搬入 |

---

## 進め方

1. [CONSTITUTION.md](../CONSTITUTION.md)
2. [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)
3. 該当 [prompts/phaseN-prompt.md](prompts/)
4. small diff → `Phase10.unity` Play 確認

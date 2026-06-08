# AoE RTS Engine — Milestone 3: Military Phase ロードマップ（Phase 29〜34）

> **Milestone:** M3 — Military Expansion  
> **前提:** [04_M2_5_ECONOMY_POLISH_PHASES.md](04_M2_5_ECONOMY_POLISH_PHASES.md)（Phase 21〜28）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **前:** M2.5 Economy Polish / **次:** [06_M4_GAMEPLAY_PHASES.md](06_M4_GAMEPLAY_PHASES.md)（Phase 35〜）

> **番号変更:** 旧計画の Phase 21〜26（Military）を **Phase 29〜34** に繰り下げ。M2.5（Phase 21〜28）を M3 の前に挿入。

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 29 | Archer | 遠距離攻撃・弾丸（または即時ヒット） | ⬜ 未着手 |
| 30 | Spearman | 槍兵・対騎兵ボーナス下地 | ⬜ 未着手 |
| 31 | Cavalry | 騎兵・高機動近接 | ⬜ 未着手 |
| 32 | Counter System | 装甲・ボーナスダメージ相性 | ⬜ 未着手 |
| 33 | Stance & Aggro | Stand Ground / Defensive / 完全 Aggro | ⬜ 未着手 |
| 34 | Formation | 隊列移動・軽量 Separation | ⬜ 未着手 |

**M3 完了条件:** 歩兵 3 種 + 騎兵 + 相性表 + スタンス + グループ隊列が `Phase10.unity` で動作。

**Phase 27 との関係:** M2.5 で **簡易 Militia Aggro** を先行実装。Phase 33 でスタンス UI と弓兵 Aggro を統合。

---

## Phase 29 — Archer ⬜

**目的:** Barracks から Archer 生産。遠距離攻撃。

**実装:** `Archer` UnitData / 射程判定 / `AttackManager` 遠距離分岐 / 弾丸 MVP

**プロンプト:** [prompts/phase29-prompt.md](prompts/phase29-prompt.md)（未作成）

---

## Phase 30 — Spearman ⬜

**目的:** 槍兵。対 Cavalry ボーナス（Phase 32 で数値化可）。

**プロンプト:** [prompts/phase30-prompt.md](prompts/phase30-prompt.md)（未作成）

---

## Phase 31 — Cavalry ⬜

**目的:** 騎兵。高 HP / 高攻撃 / 高移動速度。

**プロンプト:** [prompts/phase31-prompt.md](prompts/phase31-prompt.md)（未作成）

---

## Phase 32 — Counter System ⬜

**目的:** ユニットタイプ相性（Spear vs Cavalry 等）を Data 駆動。

**プロンプト:** [prompts/phase32-prompt.md](prompts/phase32-prompt.md)（未作成）

---

## Phase 33 — Stance & Aggro ⬜

**目的:** Aggressive / Defensive / Stand Ground。Phase 27 簡易 Aggro を拡張。

**プロンプト:** [prompts/phase33-prompt.md](prompts/phase33-prompt.md)（未作成）

---

## Phase 34 — Formation ⬜

**目的:** グループ移動の隊列維持・軽量 Separation（憲法: NavMesh / 本格 RVO 禁止）。

**プロンプト:** [prompts/phase34-prompt.md](prompts/phase34-prompt.md)（未作成）

---

## 進め方

1. M2.5 全 Phase 完了を確認
2. [CONSTITUTION.md](../CONSTITUTION.md) + [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)
3. Phase 29 から順に small diff → `Phase10.unity` Play 確認

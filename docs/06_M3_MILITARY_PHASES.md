# AoE RTS Engine — Milestone 3: Military Phase ロードマップ（Phase 33〜38）

> **Milestone:** M3 — Military Expansion  
> **前提:** [05_M2_6_RTS_UX_PHASES.md](05_M2_6_RTS_UX_PHASES.md)（Phase 29〜32）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **前:** M2.6 RTS UX / **次:** [07_M4_GAMEPLAY_PHASES.md](07_M4_GAMEPLAY_PHASES.md)（Phase 39〜）

> **番号変更:** M2.6（Phase 29〜32）挿入に伴い、旧 Phase 29〜34（Military）を **Phase 33〜38** に繰り下げ。

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 33 | Archer | 遠距離攻撃・弾丸（または即時ヒット） | ⬜ 未着手 |
| 34 | Spearman | 槍兵・対騎兵ボーナス下地 | ⬜ 未着手 |
| 35 | Cavalry | 騎兵・高機動近接 | ⬜ 未着手 |
| 36 | Counter System | 装甲・ボーナスダメージ相性 | ⬜ 未着手 |
| 37 | Stance & Aggro | Stand Ground / Defensive / 完全 Aggro | ⬜ 未着手 |
| 38 | Formation | 隊列移動・軽量 Separation | ⬜ 未着手 |

**M3 完了条件:** 歩兵 3 種 + 騎兵 + 相性表 + スタンス + グループ隊列が `Phase10.unity` で動作。

**Phase 27 との関係:** M2.5 で **簡易 Militia Aggro** を先行実装。Phase 37 でスタンス UI と弓兵 Aggro を統合。

---

## Phase 33 — Archer ⬜

**目的:** Barracks から Archer 生産。遠距離攻撃。

**実装:** `Archer` UnitData / 射程判定 / `AttackManager` 遠距離分岐 / 弾丸 MVP

**プロンプト:** [prompts/phase33-prompt.md](prompts/phase33-prompt.md)（未作成）

---

## Phase 34 — Spearman ⬜

**プロンプト:** [prompts/phase34-prompt.md](prompts/phase34-prompt.md)（未作成）

---

## Phase 35 — Cavalry ⬜

**プロンプト:** [prompts/phase35-prompt.md](prompts/phase35-prompt.md)（未作成）

---

## Phase 36 — Counter System ⬜

**プロンプト:** [prompts/phase36-prompt.md](prompts/phase36-prompt.md)（未作成）

---

## Phase 37 — Stance & Aggro ⬜

**プロンプト:** [prompts/phase37-prompt.md](prompts/phase37-prompt.md)（未作成）

---

## Phase 38 — Formation ⬜

**プロンプト:** [prompts/phase38-prompt.md](prompts/phase38-prompt.md)（未作成）

---

## 進め方

1. M2.6 全 Phase 完了を確認
2. [CONSTITUTION.md](../CONSTITUTION.md) + [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)
3. Phase 33 から順に small diff → `Phase10.unity` Play 確認

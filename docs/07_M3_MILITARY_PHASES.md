# AoE RTS Engine — Milestone 3: Military Phase ロードマップ（Phase 36〜41）

> **Milestone:** M3 — Military Expansion（AoE2 準拠の軍事建築分離）  
> **前提:** [06_M2_7_SANDBOX_PHASES.md](06_M2_7_SANDBOX_PHASES.md)（Phase 35）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **前:** M2.7 Sandbox / **次:** [08_M4_GAMEPLAY_PHASES.md](08_M4_GAMEPLAY_PHASES.md)（Phase 42〜48）

> **2026-06 修正:** 旧「Barracks から Archer」は AoE2 非準拠のため廃止。**Archery Range / Stable** を Phase 36 / 38 で導入。Phase 番号は M2.7 挿入に伴い **36〜41** へ繰り下げ。

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 36 | Archery Range + Archer | 弓兵建築・遠距離攻撃・弾丸 MVP | ⬜ 未着手 |
| 37 | Spearman | Barracks 歩兵ライン拡張・対騎兵下地 | ⬜ 未着手 |
| 38 | Stable + Cavalry | 騎兵建築・Scout 下地・高機動近接 | ⬜ 未着手 |
| 39 | Counter System | Melee / Pierce 装甲・ボーナスダメージ相性 | ⬜ 未着手 |
| 40 | Stance, Aggro & Attack-Move | Stand Ground / Defensive / 攻撃移動 MVP | ⬜ 未着手 |
| 41 | Formation | 隊列移動・軽量 Separation + CPU 軍事 AI 新兵種対応 | ⬜ 未着手 |

**M3 完了条件:**

- **Barracks** = 歩兵のみ（Militia / Spearman）
- **Archery Range** = Archer
- **Stable** = Cavalry（Scout はデータ下地のみでも可）
- 相性表 + スタンス + 攻撃移動 + 隊列が `Phase10.unity` で動作
- `CpuMilitaryAiManager` が **Archer / Spearman / Cavalry を生産・編成**できる（単純ルールで可）

**既存 Phase との関係:**

| 既存 | M3 での扱い |
|------|-------------|
| Phase 29 Militia Aggro | Phase 40 でスタンス UI と弓兵 Aggro を統合 |
| Phase 25 Info Panel | Phase 39 で Melee / Pierce 2 種装甲表示に拡張 |
| Phase 33 Rally | 新兵種建築にも Rally 適用（Phase 36 / 38 で配線） |

---

## AoE2 建築対応（本マイルストンの設計方針）

| 建築 | 生産ユニット | 導入 Phase |
|------|--------------|------------|
| Barracks | Militia, Spearman | 既存 / 37 |
| Archery Range | Archer | **36** |
| Stable | Scout, Cavalry | **38** |

Castle / Siege Workshop / Dock は [11_DEFERRED_EXTENSION_DESIGN.md](11_DEFERRED_EXTENSION_DESIGN.md) の拡張ポイントとして **データ駆動で後挿入可能**に設計する（`BuildingData` + `ProductionRecipe` パターン維持）。

---

## Phase 36 — Archery Range + Archer ⬜

**実装:** `ArcheryRange` 建築 / `Archer` UnitData / 射程判定 / `AttackManager` 遠距離分岐 / 弾丸 MVP（即時ヒット fallback 可）

**CPU:** Barracks 後に Archery Range 建設・Archer 生産を追加

**プロンプト:** [prompts/phase36-prompt.md](prompts/phase36-prompt.md)（未作成）

---

## Phase 37 — Spearman ⬜

**実装:** Barracks から Spearman 生産。対騎兵ボーナスは Phase 39 で本格化、ここでは UnitData 下地。

**プロンプト:** [prompts/phase37-prompt.md](prompts/phase37-prompt.md)（未作成）

---

## Phase 38 — Stable + Cavalry ⬜

**実装:** Stable 建築 / Cavalry UnitData / 高移動速度近接。Scout は `UnitData` のみ先行でも可。

**プロンプト:** [prompts/phase38-prompt.md](prompts/phase38-prompt.md)（未作成）

---

## Phase 39 — Counter System ⬜

**実装:** `ArmorClass` / ボーナスダメージ表。Info Panel を Melee / Pierce 表示に拡張。

**プロンプト:** [prompts/phase39-prompt.md](prompts/phase39-prompt.md)（未作成）

---

## Phase 40 — Stance, Aggro & Attack-Move ⬜

**実装:**

- Stand Ground / Aggressive / Defensive（OnGUI MVP → M5 で uGUI 移行）
- 弓兵の射程内 Aggro 統合
- **攻撃移動（A + 右クリック）** MVP — マイクロの前提

**プロンプト:** [prompts/phase40-prompt.md](prompts/phase40-prompt.md)（未作成）

---

## Phase 41 — Formation ⬜

**実装:** 隊列移動・軽量 Separation。`CpuMilitaryAiManager` 攻撃波に新兵種を混在。

**プロンプト:** [prompts/phase41-prompt.md](prompts/phase41-prompt.md)（未作成）

---

## M3 で意図的に後回し（拡張設計で吸収）

| 項目 | 先送り先 |
|------|----------|
| 攻城兵器（Ram / Trebuchet） | [11_DEFERRED_EXTENSION_DESIGN.md](11_DEFERRED_EXTENSION_DESIGN.md) |
| Monk / 変換 | 同上 |
| 海軍 / Dock | 同上 |
| Animator 本格 | [09_M5_VISUAL_UI_PHASES.md](09_M5_VISUAL_UI_PHASES.md) |
| Castle ユニット生産 | M4 Age Up 後の拡張フック |

---

## 進め方

1. M2.7 完了を確認
2. [CONSTITUTION.md](../CONSTITUTION.md) + [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)
3. Phase 36 から順に small diff → `Phase10.unity` Play 確認

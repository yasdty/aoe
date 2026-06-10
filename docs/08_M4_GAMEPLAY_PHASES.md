# AoE RTS Engine — Milestone 4: AoE Gameplay Phase ロードマップ（Phase 42〜48）

> **Milestone:** M4 — AoE2 Core Gameplay  
> **前提:** [07_M3_MILITARY_PHASES.md](07_M3_MILITARY_PHASES.md)（Phase 36〜41）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **前:** M3 Military / **次:** [09_M5_VISUAL_UI_PHASES.md](09_M5_VISUAL_UI_PHASES.md)（Phase 49〜53）

> **2026-06 拡張:** 旧 5 Phase（41〜45）に **Second TC** と **RTS UX Polish** を追加し **42〜48** へ再編。Castle / Fog / ランダムマップは意図的後回し（拡張設計は [11_DEFERRED_EXTENSION_DESIGN.md](11_DEFERRED_EXTENSION_DESIGN.md)）。

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 42 | Age Up + **Gameplay Balance** | **Phase 42 先頭:** Balance Mode 実装 → 時代昇格（Dark → Feudal MVP）・建築アンロック | ⬜ 未着手 |
| 43 | Blacksmith & Tech | 鍛冶屋 + 歩兵 UP 1 系統（例: Militia → MAA） | ⬜ 未着手 |
| 44 | Defense | 柵 / 石壁 / 箭塔 MVP | ⬜ 未着手 |
| 45 | Market | 資源交易 MVP | ⬜ 未着手 |
| 46 | Civilization | 文明ボーナス 1 種 Data 駆動 | ⬜ 未着手 |
| 47 | Second TC | 2 台目 Town Center（ブーム下地） | ⬜ 未着手 |
| 48 | RTS UX Polish | キュー取消・House Pop 減・建築ホットキー・Shift+5 キュー | ⬜ 未着手 |

**M4 完了条件:**

- Feudal 時代に昇格し、Archery Range / Stable / Blacksmith 等が **時代データでアンロック**
- 最低 1 系統の軍事 UP が研究可能
- 壁・塔で拠点防衛の MVP
- 市場で資源変換
- 1 文明の経済 or 軍事ボーナスが Data で差し替え可能
- 2 台目 TC 建設で村民ブームの下地
- AoE2 操作で欠けていた UX ギャップ（キュー取消等）を解消

---

## Phase 42 — Gameplay Balance + Age Up ⬜

**実装順（確定）:**

1. **Gameplay Balance Mode**（[12_GAMEPLAY_BALANCE_MODE.md](12_GAMEPLAY_BALANCE_MODE.md) §6）— Phase 42 **先頭**
   - `GameplayBalance` — buildTime ×0.1 / cost ×0.3（全資源）
   - `GameSessionManager` + Phase10 既定 Debug / Inspector + `AoE` メニュー
   - CPU AI 遅延も Debug 時 ×0.1
   - M3 完了時に移行済みの AoE2 基準 Data を Balance 層経由で読む
2. **Age Up**
   - `AgeData` ScriptableObject — Dark / Feudal（MVP は 2 時代）
   - TC で時代昇格（資源コスト + 建築要件）
   - `BuildingData.requiredAge` で建築アンロック

**拡張フック:** Castle / Imperial は `AgeData` 追加のみで後挿入（コード rewrite 不要）

**プロンプト:** [prompts/phase42-prompt.md](prompts/phase42-prompt.md)

---

## Phase 43 — Blacksmith & Tech ⬜

**実装:** Blacksmith 建築 / `TechnologyData` / 研究キュー（TC 生産キューと同型パターン推奨）

**プロンプト:** [prompts/phase43-prompt.md](prompts/phase43-prompt.md)（未作成）

---

## Phase 44 — Defense ⬜

**実装:** Palisade Wall / Stone Wall / Watch Tower — HP・攻撃・村民建築

**プロンプト:** [prompts/phase44-prompt.md](prompts/phase44-prompt.md)（未作成）

---

## Phase 45 — Market ⬜

**実装:** Market 建築 / 資源売買（固定レート MVP）

**プロンプト:** [prompts/phase45-prompt.md](prompts/phase45-prompt.md)（未作成）

---

## Phase 46 — Civilization ⬜

**実装:** `CivilizationData` — 経済 or 軍事ボーナス 1 種 + チーム適用

**拡張フック:** 固有ユニット・チームボーナスは `CivilizationData` フィールド追加で拡張

**プロンプト:** [prompts/phase46-prompt.md](prompts/phase46-prompt.md)（未作成）

---

## Phase 47 — Second TC ⬜

**実装:** Feudal 以降 2 台目 TC 建設 / 人口・生産のスノーボール下地

**プロンプト:** [prompts/phase47-prompt.md](prompts/phase47-prompt.md)（未作成）

---

## Phase 48 — RTS UX Polish ⬜

**実装（M2.6 後回し分の回収）:**

| 項目 | 内容 |
|------|------|
| 生産キュー取消 | クリックでキャンセル + 返金 |
| Shift+5 一括キュー | TC / Barracks |
| House 破壊 | Pop cap 減少 |
| 建築ホットキー | House / Barracks 等（Input System 拡張） |

**プロンプト:** [prompts/phase48-prompt.md](prompts/phase48-prompt.md)（未作成）

---

## M4 で意図的に後回し

| 項目 | 拡張先 |
|------|--------|
| Castle / Wonder / 複数勝利条件 | [11_DEFERRED_EXTENSION_DESIGN.md](11_DEFERRED_EXTENSION_DESIGN.md) |
| Fog of War / 探索 | 同上（`VisionManager` フック定義のみ M4 で可） |
| ランダムマップ | M6 以降 / P3 |
| 大学・全テックツリー | `TechnologyData` 追加で段階拡張 |
| 漁船 / Dock | 同上 |
| AI 難易度 Extreme | M6 以降 |

---

## 進め方

1. M3 全 Phase 完了を確認
2. [CONSTITUTION.md](../CONSTITUTION.md) + [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)
3. Phase 42 から順に small diff

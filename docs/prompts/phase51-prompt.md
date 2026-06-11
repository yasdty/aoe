# Phase 51 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜50 完了（M5 Wall Age Grades — Dark Palisade / Feudal Stone+Gate）  
> **マイルストン:** M5 — **Localization (i18n)**  
> **ロードマップ:** [09_M5_VISUAL_UI_PHASES.md](../09_M5_VISUAL_UI_PHASES.md)  
> **関連:** Phase 52〜53（uGUI HUD Migration）は **LanguageMap キー経由**で文言を参照する前提。本 Phase で基盤 + OnGUI 主要パネル移行を完了する。  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 51 実装（Localization / i18n）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 51 のみ実装。** 英語ハードコードを **LanguageMap** に集約し EN / JA 切替（rewrite 禁止）。

**前提:** HUD は OnGUI MVP（`ResourceHudView` 等）。`PlacedBuildingData.displayName` は Data 側の英語名が残っていてもよいが、**プレイヤー向け表示は LanguageMap を優先**。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| API | `Localization.Get(key)` — 静的レジストリ or ScriptableObject ラッパ |
| 言語 | **English（既定）** / **Japanese** |
| 切替 | Debug HUD ボタン or キー（例: `L`）— PlayerPrefs 保存 MVP 可 |
| 日本語ソース | [Age of Empires Series Wiki（日本語）](https://ageofempires.fandom.com/ja/) のユニット・建築・資源名 |
| 対象 HUD | 資源バー、建築ボタン、生産パネル、選択 Info、勝敗、Idle 表示 |
| フォーマット | コスト・数値は `{0}` プレースホルダ — `Localization.Format(key, args)` |
| Editor | `AoE → Sync AoE2 Game Data` 相当で LanguageMap アセット生成 or コード内テーブル |

**やらないこと:** uGUI 本格 HUD（Phase 52〜53）/ 音声ローカライズ / フォント本格対応 / 全 Debug 文字列 / CPU 内部ログ

**キー例:**

| キー | EN | JA（AoE2 Wiki 準拠） |
|------|-----|---------------------|
| `resource.wood` | Wood | 木材 |
| `unit.villager` | Villager | 村民 |
| `building.house` | House | 家 |
| `building.palisade` | Palisade | フェンス |
| `building.stone_wall` | Stone Wall | 石の城壁 |
| `building.gate` | Gate | 門 |
| `age.dark` | Dark Age | 暗黒時代 |
| `age.feudal` | Feudal Age | 城主時代 |
| `ui.victory` | VICTORY | 勝利 |
| `ui.defeat` | DEFEAT | 敗北 |
| `ui.build_house` | Build House (H) ({0} {1}) | 家を建てる (H)（{0} {1}） |

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| 資源 HUD | `ResourceHudView` — Wood/Food/建築ボタン |
| 生産 | `ProductionPanelView`, `BarracksPanelView`, `ArcheryRangePanelView`, `StablePanelView` |
| 研究 / 交易 | `BlacksmithPanelView`, `MarketPanelView` |
| 選択 | `SelectionInfoPanelView` — ユニット/建築/資源ノード名 |
| 勝敗 | `VictoryDefeatHudView` |
| Idle | `IdleUnitHudView` |
| Data 名 | `PlacedBuildingData.displayName`, `UnitData.displayName` — キーへのマッピング方針を決める |
| Debug | `GameTimeHudView` — **切替 UI をここ or 専用 `LanguageHudView` に追加** |

---

## ③ 実装タスク

### 1. Localization 基盤

- `GameLanguage` enum（`English`, `Japanese`）
- `Localization` static class:
  - `CurrentLanguage` get/set（変更時 PlayerPrefs 保存 MVP）
  - `Get(string key)` — 欠落キーは key をそのまま返す（開発時に気づける）
  - `Format(string key, params object[] args)` — `string.Format` ラッパ
- `LanguageMap` — Dictionary または ScriptableObject に EN / JA エントリ
  - 最低 **80 キー**（資源 4 + 主要建築 15 + 主要ユニット 10 + 共通 UI 20 + エラー/ヒント 30）
- Editor: `Phase1SceneBuilder.EnsureLanguageMap()` or コード内 bootstrap — Phase10 配線

### 2. HUD 移行（OnGUI — 主要パネルのみ）

優先順:

1. **`ResourceHudView`** — 資源ラベル + 全 Build ボタン + placement ヒント
2. **`ProductionPanelView`** — TC 生産 / Age Up / 時代名
3. **`VictoryDefeatHudView`** — 勝敗タイトル + Restart 案内
4. **`SelectionInfoPanelView`** — 選択名・HP・攻撃（`unit.*` / `building.*` キー）
5. **`IdleUnitHudView`** — Idle カウント
6. **兵舎系パネル** — `BarracksPanelView`, `ArcheryRangePanelView`, `StablePanelView`（Create ボタン）
7. **`BlacksmithPanelView`**, **`MarketPanelView`** — タイトル + 主要ボタン

**パターン:** ハードコード `"Build House (...)"` → `Localization.Format("ui.build_house", cost, Localization.Get("resource.wood"))`

**時代ロック:** `"Stone Wall (Feudal Age)"` → `Localization.Format("ui.requires_age", Localization.Get("building.stone_wall"), Localization.Get("age.feudal"))`

### 3. Data 表示名（任意・推奨）

- `PlacedBuildingKind` / `UnitKind` → localization key のヘルパ（例: `Localization.BuildingName(kind)`）
- `displayName` フィールドは Editor 用に残してよい

### 4. 言語切替 UX

- `GameTimeHudView` または軽量 `LanguageToggleHudView` に **Language: EN / JA** トグル
- キーボード `L` で EN↔JA（New Input System — 既存 Debug 入力パターンに合わせる）
- 切替後、開いている OnGUI が即座に反映されること

### 5. Phase10 / ドキュメント

- Play: 既定 EN → `L` で JA → 建築ボタン・TC パネル・勝敗が日本語
- Phase 49/50 回帰: 壁ドラッグ・Gate・時代別壁 — gameplay 無変更
- Console エラーなし

---

## ④ 制約

- small diff only / rewrite 禁止 — View ごとに段階的に `Localization.Get` へ置換
- Simulation / Manager 層に UI 文字列を増やさない（View のみ）
- NavMesh 禁止
- Phase 52 View Split のため、`Localization` は **Simulation から参照可能な Core 層**（`Assets/Scripts/Core/`）に置く

---

## ⑤ Play 確認

1. `Phase10.unity` — Debug + CPU Relaxed
2. **English（既定）** — 既存と同等の英語表示
3. **`L` キー or HUD トグル** — 資源・建築ボタンが日本語（村民・家・フェンス・石の城壁・門 等）
4. **Feudal 昇格後** — Stone Wall / Gate ボタンも JA 表示
5. **勝敗** — VICTORY/DEFEAT → 勝利/敗北
6. Phase 49/50 回帰 — 壁列・遮断・Gate
7. Console エラーなし

---

## ⑥ 完了時ドキュメント

- [x] `IMPLEMENTATION_STATUS.md` — Phase 51 ✅ / LanguageMap ✅
- [x] `09_M5_VISUAL_UI_PHASES.md` — Phase 51 ✅
- [x] 本プロンプト — ✅

---

Phase 51 のみ。**Phase 52（View Split）・Phase 53（HUD Migration）には触れない** — ただし LanguageMap API は Phase 52〜53 がそのまま使える設計にすること。

> **次:** [phase52-prompt.md](phase52-prompt.md) — View Layer Split

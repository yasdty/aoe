# AoE RTS Engine — Milestone 5: Gameplay Polish & Visual / UI ロードマップ（Phase 49〜56）

> **Milestone:** M5 — **防衛 gameplay 完成** + Visual / UI Polish + **Localization**  
> **前提:** [08_M4_GAMEPLAY_PHASES.md](08_M4_GAMEPLAY_PHASES.md)（Phase 42〜48）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **前:** M4 AoE Gameplay / **次:** [10_M6_MULTIPLAYER_FOUNDATION.md](10_M6_MULTIPLAYER_FOUNDATION.md)（Phase 57〜61）

> **2026-06 拡張:** Phase 44/48 の壁は **配置・HP のみ MVP**（通行遮断・Gate・AoE2 型ドラッグ未達）。本 M5 先頭で **本物の壁システム** と **i18n** を入れてから uGUI 移行する。

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 49 | **Wall & Gate System** | 通行遮断・ドラッグ連続配置・セグメント接続・**Gate（自軍通過）** | ⬜ 未着手 |
| 50 | **Wall Age Grades** | 時代に応じた柵/壁グレード（AoE2: 初期柵 → 昇格後の石壁等） | ⬜ 未着手 |
| 51 | **Localization (i18n)** | LanguageMap + **日本語表示**（AoE2 Wiki 用語）/ EN↔JA 切替 | ⬜ 未着手 |
| 52 | View Layer Split | Simulation / View 分離 + uGUI Canvas シェル | ⬜ 未着手 |
| 53 | HUD Migration | 資源・生産・選択パネルを uGUI へ（**i18n キー経由**） | ⬜ 未着手 |
| 54 | Minimap | 俯瞰ミニマップ + TC / 主要建築アイコン | ⬜ 未着手 |
| 55 | Unit Animation | Animator MVP（歩行・採集・攻撃） | ⬜ 未着手 |
| 56 | Combat VFX & Audio | 弾丸・ヒット・SE フック | ⬜ 未着手 |

**M5 完了条件:**

- 壁が **ユニットの通行を物理的にブロック**（NavMesh なし方針で StaticObstacle / Grid 占有）
- **マウスドラッグ**で壁セグメント列を AoE2 同型配置（Shift+クリック連打ではない）
- **Gate** 配置 — 自チーム（＋将来同盟）のみ通過
- 時代昇格に伴う **壁グレード**（最低: Dark 柵 / Feudal 石壁の切替 or アンロック）
- HUD 主要文言が **LanguageMap 経由** — 日本語切替で AoE2 Wiki 準拠の表示名
- OnGUI 依存 HUD が **主要パネルから除去**
- 1280×720 以上でレイアウト崩れなし
- ミニマップで両 TC 位置把握
- Villager / Militia / Archer の状態アニメ（3 種以上）

---

## Phase 49 — Wall & Gate System ⬜

**Phase 44/48 との差分（必須）:**

| 項目 | Phase 44/48 MVP | Phase 49 目標 |
|------|-----------------|---------------|
| 通行 | すり抜け可能 | **遮断** |
| 配置 | 1 マス / Shift+クリック | **ドラッグ連続** + 隣接スナップ |
| Gate | なし | **自軍通過可** |
| 見た目 | 独立セグメント | 角・直線の接続（最低限） |

**参考（AoE2 Wiki 日本語）:**  палисade → **フェンス** / 石壁 → **石の城壁** / 城門 → **門**（Gate）

**プロンプト:** [prompts/phase49-prompt.md](prompts/phase49-prompt.md)

---

## Phase 50 — Wall Age Grades ⬜

**目的:** Age Up に伴い利用可能な壁種・HP・コストが変わる AoE2 仕様の再現。

| 時代（MVP） | 壁種（例） |
|-------------|-----------|
| Dark Age | フェンス（Palisade）— Wood |
| Feudal Age | 石の城壁（Stone Wall）— Stone + Wood |

**実装方針:** `PlacedBuildingData.requiredAge` + 配置 HUD の自動差し替え / 既存 Palisade の「グレードアップ」は Data 駆動（rewrite 禁止）

**プロンプト:** [prompts/phase50-prompt.md](prompts/phase50-prompt.md)（未作成）

---

## Phase 51 — Localization (i18n) ⬜

**目的:** 英語ハードコードをやめ、**LanguageMap** で EN / JA を切替。

| 項目 | 方針 |
|------|------|
| API | `Localization.Get(key)` or `LanguageMap` ScriptableObject / static registry |
| 切替 | 設定 or ランタイム（Debug メニュー MVP 可） |
| 日本語ソース | [Age of Empires Series Wiki（日本語）](https://ageofempires.fandom.com/ja/) のユニット・建築・資源名 |
| 対象（MVP） | HUD ラベル、生産パネル、Selection Info、勝敗、建築ボタン |
| 非対象 | 音声 / フォント本格対応（Nice to have） |

**キー例:** `unit.villager` → EN `Villager` / JA `村民`、`building.house` → `House` / `家`、`resource.wood` → `Wood` / `木材`

**プロンプト:** [prompts/phase51-prompt.md](prompts/phase51-prompt.md)（未作成）

---

## Phase 52 — View Layer Split ⬜

**目的:** Manager 内 OnGUI を段階的に剥がす。

- `IHudPresenter` / `ISelectionView` 等
- Simulation Manager は View を直接参照しない
- uGUI Canvas を Editor API 生成

**プロンプト:** [prompts/phase52-prompt.md](prompts/phase52-prompt.md)（未作成）

---

## Phase 53 — HUD Migration ⬜

**移行対象:** `ResourceHudView`, `ProductionPanelView`, `SelectionInfoPanelView`, `IdleUnitHudView`, `VictoryDefeatHudView` — **Phase 51 LanguageMap 必須**

**プロンプト:** [prompts/phase53-prompt.md](prompts/phase53-prompt.md)（未作成）

---

## Phase 54 — Minimap ⬜

**プロンプト:** [prompts/phase54-prompt.md](prompts/phase54-prompt.md)（未作成）

---

## Phase 55 — Unit Animation ⬜

**プロンプト:** [prompts/phase55-prompt.md](prompts/phase55-prompt.md)（未作成）

---

## Phase 56 — Combat VFX & Audio ⬜

**プロンプト:** [prompts/phase56-prompt.md](prompts/phase56-prompt.md)（未作成）

---

## M5 完了時の位置づけ

| 観点 | 見込み |
|------|--------|
| **AoE2 機能全体** | **約 50〜55%** |
| **コアループ（1v1 CPU）** | **約 90%**（壁・Gate 完成後） |
| **マルチプレイ準備度** | **約 55〜60%**（View 分離まで。同期基盤は M6） |
| **憲法性能目標（800 体）** | **約 25〜35%** |

---

## 進め方

1. M4 Phase 48 Play 確認完了
2. **Phase 49 Wall & Gate** — gameplay 優先（UI 移行前）
3. Phase 50 Wall Age Grades → Phase 51 i18n → Phase 52 以降 Visual
4. **1 Phase ごと small diff**

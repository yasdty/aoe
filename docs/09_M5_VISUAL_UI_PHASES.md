# AoE RTS Engine — Milestone 5: Gameplay Polish & Visual / UI ロードマップ（Phase 49〜56）

> **Milestone:** M5 — **防衛 gameplay 完成** + Visual / UI Polish + **Localization**  
> **前提:** [08_M4_GAMEPLAY_PHASES.md](08_M4_GAMEPLAY_PHASES.md)（Phase 42〜48）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **前:** M4 AoE Gameplay / **次:** [10_M6_MULTIPLAYER_FOUNDATION.md](10_M6_MULTIPLAYER_FOUNDATION.md)（Phase 57〜61）

> **2026-06 拡張:** Phase 44/48 の壁は配置・HP MVP。Phase 49 で **通行遮断・ドラッグ列配置・Gate** を実装。**ドラッグ中の列ゴーストプレビュー**は Phase 52〜53（View / HUD）で対応。

---

## Phase 49 から Phase 52〜53 へ繰り越し（UI）

| 項目 | Phase 49 | 繰越先 |
|------|----------|--------|
| ドラッグ確定で壁列を配置 | ✅ | — |
| 通行遮断・Gate | ✅ | — |
| **ドラッグ中の列ゴースト**（全セグメントを有効/無効色で表示） | ✅ Phase 52 | — |
| 角・接続の見た目 polish | △ MVP | Phase 55 以降（任意） |

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 49 | **Wall & Gate System** | 通行遮断・ドラッグ連続配置・セグメント接続・**Gate（自軍通過）** | ✅ 完了 |
| 50 | **Wall Age Grades** | 時代に応じた柵/壁グレード（AoE2: 初期柵 → 昇格後の石壁等） | ✅ 完了 |
| 51 | **Localization (i18n)** | LanguageMap + **日本語表示**（AoE2 Wiki 用語）/ EN↔JA 切替 | ✅ 完了 |
| 52 | View Layer Split | Simulation / View 分離 + uGUI Canvas シェル | ✅ 完了 |
| 53 | HUD Migration | 資源・生産・選択パネルを uGUI へ（**i18n キー経由**） | ✅ 完了 |
| 54 | Minimap | 俯瞰ミニマップ + TC / 主要建築アイコン | ✅ 完了 |
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

## Phase 49 — Wall & Gate System ✅

**Play 確認済（2026-06）:** ドラッグ列配置・通行遮断・Gate（自軍通過）OK。

**Phase 44/48 との差分:**

| 項目 | Phase 44/48 MVP | Phase 49 |
|------|-----------------|----------|
| 通行 | すり抜け可能 | **遮断** ✅ |
| 配置 | Shift+クリック | **ドラッグ確定** ✅ |
| Gate | なし | **自軍通過可** ✅ |
| ドラッグ中ゴースト | — | **未実装** → Phase 52〜53 |

**プロンプト:** [prompts/phase49-prompt.md](prompts/phase49-prompt.md)

---

## Phase 50 — Wall Age Grades ✅

**目的:** Age Up に伴い利用可能な壁種・HP・コストが変わる AoE2 仕様の再現。

| 時代（MVP） | 壁種（例） |
|-------------|-----------|
| Dark Age | フェンス（Palisade）— Wood |
| Feudal Age | 石の城壁（Stone Wall）— Stone + **Gate**（Wood） |

**HUD 方針（AoE2 近似）:** Feudal 昇格後も **Palisade を継続表示**し、Stone Wall / Gate を追加アンロック（`CanBuild` + gray ボタン）。

**実装:** `PlacedBuildingData.requiredAge` + `GameSessionManager.CanBuild` を配置 HUD / ゴースト / 確定の単一真実源に。Palisade→Stone グレードアップは未実装（任意・後回し）。

**プロンプト:** [prompts/phase50-prompt.md](prompts/phase50-prompt.md)

---

## Phase 51 — Localization (i18n) ✅

**目的:** 英語ハードコードをやめ、**LanguageMap** で EN / JA を切替。

| 項目 | 実装 |
|------|------|
| API | `Localization.Get` / `Format` / `BuildingName` / `UnitName` |
| 切替 | **`L` キー** + `GameTimeHudView` ボタン — PlayerPrefs 保存 |
| 対象 | ResourceHud / 生産パネル / Selection Info / 勝敗 / Idle / Market 交易 |
| キー数 | 80+（`LanguageMapBootstrap`） |

**プロンプト:** [prompts/phase51-prompt.md](prompts/phase51-prompt.md)

---

## Phase 52 — View Layer Split ✅

**目的:** Manager 内ゴースト描画を View 層へ分離し、uGUI Canvas シェルを追加。

| 項目 | 実装 |
|------|------|
| API | `IPlacementPreviewView` + `PlacementPreviewState` DTO |
| 実装 | `PlacementPreviewView` — セグメントプール |
| 壁列ゴースト | ドラッグ中 `ShowWallLinePreview`（有効/無効・資源打ち切り） |
| uGUI | `GameplayCanvas` + EventSystem + `HudRoot`（Phase 53 用） |
| Editor | `AoE → Add View Layer (Phase52)` |

**プロンプト:** [prompts/phase52-prompt.md](prompts/phase52-prompt.md)

---

## Phase 53 — HUD Migration ✅

**移行対象:** `ResourceHudView`, `ProductionPanelView`, `SelectionInfoPanelView`, `IdleUnitHudView`, `VictoryDefeatHudView` + 生産パネル群 — **LanguageMap 経由**

| 項目 | 実装 |
|------|------|
| 基盤 | `HudUiFactory`, `HudBottomLeftStack`, `ProductionQueueListView` |
| ヒット判定 | `GameUiInput` — RectTransform 登録 |
| OnGUI 残存 | `SelectionBoxView`, `UnitHpBarView`, `CpuHudView` |
| Editor | `AoE → Migrate HUD to uGUI (Phase53)` |

**プロンプト:** [prompts/phase53-prompt.md](prompts/phase53-prompt.md)

---

## Phase 54 — Minimap ✅

**目的:** 俯瞰ミニマップで両 TC 位置を把握。カメラ視野表示 + クリック移動 MVP。

| 項目 | 実装 |
|------|------|
| 表示 | `RawImage` 単色背景 + TC アイコン（Player 青 / Enemy 赤） |
| 位置 | 右上 uGUI（`CpuHudView` OnGUI の下 — `SetAnchoredTopRight`） |
| 座標 | `MapBounds` — Phase10 地面 AABB（Ground Transform から自動算出可） |
| 視野 | Main Camera 地面交差 4 点 → UV bounding rect |
| 操作 | クリック → `RTSCameraController.FocusOnGroundPoint` |
| Editor | `AoE → Add Minimap (Phase54)` / `EnsureMinimap()` |

**プロンプト:** [prompts/phase54-prompt.md](prompts/phase54-prompt.md)

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
3. Phase 53 HUD Migration ✅ → Phase 54 Minimap ✅ → Phase 55 以降 Visual
4. **1 Phase ごと small diff**

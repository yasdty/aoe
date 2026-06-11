# Phase 53 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜52 完了（View Split — `IPlacementPreviewView` / `GameplayCanvas` + `HudRoot` / Input System UI）  
> **マイルストン:** M5 — **HUD Migration（OnGUI → uGUI）**  
> **ロードマップ:** [09_M5_VISUAL_UI_PHASES.md](../09_M5_VISUAL_UI_PHASES.md)  
> **関連:** Phase 51 `Localization` API 必須 / Phase 52 `GameplayCanvas`・`HudRoot` / Phase 54 Minimap  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 53 実装（HUD Migration）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 53 のみ実装。** 主要 HUD を **OnGUI から uGUI** へ移行（rewrite 禁止）。

**前提:** Phase 52 で `GameplayCanvas`（1280×720 scaler）+ `EventSystem`（`InputSystemUIInputModule`）+ 空の `HudRoot` あり。配置ゴーストは `PlacementPreviewView` が担当済み — **本 Phase では触らない**。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| 移行方式 | 既存 `*HudView` / `*PanelView` クラスを **Presenter として維持** — `OnGUI` を削除し uGUI を更新 |
| Canvas | Phase 52 の `GameplayCanvas` → `HudRoot` 配下に UI 階層を構築 |
| 文言 | すべて **`Localization.Get` / `Format`** — ハードコード禁止 |
| ヒット判定 | `GameUiInput.IsPointerOverHud` を uGUI RectTransform ベースに更新（配置クリック透過防止） |
| レイアウト | 1280×720 基準 — 左上資源、左下生産、上部中央タイム、中央勝敗オーバーレイ |
| Editor | `Phase10SceneBuilder` — `EnsureHudUi()` + メニュー `AoE → Migrate HUD to uGUI (Phase53)` |
| OnGUI 残存 | **ワールド空間系のみ** — `SelectionBoxView`, `UnitHpBarView`, `CpuHudView`（Debug）は Phase 53 対象外 |

**やらないこと:** Minimap（54）/ Animation（55）/ 配置プレビュー改修 / フォント・スキン本格 polish / TMP パッケージ追加（`UnityEngine.UI.Text` + `Button` で十分）

---

## ② 移行対象（優先順）

| 優先 | クラス | 内容 | アンカー目安 |
|------|--------|------|-------------|
| P0 | `ResourceHudView` | 資源 4 + Pop + 文明 + **建築メニュー 15 ボタン** | 左上 |
| P0 | `GameTimeHudView` | 経過時間・言語切替・CPU pace | 上中央 |
| P0 | `VictoryDefeatHudView` | 勝敗タイトル + リスタートヒント | 中央オーバーレイ |
| P1 | `ProductionPanelView` | TC — Villager 訓練・Age Up・キュー・進捗 | 左下 |
| P1 | `BarracksPanelView` | Militia / Spearman 等 | 左下（建築選択時） |
| P1 | `ArcheryRangePanelView` | Archer | 左下 |
| P1 | `StablePanelView` | Scout / Knight 等 | 左下 |
| P1 | `BlacksmithPanelView` | 研究ボタン | 左下 |
| P1 | `MarketPanelView` | 交易 UI | 左下 |
| P1 | `SelectionInfoPanelView` | 選択ユニット/建築/資源ノード情報 | 左下（生産パネル上） |
| P1 | `IdleUnitHudView` | Idle カウント + 次 Idle 選択 | 資源 HUD 右隣 |
| P2 | `UnitStancePanelView` | Aggressive / Defensive / Stand Ground | 左下（軍事選択時） |

**共有:** `ProductionQueuePanelUi` — OnGUI `GUILayout` 版を uGUI 版（`ProductionQueuePanelUi` 拡張 or 専用 `ProductionQueueListView`）に置換。

---

## ③ 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| Canvas シェル | `Phase10SceneBuilder.EnsureGameplayCanvas`, `EnsureInputSystemEventSystem` |
| HUD ヒット | `GameUiInput`, `ResourceHudView.IsPointerOverHud`, `SelectionManager` クリックスキップ |
| 資源・建築 | `ResourceHudView` — `Update` 内ホットキー配置、`OnGUI` 描画 |
| 生産 | `ProductionPanelView`, `BarracksPanelView`, … — `CommandQueue.Enqueue` パターン |
| キュー | `ProductionQueuePanelUi`, `ProductionManager` |
| i18n | `Localization`, `LanguageMapBootstrap` — 言語変更時 UI 再描画 |
| 配置 | `BuildingPlacementManager` — HUD 上クリックを地面命令に流さない |

---

## ④ 実装タスク

### 1. uGUI ビルダー（Editor / Runtime）

- **方針:** Prefab 手書き禁止 — `Phase10SceneBuilder` または View の `Awake` で **コード生成**
- `HudRoot` 配下に子パネル:
  - `ResourceHudPanel`（RectTransform anchor top-left）
  - `IdleHudPanel`（Resource の右）
  - `GameTimeHudPanel`（top-center）
  - `BottomLeftStack`（VerticalLayoutGroup — 生産 + Selection Info + Stance）
  - `VictoryOverlay`（stretch full screen, 初期 inactive）
- 各 View に `[SerializeField] RectTransform panelRoot` 等 — Editor メニューで `[SerializeField]` 配線

### 2. `GameUiInput` リファクタ

- OnGUI Rect 蓄積（`ExpandHudPanelScreenRect`）を **uGUI 版**に:
  - 登録 API: `RegisterHudRect(RectTransform)` or 単一 `RectTransform hudBlockerRoot`
  - `IsPointerOverHud`: `RectTransformUtility.RectangleContainsScreenPoint` + Canvas カメラ（Overlay は `null`）
  - または `EventSystem.current.IsPointerOverGameObject()` + HudRoot レイヤー限定
- OnGUI 残存パネル（SelectionBox 等）との **併用**を維持

### 3. 各 View の uGUI 化（small diff）

- `OnGUI` / `GUILayout` を **削除**
- `Update` or `LateUpdate` で:
  - テキスト（資源数・Idle 数）を `Text.text = Localization.Format(...)` で更新
  - ボタン `interactable` — 既存 `CanBuild` / 資源 / `GameSessionManager.IsGameOver` 条件をそのまま適用
  - パネル `SetActive` — 選択状態（TC / Barracks / Villager 等）で表示切替
- **ロジック変更禁止:** `CommandQueue`, `BuildingPlacementManager.Enter*PlacementMode`, ホットキー `Update` は現行維持
- 言語切替（`Localization` 変更イベント or `GameTimeHudView` から `RefreshAllHud()`）でラベル更新

### 4. 建築ボタン gray-out

- Phase 50 `GameSessionManager.CanBuild` — ボタン `interactable = false` + 視覚的 gray（`ColorBlock` disabledColor）
- Palisade / Stone Wall / Gate — Feudal 前後の表示ルール維持

### 5. Editor / Phase10

- `[MenuItem("AoE/Migrate HUD to uGUI (Phase53)")]`
  - `EnsureGameplayCanvas()` + `EnsureInputSystemEventSystem()`
  - `EnsureHudUi()` — 上記階層生成 + 各 View の SerializeField 配線
- フルシーン再生成パス（`BuildPhase10Scene`）にも `EnsureHudUi()` を組込

### 6. Play 確認・回帰

- Phase 49/50: 壁ドラッグ列配置・Gate・時代別壁
- Phase 51: `L` / 言語ボタンで JA 切替 — **全 uGUI ラベルが追従**
- Phase 52: 単体/列ゴースト — 変更なし
- HUD 上クリックが **地面選択・移動に透過しない**
- Console エラーなし

---

## ⑤ 制約

- small diff only / rewrite 禁止
- Simulation Manager が uGUI 具体型を直接参照しない（View 層に閉じる）
- NavMesh 禁止
- Asset Store / 手書き Prefab 禁止 — Editor API + コード生成 UI
- `Localization` は Core — View から参照可
- `InputSystemUIInputModule` を `StandaloneInputModule` に戻さない

---

## ⑥ Play 確認

1. `Phase10.unity` — `AoE → Migrate HUD to uGUI (Phase53)` → Ctrl+S
2. Debug + CPU Relaxed — 資源表示・建築ボタン 15 種
3. Villager 選択 → House (H) — uGUI ボタン + ホットキー両方
4. TC 選択 → Villager 訓練・Age Up・キューキャンセル
5. Barracks / Market / Blacksmith — 各パネル表示
6. `L` — 日本語（木材・村民・勝利 等）
7. HUD 上ドラッグ — 選択ボックスが地面に落ちない（ヒット判定）
8. TC 破壊 — 勝敗オーバーレイ + `R` リスタート
9. Palisade ドラッグ — 列ゴースト（Phase 52）維持
10. Console エラーなし

---

## ⑦ 完了時ドキュメント

- [x] `IMPLEMENTATION_STATUS.md` — Phase 53 ✅ / uGUI HUD ✅ / OnGUI 主要パネル除去 ✅
- [x] `09_M5_VISUAL_UI_PHASES.md` — Phase 53 ✅
- [x] 本プロンプト — ✅

---

Phase 53 のみ。**Phase 54（Minimap）・Phase 55（Animation）には触れない。**

> **次:** [phase54-prompt.md](phase54-prompt.md) — Minimap

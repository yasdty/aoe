# Phase 48 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜47 完了（M4 Second TC）  
> **マイルストン:** M4 AoE Gameplay — **RTS UX Polish**（M2.6 後回し分の回収）  
> **ロードマップ:** [08_M4_GAMEPLAY_PHASES.md](../08_M4_GAMEPLAY_PHASES.md)  
> **関連:** [05_M2_6_RTS_UX_PHASES.md](../05_M2_6_RTS_UX_PHASES.md) — Phase 31〜34 済み。本 Phase は **未実装 UX ギャップ**  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 48 実装（RTS UX Polish）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 48 のみ実装。** 既存 **生産キュー / 配置 / OnGUI パネル** を拡張（rewrite 禁止）。

**前提:** Phase 31 で TC/Barracks **ユニット生産キュー**は実装済み。Phase 44 で壁は **1 マスずつ配置 MVP**。本 Phase で AoE2 操作の残ギャップを埋める。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| 生産キュー取消 | キュー内アイテムを **クリックでキャンセル** + **資源返金**（TC / Barracks / Archery / Stable） |
| Shift+5 一括キュー | TC / Barracks 選択中 **Shift+Q**（または Shift+ホットキー）で **5 体**一括キュー |
| House 破壊 | House 破壊時 **Pop cap 減少**（`PopulationManager` 連動） |
| 建築ホットキー | **House / Barracks** 等 — Input System に `BuildHouse` 等追加（村民選択時） |
| **壁 Shift+ドラッグ** | Palisade / Stone Wall — **Shift 押下中にドラッグ**でセグメント列配置（Phase 44 の 1 マス MVP を拡張） |

**やらないこと:** uGUI 本格 HUD（M5）/ 生産キュー UI の本格リデザイン / Gate / 全建築ホットキー一括 / M5 View Layer

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| 生産キュー | `ProductionManager` / `BarracksProductionManager` 等 — `TryQueueProduction` / `activeJobs` |
| 生産 UI | `ProductionPanelView` / `BarracksPanelView` — OnGUI キュー表示 |
| 人口 | `PopulationManager` — `AddHousing` / House 完成時（Phase 45 修正済み） |
| 壁配置 | `BuildingPlacementManager` — `EnterPalisadeWallPlacementMode` / `TryConfirmPlacement` |
| Input | `RTSInputActionsBuilder` / `RTSInputReader` — 既存 TrainVillager 等 |
| HUD | `ResourceHudView` — Build ボタン列 |

---

## ③ 実装タスク

### 1. 生産キュー取消 + 返金

- 各 `*ProductionManager` に `TryCancelQueueItem(building, index)` または先頭キャンセル MVP
- **返金:** キュー追加時に消費した Food/Wood/Gold を **Data から再計算**して返却（二重返金禁止）
- `ProductionPanelView` — キュー行クリックでキャンセル（OnGUI）
- `CommandQueue` 経由の Cancel コマンド（任意・将来 Replay 用）

### 2. Shift+5 一括キュー

- TC: `Shift+Q` → Villager ×5（Pop 上限・資源不足で打ち切り）
- Barracks: `Shift+Q` → Militia ×5（同様）
- 既存 Q キー / `TrainVillagerCommand` パターンを拡張

### 3. House 破壊 → Pop cap 減

- `BuildingHealth.Die` → `House` 返却時に `PopulationManager.RemoveHousing(team, data.housingProvided)`
- 現在人口 > 新 cap のとき **既存ユニットは維持**（AoE2 同型 — 新規訓練のみブロック）

### 4. 建築ホットキー（MVP 2 種で可）

- Input System: `BuildHouse`（例: `H`）/ `BuildBarracks`（例: `B`）
- 村民選択中のみ `BuildingPlacementManager.Enter*PlacementMode`
- `ResourceHudView` ボタンと同条件（資源・Age・game over）

### 5. 壁 Shift+ドラッグ連続配置

- Shift 押下中: 配置確定後も **配置モード維持**
- ポインタ移動で **隣接セグメント**を連続 `TryConfirmPlacement`（グリッド or 最小間隔は Phase 44 の footprint 準拠）
- Esc / 右クリックで終了（既存 `CancelPlacementMode`）
- Stone Wall も同ロジック

### 6. Phase10 / ドキュメント

- Input Actions 更新 → `AoE → Fix Phase1 Input References` または Phase10 builder 配線
- 新規 `.meta` 手書き禁止

---

## ④ 制約

- rewrite 禁止 / small diff only
- Phase 42 Balance / Phase 47 Second TC / CPU Relaxed を **壊さない**
- OnGUI のまま（M5 uGUI 移行は Phase 50）
- 返金・Pop 減は **1 箇所の Utility** 経由推奨（Manager 散在禁止）

---

## ⑤ Play 確認

1. `Phase10.unity` — Debug + CPU Relaxed
2. TC キュー 3 体 → **2 体目クリック取消** → Food 返金 + キュー短縮
3. Barracks **Shift+Q** → Militia 5 体キュー（資源足りる範囲）
4. House 5 棟 → 1 棟破壊 → **Pop cap -5**
5. Shift 押しながら Palisade を **ドラッグ** → 壁列配置
6. **`H`** で House 配置モード開始（村民選択時）
7. Console エラーなし

---

## ⑥ 完了時ドキュメント

- [ ] `IMPLEMENTATION_STATUS.md` — Phase 48 ✅ / M4 完了条件更新
- [ ] `08_M4_GAMEPLAY_PHASES.md` — Phase 48 ✅
- [ ] 本プロンプト — Play 確認待ち → ✅

---

Phase 48 のみ。**M5 Phase 49 View Layer には触れない。**

# Phase 45 実行プロンプト

> **状態:** ✅ 実装済み（Play 確認推奨）  
> **前提:** Phase 1〜44 完了（M4 Defense）  
> **マイルストン:** M4 AoE Gameplay — **Market**  
> **ロードマップ:** [08_M4_GAMEPLAY_PHASES.md](../08_M4_GAMEPLAY_PHASES.md)  
> **Balance 方針:** [12_GAMEPLAY_BALANCE_MODE.md](../12_GAMEPLAY_BALANCE_MODE.md) — 建築コストは **GameplayBalance 層経由**（交易レートは固定 MVP）  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 45 実装（Market）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 45 のみ実装。** 既存 **経済建築パターン**（Mill / LumberCamp / Blacksmith）を Market に流用（rewrite 禁止）。

**前提:** Phase 42〜44 で `requiredAge` / `GameplayBalance` / `ResourceManager`（Food/Wood/Gold/Stone）/ `BuildingPlacementManager` が整備済み。本 Phase で **市場建築 + 資源売買 MVP** を完成させる。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| 建築 | **Market** — Feudal 以降、村民が建築可能（Wood） |
| 売買 | 固定レートで **1 資源 ↔ Gold**（MVP は各方向 1 ボタンずつで可） |
| 対象資源 | **Food / Wood / Stone** を売却 → Gold 獲得 / Gold で購入（AoE2 簡略版） |
| UI | Market 選択時 OnGUI パネル（`BlacksmithPanelView` 同型） |
| コマンド | `CommandQueue` 経由（将来リプレイ整合） |
| Balance | 建築コスト・時間のみ `GameplayBalance` — **交易レートは定数 Data**（Debug 短縮不要） |
| CPU | 交易は **MVP 任意**（プレイヤー優先） |

**やらないこと:** キャラバン / 交易手数料の距離減衰 / 複数 Market ボーナス / 文明ボーナス（46）/ 壁 Shift+ドラッグ（48）/ uGUI 本格 HUD / Dock・漁船

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| 建築 Kind | `PlacedBuildingKind`、`PlacedBuildingData`、`GameAge` |
| 配置 | `BuildingPlacementManager` — `EnterMillPlacementMode` 等 / `CompleteConstruction` |
| ファクトリ | `RuntimeBuildingFactory`、`PlacedBuildingDataResolver` |
| Data Sync | `Phase1SceneBuilder.EnsureMillData()` / `EnsureBlacksmithData()` |
| 資源 | `ResourceManager` — `TrySpend*` / `Add*`（Gold 含む） |
| パネル UI | `BlacksmithPanelView` — OnGUI + `CommandQueue.Enqueue` |
| HUD | `ResourceHudView` — `GameSessionManager.CanBuild` で Feudal 判定 |
| 選択 | `SelectionManager` — `SelectedBlacksmith` パターンで `SelectedMarket` |

---

## ③ 実装タスク

### 1. `PlacedBuildingKind.Market` + Data

```csharp
// PlacedBuildingKind.Market = 12
```

- `Phase1SceneBuilder.EnsureMarketData()`
- AoE2 基準: Feudal 必要、Wood 175、建築時間 60s（`GameplayBalance` で Debug 短縮）
- `GameAssetPaths.DefaultMarketData`

### 2. `Market.cs` + `MarketTradeRates`（または `MarketTradeData` ScriptableObject）

固定レート MVP 例（調整可・Data 駆動）:

| 操作 | レート例 |
|------|----------|
| 売 Food | 100 Food → 50 Gold |
| 買 Food | 50 Gold → 100 Food |
| 売 Wood | 100 Wood → 50 Gold |
| 買 Wood | 50 Gold → 100 Wood |
| 売 Stone | 100 Stone → 50 Gold |
| 買 Stone | 50 Gold → 100 Stone |

- 取引単位は **100 固定** で MVP（ボタン 1 回 = 1 単位）
- 資源不足 / Gold 不足時はボタン無効 + ラベル表示

### 3. 交易実行

- `MarketTradeManager` または static helper — 即時反映（キュー不要）
- `TradeFoodForGoldCommand` 等 — `IGameCommand` + `CommandLog` 記録
- プレイヤーチームのみ MVP（CPU は後回し可）

### 4. 配置・HUD・選択

- `RuntimeBuildingFactory.CreateMarket`
- `BuildingPlacementManager.EnterMarketPlacementMode`
- `ResourceHudView` — Feudal 以降 Market ボタン
- `SelectionManager` — Market クリック選択 + HP 表示
- `MarketPanelView` — 売買ボタン OnGUI

### 5. Phase10 配線

- `Phase10SceneBuilder` — Data 参照・Placement・HUD・Panel
- 既存シーン向け `AoE → Add Market (Phase45)` パッチメニュー（Phase 44 パターン）
- `AoE → Sync AoE2 Game Data` に `EnsureMarketData` 追加

---

## ④ 制約

- rewrite 禁止 / small diff only
- `.meta` 手書き禁止
- 建築コストは **GameplayBalance のみ**（Debug 用 duplicate Data 禁止）
- OnGUI MVP
- Phase 42 Balance / CPU Relaxed / Phase 43 Blacksmith / Phase 44 Defense を **壊さない**
- `ResourceHudView` の Gold / Stone 表示は既存を維持・取引後に即更新

---

## ⑤ Play 確認

1. `Phase10.unity` — **Debug + CPU Relaxed**
2. Feudal 昇格後 Market を Wood で配置 → 建築完了
3. Market 選択 → Food を売却 → Gold 増加・Food 減少
4. Gold で Wood 購入 → 資源が期待どおり変化
5. 資源不足時ボタン無効
6. `CommandLog: tick=... Trade*` が記録される
7. Console エラーなし

---

## ⑥ 完了時ドキュメント

- [x] `IMPLEMENTATION_STATUS.md` — Phase 45 ✅
- [x] `08_M4_GAMEPLAY_PHASES.md` — Phase 45 ✅
- [x] 本プロンプト — Play 確認待ち → ✅

---

Phase 45 のみ。**Phase 46 Civilization には触れない。**

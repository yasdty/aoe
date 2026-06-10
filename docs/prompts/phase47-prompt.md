# Phase 47 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜46 完了（M4 Civilization）  
> **マイルストン:** M4 AoE Gameplay — **Second TC**  
> **ロードマップ:** [08_M4_GAMEPLAY_PHASES.md](../08_M4_GAMEPLAY_PHASES.md)  
> **Balance 方針:** [12_GAMEPLAY_BALANCE_MODE.md](../12_GAMEPLAY_BALANCE_MODE.md) — 建築コスト・時間は **GameplayBalance 層経由**  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 47 実装（Second TC）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 47 のみ実装。** 既存 **建築配置パターン**（House / Market）と **TC 生産**（`ProductionManager` / `TownCenter`）を拡張（rewrite 禁止）。

**前提:** Phase 42 で Feudal 昇格済み。Phase 46 で文明ボーナス適用済み。現状 TC は **シーン配置 1 基のみ** — 村民による 2 台目建設は不可。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| 建築 | **2 台目 Town Center** — Feudal 以降、村民が建築可能 |
| コスト例 | Wood 275 + Stone 100（AoE2 簡略 / `GameplayBalance` 適用） |
| 上限 | チームあたり **最大 2 基**（開始 TC + 追加 1） |
| 生産 | 各 TC が **独立キュー**で村民訓練（`ProductionManager` は既に TC 単位キュー対応） |
| 搬入 | 資源搬入は **最寄り TC**（または LumberCamp 等既存 deposit 優先ロジック維持） |
| 勝敗 | **最後の TC 破壊時のみ** Defeat / Victory（2 基あって 1 基落ちてもゲーム続行） |
| HUD | 村民選択時 `Build Town Center` ボタン（Feudal + 上限未満 + 資源足り） |
| CPU | 専用 AI 不要（建設不可のままで可） |

**やらないこと:** 3 基目以降 / Imperial 追加 TC / Castle / 文明固有 TC ボーナス / RTS UX Polish（48）/ uGUI 本格 HUD

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| TC 本体 | `TownCenter.cs`、`BuildingData`（`EnsureTownCenterData`） |
| 生産 | `ProductionManager` — `Register` / `TryQueueProduction` / `GetTownCenterForTeam` |
| 配置 | `BuildingPlacementManager` — `EnterHousePlacementMode` / `CompleteConstruction` |
| ファクトリ | `RuntimeBuildingFactory`、`Phase1SceneBuilder.CreateTownCenter` |
| 勝敗 | `BuildingHealth.Die` → `GameSessionManager.NotifyTownCenterDestroyed` |
| 搬入 | `GatherManager.GetDepositPosition` / `LumberCampRegistry` / `MillRegistry` |
| HUD | `ResourceHudView` — Build ボタン列 |
| Age | `GameSessionManager.CanBuild` / `PlacedBuildingData.requiredAge` |

---

## ③ 実装タスク

### 1. 配置用 Data — `PlacedBuildingKind.TownCenter`

```csharp
// PlacedBuildingKind.TownCenter = 13
```

- `PlacedBuildingData` — Feudal 必要、Wood 275、Stone 100、建築時間 ~150s（Balance 層）
- `Phase1SceneBuilder.EnsureTownCenterPlacementData()`（既存 `BuildingData` の train 設定とは分離可）
- `GameAssetPaths.DefaultTownCenterPlacementData` 等
- 既存 `BuildingData`（村民コスト・訓練時間）は **完成 TC** に引き継ぎ

### 2. 配置・完成フロー

- `BuildingPlacementManager.EnterTownCenterPlacementMode`
- `RuntimeBuildingFactory.CreateTownCenter`（または `CreatePlacedTownCenter`）— Pool / 新規 Instantiate
- 完成時 `TownCenter` + `ProductionManager.Register`
- **上限チェック:** `ProductionManager.GetTownCenterCountForTeam(team) >= 2` なら配置開始不可

### 3. `ProductionManager` 拡張

```csharp
// 追加 API 例
GetTownCenterCountForTeam(UnitTeam team)
HasAnyTownCenterForTeam(UnitTeam team)
GetNearestTownCenter(UnitTeam team, Vector3 fromPosition) // 搬入・Age Up 用
```

- `GetTownCenterForTeam` — 後方互換のため **最初の 1 基**を返すか、呼び出し側を `GetNearestTownCenter` へ段階移行
- **採集 Manager**（Wood/Food/Gold/Stone）の TC 直搬入フォールバックを **最寄り TC** に変更

### 4. 勝敗条件

- `GameSessionManager.NotifyTownCenterDestroyed` — `HasAnyTownCenterForTeam` が false のときのみ Defeat/Victory
- `BuildingHealth` の TC 破壊フローは維持

### 5. UI / 選択

- `ResourceHudView` — `Build Town Center (275W 100S)` ボタン（Feudal + TC 数 < 2）
- `SelectionManager` / `ProductionPanelView` — 2 基目 TC 選択でも村民キュー・Age Up が動作
- `SelectionInfoPanelView` — TC 表示（既存があれば 2 基対応確認）

### 6. Phase10 / メニュー

- `Phase10SceneBuilder` — `BuildingPlacementManager.townCenterPlacementData` 配線
- `AoE → Add Second TC (Phase47)` パッチメニュー
- `AoE → Sync AoE2 Game Data` に `EnsureTownCenterPlacementData` 追加

---

## ④ 制約

- rewrite 禁止 / small diff only
- `.meta` 手書き禁止
- Phase 42 Balance / Phase 46 Civilization / Market / CPU Relaxed を **壊さない**
- 開始 TC（シーン配置）は **カウントに含める**（合計 2 上限）

---

## ⑤ Play 確認

1. `Phase10.unity` — **Debug + CPU Relaxed**
2. Feudal 昇格後、村民選択 → **Build Town Center** 表示
3. 2 台目 TC 建設完了 → 両方で村民訓練キュー投入可能
4. 1 基目 TC 破壊 → **ゲーム続行**（2 基目で生産・搬入可能）
5. 最後の TC 破壊 → Defeat / Victory
6. Console エラーなし

---

## ⑥ 完了時ドキュメント

- [ ] `IMPLEMENTATION_STATUS.md` — Phase 47 ✅
- [ ] `08_M4_GAMEPLAY_PHASES.md` — Phase 47 ✅
- [ ] 本プロンプト — Play 確認待ち → ✅

---

Phase 47 のみ。**Phase 48 RTS UX Polish には触れない。**

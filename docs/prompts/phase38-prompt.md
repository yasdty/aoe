# Phase 38 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜37 完了（M3 Spearman 含む）  
> **マイルストン:** M3 Military — **Stable + Cavalry + Scout**  
> **ロードマップ:** [07_M3_MILITARY_PHASES.md](../07_M3_MILITARY_PHASES.md)  
> **Balance 方針:** [12_GAMEPLAY_BALANCE_MODE.md](../12_GAMEPLAY_BALANCE_MODE.md) — 本 Phase は **MVP 暫定値**（Phase 36 Archery Range 同型）。AoE2 正本化は M3 完了後。  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 38 実装（Stable + Cavalry + Scout）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 38 のみ実装。** Archery Range（新建築）+ Barracks Spearman（2 スロット生産）パターンを **複製・拡張**（rewrite 禁止）。

**ユーザー確定（2026-06）:**

- **Scout:** Stable から **Cavalry と両方生産**（第 2 スロット）
- **バランス:** Phase 36 同型 **MVP 暫定値**（Balance Mode 実装は M3 完了後）

---

## ① 目的

AoE2 準拠で **Stable 建築**から **Cavalry（高機動近接）** と **Scout（軽騎兵・より高速）** を生産・戦闘させる。

| 項目 | MVP |
|------|-----|
| 建築 | Stable（村民建築・配置ゴースト） |
| 生産 | Q = Cavalry / **E = Scout** + 同一 FIFO キュー（最大 15） |
| 戦闘 | 近接（`AttackManager` 既存）。Spearman 対騎兵ボーナスは Phase 39 |
| Rally | Phase 33 / 36 パターン — Stable 選択 + 右クリック |
| CPU | Archery Range 後に Stable 建設・Cavalry / Scout 生産（単純ルール） |

**やらないこと:** Counter System 本格化 / 時代昇格 / Balance Mode 本体 / Animator / Knight 系上位ユニット

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| 新建築（Phase 36） | `ArcheryRange.cs`, `ArcheryRangeProductionManager.cs`, `BuildingPlacementManager.EnterArcheryRangePlacementMode` |
| 2 スロット生産（Phase 37） | `PlacedBuildingData.secondaryTrain*`, `Barracks.TryQueueSpearmanProduction`, `BarracksPanelView` |
| Editor Data | `EnsureArcheryRangeData`, `EnsureArcherData`, `EnsureSpearmanData`, `EnsureBarracksData` |
| 入力 | `TrainSecondary`（E）— Phase 37 で追加済み |
| CPU 軍事 | `CpuMilitaryAiManager.cs` |
| Scene | `Phase10SceneBuilder.cs` |

---

## ③ 実装タスク（small diff）

### 1. Data

- `PlacedBuildingKind.Stable`
- `GameAssetPaths.DefaultStableData`, `CavalryData`, `ScoutData`
- `Phase1SceneBuilder.EnsureCavalryData()` — 目安: HP 45 / 攻 6 / 装甲 0 / 射程 2 / CD 1s / **移動 7**（Spearman より速い）
- `Phase1SceneBuilder.EnsureScoutData()` — 目安: HP 30 / 攻 3 / 装甲 0 / 射程 2 / CD 1.5s / **移動 9**（偵察用・最速）
- `Phase1SceneBuilder.EnsureStableData(UnitData cavalryData, UnitData scoutData)` — **MVP 暫定**（Phase 36 Archery Range 同型）  
  - 建築: Wood **150** / **40s** / HP 300 / footprint 6×6  
  - Cavalry 訓練: 例 **60 Food + 20 Wood / 5s**  
  - Scout 訓練: 例 **80 Food + 0 Wood / 6s**（Food 重め・軽量）
- `PlacedBuildingDataResolver.ResolveStable`
- `secondaryTrain*` に Scout を配線（Barracks Phase 37 パターン）

### 2. 建築・生産

- `Stable.cs` — `ArcheryRange.cs` + `Barracks` 2 スロット API を合成（`TryQueueCavalryProduction` / `TryQueueScoutProduction`）
- `StableProductionManager.cs` — `ArcheryRangeProductionManager` 同型 + **Wood+Food**（Phase 37 Barracks パターン）
- `BuildingPlacementManager.EnterStablePlacementMode`
- `RuntimeBuildingFactory` / `BuildingPool` — Stable スポーン
- `ProductionRallyApplier.Apply(Stable, Unit)`

### 3. 選択・UI・入力

- `SelectionManager` — Stable クリック選択、Rally 右クリック
- `StablePanelView` — OnGUI: Q Cavalry / E Scout（`BarracksPanelView` 2 ボタン同型）
- `ResourceHudView` — Build Stable ボタン
- `SelectionInfoPanelView` — Stable 選択時 ReserveHeight（Barracks / Archery と同様）
- `TrainCavalryCommand` / `TrainScoutCommand`

### 4. CPU

- `CpuMilitaryAiManager` — Barracks + Archery Range 完成後、Wood/Food 余裕時に Stable 建設  
  - Cavalry 目標（例: 3）/ Scout 目標（例: 2）  
  - `CollectCpuAttackUnits` に `"Cavalry"` / `"Scout"` を `displayName` で追加

### 5. Phase10 SceneBuilder

- `EnsureStableData` / `EnsureCavalryData` / `EnsureScoutData` 呼び出し
- Managers / Panel / Placement 配線
- **シーン再生成:** `AoE → Setup Phase10 Scene`

---

## ④ 制約

- rewrite 禁止 / small diff only
- Unity アセット手書き禁止 — Editor API
- `.meta` 手書き禁止
- Militia / Spearman / Archer / Archery Range 既存挙動を壊さない
- **Balance Mode（GameplayBalance 層）は Phase 38 では触らない** — Data に MVP 値を直接書く

---

## ⑤ Play 確認

1. `AoE → Setup Phase10 Scene` → Play
2. Stable 建築 → **Q** Cavalry / **E** Scout 交互キュー → FIFO 混在スポーン
3. Cavalry / Scout が Militia より速く移動、近接攻撃
4. Rally 右クリック → スポーン後移動
5. CPU が Stable + 騎兵を生産（ログ or 観察）
6. Phase 31〜37 回帰

---

## ⑥ 完了時ドキュメント

- [x] `IMPLEMENTATION_STATUS.md` — Phase 38 ✅
- [x] `07_M3_MILITARY_PHASES.md` — Phase 38 ✅
- [x] 本プロンプト — ✅

---

Phase 38 のみ。**Phase 39 Counter System には触れない。**

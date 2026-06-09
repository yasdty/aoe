# Phase 30 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜29 完了（PoC + Foundation + M2 Economy + M2.5 Phase 21〜29）  
> **マイルストン:** M2.5 Economy Polish（**最終 Phase**）  
> **ロードマップ:** [04_M2_5_ECONOMY_POLISH_PHASES.md](../04_M2_5_ECONOMY_POLISH_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 30 実装（CPU 4 Resources）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜29 は完了済み。Phase 30 のみ実装すること。**

---

## ① M2.5 Economy Polish 方針（必読・遵守）

Phase 29 で Militia Basic Aggro が完成。Phase 30 は **M2.5 最終** — CPU が 4 資源経済を回す。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | `CpuEconomyAiManager` 拡張 — Food / Gold / Stone + Camp 建築 |
| **small diff** | **`CpuEconomyAiManager.cs` 拡張** — rewrite / 新 AI フレームワーク禁止 |
| **既存パターン再利用** | `GatherManager.IssueGatherCommand` / `FoodGatherManager.*` / `MineralGatherManager.*` / `BuildingPlacementManager.TryStartTeamConstruction` |
| **既存ゲームを壊さない** | Player 操作 / Militia Aggro / Boar / 羊 / Mill / Foundation 全機能 |
| **Foundation 維持** | CPU は **Command 化しない** — Manager 直接 `Issue*` 呼び出し（既存方針） |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- CPU 評価は **`ISimulationTickable`**（`EvaluateInterval = 2f` 維持）
- **`.meta` は 32 文字 GUID**

---

## ② Phase 29 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **Player 経済** — Wood / Food / Gold / Stone 全採取 + TC / Lumber Camp / Mining Camp / Mill Drop-off + 採取リピート
- **CPU 経済** — **Wood のみ**（`AssignIdleVillagersToTrees`）
- **CPU 建築** — House（Pop 逼迫時）のみ — **Camp / Mill / Farm なし**
- **CPU Villager** — 目標 6 体、Idle → 最寄り木へ `GatherManager.IssueGatherCommand`
- **CPU 資源ノード** — Phase10 シーンに CPU 側 Berry / Deer / Sheep / Gold / Stone 配置済み
- **CPU Militia** — `CpuMilitaryAiManager`（Barracks + 攻撃波）— Phase 29 Aggro 対称

### 現状のギャップ（Phase 30 で解消）

| 項目 | 現状 |
|------|------|
| CPU Food | 採取なし — Berry / 狩り / Farm 未使用 |
| CPU Gold / Stone | 採取なし — Mining Camp 未建築 |
| CPU Drop-off 拠点 | Lumber / Mining / Mill 未建築 |
| CPU 需要判断 | Wood + Pop + Villager のみ |
| CpuHudView | CPU Wood / Pop 表示 — Food/Gold/Stone は内部で増えるが AI が触らない |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| CPU 経済（拡張対象） | `Assets/Scripts/AI/CpuEconomyAiManager.cs` |
| CPU 軍事（共存） | `Assets/Scripts/AI/CpuMilitaryAiManager.cs` |
| 木採取参考 | `Assets/Scripts/Economy/GatherManager.cs` — `IssueGatherCommand` / `IsUnitGathering` |
| Food 採取 | `Assets/Scripts/Economy/FoodGatherManager.cs` — Berry / Farm / Hunt |
| 鉱物採取 | `Assets/Scripts/Economy/MineralGatherManager.cs` — Gold / Stone |
| Berry 探索 | `Assets/Scripts/Spatial/BerryBushSpatialIndex.cs` — `FindNearestAvailable` |
| 木探索 | `Assets/Scripts/Spatial/TreeSpatialIndex.cs` — `FindRankedAvailable` |
| 建築 | `Assets/Scripts/Buildings/BuildingPlacementManager.cs` — `TryStartTeamConstruction` / `TryFindPlacementNear` |
| Camp Registry | `LumberCampRegistry.cs` / `MiningCampRegistry.cs` / `MillRegistry.cs` |
| 羊 / 狩り | `SheepResource.cs` / `SheepRegistry.cs` / `AnimalDiscoveryManager.cs` |
| Phase10 配置 | `Assets/Scripts/Editor/Phase10SceneBuilder.cs` — CPU Berry / Mine 座標 |
| HUD | `Assets/Scripts/Selection/CpuHudView.cs` |

---

## ③ Phase 30 目的

**CPU が Player と同様に 4 資源を増やし、Villager / Militia 生産を継続する。** M2.5 Economy Polish の締め。

### MVP フロー（評価 Tick 毎 — 2 秒）

```
TryBuildHouse()          — 既存維持
TryTrainVillager()       — 既存維持
TryBuildEconomyBuildings() — 新規: Mill / Lumber Camp / Mining Camp / Farm（条件付き）
AssignIdleVillagers()    — 既存 Wood 専用 → 優先度付き 4 資源割当
```

### Villager 割当優先度（MVP 推奨）

| 優先 | 条件 | アクション |
|------|------|------------|
| 1 | Food < 閾値 & 近傍 Berry あり | `FoodGatherManager.IssueGatherCommand` |
| 2 | Food < 閾値 & 所属 Sheep/Deer あり（CPU 発見済み） | `FoodGatherManager.IssueHuntCommand` |
| 3 | Gold < 閾値 & CPU Mining Camp あり & 近傍 Gold Mine | `MineralGatherManager.IssueGatherGoldCommand` |
| 4 | Stone < 閾値 & CPU Mining Camp あり & 近傍 Stone Mine | `MineralGatherManager.IssueGatherStoneCommand` |
| 5 | Food < 閾値 & CPU Farm あり & 空き Farm | `FoodGatherManager.IssueGatherFarmCommand` |
| 6 | デフォルト | 既存 `GatherManager.IssueGatherCommand`（Wood） |

**閾値（案）:** Food < 200 / Gold < 100 / Stone < 100 — 定数で OK。Wood は既存どおり常時割当可。

### 建築判断（MVP — Wood 余裕時）

| 建築 | 条件（案） |
|------|------------|
| **Mill** | Food 採取需要 & 未所有 & Wood ≥ 100 & Idle builder あり |
| **Mining Camp** | (Gold OR Stone) 需要 & 未所有 & Wood ≥ 100 |
| **Lumber Camp** | Wood 需要が高い & 未所有 & Wood ≥ 100（optional — TC のみでも可） |
| **Farm** | Berry 枯渇 or Food 需要 & Mill or TC あり & Wood ≥ 60 |

`BuildingPlacementManager.TryStartTeamConstruction(data, placement, builder)` — House と同型。  
配置: CPU TC 近傍 `TryFindPlacementNear(center, minRadius, maxRadius, data, out placement)`。

**Camp 所有判定:** Registry 走査で `camp.Team == CpuTeam && camp.IsAlive`（`BuildingHealth` 参照）。

---

## ④ 今回実装するもの

### 1. `CpuEconomyAiManager` 拡張

- **SerializeField** — `millData`, `lumberCampData`, `miningCampData`, `farmData`（`PlacedBuildingDataResolver` で Awake 解決 — `CpuMilitaryAiManager.barracksData` 同型）
- **定数** — Food/Gold/Stone 閾値、建築 min/max radius（House 流用可）
- **`IsIdleForEconomy` 拡張** — Food / Mineral ジョブ中も Idle でない扱い  
  - 必要なら `FoodGatherManager.IsUnitGathering` / `MineralGatherManager.IsUnitGathering` を **small diff** で追加（内部 jobs 走査）
- **`TryBuildEconomyBuildings()`** — Mill → Mining Camp → Farm → Lumber Camp 順（1 Tick 1 建築まで）
- **`AssignIdleVillagers()`** — 上表優先度。1 Villager 1 ジョブ。既存 `gatherCommandBuffer` 再利用
- **Gold / Stone Mine 探索** — MVP: `Object.FindObjectsByType` または TC から最近傍 & `!IsDepleted`（専用 SpatialIndex 不要）
- **Berry** — `BerryBushSpatialIndex.FindNearestAvailable(villagerPos)`
- **狩り** — CPU 発見済み `SheepResource`（`!IsNeutral && OwnerTeam == CpuTeam`）または `DeerResource` を TC 近傍から最近傍
- **Farm** — `Farm[]` 走査で `farm.Team == CpuTeam && !farm.IsDepleted && !FoodGatherManager.IsFarmOccupiedByOther(...)`

### 2. Debug ログ（推奨）

House / Barracks と同型:

```csharp
Debug.Log($"CPU Mill construction started at {FormatTime(...)}");
Debug.Log($"CPU villager → Berry Bush (Food={ResourceManager.GetFood(CpuTeam):0})");
```

Console で CPU 経済判断が追えるようにする。

### 3. `CpuHudView` 拡張（optional / small）

- CPU Food / Gold / Stone 表示追加（1 行 or 既存行拡張）— **推奨**（Play 確認しやすい）

### 4. Phase10 SceneBuilder（optional）

- `CpuEconomyAiManager` の SerializeField に Mill/Lumber/Mining/Farm Data 参照を `CreateManagers` で設定（手動 Assign 不要に）

---

## ⑤ 今回やらないこと

- CPU 羊誘導（`SheepMoveCommand`）— **optional サブタスク**。発見 + 狩りのみで MVP OK
- CPU Militia / 軍事 AI 変更（Phase 29 維持）
- Player UI / Command 変更
- 本格ビルドオーダー / 難易度 / スカウト
- 市場・交易 / 時代昇格
- **M2.6 Phase 31**（Production Queue）以降

---

## ⑥ 実装ステップ（推奨）

| Step | 内容 |
|------|------|
| 30-1 | `IsUnitGathering` ヘルパー（Food / Mineral）+ `IsIdleForEconomy` 拡張 |
| 30-2 | `TryBuildEconomyBuildings` — Mill / Mining Camp / Farm |
| 30-3 | `AssignIdleVillagers` 優先度ロジック — Berry / Hunt / Gold / Stone / Farm / Wood |
| 30-4 | CpuHudView + Phase10 SerializeField + ドキュメント |

---

## ⑦ 技術メモ

### Issue* API（CPU から直接呼び出し）

```csharp
// Wood — 既存
GatherManager.IssueGatherCommand(buffer, tree);

// Food
FoodGatherManager.IssueGatherCommand(buffer, bush);
FoodGatherManager.IssueHuntCommand(buffer, deerOrSheep);
FoodGatherManager.IssueGatherFarmCommand(buffer, farm);

// Minerals
MineralGatherManager.IssueGatherGoldCommand(buffer, goldMine);
MineralGatherManager.IssueGatherStoneCommand(buffer, stoneMine);
```

いずれも Phase 21 **採取リピート** がそのまま CPU にも効く（追加実装不要）。

### House 建築との builder 競合

- `ShouldReserveBuilderForHouse()` — 既存維持
- 新 Camp 建築も `FindBuilderForHouse()` 同型の Idle villager を使用
- **1 評価 Tick で House OR Economy 建築 1 件** — 競合回避

### CPU Sheep 狩り

- Phase 28 `AnimalDiscoveryManager` が CPU Villager で Neutral Sheep を `Discover(Enemy)` 済み
- `IssueHuntCommand` — 所属 Sheep のみ（`CanBeHuntedBy(CpuTeam)`）
- 羊誘導（Move）は **不要** — TC 近くの Sheep を狩るだけで MVP

### Phase10 CPU 資源位置（参考）

- Berry: `(-6,-28), (6,-32), (0,-38)` — CPU TC `(0,0,-35)` 近傍
- Gold Mine: `(0,0,-28)` — CPU 側
- Stone Mine: `(-8,0,-38)` — CPU 側
- Deer / Sheep: CPU エリア配置済み

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`
- **Setup:** 変更不要（CPU 資源ノード既存）。Play 中 **CpuHudView** で CPU 4 資源増加を確認
- SerializeField 未設定時: `PlacedBuildingDataResolver.Resolve*` フォールバック

---

## ⑨ 完了条件（Phase 30 MVP）

- [ ] **CPU Berry** — Food 不足時 Idle Villager → Berry 採取 → TC/Mill 搬入 → Food 増加
- [ ] **CPU 狩り** — Deer or 発見済み Sheep → Hunt → Food 搬入
- [ ] **CPU Gold / Stone** — Mining Camp 建築後 → 採掘 → 搬入
- [ ] **CPU Farm**（optional 1 件）— 建築 → Farm Food 採取
- [ ] **CPU Wood** — 既存通り維持
- [ ] **採取リピート** — 搬入後同リソース継続（Phase 21 回帰）
- [ ] **House / Villager / Militia 波** — 既存 CPU AI 回帰
- [ ] **Player Aggro / 4 資源** 回帰
- [ ] Console エラーなし
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `04_M2_5` Phase 30 を ✅ — **M2.5 完了**

---

## ⑩ テスト手順（Play チェックリスト）

1. `Phase10.unity` → Play（Setup 不要）
2. **CpuHudView** — 時間経過で CPU **Food** が Berry/狩りで増える
3. CPU **Mining Camp** 建築（Console ログ）→ **Gold / Stone** 増加
4. CPU Villager が **Wood も継続** 採取
5. CPU **Militia 攻撃波**（30 秒）— 回帰
6. Player **Militia AutoAggro** — 回帰
7. Player 4 資源操作 — 回帰
8. Console エラーなし

Phase 30 のみ実装。**M2.6 Phase 31（Unit Production Queue）** に触れない。  
M2.5 完了後 → [05_M2_6_RTS_UX_PHASES.md](../05_M2_6_RTS_UX_PHASES.md) Phase 31 へ。

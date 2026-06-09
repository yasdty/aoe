# Phase 30 実行プロンプト

> **状態:** ✅ 実装済み（Play 確認待ち）  
> **前提:** Phase 1〜29 完了（PoC + Foundation + M2 Economy + M2.5 Phase 21〜29）  
> **マイルストン:** M2.5 Economy Polish（**最終 Phase**）  
> **ロードマップ:** [04_M2_5_ECONOMY_POLISH_PHASES.md](../04_M2_5_ECONOMY_POLISH_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 30 実装（CPU 4 Resources + AI 調整）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜29 は完了済み。Phase 30 のみ実装すること。**

---

## ① M2.5 Economy Polish 方針（必読・遵守）

Phase 29 で Militia Basic Aggro が完成。Phase 30 は **M2.5 最終** — CPU が 4 資源経済を回し、**経済 AI と軍事 AI が競合しない**ように調整する。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | CPU 4 資源 + **Economy / Military AI の競合解消** |
| **small diff** | `CpuEconomyAiManager` 拡張 + `CpuMilitaryAiManager` **最小調整** + 共有ガード（`CpuAiCoordination.cs` 新規可） |
| **既存パターン再利用** | `GatherManager` / `FoodGatherManager` / `MineralGatherManager` / `BuildingPlacementManager.TryStartTeamConstruction` |
| **既存ゲームを壊さない** | Player 操作 / Militia Aggro / Boar / 羊 / Foundation 全機能 |
| **Foundation 維持** | CPU は **Command 化しない** — Manager 直接 `Issue*` 呼び出し |
| **AI 調整の考え方** | **資源不足時は何もしない（スキップ）** — クラッシュ・例外・無限ループ禁止。たまったら実行 |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- CPU 評価は **`ISimulationTickable`**（`EvaluateInterval = 2f` 維持）
- **`.meta` は 32 文字 GUID**

---

## ② Phase 29 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **Player 経済** — 4 資源 + Camp / Mill Drop-off + 採取リピート
- **CPU 経済** — **Wood のみ**（`AssignIdleVillagersToTrees`）+ House + Villager 6 体目標
- **CPU 軍事** — `CpuMilitaryAiManager` 別 Tick:
  - **60 秒後**から Barracks 建設試行（Wood 50）
  - Barracks 完成後 **Militia 生産**（Wood コスト、目標 8 体）
  - **30 秒ごと** `LaunchAttackWave()` — **既存 Militia 全員**に攻撃命令（**新規スポーンではない**）
- **誤解しやすい点:** 30 秒タイマーは「兵が湧く」ではなく「既存 Militia に再攻撃命令」するだけ。Militia 0 体なら波は **何もしない**

### 現状のギャップ（Phase 30 で解消）

| 項目 | 現状 |
|------|------|
| CPU Food / Gold / Stone | 採取・Camp 建築なし |
| Economy ↔ Military 競合 | **未調整** — Mill 等が Barracks 用 Wood / builder を奪う可能性 |
| 攻撃の見え方 | Wood 不足で Barracks 遅延 → Militia 少ないのに 30 秒タイマーだけ動く（空振り） |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| CPU 経済（拡張） | `Assets/Scripts/AI/CpuEconomyAiManager.cs` |
| CPU 軍事（調整） | `Assets/Scripts/AI/CpuMilitaryAiManager.cs` |
| 木 / Food / 鉱物 | `GatherManager` / `FoodGatherManager` / `MineralGatherManager` |
| Berry 探索 | `BerryBushSpatialIndex.FindNearestAvailable` |
| 建築 | `BuildingPlacementManager` — `HasActiveConstructionForTeam` / `TryStartTeamConstruction` |
| Camp Registry | `LumberCampRegistry` / `MiningCampRegistry` / `MillRegistry` |
| 羊 / 狩り | `SheepResource` / `AnimalDiscoveryManager` |
| HUD | `CpuHudView.cs`（Food/Gold/Stone 表示済み） |

---

## ③ Phase 30 目的 — CPU 全体フロー（経済 + 軍事）

**望ましい CPU プレイの流れ（MVP）:**

```
① Villager が Wood（+ Phase30 で Food/Gold/Stone）をためる
        ↓ 資源足りなければスキップ（バグなし）
② 60 秒経過 & Wood ≥ 50 → Barracks 建設（軍事 AI）
        ↓ Economy 建築はこの間 **譲る**（競合回避）
③ Barracks 完成 → Wood/Food 足りれば Militia 生産（1 体ずつ）
        ↓
④ Militia が 1 体以上いる → 30 秒周期で **全 Militia** 攻撃命令
        ↓ Militia 0 体 → 波はスキップ（タイマーは回るが IssueAttack しない）
⑤ 並行: Barracks 完成後、Wood 余裕があれば Mill / Mining Camp / Farm（経済 AI）
```

**重要:** 攻撃波は **Barracks 生産 Militia のみ**が対象。タイマーでユニットが増えるわけではない。

---

## ④ 競合回避ルール（必須）

`CpuAiCoordination.cs`（static ヘルパー新規 **推奨**）または各 Manager 内ガードで実装。

### A. 建築 — 同時 1 件（既存）

- `BuildingPlacementManager.HasActiveConstructionForTeam(CpuTeam)` が true なら **新規建築開始しない**
- Economy / Military どちらも遵守

### B. 軍事フェーズ中 — Economy 建築を **延期**

```csharp
// Barracks 未完成 かつ ゲーム開始から barracksBuildDelay 経過後 → Economy Camp 建築しない
static bool ShouldDeferEconomyBuildings(UnitTeam cpuTeam)
{
    if (BarracksProductionManager.HasBarracksForTeam(cpuTeam))
        return false;
    // Military が Barracks を建てる猶予期間
    return true;
}
```

- **Barracks 完成後** に `TryBuildEconomyBuildings()` を有効化

### C. Wood 予約 — Barracks 優先

```csharp
static bool HasWoodReserveForBarracks(UnitTeam cpuTeam, float barracksWoodCost, float extraCost = 0f)
{
    if (BarracksProductionManager.HasBarracksForTeam(cpuTeam))
        return true; // 予約不要
    return ResourceManager.GetWood(cpuTeam) >= barracksWoodCost + extraCost;
}
```

- Economy 建築（Mill 100 等）開始前: `HasWoodReserveForBarracks(cpu, 50, millWoodCost)` を満たすこと
- 足りなければ **スキップ**（ログ optional）

### D. Builder（村民）— 建築中は採集割当しない

- `IsIdleForEconomy` に `BuildingPlacementManager.IsUnitBuilding` 済み
- Military / Economy とも **建築中の Villager を builder として二重使用しない**（`TryStartTeamConstruction` 側で既に割当）

### E. 採集 job — Militia 訓練用 Food

- Militia 生産に Food コストがある場合（`Barracks.Data` 確認）、`TryTrainMilitia` は Food 不足で **スキップ**（既存 Wood チェック同型）
- Economy が Food を稼ぐ Phase30 ロジックと **共存**（Food 不足なら Militia 生産待ちで OK）

### F. 攻撃波 — Militia 存在時のみ

`CpuMilitaryAiManager.LaunchAttackWave()` — **small diff 可:**

- Militia 0 体 → **return**（既存 `MinMilitiaForWave`）
- スキップ時 optional ログ: `[CPU Military] Attack wave skipped — no Militia`
- 成功時: 既存どおり **全 Militia** に `AttackManager.IssueAttack`
- **30 秒間隔は維持** — 間隔変更・難易度調整は将来 Phase 可

---

## ⑤ 今回実装するもの

### 1. `CpuAiCoordination.cs`（新規・推奨）

- `ShouldDeferEconomyBuildings(CpuTeam)`
- `HasWoodReserveForBarracks(...)`
- `HasActiveCpuConstruction()` ラッパー
- Economy / Military 両方から参照 — **duplicate 定数**（`BarracksWoodCost = 50`, `BarracksBuildDelay = 60`）は 1 箇所に

### 2. `CpuEconomyAiManager` 拡張

- SerializeField: `millData`, `miningCampData`, `farmData`（`lumberCamp` optional）
- `TryBuildEconomyBuildings()` — **§④ B/C ガード通過後のみ**
- `AssignIdleVillagers()` — 優先度（下表）、Wood は常時フォールバック
- `IsIdleForEconomy` — Food / Mineral gathering 中は false（必要なら `IsUnitGathering` 追加）

**Villager 割当優先度:**

| 優先 | 条件 | アクション |
|------|------|------------|
| 1 | Food < 200 & Berry あり | Berry 採取 |
| 2 | Food < 200 & 発見済み Sheep/Deer | Hunt |
| 3 | Gold < 100 & Mining Camp & Gold Mine | Gold 採掘 |
| 4 | Stone < 100 & Mining Camp & Stone Mine | Stone 採掘 |
| 5 | Food < 200 & CPU Farm 空き | Farm |
| 6 | デフォルト | Wood |

**Economy 建築（Barracks 完成後 & Wood 予約 OK 時）:**

| 建築 | 条件 |
|------|------|
| Mining Camp | (Gold or Stone) 需要 & 未所有 & Wood ≥ 100 + 予約 |
| Mill | Food 需要 & 未所有 & Wood ≥ 100 + 予約 |
| Farm | Food 需要 & Wood ≥ 60 + 予約 |

### 3. `CpuMilitaryAiManager` — 最小調整

- `LaunchAttackWave` — Militia 0 でスキップログ（optional）
- `TryBuildBarracks` / `TryTrainMilitia` — **ロジック維持**。Food 不足ガード追加（必要時）
- **rewrite 禁止** — タイマー・目標 8 体・60 秒 Barracks 開始は維持

### 4. Debug ログ（推奨）

```
[CPU Economy] Mill deferred — Barracks not built yet
[CPU Economy] villager → Berry (Food=120)
[CPU Military] Barracks construction started at 01:00
[CPU Military] Attack wave: 3 Militia → Player Villager
[CPU Military] Attack wave skipped — no Militia
```

### 5. Phase10 SceneBuilder（optional）

- `CpuEconomyAiManager` SerializeField 参照を `CreateManagers` で設定

---

## ⑥ 今回やらないこと

- CPU 羊誘導（`SheepMoveCommand`）— 発見 + 狩りのみ MVP
- 攻撃波間隔・Militia 目標数の **バランス大幅変更**（定数微調整は OK）
- Stand Ground / 本格ビルドオーダー / 難易度段階
- Player UI / Command 変更
- **M2.6 Phase 31** 以降

---

## ⑦ 実装ステップ（推奨）

| Step | 内容 |
|------|------|
| 30-1 | `CpuAiCoordination` — 競合ガード |
| 30-2 | `CpuEconomyAiManager` — IsIdle 拡張 + Economy 建築（ガード付き） |
| 30-3 | `CpuEconomyAiManager` — Villager 4 資源割当 |
| 30-4 | `CpuMilitaryAiManager` — スキップログ + Food ガード（必要なら） |
| 30-5 | Play 確認 + ドキュメント |

---

## ⑧ 技術メモ

### Issue* API（CPU 直接呼び出し — 変更なし）

`GatherManager` / `FoodGatherManager` / `MineralGatherManager` の `Issue*` — Phase 21 採取リピートそのまま CPU にも効く。

### 2 Manager の Tick 関係

| Manager | 周期 | 役割 |
|---------|------|------|
| `CpuEconomyAiManager` | 2 秒 | House / Villager / 4 資源 / Economy 建築 |
| `CpuMilitaryAiManager` | 2 秒評価 + **毎 Tick** waveTimer | Barracks / Militia / 攻撃波 |

両者は **別コンポーネント** — 共有状態は `ResourceManager` / `BuildingPlacementManager` / `CpuAiCoordination` のみ。

### Militia 生産 vs 攻撃波

| 処理 | 実体 |
|------|------|
| Militia 増える | `Barracks.TryQueueMilitiaProduction()` — Wood（+ Food）消費 |
| 30 秒タイマー | 既存 Militia に `IssueAttack` — **増殖しない** |

---

## ⑨ 完了条件（Phase 30 MVP）

- [ ] **競合なし** — Barracks 建設中 / Wood 予約中に Mill 等が builder や Wood を奪わない
- [ ] **CPU 4 資源** — Berry / Hunt / Gold / Stone（+ Wood）が増える
- [ ] **軍事フロー** — Wood ためる → Barracks → Militia 生産 → Militia いれば全員攻撃
- [ ] **資源不足** — スキップのみ（Console エラー・例外なし）
- [ ] **採取リピート** / **Player Aggro** / **Player 4 資源** 回帰
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `04_M2_5` Phase 30 ✅ — **M2.5 完了**

---

## ⑩ テスト手順（Play チェックリスト）

1. `Phase10.unity` → Play
2. **0〜60 秒** — CPU Villager が Wood（+ Food 開始後）を稼ぐ。Mill **建てない**
3. **~60 秒** — Console `[CPU Military] Barracks construction started`
4. Barracks 完成 → Militia 生産開始（Wood/Food 足りるまで待つ — **クラッシュなし**）
5. Militia 1 体以上 → `[CPU Military] Attack wave: N Militia`
6. Militia 0 体の間 → `Attack wave skipped`（または無ログで return）
7. Barracks 完成 **後** — Mining Camp / Mill 建築ログ
8. Player 操作・Aggro 回帰
9. Console エラーなし

Phase 30 のみ実装。**M2.6 Phase 31** に触れない。

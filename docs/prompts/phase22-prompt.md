# Phase 22 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜21 完了（PoC + Foundation + M2 Economy + Phase 21 Gather Repeat）  
> **マイルストン:** M2.5 Economy Polish  
> **ロードマップ:** [04_M2_5_ECONOMY_POLISH_PHASES.md](../04_M2_5_ECONOMY_POLISH_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 22 実装（Farm One-Worker + Spawn Grid）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜21 は完了済み。Phase 22 のみ実装すること。**

---

## ① M2.5 Economy Polish 方針（必読・遵守）

Phase 21 で採取リピートが完成。Phase 22 は **Farm 1 人制限** と **建物スポーン周囲グリッド** の 2 点のみ。

| 方針 | 内容 |
|------|------|
| **1 Phase = 2 目的（関連）** | Farm 占有 + TC/Barracks スポーン配置 |
| **small diff** | `FoodGatherManager` + `TownCenter` + `Barracks`（+ 必要なら共有ヘルパー 1 ファイル） |
| **既存パターン再利用** | `GroupMoveFormation` のグリッド計算 / Farm ジョブ既存構造 |
| **既存ゲームを壊さない** | Phase 21 採取リピート / 4 資源 / 建築・生産・CPU + Foundation 全機能 |
| **Foundation 維持** | Command Queue / Fixed Tick 20 TPS / Pool / Spatial Hash |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- Setup メニューは **Edit モード専用**（本 Phase で Scene 変更は **不要**）
- Farm 枯渇 Tick 安全（Phase 18 バグ再発禁止）

---

## ② Phase 21 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **採取リピート** — Wood / Berry / Farm / Gold / Stone が搬入後自動復帰
- **Farm** — 複数村民が **同一 Farm** に同時採取可能（`UnitPositionOffsets` で並ぶ）
- **スポーン** — TC / Barracks はカメラ方向出口 + **8 スロットリング**（重なりやすい）
- **Command Queue** — 全 Gather 命令 + Cancel 相互排他

### 現状のギャップ（Phase 22 で解消）

| 項目 | 現状 |
|------|------|
| Farm 1 人制限 | **なし** — AoE2 は 1 Farm = 1 村民 |
| スポーン配置 | リング 8 スロットのみ — 連続生産で重なる |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| Farm 採集 | `Assets/Scripts/Economy/FoodGatherManager.cs` — `IssueGatherFarmCommand` / `farmJobs` |
| TC スポーン | `Assets/Scripts/Buildings/TownCenter.cs` — `GetVillagerSpawnPosition` |
| Barracks スポーン | `Assets/Scripts/Buildings/Barracks.cs` — `GetUnitSpawnPosition` |
| グリッド参考 | `Assets/Scripts/Selection/GroupMoveFormation.cs` |
| 生産 | `Assets/Scripts/Buildings/ProductionManager.cs` / `BarracksProductionManager.cs` |

---

## ③ Phase 22 目的

### A. Farm One-Worker（AoE2 準拠）

**1 枚の Farm に同時に働ける村民は 1 人。** 2 人目が occupied Farm を右クリックしても **新ジョブを開始しない**。

- 別 Farm への割当は従来どおり
- Phase 21 採取リピートは **維持**
- 占有中 Farm の `GatherFarmFoodCommand` は **無視**（Move 命令は別途有効）

### B. Building Spawn Grid

TC / Barracks から生産されたユニットが **建物周囲のグリッド** に出現。連続 Q 生産でも重なりにくくする。

- カメラ方向「出口側」をグリッド中心の手前に置く（既存 `ResolveSpawnDirection` 再利用可）
- スロットは建物ごとにインクリメント（リング 8 固定を **グリッド N スロット** に拡張）

### 変更後（Farm）

```
村民 A → Farm X 右クリック → 採取開始（Farm X occupied by A）
村民 B → Farm X 右クリック → 命令無視（B は動かない / 既存命令維持）
村民 B → Farm Y 右クリック → 採取開始（別 Farm は OK）
```

### 変更後（Spawn）

```
TC 連続 Villager 生産
    ↓
建物外周グリッド slot 0, 1, 2, ...（2m 間隔、√n グリッド）
    ↓
slot が尽きたら 0 から再利用（または列数拡張 — small diff で選択可）
```

---

## ④ 今回実装するもの

### Farm One-Worker

1. **`FoodGatherManager.IsFarmOccupiedByOther(Farm farm, Unit requestingUnit)`** — `farmJobs` を走査。Gather 中（`MoveToFarm` / `Gather` / `MoveToDeposit`）の別 Unit がいれば true
2. **`IssueGatherFarmCommand`** — 占有中なら **その Unit をスキップ**（ジョブ追加しない）
3. **`SelectionManager.TryIssueGatherFarmCommand`** — 全選択村民が拒否されたら false（既存フロー）

**占有判定に含める状態:** `MoveToFarm`, `Gather`, `MoveToDeposit`（Deposit 往路も同一 Farm ジョブとして占有）

**占有から除外:** ジョブなし / 別 Farm / 死亡ユニット

### Spawn Grid

4. **共有ヘルパー（推奨）** — `BuildingSpawnFormation.cs` 等、`static Vector3 GetGridSlotPosition(Vector3 buildingCenter, Vector3 forwardDirection, int slotIndex, float spacing = 2f, float clearance = 4f, float groundY = 1f)`
   - `GroupMoveFormation.GetGridDimensions` と同じ √n グリッド
   - グリッド中心 = `buildingCenter + forward * clearance`
   - slotIndex は建物インスタンスごとに保持
5. **`TownCenter.GetVillagerSpawnPosition`** — リング方式をグリッド方式に置換
6. **`Barracks.GetUnitSpawnPosition`** — 同左

**代替（共通化しない場合）:** TC / Barracks に同型コードをコピー（small diff なら可）

### 禁止（Phase 22 範囲外）

- Mining Camp / Mill / 狩り（Phase 23〜26）
- Militia Aggro（Phase 29）
- CPU Farm 建築 AI（Phase 30）
- Berry / Tree / Mine の占有制限
- 移動時ユニット押し出し / RVO
- `Phase10SceneBuilder` 変更
- Gather リピートの変更（Phase 21 維持）

---

## ⑤ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 22-1 | Farm 占有チェック + `IssueGatherFarmCommand` |
| 22-2 | Play — 2 村民同一 Farm 拒否 / 別 Farm OK |
| 22-3 | `BuildingSpawnFormation` + TC グリッド |
| 22-4 | Barracks グリッド + 連続生産 Play |
| 22-5 | 回帰 + ドキュメント更新 |

---

## ⑥ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**
2. **Farm 占有判定ロジック**（どの state を occupied とみなすか）
3. **Spawn グリッド座標計算**（forward / slotIndex）
4. **影響範囲**（SelectionManager / ProductionManager）
5. **リスク**（Farm Deposit 中の占有解放タイミング）
6. **ロールバック方法**
7. **完了条件**
8. **テスト手順**

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### Farm 占有 — `IssueGatherFarmCommand`（例）

```csharp
public static void IssueGatherFarmCommand(IReadOnlyList<Unit> units, Farm farm)
{
    if (instance == null || farm == null || farm.IsDepleted)
        return;

    for (int i = 0; i < units.Count; i++)
    {
        Unit unit = units[i];
        // ... existing guards ...

        if (IsFarmOccupiedByOther(farm, unit))
            continue;

        instance.RemoveJobForUnit(unit);
        instance.farmJobs.Add(...);
    }
}

static bool IsFarmOccupiedByOther(Farm farm, Unit requestingUnit)
{
    for (int i = 0; i < instance.farmJobs.Count; i++)
    {
        FarmGatherJob job = instance.farmJobs[i];
        if (job.farm != farm || job.unit == null || job.unit == requestingUnit)
            continue;
        if (!job.unit.IsAlive)
            continue;
        return true;
    }
    return false;
}
```

- **待機キューは Phase 22 では不要** — 拒否のみ（AoE2 も即時別 Farm へ）
- Farm 枯渇 → `CancelJobsForFarm` / `FinishFarmJobWithoutTarget` で占有解放（既存）

### Spawn Grid — 座標（例）

```csharp
// slot 0 = grid center; slots spread in columns × rows
Vector3 gridCenter = buildingPosition + exitDirection * (halfExtent + clearance);
// reuse GroupMoveFormation-style column/row from slotIndex
float groundY = 1f;
```

- TC / Barracks それぞれ `int spawnSlotIndex` を保持（mod で循環可）
- **カメラ依存の forward** は既存 `ResolveSpawnDirection()` をそのまま使う

### SelectionManager

- `TryIssueGatherFarmCommand` — 1 人でも成功すれば true（複数選択で一部のみ開始は OK）
- 全員拒否時のみ false → 攻撃判定へ fallthrough しないよう注意（既存 Farm 採集優先順位維持）

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`
- **Setup 再実行:** 不要
- Phase 1〜21 Setup メニューは壊さない

---

## ⑨ 完了条件（Phase 22 MVP）

- [x] **Farm** — 2 人目が **同一 Farm** を右クリックしても採取開始しない
- [x] **Farm** — **別 Farm** への同時採取は可能
- [x] **Farm** — Phase 21 **採取リピート** が動作（回帰）
- [x] **TC** — Villager 連続生産（Q）で周囲グリッドに出現、重なりにくい
- [x] **Barracks** — Militia 連続生産で周囲グリッドに出現
- [x] Wood / Berry / Gold / Stone 採取リピート（回帰）
- [ ] Console エラーなし（Play 確認待ち）
- [x] `docs/IMPLEMENTATION_STATUS.md` / `04_M2_5_ECONOMY_POLISH_PHASES.md` Phase 22 を ✅

### Victory 確認について

M2.5 では **毎回 Victory まで確認不要**。

---

## ⑩ テスト手順（Play チェックリスト）

1. `Phase10.unity` → Play
2. **Farm 2 人テスト:** Farm 1 枚 → 村民 A 右クリック → 採取開始 → 村民 B で **同 Farm** 右クリック → **B は採取開始しない**
3. **Farm 2 枚テスト:** Farm 2 枚 → A / B が **別 Farm** → 両方採取 OK
4. **Farm リピート:** A が 1 Farm で複数往復（Phase 21 回帰）
5. **TC スポーン:** Q 連打で Villager 3 体以上 → 建物周囲に散開
6. **Barracks スポーン:** Militia 3 体以上連続生産 → 周囲グリッド
7. 木 / Berry / Gold / Stone 採取リピート（回帰）
8. Console エラーなし

Phase 22 のみ実装。**Phase 23 以降（M2.5）** に触れない。

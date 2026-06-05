# Phase 16 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜15 完了（PoC + Foundation Phase 11〜15）  
> **ロードマップ:** [FOUNDATION_PHASES.md](../FOUNDATION_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/RTS_IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 16 実装（Command Queue）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜15 は完了済み。Phase 16 のみ実装すること。**

---

## ① Foundation 方針（必読・遵守）

[FOUNDATION_PHASES.md](../FOUNDATION_PHASES.md) の最重要方針を厳守:

- **AoE 機能を増やさない** — Archer / Food / Age Up 等は禁止
- **small diff** — 1 Phase = 1 目的（Command Queue のみ）
- **既存ゲームを壊さない** — 完了時 `Phase10.unity` でコアループ + **Victory / Defeat** + Pool + Spatial Hash + Fixed Tick が動くこと
- **Simulation 優先** — Replay / Lockstep 準備。見た目 polish 禁止

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止
- Manager 更新方式を維持（**Manager の Tick ロジック自体は rewrite しない** — Command から既存 static API を呼ぶ）
- **Unity アセット手書き禁止**: Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- Setup メニューは **Edit モード専用**
- **`GetInstanceID()` 禁止**

---

## ② Phase 15 完了状態（現状）

`Phase10.unity` / `Benchmark.unity` で動作確認済み:

- **Victory / Defeat** — TC 破壊で終了 UI
- **Object Pool** — `UnitPool` / `BuildingPool`
- **Benchmark** — FPS / FrameTime / GC HUD
- **Spatial Hash** — `UnitSpatialIndex` / `TreeSpatialIndex`
- **Fixed Tick** — `SimulationTick`（20 TPS）+ `ISimulationTickable`、8 Manager が Tick 駆動
- **Phase 10 コアループ** — 採集・建築・生産・CPU 経済 / 軍事 AI

### 現状のギャップ（Phase 16 で解消）

| 項目 | 現状 |
|------|------|
| プレイヤー入力 | **Input / View → Manager を直接呼ぶ**（即時実行） |
| コマンド抽象 | **なし** — Replay 記録不可 |
| Tick との関係 | 入力は可変フレーム、Simulation は Fixed Tick。**同一 Tick 内の命令順序が不定** |
| CPU AI | `GatherManager` / `AttackManager` / `ProductionManager` を **直接呼ぶ**（Phase 16 では **触らない**） |

**プレイヤー命令の入口（実装前に必ず開いて読む）:**

| 入口 | 直接呼び出し先 |
|------|----------------|
| `SelectionManager.HandleMoveCommand` | `GatherManager.IssueGatherCommand`, `AttackManager.IssueAttack`, `GroupMoveFormation.AssignMoveTargets`, 各 `Cancel` / `AbortConstructionForUnits` |
| `SelectionManager`（建築モード） | `BuildingPlacementManager.TryConfirmPlacement`, `CancelPlacementMode` |
| `ResourceHudView` | `BuildingPlacementManager.EnterHousePlacementMode`, `EnterBarracksPlacementMode` |
| `ProductionPanelView` | `TownCenter.TryQueueVillagerProduction` → `ProductionManager.TryQueueProduction` |
| `BarracksPanelView` | `Barracks.TryQueueMilitiaProduction` → `BarracksProductionManager` |

**非 Command 化（現状維持）:**

- **選択** — クリック / 矩形選択（`SelectionManager` の選択状態更新）
- **カメラ** — `RTSCameraController`
- **建築ゴースト追従** — `BuildingPlacementManager.Update`（プレビュー表示）
- **HUD 表示** — OnGUI パネル（ボタン押下 → **Command を Enqueue するだけ**に変更）
- **CPU AI** — 直接 Manager 呼び出しのまま（将来 Phase で Command 化）

主要ファイル:

- `Assets/Scripts/Selection/SelectionManager.cs`
- `Assets/Scripts/Selection/ResourceHudView.cs`
- `Assets/Scripts/Selection/ProductionPanelView.cs`
- `Assets/Scripts/Selection/BarracksPanelView.cs`
- `Assets/Scripts/Selection/GroupMoveFormation.cs`
- `Assets/Scripts/Core/SimulationTick.cs`
- `Assets/Scripts/Combat/AttackManager.cs`（`IssueAttack` / `CancelForUnits`）
- `Assets/Scripts/Economy/GatherManager.cs`（`IssueGatherCommand` / `CancelForUnits`）
- `Assets/Scripts/Buildings/BuildingPlacementManager.cs`
- `Assets/Scripts/Buildings/ProductionManager.cs` / `BarracksProductionManager.cs`
- `Assets/Scripts/Editor/Phase10SceneBuilder.cs`

---

## ③ Phase 16 目的

**プレイヤー操作を Command 経由に統一** — Input と Simulation の間にキューを挟み、Replay / Lockstep の土台を作る。

### 変更後の流れ

```
Input / HUD（可変フレーム）
    ↓ Enqueue（即時実行しない）
CommandQueue
    ↓ 各 Simulation Tick の先頭で Drain & Execute
既存 Manager static API（IssueAttack / IssueGather / TryQueueProduction 等）
    ↓
Fixed Tick Simulation（Phase 15 既存）
```

### 今回実装するもの

1. **`IGameCommand`**（または `GameCommandBase`）— `Execute()` を持つ命令インターフェース
2. **具体 Command クラス** — Move / Attack / Gather / Build / Train（下表）
3. **`CommandQueue`** — Enqueue + Tick 先頭で Execute、`ISimulationTickable` 登録
4. **入口の置き換え** — 上記 5 入口が **Manager を直接呼ばず** `CommandQueue.Enqueue(...)` のみ
5. **`CommandLog`（MVP 軽量版）** — `SimulationTick.CurrentTick` 付きで記録（**再生は Phase 16 範囲外**）
6. **`Phase10SceneBuilder`** — Systems に `CommandQueue` 追加
7. **README / `docs/FOUNDATION_PHASES.md` 更新**

### Command 種別（MVP — 必須）

| Command | トリガー | Execute 内で呼ぶ既存 API（例） |
|---------|----------|-------------------------------|
| `MoveCommand` | 地面右クリック | `AbortConstruction` / `Cancel` 系 → `GroupMoveFormation.AssignMoveTargets` |
| `AttackUnitCommand` | 敵 Unit 右クリック | 上記 Cancel → `AttackManager.IssueAttack(units, targetUnit)` |
| `AttackBuildingCommand` | 敵 Building 右クリック | 上記 Cancel → `AttackManager.IssueAttack(units, buildingHealth)` |
| `GatherCommand` | 木右クリック | 上記 Cancel → `GatherManager.IssueGatherCommand(units, tree)` |
| `BuildConfirmCommand` | 建築モードで地面クリック | `BuildingPlacementManager.TryConfirmPlacement(builders)` |
| `TrainVillagerCommand` | Q / TC パネル | `TownCenter.TryQueueVillagerProduction()` |
| `TrainMilitiaCommand` | Barracks パネル | `Barracks.TryQueueMilitiaProduction()` |

**建築モード開始**（`EnterHousePlacementMode` / `EnterBarracksPlacementMode`）は **UI 状態** のため Command 化 **不要**（ゴースト追従は `Update` のまま）。**確定クリックのみ** `BuildConfirmCommand`。

**Cancel 系**（建築 Esc / 右クリックキャンセル）は UI 即時反応のため **直接 `CancelPlacementMode` 可**（Simulation 命令ではない）。

### 設計指針（MVP）

| 項目 | 推奨 |
|------|------|
| 実行タイミング | **各 Tick の最初** — `CommandQueue` を Tickable リストの **先頭**（Register 順を SceneBuilder で制御、または `SimulationTick` に Priority なしなら CommandQueue を最初に Add） |
| Enqueue | Input フレーム中は **キューに積むだけ**。同一フレーム複数 Enqueue → **同一 Tick で FIFO 実行** |
| 参照 | **GameObject / Component 参照のまま可**（Entity ID 化は Phase 17 以降） |
| ログ | `List<CommandRecord>` — `{ tick, commandType, team }` 程度。Debug.Log 1 行オプション可 |
| Game Over | Enqueue 拒否 or Execute  no-op（`GameSessionManager.IsGameOver`） |
| CPU | **変更しない** — プレイヤー操作のみ Command 化 |

### 禁止（Phase 16 範囲外）

- Replay **再生** / ファイル保存
- ネットワーク / Lockstep 同期
- Entity ID 全面移行
- CPU AI の Command 化
- 新ユニット種別・新資源
- Manager の Tick ロジック rewrite
- Selection / Camera の Command 化

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 16-1 | `IGameCommand` + `CommandQueue`（Enqueue / TickSimulation で Drain） |
| 16-2 | `MoveCommand` + `SelectionManager.HandleMoveCommand` 置き換え |
| 16-3 | `AttackUnitCommand` / `AttackBuildingCommand` / `GatherCommand` |
| 16-4 | `BuildConfirmCommand` + `TrainVillagerCommand` / `TrainMilitiaCommand` |
| 16-5 | `CommandLog` + SceneBuilder + Phase10 回帰 + ドキュメント |

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**（新規 / 変更 / 削除）
2. **新規追加ファイル一覧**
3. **Command クラス一覧と Execute 内の Manager 呼び出し対応表**
4. **CommandQueue の Tick 実行順**（他 Tickable より先か後か — **推奨: 先**）
5. **影響範囲**（CPU AI 非対象、Selection 非対象の理由）
6. **リスク**（1 Tick 遅延の体感 / null 参照（選択ユニット死亡）/ 同一 Tick 命令順）
7. **ロールバック方法**
8. **完了条件**（下記チェックリスト）
9. **テスト手順**

---

## ⑥ 実装後に必ず出力

1. **変更内容サマリ**
2. **変更ファイル一覧**
3. **Command 種別一覧と Enqueue 元**
4. **テスト結果**（Phase10 コアループ + Victory）
5. **既知の制限**（CPU 直接呼び出し / Replay 再生未実装 / Entity ID 未導入 等）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### IGameCommand 例

```csharp
public interface IGameCommand
{
    void Execute();
    string DebugName { get; }
}
```

### CommandQueue 例

```csharp
public class CommandQueue : MonoBehaviour, ISimulationTickable
{
    readonly Queue<IGameCommand> pending = new Queue<IGameCommand>();

    public static void Enqueue(IGameCommand command)
    {
        if (GameSessionManager.IsGameOver || command == null)
            return;
        instance?.pending.Enqueue(command);
    }

    public void TickSimulation(float fixedDeltaTime)
    {
        while (pending.Count > 0)
        {
            IGameCommand cmd = pending.Dequeue();
            cmd.Execute();
            CommandLog.Record(SimulationTick.CurrentTick, cmd);
        }
    }
}
```

- **注意:** 上記は 1 Tick 内でキューを空にする。命令が Tick 中にさらに Enqueue する場合は **再入防止**（`while` の上限 or スナップショット Drain）を検討。

### MoveCommand 例

```csharp
public sealed class MoveCommand : IGameCommand
{
    readonly List<Unit> units;
    readonly Vector3 destination;
    readonly float spacing;

    public void Execute()
    {
        BuildingPlacementManager.AbortConstructionForUnits(units);
        GatherManager.CancelForUnits(units);
        AttackManager.CancelForUnits(units);
        GroupMoveFormation.AssignMoveTargets(units, destination, spacing);
    }
}
```

- Enqueue 時点で `List<Unit>` を **コピー** して死亡ユニットを Execute 時に skip（既存 Manager と同様 null チェック）。

### SelectionManager 置き換えパターン

```csharp
// Before
GatherManager.IssueGatherCommand(selectedUnits, tree);

// After
CommandQueue.Enqueue(new GatherCommand(CopySelectedUnits(), tree));
```

- `HandleMoveCommand` 内の Raycast / 分岐は **Input 側に残す**（ワールド座標・ターゲット解決は可変フレームで OK）。**Simulation 副作用だけ Command 化**。

### SceneBuilder

```csharp
GameObject commandQueueObject = new GameObject("CommandQueue");
commandQueueObject.transform.SetParent(systems.transform);
commandQueueObject.AddComponent<CommandQueue>();
// SimulationTick の直後に配置（Awake/Start 順で Tick 先頭実行を意図）
```

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`（主）+ `Benchmark.unity`（回帰 — Command 経由の変更は Phase10 中心）
- **`AoE → Setup Phase10 Scene`** — `CommandQueue` 追加
- Phase 1〜10 Setup メニューは壊さない

---

## ⑨ 完了条件（Phase 16 MVP）

- [ ] `CommandQueue` が Systems に存在し、**各 Simulation Tick 先頭**でキューを消化
- [ ] **プレイヤー操作 7 種**（Move / Attack×2 / Gather / Build 確定 / Train×2）が **Manager 直接呼び出しなし**（Cancel 系 UI を除く）
- [ ] `CommandLog`（または同等）に Tick 番号付きで記録される
- [ ] **CPU AI** は従来どおり直接 Manager 呼び出し（変更なし）
- [ ] **Selection / Camera / 建築ゴースト** は可変フレームのまま
- [ ] **Phase10** — 採集・建築・生産・CPU 攻撃波・Victory / Defeat が **Phase 15 同様に動作**
- [ ] Console に **Null 参照・例外なし**
- [ ] `docs/FOUNDATION_PHASES.md` Phase 16 を ✅ に更新

---

## ⑩ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene` → Play
2. 村民選択 → 地面右クリック **移動**
3. 木右クリック **採集** — Wood 増加
4. HUD **Build House** → 配置クリック **建築** 完成
5. **Build Barracks** → Militia 生産
6. TownCenter 選択 → **Q** またはボタンで村民生産
7. Militia 選択 → 敵 Unit / 敵 TC **攻撃**
8. 敵 TC 破壊 → **VICTORY**
9. Console に Command ログ（実装した場合）が Tick 付きで出ること — **エラーなし**
10. 操作に **1 Tick（0.05s）程度の遅延** があっても gameplay が破綻しないこと

Phase 16 のみ実装。Phase 17 以降（Food / Farm 等）に触れない。

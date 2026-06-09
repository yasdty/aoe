# Phase 33 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜32 完了（PoC + Foundation + M2 Economy + M2.5 + M2.6 Phase 31〜32）  
> **マイルストン:** M2.6 RTS UX（**第 3 Phase**）  
> **ロードマップ:** [05_M2_6_RTS_UX_PHASES.md](../05_M2_6_RTS_UX_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 33 実装（Rally Point）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜32 は完了済み。Phase 33 のみ実装すること。**

---

## ① M2.6 RTS UX 方針（必読・遵守）

Phase 32 で Idle Unit UX が完成。Phase 33 は **生産建物の Rally Point（集合地点）** — 生まれたユニットが自動で行き先へ。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | TC / Barracks の Rally 設定 + **Spawn 直後に命令適用** |
| **small diff** | `TownCenter` / `Barracks` 拡張 + `SelectionManager` 右クリック分岐 + Spawn フック — rewrite 禁止 |
| **既存パターン再利用** | `MoveCommand` / `GatherCommand` / `GatherManager.IssueGatherCommand` 等 |
| **既存ゲームを壊さない** | 生産キュー / Idle UX / 採集 / Aggro / CPU AI / Victory |
| **Player のみ（MVP）** | Rally 設定 UI は **Player TC / Barracks** のみ（CPU Rally は Phase 33 対象外） |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- Player 操作は **CommandQueue 経由**（Rally 設定も Command 化推奨）
- **`.meta` は 32 文字 GUID**

---

## ② Phase 32 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **生産** — TC / Barracks FIFO キュー（最大 15）。完了時 `UnitSpawner.Spawn(...)` — **Rally 適用なし**
- **Spawn** — `ProductionManager` / `BarracksProductionManager` が `UnitSpawner.Spawn` を直接呼ぶ。戻り値 `Unit` は **未使用**
- **建物選択** — `SelectionManager` — `selectedTownCenter` / `selectedBarracks`。ユニット未選択時
- **右クリック** — `HandleMoveCommand()` — `selectedUnits.Count == 0` なら **Sheep 誘導のみ**、それ以外は return。**建物 Rally 未対応**
- **ユニット右クリック** — 採集 / 狩り / 攻撃 / 地面 Move（既存 Command 経路）

### 現状のギャップ（Phase 33 で解消）

| 項目 | 現状 |
|------|------|
| Rally データ | TC / Barracks に **行き先なし** |
| Rally 設定操作 | 建物選択 + 右クリック → **何も起きない** |
| Spawn 後自動移動 | **なし** — スポーン地点で待機 |
| TC Rally → 資源採集 | **なし** |
| Rally ビジュアル | **なし** |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| 生産完了 | `ProductionManager.cs` / `BarracksProductionManager.cs` |
| Spawn | `UnitSpawner.cs` / `UnitPool.Rent` |
| 建物 | `TownCenter.cs` / `Barracks.cs` |
| 選択・右クリック | `SelectionManager.cs` — `HandleMoveCommand` / `SetTownCenterSelection` |
| 命令 | `GameCommands.cs` — `MoveCommand` / `GatherCommand` 等 |
| 採集 API | `GatherManager` / `FoodGatherManager` / `MineralGatherManager` — `Issue*` |
| レイヤー | `GameLayers` — Ground / Resource / Building |
| Editor | `Phase10SceneBuilder.cs` |

---

## ③ Phase 33 目的 — Rally Point（MVP）

**建物選択 → 右クリックで Rally 設定 → 生産ユニットが Spawn 直後に自動でその先へ**

### MVP 挙動

```
Player が TC 選択（ユニット未選択）
    ↓ 地面右クリック
Rally Point を地面座標に保存
    ↓
Villager 生産完了 → Spawn
    ↓
SetMoveTarget(rallyPoint)  // または GatherCommand
```

Barracks + Militia も **地面 Rally → Move** が MVP。

### Rally 種別（MVP）

| Rally 先 | TC Villager | Barracks Militia |
|----------|-------------|------------------|
| **地面** | ✅ `SetMoveTarget` | ✅ `SetMoveTarget` |
| **木** | ✅ `GatherCommand`（1 体） | ❌ — 地面のみ |
| **Berry / Farm / 狩り** | ✅ 既存 Food 系 Command | ❌ |
| **Gold / Stone** | ✅ 既存 Mineral 系 Command | ❌ |
| **敵 Unit / Building** | ❌ | ❌（Phase 33 対象外） |

**TC + 資源 Rally** は AoE2 準拠の **推奨サブステップ**。時間がなければ **地面 Rally のみ** でも Phase 33 MVP 可（ドキュメントに明記）。

---

## ④ 今回実装するもの

### 1. Rally データ — `ProductionRallyPoint.cs`（新規 struct 推奨）または建物フィールド

```csharp
enum RallyTargetKind { None, Ground, Tree, BerryBush, Farm, GoldMine, StoneMine, /* Hunt optional */ }

struct ProductionRallyPoint
{
    public RallyTargetKind kind;
    public Vector3 groundPoint;      // Ground 用 + ビジュアル位置
    public Component resourceTarget; // TreeResource 等（null 可）
}
```

- **`TownCenter`** / **`Barracks`** — `ProductionRallyPoint rally` フィールド + `SetRally` / `ClearRally` / `HasRally` / `Rally` getter
- 初期値 `kind = None`

### 2. `SetRallyPointCommand`（新規・推奨）

```csharp
public sealed class SetRallyPointCommand : IGameCommand
{
    // TownCenter or Barracks + ProductionRallyPoint
}
```

- `CommandQueue.Enqueue` — Player 操作の Foundation 整合

### 3. `SelectionManager` — 建物選択中の右クリック

`HandleMoveCommand()` 先頭付近 — **`selectedUnits.Count == 0`** 分岐を拡張:

```csharp
if (selectedUnits.Count == 0)
{
    if (TrySetProductionRallyFromClick())
        return;
    TryIssueSheepMoveCommand();
    return;
}
```

**`TrySetProductionRallyFromClick()` ロジック:**

1. `selectedTownCenter != null`（Player）→ Rally 対象 TC
2. else `selectedBarracks != null`（Player）→ Rally 対象 Barracks
3. else return false
4. Raycast — **ResourceMask 優先** → 資源種別を判定して `ProductionRallyPoint` 構築
   - Barracks 選択時は **Resource は無視** し Ground のみ（または Resource クリックも Ground 座標として扱う）
5. なければ **GroundMask** → `groundPoint = hit.point`
6. `CommandQueue.Enqueue(new SetRallyPointCommand(...))`
7. return true

**優先度:** 建物 Rally > Sheep 誘導（TC/Barracks 選択中）

### 4. `ProductionRallyApplier.cs`（新規 static 推奨）

Spawn 直後に呼ぶ:

```csharp
public static void Apply(TownCenter building, Unit spawnedUnit);
public static void Apply(Barracks building, Unit spawnedUnit);
```

| Rally kind | Villager | Militia |
|------------|----------|---------|
| None | 何もしない | 何もしない |
| Ground | `spawnedUnit.SetMoveTarget(groundPoint)` | 同上 |
| Tree | `GatherManager.IssueGatherCommand([unit], tree)` | Move to ground fallback |
| Berry | `FoodGatherManager.IssueGatherCommand` | — |
| Gold / Stone | `MineralGatherManager.IssueGather*` | — |
| Farm | `FoodGatherManager.IssueGatherFarmCommand` | — |

- Militia + 資源 Rally → **Ground 座標へ Move**（Barracks は戦闘ユニット想定）

### 5. 生産 Manager — Spawn 後 Apply

**`ProductionManager`** — Spawn 後:

```csharp
Unit unit = UnitSpawner.Spawn(...);
ProductionRallyApplier.Apply(job.townCenter, unit);
```

**`BarracksProductionManager`** — 同様。

### 6. Rally ビジュアル（任意・推奨）

- **`RallyPointMarkerView.cs`** — 建物ごとに **小さな LineRenderer / シリンダー** で Rally 地点を表示
- TC / Barracks の `SetRally` 時に更新、`ClearRally` / 建物破壊で非表示
- **MVP 省略可** — 地面にユニットが向かえば OK

### 7. Info Panel / HUD（任意）

- `SelectionInfoPanelView` — TC / Barracks 選択時 `Rally: Set` / `Rally: None` 1 行追加（small diff なら推奨）

---

## ⑤ 今回やらないこと

- CPU 建物 Rally
- 複数 TC / Barracks 選択時の一括 Rally
- Rally 先が敵ユニット / 建物（攻撃 Rally）
- 狩り（Deer / Sheep / Boar）Rally — **optional**、時間あれば
- Control Groups（**Phase 34**）
- Phase10 マップ拡張
- `UnitSpawner` / `UnitPool` の rewrite

---

## ⑥ 実装ステップ（推奨）

| Step | 内容 |
|------|------|
| 33-1 | `ProductionRallyPoint` + TC / Barracks フィールド + Set/Clear API |
| 33-2 | `SetRallyPointCommand` + `ProductionRallyApplier` |
| 33-3 | `SelectionManager.TrySetProductionRallyFromClick` |
| 33-4 | `ProductionManager` / `BarracksProductionManager` Spawn フック |
| 33-5 | Rally ビジュアル or Info Panel（任意） |
| 33-6 | Play 確認 + ドキュメント |

---

## ⑦ 技術メモ

### HandleMoveCommand との関係

- **ユニット選択あり** — 既存どおり（Rally 変更なし）
- **建物のみ選択** — Phase 33 で Rally 設定
- **Sheep 資源選択 + 右クリック** — 既存 `TryIssueSheepMoveCommand`（建物未選択時）

### Spawn タイミング

- `UnitSpawner.Spawn` は **`Unit` を返す** — Apply は **同 Tick 内** Spawn 直後で OK
- `SetMoveTarget` は `Unit.TickMovement` で次 Tick から移動開始

### Gather Rally — 1 体バッファ

既存 Manager は `List<Unit>` を要求:

```csharp
static readonly List<Unit> rallyBuffer = new List<Unit>(1);
rallyBuffer.Clear();
rallyBuffer.Add(spawnedUnit);
GatherManager.IssueGatherCommand(rallyBuffer, tree);
```

### Ground Rally 座標

- `hit.point` の y は地面高さ — `SetMoveTarget` にそのまま渡す（既存 MoveCommand 同型）

---

## ⑧ 完了条件（Phase 33 MVP）

- [ ] TC 選択 → 地面右クリック → Rally 設定（Console ログ or ビジュアルで確認可）
- [ ] Barracks 選択 → 地面右クリック → Rally 設定
- [ ] Villager 生産完了 → **自動で Rally 地点へ移動**
- [ ] Militia 生産完了 → **自動で Rally 地点へ移動**
- [ ] TC 選択 → **木** 右クリック Rally → 生産 Villager が **自動採集開始**（サブステップ — 地面のみでも MVP 可）
- [ ] Rally 未設定 → Spawn 後 **従来どおりスポーン地点待機**
- [ ] 生産キュー / Idle UX / 採集 / Aggro / CPU / Victory **回帰**
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `05_M2_6` Phase 33 ✅

---

## ⑨ テスト手順（Play チェックリスト）

1. `Phase10.unity` → Play
2. TC 選択（村民未選択）→ 木の近くの **地面** を右クリック
3. Q で Villager 生産 → Spawn 後 **Rally 方向へ移動**
4. TC 選択 → **木** を右クリック（Rally 採集）→ Villager Spawn → **自動で木へ**
5. Barracks 建設 → 選択 → 前方地面 Rally → Militia 生産 → **自動移動**
6. Rally 設定前の Villager — **スポーン地点で待機**（回帰）
7. ユニット選択中の右クリック Move / Gather — **従来どおり**
8. Idle `.` / 生産 Q キュー — **回帰**
9. Console エラーなし

Phase 33 のみ実装。**Phase 34 / マップ拡張** に触れない。

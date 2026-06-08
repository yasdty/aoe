# Phase 17 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜16 完了（PoC + Foundation）  
> **マイルストン:** M2 Economy 開始  
> **ロードマップ:** `03_M2_ECONOMY_PHASES.md`（未作成の場合は本プロンプトを正とする）  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 17 実装（Food 資源）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜16 は完了済み。Phase 17 のみ実装すること。**

---

## ① M2 Economy 方針（必読・遵守）

Foundation（Phase 11〜16）完了後、**AoE 経済機能の段階的追加**に入る。Phase 17 は **Food のみ**。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | Food 資源 + 採集ループのみ |
| **small diff** | `GatherManager` の rewrite 禁止。Wood 用はそのまま維持 |
| **既存ゲームを壊さない** | 完了時 `Phase10.unity` で Wood 採集・建築・生産・戦闘・CPU・Victory / Defeat + Foundation 全機能が動くこと |
| **Foundation 維持** | Command Queue / Fixed Tick 20 TPS / Pool / Spatial Hash を壊さない |
| **Simulation 優先** | 見た目 polish・Animator 禁止 |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止
- Manager 更新方式を維持（`FoodGatherManager` も `ISimulationTickable`）
- **Unity アセット手書き禁止**: Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- Setup メニューは **Edit モード専用**
- **OnGUI は左上原点** — `GameUiInput.GuiRectToScreenRect`
- **`GetInstanceID()` 禁止**

---

## ② Phase 16 完了状態（現状）

`Phase10.unity` / `Benchmark.unity` で動作確認済み:

- **Victory / Defeat** — TC 破壊で終了 UI
- **Object Pool** — `UnitPool` / `BuildingPool`
- **Benchmark** — FPS / FrameTime / GC HUD
- **Spatial Hash** — `UnitSpatialIndex` / `TreeSpatialIndex`
- **Fixed Tick** — `SimulationTick`（20 TPS）+ `ISimulationTickable`
- **Command Queue** — プレイヤー操作 7 種（Move / Attack×2 / Gather / Build / Train×2）
- **経済（Wood のみ）** — `TreeResource` → `GatherManager` → `ResourceManager.AddWood(team)`
- **Phase 10 コアループ** — 採集・建築・生産・CPU 経済 / 軍事 AI

### 現状のギャップ（Phase 17 で解消）

| 項目 | 現状 |
|------|------|
| 資源種別 | **Wood のみ** — `ResourceManager` は `playerWood` / `enemyWood` のみ |
| Food 採集 | **なし** |
| HUD | **Wood + Pop** のみ（Food 表示なし） |
| 採集ノード | `TreeResource` のみ（Resource レイヤー） |
| Villager 生産コスト | **Food コストなし**（Pop cap のみ） |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| 資源 | `Assets/Scripts/Economy/ResourceManager.cs` |
| Wood 採集 | `Assets/Scripts/Economy/GatherManager.cs` |
| 木ノード | `Assets/Scripts/Economy/TreeResource.cs` / `ResourceNodeData.cs` |
| 命令 | `Assets/Scripts/Commands/GameCommands.cs` / `CommandQueue.cs` |
| 入力 | `Assets/Scripts/Selection/SelectionManager.cs` |
| HUD | `Assets/Scripts/Selection/ResourceHudView.cs` / `CpuHudView.cs` |
| Spatial | `Assets/Scripts/Spatial/TreeSpatialIndex.cs` |
| CPU 経済 | `Assets/Scripts/AI/CpuEconomyAiManager.cs` |
| 生産 | `Assets/Scripts/Buildings/ProductionManager.cs` / `BuildingData.cs` / `TownCenter.cs` |
| Editor | `Assets/Scripts/Editor/Phase10SceneBuilder.cs` / `Phase1SceneBuilder.cs` |
| Visual | `Assets/Scripts/Editor/EntityVisualBuilder.cs`（存在すれば） |

---

## ③ Phase 17 目的

**Food 資源の導入** — AoE2 の Dark Age 初期と同様、**Berry Bush（野生の食料）** から Food を採集し TownCenter に搬入する。

Farm 建築・継続生産は **Phase 18**。今回は **マップ上の採集可能ノード** のみ。

### 変更後の経済ループ（追加部分）

```
Berry Bush 右クリック
    ↓ CommandQueue.Enqueue(GatherFoodCommand)
FoodGatherManager（Fixed Tick）
    MoveToBush → Gather → MoveToDeposit → ResourceManager.AddFood(team)
    ↓
HUD: Food: N 表示
```

Wood 採集ループは **一切変更しない**（`GatherManager` / `GatherCommand` 維持）。

### 今回実装するもの

1. **`ResourceManager` 拡張** — チーム別 Food（`GetFood` / `AddFood` / `TrySpendFood`）。Wood API は維持
2. **`FoodNodeData`**（ScriptableObject）— `initialFood`、色、表示名（Editor `AssetDatabase.CreateAsset`）
3. **`BerryBushResource`** — `TreeResource` と同パターン（`TakeFood` / `IsDepleted` / Resource レイヤー Collider）
4. **`FoodGatherManager`** — `GatherManager` と **並列**（`ISimulationTickable`）。状態機械: MoveToBush → Gather → MoveToDeposit
5. **`GatherFoodCommand`** — `IGameCommand`。Execute 内で `FoodGatherManager.IssueGatherCommand`
6. **`SelectionManager` 拡張** — Resource レイヤー Raycast で `BerryBushResource` を検出 → `GatherFoodCommand` を Enqueue（**木より先 or 後を統一**。同一 Collider に両方付けない）
7. **`MoveCommand` 等の Cancel 拡張** — `FoodGatherManager.CancelForUnits` を Move / Attack Command の Execute に追加
8. **HUD** — `ResourceHudView` に `Food: N` 行追加。`CpuHudView` に `CPU Food: N` 追加
9. **`BerryBushSpatialIndex`** — `TreeSpatialIndex` と同パターン（CPU AI 用。MVP で CPU 採集は **任意** — 時間があれば実装）
10. **Villager 生産の Food コスト** — `BuildingData.villagerFoodCost`（推奨 **50**）+ `ProductionManager.TryQueueProduction` で `TrySpendFood`。**開始 Food**（推奨 Player/CPU 各 **200**）を SceneBuilder または `ResourceManager` 初期化で設定
11. **生産 UI** — TC パネルに Food 不足時は生産不可（既存 Pop cap チェックと同様）
12. **`Phase10SceneBuilder` 更新** — Berry Bush 数本配置（Player / CPU 陣営付近）、`FoodGatherManager` / `BerryBushSpatialIndex` を Systems に追加、`FoodNodeData` アセット生成
13. **README / `docs/IMPLEMENTATION_STATUS.md` 更新** — Phase 17 完了を明記

### Berry Bush 採集パラメータ（MVP — Inspector / Data で調整可）

| 項目 | 推奨値 | 備考 |
|------|--------|------|
| 初期 Food 量 | **250** / Bush | AoE2 ベリー bush 相当 |
| 搬送上限 | **10** | Wood と同じ |
| 採集速度 | **2.5 Food / 秒** | Wood と同じ |
| Deposit 先 | **TownCenter** | チーム別 TC |
| 開始 Food | **200** / チーム | Villager コスト用 |
| Villager Food コスト | **50** | `BuildingData` フィールド |

### 禁止（Phase 17 範囲外）

- **Farm** 建築・継続 Food 生産（Phase 18）
- **Lumber Camp** / **Gold** / **Stone**（Phase 19〜20）
- Archer / Militia Food コスト変更（Wood のみ維持）
- House / Barracks の Food コスト
- `GatherManager` の rewrite / 汎用化リファクタ
- 狩り（Deer / Boar）
- 漁業・市場
- CPU AI の全面書き換え
- 新シーン `Phase17.unity`（**検証は `Phase10.unity` のみ**）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 17-1 | `ResourceManager` Food API + `FoodNodeData` + Editor で DefaultBerryBushData 生成 |
| 17-2 | `BerryBushResource` + `Phase1SceneBuilder.CreateBerryBush`（Editor API、Primitive or EntityVisualBuilder） |
| 17-3 | `FoodGatherManager`（Tick 駆動、Deposit で `AddFood`） |
| 17-4 | `GatherFoodCommand` + `SelectionManager` + `MoveCommand` Cancel 拡張 |
| 17-5 | HUD（Food 行）+ Villager Food コスト + 開始 Food |
| 17-6 | `BerryBushSpatialIndex` + CPU ベリー採集（任意） |
| 17-7 | `Phase10SceneBuilder` + Play 回帰 + ドキュメント |

各サブステップ後に **Wood 採集が壊れていないこと** を確認すること。

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**（新規 / 変更 / 削除）
2. **新規追加ファイル一覧**
3. **Wood 採集との分離方針**（`FoodGatherManager` 並列の理由）
4. **Resource レイヤー Raycast 優先順位**（Tree vs BerryBush の判定）
5. **影響範囲**（Command Queue / CPU AI / 生産コスト）
6. **リスク**（2 系統 Gather の Cancel 漏れ / 同一 Villager が Wood+Food 同時ジョブ / HUD レイアウト）
7. **ロールバック方法**
8. **完了条件**（下記チェックリスト）
9. **テスト手順**

---

## ⑥ 実装後に必ず出力

1. **変更内容サマリ**
2. **変更ファイル一覧**
3. **Food 関連 API 一覧**
4. **テスト結果**（Phase10 コアループ + Wood 回帰 + Victory）
5. **既知の制限**（Farm 未実装 / CPU ベリー未対応 等）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### ResourceManager 拡張例

```csharp
public static float GetFood(UnitTeam team) { ... }
public static void AddFood(UnitTeam team, float amount) { ... }
public static bool TrySpendFood(UnitTeam team, float amount) { ... }
```

- Wood の既存 API（`Wood` プロパティ、`AddWood` 等）は **シグネチャ変更しない**

### FoodGatherManager 構造（GatherManager コピー可）

- `GatherJob` の `tree` → `BerryBushResource bush`、`carriedWood` → `carriedFood`
- Deposit: `ResourceManager.AddFood(job.unit.Team, job.carriedFood)`
- `IssueGatherCommand(IReadOnlyList<Unit> units, BerryBushResource bush)`
- `CancelForUnits` — `MoveCommand` / `AttackUnitCommand` / `AttackBuildingCommand` から呼ぶ

### GatherFoodCommand 例

```csharp
public sealed class GatherFoodCommand : IGameCommand
{
    public string DebugName => "GatherFood";
    public void Execute()
    {
        BuildingPlacementManager.AbortConstructionForUnits(units);
        AttackManager.CancelForUnits(units);
        GatherManager.CancelForUnits(units);      // Wood ジョブ解除
        FoodGatherManager.IssueGatherCommand(units, bush);
    }
}
```

- `GatherCommand.Execute` 側にも **`FoodGatherManager.CancelForUnits`** を追加（相互排他）

### SelectionManager 右クリック判定

```csharp
if (Physics.Raycast(ray, out hit, 1000f, GameLayers.ResourceMask))
{
    BerryBushResource bush = hit.collider.GetComponentInParent<BerryBushResource>();
    if (bush != null && !bush.IsDepleted)
    {
        CommandQueue.Enqueue(new GatherFoodCommand(selectedUnits, bush));
        return;
    }

    TreeResource tree = hit.collider.GetComponentInParent<TreeResource>();
    if (tree != null && !tree.IsDepleted)
    {
        CommandQueue.Enqueue(new GatherCommand(selectedUnits, tree));
        return;
    }
}
```

- BerryBush と Tree は **別 GameObject**（Collider 共有しない）

### Villager 生産コスト

- `BuildingData` に `public float villagerFoodCost = 50f;`
- `ProductionManager.TryQueueProduction` 内で `ResourceManager.TrySpendFood(townCenter.Team, foodCost)` を Pop チェック後に実行
- 失敗時は Wood を消費しない（現状 Villager に Wood コストなし）

### Phase10 マップ配置（例）

| 要素 | 推奨 |
|------|------|
| Player 付近 Berry Bush | 2〜3 本（TC から 10〜20m） |
| CPU 付近 Berry Bush | 2〜3 本（CPU TC 付近） |
| 既存の木 | **変更しない** |

### SceneBuilder Systems 追加

```csharp
// FoodGatherManager
GameObject foodGatherObject = new GameObject("FoodGatherManager");
foodGatherObject.transform.SetParent(systems.transform);
foodGatherObject.AddComponent<FoodGatherManager>();

// BerryBushSpatialIndex（CPU 採集する場合）
GameObject berryIndexObject = new GameObject("BerryBushSpatialIndex");
berryIndexObject.transform.SetParent(systems.transform);
berryIndexObject.AddComponent<BerryBushSpatialIndex>();
```

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`（主）。`Benchmark.unity` は回帰確認任意
- **`AoE → Setup Phase10 Scene`** — Berry Bush 配置 + Systems 更新 + `FoodNodeData` 生成
- Phase 1〜16 Setup メニューは壊さない
- Berry Bush 見た目: Editor で Primitive（Sphere 等）または `EntityVisualBuilder` — **`.prefab` 手書き禁止**

---

## ⑨ 完了条件（Phase 17 MVP）

- [ ] `ResourceManager` にチーム別 **Food** がある（Get / Add / TrySpend）
- [ ] マップに **Berry Bush** があり、右クリックで採集開始できる
- [ ] 採集 → TC 搬入 → **Food カウンタが増える**
- [ ] Bush の储量が減り、枯渇すると採集できない
- [ ] HUD に **Food: N**（+ CPU Food）が表示される
- [ ] **Wood 採集**（木右クリック）が Phase 16 同様に動作する
- [ ] **Command Queue** 経由（`GatherFoodCommand`）で採集が動く
- [ ] Villager 生産に **Food コスト** があり、不足時は生産不可
- [ ] **Phase10** — 建築・Militia・CPU 攻撃波・**Victory / Defeat** が動作する
- [ ] Console に **Null 参照・例外なし**
- [ ] `docs/IMPLEMENTATION_STATUS.md` と `README.md` を Phase 17 完了に更新

---

## ⑩ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene` → Play
2. HUD に **Food: 200**（開始値）と **Wood: 0** が表示される
3. 村民選択 → **Berry Bush 右クリック** → 採集 → TC 搬入 → **Food 増加**
4. 村民選択 → **木右クリック** → Wood 採集が従来どおり動作
5. Food 採集中に **地面右クリック** → Food ジョブがキャンセルされ移動する
6. TownCenter 選択 → **Q** で村民生産 — Food **50 消費**、不足時は不可
7. **Build House / Barracks**、Militia 生産、CPU 攻撃波が動作
8. 敵 TC 破壊 → **VICTORY**
9. Console エラーなし

Phase 17 のみ実装。Phase 18（Farm）以降に触れない。

# Phase 18 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜17 完了（PoC + Foundation + Food）  
> **マイルストン:** M2 Economy  
> **ロードマップ:** [03_M2_ECONOMY_PHASES.md](../03_M2_ECONOMY_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 18 実装（Farm）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜17 は完了済み。Phase 18 のみ実装すること。**

---

## ① M2 Economy 方針（必読・遵守）

Phase 17 で Food（Berry Bush 採集）が導入済み。Phase 18 は **Farm 建築 + Villager による継続 Food 採集** のみ。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | Farm のみ（Lumber Camp / Gold / Stone は触らない） |
| **small diff** | `FoodGatherManager` の rewrite 禁止。Berry Bush 経路は維持 |
| **既存パターン再利用** | House 建築フロー + Berry Bush 採集フローを Farm に合成 |
| **既存ゲームを壊さない** | 完了時 `Phase10.unity` で Berry / Wood 採集・建築・生産・戦闘・CPU・Victory / Defeat + Foundation 全機能が動くこと |
| **Foundation 維持** | Command Queue / Fixed Tick 20 TPS / Pool / Spatial Hash を壊さない |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止
- Manager 更新方式を維持
- **Unity アセット手書き禁止**: Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- Setup メニューは **Edit モード専用**
- **OnGUI は左上原点** — `GameUiInput.GuiRectToScreenRect`
- **`GetInstanceID()` 禁止**

---

## ② Phase 17 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **Food** — `BerryBushResource` → `FoodGatherManager` → TC 搬入（`GatherFoodCommand`）
- **Wood** — `TreeResource` → `GatherManager` → TC 搬入（`GatherCommand`）
- **資源** — `ResourceManager`（Wood / Food、チーム別、開始 Food 200）
- **Villager コスト** — `BuildingData.villagerFoodCost = 50`
- **建築** — House / Barracks（`BuildingPlacementManager` + `BuildingPool`）
- **Foundation** — Victory / Defeat、Pool、Spatial Hash、Fixed Tick、Command Queue
- **CPU** — 木採集・House・Villager 増産（Berry 採集は **未対応**）

### 現状のギャップ（Phase 18 で解消）

| 項目 | 現状 |
|------|------|
| Farm 建築 | **なし** — House / Barracks のみ |
| 継続 Food 生産 | Berry Bush のみ（枯渇すると採集不可） |
| Farm からの採集 | **なし** |
| `PlacedBuildingKind` | House / Barracks のみ |
| Building レイヤー右クリック | **攻撃のみ**（Farm 採集判定なし） |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| Food 採集 | `Assets/Scripts/Economy/FoodGatherManager.cs` |
| Berry Bush | `Assets/Scripts/Economy/BerryBushResource.cs` / `FoodNodeData.cs` |
| 建築配置 | `Assets/Scripts/Buildings/BuildingPlacementManager.cs` |
| 建築データ | `Assets/Scripts/Buildings/PlacedBuildingData.cs` / `PlacedBuildingDataResolver.cs` |
| 建築生成 | `Assets/Scripts/Buildings/RuntimeBuildingFactory.cs` / `BuildingPool.cs` |
| House 参考 | `Assets/Scripts/Buildings/House.cs` |
| 命令 | `Assets/Scripts/Commands/GameCommands.cs` |
| 入力 | `Assets/Scripts/Selection/SelectionManager.cs` |
| HUD | `Assets/Scripts/Selection/ResourceHudView.cs` |
| Visual | `Assets/Scripts/Visuals/EntityVisualBuilder.cs` / `PlaceholderVisualKind.cs` |
| Editor | `Assets/Scripts/Editor/Phase1SceneBuilder.cs` / `Phase10SceneBuilder.cs` |
| パス | `Assets/Scripts/Core/GameAssetPaths.cs` |

---

## ③ Phase 18 目的

**Farm 建築と採集** — AoE2 と同様、Wood で Farm を建て、村民が Farm から Food を採集する。Farm の Food が尽きると **建物が消える**（Pool 返却）。

Berry Bush は Phase 17 のまま維持。Farm は **建築後の長期 Food 源**。

### 変更後の Food 経済（追加部分）

```
HUD「Build Farm」→ 配置モード → 地面クリック（BuildConfirmCommand）
    ↓
BuildingPlacementManager — Villager 建築 → 完成
    ↓
Farm 完成（remainingFood = foodCapacity）
    ↓
村民が Farm 右クリック → GatherFarmFoodCommand
    ↓
FoodGatherManager — MoveToFarm → Gather → MoveToDeposit → AddFood
    ↓
remainingFood ≦ 0 → Farm 枯渇 → BuildingPool.ReturnFarm
```

### 今回実装するもの

1. **`PlacedBuildingKind.Farm`** — enum 拡張
2. **`PlacedBuildingData` 拡張** — `foodCapacity`（Farm 用、推奨 **250**）。`housingProvided = 0`
3. **`FarmData` アセット** — Editor で `Assets/Data/BuildingData/FarmData.asset` 生成（`GameAssetPaths.DefaultFarmData`）
4. **`Farm.cs`** — House と同パターン + Food 採集 API（`TakeFood` / `IsDepleted` / `GetGatherPosition` / `RemainingFood`）
5. **`RuntimeBuildingFactory` / `BuildingPool`** — `CreateFarm` / `RentFarm` / `ReturnFarm`（House と同パターン）
6. **`BuildingPlacementManager` 拡張** — `EnterFarmPlacementMode`、`CompleteConstruction` で Farm 生成、`TrySpendWood` で建設コスト
7. **`FoodGatherManager` 拡張** — Farm からの採集ジョブ追加（Berry Bush ジョブは維持）。`IssueGatherFarmCommand(units, Farm farm)`
8. **`GatherFarmFoodCommand`** — `IGameCommand`（Execute 内で `FoodGatherManager.IssueGatherFarmCommand`）
9. **`SelectionManager` 拡張** — Building レイヤー Raycast で **Farm 採集を攻撃より先**に判定（同チーム Farm + 非戦闘ユニットのみ）
10. **Command Cancel 相互排他** — `MoveCommand` / `Attack*` / `GatherFoodCommand` に Farm Cancel を追加。`GatherFarmFoodCommand` は Wood / Berry Cancel も呼ぶ
11. **Farm 枯渇処理** — `TakeFood` で `remainingFood ≦ 0` → アクティブ Gather ジョブ解除 → `BuildingPool.ReturnFarm`（`Destroy` 禁止）
12. **HUD** — `ResourceHudView` に **Build Farm (60 Wood)** ボタン（パネル高さ調整）
13. **`PlacedBuildingDataResolver.ResolveFarm`**
14. **`Phase10SceneBuilder`** — `FarmData` 生成確認（マップ上の初期 Farm 配置は **不要** — プレイヤーが建てる）
15. **README / `docs/IMPLEMENTATION_STATUS.md` / `03_M2_ECONOMY_PHASES.md` 更新**

### Farm パラメータ（MVP — Data で調整可）

| 項目 | 推奨値 | 備考 |
|------|--------|------|
| Wood コスト | **60** | AoE2 相当 |
| 建築時間 | **8 秒** | House 3s / Barracks 5s より長め |
| Food 容量 | **250** | `PlacedBuildingData.foodCapacity` |
| 搬送上限 | **10** | FoodGatherManager 既存定数 |
| 採集速度 | **2.5 Food / 秒** | Berry と同じ |
| Deposit 先 | **TownCenter** | チーム別 TC |
| Pop 加算 | **0** | Farm は人口を増やさない |
| HP | **100** | 攻撃可能（破壊で Pool 返却） |
| フットプリント | **4×4** | House と同サイズ可 |

### 禁止（Phase 18 範囲外）

- **Lumber Camp** / **Gold** / **Stone**（Phase 19〜20）
- Farm の **自動再生**（枯渇後に Food が戻る）
- Farm からの **直接リソース加算**（村民採集必須）
- `FoodGatherManager` の Berry Bush ロジック rewrite
- `GatherFoodCommand`（Berry 用）のシグネチャ変更
- Militia / Archer の Food コスト
- 新シーン `Phase18.unity`（**検証は `Phase10.unity` のみ**）
- CPU Farm 建築 AI（**任意** — 時間があれば `CpuEconomyAiManager` に Berry 代替で Farm 建設を追加）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 18-1 | `PlacedBuildingKind.Farm` + `FarmData` + `PlacedBuildingDataResolver` + `GameAssetPaths` |
| 18-2 | `Farm.cs` + `RuntimeBuildingFactory.CreateFreshFarm` + `BuildingPool` Rent/Return |
| 18-3 | `BuildingPlacementManager` — EnterFarm / CompleteConstruction / Wood 支払い |
| 18-4 | `FoodGatherManager` — Farm ジョブ + 枯渇時 Pool 返却 |
| 18-5 | `GatherFarmFoodCommand` + `SelectionManager` Building レイヤー判定 |
| 18-6 | HUD Build Farm ボタン + Phase10SceneBuilder + ドキュメント |

各サブステップ後に **Berry Bush / Wood 採集が壊れていないこと** を確認すること。

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**（新規 / 変更 / 削除）
2. **新規追加ファイル一覧**
3. **Farm 採集と Berry 採集の共存方針**（同一 Manager 内 2 ジョブ型 or 統合フィールド）
4. **Building レイヤー Raycast 優先順位**（Farm 採集 vs 建物攻撃）
5. **Farm 枯渇時のジョブ解除・Pool 返却フロー**
6. **影響範囲**（BuildingPool / BuildingHealth 破壊 / Command Queue）
7. **リスク**（Farm 上で建築ゴースト重複 / 枯渇中の搬送 / Militia が Farm を攻撃）
8. **ロールバック方法**
9. **完了条件**（下記チェックリスト）
10. **テスト手順**

---

## ⑥ 実装後に必ず出力

1. **変更内容サマリ**
2. **変更ファイル一覧**
3. **Farm 関連 API 一覧**
4. **テスト結果**（Phase10 コアループ + Berry / Wood 回帰 + Victory）
5. **既知の制限**（CPU Farm 未対応 / Farm 再生なし 等）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### PlacedBuildingData 拡張

```csharp
public enum PlacedBuildingKind { House = 0, Barracks = 1, Farm = 2 }

// PlacedBuildingData に追加
public float foodCapacity; // Farm のみ使用（0 なら非 Farm）
```

### Farm.cs（BerryBushResource + House の合成）

- `Building` レイヤー、`BuildingHealth` 付き
- `TakeFood(float amount)` — 枯渇時 `OnDepleted()` → `FoodGatherManager.CancelJobsForFarm(this)` → `BuildingPool.ReturnFarm(this)`
- `PrepareForReuse` — `remainingFood = data.foodCapacity` でリセット
- `UnitTeam Team` — 建設者チームと一致

### FoodGatherManager 拡張パターン

```csharp
struct FarmGatherJob
{
    public Unit unit;
    public Farm farm;
    public GatherState state;
    public float carriedFood;
}

public static void IssueGatherFarmCommand(IReadOnlyList<Unit> units, Farm farm)
{
    // farm.Team == unit.Team、!farm.IsDepleted、!unit.CanAttack を検証
}

public static void CancelJobsForFarm(Farm farm) { ... }
```

- Deposit は既存 `GetDepositPosition(unit)` を再利用
- Berry 用 `FoodGatherJob` はそのまま別リスト、または 1 リストに `Farm` / `BerryBushResource` の nullable フィールド（small diff ならどちらでも可）

### SelectionManager — Building レイヤー（Farm 優先）

```csharp
if (Physics.Raycast(ray, out hit, 1000f, GameLayers.BuildingMask))
{
    Farm farm = hit.collider.GetComponentInParent<Farm>();
    if (farm != null && !farm.IsDepleted && TryIssueGatherFarmCommand(farm))
        return;

    BuildingHealth targetBuilding = hit.collider.GetComponentInParent<BuildingHealth>();
    if (targetBuilding != null && TryIssueAttackBuildingCommand(targetBuilding))
        return;
}
```

```csharp
bool TryIssueGatherFarmCommand(Farm farm)
{
    gatherFoodBuffer.Clear();
    for (int i = 0; i < selectedUnits.Count; i++)
    {
        Unit unit = selectedUnits[i];
        if (unit == null || unit.CanAttack || unit.Team != farm.Team)
            continue;
        gatherFoodBuffer.Add(unit);
    }
    if (gatherFoodBuffer.Count == 0) return false;
    CommandQueue.Enqueue(new GatherFarmFoodCommand(selectedUnits, farm));
    return true;
}
```

- 敵 Farm を攻撃する場合: Militia 選択時は `TryIssueAttackBuildingCommand` が動作（Farm に `BuildingHealth` あり）

### BuildingPlacementManager — CompleteConstruction 分岐

```csharp
if (site.data.kind == PlacedBuildingKind.Farm)
{
    UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
    RuntimeBuildingFactory.CreateFarm(site.data, site.position, team);
    return;
}
```

- `housingProvided` は Farm で 0 — `PopulationManager.AddHousing` を呼ばない

### BuildingPool — Farm（House パターンコピー）

- `RentFarm` / `ReturnFarm` / Prewarm は Phase 18 では **0 でも可**（初回 Play で spawn ログ確認）
- `BuildingHealth.OnDestroyed` から Farm 返却パスが既に House と同様に動くよう `BuildingHealth` を確認

### Visual（MVP）

- `PlaceholderVisualKind` に `Farm` 追加、または House シェル + 緑系 `defaultColor`（`PlacedBuildingData.defaultColor`）
- Ghost プレビューは既存 `BuildingPlacementManager` のゴースト機構を FarmData で再利用

### HUD レイアウト

```
Wood: N
Food: N
Pop: N/M
[Build House (...)]
[Build Barracks (...)]
[Build Farm (60 Wood)]   ← 追加
```

- パネル高さ・`GameUiInput.SetHudPanelScreenRect` をボタン 1 行分拡張

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`（主）
- **`AoE → Setup Phase10 Scene`** — `FarmData` アセット生成、`BuildingPool` Prewarm 任意
- 初期 Farm 配置は **不要**（プレイヤーが HUD から建設して検証）
- Phase 1〜17 Setup メニューは壊さない

---

## ⑨ 完了条件（Phase 18 MVP）

- [ ] HUD **Build Farm** で配置モードに入り、Wood **60** 消費で建築できる
- [ ] Villager が現場へ移動し、建築完了後 **Farm** が出現する
- [ ] 村民選択 → **Farm 右クリック** → Food 採集 → TC 搬入 → **Food 増加**
- [ ] Farm の `remainingFood` が減り、**枯渇で Farm が消える**（Pool 返却）
- [ ] **Berry Bush 採集**が Phase 17 同様に動作する
- [ ] **Wood 採集**が従来どおり動作する
- [ ] **Command Queue** 経由（`GatherFarmFoodCommand` / `BuildConfirmCommand`）
- [ ] Militia が **敵 Farm**（将来 CPU 用）または敵建築を攻撃できる（既存攻撃が壊れない）
- [ ] **Phase10** — House / Barracks / Villager 生産 / CPU / **Victory / Defeat** が動作する
- [ ] Console に **Null 参照・例外なし**
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `03_M2_ECONOMY_PHASES.md` / `README.md` を Phase 18 完了に更新

---

## ⑩ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene` → Play
2. Wood を採集（木右クリック）して **60 以上**確保
3. HUD **Build Farm (60 Wood)** → 地面クリックで配置 → Villager が建築 → **Farm 完成**
4. 村民選択 → **Farm 右クリック** → 採集 → TC 搬入 → **Food 増加**
5. Farm を枯渇させる → **Farm が消える**
6. **Berry Bush 右クリック** → Food 採集が動作（Phase 17 回帰）
7. **木右クリック** → Wood 採集が動作
8. Food 採集中に **地面右クリック** → ジョブキャンセル + 移動
9. **Build House / Barracks**、Militia 生産、敵 TC 破壊 → **VICTORY**
10. Console エラーなし

Phase 18 のみ実装。Phase 19（Lumber Camp）以降に触れない。

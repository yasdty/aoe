# Phase 24 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜23 完了（PoC + Foundation + M2 Economy + M2.5 Phase 21〜23）  
> **マイルストン:** M2.5 Economy Polish  
> **ロードマップ:** [04_M2_5_ECONOMY_POLISH_PHASES.md](../04_M2_5_ECONOMY_POLISH_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 24 実装（Hunting: Deer / Sheep）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜23 は完了済み。Phase 24 のみ実装すること。**

---

## ① M2.5 Economy Polish 方針（必読・遵守）

Phase 21〜23 で採取リピート・Farm 1 人・Spawn グリッド・Mining Camp が完成。Phase 24 は **被動動物（Deer / Sheep）からの Food 狩り** のみ。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | Deer / Sheep 狩り → TC 搬入 |
| **small diff** | **Berry Bush パターンを拡張** — rewrite / 統合リファクタ禁止 |
| **既存パターン再利用** | `BerryBushResource` / `FoodGatherManager` Berry ジョブ / `GatherFoodCommand` |
| **既存ゲームを壊さない** | Berry / Farm / 4 資源 / Mining Camp / CPU + Foundation 全機能 |
| **Foundation 維持** | Command Queue / Fixed Tick 20 TPS / Pool / Spatial Hash |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- Setup メニューは **Edit モード専用**（本 Phase で **Phase10SceneBuilder に Deer/Sheep 配置を追加**）

---

## ② Phase 23 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **Food 源** — Berry Bush / Farm のみ（狩り **なし**）
- **FoodGatherManager** — Berry ジョブ + Farm ジョブ（別リスト）
- **Berry 右クリック** — `GatherFoodCommand` → `IssueGatherCommand`
- **採取リピート** — Berry / Farm 搬入後復帰（Phase 21）
- **Drop-off** — Food は TC のみ（Mill は Phase 26）

### 現状のギャップ（Phase 24 で解消）

| 項目 | 現状 |
|------|------|
| Deer / Sheep | **未実装** |
| 狩り Food 源 | Berry → Farm の間の序盤ルートが欠ける |
| 右クリック狩り | Resource レイヤーは Berry / Tree / Mine のみ |

**実装前に必ず開いて読むファイル（Berry テンプレート）:**

| 領域 | ファイル |
|------|----------|
| Berry 本体 | `Assets/Scripts/Economy/BerryBushResource.cs` |
| Food Data | `Assets/Scripts/Economy/FoodNodeData.cs` |
| Food 採集 | `Assets/Scripts/Economy/FoodGatherManager.cs` — Berry ジョブ |
| 命令 | `Assets/Scripts/Commands/GameCommands.cs` — `GatherFoodCommand` |
| 選択 | `Assets/Scripts/Selection/SelectionManager.cs` — ResourceMask 右クリック |
| 空間索引 | `Assets/Scripts/Spatial/BerryBushSpatialIndex.cs`（参考 — 狩り対象が少なら optional） |
| Editor | `Assets/Scripts/Editor/Phase1SceneBuilder.cs` — `CreateBerryBush` |
| シーン | `Assets/Scripts/Editor/Phase10SceneBuilder.cs` — Berry 配置 |

---

## ③ Phase 24 目的

**被動動物 Deer / Sheep から Food を採取。** AoE2 Dark Age の Berry 枯渇後〜Farm 前の Food 源。

### 変更後の狩りフロー（MVP）

```
村民 → Deer/Sheep 右クリック
    ↓
MoveToAnimal → Hunt（近接で Food 取得）→ MoveToDeposit（TC）
    ↓
動物 Food 残あり → MoveToAnimal（リピート）
    ↓
動物枯渇 → ジョブ終了（オブジェクト非表示 or Destroy）
```

### AoE2 参考

- Deer / Sheep は **反撃しない**（Boar は Phase 25）
- 殴ると Food が減り、枯渇で消える
- 逃げる AI は **Phase 24 では optional**（small diff なら省略可）

---

## ④ 今回実装するもの

### 動物リソース

1. **`DeerResource.cs` / `SheepResource.cs`** — `BerryBushResource` と同型
   - `FoodNodeData` 再利用可（Deer / Sheep 用 asset を分けても OK）
   - `TakeFood(float)` / `IsDepleted` / `GetGatherPosition`
   - 枯渇時: 非表示 or `SetActive(false)`（Berry と同様 SpatialIndex Unregister）
2. **`DefaultDeer.asset` / `DefaultSheep.asset`** — `FoodNodeData`（案: Deer 140 Food / Sheep 100 Food — AoE2 参考で調整可）
3. **`GameAssetPaths`** — パス定数 + `Phase1SceneBuilder.EnsureDefaultDeerData` / `EnsureDefaultSheepData`
4. **`Phase1SceneBuilder.CreateDeer` / `CreateSheep`** — Berry Bush コピー（Capsule 等で色分け）

### 狩りジョブ

5. **`FoodGatherManager` 拡張** — 狩りジョブリスト（`huntJobs`）追加
   - 状態: `MoveToAnimal` / `Hunt` / `MoveToDeposit`
   - `IssueHuntCommand(units, IAnimalFoodSource)` または Deer/Sheep 別メソッド
   - Hunt 中: `GatherRate` で `TakeFood`（Berry `TickGather` と同パターン）
   - 搬入先: 既存 TC Deposit（Berry と同 `GetDepositPosition`）
   - **採取リピート維持** — 搬入後、動物が有効なら `MoveToAnimal` へ復帰
6. **`HuntFoodCommand`**（`GameCommands.cs`）— `GatherFoodCommand` コピー
7. **`SelectionManager`** — ResourceMask で Deer/Sheep ヒット → `HuntFoodCommand` enqueue
   - Militia 選択時は狩り不可（`CanAttack` フィルタ — Berry と同様）

### シーン配置

8. **`Phase10SceneBuilder`** — Player / CPU 近傍に Deer 2〜3 / Sheep 1〜2 配置
   - `Setup Phase10 Scene` 再実行で反映

### パラメータ（MVP 案）

| 項目 | Deer | Sheep |
|------|------|-------|
| 初期 Food | 140 | 100 |
| 色 | 茶系 | 白系 |
| コライダー | Capsule 系 | Capsule 系（やや小） |
| 反撃 | なし | なし |

### 禁止（Phase 24 範囲外）

- **Boar 反撃**（Phase 25）
- Mill Drop-off（Phase 26）
- CPU 狩り AI（Phase 28）
- 動物の逃げ / ランダム移動 AI（optional — 入れるなら small diff）
- Militia が動物を攻撃
- Berry / Farm ジョブの rewrite
- `FoodGatherManager` 3 リストを 1 つに統合

---

## ⑤ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 24-1 | `DeerResource` / `SheepResource` + Data asset + Editor Create |
| 24-2 | `FoodGatherManager` 狩りジョブ + リピート |
| 24-3 | `HuntFoodCommand` + `SelectionManager` |
| 24-4 | `Phase10SceneBuilder` 配置 + Setup 再実行 |
| 24-5 | Play 確認 + 回帰 + ドキュメント更新 |

---

## ⑥ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**
2. **Berry との差分マップ**
3. **狩りジョブ状態機械**（MoveToAnimal / Hunt / Deposit）
4. **影響範囲**（Cancel 相互排他 / AttackManager）
5. **リスク**（同一動物への複数村民 — MVP は Berry 同様複数可で OK）
6. **ロールバック方法**
7. **完了条件**
8. **テスト手順**

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### DeerResource（BerryBushResource コピー例）

```csharp
public float TakeFood(float amount)
{
    if (amount <= 0f || IsDepleted) return 0f;
    float taken = Mathf.Min(amount, remainingFood);
    remainingFood -= taken;
    UpdateVisual();
    if (IsDepleted) { /* unregister + hide */ }
    return taken;
}
```

### FoodGatherManager 狩りジョブ

- Berry `FoodGatherJob` と **別 struct + 別 list**（Farm パターン）
- `CancelForUnit` / `CancelForUnits` に狩りジョブ解除を追加
- `TickHunt` — `GatherRate * deltaTime` → `animal.TakeFood`

### SelectionManager

```csharp
DeerResource deer = hit.collider.GetComponentInParent<DeerResource>();
if (deer != null && !deer.IsDepleted)
{
    CommandQueue.Enqueue(new HuntFoodCommand(selectedUnits, deer));
    return;
}
// Sheep も同様 — または共通 interface / base class（small diff なら 2 分岐で OK）
```

### Cancel 連携

- `GameCommands` の Move / Attack / Gather 系は既存どおり `FoodGatherManager.CancelForUnits` 呼び出し
- 狩りジョブも **同 Cancel 経路** に含める

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`
- **Setup 再実行:** **必要** — Deer / Sheep 配置追加後 `AoE → Setup Phase10 Scene`
- Phase 1〜23 Setup メニューは壊さない

---

## ⑨ 完了条件（Phase 24 MVP）

- [x] **Deer** — 村民右クリック → 狩り → TC 搬入 → Food 増加
- [x] **Sheep** — 同上
- [x] **枯渇** — 動物 Food 0 で採取不可・見た目変化
- [x] **採取リピート** — 搬入後同動物へ復帰（Phase 21 回帰）
- [x] Berry / Farm / Wood / Gold / Stone / Mining Camp 回帰
- [x] Militia 選択時は狩り命令しない（`CanAttack` フィルタ）
- [ ] Console エラーなし（Play 確認待ち）
- [x] `docs/IMPLEMENTATION_STATUS.md` / `04_M2_5_ECONOMY_POLISH_PHASES.md` Phase 24 を ✅

### Victory 確認について

M2.5 では **毎回 Victory まで確認不要**。

---

## ⑩ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene` → `Phase10.unity` → Play
2. **Deer:** 村民 → Deer 右クリック → 狩り → TC 搬入 → Food 増加
3. **Sheep:** 村民 → Sheep 右クリック → 同上
4. **リピート:** 1 体で複数往復（Phase 21 回帰）
5. **枯渇:** Food 0 まで狩り → 灰色化 / 採取不可
6. Berry / Farm / 4 資源 / Mining Camp 回帰
7. Militia 選択 → 動物右クリック → 攻撃（狩りではない）— 既存 Attack 優先
8. Console エラーなし

Phase 24 のみ実装。**Phase 25 以降（M2.5）** に触れない。

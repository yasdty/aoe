# Phase 27 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜26 完了（PoC + Foundation + M2 Economy + M2.5 Phase 21〜26）  
> **マイルストン:** M2.5 Economy Polish  
> **ロードマップ:** [04_M2_5_ECONOMY_POLISH_PHASES.md](../04_M2_5_ECONOMY_POLISH_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 27 実装（Mill — Food Drop-off）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜26 は完了済み。Phase 27 のみ実装すること。**

---

## ① M2.5 Economy Polish 方針（必読・遵守）

Phase 26 で Boar 反撃狩り（HP / Food 分離）が完成。Phase 27 は **Mill（Food Drop-off 拠点）** のみ。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | Mill 建築 + Berry / Farm / 狩り肉の搬入先 |
| **small diff** | **Lumber Camp / Mining Camp パターンをコピー拡張** — rewrite / 統合リファクタ禁止 |
| **既存パターン再利用** | `LumberCamp` / `MiningCamp` / `*Registry` / `FoodGatherManager.GetDepositPosition` |
| **既存ゲームを壊さない** | Boar / Deer / Sheep / 4 資源 / Info Panel / CPU + Foundation 全機能 |
| **Foundation 維持** | Command Queue / Fixed Tick 20 TPS / Pool / Spatial Hash |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- Setup メニューは **Edit モード専用**（本 Phase で Scene 変更は **不要** — Mill はプレイ中に建築）
- **`.meta` は 32 文字 GUID**（`LumberCamp.cs.meta` 形式で OK）

---

## ② Phase 26 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **Food 採取** — Berry / Farm / Deer / Sheep / Boar（死体）
- **Food Drop-off** — **TC のみ**（`FoodGatherManager.GetDepositPosition` が TC 固定）
- **Wood Drop-off** — TC + Lumber Camp（`LumberCampRegistry`）
- **Gold / Stone Drop-off** — TC + Mining Camp（`MiningCampRegistry`）
- **Boar** — HP / Food 分離、Militia 攻撃、反撃、Info Panel
- **Mill** — **未実装**

### 現状のギャップ（Phase 27 で解消）

| 項目 | 現状 |
|------|------|
| Mill | **未実装** |
| Food 搬入 | 常に TC — Berry / Farm / 狩り拠点から遠いと往復が長い |
| HUD Build ボタン | Lumber / Mining Camp のみ — **Build Mill なし** |

**実装前に必ず開いて読むファイル（Mining Camp テンプレート — Phase 23）:**

| 領域 | ファイル |
|------|----------|
| Camp 本体 | `Assets/Scripts/Buildings/MiningCamp.cs` / `LumberCamp.cs` |
| レジストリ | `Assets/Scripts/Buildings/MiningCampRegistry.cs` / `LumberCampRegistry.cs` |
| Food 搬入先 | `Assets/Scripts/Economy/FoodGatherManager.cs` — `GetDepositPosition`（**TC 固定 — 要変更**） |
| 配置 | `Assets/Scripts/Buildings/BuildingPlacementManager.cs` — `EnterMiningCampPlacementMode` |
| HUD | `Assets/Scripts/Selection/ResourceHudView.cs` — Build Mining Camp ボタン |
| 生成 / Pool | `Assets/Scripts/Buildings/RuntimeBuildingFactory.cs` / `BuildingPool.cs` |
| 種別 | `Assets/Scripts/Buildings/PlacedBuildingData.cs` — `PlacedBuildingKind` |
| Data | `Assets/Data/BuildingData/MiningCampData.asset` |
| Editor | `Assets/Scripts/Editor/Phase1SceneBuilder.cs` — `EnsureMiningCampData` |
| Info Panel | `Assets/Scripts/Selection/SelectionInfoPanelView.cs` — Lumber/Mining Camp 左クリック |

---

## ③ Phase 27 目的

**Lumber / Mining Camp と同パターンで Mill を追加。** Food 採取村民が **最寄りの TC または Mill** へ搬入する。

### 変更後の Food 搬入フロー

```
村民 → Berry / Farm / Deer / Sheep / Boar(死体) 右クリック
    ↓
Gather / Hunt → MoveToDeposit
    ↓
最寄り Drop-off = min(TC, Mill) 距離
    ↓
AddFood → 採取リピート（Phase 21 維持）
```

### AoE2 参考

- Mill は Dark Age から建築可能（100 Wood）
- Berry / Farm / 狩り肉 / 漁業（将来）の Drop-off
- TC から離れた Food 源近くに建てて往復短縮

---

## ④ 今回実装するもの

### Mill 建築

1. **`PlacedBuildingKind.Mill`** — enum 追加（次の整数 — 既存 enum を確認）
2. **`Mill.cs`** — `MiningCamp` と同型（`GetDepositPosition` / Register / Visual）
3. **`MillRegistry.cs`** — `TryGetNearestFoodDepositPosition(Unit, radius, out position)` — TC + 自チーム Mill から最寄り
4. **`MillData.asset`** — `PlacedBuildingData`（下表パラメータ）
5. **`RuntimeBuildingFactory` / `BuildingPool`** — Create / Rent / Return（Mining Camp コピー）
6. **`BuildingPlacementManager`** — `EnterMillPlacementMode` + 建築完了時 `CreateMill`
7. **`BuildingHealth`** — 破壊時 `BuildingPool.ReturnMill`
8. **`PlacedBuildingDataResolver.ResolveMill`** + `GameAssetPaths.DefaultMillData`
9. **`Phase1SceneBuilder.EnsureMillData`** + `Phase10SceneBuilder` で Ensure 呼び出し + `ResourceHudView` 用 SerializeField

### Food 搬入先変更

10. **`FoodGatherManager.GetDepositPosition`** — `MillRegistry.TryGetNearestFoodDepositPosition` を使用（TC 固定を置換）
    - 対象: Berry jobs / Farm jobs / Hunt jobs — **すべて同一 `GetDepositPosition` 経由**

### HUD / Selection

11. **`ResourceHudView`** — **Build Mill (100 Wood)** ボタン追加（Mining Camp ボタン下）
12. **`SelectionInfoPanelView`** — Mill 左クリック → 建物名 + HP（Lumber/Mining Camp 同型）
13. **`SelectionManager`** — Mill 左クリック選択（BuildingMask or 既存 PlacedBuilding 流）

### パラメータ（MVP）

| 項目 | 値（案） |
|------|----------|
| displayName | Mill |
| woodCost | 100 |
| buildTime | 6 秒 |
| maxHp | 400 |
| footprint | Lumber Camp 同型（1×1 または既存 Camp と同じ） |
| defaultColor | 薄い茶 / ベージュ系 `(0.62, 0.52, 0.38)` |

---

## ⑤ 今回やらないこと

- Sheep 誘導 / 動物徘徊（**Phase 28**）
- Militia Aggro（**Phase 29**）
- CPU Mill 建築（**Phase 30**）
- Mill からのユニット生産
- 漁船 / 沿岸 Fish（M3 以降）
- Drop-off 統合 Registry リファクタ（TC + 3 Camp を 1 クラスに — **禁止**）

---

## ⑥ 実装ステップ（推奨）

| Step | 内容 |
|------|------|
| 27-1 | `MillData` + `Mill.cs` + `MillRegistry` + `.meta` |
| 27-2 | `BuildingPool` / `RuntimeBuildingFactory` / `BuildingHealth` / `PlacedBuildingKind` |
| 27-3 | `BuildingPlacementManager` + `PlacedBuildingDataResolver` + Editor Ensure |
| 27-4 | `FoodGatherManager.GetDepositPosition` → MillRegistry |
| 27-5 | `ResourceHudView` + Selection Info + Phase10 Ensure + ドキュメント |

---

## ⑦ 技術メモ

### FoodGatherManager — GetDepositPosition（案）

```csharp
static Vector3 GetDepositPosition(Unit unit)
{
    if (unit == null)
        return Vector3.zero;

    if (MillRegistry.TryGetNearestFoodDepositPosition(unit, DepositStandRadius, out Vector3 position))
        return position;

    return Vector3.zero;
}
```

`MillRegistry` 内部で TC も候補に含める（`LumberCampRegistry.TryGetNearestWoodDepositPosition` と同型）。

### MillRegistry — 最寄り判定

```csharp
// TC を初期 best に、自チーム Mill を走査して距離比較
// IsActiveDropOff: activeInHierarchy + Team 一致 + BuildingHealth.IsAlive
```

### IssueGather / Hunt との関係

- **変更不要:** `IssueGatherCommand` / `IssueHuntCommand` / farm jobs — すべて deposit 時に `GetDepositPosition` を呼ぶだけ
- Mill 建築 **後** に進行中ジョブも次回 `MoveToDeposit` から最寄り Mill を使う

### CPU

- Phase 27 では **Player のみ** Mill 建築 UI — CPU Mill は Phase 30

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`
- **Setup:** `AoE → Setup Phase10 Scene` — `EnsureMillData` + `ResourceHudView.millData` 参照追加
- 既存シーン: Play 中に HUD から Mill 建築で検証（専用 Add メニューは optional）

---

## ⑨ 完了条件（Phase 27 MVP）

- [ ] **Build Mill** — HUD ボタン → 100 Wood 消費 → 配置 → 建築完了
- [ ] **Berry** — Mill 近くの Bush 採取 → **Mill へ搬入** → Food 増加
- [ ] **Farm** — Mill 近く Farm → Mill 搬入
- [ ] **狩り** — Deer / Sheep / Boar 死体 → Mill 搬入
- [ ] **最寄り判定** — TC と Mill 両方ある場合、近い方へ搬入
- [ ] **Info Panel** — Mill 左クリック → 名前 + HP
- [ ] Lumber / Mining Camp / Boar / 4 資源 回帰
- [ ] Console エラーなし
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `04_M2_5` Phase 27 を ✅

---

## ⑩ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene`（または既存 Phase10）→ Play
2. **TC から離れた Berry Bush** 付近に **Mill を建築**
3. 村民 → Berry **右クリック** — 搬入先が **Mill**（TC より近い場合）
4. HUD Food カウント増加を確認
5. **Farm / Deer** でも Mill 搬入を 1 件ずつ確認
6. **Boar 死体** 狩り → Mill 搬入
7. Mill **左クリック** — Info Panel に HP 表示
8. Lumber Camp / Mining Camp / Boar 回帰
9. Console エラーなし

Phase 27 のみ実装。**Phase 28 以降（Sheep Herding 等）** に触れない。

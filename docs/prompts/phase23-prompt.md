# Phase 23 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜22 完了（PoC + Foundation + M2 Economy + M2.5 Phase 21〜22）  
> **マイルストン:** M2.5 Economy Polish  
> **ロードマップ:** [04_M2_5_ECONOMY_POLISH_PHASES.md](../04_M2_5_ECONOMY_POLISH_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 23 実装（Mining Camp）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜22 は完了済み。Phase 23 のみ実装すること。**

---

## ① M2.5 Economy Polish 方針（必読・遵守）

Phase 21〜22 で採取リピート・Farm 1 人制限・スポーングリッドが完成。Phase 23 は **Mining Camp（Gold / Stone Drop-off 拠点）** のみ。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | Mining Camp 建築 + Gold/Stone 搬入先 |
| **small diff** | **Lumber Camp パターンをコピー拡張** — rewrite / 統合リファクタ禁止 |
| **既存パターン再利用** | `LumberCamp` / `LumberCampRegistry` / `GatherManager.GetDepositPosition` 相当 |
| **既存ゲームを壊さない** | Phase 21 リピート / Phase 22 Farm+Spawn / 4 資源 / CPU + Foundation 全機能 |
| **Foundation 維持** | Command Queue / Fixed Tick 20 TPS / Pool / Spatial Hash |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- Setup メニューは **Edit モード専用**（本 Phase で Scene 変更は **不要** — Gold/Stone Mine は Phase10 に既存）

---

## ② Phase 22 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **採取リピート** — Wood / Berry / Farm / Gold / Stone
- **Farm 1 人制限** — `FoodGatherManager.IsFarmOccupiedByOther`
- **スポーングリッド** — `BuildingSpawnFormation`（TC / Barracks）
- **Wood Drop-off** — TC + **Lumber Camp**（`LumberCampRegistry.TryGetNearestWoodDepositPosition`）
- **Gold / Stone Drop-off** — **TC のみ**（`MineralGatherManager.GetDepositPosition` が TC 固定）

### 現状のギャップ（Phase 23 で解消）

| 項目 | 現状 |
|------|------|
| Mining Camp | **未実装** |
| Gold / Stone 搬入 | 常に TC — 鉱山から遠いと往復が長い |
| HUD Build ボタン | Lumber Camp のみ — Mining Camp なし |

**実装前に必ず開いて読むファイル（Lumber Camp テンプレート）:**

| 領域 | ファイル |
|------|----------|
| Lumber Camp 本体 | `Assets/Scripts/Buildings/LumberCamp.cs` |
| レジストリ | `Assets/Scripts/Buildings/LumberCampRegistry.cs` |
| Wood 搬入先 | `Assets/Scripts/Economy/GatherManager.cs` — `GetDepositPosition` / Registry 呼び出し |
| Mineral 搬入先 | `Assets/Scripts/Economy/MineralGatherManager.cs` — `GetDepositPosition`（**TC 固定 — 要変更**） |
| 配置 | `Assets/Scripts/Buildings/BuildingPlacementManager.cs` — `EnterLumberCampPlacementMode` |
| HUD | `Assets/Scripts/Selection/ResourceHudView.cs` — Build Lumber Camp ボタン |
| 生成 / Pool | `Assets/Scripts/Buildings/RuntimeBuildingFactory.cs` / `BuildingPool.cs` |
| 種別 | `Assets/Scripts/Buildings/PlacedBuildingData.cs` — `PlacedBuildingKind` |
| Data | `Assets/Data/BuildingData/LumberCampData.asset` |
| Editor | `Assets/Scripts/Editor/Phase1SceneBuilder.cs` — `EnsureLumberCampData` |

---

## ③ Phase 23 目的

**Lumber Camp と同パターンで Mining Camp を追加。** Gold / Stone 採掘村民が **最寄りの TC または Mining Camp** へ搬入する。

### 変更後の Mineral 搬入フロー

```
村民 → Gold/Stone Mine 右クリック
    ↓
Gather → MoveToDeposit
    ↓
最寄り Drop-off = min(TC, Mining Camp) 距離
    ↓
AddResource → 採取リピート（Phase 21 維持）
```

### AoE2 参考

- Mining Camp は Dark Age から建築可能（100 Wood）
- Gold / Stone 両方の Drop-off 拠点
- 鉱山近くに建てて往復短縮

---

## ④ 今回実装するもの

### Mining Camp 建築

1. **`PlacedBuildingKind.MiningCamp = 4`** — enum 追加
2. **`MiningCamp.cs`** — `LumberCamp` と同型（`GetDepositPosition` / Register / Visual）
3. **`MiningCampRegistry.cs`** — `TryGetNearestMineralDepositPosition(Unit, radius, out position)` — TC + 自チーム Mining Camp から最寄り
4. **`MiningCampData.asset`** — `PlacedBuildingData`（下表パラメータ）
5. **`RuntimeBuildingFactory` / `BuildingPool`** — Create / Rent / Return（Lumber Camp コピー）
6. **`BuildingPlacementManager`** — `EnterMiningCampPlacementMode` + 建築完了時 `CreateMiningCamp`
7. **`BuildingHealth`** — 破壊時 `BuildingPool.ReturnMiningCamp`
8. **`PlacedBuildingDataResolver.ResolveMiningCamp`** + `GameAssetPaths.DefaultMiningCampData`
9. **`Phase1SceneBuilder.EnsureMiningCampData`** + `Phase10SceneBuilder` で Ensure 呼び出し

### Mineral 搬入先変更

10. **`MineralGatherManager.GetDepositPosition`** — `MiningCampRegistry.TryGetNearestMineralDepositPosition` を使用（TC 固定を置換）

### HUD

11. **`ResourceHudView`** — **Build Mining Camp (100 Wood)** ボタン追加（Lumber Camp ボタン下）

### パラメータ（MVP）

| 項目 | 値 |
|------|-----|
| コスト | 100 Wood |
| 建築時間 | 6 秒 |
| HP | 400 |
| フットプリント | 4×4 |
| 色 | グレー系（Gold/Stone 鉱山に合わせた識別色 — Lumber Camp と被らない程度） |

### 禁止（Phase 23 範囲外）

- Mill / 狩り / Boar（Phase 24〜26）
- Militia Aggro（Phase 27）
- CPU Mining Camp 建築 AI（Phase 28）
- Gold / Stone を別 Camp に分離（MVP は **1 建物で両方 Drop-off**）
- Lumber Camp / Wood 搬入ロジックの変更
- Gather リピート / Farm 1 人 / Spawn グリッドの変更
- `Phase10.unity` 手動編集（Setup 再実行不要）

---

## ⑤ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 23-1 | `PlacedBuildingKind` + Data asset + `EnsureMiningCampData` |
| 23-2 | `MiningCamp` + `MiningCampRegistry` + Factory / Pool |
| 23-3 | `BuildingPlacementManager` + `BuildingHealth` 破棄返却 |
| 23-4 | `MineralGatherManager.GetDepositPosition` → Registry |
| 23-5 | `ResourceHudView` ボタン + Play 確認 |
| 23-6 | 回帰 + ドキュメント更新 |

---

## ⑥ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**
2. **Lumber Camp との差分マップ**（コピー元 → 新規名）
3. **`GetDepositPosition` 変更**（TC のみ → 最寄り TC/Camp）
4. **影響範囲**（Gold / Stone 両ジョブ / Phase 21 リピート）
5. **リスク**（enum 値追加による既存 asset 互換）
6. **ロールバック方法**
7. **完了条件**
8. **テスト手順**

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### MiningCampRegistry（LumberCampRegistry コピー例）

```csharp
public static bool TryGetNearestMineralDepositPosition(Unit unit, float depositStandRadius, out Vector3 position)
{
    // 1. TC を候補に
    // 2. 自チーム MiningCamp を走査、距離が近い方を選択
    // 3. UnitPositionOffsets.ApplyRingOffset(bestCenter, unit, depositStandRadius)
}
```

### MineralGatherManager.GetDepositPosition

```csharp
static Vector3 GetDepositPosition(Unit unit)
{
    if (MiningCampRegistry.TryGetNearestMineralDepositPosition(unit, DepositStandRadius, out Vector3 position))
        return position;
    return Vector3.zero;
}
```

- **Gold / Stone 共通** — 同一 Registry で OK（AoE2 Mining Camp も両方受け付け）
- Deposit 先なし（TC 破壊 + Camp なし）→ 既存どおりジョブ削除

### BuildingPool

- Lumber Camp と同様に `Stack<MiningCamp>` + spawn/reuse ログ（optional）

### HUD レイアウト

- パネル高さを 1 ボタン分増やす（Lumber Camp ボタンと同パターン）
- Wood 不足時は disabled

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`（Gold Mine / Stone Mine は既存）
- **Setup 再実行:** 不要（Data asset の Ensure のみ Phase10SceneBuilder に追加可）
- Phase 1〜22 Setup メニューは壊さない

---

## ⑨ 完了条件（Phase 23 MVP）

- [ ] **HUD** — Build Mining Camp ボタンで配置モード開始
- [ ] **建築** — 100 Wood / 6 秒 / 完成後 Drop-off 有効
- [ ] **Gold** — 鉱山近く Camp 建築 → 搬入先が Camp（TC より近い場合）
- [ ] **Stone** — 同上
- [ ] **TC 搬入** — Camp より TC が近い場合は TC へ（Lumber Camp と同ロジック）
- [ ] **採取リピート** — Phase 21 回帰（Gold / Stone 複数往復）
- [ ] Wood / Berry / Farm / Lumber Camp 回帰
- [ ] Console エラーなし
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `04_M2_5_ECONOMY_POLISH_PHASES.md` Phase 23 を ✅

### Victory 確認について

M2.5 では **毎回 Victory まで確認不要**。

---

## ⑩ テスト手順（Play チェックリスト）

1. `Phase10.unity` → Play
2. **建築:** 100 Wood 確保 → Build Mining Camp → 鉱山近くに配置 → 完成
3. **Gold:** 村民 → Gold Mine 右クリック → 搬入先が **近い Camp**（往復が TC より短い）
4. **Stone:** 村民 → Stone Mine 右クリック → 同上
5. **TC 優先:** Camp より TC が近い位置で採掘 → TC へ搬入
6. **リピート:** Gold / Stone 複数往復（Phase 21 回帰）
7. Wood / Berry / Farm / Lumber Camp 回帰
8. Console エラーなし

Phase 23 のみ実装。**Phase 24 以降（M2.5）** に触れない。

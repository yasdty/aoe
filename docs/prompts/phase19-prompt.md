# Phase 19 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜18 完了（PoC + Foundation + Food + Farm）  
> **マイルストン:** M2 Economy  
> **ロードマップ:** [03_M2_ECONOMY_PHASES.md](../03_M2_ECONOMY_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 19 実装（Lumber Camp）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜18 は完了済み。Phase 19 のみ実装すること。**

---

## ① M2 Economy 方針（必読・遵守）

Phase 17〜18 で Food 経済が成立済み。Phase 19 は **Lumber Camp 建築 + Wood 搬入先（Drop-off）** のみ。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | Lumber Camp のみ（Gold / Stone は触らない） |
| **small diff** | `GatherManager` の rewrite 禁止。採集状態機械は維持し **Deposit 先解決のみ拡張** |
| **既存パターン再利用** | Farm / House 建築フロー + `GatherManager` の TC 搬入 |
| **既存ゲームを壊さない** | 完了時 `Phase10.unity` で Wood / Food / Farm / Berry / 建築・生産・戦闘・CPU・Victory / Defeat + Foundation 全機能が動くこと |
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

## ② Phase 18 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **Wood** — `TreeResource` → `GatherManager` → **常に TC 搬入**（`GatherCommand`）
- **Food** — Berry Bush / Farm → `FoodGatherManager` → TC 搬入
- **資源** — `ResourceManager`（Wood / Food、チーム別）
- **建築** — House / Barracks / Farm（`BuildingPlacementManager` + `BuildingPool`）
- **Foundation** — Victory / Defeat、Pool、Spatial Hash、Fixed Tick、Command Queue
- **CPU** — 木採集・House・Villager 増産（Berry / Farm 未対応）

### 現状のギャップ（Phase 19 で解消）

| 項目 | 現状 |
|------|------|
| Lumber Camp 建築 | **なし** |
| Wood Drop-off | **TC のみ** — 森が遠いと往復が長い |
| `PlacedBuildingKind` | House / Barracks / Farm のみ |
| 最寄り搬入先選択 | **なし** |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| Wood 採集 | `Assets/Scripts/Economy/GatherManager.cs` |
| 木ノード | `Assets/Scripts/Economy/TreeResource.cs` / `ResourceNodeData.cs` |
| Farm 参考（建築） | `Assets/Scripts/Buildings/Farm.cs` / `BuildingPool.cs` |
| 建築配置 | `Assets/Scripts/Buildings/BuildingPlacementManager.cs` |
| 建築データ | `Assets/Scripts/Buildings/PlacedBuildingData.cs` / `PlacedBuildingDataResolver.cs` |
| 建築生成 | `Assets/Scripts/Buildings/RuntimeBuildingFactory.cs` |
| TC 登録 | `Assets/Scripts/Buildings/ProductionManager.cs` |
| 命令 | `Assets/Scripts/Commands/GameCommands.cs` |
| HUD | `Assets/Scripts/Selection/ResourceHudView.cs` |
| Editor | `Assets/Scripts/Editor/Phase1SceneBuilder.cs` / `Phase10SceneBuilder.cs` |
| パス | `Assets/Scripts/Core/GameAssetPaths.cs` |

---

## ③ Phase 19 目的

**Lumber Camp を Wood の Drop-off 拠点にする** — AoE2 と同様、森の近くに Lumber Camp を建て、村民は **最寄りの Drop-off**（Lumber Camp または TC）へ Wood を搬入する。Lumber Camp がなければ **従来どおり TC のみ**（後方互換）。

Food 経済（Berry / Farm）は **一切変更しない**。

### 変更後の Wood 経済（追加部分）

```
HUD「Build Lumber Camp」→ 配置モード → 地面クリック（BuildConfirmCommand）
    ↓
BuildingPlacementManager — Villager 建築 → 完成
    ↓
LumberCamp 完成（チーム別 Drop-off 登録）
    ↓
村民が木右クリック → GatherCommand（変更なし）
    ↓
GatherManager — MoveToTree → Gather → MoveToDeposit
    ↓
GetDepositPosition — 同チームの TC + 有効な LumberCamp から最寄りを選択
    ↓
ResourceManager.AddWood(team)
```

### 今回実装するもの

1. **`PlacedBuildingKind.LumberCamp`** — enum 拡張
2. **`LumberCampData` アセット** — Editor で `Assets/Data/BuildingData/LumberCampData.asset`（`GameAssetPaths.DefaultLumberCampData`）
3. **`LumberCamp.cs`** — House と同パターン + Drop-off API（`GetDepositPosition` / `Team`）
4. **`LumberCampRegistry`**（または `ProductionManager` 小拡張）— Register / Unregister / `GetNearestWoodDepositForTeam`
5. **`RuntimeBuildingFactory` / `BuildingPool`** — `CreateLumberCamp` / `RentLumberCamp` / `ReturnLumberCamp`
6. **`BuildingPlacementManager` 拡張** — `EnterLumberCampPlacementMode`、`CompleteConstruction` で LumberCamp 生成
7. **`GatherManager` 拡張** — `GetDepositPosition` を最寄り Drop-off 選択に変更（**Tick ロジック rewrite 禁止**）
8. **`BuildingHealth`** — 破壊時 `ReturnLumberCamp` + Registry 解除
9. **HUD** — `ResourceHudView` に **Build Lumber Camp (N Wood)** ボタン
10. **`PlacedBuildingDataResolver.ResolveLumberCamp`**
11. **`Phase10SceneBuilder`** — `LumberCampData` 生成確認（マップ上の初期配置は **不要**）
12. **README / `docs/IMPLEMENTATION_STATUS.md` / `03_M2_ECONOMY_PHASES.md` 更新**

### Lumber Camp パラメータ（MVP — Data で調整可）

| 項目 | 推奨値 | 備考 |
|------|--------|------|
| Wood コスト | **100** | AoE2 相当 |
| 建築時間 | **6 秒** | Farm 8s より短め可 |
| Pop 加算 | **0** | |
| HP | **400** | 攻撃可能（破壊で Pool 返却） |
| フットプリント | **4×4** | Farm と同サイズ可 |
| 採集速度ボーナス | **なし** | Drop-off のみ（Phase 19 範囲外） |
| Deposit 優先 | **最寄り** | 水平距離（XZ）で TC と Lumber Camp を比較 |

### 禁止（Phase 19 範囲外）

- **Gold** / **Stone**（Phase 20）
- Wood 採集速度の変更（`GatherRate` 定数は維持）
- `GatherCommand` のシグネチャ変更
- `FoodGatherManager` / Farm / Berry の変更
- Lumber Camp への右クリック命令（採集対象は **木のみ**）
- 新シーン `Phase19.unity`（**検証は `Phase10.unity` のみ**）
- CPU Lumber Camp 建築 AI（**任意** — 時間があれば `CpuEconomyAiManager` に追加）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 19-1 | `PlacedBuildingKind.LumberCamp` + `LumberCampData` + Resolver + `GameAssetPaths` |
| 19-2 | `LumberCamp.cs` + Registry + `RuntimeBuildingFactory` + `BuildingPool` |
| 19-3 | `BuildingPlacementManager` — EnterLumberCamp / CompleteConstruction |
| 19-4 | `GatherManager.GetDepositPosition` — 最寄り Drop-off |
| 19-5 | HUD Build Lumber Camp + `BuildingHealth` + Phase10SceneBuilder + ドキュメント |

各サブステップ後に **Food / Farm / Berry 採集が壊れていないこと** を確認すること。

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**（新規 / 変更 / 削除）
2. **新規追加ファイル一覧**
3. **Drop-off 選択アルゴリズム**（TC のみ / Lumber Camp あり / 複数 Camp）
4. **Registry のライフサイクル**（Rent / Return / 破壊）
5. **GatherManager 変更範囲**（どのメソッドだけ触るか）
6. **影響範囲**（CPU 木採集・Command Queue・既存 TC 搬入）
7. **リスク**（Camp 0 件時の後方互換 / Pool 返却後の参照）
8. **ロールバック方法**
9. **完了条件**（下記チェックリスト）
10. **テスト手順**

---

## ⑥ 実装後に必ず出力

1. **変更内容サマリ**
2. **変更ファイル一覧**
3. **Lumber Camp 関連 API 一覧**
4. **テスト結果**（Phase10 コアループ + Food 回帰 + Victory）
5. **既知の制限**（CPU 未対応 / 採集速度ボーナスなし 等）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### PlacedBuildingKind 拡張

```csharp
public enum PlacedBuildingKind { House = 0, Barracks = 1, Farm = 2, LumberCamp = 3 }
```

### LumberCamp.cs（House ベース）

- `Building` レイヤー、`BuildingHealth` 付き
- `OnEnable` / `OnDisable` または `PrepareForReuse` / `Return` で Registry 登録解除
- `GetDepositPosition()` — 建物縁付近の立ち位置（`BuildingHealth.GetMeleeStandPosition` または Farm `GetGatherPosition` 相当）

### Drop-off 選択（GatherManager）

```csharp
static Vector3 GetDepositPosition(Unit unit)
{
    // 1. 同チームの有効な LumberCamp を列挙
    // 2. TownCenter を候補に追加
    // 3. unit 現在地から XZ 距離が最短の候補を選択
    // 4. UnitPositionOffsets.ApplyRingOffset で散開
    // 候補ゼロ → Vector3.zero（既存と同じ失敗扱い）
}
```

- **Lumber Camp が 1 つもない** → 現行どおり TC のみ（挙動不変）
- **複数 Camp** → 常に最寄り（木の位置ではなく **村民の現在地** 基準でよい）

### BuildingPlacementManager — CompleteConstruction 分岐

```csharp
if (site.data.kind == PlacedBuildingKind.LumberCamp)
{
    UnitTeam team = site.builder != null ? site.builder.Team : UnitTeam.Player;
    RuntimeBuildingFactory.CreateLumberCamp(site.data, site.position, team);
    return;
}
```

### Registry パターン（Farm 枯渇バグを避ける）

- Tick ループ中に **リスト一括削除しない**
- `ReturnLumberCamp` は Pool 返却のみ。Registry は `OnDisable` / `PrepareForReuse` で解除
- `GatherManager` のループ内で Registry を変更しない

### Visual（MVP）

- House シェル + 茶系 `defaultColor`（`PlacedBuildingData.defaultColor`）
- または `PlaceholderVisualKind` 追加（任意）

### HUD レイアウト

```
Wood: N
Food: N
Pop: N/M
[Build House (...)]
[Build Barracks (...)]
[Build Farm (...)]
[Build Lumber Camp (100 Wood)]   ← 追加
```

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`（主）
- **`AoE → Setup Phase10 Scene`** — `LumberCampData` アセット生成
- 初期 Lumber Camp 配置は **不要**（プレイヤーが HUD から建設して検証）
- Phase 1〜18 Setup メニューは壊さない

---

## ⑨ 完了条件（Phase 19 MVP）

- [ ] HUD **Build Lumber Camp** で配置モードに入り、Wood コスト消費で建築できる
- [ ] Villager が現場へ移動し、建築完了後 **Lumber Camp** が出現する
- [ ] **Lumber Camp なし** — 木採集 → TC 搬入（Phase 18 以前と同じ）
- [ ] **Lumber Camp あり** — 森近くに Camp を建て、木採集 → **Camp へ搬入**（TC より近い場合）
- [ ] 搬入で **Wood 増加**
- [ ] **Berry / Farm / Wood 採集開始**（`GatherCommand` / `GatherFoodCommand`）が従来どおり
- [ ] Militia が **敵 Lumber Camp** を攻撃・破壊できる
- [ ] **Phase10** — House / Barracks / Farm / Villager 生産 / CPU / **Victory / Defeat** が動作
- [ ] Console に **Null 参照・例外なし**
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `03_M2_ECONOMY_PHASES.md` / `README.md` を Phase 19 完了に更新

---

## ⑩ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene` → Play
2. **Lumber Camp なし** — 木右クリック → TC 搬入 → Wood 増加（回帰）
3. Wood **100 以上**確保 → HUD **Build Lumber Camp** → 森の近くに配置 → 完成
4. 同じ森で採集 → **Lumber Camp へ搬入**（往復が TC より短いことを目視）
5. Lumber Camp を Militia で破壊 → 再採集は **TC 搬入**に戻る
6. **Berry Bush / Farm** 右クリック → Food 採集（回帰）
7. Food 採集中に **地面右クリック** → ジョブキャンセル + 移動
8. **Build House / Barracks / Farm**、Militia 生産、敵 TC 破壊 → **VICTORY**
9. Console エラーなし

Phase 19 のみ実装。Phase 20（Gold + Stone）以降に触れない。

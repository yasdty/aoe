# Phase 20 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜19 完了（PoC + Foundation + M2 Economy 17〜19）  
> **マイルストン:** M2 Economy **完了 Phase**  
> **ロードマップ:** [03_M2_ECONOMY_PHASES.md](../03_M2_ECONOMY_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 20 実装（Gold + Stone）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜19 は完了済み。Phase 20 のみ実装すること。**

---

## ① M2 Economy 方針（必読・遵守）

Phase 17〜19 で Wood / Food 経済が成立済み。Phase 20 は **Gold + Stone 採掘ノード + TC 搬入** で **4 資源完成**。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | Gold + Stone 採集のみ（Mining Camp 建築は触らない） |
| **small diff** | `GatherManager` / `FoodGatherManager` の rewrite 禁止。新 Manager は **並列追加** |
| **既存パターン再利用** | Phase 17 Berry Bush（`FoodGatherManager` + `GatherFoodCommand`）を Gold / Stone に適用 |
| **既存ゲームを壊さない** | Wood / Food / Farm / Lumber Camp / 建築・生産・CPU + Foundation 全機能が動くこと |
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

## ② Phase 19 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **Wood** — `TreeResource` → `GatherManager` → 最寄り Drop-off（TC / Lumber Camp）
- **Food** — Berry Bush / Farm → `FoodGatherManager` → TC 搬入
- **資源 HUD** — Wood / Food（Player + CPU）
- **建築** — House / Barracks / Farm / Lumber Camp
- **Foundation** — Victory / Defeat、Pool、Spatial Hash、Fixed Tick、Command Queue
- **CPU** — 木採集・House・Villager 増産（Berry / Farm / Gold / Stone **未対応**）

### 現状のギャップ（Phase 20 で解消）

| 項目 | 現状 |
|------|------|
| Gold / Stone | **なし** — `ResourceManager` は Wood / Food のみ |
| 採掘ノード | Tree / Berry Bush / Farm のみ |
| Resource レイヤー右クリック | 木 / Berry / Farm 採集のみ |
| HUD | Gold / Stone 表示なし |
| **M2 完了条件** | 4 資源中 **2 資源不足** |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| 資源 | `Assets/Scripts/Economy/ResourceManager.cs` |
| Food 採集（参考） | `Assets/Scripts/Economy/FoodGatherManager.cs` |
| Berry Bush（参考） | `Assets/Scripts/Economy/BerryBushResource.cs` / `FoodNodeData.cs` |
| Wood 採集 | `Assets/Scripts/Economy/GatherManager.cs`（**変更禁止**） |
| 命令 | `Assets/Scripts/Commands/GameCommands.cs` |
| 入力 | `Assets/Scripts/Selection/SelectionManager.cs` |
| HUD | `Assets/Scripts/Selection/ResourceHudView.cs` / `CpuHudView.cs` |
| Editor | `Assets/Scripts/Editor/Phase1SceneBuilder.cs` / `Phase10SceneBuilder.cs` |
| パス | `Assets/Scripts/Core/GameAssetPaths.cs` |

---

## ③ Phase 20 目的

**Gold Mine / Stone Mine から採掘し TC に搬入** — AoE2 の採石場・金鉱と同様、マップ上のノードを村民が採掘する。Deposit 先は **TownCenter のみ**（Mining Camp は Phase 21 以降）。

Phase 20 完了 = **Milestone 2 Economy 完了**（Wood / Food / Gold / Stone の 4 資源が機能）。

### 変更後の経済（追加部分）

```
Gold Mine / Stone Mine 右クリック
    ↓ GatherGoldCommand / GatherStoneCommand
MineralGatherManager（Fixed Tick、FoodGatherManager と並列）
    MoveToMine → Gather → MoveToDeposit → ResourceManager.AddGold/AddStone
    ↓
HUD: Gold: N / Stone: N
```

Wood / Food ループは **一切 rewrite しない**。

### 今回実装するもの

1. **`ResourceManager` 拡張** — チーム別 Gold / Stone（`GetGold` / `AddGold` / `GetStone` / `AddStone`）。開始値 **0** でよい
2. **`MineralNodeData`**（ScriptableObject）— `mineralKind`（Gold / Stone）、`initialAmount`、色。または `GoldNodeData` + `StoneNodeData` を **別アセット**（small diff ならどちらでも可）
3. **`GoldMineResource` / `StoneMineResource`** — `BerryBushResource` パターン（`TakeMineral` / `IsDepleted` / Resource レイヤー）
4. **`MineralGatherManager`** — `ISimulationTickable`。Gold ジョブ + Stone ジョブ（FoodGatherManager の Berry + Farm 分離と同パターン可）
5. **`GatherGoldCommand` / `GatherStoneCommand`** — `IGameCommand`
6. **`SelectionManager` 拡張** — Resource レイヤー Raycast 優先順位に Gold / Stone を追加（Berry → Gold → Stone → Tree 等、**一貫した順序**）
7. **Command Cancel 相互排他** — Move / Attack / Gather / GatherFood / GatherFarmFood に Mineral Cancel を追加。Mineral Command は他 Cancel も呼ぶ
8. **HUD** — `ResourceHudView` に `Gold:` / `Stone:` 行。`CpuHudView` に CPU 表示。パネル高さ調整
9. **`Phase10SceneBuilder`** — Gold Mine / Stone Mine を Player / CPU 付近に **各 1〜2 個**配置、`MineralGatherManager` を Systems に追加、アセット生成
10. **README / `docs/IMPLEMENTATION_STATUS.md` / `03_M2_ECONOMY_PHASES.md` 更新** — **M2 Economy 完了**を明記

### 採掘パラメータ（MVP — Data で調整可）

| 項目 | Gold | Stone |
|------|------|-------|
| 初期储量 | **800** | **350** |
| 搬送上限 | **10** | **10** |
| 採集速度 | **2.5 / 秒** | **2.5 / 秒** |
| Deposit 先 | **TownCenter** | **TownCenter** |
| 枯渇 | ✅ 储量 0 で採集不可 | ✅ 同左 |
| 開始資源 | **0** | **0** |
| ビジュアル色 | 黄系 | 灰系 |

### 禁止（Phase 20 範囲外）

- **Mining Camp** / **Market** 建築（将来 Phase）
- Gold / Stone の **Drop-off 拠点**（Lumber Camp パターンは Phase 21 以降）
- Militia / Villager の Gold / Stone コスト
- `GatherManager` / `FoodGatherManager` の rewrite / 統合リファクタ
- 交易・文明ボーナス
- 新シーン `Phase20.unity`（**検証は `Phase10.unity` のみ**）
- CPU Gold / Stone 採集 AI（**任意**）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 20-1 | `ResourceManager` Gold/Stone + `MineralNodeData` + アセットパス |
| 20-2 | `GoldMineResource` / `StoneMineResource` + Editor 生成 |
| 20-3 | `MineralGatherManager` — Gold / Stone ジョブ |
| 20-4 | `GatherGoldCommand` / `GatherStoneCommand` + SelectionManager + Cancel |
| 20-5 | HUD + Phase10SceneBuilder + ドキュメント（M2 完了） |

各サブステップ後に **Wood / Food 採集が壊れていないこと** を確認すること。

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**（新規 / 変更 / 削除）
2. **Gold / Stone の Manager 共存方針**（1 Manager 2 ジョブリスト vs 2 Manager）
3. **Resource レイヤー Raycast 優先順位**
4. **GatherManager / FoodGatherManager 非変更の確認**
5. **影響範囲**（Command Queue 種類数 / HUD レイアウト）
6. **リスク**（Raycast 競合 / 枯渇 Tick）
7. **ロールバック方法**
8. **完了条件**（下記チェックリスト）
9. **テスト手順**

---

## ⑥ 実装後に必ず出力

1. **変更内容サマリ**
2. **変更ファイル一覧**
3. **Mineral 関連 API 一覧**
4. **テスト結果**
5. **既知の制限**（CPU 未対応 / Mining Camp なし 等）
6. **M2 Economy 完了宣言**

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### MineralGatherManager（FoodGatherManager コピー可）

```csharp
struct GoldGatherJob { Unit unit; GoldMineResource mine; GatherState state; float carried; }
struct StoneGatherJob { Unit unit; StoneMineResource mine; GatherState state; float carried; }
```

- Deposit は `ProductionManager.GetTownCenterForTeam` + `UnitPositionOffsets`（Food と同じ）
- Tick ループ中の **リスト一括削除禁止**（Phase 18 Farm 枯渇バグ参照）

### SelectionManager — Resource レイヤー（例）

```csharp
if (Physics.Raycast(ray, out hit, 1000f, GameLayers.ResourceMask))
{
    if (TryGatherBerry(...)) return;
    if (TryGatherGold(...)) return;
    if (TryGatherStone(...)) return;
    if (TryGatherWood(...)) return;
}
```

- 非戦闘ユニット（`!unit.CanAttack`）のみ採掘命令

### HUD レイアウト（例）

```
Wood: N
Food: N
Gold: N      ← 追加
Stone: N     ← 追加
Pop: N/M
[Build House ...]
...
```

### Phase10 マップ配置（例）

| 要素 | 推奨 |
|------|------|
| Player 付近 Gold | 1 個（TC から 15〜25m） |
| Player 付近 Stone | 1 個 |
| CPU 付近 | 各 1 個（任意） |

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`（主）
- **`AoE → Setup Phase10 Scene`** — 鉱山ノード配置 + `MineralGatherManager` + Data アセット
- Phase 1〜19 Setup メニューは壊さない

---

## ⑨ 完了条件（Phase 20 MVP = M2 完了）

- [ ] HUD に **Gold: N** / **Stone: N**（+ CPU 表示）
- [ ] **Gold Mine 右クリック** → 採集 → TC 搬入 → Gold 増加
- [ ] **Stone Mine 右クリック** → 採集 → TC 搬入 → Stone 増加
- [ ] ノード枯渇 → 採集不可
- [ ] **Wood / Food / Farm / Lumber Camp** が Phase 19 同様に動作（回帰）
- [ ] **Command Queue** 経由（`GatherGoldCommand` / `GatherStoneCommand`）
- [ ] Console に **Null 参照・例外なし**
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `03_M2_ECONOMY_PHASES.md` / `README.md` を **M2 完了**に更新

### Victory 確認について

Economy Phase では **毎回 Victory まで確認不要**。Phase 20 完了時は以下で十分:

- 今回の Gold / Stone 主パス
- Wood / Food スモーク回帰
- Console エラーなし

**Victory 全確認**は M2 完了後の push 前 **1 回**、または戦闘系 Phase 変更時のみ。

---

## ⑩ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene` → Play
2. HUD に **Gold: 0 / Stone: 0** が表示される
3. 村民 → **Gold Mine 右クリック** → TC 搬入 → **Gold 増加**
4. 村民 → **Stone Mine 右クリック** → TC 搬入 → **Stone 増加**
5. **木 / Berry / Farm** 採集が動作（回帰）
6. **Build Lumber Camp** → 森近く Wood 搬入（回帰）
7. 採集中 **地面右クリック** → キャンセル + 移動
8. Militia 生産・CPU 攻撃波が動作（スモーク）
9. Console エラーなし

Phase 20 のみ実装。Phase 21（Mining Camp 等）以降に触れない。

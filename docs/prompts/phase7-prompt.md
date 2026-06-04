# Phase 7 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜6 完了  
> **使い方:** `@CONSTITUTION.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 7 実装

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜6 は完了済み。Phase 7 のみ実装すること。**

---

## ① プロジェクト憲法（必読・遵守）

リポジトリの `CONSTITUTION.md` を読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止 / クリック時以外の Raycast 乱用禁止
- Manager 更新方式を維持（`UnitManager` が `TickMovement` を一括呼び出し。**戦闘 Tick も Manager 側**）
- **Unity アセット手書き禁止**: `*.inputactions`, `*.prefab`, `*.mat` 等は Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- `RTSInputActions` を Project-wide Input Actions にしない
- `MaterialPropertyBlock` 等の Unity オブジェクトを MonoBehaviour の**フィールド初期化子**で `new` しない
- Setup メニューは **Edit モード専用**（Play 中は実行不可）
- **OnGUI は左上原点、Input System Pointer は左下原点** — HUD ヒット判定は `GameUiInput.GuiRectToScreenRect` を使う

---

## ② Phase 1〜6 完了状態（現状）

動作確認済み（Phase6.unity）:

- RTS カメラ（TC 中心・俯瞰開始、`RTSCameraController.ApplyOverviewView`）
- 左クリック / ドラッグ / Shift 複数選択、右クリック移動・採集
- **TownCenter** 選択、**Q** / UI で Villager 生産（**3 秒**）
- **木材採集:** 1 満載（10 Wood）≈ **4 秒**、TC 搬入
- **House 建築:** 左上 **Build House (25 Wood)** → ゴースト → 地面左クリック → Villager 現場 → **3 秒**で完成
- **人口:** 初期 **Pop: 0/5**、House 完成 **+5**、上限時 TC 生産拒否
- 建築中: Villager が現場にいる間のみタイマー進行
- 建築中断: 右クリック移動 → 仮サイト削除（Wood 返金なし）
- `GameUiInput` による HUD / ワールド入力分離

**Phase 6 から Phase 7 以降へ回す既知課題（今回必須ではない）:**

- 建築中断時の **Wood 返金**
- 建築 **一時停止 → 後から Villager が戻って再開**（現状は中断 = 破棄）
- 複数 Villager スポーン位置の重なり
- House 破壊時の cap 減少
- Villager / Militia の **死亡・HP バー表示**（Phase 8）
- 遠距離攻撃・弓兵（Phase 7 範囲外）

主要ファイル（**実装前に必ず開いて読む**）:

- `Assets/Scripts/Buildings/BuildingPlacementManager.cs` / `House.cs` / `PlacedBuildingData.cs` / `RuntimeBuildingFactory.cs`
- `Assets/Scripts/Buildings/ProductionManager.cs` / `TownCenter.cs` / `BuildingData.cs`
- `Assets/Scripts/Buildings/ProductionPanelView.cs`（TC 生産 UI）
- `Assets/Scripts/Economy/PopulationManager.cs` / `ResourceManager.cs`
- `Assets/Scripts/Selection/SelectionManager.cs` / `ResourceHudView.cs` / `GameUiInput.cs`
- `Assets/Scripts/Units/Unit.cs` / `UnitData.cs` / `UnitManager.cs` / `UnitSpawner.cs`
- `Assets/Scripts/Editor/Phase6SceneBuilder.cs`
- `Assets/Scenes/Phase6.unity`

---

## ③ Phase 7 目的

**Barracks（兵舎）建築** と **Militia（民兵）生産・近接攻撃** により、軍事の開始。

### 今回実装するもの

1. **Barracks** 建築データ + 配置（House と同様のゴースト → 配置 → Villager 建築）
2. **Barracks** 選択時 UI — **Create Militia** ボタン（人口上限・Wood コスト考慮）
3. **Militia UnitData** — HP / Attack / Armor / 攻撃レンジ / 攻撃クールダウン
4. **AttackManager**（Manager 方式）— 攻撃命令・追跡・クールダウン Tick
5. **右クリック攻撃** — Militia 選択中、敵 Unit を右クリック → 移動して攻撃
6. **テスト用敵 Unit** — Phase7 シーンに 1〜2 体（Player とは別チーム、AI なし）
7. **Phase7 シーン** — `AoE → Setup Phase7 Scene` → `Assets/Scenes/Phase7.unity`

### ルール（MVP）

| 項目 | 推奨値（調整可） |
|------|------------------|
| Barracks コスト | Wood **50** |
| Barracks 建築時間 | **5 秒**（Villager 現場待機ルールは House 同様） |
| Barracks サイズ | footprint 6×6 程度（House より大きめ） |
| Militia 生産時間 | **3 秒** |
| Militia コスト | Wood **20**（または無料 — 実装前に README に明記） |
| Militia HP | **40** |
| Militia Attack | **4** |
| Militia Armor | **0** |
| 攻撃レンジ | 近接 **1.5**（Capsule 中心間） |
| 攻撃クールダウン | **1.0 秒** |
| ダメージ式 | `max(1, attack - targetArmor)` |
| 人口 | Militia も **1 人口**（`PopulationManager.CanTrainUnit()` 共通） |

- **Villager は攻撃不可**（`UnitData` に `canAttack` または `attack > 0` で判定）
- **Phase 7 では死亡しない** — HP は減るが 0 以下でも Destroy / Unregister しない（Phase 8）
- **敵は反撃しない**（Phase 7 は攻撃命令の検証のみ）
- Barracks 生産中も TC 生産と同様 **1 スロット**

### 建築フロー（Barracks）

```
Build Barracks ボタン（HUD）
  ↓ 配置モード（ゴースト）
左クリック確定（Wood 消費、重なりなし）
  ↓ Villager 現場移動 → 建築 Tick
Barracks 完成（Building レイヤー、選択可能）
```

- `BuildingPlacementManager` を **拡張**して Barracks 対応（rewrite 禁止）
- または配置種別 enum を追加する小 diff

### 攻撃フロー（MVP）

```
Militia 選択 → 敵 Unit 右クリック
  ↓ AttackManager.IssueAttack(selected, target)
射程外 → MoveTarget で接近
射程内 → AttackCooldown 毎にダメージ
ターゲット消失 / 移動命令 / 採集命令 → 攻撃解除
```

- `SelectionManager.HandleMoveCommand` — Unit レイヤー Raycast で**敵**を先に判定（Ground より優先）
- 味方 Unit 右クリックは従来通り（移動 or 無視 — 実装方針を統一）
- 攻撃中の移動は `UnitManager.TickMovement` と `AttackManager` の協調

### チーム（MVP）

- `UnitTeam` enum（`Player` / `Enemy`）を `Unit` または `UnitData` に追加
- 同チームは攻撃不可
- Phase7 シーン: 敵 1〜2 体を `(15, 1, 0)` 付近等に配置（Editor 生成）

### UI（MVP）

- HUD に **Build Barracks (50 Wood)** ボタン（House ボタンの下。パネル高さ・`GameUiInput` fallback を更新）
- Barracks 選択時パネル（`BarracksPanelView` または `ProductionPanelView` 拡張）— **Create Militia**
- 人口上限時は Militia 生産拒否 + 短いメッセージ
- Wood 不足時はボタン無効

### 禁止（Phase 7 範囲外）

- 死亡処理・HP バー・UnitState 機械（Phase 8）
- 遠距離攻撃・攻城
- 敵 AI・反撃
- Food / Gold / Stone
- Barracks 以外の兵舎・複数兵種
- `SelectionManager` / `UnitManager` / `BuildingPlacementManager` の rewrite（**拡張・小さな差分で**）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 7-1 | `UnitData` 拡張（attack / armor / attackRange / attackCooldown / canAttack or team）+ `MilitiaData` SO |
| 7-2 | `UnitTeam` + `Unit.TakeDamage`（Phase 8 死亡は未実装） |
| 7-3 | `BarracksData`（PlacedBuildingData 拡張 or 別 SO）+ `RuntimeBuildingFactory.CreateBarracks` |
| 7-4 | `BuildingPlacementManager` — Barracks 配置モード |
| 7-5 | `Barracks` コンポーネント + 選択（`SelectionManager` 拡張） |
| 7-6 | Barracks 生産（`ProductionManager` 拡張 or 専用 Manager）+ UI |
| 7-7 | `AttackManager` + `UnitManager` から Tick 呼び出し |
| 7-8 | `SelectionManager` — 右クリック攻撃命令 |
| 7-9 | `Phase7SceneBuilder`（敵 Unit 配置）+ README / `docs/PHASES.md` 更新 |

各サブステップ後に Play 可能な状態を維持すること。

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更ファイル一覧**（新規 / 変更 / 削除）
2. **影響範囲**（Buildings / Units / Combat / Selection / UI / Editor）
3. **パフォーマンス影響**（Update 数、GC、Raycast 回数）
4. **save / multiplayer 将来互換**（`AttackCommand` / `TrainUnitCommand` 化しやすい API か）

---

## ⑥ 実装後に必ず出力

1. **テスト手順**（チェックリスト 1〜12 程度）
2. **想定動作**
3. **残課題**（Phase 8 へ回すもの）
4. Unity メニュー手順（`AoE → Setup Phase7 Scene`）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

- Barracks 見た目: Cube（House より大きく、色を変える — 例: 灰赤）
- Militia 見た目: Capsule、色を Villager と区別（例: 赤系）
- 生産スポーン: Barracks 付近（`TownCenter.GetVillagerSpawnPosition` と同様の出口計算を Barracks に）
- `ProductionManager` は `TownCenter` 固定 — **Barracks 用に job に building 参照を一般化**するか、小さな `BarracksProductionManager` を分離（どちらも可、diff を小さく）
- 攻撃 Tick: `AttackManager.Update` で全攻撃ジョブを処理（Unit ごと Update 禁止）
- 移動と攻撃: 射程内なら `ClearMoveTarget` して攻撃、射程外なら `SetMoveTarget` で接近
- HUD パネル高さ変更時は `GameUiInput` の fallback `panelHeight` も更新
- Phase6 の `EnsureHouseData` パターンで `EnsureBarracksData` / `EnsureMilitiaData` を Editor に追加

---

## ⑧ シーン / Editor

- `Assets/Scenes/Phase7.unity` を新規作成（Phase6 と同配置 + 敵 Unit 1〜2）
- Phase1〜6 シーン・メニューは壊さない
- 初回 or ピンク地面時: `AoE → Fix Render Pipeline`

---

## ⑨ 完了条件（Phase 7 MVP）

- [ ] **Build Barracks** で Barracks を建築できる（Wood 消費・建築 5 秒）
- [ ] Barracks 選択 → **Create Militia** で Militia 生産（3 秒、人口・Wood 制限）
- [ ] Militia 選択 → 敵 Unit 右クリック → 接近して攻撃（クールダウン付き）
- [ ] ダメージが HP に反映される（Console ログ or 内部値 — HP バーは Phase 8）
- [ ] Villager は敵を右クリックしても攻撃しない（移動 or 無視）
- [ ] Phase 6 機能（Wood 採集・House・人口・TC 生産）が壊れていない
- [ ] Console にエラーなし

Phase 7 のみ実装。Phase 8 以降に触れない。

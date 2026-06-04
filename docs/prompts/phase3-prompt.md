# Phase 3 実行プロンプト

> **状態:** ✅ 完了

---

# 依頼: AoE RTS Engine — Phase 3 実装

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。
Low-Spec RTS（AoE2 インスパイア）。**Phase 1・2 は完了済み。Phase 3 のみ実装すること。**

---

## ① プロジェクト憲法（必読・遵守）

リポジトリの `CONSTITUTION.md` を読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止 / クリック時以外の Raycast 乱用禁止
- Manager 更新方式を維持（`UnitManager` が `TickMovement` を一括呼び出し）
- **Unity アセット手書き禁止**: `*.inputactions`, `*.prefab`, `*.mat` 等は Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- `RTSInputActions` を Project-wide Input Actions にしない
- `MaterialPropertyBlock` 等の Unity オブジェクトを MonoBehaviour の**フィールド初期化子**で `new` しない（Unity 6 で例外になる）

---

## ② Phase 1・2 完了状態（現状）

動作確認済み（Phase2.unity）:
- RTS カメラ（WASD / 端スクロール / ズーム）
- 地面 100×100
- 左クリック単体選択、左ドラッグ矩形選択、Shift 追加選択
- 右クリック移動（単体・グループ整列移動）
- SelectionBox 表示（OnGUI、Y 座標は Screen→GUI 変換済み）
- `UnitManager.Register` は `OnEnable` + `Start` の両方（実行順対策済み）
- `AoE → Setup Phase2 Scene` / Play でエラーなし

主要ファイル（**実装前に必ず開いて読む**）:
- `Assets/Scripts/Selection/SelectionManager.cs` — 複数 `Unit` 選択、ドラッグ、グループ移動
- `Assets/Scripts/Selection/SelectionBoxView.cs` — OnGUI 矩形（Y 反転済み）
- `Assets/Scripts/Selection/GroupMoveFormation.cs` — グリッド移動
- `Assets/Scripts/Units/Unit.cs` — 移動・選択色・`MaterialPropertyBlock` 遅延初期化
- `Assets/Scripts/Units/UnitManager.cs` — 登録リスト + 一括 Tick
- `Assets/Scripts/Units/UnitData.cs` — ScriptableObject（`DefaultUnit.asset` = Villager）
- `Assets/Scripts/Input/RTSInputReader.cs` / `RTSInputActionsBuilder.cs`
- `Assets/Scripts/Core/GameLayers.cs` — Ground / Unit レイヤーのみ
- `Assets/Scripts/Editor/Phase1SceneBuilder.cs` — 共有ヘルパー（public）
- `Assets/Scripts/Editor/Phase2SceneBuilder.cs`
- `Assets/Scenes/Phase2.unity`

---

## ③ Phase 3 目的

AoE らしい **経済の起点** として **TownCenter（市庁舎）から Villager（村人）を生産** できるようにする。

### 今回実装するもの

1. **TownCenter 建築物**（静止、選択可能、視覚的にユニットと区別）
2. **Building レイヤー**（`GameLayers` 拡張 + Editor でレイヤー登録）
3. **建築データ**（`BuildingData` 等の ScriptableObject — Editor `CreateAsset` で生成）
4. **生産キュー**（TownCenter 1 件あたり。MVP は **1 スロット** でよい）
5. **生産タイマー + スポーン**（時間経過後、TownCenter 付近に Villager を生成）
6. **生産 UI**（TownCenter 選択時に「Villager 生産」操作ができる最小 UI）
7. **ProductionManager**（建築ごとの `Update` 禁止。Manager が生産 Tick を一括処理）
8. **Phase3 シーン** — `AoE → Setup Phase3 Scene` → `Assets/Scenes/Phase3.unity`

### TownCenter の仕様（MVP）

- 見た目: Primitive（例: Cube 2×2×2 程度）+ URP Lit Material（Editor ランタイム生成、`.mat` 手書き禁止）
- 位置: 地面中央付近（例: `(0, 1, 0)`）
- Collider あり、**Building レイヤー**
- 左クリックで選択（ユニット選択と排他 — TownCenter 選択時はユニット選択解除、逆も同様）
- 選択時: 色変更または枠など視覚フィードバック（`MaterialPropertyBlock` は `Awake`/`UpdateVisual` 内で遅延初期化）

### Villager 生産の仕様（MVP）

- 生産時間: 固定値でよい（例: 5 秒。`BuildingData` または `UnitProductionEntry` で設定）
- **資源コストは Phase 4 以降** — 今回はコストチェックなし（常に生産可能）
- **人口上限は Phase 6 以降** — 今回は上限なし
- キュー: 1 件のみ（生産中は追加不可、またはキュー 1 件待ちまで — 実装者判断、どちらか一方で統一）
- スポーン位置: TownCenter 前方オフセット（例: `transform.position + transform.forward * 4f`、Y は Villager と同じ）
- スポーンした Villager は既存 `Unit` + `UnitData`（`DefaultUnit.asset`）を使用
- `UnitManager` に自動登録され、選択・移動が Phase 2 と同様に動くこと

### 生産 UI（MVP）

- TownCenter 選択中のみ表示
- **OnGUI 最小** またはランタイム生成 uGUI（手書き `.prefab` 禁止）
- 例: 「Create Villager (Q)」ボタン + 生産中プログレス表示（残り秒数でも可）
- 入力: ボタンクリック、またはキー `Q`（Input Actions 追加する場合は `RTSInputActionsBuilder` + `RTSInputActionsFactory` 経由のみ）

### 禁止（Phase 3 範囲外）

- 木材・食物・金などの資源システム（Phase 4）
- House 建築・配置（Phase 5）
- 人口上限（Phase 6）
- Barracks / 兵士 / 戦闘（Phase 7–8）
- CPU AI（Phase 9–10）
- NavMesh / 経路探索改善
- 複数建築タイプ（今回は TownCenter のみ）
- Rally Point 設定、生産キャンセル、返金
- `SelectionManager` / `UnitManager` の rewrite（**拡張・小さな差分で**）

---

## ④ 推奨実装順（30〜60分単位の小ステップ）

| サブステップ | 内容 |
|-------------|------|
| 3-1 | `Building` レイヤー + `GameLayers.BuildingMask` |
| 3-2 | `BuildingData` ScriptableObject + Editor で `TownCenterData.asset` 生成 |
| 3-3 | `TownCenter.cs`（選択状態、生産要求 API、スポーン位置） |
| 3-4 | `ProductionManager`（登録建築リスト + 生産 Tick + 完了時スポーン） |
| 3-5 | `SelectionManager` 拡張（建築クリック選択、ユニット選択との排他） |
| 3-6 | `ProductionPanelView`（OnGUI 等、TownCenter 選択時 UI） |
| 3-7 | `Phase3SceneBuilder` + メニュー `AoE → Setup Phase3 Scene` |
| 3-8 | `README.md` に Phase 3 操作を追記 |

各サブステップ後に Play 可能な状態を維持すること。

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更ファイル一覧**（新規 / 変更 / 削除）
2. **影響範囲**（Selection / Units / Buildings / Input / Editor）
3. **パフォーマンス影響**（Update 数、GC、Raycast 回数）
4. **save / multiplayer 将来互換**（`TrainVillagerCommand` 化しやすい API か）

---

## ⑥ 実装後に必ず出力

1. **テスト手順**（チェックリスト 1〜9 程度）
2. **想定動作**
3. **残課題**（Phase 4 へ回すもの）
4. Unity メニュー手順（`AoE → Setup Phase3 Scene`）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

- 建築生成: `Phase1SceneBuilder.CreateGround()` と同様、Editor 内 `GameObject.CreatePrimitive` + `AddComponent<TownCenter>()`
- Villager 生成: `Phase1SceneBuilder.CreateUnit(unitData, spawnPos)` を public 化して再利用可
- 生産 Tick: `ProductionManager.Update()` で全 TownCenter の残り時間を減算（建築数は少ないので OK。将来は Fixed Tick 化）
- 選択: `HandleClickSelect` で Unit Raycast → 失敗時 Building Raycast、または Building を先に判定（実装者判断、排他を守る）
- 矩形選択: TownCenter は 1 体なので矩形選択対象外でも可（MVP）。ユニット矩形選択は Phase 2 互換を維持
- `Unit.OnEnable` / `Start` の Register パターンを Villager スポーン後も踏襲

---

## ⑧ シーン / Editor

- `Assets/Scenes/Phase3.unity` を新規作成（`EditorSceneManager.SaveScene`）
- 初期配置例: **TownCenter 1 + 開始 Villager 0〜1 体**（9 体グリッドは Phase 3 では不要）
- Phase1 / Phase2 シーン・メニューは壊さない
- `Fix Phase1 Input References` は壊さない

---

## ⑨ 完了条件（Phase 3 MVP）

- [ ] Phase3 シーンに TownCenter が配置されている
- [ ] TownCenter を左クリックで選択できる（視覚フィードバックあり）
- [ ] TownCenter 選択中、UI から Villager 生産を開始できる
- [ ] 生産時間後、TownCenter 付近に Villager が出現する
- [ ] スポーンした Villager を選択・右クリック移動できる（Phase 2 機能）
- [ ] TownCenter 選択とユニット選択が排他的（同時選択しない）
- [ ] Console にエラーなし
- [ ] Phase 1・2 のカメラ / 選択 / 移動が壊れていない

Phase 3 のみ実装。Phase 4 以降に触れない。

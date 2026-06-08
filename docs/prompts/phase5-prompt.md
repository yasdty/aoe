# Phase 5 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜4 完了  
> **使い方:** `@CONSTITUTION.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 5 実装

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜4 は完了済み。Phase 5 のみ実装すること。**

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
- `MaterialPropertyBlock` 等の Unity オブジェクトを MonoBehaviour の**フィールド初期化子**で `new` しない
- Setup メニューは **Edit モード専用**（Play 中は実行不可）

---

## ② Phase 1〜4 完了状態（現状）

動作確認済み（Phase4.unity）:
- RTS カメラ（WASD / 端スクロール / ズーム。Play 開始直後 0.25s 端スクロール無効）
- 地面 100×100、URP 設定済み（`AoE → Fix Render Pipeline`）
- 左クリック / ドラッグ / Shift 複数選択、右クリック移動（グループ整列）
- **TownCenter** 選択、**Q** / UI で Villager 生産（5 秒）
- **木材採集:** 木右クリック → Gather → TC 搬入 → **Wood カウンタ増加**（左上 OnGUI）
- 採集中に地面右クリック → 移動命令に切り替え（Gather ジョブ解除）
- 地面 / Resource / Unit / Building レイヤー分離

**Phase 4 から Phase 5 へ回す既知課題（今回必須ではない）:**
- 複数 Villager スポーン時の位置重なり

**Phase 5 で必ず直す UX 課題（ユーザー報告・必須）:**
- **Play 開始時のカメラ:** 現状、TownCenter の**右上寄りでややズームイン**した状態で始まる
- **期待:** TC を画面中心付近に置き、**周囲の木・地面が見渡せる引きのズーム**で開始する
- Phase3 / Phase4 シーン builder および `RTSCameraController` の初期値を調整すること（全 Phase シーンで一貫した開始視点が望ましい）

主要ファイル（**実装前に必ず開いて読む**）:
- `Assets/Scripts/Camera/RTSCameraController.cs` — 移動・ズーム（Y = 高度）
- `Assets/Scripts/Buildings/TownCenter.cs` / `BuildingData.cs` / `ProductionManager.cs`
- `Assets/Scripts/Economy/ResourceManager.cs` / `GatherManager.cs` / `TreeResource.cs`
- `Assets/Scripts/Selection/SelectionManager.cs` — 右クリック命令の入口
- `Assets/Scripts/Units/Unit.cs` / `UnitManager.cs`
- `Assets/Scripts/Core/GameLayers.cs`
- `Assets/Scripts/Editor/Phase1SceneBuilder.cs` / `Phase4SceneBuilder.cs`
- `Assets/Scenes/Phase4.unity`

---

## ③ Phase 5 目的

**House（家）の建築配置** による建築システムの開始。あわせて **Play 開始カメラ** を改善する。

### 今回実装するもの

1. **House** 建築データ（ScriptableObject または `BuildingData` 拡張）
2. **建築配置モード** — UI ボタン（OnGUI 最小）→ ゴーストプレビュー → 左クリックで配置確定
3. **コスト:** Wood **25**（`ResourceManager` から減算。不足時は配置不可）
4. **建築フロー:** 配置確定 → 選択 Villager が現場へ移動 → 到達後 **BuildTime 10 秒** → House 完成
5. **BuildingPlacementManager**（または同等）— 配置モード・建築中 Tick（Manager 方式）
6. **Phase5 シーン** — `AoE → Setup Phase5 Scene` → `Assets/Scenes/Phase5.unity`
7. **Play 開始カメラ修正** — TC 中心・全体俯瞰ズーム（§② 必須）

### 建築フロー（MVP）

```
（常時）Wood 採集・TC 生産は Phase 4 互換
  ↓ UI「Build House」
配置モード（ゴーストがマウス位置に追従、Ground Raycast）
  ↓ 左クリック（Wood >= 25、重なりなし）
Wood 消費 → 建築サイト確定 → 選択 Villager に MoveToBuildSite
  ↓ 到達
Building（10 秒タイマー、Villager はその場待機 or 簡易「作業中」）
  ↓ 完了
House 出現（Building レイヤー、Collider あり）
```

- **配置可否:** 他 Building / Tree と重ならない（簡易 AABB または Physics.OverlapBox）
- **建築者:** 選択中 Villager（複数可 — MVP は 1 体でも可、方針を統一）
- **建築中キャンセル:** Phase 5 必須ではない

### 入力・命令

- **配置モード中:** 左クリック = 配置確定、Esc または右クリック = キャンセル（推奨）
- **配置モード中:** 通常のユニット選択は無効化 or 最小限（実装者判断、衝突を避ける）
- Phase 2〜4 の移動・採集・生産は**壊さない**

### UI（MVP）

- **Build House** ボタン（OnGUI。TownCenter 選択時 or 常時表示 — 実装者判断）
- Wood 不足時はボタン無効 or メッセージ
- 既存 **Wood: N** 表示は維持

### 禁止（Phase 5 範囲外）

- 人口上限（Phase 6）
- Barracks / 兵士（Phase 7）
- Food / Gold / Stone
- LumberCamp
- 建築の回転・複数建築タイプ（House のみ）
- NavMesh / 経路探索改善
- Villager スポーン重なりの完全解消（任意）
- `SelectionManager` / `GatherManager` の rewrite（**拡張・小さな差分で**）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 5-0 | **Play 開始カメラ修正**（TC 中心・俯瞰ズーム。SceneBuilder + 必要なら `RTSCameraController` 初期化） |
| 5-1 | `HouseData` / `BuildingData` 拡張（Wood コスト、BuildTime、サイズ） |
| 5-2 | `ResourceManager.TrySpendWood`（または同等） |
| 5-3 | 配置モード + ゴーストプレビュー（Ground Raycast） |
| 5-4 | `BuildingPlacementManager` — 配置確定・建築 Tick・House 生成 |
| 5-5 | Villager 移動 → 現場到達 → 建築待機 |
| 5-6 | `BuildHousePanelView`（OnGUI）+ `SelectionManager` 最小拡張 |
| 5-7 | `Phase5SceneBuilder` + README / `docs/01_M0_POC_PHASES.md` 更新 |

各サブステップ後に Play 可能な状態を維持すること。**5-0 は最初に着手推奨**（UX 改善が独立しているため）。

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更ファイル一覧**（新規 / 変更 / 削除）
2. **影響範囲**（Buildings / Economy / Selection / Camera / Editor）
3. **パフォーマンス影響**（Update 数、GC、Raycast 回数）
4. **save / multiplayer 将来互換**（`PlaceBuildingCommand` / `SpendWoodCommand` 化しやすい API か）

---

## ⑥ 実装後に必ず出力

1. **テスト手順**（チェックリスト 1〜10 程度。カメラ開始位置を含む）
2. **想定動作**
3. **残課題**（Phase 6 へ回すもの）
4. Unity メニュー手順（`AoE → Setup Phase5 Scene`）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

- House 見た目: Cube Primitive + Editor 生成（`.prefab` 手書き禁止）
- ゴースト: 半透明 Material または `MaterialPropertyBlock` で色変更（既存 `SceneMaterialFactory` 再利用）
- 配置 Raycast: `GameLayers.GroundMask`（Resource / Building と競合しない）
- 建築 Tick: `BuildingPlacementManager.Update()` で進行中サイトのみ（数が少ないので OK）
- Wood コスト: 配置**確定時**に即減算（AoE2 ライク。返金は Phase 5 不要）
- カメラ: `Phase4SceneBuilder` は camera `(18, 22, 18)` 固定 — TC `(0,0,0)` に対し斜め上オフセットのため「右上・やや近い」に見える。**注視点を TC 付近にし、Y（高度）を上げる**か、起動時 `FocusOn(Vector3)` を 1 回呼ぶ方式が有効
- Phase3 / Phase4 / Phase5 の Setup を再実行したときも同じ開始視点になるよう SceneBuilder で統一

---

## ⑧ シーン / Editor

- `Assets/Scenes/Phase5.unity` を新規作成
- 初期配置: **TownCenter 1 + Tree 5〜10 + 開始 Villager 0**（Phase4 と同様で可）
- Phase1〜4 シーン・メニューは壊さない
- 初回 or ピンク地面時: `AoE → Fix Render Pipeline`

---

## ⑨ 完了条件（Phase 5 MVP）

- [ ] **Play 開始時、TC が画面中心付近で周囲が見渡せるズームになっている**
- [ ] Phase5 シーンで Wood を採集できる（Phase 4 互換）
- [ ] UI から House 配置モードに入れる
- [ ] Wood 25 以上で地面クリック → 配置確定・Wood 減少
- [ ] Villager が現場へ移動し、10 秒後に House が完成する
- [ ] Wood 不足・重なり時は配置できない
- [ ] TownCenter 生産・木材採集・地面移動が壊れていない
- [ ] Console にエラーなし

Phase 5 のみ実装。Phase 6 以降に触れない。

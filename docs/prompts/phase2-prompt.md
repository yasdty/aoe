# Phase 2 実行プロンプト

> **状態:** ✅ 実行済み  
> **前提:** Phase 1 完了  
> **使い方:** `@CONSTITUTION.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 2 実装

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。
Low-Spec RTS（AoE2 インスパイア）。**Phase 1 は完了済み。Phase 2 のみ実装すること。**

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

---

## ② Phase 1 完了状態（現状）

動作確認済み:
- RTS カメラ（WASD / 端スクロール / ズーム）
- 地面 100×100、ユニット 1 体（Capsule）
- 左クリック単体選択、右クリック移動
- `AoE → Fix Phase1 Input References` / Play でエラーなし

主要ファイル（**実装前に必ず開いて読む**）:
- `Assets/Scripts/Selection/SelectionManager.cs` — 単体選択のみ（`Unit selectedUnit`）
- `Assets/Scripts/Units/UnitManager.cs` — 登録リスト + 一括 Tick
- `Assets/Scripts/Units/Unit.cs` — `SetSelected`, `SetMoveTarget`, `TickMovement`
- `Assets/Scripts/Input/RTSInputReader.cs` — Select / Command / PointerPosition
- `Assets/Scripts/Input/RTSInputActionsBuilder.cs` — 入力定義
- `Assets/Scripts/Camera/RTSCameraController.cs`
- `Assets/Scripts/Core/GameLayers.cs` — Ground / Unit レイヤー
- `Assets/Scripts/Editor/Phase1SceneBuilder.cs` — シーン構築（拡張可）
- `Assets/Scenes/Phase1.unity`

---

## ③ Phase 2 目的

AoE らしい **複数ユニット操作** を追加する。

### 今回実装するもの

1. **ドラッグ選択**（画面上の矩形）
2. **複数選択**（複数ユニット同時選択）
3. **Shift 追加選択**（既存選択に追加 / トグル）
4. **グループ移動**（複数選択中に右クリック → 整列移動）
5. **SelectionBox UI**（ドラッグ矩形の表示）
6. **UnitManager 拡張**（選択・配置計算に必要なら。既存の移動 Tick は維持）

### 移動（グループ）

- 複数選択状態で地面を右クリック
- 到達点を中心に、選択ユニットを **3×3 / 4×4 等のグリッド** で配置して移動
- 例: 4 体 → 2×2、9 体 → 3×3（人数に応じて近い正方形グリッドでよい）
- 各ユニットは既存 `Unit.SetMoveTarget` を使用（直線移動のまま）

### 禁止（Phase 2 範囲外）

- フォーメーション最適化（動的リフォーム、回転フォーメーション等）
- 回避処理 / 群衆回避 / Flow Field / NavMesh
- 建築・資源・AI・戦闘・文明差
- Phase 1 機能の rewrite
- `SelectionManager` の全置き換え（**拡張・小さな差分で**）

### 入力の想定（既存 Input Actions を拡張する場合）

- 左クリック: 単体選択（空クリックで全解除 — Phase 1 互換）
- 左ドラッグ: 矩形選択
- Shift + 左クリック / Shift + ドラッグ: 追加選択
- 右クリック: グループ移動命令

`RTSInputActions` を変更する場合は `RTSInputActionsBuilder` + `RTSInputActionsFactory` 経由のみ（JSON 手書き禁止）。

SelectionBox UI は **uGUI 最小**（1 Image または GL/OnGUI でも可。手書き `.prefab` 禁止 → Editor で Canvas 生成 or ランタイム生成）。

---

## ④ 推奨実装順（30〜60分単位の小ステップ）

Phase 2 を一度に巨大実装しない。可能なら順に:

| サブステップ | 内容 |
|-------------|------|
| 2-1 | `SelectionManager` を複数 `Unit` 保持に変更（リスト化） |
| 2-2 | ドラッグ矩形の開始/更新/終了 + Unit レイヤー Raycast（矩形内判定） |
| 2-3 | `SelectionBoxView`（UI 矩形表示） |
| 2-4 | Shift 追加選択 |
| 2-5 | グループ移動（グリッドオフセット計算 → 各 `SetMoveTarget`） |
| 2-6 | `Phase1SceneBuilder` → **Setup Phase2 Scene**（ユニット複数体配置）または Phase1 シーン拡張 |

各サブステップ後に Play 可能な状態を維持すること。

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更ファイル一覧**（新規 / 変更 / 削除）
2. **影響範囲**（Selection / Unit / Input / Editor）
3. **パフォーマンス影響**（Raycast 回数、GC、Update 数）
4. **save / multiplayer 将来互換**（command 化しやすい API か）

---

## ⑥ 実装後に必ず出力

1. **テスト手順**（チェックリスト）
2. **想定動作**
3. **残課題**（Phase 3 へ回すもの）
4. Unity メニュー手順（例: `AoE → Setup Phase2 Scene` があれば）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

- 矩形選択: スクリーン矩形 ↔ 地面平面への投影、または Unit の `Renderer.bounds` / Collider を Camera 投影で AABB 判定
- Raycast は **ドラッグ終了時・クリック時** に限定（毎フレーム全 Unit Raycast 禁止）
- `UnitManager` に全 Unit リストがある → 選択ループに利用可
- 選択状態は `Unit.SetSelected(true/false)` を一括で呼ぶ
- グループ移動オフセット例: `spacing = 2f`、グリッド index から `worldOffset` を加算して `SetMoveTarget(center + offset)`

---

## ⑧ シーン / Editor

- `Assets/Scenes/Phase1.unity` を拡張するか `Phase2.unity` を新規作成するかは実装者判断（Editor `SaveScene` で生成）
- テスト用に **少なくとも 9 体程度** のユニットが配置できること
- `Fix Phase1 Input References` は壊さないこと

---

## ⑨ 完了条件（Phase 2 MVP）

- [ ] ドラッグで複数ユニット選択できる
- [ ] Shift で選択追加できる
- [ ] 複数選択が視覚的に分かる（色変更）
- [ ] 複数選択中の右クリックでグリッド整列移動する
- [ ] SelectionBox がドラッグ中に表示される
- [ ] Console にエラーなし
- [ ] Phase 1 の WASD / 端スクロール / ズームが壊れていない

Phase 2 のみ実装。Phase 3 以降に触れない。

# Phase 4 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜3 完了  
> **使い方:** `@CONSTITUTION.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 4 実装

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜3 は完了済み。Phase 4 のみ実装すること。**

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

## ② Phase 1〜3 完了状態（現状）

動作確認済み（Phase3.unity）:
- RTS カメラ（WASD / 端スクロール / ズーム。Play 開始直後の端スクロール・ズーム暴れ対策済み）
- 地面 100×100、URP 設定済み（`AoE → Fix Render Pipeline`）
- 左クリック / ドラッグ / Shift 複数選択、右クリック移動（グループ整列）
- **TownCenter** 選択（Building レイヤー）、**Q** / UI で Villager 生産（5 秒）
- 生産 Villager は TownCenter **カメラ側**にスポーン（建物と重ならない）
- Phase 2 の選択・移動は Villager にも適用

**Phase 3 から Phase 4 へ回す既知課題（今回必須ではない）:**
- 複数 Villager スポーン時の位置重なり（次 Phase で調整可）

主要ファイル（**実装前に必ず開いて読む**）:
- `Assets/Scripts/Buildings/TownCenter.cs` — 生産・スポーン位置
- `Assets/Scripts/Buildings/ProductionManager.cs`
- `Assets/Scripts/Buildings/BuildingData.cs`
- `Assets/Scripts/Units/Unit.cs` / `UnitManager.cs` / `UnitSpawner.cs`
- `Assets/Scripts/Selection/SelectionManager.cs` — 右クリック命令の入口
- `Assets/Scripts/Core/GameLayers.cs` — Ground / Unit / Building
- `Assets/Scripts/Editor/Phase1SceneBuilder.cs` / `Phase3SceneBuilder.cs`
- `Assets/Scripts/Editor/RenderPipelineSetup.cs` / `SceneMaterialFactory.cs`
- `Assets/Scenes/Phase3.unity`

---

## ③ Phase 4 目的

**木材（Wood）採集** による経済ループの開始。

### 今回実装するもの

1. **Tree / ResourceNode**（地图上の木。採集可能、储量あり）
2. **GatherTask / Villager 作業状態**（Move → Gather → Carry → Deposit）
3. **右クリックで木を指定** → Villager が移動して採集開始
4. **Wood 所持・TownCenter への搬入** → プレイヤー Wood 資源が増える
5. **ResourceManager**（資源カウント。Manager 方式）
6. **作業 Tick**（`GatherManager` 等。Unit ごと Update 禁止）
7. **Phase4 シーン** — `AoE → Setup Phase4 Scene` → `Assets/Scenes/Phase4.unity`

### Villager 状態機械（MVP）

```
Idle / Move（既存）
  ↓ 木を右クリック
MoveToTree
  ↓ 到達
Gather（タイマー。木の Wood を減算）
  ↓ 満載 or 木が空
MoveToDeposit（TownCenter）
  ↓ 到達
Deposit（Wood を ResourceManager に加算、所持量クリア）
  ↓
Idle
```

- _carry 上限: 固定値（例: 10 Wood）
- 採集速度: 固定値（例: 1 Wood / 秒）
- **Deposit 先:** TownCenter（Phase 4 では TC のみ）

### 入力・命令

- **木を右クリック:** 選択中 Villager（複数可）に採集命令
  - 移動命令（地面右クリック）と排他ルールを整理（例: 木レイヤー Raycast を先に判定）
- 地面右クリックは従来どおり移動（Phase 2 互換）

### UI（MVP）

- 画面に **Wood: N** 表示（OnGUI 最小で可）
- 所持中 Wood を Villager 上に表示するのは任意（Phase 4 必須ではない）

### 禁止（Phase 4 範囲外）

- Food / Gold / Stone
- House / 建築配置（Phase 5）
- 人口上限（Phase 6）
- LumberCamp 建築（Phase 5 以降で可。今回は Tree を直置き）
- 採集アニメーション
- NavMesh / 経路探索改善
- Villager スポーン位置の完全最適化（重なり解消は次 Phase 可）
- `Unit` / `SelectionManager` の rewrite（**拡張・小さな差分で**）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 4-1 | Tree 配置（Resource レイヤー、`ResourceNode` ScriptableObject、Editor で数本配置） |
| 4-2 | `ResourceManager`（Wood カウント）+ UI 表示 |
| 4-3 | Villager 状態 + `GatherManager` Tick（Gather / Carry / Deposit） |
| 4-4 | `SelectionManager` 拡張（木への右クリック Gather 命令） |
| 4-5 | `Phase4SceneBuilder`（TC + Trees + 開始 Villager 0〜1） |
| 4-6 | `README.md` / `docs/PHASES.md` 更新 |

各サブステップ後に Play 可能な状態を維持すること。

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更ファイル一覧**（新規 / 変更 / 削除）
2. **影響範囲**（Units / Resources / Selection / Editor）
3. **パフォーマンス影響**（Update 数、GC、Raycast 回数）
4. **save / multiplayer 将来互換**（`GatherCommand` / `DepositCommand` 化しやすい API か）

---

## ⑥ 実装後に必ず出力

1. **テスト手順**（チェックリスト 1〜9 程度）
2. **想定動作**
3. **残課題**（Phase 5 へ回すもの）
4. Unity メニュー手順（`AoE → Setup Phase4 Scene`）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

- Tree 見た目: Cylinder / Capsule Primitive + Editor 生成（`.prefab` 手書き禁止）
- Resource レイヤー追加: `GameLayers` + `TagManager`（Editor `EnsureLayers`）
- 木 Raycast: 専用 Resource レイヤーマスク。Unit / Building / Ground と競合しない判定順
- Gather Tick: `GatherManager.Update()` でアクティブタスクのみ処理
- 複数 Villager → 同一 Tree: MVP は許可（同時採集）または 1 体のみ — どちらかに統一
- TownCenter Deposit: TC `Collider` 距離判定 or 既存 Building レイヤー Raycast
- Phase3 の `UnitSpawner` / `CreateUnit` パターンを再利用

---

## ⑧ シーン / Editor

- `Assets/Scenes/Phase4.unity` を新規作成
- 初期配置例: **TownCenter 1 + Tree 5〜10 本**（TC 近くとやや離れた木）
- Phase1〜3 シーン・メニューは壊さない
- 初回 or ピンク地面時: `AoE → Fix Render Pipeline`

---

## ⑨ 完了条件（Phase 4 MVP）

- [ ] Phase4 シーンに Tree と TownCenter がある
- [ ] Villager を選択し、木を右クリックすると採集を開始する
- [ ] 採集 → TownCenter 搬入 → **Wood カウンタが増える**
- [ ] 木の储量が減り、枯渇すると採集できない
- [ ] 地面右クリック移動（Phase 2）が壊れていない
- [ ] TownCenter 生産（Phase 3）が壊れていない
- [ ] Console にエラーなし

Phase 4 のみ実装。Phase 5 以降に触れない。

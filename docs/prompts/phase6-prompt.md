# Phase 6 実行プロンプト

> **状態:** 📋 未実行（プロンプト作成済み）  
> **前提:** Phase 1〜5 完了  
> **使い方:** `@CONSTITUTION.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 6 実装

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜5 は完了済み。Phase 6 のみ実装すること。**

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
- **OnGUI は左上原点、Input System Pointer は左下原点** — HUD ヒット判定は `GameUiInput.GuiRectToScreenRect` を使う

---

## ② Phase 1〜5 完了状態（現状）

動作確認済み（Phase5.unity）:

- RTS カメラ（TC 中心・俯瞰開始、`RTSCameraController.ApplyOverviewView`）
- 左クリック / ドラッグ / Shift 複数選択、右クリック移動・採集
- **TownCenter** 選択、**Q** / UI で Villager 生産（**3 秒**）
- **木材採集:** 1 満載（10 Wood）≈ **4 秒**、TC 搬入
- **House 建築:** 左上 **Build House (25 Wood)** → ゴースト → 地面左クリック → Villager 現場 → **3 秒**で完成
- 建築中: Villager が現場にいる間のみタイマー進行
- 建築中に右クリック移動 → **建築中断**（仮サイト削除。**Wood 返金なし** — Phase 5 MVP）
- 配置モード: Esc / 右クリックでキャンセル（ゴーストのみ）
- `GameUiInput` による HUD / ワールド入力分離

**Phase 5 から Phase 6 以降へ回す既知課題（今回必須ではない）:**

- 建築中断時の **Wood 返金**
- 建築 **一時停止 → 後から Villager が戻って再開**（現状は中断 = 破棄）
- 複数 Villager スポーン位置の重なり
- House 完成時の **人口加算**（Phase 6 で実装）

主要ファイル（**実装前に必ず開いて読む**）:

- `Assets/Scripts/Buildings/BuildingPlacementManager.cs` / `House.cs` / `PlacedBuildingData.cs`
- `Assets/Scripts/Buildings/ProductionManager.cs` / `TownCenter.cs` / `BuildingData.cs`
- `Assets/Scripts/Economy/ResourceManager.cs` / `GatherManager.cs`
- `Assets/Scripts/Selection/ResourceHudView.cs` / `GameUiInput.cs` / `SelectionManager.cs`
- `Assets/Scripts/Units/UnitManager.cs`
- `Assets/Scripts/Editor/Phase5SceneBuilder.cs`
- `Assets/Scenes/Phase5.unity`

---

## ③ Phase 6 目的

**人口（Population）システム** により RTS としての基本制約を導入する。

### 今回実装するもの

1. **PopulationManager**（現在人口 / 上限の管理。Manager 方式）
2. **初期値:** 人口 **5 / 5**（TC 相当の初期 Housing。Villager 0 体スタートでも cap は 5）
3. **House 完成時:** 上限 **+5**（完成した `House` ごと。建築中は加算しない）
4. **人口 UI:** 画面左上に `Pop: 3/10` 等（OnGUI、`ResourceHudView` 拡張 or 別 View）
5. **生産制限:** 人口 ≧ 上限のとき **Villager 生産不可**（TownCenter Q / UI ボタン無効 + 試行拒否）
6. **Phase6 シーン** — `AoE → Setup Phase6 Scene` → `Assets/Scenes/Phase6.unity`

### ルール（MVP）

| 項目 | 値 |
|------|-----|
| 初期上限 | 5 |
| House 完成 | 上限 +5 |
| 現在人口 | 生存中 Villager 数（`UnitManager` から集計） |
| 上限超過時 | TC 生産不可（House 建築は Phase 5 通り Wood のみ） |

- Villager 死亡システムは Phase 8 — 今回は **Unregister されない限り** 全 Unit を Villager としてカウントで可
- House 破壊 — Phase 6 範囲外（上限は減らさない）

### UI（MVP）

- `Wood: N` の下または横に **`Pop: current/max`**
- 上限時 TC パネルに「Population full」等の短い表示（任意）

### 禁止（Phase 6 範囲外）

- Barracks / 兵士（Phase 7）
- 戦闘・死亡（Phase 8）
- Food / Gold / Stone
- 建築中断の返金・再開
- `ProductionManager` / `BuildingPlacementManager` の rewrite（**拡張・小さな差分で**）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 6-1 | `PopulationManager`（current / max、House 完成フック） |
| 6-2 | `UnitManager` または Manager から Villager 数集計 |
| 6-3 | `House` 完成時 `PopulationManager.AddHousing(5)`（`CompleteConstruction` から呼ぶ） |
| 6-4 | `ProductionManager` / `TownCenter` — 上限時生産拒否 |
| 6-5 | `ResourceHudView` または `PopulationHudView` — Pop 表示 |
| 6-6 | `Phase6SceneBuilder` + README / `docs/PHASES.md` 更新 |

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更ファイル一覧**（新規 / 変更 / 削除）
2. **影響範囲**（Buildings / Economy / Production / UI / Editor）
3. **パフォーマンス影響**（Update 数、GC）
4. **save / multiplayer 将来互換**（`PopulationChangedEvent` 化しやすい API か）

---

## ⑥ 実装後に必ず出力

1. **テスト手順**（チェックリスト 1〜10 程度）
2. **想定動作**
3. **残課題**（Phase 7 へ回すもの）
4. Unity メニュー手順（`AoE → Setup Phase6 Scene`）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

- 初期 Housing 5 は `PopulationManager` の `[SerializeField] int initialHousingCap = 5` または定数
- House +5 は `PlacedBuildingData.housingProvided = 5`（SO 拡張）でも可
- 完成フック: `BuildingPlacementManager.CompleteConstruction` → `RuntimeBuildingFactory.CreateHouse` 後に通知
- 生産拒否: `TownCenter.TryQueueVillagerProduction` または `ProductionManager.TryQueueProduction` 入口で `PopulationManager.CanTrainUnit()` チェック
- Phase5 の `Phase5SceneBuilder.CreateManagers` をコピー拡張して Phase6 用に

---

## ⑧ シーン / Editor

- `Assets/Scenes/Phase6.unity` を新規作成（Phase5 と同配置で可）
- Phase1〜5 シーン・メニューは壊さない
- 初回 or ピンク地面時: `AoE → Fix Render Pipeline`

---

## ⑨ 完了条件（Phase 6 MVP）

- [ ] ゲーム開始時 **Pop: 0/5**（Villager 0 体の場合）
- [ ] Villager を 5 体まで TC 生産できる
- [ ] 6 体目は **生産不可**（UI 無効 + 拒否）
- [ ] House 完成後 **上限 +5**（例: 5 体 + House 1 → **0/10** で 5 体まで追加生産可）
- [ ] Wood 採集・House 建築・移動（Phase 5）が壊れていない
- [ ] Console にエラーなし

Phase 6 のみ実装。Phase 7 以降に触れない。

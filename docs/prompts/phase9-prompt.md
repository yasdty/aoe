# Phase 9 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜8 完了  
> **使い方:** `@CONSTITUTION.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 9 実装

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜8 は完了済み。Phase 9 のみ実装すること。**

---

## ① プロジェクト憲法（必読・遵守）

リポジトリの `CONSTITUTION.md` を読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止 / クリック時以外の Raycast 乱用禁止
- Manager 更新方式を維持（`UnitManager` / `GatherManager` / `ProductionManager` 等）
- **Unity アセット手書き禁止**: Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- Setup メニューは **Edit モード専用**
- **OnGUI は左上原点、Input System Pointer は左下原点** — `GameUiInput.GuiRectToScreenRect`

---

## ② Phase 1〜8 完了状態（現状）

動作確認済み（Phase8.unity）:

- **プレイヤー経済** — Wood 採集、House（25 Wood）、Pop 上限、TC Villager 生産（3 秒・Wood コストなし）
- **Barracks / Militia** — 建築 50 Wood、生産 20 Wood、右クリック近接攻撃
- **戦闘** — HP ≦ 0 で死亡、`UnitState`、選択ユニット HP バー、攻撃中オレンジ tint
- **UnitTeam** — `Player` / `Enemy`。自軍のみ選択可（`SelectionManager.IsPlayerUnit`）
- **テスト敵** — Enemy Dummy 1〜2 体（AI なし、反撃なし）

### 現状の単一プレイヤー前提（Phase 9 で拡張が必要）

| システム | 現状 | Phase 9 で必要なこと |
|---------|------|---------------------|
| `ResourceManager` | Wood 1 プール | **チーム別 Wood**（Player / CPU） |
| `PopulationManager` | Player 人口のみ | **チーム別 Pop / Cap** |
| `GatherManager` | 最初の `TownCenter` へ搬入 | **ユニットの Team に応じた TC へ搬入** |
| `ProductionManager` | 全 TC 共通 Pop チェック、Spawn は Player 固定 | **TC の Team で Pop / Spawn** |
| `BuildingPlacementManager` | プレイヤー UI ゴースト配置のみ | **CPU 用プログラム配置 API** |
| `TownCenter` / `House` | Team 属性なし | **Team 所有**（選択不可・色分け） |

**Phase 8 から Phase 9 以降へ回す既知課題（今回必須ではない）:**

- 建築中断時の Wood 返金・建築再開
- House 破壊時の cap 減少
- CPU 軍事 AI（Barracks / Militia / 攻撃波）→ **Phase 10**
- 複数 CPU チーム、Food / Gold
- 経路探索・フォーメーション最適化

主要ファイル（**実装前に必ず開いて読む**）:

- `Assets/Scripts/Economy/ResourceManager.cs` / `GatherManager.cs` / `PopulationManager.cs`
- `Assets/Scripts/Buildings/ProductionManager.cs` / `TownCenter.cs` / `BuildingPlacementManager.cs`
- `Assets/Scripts/Units/Unit.cs` / `UnitTeam.cs` / `UnitManager.cs` / `UnitSpawner.cs`
- `Assets/Scripts/Selection/SelectionManager.cs` / `ResourceHudView.cs`
- `Assets/Scripts/Editor/Phase8SceneBuilder.cs`
- `Assets/Scenes/Phase8.unity`

---

## ③ Phase 9 目的

**CPU 経済 AI** — 人間プレイヤーと同一マップ上で、CPU が木採集・House 建築・Villager 生産を自動実行する。

### 今回実装するもの

1. **チーム別経済** — `ResourceManager` / `PopulationManager` を `UnitTeam` 対応（既存 Player API は後方互換）
2. **Team 所有建築** — `TownCenter`（+ 必要なら `House`）に `UnitTeam` を持たせ、Gather / Production が参照
3. **Gather 搬入先修正** — CPU Villager の Wood は CPU TC へ、Player は Player TC へ
4. **CPU 建築 API** — ゴースト UI なしで House を配置開始（既存 `ConstructionSite` ロジック再利用）
5. **`CpuEconomyAiManager`** — Manager 方式の簡易経済 AI（1 チームのみ）
6. **Phase9 シーン** — Player TC + CPU TC、共有の木、CPU 初期 Villager
7. **最小デバッグ表示** — CPU Wood / Pop（画面右上 OnGUI 等。プレイヤー HUD は従来どおり）

### CPU チーム定義（MVP）

| 項目 | 値 |
|------|-----|
| 人間 | `UnitTeam.Player` |
| CPU | `UnitTeam.Enemy`（Phase 10 の軍事 AI も同チーム想定） |
| Phase 9 シーン | **Enemy Dummy 戦闘用ユニットは置かない**（Phase 8 シーンで戦闘テスト継続） |

### CPU 初期配置（Phase9 シーン）

| 要素 | 推奨 |
|------|------|
| Player TC | マップ中央付近 `(0, 0, 0)` — Phase 8 と同様 |
| CPU TC | 反対側 `(0, 0, -35)` 付近（色を変えて識別） |
| CPU 初期 Villager | **3 体**（TC 付近スポーン、`UnitTeam.Enemy`） |
| Player 初期 Villager | **0 体**（プレイヤーは手動生産 — Phase 8 と同様） |
| 木 | 両陣営から届く位置に Phase 8 同数配置（共有資源） |
| カメラ | 俯瞰で両 TC が見える位置 |

### CPU AI 行動（MVP — 単純なルールベースで可）

**評価間隔:** 約 **2 秒**ごと（毎フレーム不要）

```
1. アイドル CPU Villager → 最寄りの未枯渇 Tree へ Gather 命令
2. CPU Pop ≧ Cap かつ Wood ≧ 25 かつ House 建築中でない
   → CPU TC 近くの有効タイルに House 配置（1 体の Villager を builder に）
3. CPU Pop < Cap かつ CPU TC が生産中でない
   → Villager 生産キュー（Wood コストなし — 現行仕様）
4. 目標 Villager 数（例: 6 体）まで 3 を繰り返し
```

- **House 配置位置** — CPU TC から半径 8〜20、他建築・サイトと `MinSiteSeparation` 抵触しない格子（既存 `CanPlaceAt` 再利用）
- **Builder 選定** — アイドル Villager のうち TC に最も近い 1 体
- **Gather 中 / 建築中 Villager** — AI が上書き命令しない（ジョブ中はスキップ）
- **Barracks / Militia** — Phase 9 範囲外（CPU は経済のみ）

### プレイヤー操作との分離

- プレイヤーは **CPU ユニット・CPU TC を選択不可**（既存 `IsPlayerUnit` + TC 選択フィルタ拡張）
- プレイヤーの Wood / Pop HUD は **Player チームのみ**（CPU 値は別表示 or デバッグ行）
- 木は **共有** — 先に切った側が取得（競合 OK）

### 禁止（Phase 9 範囲外）

- CPU 軍事（Barracks 建築、Militia 生産、攻撃命令、攻撃波）
- 敵 Dummy の Phase 9 シーン配置
- `GatherManager` / `ProductionManager` / `BuildingPlacementManager` の rewrite（**拡張・小 diff**）
- NavMesh / 経路探索ライブラリ導入
- 4 チーム対応（将来用コメント程度は可）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 9-1 | `ResourceManager` — `UnitTeam` 別 Wood（`Wood` プロパティは Player 向け後方互換） |
| 9-2 | `PopulationManager` — チーム別 Current / Max / `CanTrainUnit(team)` / `AddHousing(team, n)` |
| 9-3 | `TownCenter` に `UnitTeam` + `UnitManager` で TC 列挙 or `FindTownCenterForTeam` |
| 9-4 | `GatherManager` — 搬入先を `job.unit.Team` の TC に変更 |
| 9-5 | `ProductionManager` — Pop チェック・Spawn を TC の Team に合わせる |
| 9-6 | `BuildingPlacementManager` — `TryStartCpuConstruction(...)`（Wood 消費・サイト追加・builder 移動） |
| 9-7 | House 完成時 `AddHousing(builder.Team, ...)` |
| 9-8 | `SelectionManager` — CPU TC / Barracks / House 選択不可 |
| 9-9 | `CpuEconomyAiManager` — 上記 AI ルール |
| 9-10 | `Phase9SceneBuilder` + README / `docs/PHASES.md` 更新 |

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更ファイル一覧**（新規 / 変更 / 削除）
2. **影響範囲**（Economy / Buildings / AI / Selection / Editor）
3. **後方互換** — Phase 1〜8 シーンが壊れないか（Player 専用 API 維持）
4. **パフォーマンス** — AI 評価間隔、TC 検索方法（キャッシュ推奨）

---

## ⑥ 実装後に必ず出力

1. **テスト手順**（チェックリスト 1〜12 程度）
2. **想定動作**（Play 5〜10 分で CPU が House + Villager 増加）
3. **残課題**（Phase 10 へ回すもの）
4. Unity メニュー手順（`AoE → Setup Phase9 Scene`）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### ResourceManager 拡張例

```csharp
// 後方互換: Wood => GetWood(UnitTeam.Player)
public static float GetWood(UnitTeam team);
public static void AddWood(UnitTeam team, float amount);
public static bool TrySpendWood(UnitTeam team, float amount);
```

### GatherManager

- `Awake` の `FindAnyObjectByType<TownCenter>()` は Player 固定になっている — **削除 or フォールバックのみ**
- 搬入: `GetTownCenterForTeam(unit.Team)` — 初回はリストキャッシュ、`TownCenter.OnEnable` で Register パターンが望ましい

### ProductionManager

- `UnitSpawner.Spawn(..., townCenter.Team)` を必ず渡す
- `PopulationManager.CanTrainUnit(townCenter.Team)` に変更

### CPU 建築

- プレイヤー: `EnterHousePlacementMode` → クリック → `TryConfirmPlacement`
- CPU: 座標決定済み → `TrySpendWood(Enemy, 25)` → `sites.Add` + builder `SetMoveTarget`（既存 `TickConstructionSites` をそのまま利用）

### CpuEconomyAiManager

- `MonoBehaviour` 1 つ、`Update` 内で `evaluateTimer -= deltaTime`
- CPU TC / Villager 参照は Start 時キャッシュ + 死亡時再取得
- `UnitManager.CopyUnitsTo` で Enemy Villager をフィルタ

### ビジュアル

- CPU TC / Villager — `UnitTeam.Enemy` の色（`UnitData` / `BuildingData` の defaultColor 差し替え or `MaterialPropertyBlock`）
- CPU House — 色を少し変えて識別（任意）

### Phase9SceneBuilder

- `Phase8SceneBuilder` をコピー拡張
- `CreateEnemyUnits` は **呼ばない**
- `CreateCpuTownCenter` + 初期 Villager 3 体
- カメラは両陣営が見える overview（中点 `(0,0,-17)` 等）

---

## ⑧ シーン / Editor

- `Assets/Scenes/Phase9.unity` を新規作成
- Phase 1〜8 シーン・メニューは壊さない
- 初回 or ピンク地面時: `AoE → Fix Render Pipeline`

---

## ⑨ 完了条件（Phase 9 MVP）

- [ ] **Player 操作** — Phase 8 と同様（採集・House・TC 生産・Barracks・Militia・攻撃・死亡）が壊れていない
- [ ] **CPU TC + Villager 3 体** がマップ反対側に存在する
- [ ] CPU Villager が **自動で木を切り CPU Wood が増える**（デバッグ HUD or Console）
- [ ] CPU Wood ≧ 25 かつ Pop 上限到達後、**CPU が House を建築**する
- [ ] House 完成で **CPU Pop 上限 +5**、続けて Villager 生産が行われる
- [ ] CPU Wood は **Player Wood に混ざらない**
- [ ] CPU 採集 Wood は **CPU TC に搬入**される（Player TC へ入らない）
- [ ] プレイヤーは **CPU ユニット / CPU TC を選択できない**
- [ ] Play **5〜10 分** で CPU Villager 数が増加傾向（目安 5 体以上）
- [ ] Console にエラーなし

Phase 9 のみ実装。Phase 10（CPU 軍事・攻撃波）に触れない。

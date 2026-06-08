# Phase 14 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜13 完了（PoC + Foundation Phase 11〜13）  
> **ロードマップ:** [02_M1_FOUNDATION_PHASES.md](../02_M1_FOUNDATION_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 14 実装（Spatial Hash）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜13 は完了済み。Phase 14 のみ実装すること。**

---

## ① Foundation 方針（必読・遵守）

[02_M1_FOUNDATION_PHASES.md](../02_M1_FOUNDATION_PHASES.md) の最重要方針を厳守:

- **AoE 機能を増やさない** — Archer / Food / Age Up 等は禁止
- **small diff** — 1 Phase = 1 目的（Spatial Hash のみ）
- **既存ゲームを壊さない** — 完了時 `Phase10.unity` でコアループ + **Victory / Defeat** + **UnitPool** + **Benchmark** が動くこと
- **Simulation 優先** — 探索経路の置き換え。Unit 移動ロジック自体の rewrite は禁止

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / **Unit ごとの Update 乱立禁止**
- Manager 更新方式を維持
- **Unity アセット手書き禁止**: Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- Setup メニューは **Edit モード専用**
- **`GetInstanceID()` 禁止**

---

## ② Phase 13 完了状態（現状）

`Phase10.unity` / `Benchmark.unity` で動作確認済み:

- **Victory / Defeat** — TC 破壊で終了 UI
- **Object Pool** — `UnitPool` / `BuildingPool`
- **Benchmark** — `AoE → Setup Benchmark Scene`、50〜800 体 Idle 計測 HUD
- **Phase 10 コアループ** — 採集・建築・生産・CPU 経済 / 軍事 AI

### 現状のギャップ（Phase 14 で解消）

| 項目 | 現状 | 計算量 |
|------|------|--------|
| CPU 攻撃目標 | `CpuMilitaryAiManager.FindNearestPlayerUnit` — 全 Unit 線形走査 | O(n) |
| CPU 経済 — 村人選択 | `FindNearestCpuVillagerToTownCenter` — 全 Unit 走査 | O(n) |
| CPU 経済 — 木探索 | `FindNearestAvailableTree` — 全 Tree 走査 | O(t) |
| CPU 経済 — 順位付き木 | `FindRankedAvailableTree` — **二重ループ** | **O(t²)** |
| 矩形選択 | `SelectionManager.ApplyBoxSelection` — 全 Unit をスクリーン投影判定 | O(n) |
| Tree キャッシュ | `FindObjectsByType<TreeResource>()` — シーン全体検索（起動時キャッシュ） | 初回 O(t) |

**Spatial Hash 未導入** — [IMPLEMENTATION_STATUS.md](../IMPLEMENTATION_STATUS.md) §10「リスト線形探索」がボトルネック候補として記載済み。

主要ファイル（**実装前に必ず開いて読む**）:

- `Assets/Scripts/Units/UnitManager.cs`（Register / Unregister / `TickMovement`）
- `Assets/Scripts/Units/Unit.cs`（`OnEnable` / `OnDisable` / 移動）
- `Assets/Scripts/AI/CpuMilitaryAiManager.cs`（`FindNearestPlayerUnit`）
- `Assets/Scripts/AI/CpuEconomyAiManager.cs`（`FindNearestCpuVillagerToTownCenter` / `FindNearestAvailableTree` / `FindRankedAvailableTree`）
- `Assets/Scripts/Selection/SelectionManager.cs`（`ApplyBoxSelection`）
- `Assets/Scripts/Economy/TreeResource.cs`（`IsDepleted` / 枯渇）
- `Assets/Scripts/Benchmark/BenchmarkSpawner.cs`（800 体ストレステスト）
- `Assets/Scripts/Editor/Phase10SceneBuilder.cs`

---

## ③ Phase 14 目的

**O(n²) / 全件走査探索を Spatial Hash 経由に置き換え** — 200〜800 ユニット規模で AI・採集探索の CPU 負荷を下げる。

### 今回実装するもの

1. **`SpatialHashGrid<T>`** または **`UnitSpatialIndex` + `TreeSpatialIndex`** — 2D グリッド（XZ 平面）
2. **登録 / 更新 / 解除** — Unit の Register・移動・Pool Return / Tree の Register・枯渇
3. **クエリ API** — 最近傍 Unit（チームフィルタ）、半径内 Unit、最近傍 Tree、矩形内 Unit（選択用）
4. **既存 FindNearest 置き換え** — 下記「必須置き換え一覧」
5. **`SpatialHashGrid` を Systems に配置**（または static Manager）
6. **README / `docs/02_M1_FOUNDATION_PHASES.md` 更新**

### 必須置き換え一覧（MVP）

| 呼び出し元 | 現メソッド | 置き換え先（例） |
|-----------|-----------|-----------------|
| `CpuMilitaryAiManager` | `FindNearestPlayerUnit` | `UnitSpatialIndex.FindNearest(position, UnitTeam.Player)` |
| `CpuEconomyAiManager` | `FindNearestCpuVillagerToTownCenter` | `UnitSpatialIndex.FindNearestCpuVillager(...)` または Grid + フィルタ |
| `CpuEconomyAiManager` | `FindNearestAvailableTree` | `TreeSpatialIndex.FindNearest(position)` |
| `CpuEconomyAiManager` | `FindRankedAvailableTree` | Grid から k 近傍取得、または **O(t²) 二重ループを削除** |
| `SelectionManager` | `ApplyBoxSelection` 内の全 Unit 走査 | ワールド AABB / スクリーン矩形に対応する **セル範囲クエリ** + 投影判定 |

### グリッド設計（MVP 指針）

| 項目 | 推奨 |
|------|------|
| 平面 | **XZ**（Y は無視） |
| セルサイズ | 8〜16（Inspector 調整可。Unit 密度に合わせる） |
| Unit 更新タイミング | `UnitManager.TickMovement` 後、またはセル境界を跨いだときのみ `UpdatePosition` |
| Pool Return | `OnDisable` / Return 前に **Unregister** |
| Tree 枯渇 | `TreeResource` 枯渇時 **Unregister**（Register は Awake / シーン構築時） |
| 最近傍探索 | クエリセルから **同心円にセルリングを拡大** 直到着（全グリッド走査禁止） |

### 禁止（Phase 14 範囲外）

- Fixed Tick / Command Queue（Phase 15〜16）
- 新ユニット種別・新資源
- `UnitManager.Update` の移動ロジック rewrite
- Physics.Overlap への全面置換（Raycast クリック攻撃は現状維持）
- Octree / R-Tree 等の過剰抽象化
- Benchmark シーンの FPS 最適化自体（計測のみ。Hash 導入後の Before/After は任意ログ）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 14-1 | `SpatialHashGrid<T>` — Insert / Remove / Update / QueryRadius / QueryNearest |
| 14-2 | `UnitSpatialIndex` — UnitManager と連携（Register / 移動更新 / Unregister） |
| 14-3 | `TreeSpatialIndex` — TreeResource 登録・枯渇解除 |
| 14-4 | `CpuMilitaryAiManager` / `CpuEconomyAiManager` 置き換え |
| 14-5 | `SelectionManager.ApplyBoxSelection` 置き換え |
| 14-6 | Phase10 + Benchmark 回帰確認、ドキュメント更新 |

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**（新規 / 変更 / 削除）
2. **新規追加ファイル一覧**
3. **Grid のセルサイズと更新タイミング**
4. **各 Query API のシグネチャ**
5. **FindRankedAvailableTree の O(t²) 解消方針**
6. **影響範囲**（AI / Selection / UnitManager / TreeResource）
7. **リスク**（Pool 再利用時の二重 Register、移動中セルずれ、枯渇 Tree 残留）
8. **ロールバック方法**
9. **完了条件**（下記チェックリスト）
10. **テスト手順**

---

## ⑥ 実装後に必ず出力

1. **変更内容サマリ**
2. **変更ファイル一覧**
3. **置き換えた探索箇所一覧**（旧 O(n) → Grid 経由）
4. **テスト結果**（Phase10 コアループ + Benchmark 800）
5. **既知の制限**（AttackManager 内部走査は未対象 等）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### SpatialHashGrid 例

```csharp
public class SpatialHashGrid<T> where T : class
{
    public void Insert(T item, Vector3 worldPosition) { }
    public void Remove(T item) { }
    public void Update(T item, Vector3 worldPosition) { }
    public void QueryNearest(Vector3 origin, float maxRadius, Func<T, bool> filter, List<T> results) { }
}
```

- キーは **オブジェクト参照**（`GetInstanceID()` 禁止 → `Dictionary<T, CellKey>` 等）
- 同一セルに複数 Entity を `List<T>` で保持

### Unit 連携例

```csharp
// UnitManager.Register 内
UnitSpatialIndex.Register(unit);

// TickMovement 後（セル変更時のみ）
UnitSpatialIndex.UpdatePosition(unit, unit.transform.position);

// Unit.OnDisable / Pool Return 前
UnitSpatialIndex.Unregister(unit);
```

### FindRankedAvailableTree の簡略案

- Grid で origin から半径を広げ、k 件収集 → rank 番目を返す
- k が不足時は `FindNearestAvailableTree` にフォールバック

### Phase10SceneBuilder

```csharp
GameObject spatialObject = new GameObject("UnitSpatialIndex");
spatialObject.transform.SetParent(systems.transform);
spatialObject.AddComponent<UnitSpatialIndex>();
// TreeSpatialIndex 同様
```

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`（コアループ）+ `Benchmark.unity`（800 体ストレス）
- **`AoE → Setup Phase10 Scene`** — Spatial Index を Systems に追加
- **`AoE → Setup Benchmark Scene`** — 同上（Benchmark 用 Systems にも追加）
- Phase 1〜10 Setup メニューは壊さない

---

## ⑨ 完了条件（Phase 14 MVP）

- [ ] `SpatialHashGrid`（または同等）が **Unit / Tree** で使用されている
- [ ] **必須置き換え一覧** の 5 箇所が Grid 経由（線形全件走査を削除）
- [ ] `FindRankedAvailableTree` の **O(t²) 二重ループが解消** されている
- [ ] Pool Return / Unit 死亡後に Grid から **残留参照なし**
- [ ] **Phase10** — 採集・CPU 攻撃波・矩形選択・Victory / Defeat が動作
- [ ] **Benchmark 800** — クラッシュ / 例外なし
- [ ] Console に **Null 参照・二重 Register エラーなし**
- [ ] `docs/02_M1_FOUNDATION_PHASES.md` Phase 14 を ✅ に更新

---

## ⑩ テスト手順（Play チェックリスト）

### Phase10

1. `AoE → Setup Phase10 Scene` → Play
2. 矩形選択で Villager 複数選択
3. 木を右クリック採集
4. CPU 攻撃波（30 秒）で Militia が Player ユニット / TC を攻撃
5. CPU が木を採集・House 建築・村人再生産
6. 敵 TC 破壊 → **VICTORY**
7. Console エラーなし

### Benchmark（任意）

8. `Benchmark.unity` → **800** スポーン → FPS / 例外なし

Phase 14 のみ実装。Phase 15 以降（Fixed Tick 等）に触れない。

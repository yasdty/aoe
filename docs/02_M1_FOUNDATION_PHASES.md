# AoE RTS Engine — Milestone 1: Foundation Phase ロードマップ（Phase 11〜16）

> **Milestone:** M1 — Foundation（RTS Engine 基盤）✅ **完了**  
> **前提:** [01_M0_POC_PHASES.md](01_M0_POC_PHASES.md)（Phase 1〜10.5）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **次（未着手）:** `03_M2_ECONOMY_PHASES.md`（Phase 17〜20）

---

## このフェーズ群の目的

PoC 完成後、**AoE2 機能追加ではなく RTS Engine 基盤**へ進化させる。

| 目標 | 内容 |
|------|------|
| パフォーマンス | Pool / Spatial Hash / Benchmark |
| 拡張性 | Command Queue / Fixed Tick |
| 将来のマルチプレイ | Deterministic Simulation 準備 |
| Replay 対応 | Fixed Tick + Command 基盤 |

**検証シーン:** 各 Phase 完了時に `Phase10.unity` で採集・建築・生産・戦闘・CPU が動作すること。

---

## 最重要方針

### 1. AoE 機能を増やさない（Foundation 完了まで）

**禁止:** Archer / Cavalry / Spearman / Food / Gold / Stone / Farm / Age Up / Technology / Civilization

### 2. small diff

- 1 Phase = 1 目的
- 同時に複数システムを作らない

### 3. 既存ゲームを壊さない

毎 Phase 完了時に `Phase10.unity` でコアループが動作すること。

### 4. Simulation 優先

見た目より Simulation を改善する（Phase 10.5 の Visual は例外）。

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 10.5 | Visual Placeholder Upgrade | GLB 差し替え（モチベーション） | ✅ 完了 |
| 11 | Victory & Defeat | ゲーム終了可能にする | ✅ 実装済み |
| 12 | Object Pool | 大量ユニット生成に備える | ✅ 実装済み |
| 13 | Benchmark Infrastructure | 性能可視化 | ✅ 実装済み |
| 14 | Spatial Hash | O(n²) 探索削減 | ✅ 実装済み |
| 15 | Fixed Tick | Simulation 安定化 / Replay 準備 | ✅ 実装済み |
| 16 | Command Queue | Replay / Lockstep 準備 | ✅ 実装済み |

---

## Phase 10.5 — Visual Placeholder Upgrade ✅

**目的:** 開発モチベーション向上。Capsule / Cube / Cylinder → 低ポリ GLB（または Editor Mesh Prefab）。

**制約:** Animator / Animation / VFX 禁止。ロジック変更禁止。

**セットアップ:** `AoE → Setup Phase10.5 Scene`

---

## Phase 11 — Victory & Defeat

**目的:** ゲームを終了可能にする。

**実装:**

- Building HP（TownCenter 必須。House / Barracks はデータ準備可）
- TownCenter 破壊
- Victory / Defeat UI

**勝利条件:** Enemy TownCenter 破壊  
**敗北条件:** Player TownCenter 破壊

**完了条件:** ゲーム終了画面が表示される。

**プロンプト:** [prompts/phase11-prompt.md](prompts/phase11-prompt.md)

---

## Phase 12 — Object Pool ✅

**目的:** 大量ユニット生成に備える。

**実装:** `UnitPool` / `BuildingPool`（Villager / Militia / House / Barracks）

**方針:** 死亡・ Despawn は `Destroy()` 禁止 → `ReturnToPool()`（TownCenter / Tree は除外）

**完了条件:** 生産・死亡で新規 `Instantiate` 数が大幅減少。Console に `UnitPool: spawn=X reuse=Y` ログ。

**プロンプト:** [prompts/phase12-prompt.md](prompts/phase12-prompt.md)

---

## Phase 13 — Benchmark Infrastructure ✅

**目的:** 性能を可視化する。

**実装:** `Benchmark.unity` / `BenchmarkSpawner` / `BenchmarkMetricsView`

**規模:** 50 / 100 / 200 / 500 / 800 Unit（OnGUI ボタン）

**表示:** FPS / FrameTime / GC

**セットアップ:** `AoE → Setup Benchmark Scene`

**完了条件:** ワンクリックで負荷試験可能。

**プロンプト:** [prompts/phase13-prompt.md](prompts/phase13-prompt.md)

## Phase 14 — Spatial Hash ✅

**目的:** O(n²) 探索削減。

**実装:** `SpatialHashGrid` / `UnitSpatialIndex` / `TreeSpatialIndex`

**対象:** CPU 最近傍 Unit / Tree 探索、矩形選択候補クエリ

**完了条件:** 必須 5 箇所の全件走査を Grid 経由に置き換え。

**プロンプト:** [prompts/phase14-prompt.md](prompts/phase14-prompt.md)

---

## Phase 15 — Fixed Tick ✅

**目的:** Simulation 安定化。Replay 準備。

**実装:** `SimulationTick`（20 TPS）+ `ISimulationTickable`

**Tick 化:** Attack / Gather / Production / Construction / AI / Unit 移動  
**非 Tick 化:** Camera / UI / Input

**セットアップ:** `AoE → Setup Phase10 Scene`

**完了条件:** Simulation が Fixed Tick 上で動作。

**プロンプト:** [prompts/phase15-prompt.md](prompts/phase15-prompt.md)

---

## Phase 16 — Command Queue ✅

**目的:** Replay / Lockstep 準備。

**現状:** Input → Manager 直接実行  
**変更後:** Input → Command → Queue → Simulation

**Command 例:** Move / Attack / Gather / Build / Train

**注意:** Manager を直接呼ばず Command 経由。

**完了条件:** プレイヤー操作が全て Command になる。

**プロンプト:** [prompts/phase16-prompt.md](prompts/phase16-prompt.md)

## Foundation Milestone 1 完了条件

以下を全て満たした時点で **Milestone 1 完了**:

- [x] Victory / Defeat 実装済み
- [x] Pool 導入済み
- [x] Benchmark 導入済み
- [x] Spatial Hash 導入済み
- [x] Fixed Tick 導入済み
- [x] Command Queue 導入済み

---

## Foundation 完了後（Phase 17〜 — 現時点では実装禁止）

| Phase | 候補 |
|-------|------|
| 17 | Food |
| 18 | Farm |
| 19 | Lumber Camp |
| 20 | Gold & Stone |
| 21 | Archer |
| 22 | Spearman |
| 23 | Cavalry |
| 24 | Counter System |
| 25 | Auto Aggro |
| 26 | Formation |

---

## Cursor への共通指示

各 Phase 開始前に必ず出力:

1. 変更対象ファイル一覧
2. 新規追加ファイル一覧
3. 影響範囲
4. リスク
5. ロールバック方法
6. 完了条件
7. テスト手順

実装後は:

1. 変更内容サマリ
2. 変更ファイル一覧
3. テスト結果
4. 既知の制限

**small diff を厳守すること。**

---

## プロンプト一覧（Foundation）

| Phase | ファイル | 状態 |
|-------|----------|------|
| 11 | [prompts/phase11-prompt.md](prompts/phase11-prompt.md) | ✅ |
| 12 | [prompts/phase12-prompt.md](prompts/phase12-prompt.md) | ✅ |
| 13 | [prompts/phase13-prompt.md](prompts/phase13-prompt.md) | ✅ |
| 14 | [prompts/phase14-prompt.md](prompts/phase14-prompt.md) | ✅ |
| 15 | [prompts/phase15-prompt.md](prompts/phase15-prompt.md) | ✅ |
| 16 | [prompts/phase16-prompt.md](prompts/phase16-prompt.md) | ✅ |

---

## 進め方

1. [CONSTITUTION.md](../CONSTITUTION.md) を読ませる
2. [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) を読ませる
3. 本ファイルで Phase 全体像を把握させる
4. 該当 Phase の [prompts/phaseN-prompt.md](prompts/) を渡す
5. 実装前設計 → small diff → `Phase10.unity` で Play 確認 → 次 Phase

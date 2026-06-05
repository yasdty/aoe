# Phase 15 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜14 完了（PoC + Foundation Phase 11〜14）  
> **ロードマップ:** [FOUNDATION_PHASES.md](../FOUNDATION_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/RTS_IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 15 実装（Fixed Tick）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜14 は完了済み。Phase 15 のみ実装すること。**

---

## ① Foundation 方針（必読・遵守）

[FOUNDATION_PHASES.md](../FOUNDATION_PHASES.md) の最重要方針を厳守:

- **AoE 機能を増やさない** — Archer / Food / Age Up 等は禁止
- **small diff** — 1 Phase = 1 目的（Fixed Tick のみ）
- **既存ゲームを壊さない** — 完了時 `Phase10.unity` でコアループ + **Victory / Defeat** + Pool + Spatial Hash が動くこと
- **Simulation 優先** — Replay / Determinism 準備。見た目 polish 禁止

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止
- Manager 更新方式を維持（**Update → Fixed Tick 駆動に置き換え**）
- **Unity アセット手書き禁止**: Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- Setup メニューは **Edit モード専用**
- **`GetInstanceID()` 禁止**
- **`Time.deltaTime` を Simulation 内で直接使わない** — Tick 経由の `fixedDeltaTime`

---

## ② Phase 14 完了状態（現状）

`Phase10.unity` / `Benchmark.unity` で動作確認済み:

- **Victory / Defeat** — TC 破壊で終了 UI
- **Object Pool** — `UnitPool` / `BuildingPool`
- **Benchmark** — FPS / FrameTime / GC HUD
- **Spatial Hash** — `UnitSpatialIndex` / `TreeSpatialIndex`、CPU 探索・矩形選択 Grid 化
- **Phase 10 コアループ** — 採集・建築・生産・CPU 経済 / 軍事 AI

### 現状のギャップ（Phase 15 で解消）

| 項目 | 現状 |
|------|------|
| Simulation 更新 | 各 Manager が **`Update()` + `Time.deltaTime`** で可変フレームレート駆動 |
| Tick 統一 | **なし** — フレーム落ちで Simulation 速度が変わる |
| Replay 準備 | Command 記録基盤なし（Phase 16） |
| Determinism | float + deltaTime 依存で再現性低 |

**Simulation を駆動している Manager（`Update` あり — 実装前に必ず確認）:**

- `UnitManager` — 移動 `TickMovement`
- `AttackManager` — 攻撃ジョブ
- `GatherManager` — 採集ジョブ
- `ProductionManager` / `BarracksProductionManager` — 生産タイマー
- `BuildingPlacementManager` — 建築進行
- `CpuEconomyAiManager` / `CpuMilitaryAiManager` — AI 評価・攻撃波
- `GameSessionManager` — 勝敗（Tick 化は **不要**。状態フラグのみ）

**非 Tick 化（現状維持）:**

- `SelectionManager` / 各 HUD View — Input / OnGUI
- `RTSCameraController` / `RTSInputReader`
- `VictoryDefeatHudView` — 表示 + R キー再起動
- `BenchmarkMetricsView` — 計測表示

主要ファイル（**実装前に必ず開いて読む**）:

- 上記 Simulation Manager 全て
- `Assets/Scripts/Core/GameSessionManager.cs`
- `Assets/Scripts/Units/UnitManager.cs`
- `Assets/Scripts/Editor/Phase10SceneBuilder.cs`
- `Assets/Scripts/Editor/BenchmarkSceneBuilder.cs`

---

## ③ Phase 15 目的

**Simulation を Fixed Tick 上で動かす** — 可変 `Update` から独立し、Replay / Lockstep の土台を作る。

### 今回実装するもの

1. **`SimulationTick`**（または `SimulationClock`）— 固定 TPS ドライバ（**推奨 20 TPS**。Inspector で 20 / 30 切替可）
2. **Tick ループ** — フレーム内で `while (accumulator >= tickInterval)` により複数 Tick 消化（最大 Tick 上限でスパイラル防止）
3. **Manager Tick 化** — `Update()` の Simulation 処理を `TickSimulation(float fixedDeltaTime)` 等に移行
4. **`Phase10SceneBuilder` / `BenchmarkSceneBuilder`** — `SimulationTick` を Systems に追加
5. **README / `docs/FOUNDATION_PHASES.md` 更新**

### Tick 化対象（MVP — 必須）

| Manager | 現状 | Phase 15 |
|---------|------|----------|
| `UnitManager` | `Update` → 全 Unit 移動 | Tick 駆動 |
| `AttackManager` | `Update` → 攻撃処理 | Tick 駆動 |
| `GatherManager` | `Update` → 採集 | Tick 駆動 |
| `ProductionManager` | `Update` → TC 生産 | Tick 駆動 |
| `BarracksProductionManager` | `Update` → Militia 生産 | Tick 駆動 |
| `BuildingPlacementManager` | `Update` → 建築 | Tick 駆動 |
| `CpuEconomyAiManager` | `Update` → 2 秒評価 | Tick 駆動（**評価間隔は Tick カウントで維持**） |
| `CpuMilitaryAiManager` | `Update` → 攻撃波 + 2 秒評価 | Tick 駆動 |

### 非 Tick 化（触らない）

- Input / Selection / Camera / UI / Benchmark 計測 HUD

### 設計指針（MVP）

| 項目 | 推奨 |
|------|------|
| TPS | **20**（1 Tick = 0.05s）。30 も Inspector 選択可 |
| 時間源 | `Time.unscaledDeltaTime` で accumulator（Pause / スローモーション将来用） |
| スパイラル防止 | 1 フレームあたり最大 Tick 数（例: 5） |
| Game Over | `GameSessionManager.IsGameOver` 中は **Tick 停止** |
| AI 間隔 | `EvaluateInterval = 2f` → `EvaluateIntervalTicks = 40`（20 TPS 時） |
| 攻撃波 | `attackWaveIntervalSeconds = 30f` → Tick 換算 |

### 禁止（Phase 15 範囲外）

- Command Queue / Replay 記録（Phase 16）
- 新ユニット種別・新資源
- Unit コンポーネントへの `Update` 追加
- 全プロジェクトの `Time.deltaTime` 一括置換（**Simulation Manager のみ**）
- Physics / NavMesh 導入
- Fixed Tick と無関係な最適化

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 15-1 | `SimulationTick` — accumulator / maxTicksPerFrame / TPS 設定 |
| 15-2 | `ISimulationTickable` または static 登録リストで Tick 配信 |
| 15-3 | `UnitManager` + `AttackManager` Tick 化 |
| 15-4 | `GatherManager` + Production 系 Tick 化 |
| 15-5 | `BuildingPlacementManager` + CPU AI Tick 化 |
| 15-6 | SceneBuilder 更新 + Phase10 回帰 + ドキュメント |

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**（新規 / 変更 / 削除）
2. **新規追加ファイル一覧**
3. **Tick 配信方式**（interface / delegate / 明示 Register）
4. **各 Manager の Tick 換算表**（秒 → Tick 数）
5. **影響範囲**（UI は非 Tick の理由）
6. **リスク**（1 フレーム複数 Tick / AI タイミングずれ / 移動速度体感）
7. **ロールバック方法**
8. **完了条件**（下記チェックリスト）
9. **テスト手順**

---

## ⑥ 実装後に必ず出力

1. **変更内容サマリ**
2. **変更ファイル一覧**
3. **TPS 設定値と Tick 換算例**
4. **テスト結果**（Phase10 コアループ + Victory）
5. **既知の制限**（Input は可変フレーム / Replay 未実装 等）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### SimulationTick 例

```csharp
public class SimulationTick : MonoBehaviour
{
    [SerializeField] int ticksPerSecond = 20;
    [SerializeField] int maxTicksPerFrame = 5;

    float tickInterval;
    float accumulator;
    int currentTick;

    public int CurrentTick => currentTick;
    public float FixedDeltaTime => tickInterval;

    void Update()
    {
        if (GameSessionManager.IsGameOver)
            return;

        accumulator += Time.unscaledDeltaTime;
        int ticksThisFrame = 0;
        while (accumulator >= tickInterval && ticksThisFrame < maxTicksPerFrame)
        {
            accumulator -= tickInterval;
            currentTick++;
            ticksThisFrame++;
            SimulationDispatcher.Tick(tickInterval);
        }
    }
}
```

### Manager 移行パターン

```csharp
// Before
void Update() {
    if (GameSessionManager.IsGameOver) return;
    job.remainingSeconds -= Time.deltaTime;
}

// After
public void TickSimulation(float fixedDeltaTime) {
    job.remainingSeconds -= fixedDeltaTime;
}
```

- `Update()` は **削除** または **空**（MonoBehaviour 要件のみ残さない）
- Tick 登録は `SimulationTick` Awake で各 Manager を Register

### CPU AI 間隔

```csharp
// 20 TPS, 2 秒評価 → 40 Tick ごと
int evaluateIntervalTicks = Mathf.RoundToInt(EvaluateInterval * ticksPerSecond);
if (currentTick % evaluateIntervalTicks == 0) Evaluate();
```

または Manager 内で `ticksUntilEvaluate--` 方式（**可変 TPS 変更に強い**）。

### SceneBuilder

```csharp
GameObject simulationTickObject = new GameObject("SimulationTick");
simulationTickObject.transform.SetParent(systems.transform);
simulationTickObject.AddComponent<SimulationTick>();
```

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`（主）+ `Benchmark.unity`（回帰）
- **`AoE → Setup Phase10 Scene`** — `SimulationTick` 追加
- **`AoE → Setup Benchmark Scene`** — 同上
- Phase 1〜10 Setup メニューは壊さない

---

## ⑨ 完了条件（Phase 15 MVP）

- [ ] `SimulationTick` が Systems に存在し **20 TPS**（または設定 TPS）で Tick 配信
- [ ] **Tick 化対象 8 Manager** が `Time.deltaTime` ではなく **fixedDeltaTime** で Simulation 更新
- [ ] **Game Over 中 Tick 停止**
- [ ] **Input / Camera / UI** は可変フレームのまま
- [ ] **Phase10** — 採集・建築・生産・CPU 攻撃波・Victory / Defeat が **Phase 14 同様に動作**
- [ ] Console に **Null 参照・例外なし**
- [ ] `docs/FOUNDATION_PHASES.md` Phase 15 を ✅ に更新

---

## ⑩ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene` → Play
2. Villager 生産（Q）— 3 秒タイマーが Tick 上で完了
3. 木採集 — Wood 増加
4. House 建築 — 完成
5. Barracks + Militia 生産
6. CPU 攻撃波（30 秒）— Militia 攻撃
7. 敵 TC 破壊 → **VICTORY**
8. フレームレートを変えても（VSync OFF 等）**Simulation 速度が大きく変わらない**こと（目視）
9. Console エラーなし

Phase 15 のみ実装。Phase 16 以降（Command Queue 等）に触れない。

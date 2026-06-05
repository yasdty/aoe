# Phase 13 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜12 完了（PoC + Foundation Phase 11〜12）  
> **ロードマップ:** [FOUNDATION_PHASES.md](../FOUNDATION_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/RTS_IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 13 実装（Benchmark Infrastructure）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜12 は完了済み。Phase 13 のみ実装すること。**

---

## ① Foundation 方針（必読・遵守）

[FOUNDATION_PHASES.md](../FOUNDATION_PHASES.md) の最重要方針を厳守:

- **AoE 機能を増やさない** — Archer / Food / Age Up 等は禁止
- **small diff** — 1 Phase = 1 目的（Benchmark のみ）
- **既存ゲームを壊さない** — 完了時 `Phase10.unity` でコアループ + **Victory / Defeat** + **UnitPool** が動くこと
- **Simulation 優先** — 計測基盤の追加。ゲームロジックの最適化は Phase 14 以降

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止
- Manager 更新方式を維持
- **Unity アセット手書き禁止**: Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- Setup メニューは **Edit モード専用**
- **OnGUI は左上原点** — 既存 HUD パターンに合わせる
- **`GetInstanceID()` 禁止**

---

## ② Phase 12 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **Victory / Defeat** — TC 破壊で終了 UI、`GameSessionManager.IsGameOver`
- **Object Pool** — `UnitPool` / `BuildingPool`（Villager / Militia / House / Barracks）
- **Spawn 経路** — `UnitSpawner.Spawn` → `UnitPool.Rent` / `Die()` → `UnitPool.Return`
- **Pool 統計** — Console: `UnitPool: spawn=X reuse=Y (Villager|Militia)`
- **AttackManager** — 死亡時 `CancelJobsForUnit` とリスト走査の競合を修正済み
- **Phase 10 コアループ** — 採集・建築・生産・CPU 経済 / 軍事 AI

### 現状のギャップ（Phase 13 で解消）

| 項目 | 現状 |
|------|------|
| FPS / FrameTime 計測 | **なし** |
| GC 計測 | **なし** |
| 大量ユニット負荷試験 | **手動** — TC 生産を繰り返すのみ |
| 専用 Benchmark シーン | **なし** |
| `RTS_IMPLEMENTATION_STATUS.md` §Performance Benchmark | **TBD / 未計測** |

主要ファイル（**実装前に必ず開いて読む**）:

- `Assets/Scripts/Units/UnitPool.cs` / `UnitSpawner.cs`
- `Assets/Scripts/Units/UnitManager.cs`（`TickMovement` 更新方式）
- `Assets/Scripts/Editor/Phase10SceneBuilder.cs`（Setup メニューパターン）
- `Assets/Scripts/Editor/Phase1SceneBuilder.cs`（地面・カメラ・Unit 生成参考）
- `Assets/Scripts/Selection/GameTimeHudView.cs` または `ResourceHudView.cs`（OnGUI 参考）
- `docs/RTS_IMPLEMENTATION_STATUS.md` — §Performance Benchmark
- `Assets/Scenes/Phase10.unity`

---

## ③ Phase 13 目的

**性能を可視化する** — ワンクリックで 50 / 100 / 200 / 500 / 800 ユニット規模の負荷試験ができ、FPS / FrameTime / GC を画面または Console で確認できるようにする。

### 今回実装するもの

1. **`BenchmarkScene`** — 専用検証シーン `Assets/Scenes/Benchmark.unity`（**新規。Phase10 は壊さない**）
2. **`BenchmarkSceneBuilder`** — Editor メニュー `AoE → Setup Benchmark Scene`
3. **`BenchmarkSpawner`** — 指定数の Villager / Militia を `UnitPool.Rent` 経由で一括配置
4. **`BenchmarkMetricsView`**（名称任意）— FPS / avg frame ms / GC alloc（フレーム or 秒）を OnGUI 表示
5. **規模プリセット** — 50 / 100 / 200 / 500 / 800（キーまたは OnGUI ボタン）
6. **README / `docs/FOUNDATION_PHASES.md` / `RTS_IMPLEMENTATION_STATUS.md` 更新**

### Benchmark シーン構成（MVP）

| 要素 | 内容 |
|------|------|
| 地面 | Phase1 と同様の Plane（Builder から生成） |
| カメラ | Overview 固定または Phase10 同等の RTS カメラ（Input 最小で可） |
| Systems | `UnitManager` / `UnitPool` / `AttackManager`（必要最小限） |
| UI | `BenchmarkMetricsView` + スポーン数選択 |
| 除外 | CPU AI / 採集 / 建築 / Victory — **Benchmark 専用のため不要** |

### 計測項目（MVP — 必須）

| 指標 | 内容 |
|------|------|
| **FPS** | `1f / Time.unscaledDeltaTime` の移動平均（例: 30 フレーム） |
| **FrameTime** | `Time.unscaledDeltaTime * 1000f` ms |
| **GC Alloc** | `ProfilerRecorder` または `GC.GetTotalMemory` 差分（フレームあたり KB）。Editor / Development Build で可 |

### スポーン動作（MVP）

- 既存ユニットを **Pool Return** または非アクティブ化してから再スポーン（Destroy 乱立禁止）
- スポーン位置: グリッドまたは同心円配置（重なり最小化。Formation は Phase 26 候補）
- **`UnitManager.TickMovement` が動く**こと — 静止 800 体でも Manager コストは計測対象

### 禁止（Phase 13 範囲外）

- Spatial Hash / Fixed Tick / Command Queue（Phase 14〜16）
- 新ユニット種別・新資源
- Phase10 コアループの rewrite
- `Phase10.unity` を Benchmark 用途に改変（**専用シーンを新設**）
- 本番ビルド向け Profiler 深堀り（Editor/Dev で十分）
- Pool ロジック自体の rewrite（Rent/Return 経路はそのまま利用）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 13-1 | `BenchmarkMetricsView` — FPS / FrameTime / GC 表示 |
| 13-2 | `BenchmarkSpawner` — UnitPool 経由一括スポーン + クリア |
| 13-3 | `BenchmarkSceneBuilder` — 地面・カメラ・Systems・UI |
| 13-4 | 規模プリセット 50〜800 + Play 確認 |
| 13-5 | README / FOUNDATION_PHASES / RTS_IMPLEMENTATION_STATUS 更新 |

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**（新規 / 変更 / 削除）
2. **新規追加ファイル一覧**
3. **Benchmark シーンと Phase10 の責務分担**
4. **計測方式**（FPS 平滑化 / GC 取得 API）
5. **影響範囲**（UnitPool / UnitManager のみ利用する理由）
6. **リスク**（800 体での Editor フリーズ / GC API 差異）
7. **ロールバック方法**
8. **完了条件**（下記チェックリスト）
9. **テスト手順**

---

## ⑥ 実装後に必ず出力

1. **変更内容サマリ**
2. **変更ファイル一覧**
3. **計測結果サンプル**（50 / 200 / 800 の FPS 目安。Editor 値で可）
4. **テスト結果**
5. **既知の制限**（Editor のみ / 静止ユニット / AI なし 等）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### Editor メニュー

```csharp
[MenuItem("AoE/Setup Benchmark Scene", true)]
static bool Validate() => !EditorApplication.isPlaying;

[MenuItem("AoE/Setup Benchmark Scene")]
public static void SetupBenchmarkScene() { /* ... */ }
```

### スポーン経路

```
BenchmarkSpawner.Spawn(count)
  → UnitPool.Rent (ループ)
  → Unit.SetMoveTarget なし（Idle 静止で可）
```

### OnGUI 表示例

```
FPS: 58.2  |  Frame: 17.2 ms  |  GC/frame: 0.1 KB
Units: 200  |  [50] [100] [200] [500] [800] [Clear]
```

### RTS_IMPLEMENTATION_STATUS 更新

§Performance Benchmark の TBD 表に、Benchmark シーンでの初回計測値（Editor）を記載。

---

## ⑧ シーン / Editor

- **新規検証シーン:** `Assets/Scenes/Benchmark.unity`
- **既存ゲーム検証:** `Assets/Scenes/Phase10.unity`（**変更後も Play 可能であること**）
- `AoE → Setup Benchmark Scene` — Edit モード専用
- Phase 1〜10 Setup メニューは壊さない

---

## ⑨ 完了条件（Phase 13 MVP）

- [ ] `AoE → Setup Benchmark Scene` で `Benchmark.unity` が生成される
- [ ] 50 / 100 / 200 / 500 / 800 の **ワンクリック（またはキー 1 回）** でスポーン切替
- [ ] 画面上に **FPS / FrameTime / GC** が表示される
- [ ] スポーンは **`UnitPool.Rent` 経由**（Destroy 乱立なし）
- [ ] `Phase10.unity` — Victory / Defeat / Pool / コアループが **Phase 12 同様に動作**
- [ ] Console に **Null 参照・例外なし**
- [ ] `docs/FOUNDATION_PHASES.md` Phase 13 を ✅ に更新

---

## ⑩ テスト手順（Play チェックリスト）

### Benchmark シーン

1. `AoE → Setup Benchmark Scene` → `Benchmark.unity` を Play
2. **200** プリセット → FPS / FrameTime が表示される
3. **800** → フレーム落ちしてもクラッシュしない
4. **Clear** → ユニットが Pool に返る（Destroy ではない）
5. 再 **200** → `UnitPool reuse` が Console に増える（任意確認）

### Phase10 回帰

6. `Phase10.unity` を Play → Q 生産 / CPU 攻撃 / Victory が動く
7. Console エラーなし

Phase 13 のみ実装。Phase 14 以降（Spatial Hash 等）に触れない。

# Phase 31 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜30 完了（PoC + Foundation + M2 Economy + M2.5 Phase 21〜30）  
> **マイルストン:** M2.6 RTS UX（**第 1 Phase**）  
> **ロードマップ:** [05_M2_6_RTS_UX_PHASES.md](../05_M2_6_RTS_UX_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 31 実装（Unit Production Queue）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜30 は完了済み。Phase 31 のみ実装すること。**

---

## ① M2.6 RTS UX 方針（必読・遵守）

Phase 30 で M2.5 完了。Phase 31 は **ユニット生産キュー** — TC / Barracks で Villager / Militia を **複数予約**し、AoE2 の Q 連打 UX を実現する。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | 建物ごとの **FIFO 生産キュー**（最大 15）+ HUD / Barracks Q |
| **small diff** | `ProductionManager` / `BarracksProductionManager` **拡張** — rewrite 禁止 |
| **既存パターン再利用** | `TrainVillagerCommand` / `TrainMilitiaCommand` / `CommandQueue` / `ISimulationTickable` |
| **既存ゲームを壊さない** | 採集 / 建築 / CPU AI / Aggro / Victory / Foundation 全機能 |
| **Foundation 維持** | Fixed Tick 20 TPS / Command Queue（Player 操作は Command 経由） |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- 生産 Tick は **`ISimulationTickable`**（`Update` で進捗を回さない）
- **`.meta` は 32 文字 GUID**

---

## ② Phase 30 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **TC 生産** — `ProductionManager` が **1 建物 1 ジョブ**のみ。`IsProducing()` が true なら 2 体目拒否
- **Barracks 生産** — `BarracksProductionManager` も同様 **1 ジョブのみ**
- **Player TC** — `ProductionPanelView`: Q キー + ボタン `TrainVillagerCommand`。生産中は **ボタン無効化**
- **Player Barracks** — `BarracksPanelView`: ボタンのみ。**Q キーなし**。生産中は **ボタン無効化**
- **資源消費** — キュー追加時に `TrySpendFood` / `TrySpendWood`（既存）
- **CPU** — `CpuEconomyAiManager.TryTrainVillager` / `CpuMilitaryAiManager.TryTrainMilitia` が `IsProducing` で **1 体ずつ**のみキュー

### 現状のギャップ（Phase 31 で解消）

| 項目 | 現状 |
|------|------|
| 生産キュー | **なし** — 2 体目を `TryQueueProduction` が拒否 |
| TC Q 連打 | 1 体目生産中は **追加不可** |
| Barracks Q | **未実装** |
| HUD | キュー長表示なし。生産中パネル全体が無効 |
| Shift+Q 5 体 | **対象外**（M2.6 後回し） |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| TC 生産 | `Assets/Scripts/Buildings/ProductionManager.cs` |
| Barracks 生産 | `Assets/Scripts/Buildings/BarracksProductionManager.cs` |
| 建物 API | `TownCenter.cs` / `Barracks.cs` — `TryQueueVillagerProduction` / `TryQueueMilitiaProduction` |
| 命令 | `Assets/Scripts/Commands/GameCommands.cs` — `TrainVillagerCommand` / `TrainMilitiaCommand` |
| TC HUD | `Assets/Scripts/Selection/ProductionPanelView.cs` |
| Barracks HUD | `Assets/Scripts/Selection/BarracksPanelView.cs` |
| 入力 | `Assets/Scripts/Input/RTSInputReader.cs` / `RTSInputActionsBuilder.cs` |
| CPU 経済 | `Assets/Scripts/AI/CpuEconomyAiManager.cs` — `TryTrainVillager` |
| CPU 軍事 | `Assets/Scripts/AI/CpuMilitaryAiManager.cs` — `TryTrainMilitia` |
| Pop / 資源 | `PopulationManager` / `ResourceManager` |

---

## ③ Phase 31 目的 — AoE2 ユニット生産キュー

**1 建物あたり最大 15 体**（AoE2 原版準拠）を FIFO で予約。先頭のみ Tick 進行、完了で Spawn → 次ジョブ開始。

### MVP 挙動

```
Player が TC 選択 → Q 連打
    ↓ 各押下
TrainVillagerCommand → TryQueueProduction（Food 即 Spend、Pop チェック）
    ↓ キューに追加（最大 15）
先頭ジョブのみ remainingSeconds を Tick
    ↓ 0 到達
UnitSpawner.Spawn → キュー先頭削除 → 次ジョブが先頭に
```

Barracks + Militia も **同パターン**（Wood Spend、Q キー対応）。

### 資源・Pop ルール（MVP）

| タイミング | ルール |
|------------|--------|
| **キュー追加時** | Pop 余裕 (`CanTrainUnit`) + 資源足りること — **足りなければ追加しない**（silent fail） |
| **Spawn 時** | 追加時に Spend 済み — **再チェック不要**（TC 破壊等で Pop 溢れは MVP 許容） |
| **キャンセル / 返金** | **Phase 31 対象外** — キャンセル UI なし |

### AoE2 参考（Phase 31 では省略）

- Shift+Q → 5 体一括
- 複数 Barracks 選択 → 最短キューへ分散

---

## ④ 今回実装するもの

### 1. `ProductionManager` — 建物ごと FIFO キュー

**変更要点（small diff）:**

1. `IsProducing(townCenter)` による **2 体目拒否を削除**
2. 同一 `TownCenter` に最大 **15** ジョブ（定数 `MaxQueueSize = 15`）
3. **Tick** — 各 TC について **先頭ジョブのみ** `remainingSeconds` を減算。完了で Spawn + RemoveAt(0)
4. 新 API（例）:
   - `GetQueueCount(TownCenter)`
   - `GetRemainingSeconds` / `GetTotalSeconds` — **先頭ジョブ**の値を返す（HUD 互換）
5. `Unregister` — 既存どおりその TC の全ジョブ削除

**データ構造:** 既存 `List<ProductionJob>` を活かし、**同一 TC のジョブは追加順 = FIFO** でも可。Tick 時に TC ごとに先頭のみ処理。

```csharp
// TryQueueProduction — 変更イメージ
if (GetQueueCount(townCenter) >= MaxQueueSize)
    return false;
if (!PopulationManager.CanTrainUnit(townCenter.Team))
    return false;
if (foodCost > 0f && !ResourceManager.TrySpendFood(...))
    return false;
// IsProducing チェックは削除
instance.activeJobs.Add(...);
```

### 2. `BarracksProductionManager` — 同パターン

- `MaxQueueSize = 15`
- `GetQueueCount(Barracks)` 等
- Tick / TryQueue / GetRemainingSeconds は TC と **対称**

### 3. `ProductionPanelView` — キュー UX

| 変更 | 内容 |
|------|------|
| ボタン | 生産中でも **無効化しない** — Pop 満 / Food 不足 / キュー満（15）/ GameOver のみ無効 |
| 表示 | `Queue: N`（N = 待ち + 生産中の合計、または待ちのみ — **合計**推奨） |
| プログレス | **先頭ジョブ**の `Training... X.Xs` + スライダー（現状維持） |
| Q キー | 既存 `WasTrainVillagerPressedThisFrame` — 変更なし（連打でキュー追加） |

### 4. `BarracksPanelView` — Q キー + キュー UX

| 変更 | 内容 |
|------|------|
| ボタン | `Create Militia (Q) (20 Wood)` — 生産中も押せる（TC 同様） |
| Q キー | `RTSInputReader` + `Update` — TC と **同じ Q アクション**で OK（選択中の建物で分岐） |
| 表示 | `Queue: N` + 先頭プログレス |

**Q キー実装方針（推奨）:**

- `ProductionPanelView` / `BarracksPanelView` それぞれで `WasTrainVillagerPressedThisFrame()` を listen（**同時選択不可**なので競合なし）
- または `SelectionManager` 配下の単一 `ProductionHotkeyHandler` 新規 — **small diff 優先**なら各 Panel に `Update` 追加で可

**Input アセット:** 既存 `TrainVillager` アクションを Barracks でも流用（新 Binding 不要）。ラベルのみ `(Q)` 追加。

### 5. CPU AI — 最小調整

| ファイル | 変更 |
|----------|------|
| `CpuEconomyAiManager.TryTrainVillager` | `IsProducing` ガード **削除** — 代わり `GetQueueCount < Max` なら 1 体追加（2 秒周期のまま） |
| `CpuMilitaryAiManager.TryTrainMilitia` | 同上 |

CPU が一気に 15 体キューする必要はない — **IsProducing ブロック解除**が目的。

### 6. 選択維持

- `TrainVillagerCommand` / `TrainMilitiaCommand` が Selection をクリアしていないことを **確認**。変更不要なら触らない。

---

## ⑤ 今回やらないこと

- Shift+Q 5 体一括
- キュー個別キャンセル・返金 UI
- 複数 TC / 複数 Barracks 選択時の分散キュー
- Rally Point（**Phase 33**）
- Idle Unit UX（**Phase 32**）
- Control Groups（**Phase 34**）
- **Phase10 マップ拡張**（M2.6 完了後 — [05_M2_6](../05_M2_6_RTS_UX_PHASES.md) 参照）
- Barracks から Archer 等（**M3 Phase 35**）

---

## ⑥ 実装ステップ（推奨）

| Step | 内容 |
|------|------|
| 31-1 | `ProductionManager` — FIFO キュー + Tick 先頭のみ + `GetQueueCount` |
| 31-2 | `BarracksProductionManager` — 同パターン |
| 31-3 | `ProductionPanelView` — ボタン常時有効 + Queue 表示 |
| 31-4 | `BarracksPanelView` — Q キー + Queue 表示 + ボタンラベル |
| 31-5 | CPU AI — `IsProducing` ガード削除 |
| 31-6 | Play 確認 + ドキュメント |

---

## ⑦ 技術メモ

### Tick — 先頭のみ進行

```csharp
// 各 TownCenter について activeJobs 内の最初の一致ジョブだけ減算
// 完了時: Spawn → その 1 件 Remove（インデックス i を削除）
// 同一 TC に複数ジョブが並ぶ場合、後続は remainingSeconds 固定（待機）
```

**注意:** 現状の Tick は **全ジョブを並列減算**している。Phase 31 で **建物ごと先頭 1 本**に変更すること。

### Command 経路（変更なし）

```
Q キー / ボタン
  → CommandQueue.Enqueue(TrainVillagerCommand)
  → townCenter.TryQueueVillagerProduction()
  → ProductionManager.TryQueueProduction(...)
```

### HUD — GUI.enabled 条件（例）

```csharp
bool queueFull = ProductionManager.GetQueueCount(townCenter) >= ProductionManager.MaxQueueSize;
bool populationFull = !PopulationManager.CanTrainUnit();
bool canAffordFood = ResourceManager.GetFood(UnitTeam.Player) >= foodCost;
GUI.enabled = !queueFull && !populationFull && canAffordFood && !GameSessionManager.IsGameOver;
// isProducing は GUI.enabled に含めない
```

### Barracks Wood チェック

`BarracksPanelView` は現状 `ResourceManager.Wood`（Player 固定）。Team 引数版があれば Barracks.Team を使う — **既存パターンに合わせる**。

---

## ⑧ 完了条件（Phase 31 MVP）

- [ ] TC 選択 → **Q 連打**で Villager が最大 15 体までキューに積まれる
- [ ] 生産中も **Q / ボタン**で追加キュー可能（資源・Pop・上限のみ制限）
- [ ] HUD に **Queue: N** と先頭ジョブのプログレスバー
- [ ] Barracks 選択 → **Q** で Militia キュー（TC と同 UX）
- [ ] 先頭完了 → Spawn → **自動で次ジョブ開始**
- [ ] 資源不足 / Pop 満 / キュー 15 → **追加 silently 失敗**（例外なし）
- [ ] CPU Villager / Militia 生産が **IsProducing で止まらない**（1 体ずつ追加で OK）
- [ ] 採集 / 建築 / Aggro / Victory / Player 4 資源 **回帰**
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `05_M2_6` Phase 31 ✅

---

## ⑨ テスト手順（Play チェックリスト）

1. `Phase10.unity` → Play
2. TC 選択 → Food 十分な状態で **Q を 5 回** → Console / HUD `Queue: 5`
3. 1 体目 Spawn 後 → キューが 4 に減り、**次が自動開始**
4. Food 0 にして Q → **追加されない**（クラッシュなし）
5. Pop 上限まで Villager 作成 → Q → **追加されない**
6. Barracks 建設 → 選択 → **Q 3 回** → Militia キュー 3
7. 生産中に TC / Barracks **選択が維持**されること
8. CPU が Barracks 完成後 **連続で Militia をキュー**できること（Inspector または Console で確認）
9. 既存コアループ（採集・攻撃・Aggro）回帰
10. Console エラーなし

Phase 31 のみ実装。**Phase 32〜34 / マップ拡張** に触れない。

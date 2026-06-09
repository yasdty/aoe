# Phase 37 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜36 完了（M3 Archery Range + Archer 含む）  
> **マイルストン:** M3 Military — **Spearman（Barracks 歩兵ライン拡張）**  
> **ロードマップ:** [07_M3_MILITARY_PHASES.md](../07_M3_MILITARY_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 37 実装（Spearman）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 37 のみ実装。** 新規建築は作らず、既存 **Barracks / Militia** を **拡張**（rewrite 禁止）。

---

## ① 目的

AoE2 準拠で **Barracks から 2 種類目の歩兵 Spearman** を生産・戦闘させる。

| 項目 | MVP |
|------|-----|
| 建築 | **新規なし** — 既存 Barracks を複数ユニット生産対応 |
| 生産 | Q = Militia（既存）/ **E = Spearman** + 同一 FIFO キュー（最大 15） |
| 戦闘 | 近接（Militia 同型 `AttackManager`）。対騎兵ボーナスは **Phase 39 へ** |
| Rally | 変更なし — Barracks 選択 + 右クリック（Spearman スポーン後も適用） |
| CPU | Militia に加え Spearman を少数生産・攻撃波に混在 |

**やらないこと:** Stable / Cavalry / Counter System 本格化 / 時代昇格 / 新 Input 以外の UI 刷新 / Animator

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| Barracks 生産（1 種のみ現状） | `Barracks.cs`, `BarracksProductionManager.cs`, `BarracksPanelView.cs` |
| Archery Range（Wood+Food キュー） | `ArcheryRangeProductionManager.cs` — Food 消費パターン |
| Data | `PlacedBuildingData`, `Phase1SceneBuilder.EnsureBarracksData`, `EnsureMilitiaData` |
| コマンド | `TrainMilitiaCommand`, `GameCommands.cs` |
| 入力 | `RTSInputActionsBuilder.cs`, `RTSInputReader.cs` — Q は `TrainVillager` |
| CPU 軍事 | `CpuMilitaryAiManager.cs` — `displayName` フィルタ（Militia / Archer） |
| Idle 軍 | `UnitIdleTracker.CopyIdleMilitaryTo` — `CanAttack` ベース（Spearman 自動包含想定） |
| Scene | `Phase10SceneBuilder.cs` — `EnsureBarracksData` 呼び出し |

---

## ③ 実装タスク（small diff）

### 1. Data

- `GameAssetPaths.SpearmanData` — `Assets/Data/UnitData/SpearmanData.asset`
- `Phase1SceneBuilder.EnsureSpearmanData()` — Editor API で `UnitData` 生成  
  - 目安: **HP 45** / 攻撃 **3** / 装甲 **0** / 射程 **2** / CD **1s** / 移動 **5**  
  - `displayName = "Spearman"`（CPU・Idle フィルタで使用）  
  - 色は Militia と区別（例: 青灰 `#5070a0` 系）
- `PlacedBuildingData` — **第 2 生産スロット**を追加（Barracks 専用フィールドで可）  
  ```csharp
  public UnitData secondaryTrainUnitData;
  public float secondaryTrainTime;
  public float secondaryTrainWoodCost;
  public float secondaryTrainFoodCost;
  ```
- `EnsureBarracksData(UnitData militiaData, UnitData spearmanData)` — 既存 Militia 設定を維持しつつ secondary を配線  
  - Spearman 訓練目安: **25 Wood + 35 Food / 4s**（Militia より Food 重め）
- `PlacedBuildingDataResolver.ResolveBarracks` — secondary フィールドの fallback 同期

### 2. 生産ロジック

- `Barracks.TryQueueSpearmanProduction()` — `TryQueueMilitiaProduction` と同型
- `BarracksProductionManager.TryQueueProduction` — **Food コスト対応**（`ArcheryRangeProductionManager` パターン: Wood+Food Spend-on-enqueue、失敗時ロールバック）
- 既存 Militia キュー（Wood のみ）の挙動を **壊さない**

### 3. コマンド・UI・入力

- `TrainSpearmanCommand` — `TrainMilitiaCommand` 同型
- `BarracksPanelView` — 2 ボタン化 + パネル高さ調整  
  - `Create Militia (Q) (...)` — 既存  
  - `Create Spearman (E) (... Wood, ... Food)` — 新規  
  - Food / Wood 不足メッセージ分岐
- **入力:** `RTSInputActionsBuilder` に `TrainSecondary`（`<Keyboard>/e`）追加  
  - `RTSInputReader.WasTrainSecondaryPressedThisFrame()`  
  - `BarracksPanelView.Update` — E で Spearman キュー  
  - **Q/E 競合:** TC / ArcheryRange / Barracks は各 Panel の `Update` が選択状態で分岐（既存パターン踏襲）
- `SelectionInfoPanelView` — 単体 Spearman 選択時 HP / 攻撃 / 装甲表示（既存 Unit 分岐で自動可。要確認）

### 4. Rally / キュー / Idle

- `ProductionRallyApplier.Apply(Barracks, ...)` — 変更不要（Spearman も同経路）
- 同一 Barracks キューに Militia / Spearman **混在可**（FIFO、先頭のみ Tick — Phase 31 パターン）
- `UnitIdleTracker` — Spearman は `CanAttack` のため Idle 軍 `,` キー対象に含まれること

### 5. CPU

- `CpuMilitaryAiManager` — `TryTrainSpearman()` 追加  
  - 目安: Spearman 目標 **4**（Militia 8 / Archer 4 とバランス調整可）  
  - Barracks 完成後、Wood **かつ** Food 余裕時にキュー  
  - `CollectCpuAttackUnits` — `"Spearman"` を Militia / Archer と同様 `displayName` で追加  
  - Militia 専用カウント `CountCpuUnitsByName("Militia")` は **維持**（Spearman を Militia 枠に数えない）

### 6. Phase10 SceneBuilder

- `EnsureSpearmanData()` 呼び出し  
- `EnsureBarracksData(militiaData, spearmanData)` に更新  
- **シーン再生成:** `AoE → Setup Phase10 Scene`（Input Action アセット更新含む）

---

## ④ 制約

- rewrite 禁止 / small diff only
- **新規建築・新 Manager 禁止** — Barracks 系の拡張のみ
- Unity アセット手書き禁止 — ScriptableObject は Editor API
- `.meta` は Unity 再コンパイル生成（手書き禁止）
- Militia / Archer / Archery Range 既存挙動を壊さない
- Phase 39 用に `UnitData` へ `armorClass` 等は **まだ追加しない**（単一 `armor` のまま）

---

## ⑤ Play 確認

1. `AoE → Setup Phase10 Scene` → Play
2. Barracks 選択 → **Q** Militia / **E** Spearman を交互にキュー → 混在 FIFO でスポーン
3. Spearman 右クリック敵 → 近接攻撃（Militia 同様）
4. Rally 右クリック → Spearman スポーン後移動
5. CPU が Spearman を少数生産し攻撃波に混在（Console ログ）
6. Phase 31〜36 回帰（TC Q / Archery Q / Control Group / Idle `,`）

---

## ⑥ 完了時ドキュメント

- [x] `IMPLEMENTATION_STATUS.md` — Phase 37 ✅
- [x] `07_M3_MILITARY_PHASES.md` — Phase 37 ✅
- [x] 本プロンプト — ✅

---

Phase 37 のみ。**Phase 38 Stable + Cavalry には触れない。**

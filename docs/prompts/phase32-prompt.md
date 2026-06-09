# Phase 32 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜31 完了（PoC + Foundation + M2 Economy + M2.5 + M2.6 Phase 31）  
> **マイルストン:** M2.6 RTS UX（**第 2 Phase**）  
> **ロードマップ:** [05_M2_6_RTS_UX_PHASES.md](../05_M2_6_RTS_UX_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 32 実装（Idle Unit UX）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜31 は完了済み。Phase 32 のみ実装すること。**

---

## ① M2.6 RTS UX 方針（必読・遵守）

Phase 31 でユニット生産キューが完成。Phase 32 は **待機ユニットの可視化 + 素早い選択** — AoE2 の Idle Villager UX の最小版。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | 待機村民 / 軍の **カウント表示** + **ホットキーでジャンプ選択** |
| **small diff** | **`UnitIdleTracker` 新規** + HUD 拡張 + `SelectionManager` 最小 API — rewrite 禁止 |
| **既存パターン再利用** | `CpuEconomyAiManager.IsIdleForEconomy` ロジック / `UnitManager.CopyUnitsTo` / `SelectionManager` |
| **既存ゲームを壊さない** | 生産キュー / 採集 / Aggro / CPU AI / Victory / Foundation 全機能 |
| **Player のみ** | Idle 表示・ホットキーは **`UnitTeam.Player`** 対象（CPU は対象外） |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- Idle 判定は **毎フレーム全走査 OK**（MVP）— 将来 Tick 化可
- **`.meta` は 32 文字 GUID**

---

## ② Phase 31 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **生産キュー** — TC / Barracks FIFO 最大 15、Q 連打、HUD `Queue: N`
- **ユニット状態** — `Unit.State` = `Idle` / `Move` / `Attack` / `Dead`
- **`Unit.State.Idle`** — 移動目標なし & 攻撃ジョブなし。**採集中でも Idle になりうる**（`HasMoveTarget` false 時）
- **CPU 待機判定** — `CpuEconomyAiManager.IsIdleForEconomy` が Gather / Build / Move を除外（**Player 向け Idle UX の参考実装**）
- **HUD** — `ResourceHudView`（Wood / Food / Gold / Stone / Pop + 建築ボタン）。**Idle 表示なし**
- **ホットキー** — Q（TrainVillager）のみ。**`.` / `,` 未実装**
- **SelectionManager** — `SetSelection(Unit)` は **private**。公開 API で単体選択する手段が弱い

### 現状のギャップ（Phase 32 で解消）

| 項目 | 現状 |
|------|------|
| Idle Villager カウント | なし |
| 待機村民の素早い選択 | マップを探すしかない |
| 待機 Militia の素早い選択 | 同上 |
| Shift+. 全待機村民 | なし |
| 待機ビジュアル（ティント等） | なし |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| 待機判定参考 | `Assets/Scripts/AI/CpuEconomyAiManager.cs` — `IsIdleForEconomy` |
| ユニット状態 | `Assets/Scripts/Units/Unit.cs` — `State` / `HasMoveTarget` / `CanAttack` |
| ユニット一覧 | `Assets/Scripts/Units/UnitManager.cs` — `CopyUnitsTo` |
| 攻撃 / 採集 | `AttackManager.IsUnitAttacking` / `GatherManager.IsUnitGathering` / `FoodGatherManager` / `MineralGatherManager` / `BuildingPlacementManager.IsUnitBuilding` |
| Aggro | `Assets/Scripts/Combat/UnitAggroManager.cs` |
| 選択 | `Assets/Scripts/Selection/SelectionManager.cs` |
| HUD | `Assets/Scripts/Selection/ResourceHudView.cs` — `IsPointerOverHud` / レイアウト |
| 入力 | `Assets/Scripts/Input/RTSInputReader.cs` / `RTSInputActionsBuilder.cs` |
| カメラ | `Assets/Scripts/Camera/RTSCameraController.cs` — `ApplyOverviewView`（ジャンプ任意） |
| Editor | `Assets/Scripts/Editor/Phase10SceneBuilder.cs` |

---

## ③ Phase 32 目的 — Idle Unit UX（MVP）

**「何もしていない村民 / 兵が何人いるかわかる」+「`.` で次の待機村民に飛べる」**

### 待機（Idle）の定義 — Player ユニット

**村民（`!CanAttack`）:**

```
IsAlive
&& Team == Player
&& !HasMoveTarget
&& !GatherManager.IsUnitGathering
&& !FoodGatherManager.IsUnitGathering
&& !MineralGatherManager.IsUnitGathering
&& !BuildingPlacementManager.IsUnitBuilding
```

**軍事ユニット（`CanAttack` — MVP は Militia のみ）:**

```
IsAlive
&& Team == Player
&& CanAttack
&& !HasMoveTarget
&& !AttackManager.IsUnitAttacking
&& !BoarAttackManager.IsUnitAttackingBoar
```

**注意:** `Unit.State == Idle` だけでは **不十分**（採取中に MoveTarget が false の瞬間がある）。上記 **ジョブ除外** を必須とする。

### AoE2 参考 vs MVP スコープ

| 機能 | AoE2 | Phase 32 MVP |
|------|------|--------------|
| Idle Villager カウント | 画面上部 | ✅ HUD `Idle Villagers: N` |
| `.` 次の待機村民 | ✅ | ✅ |
| Shift+. 全待機村民選択 | ✅ | ✅ **推奨** |
| `,` 次の待機軍 | ✅ | ✅ |
| Shift+, 全待機軍 | 弱い / なし | ❌ 後回し |
| カメラジャンプ | あり | **任意**（`ApplyOverviewView` 利用可） |
| 待機ティント / アイコン | あり | **任意** — 時間があれば |

---

## ④ 今回実装するもの

### 1. `UnitIdleTracker.cs`（新規・static 推奨）

- `IsIdleVillager(Unit unit)` — 上記村民条件（Player チーム固定 or 引数 `UnitTeam`）
- `IsIdleMilitary(Unit unit)` — 上記軍事条件
- `CountIdleVillagers(UnitTeam team = Player)`
- `CountIdleMilitary(UnitTeam team = Player)`
- `CopyIdleVillagersTo(List<Unit> buffer, UnitTeam team)` — 安定順（`UnitManager` 登録順 or InstanceID 昇順）
- `CopyIdleMilitaryTo(List<Unit> buffer, UnitTeam team)`

**CPU 向け `IsIdleForEconomy` との関係:** duplicate を避けるなら `UnitIdleTracker.IsEconomyIdle(Unit)` を **internal 共通化**して CPU から呼ぶのは **optional**（Phase 32 では Player UX 優先、CPU refactor は必須にしない）。

### 2. HUD — `IdleUnitHudView.cs`（新規）または `ResourceHudView` 拡張

**推奨:** 新規 `IdleUnitHudView`（`ResourceHudView` の肥大化を避ける）

| 表示 | 内容 |
|------|------|
| 行 1 | `Idle Villagers: N` |
| 行 2 | `Idle Military: M`（Militia 待機数） |
| ボタン（任意） | `Next Idle Villager (.)` — クリックで `.` と同じ |

- 配置: 画面左上、`ResourceHudView` の **右隣** または **下**（重ならないよう `GameUiInput.SetHudPanelScreenRect` 整合）
- `ResourceHudView.IsPointerOverHud` が Idle パネルもカバーするよう **GameUiInput** 登録を忘れないこと

### 3. `SelectionManager` — 最小公開 API

`SetSelection(Unit)` が private のため、以下を **public** 追加（small diff）:

```csharp
public void SelectSingleUnit(Unit unit);
public void SelectUnits(IReadOnlyList<Unit> units); // Shift+. 用 — 既存 Clear + Add パターン
```

- 建物 / 資源選択は **クリア**してからユニット選択（既存 `ClearAllSelection` 流用）
- 死亡ユニットは選択しない

### 4. `IdleUnitSelectionController.cs`（新規）— ホットキー

| 入力 | 動作 |
|------|------|
| `.` | 次の **待機村民** を単体選択（リストを循環 — lastIndex 保持） |
| Shift + `.` | **全待機村民** を複数選択 |
| `,` | 次の **待機軍** を単体選択（循環） |

**実装:**

- `RTSInputActionsBuilder` にアクション追加:
  - `SelectNextIdleVillager` → `<Keyboard>/period`
  - `SelectAllIdleVillagers` → Shift 修飾は `RTSInputReader.IsShiftHeld` + `.` または composite
  - `SelectNextIdleMilitary` → `<Keyboard>/comma`
- `RTSInputReader` に `WasSelectNextIdleVillagerPressedThisFrame()` 等
- **`RTSInputActionsFactory` / 既存 `.inputactions` アセット** — Editor Setup 再生成 or Fix メニュー確認

**カメラジャンプ（任意）:**

```csharp
cameraController.ApplyOverviewView(idleVillager.transform.position);
```

### 5. Phase10 SceneBuilder

- `IdleUnitHudView` + `IdleUnitSelectionController` を SelectionManager オブジェクト等に AddComponent
- SerializeField 参照（`selectionManager`, `input`, `cameraController`）を配線

---

## ⑤ 今回やらないこと

- Shift+, 全待機軍一括選択
- 待機ユニットの **3D マーカー / ミニマップ**
- CPU Idle 表示
- Rally Point（**Phase 33**）
- Control Groups（**Phase 34**）
- Phase10 マップ拡張（M2.6 完了後）
- `IsIdleForEconomy` の大規模 refactor（optional のみ）

---

## ⑥ 実装ステップ（推奨）

| Step | 内容 |
|------|------|
| 32-1 | `UnitIdleTracker` — 判定 + カウント + リスト収集 |
| 32-2 | `SelectionManager` — `SelectSingleUnit` / `SelectUnits` |
| 32-3 | Input — `.` / Shift+. / `,` アクション + `RTSInputReader` |
| 32-4 | `IdleUnitSelectionController` — ホットキー + 循環インデックス |
| 32-5 | `IdleUnitHudView` — カウント表示 + GameUiInput 登録 |
| 32-6 | Phase10 SceneBuilder + Play 確認 + ドキュメント |

---

## ⑦ 技術メモ

### 循環選択（`.` キー）

```csharp
// 毎押下: idleVillagers リストを再構築 → lastIndex = (lastIndex + 1) % count
// count == 0 → 何もしない
// selectionManager.SelectSingleUnit(idleVillagers[lastIndex]);
```

- リスト順は **毎回同じ**（InstanceID 昇順推奨）— 押すたびに「次」が predictable

### Shift+. 全選択

```csharp
UnitIdleTracker.CopyIdleVillagersTo(buffer, UnitTeam.Player);
if (buffer.Count > 0)
    selectionManager.SelectUnits(buffer);
```

- 既存 **Shift 追加選択** との整合: Shift+. は **置換選択**（AoE2 準拠）

### HUD レイアウト例

```
[ResourceHudView 左]
Wood: N
Food: N
...

[IdleUnitHudView — ResourceHud の右]
Idle Villagers: 2
Idle Military: 1
```

### Gather 中は Idle ではない

採取ジョブ中は `GatherManager` / `FoodGatherManager` / `MineralGatherManager` の `IsUnitGathering` が true — **必ず除外**。

---

## ⑧ 完了条件（Phase 32 MVP）

- [ ] HUD に **Idle Villagers: N**（Player 村民のみ）
- [ ] HUD に **Idle Military: M**（Player Militia 待機）
- [ ] **`.`** — 次の待機村民を単体選択（全員待機中は循環）
- [ ] **Shift+.** — 全待機村民を複数選択
- [ ] **`,`** — 次の待機 Militia を単体選択
- [ ] 採集中 / 建築中 / 移動中 / 攻撃中のユニットは **Idle にカウントされない**
- [ ] 生産キュー / Q 連打 / Aggro / CPU / Victory **回帰**
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `05_M2_6` Phase 32 ✅

---

## ⑨ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene` → Play
2. 初期状態 — 待機村民がいれば HUD `Idle Villagers: N`（N ≥ 1）
3. 村民を木に割当 → カウント **減る**
4. 割当解除（Idle に戻る）→ カウント **増える**
5. **`.` 連打** — 待機村民が **順に選択**される
6. **Shift+.** — **全待機村民** が選択される
7. Militia 生産 → 待機状態で **`,`** — Militia が選択される
8. Militia を右クリック移動 → Idle Military **減る**
9. Aggro / 採集 / TC Q キュー / Barracks Q **回帰**
10. Console エラーなし

Phase 32 のみ実装。**Phase 33〜34 / マップ拡張** に触れない。

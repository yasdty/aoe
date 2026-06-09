# Phase 29 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜28 完了（PoC + Foundation + M2 Economy + M2.5 Phase 21〜28）  
> **マイルストン:** M2.5 Economy Polish  
> **ロードマップ:** [04_M2_5_ECONOMY_POLISH_PHASES.md](../04_M2_5_ECONOMY_POLISH_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 29 実装（Militia Basic Aggro）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜28 は完了済み。Phase 29 のみ実装すること。**

---

## ① M2.5 Economy Polish 方針（必読・遵守）

Phase 28 で Sheep Herding + Deer wander が完成。Phase 29 は **待機 Militia の自動反撃（Basic Aggro）** のみ。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | Idle 近接戦闘ユニットが射程内の敵を自動攻撃 |
| **small diff** | **`UnitAggroManager` 新規** + `AttackManager.IssueAttack` 再利用 — rewrite 禁止 |
| **既存パターン再利用** | `AttackManager` / `UnitSpatialIndex.FindNearestUnit` / `BoarAggroManager`（Tick 構造参考） |
| **既存ゲームを壊さない** | 右クリック攻撃 / CPU ウェーブ / Boar / 羊 / Mill / 4 資源 / Foundation 全機能 |
| **Foundation 維持** | Command Queue / Fixed Tick 20 TPS / Spatial Hash |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- Aggro 判定は **`ISimulationTickable`**（個別 `Update` 禁止）
- **`.meta` は 32 文字 GUID**

---

## ② Phase 28 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **Militia 攻撃** — **右クリックのみ**（敵 Unit / Building / Boar）
- **Move 命令** — `MoveCommand` が `AttackManager.CancelForUnits` を呼び Aggro 解除
- **AttackManager** — `IssueAttack` + `activeJobs` で近接追従・クールダウン・ダメージ
- **Unit.State** — `Idle` / `Move` / `Attack`（`AttackManager.IsUnitAttacking` 参照）
- **UnitSpatialIndex** — `FindNearestUnit(origin, team, filter)` 利用可能
- **CPU Militia** — `CpuMilitaryAiManager` が定期ウェーブ攻撃。**待機中の自動反撃なし**
- **自軍 Militia** — 敵が近づいても **反応しない**

### 現状のギャップ（Phase 29 で解消）

| 項目 | 現状 |
|------|------|
| Player Militia Aggro | なし — 待機中は敵を無視 |
| CPU Militia Aggro | なし — ウェーブ命令以外は待機 |
| 移動中 Aggro | N/A — MVP では **移動中は Aggro しない** |
| Stand Ground UI | なし — **Phase 39（M3）** へ |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| 近接戦闘 | `Assets/Scripts/Combat/AttackManager.cs` — `IssueAttack` / `IsUnitAttacking` / `CancelForUnits` |
| Boar 反撃参考 | `Assets/Scripts/Economy/BoarAggroManager.cs` — Tick + 射程判定 |
| ユニット状態 | `Assets/Scripts/Units/Unit.cs` — `State` / `HasMoveTarget` / `CanAttack` / `AttackRange` |
| 空間検索 | `Assets/Scripts/Spatial/UnitSpatialIndex.cs` — `FindNearestUnit` |
| 命令 | `Assets/Scripts/Commands/GameCommands.cs` — `MoveCommand`（Aggro キャンセル確認） |
| CPU 軍事 | `Assets/Scripts/AI/CpuMilitaryAiManager.cs` — ウェーブ攻撃との共存 |
| Idle 判定参考 | `Assets/Scripts/AI/CpuEconomyAiManager.cs` — `IsIdleForEconomy` |
| Militia データ | `Assets/Data/UnitData/MilitiaData.asset` — attackRange: 2 |

---

## ③ Phase 29 目的

**AoE2 の「待機兵が近接敵を自動で殴る」最小版。** M3 Phase 39（Stance & Aggro UI）の土台。

### MVP 挙動

```
Militia が Idle（移動目標なし・攻撃ジョブなし）
    ↓
Tick: 射程内（attackRange）に敵 Unit がいる？
    ↓ Yes
AttackManager.IssueAttack([self], enemy)  // 既存フロー
    ↓
追従・クールダウン・Kill は AttackManager 既存処理
```

### Aggro 対象

| 対象 | MVP |
|------|-----|
| 敵 **Unit**（Villager / Militia） | ✅ |
| 敵 **Building** | ❌ — 右クリック攻撃のみ（Phase 29 では Unit のみ） |
| Boar | ❌ — `BoarAttackManager` 経路を維持 |
| 味方 / 中立 | ❌ |

### チーム

- **Player Militia** → `UnitTeam.Enemy` を Aggro
- **CPU Militia** → `UnitTeam.Player` を Aggro
- **両チーム対称** — Player / CPU 同じロジック

### 優先順位（MVP）

| 状況 | 挙動 |
|------|------|
| プレイヤー **Move** 右クリック | **Move 優先** — 既存 `MoveCommand` が Attack キャンセル |
| 移動中（`HasMoveTarget`） | **Aggro しない** |
| 攻撃中（`AttackManager.IsUnitAttacking`） | Tick スキップ（二重 Issue 防止） |
| Boar 攻撃中 | **Aggro しない** — `BoarAttackManager.IsUnitAttackingBoar` |
| 採集 / 建築中 | Militia は `CanAttack` のみ対象 — **通常該当なし** |

---

## ④ 今回実装するもの

### 1. `UnitAggroManager.cs`（新規）

- `ISimulationTickable` — 20 TPS（`BoarAggroManager` 同型）
- `UnitManager.CopyUnitsTo` で全 Unit 走査 **または** 空間インデックス活用
- 各 Unit について `CanAggro(unit)` → 敵検索 → `IssueAggroAttack`

**`CanAggro` 条件（案）:**

```csharp
static bool CanAggro(Unit unit)
{
    if (unit == null || !unit.IsAlive || !unit.CanAttack)
        return false;
    if (unit.HasMoveTarget)
        return false;
    if (AttackManager.IsUnitAttacking(unit))
        return false;
    if (BoarAttackManager.IsUnitAttackingBoar(unit))
        return false;
    return true;
}
```

**敵検索（案）:**

```csharp
UnitTeam enemyTeam = unit.Team == UnitTeam.Player ? UnitTeam.Enemy : UnitTeam.Player;
Unit enemy = UnitSpatialIndex.FindNearestUnit(
    unit.transform.position,
    enemyTeam,
    candidate => candidate != null && candidate.IsAlive);
if (enemy == null)
    return;
if (!unit.IsNear(enemy.transform.position, unit.AttackRange))
    return;
// IssueAttack
```

- **検索半径:** `unit.AttackRange`（Militia = 2m）。将来 Phase 39 で aggroRange > attackRange 拡張可
- **Issue 方法:** `AttackManager.IssueAttack(aggroBuffer, enemy)` — `aggroBuffer` は static `List<Unit>(1)` 再利用

### 2. Editor / シーン

- **`Phase10SceneBuilder.CreateManagers`** — `UnitAggroManager` を Systems 配下に追加
- **`EnsureUnitAggroManager`** — 既存シーン用 optional メニュー `AoE → Add Unit Aggro (Phase10)`（Boar マネージャーと同パターン）

### 3. 既存コード — 変更最小

- **`AttackManager`** — 原則 **変更不要**（`IssueAttack` 再利用）
- **`MoveCommand`** — 既に `AttackManager.CancelForUnits` — **変更不要**
- **`Unit.State`** — 攻撃 tint 既存 — **変更不要**

---

## ⑤ 今回やらないこと

- Stand Ground / Defensive / Aggressive **スタンス UI**（**Phase 39 M3**）
- 弓兵 / 遠距離 Aggro（**Phase 35 Archer** 以降）
- 建築物への自動 Aggro
- 追撃（Leash）— 敵が射程外に出ても追わない MVP 可、または AttackManager 既存追従に任せる
- CPU Aggro AI の新規 Command 化（Manager 直接 `IssueAttack` で OK）
- Militia 以外の将来兵種 — **現状 `CanAttack` 全ユニット** が対象（Enemy Dummy 等も含む — 問題なければそのまま）

---

## ⑥ 実装ステップ（推奨）

| Step | 内容 |
|------|------|
| 29-1 | `UnitAggroManager.cs` — CanAggro + FindNearest + IssueAttack |
| 29-2 | `Phase10SceneBuilder` — CreateManagers + Ensure メニュー |
| 29-3 | Play 確認 + ドキュメント更新 |

---

## ⑦ 技術メモ

### AttackManager.IssueAttack との関係

- Aggro も **プレイヤー右クリック攻撃と同じ `activeJobs` パイプライン**
- `IssueAttack` 内で Gather / Food / Mineral / Boar キャンセル済み — Aggro 側で追加キャンセル不要
- 二重 Issue 防止: `CanAggro` で `IsUnitAttacking` をチェック

### CpuMilitaryAiManager との共存

- ウェーブ攻撃は `IssueAttack(militiaList, target)` — Aggro も同 API
- ウェーブ中に Militia が移動中 → Aggro しない（`HasMoveTarget`）
- ウェーブ到着後 Idle → 近接敵がいれば Aggro 開始（**期待動作**）

### 性能

- 全 Unit 線形走査（Phase 10 規模 ~20 体）で十分
- 将来 Benchmark 200+ 体時は `UnitSpatialIndex` 半径クエリ最適化（Phase 29 では不要）

### Tick 順序

- `UnitAggroManager` は `AttackManager` と同 Tick — 同一フレーム内 Issue → 次 Tick から Process
- `SimulationTick` 登録順は既存 Manager と同列で OK

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`
- **Setup:** `AoE → Setup Phase10 Scene` — `UnitAggroManager` 自動追加
- **既存シーン:** `AoE → Add Unit Aggro (Phase10)` → 保存 → Play

---

## ⑨ 完了条件（Phase 29 MVP）

- [ ] **Player Militia Idle** — CPU Villager / Militia が attackRange 内 → **自動攻撃開始**
- [ ] **CPU Militia Idle** — Player ユニット接近 → **自動反撃**（ウェーブ外でも）
- [ ] **Move 優先** — Move 命令後は Aggro せず移動（到着後 Idle なら再 Aggro 可）
- [ ] **移動中** — 移動中 Militia は Aggro しない
- [ ] **右クリック攻撃** — 既存通り動作（回帰）
- [ ] **Boar / 羊 / Mill / 4 資源** 回帰
- [ ] Console エラーなし
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `04_M2_5` Phase 29 を ✅

---

## ⑩ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene`（または `Add Unit Aggro (Phase10)`）→ Play
2. Barracks から **Militia 1 体** 生産 → TC 付近で **待機**
3. CPU Villager / Militia を **近づける**（右クリック命令なし）→ **自動攻撃** 開始
4. Militia 選択 → **地面右クリック Move** → 移動中は他敵を **無視**
5. 到着後 Idle → 敵再接近 → **再 Aggro**
6. Militia **右クリック敵** — 従来通り攻撃（回帰）
7. CPU ウェーブ（30 秒）— 既存攻撃も動作
8. Boar / Sheep / Berry / Mill 回帰
9. Console エラーなし

Phase 29 のみ実装。**Phase 30 以降（CPU 4 Resources 等）** に触れない。

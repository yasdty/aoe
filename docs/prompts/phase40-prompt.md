# Phase 40 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜39 完了（M3 Counter System 含む）  
> **マイルストン:** M3 Military — **Stance, Aggro & Attack-Move**  
> **ロードマップ:** [07_M3_MILITARY_PHASES.md](../07_M3_MILITARY_PHASES.md)  
> **Balance 方針:** [12_GAMEPLAY_BALANCE_MODE.md](../12_GAMEPLAY_BALANCE_MODE.md) — 本 Phase では **GameplayBalance 層は触らない**  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 40 実装（Stance, Aggro & Attack-Move）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 40 のみ実装。** Phase 29 `UnitAggroManager` と `SelectionManager` 右クリック経路を **拡張**（rewrite 禁止）。

---

## ① 目的

AoE2 マイクロの前提となる **スタンス** と **攻撃移動** を MVP 導入する。Phase 29 の全兵種 Aggro（Archer 含む）をスタンスで制御し、Stand Ground で「追撃しない」挙動を実現する。

| 項目 | MVP |
|------|-----|
| スタンス | **Aggressive**（既定）/ **Defensive** / **Stand Ground** |
| UI | 軍事ユニット選択時 OnGUI 3 ボタン（M5 uGUI 移行は後回し） |
| Aggro | `UnitAggroManager` — スタンスに応じて自動攻撃可否・追撃可否を分岐 |
| 攻撃移動 | **A 押下 + 右クリック地面** → 移動しながら射程内敵を攻撃 |
| 弓兵 | `attackRange 6` — Aggressive/Defensive は Detect 範囲内 Aggro、Stand Ground は **射程内のみ**（移動しない） |

**やらないこと:** Formation（Phase 41）/ 建築への Attack-Move / CPU スタンス AI / Balance Mode / 弾丸飛翔 / Patrol / uGUI 本格 HUD

---

## ② スタンス定義（MVP 暫定）

| スタンス | Aggro | 追撃（AttackManager 接近移動） | 備考 |
|----------|-------|-------------------------------|------|
| **Aggressive** | ✅ Detect 範囲内 | ✅ 既存通り | Phase 29 相当（全 `CanAttack` ユニット） |
| **Defensive** | ✅ Detect 範囲内 | ✅ 既存通り | MVP では Aggressive と **同一 Aggro**（UI のみ先行。将来「被弾後のみ」拡張可） |
| **Stand Ground** | ✅ **AttackRange 内のみ** | ❌ **移動しない** — 射程外敵は無視 | Archer はその場から射撃 |

- 新規 enum 例: `UnitCombatStance { Aggressive, Defensive, StandGround }`
- ランタイム状態は **`Unit` コンポーネント** に保持（`UnitData` は触らない）
- 既定: **Aggressive**（既存 Phase 29 挙動維持）
- Villager / 非戦闘ユニット: スタンス UI **非表示**（`CanAttack`  false）

---

## ③ 攻撃移動（Attack-Move）

```
A キー押下中 + 右クリック地面
    ↓
AttackMoveCommand(units, destination)
    ↓
移動中も Aggro 対象 — 射程内敵を IssueAttack
    ↓
目的地到着 or 敵全滅 → 通常 Idle
```

| 項目 | MVP |
|------|-----|
| 入力 | **A** ホールド + 右クリック（地面）。敵 Unit 右クリックは **従来の直接 Attack** 優先 |
| 実装 | `AttackMoveManager`（`ISimulationTickable`）または `Unit` + `AttackMoveJob` リスト — **small diff 優先** |
| Move との差 | 通常 Move = Aggro しない（Phase 29）。Attack-Move = **移動中も Aggro/攻撃** |
| キャンセル | 通常 Move / 新 Attack-Move / 採集命令でキャンセル — `MoveCommand` パターン踏襲 |

**AttackMove 中の Stand Ground:** ユニット個別スタンスを尊重 — Stand Ground ユニットは **移動せず射撃のみ**（Attack-Move グループに混在可）。

---

## ④ 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| Aggro 現状 | `UnitAggroManager.cs` — `CanAggro` / `GetAggroDetectRange` |
| 右クリック | `SelectionManager.HandleMoveCommand` — 地面 Move / 敵 Attack 分岐 |
| 命令 | `GameCommands.cs` — `MoveCommand`（Attack キャンセル）/ `AttackUnitCommand` |
| 戦闘追従 | `AttackManager.cs` — 射程外接近。Stand Ground 用 **追撃抑制フック** を追加 |
| 入力 | `RTSInputActionsBuilder.cs`, `RTSInputReader.cs` — Q/E 等の既存パターン |
| Phase 29 意図 | `phase29-prompt.md` — Move 優先 / 移動中 Aggro なし |

---

## ⑤ 実装タスク（small diff）

### 1. スタンス Data / Unit

- `UnitCombatStance` enum（`Assets/Scripts/Combat/` 可）
- `Unit.cs` — `CombatStance` プロパティ + `SetCombatStance` / 既定 Aggressive
- `UnitPool.PrepareForSpawn` — スタンス Aggressive にリセット

### 2. UnitAggroManager 拡張

- `CanAggro` — Stand Ground でも **AttackRange 内**なら Aggro 可（`HasMoveTarget` ルールは Stand Ground + 非 Attack-Move 時のみ適用）
- Detect 範囲:
  - Aggressive / Defensive: 既存 `GetAggroDetectRange`（max(attackRange+2, 5)）
  - Stand Ground: **`attackRange` のみ**（弓兵 6m 内のみ反応）
- Attack-Move 中ユニット: `CanAggro` = true（移動中でも Aggro）

### 3. AttackManager — Stand Ground 追撃抑制

- `ProcessAttackCycle` 接近移動前に `if (attacker.IsStandGround && !IsAttackMoving(attacker))` → **接近せず**（射程外はジョブ解除 or 待機）
- 弓兵 Stand Ground: 射程内なら射撃、射程外は **動かない**

### 4. AttackMoveManager + Command

- `AttackMoveCommand` — `MoveCommand` 同型だが AttackMove ジョブ登録
- Tick: 各ユニット `SetMoveTarget` + Detect 範囲内敵へ `IssueAttack`
- `AttackManager.CancelForUnits` / Move 命令で AttackMove ジョブもクリア

### 5. 入力

- `RTSInputActionsBuilder` — `AttackMove` アクション（`<Keyboard>/a`）— **Q 生産と競合しない**（A はホールド修飾キー）
- `RTSInputReader.WasAttackMoveModifierHeld()` — 右クリック時に `SelectionManager` が分岐
- `HandleMoveCommand` — A 押下中 + 地面 → `AttackMoveCommand`、それ以外は既存

### 6. UI（OnGUI MVP）

- `UnitStancePanelView.cs`（新規）— 軍事ユニット **1 体以上** 選択時、画面左下（Info Panel 上）に 3 ボタン  
  - `Aggressive` / `Defensive` / `Stand Ground`  
  - クリックで選択ユニット全員のスタンス変更
- `SelectionInfoPanelView` — 単体軍事ユニット時 `Stance: Aggressive` 1 行追加（任意だが推奨）

### 7. Phase10 SceneBuilder

- `UnitStancePanelView` 配線
- `AttackMoveManager` を Systems 配下に追加
- Input Action 更新 → **`AoE → Setup Phase10 Scene`**

---

## ⑥ 制約

- rewrite 禁止 / small diff only
- `.meta` 手書き禁止
- Phase 29〜39 回帰 — 通常 Move / 右クリック Attack / CPU ウェーブ / Counter ダメージ
- **Defensive ≒ Aggressive**（Aggro ロジック同一）— UI だけ先行 OK
- CPU はスタンス変更しない（Phase 41 で攻撃波改善可）

---

## ⑦ Play 確認

1. `AoE → Setup Phase10 Scene` → Play
2. **Aggressive（既定）** — Militia Idle → 5m 内敵へ AutoAggro（Phase 29 回帰）
3. **Archer Aggressive** — 8m Detect で Idle 射撃開始
4. **Stand Ground** — Militia 選択 → Stand Ground → 敵が 2m 外 → **追わない**。進入後のみ攻撃
5. **Archer Stand Ground** — 6m 内のみ射撃、それ以外は **移動しない**
6. **Attack-Move** — Militia 複数選択 → **A + 右クリック地面** → 移動し敵に接触して戦闘
7. 通常 **右クリック Move** — 移動中 Aggro しない（Phase 29 回帰）
8. Spearman +12 / Pierce ログ（Phase 39 回帰）
9. Console エラーなし

---

## ⑧ 完了時ドキュメント

- [ ] `IMPLEMENTATION_STATUS.md` — Phase 40 ✅
- [ ] `07_M3_MILITARY_PHASES.md` — Phase 40 ✅
- [ ] 本プロンプト — Play 確認待ち → ✅

---

Phase 40 のみ。**Phase 41 Formation には触れない。**

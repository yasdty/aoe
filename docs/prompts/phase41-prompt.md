# Phase 41 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜40 完了（M3 Stance & Attack-Move 含む）  
> **マイルストン:** M3 Military — **Formation（隊列移動・軽量 Separation）** — **M3 最終 Phase**  
> **ロードマップ:** [07_M3_MILITARY_PHASES.md](../07_M3_MILITARY_PHASES.md)  
> **Balance 方針:** [12_GAMEPLAY_BALANCE_MODE.md](../12_GAMEPLAY_BALANCE_MODE.md) — 本 Phase では **GameplayBalance 層は触らない**（Phase 42 先頭で実装）  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 41 実装（Formation）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 41 のみ実装。** 既存 `GroupMoveFormation` / `MoveCommand` / `AttackMoveManager` を **拡張**（rewrite 禁止）。

**M3 完了:** 本 Phase 完了で Milestone 3 Military が **完了**。次は [08_M4_GAMEPLAY_PHASES.md](../08_M4_GAMEPLAY_PHASES.md) Phase 42（**Gameplay Balance Mode 先頭** + Age Up）。

---

## ① 目的

グループ移動時に **隊形を維持したまま** 目的地へ進み、移動中の **重なりを軽減**する。CPU 攻撃波も同じ隊列 API を使い、新兵種混在が Play で確認できるようにする。

| 項目 | MVP |
|------|-----|
| ソフト隊列 | 移動中も **スロット相対位置** を維持（現状は「到着先グリッドのみ」） |
| Separation | 同一チーム近接ユニットの **軽量押し出し**（RVO / NavMesh 不要） |
| 対象命令 | `MoveCommand` + `AttackMoveCommand` + CPU 攻撃波の前進分岐 |
| CPU | `CpuMilitaryAiManager` — 攻撃波ログに **兵種内訳**、前進は `GroupMoveFormation` 経由 |
| 既存 | `GroupMoveFormation` の √n グリッド・spacing=2f を **正本**として再利用 |

**やらないこと:** GameplayBalance / Age Up（Phase 42）/ 本格 Pathfinding / RVO / 建築回避 / Patrol / 編隊ホットキー / uGUI / CPU 相性 AI / スタンス変更

---

## ② 現状ギャップ（必読）

| 現状 | 問題 |
|------|------|
| `GroupMoveFormation.AssignMoveTargets` | 各ユニットに **最終スロット座標** を 1 回セットするだけ |
| `Unit.TickMovement` | 直線移動。他ユニットを考慮しない |
| 結果 | 大人数 Move で **経路が交差・重なる**。到着後も密集 |
| `CpuMilitaryAiManager.LaunchAttackWave` | ターゲット不在時 `UnitPositionOffsets.ApplyRingOffset` で個別前進（隊列と不統一） |
| CPU 収集 | `CollectCpuAttackUnits` は Militia / Spearman / Archer / Cavalry / Scout **既に混在** — ログと前進 API の統一が主タスク |

---

## ③ 設計方針（MVP）

### A. ソフト隊列（Formation Move）

```
MoveCommand / AttackMoveCommand 実行
    ↓
FormationMoveManager.Register(units, center, spacing)
  - 各 unit に slotOffset（グリッド）を記録
  - formationCenter = 選択群の重心（または右クリック地点）
  - destination = center（Move）または AttackMove の各 slot 先
    ↓
Tick: formationCenter を destination 方向へ advance
  - 各 unit.SetMoveTarget(formationCenter + slotOffset)
  - 全員到着 → ジョブ解除
```

| 項目 | MVP |
|------|-----|
| 中心の進行 | `formationCenter` を **最遅ユニット基準** または **固定リーダー（先頭 slot）** で前進 — **small diff 優先** |
| AttackMove | 既存 `AttackMoveManager` と **統合 or 委譲** — 二重 Tick 禁止 |
| キャンセル | 既存 `CancelForUnits` / 新 Move / Gather / Attack と同様に Formation ジョブもクリア |
| Villager | Formation 対象 **任意**（MVP は **CanAttack ユニットのみ** でも可 — 軍事 Phase のため Player 軍事 Move 必須） |

### B. 軽量 Separation

```
UnitManager.TickSimulation（移動後）
    ↓
UnitSeparation.Apply(units) — 同一 Team、距離 < minSep（例 1.2m）
    ↓
相手方向へ小さく nudge（最大 step 上限、Y 固定）
```

| 項目 | MVP |
|------|-----|
| 対象 | **移動中**（`HasMoveTarget`）のユニット優先。Idle 重なりも軽く押し出し可 |
| 性能 | O(n²) 許容（Phase10 規模 ~30 体）。SpatialIndex 利用 **任意** |
| チーム | **同一 Team のみ** — 敵との押し合いは不要 |
| 採集 / 建築 | Villager が Farm / Build 立位で **過剰に跳ねない** — 移動中のみ、または CanAttack のみに限定 |

---

## ④ 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| グリッド | `GroupMoveFormation.cs` — `AssignMoveTargets` / `GetGridDimensions` |
| 命令 | `GameCommands.cs` — `MoveCommand`, `AttackMoveCommand` |
| 攻撃移動 | `AttackMoveManager.cs` — Register / Tick / Cancel |
| 移動 Tick | `UnitManager.cs`, `Unit.cs` — `TickMovement`, `SetMoveTarget` |
| リング配置 | `UnitPositionOffsets.cs` — 攻撃接近・CPU 前進の既存パターン |
| CPU 波 | `CpuMilitaryAiManager.cs` — `LaunchAttackWave`, `CollectCpuAttackUnits` |
| Phase 40 回帰 | Stand Ground / Attack-Move / Aggro |

---

## ⑤ 実装タスク（small diff）

### 1. FormationMoveManager（新規）

- `Assets/Scripts/Selection/` または `Units/` — `ISimulationTickable`
- `Register(IReadOnlyList<Unit> units, Vector3 destination, float spacing)` — slotOffset 計算は `GroupMoveFormation` と **同一式**
- Tick: 隊形中心を更新し各ユニットへ `SetMoveTarget`
- `CancelForUnits` / `IsUnitInFormation` — `AttackMoveManager` から呼べる API
- `Phase10SceneBuilder` — Systems 配下に追加（Phase 40 `AttackMoveManager` パターン踏襲）

### 2. MoveCommand / AttackMoveCommand 配線

- `MoveCommand.Execute` — `GroupMoveFormation.AssignMoveTargets` の **代わり or 直後** に `FormationMoveManager.Register`
- `AttackMoveManager` — 個別 destination を Formation ジョブに統合するか、AttackMove 専用 Tick を Formation に委譲（**二重管理を避ける**）

### 3. UnitSeparation（新規 static または Manager）

- `UnitManager.TickSimulation` 末尾で `UnitSeparation.Apply(fixedDeltaTime)` 
- 定数: `minSeparation`（1.0〜1.5m）, `pushStrength`（小さく）
- **CanAttack ユニットのみ** に限定するのが安全（Villager 採集乱れ防止）

### 4. GroupMoveFormation 拡張（任意・推奨）

- `TryGetSlotOffset(int index, int count, float spacing, out Vector3 offset)` を public 化し Formation / AttackMove / CPU で **DRY**

### 5. CpuMilitaryAiManager

- 前進分岐（Player TC / Unit 不在時）— `UnitPositionOffsets` ループを **`GroupMoveFormation.AssignMoveTargets` + FormationMoveManager** に置換
- Debug.Log 拡張例:  
  `[CPU Military] Attack wave: 9 units (Militia×3, Spearman×2, Archer×2, Cavalry×1, Scout×1) advancing`
- `IssueAttack` 分岐は **現状維持**（ターゲット発見時は個別接近で OK）

### 6. Phase10 SceneBuilder

- `FormationMoveManager` 配線
- **`AoE → Setup Phase10 Scene`** 必須

---

## ⑥ 制約

- rewrite 禁止 / small diff only
- `.meta` 手書き禁止
- Phase 29〜40 回帰 — Aggro / Stand Ground / Attack-Move / Counter +12 / Q/E 生産
- **GameplayBalance 層は触らない**
- NavMesh / A* / 本格 RVO **禁止**
- CPU スタンス変更 **不要**（Phase 40 方針維持）

---

## ⑦ Play 確認

1. `AoE → Setup Phase10 Scene` → Play
2. **Player 大人数 Move** — Militia 8+ 選択 → 右クリック遠方 → 移動中に **横一列グリッドが崩れにくい**（完全固定でなくてよいが、到着前から密集しにくい）
3. **Separation** — 2 体を同地点へ Move → 完全重ならず **わずかに離れる**
4. **Attack-Move** — A + 右クリック → 移動中 Aggro + 隊形維持（Stand Ground 混在は Phase 40 回帰）
5. **CPU 攻撃波** — 2〜3 波待機 → Console に **兵種内訳付き** ログ。Archer / Cavalry が混在
6. Phase 39 Spearman +12 / Phase 38 Stable 生産 / Phase 36 Archer 射程 — 回帰
7. Console エラーなし

**簡易目視:** Scene ビューで 10 体 Move — 移動経路で完全に 1 点に重ならないこと。

---

## ⑧ 完了時ドキュメント

- [ ] `IMPLEMENTATION_STATUS.md` — Phase 41 ✅、**M3 Military ✅ 完了**
- [ ] `07_M3_MILITARY_PHASES.md` — Phase 41 ✅、一覧表更新
- [ ] 本プロンプト — Play 確認待ち → ✅
- [ ] `08_M4_GAMEPLAY_PHASES.md` — 必要なら Phase 42 への導線確認のみ（Balance 実装は Phase 42）

---

Phase 41 のみ。**Phase 42 Gameplay Balance / Age Up には触れない。**

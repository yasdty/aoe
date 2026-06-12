# Phase 58 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜57 完了（Entity ID & PlayerId 基盤）  
> **マイルストン:** M6 — **CPU Command Queue**（4 人 CPU×3 の前段）  
> **ロードマップ:** [10_M6_MULTIPLAYER_FOUNDATION.md](../10_M6_MULTIPLAYER_FOUNDATION.md)  
> **関連:** Phase 16 `CommandQueue` / Phase 57 `EntityRegistry` / `PlayerId` / `CpuEconomyAiManager` / `CpuMilitaryAiManager`  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 58 実装（CPU Command Queue）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 58 のみ実装。** CPU AI の意思決定を **`CommandQueue.Enqueue` 経由**に統一し、**PlayerId ごとに AI を複数インスタンス化できる形**にする（rewrite 禁止）。

**前提:** 現状 **1 人間（Player0）vs 1 CPU（Enemy = Player1 相当）**。CPU は `GatherManager` / `AttackManager` / `BuildingPlacementManager` 等を **直接呼び出し**。プレイヤー操作のみ `CommandQueue` 経由。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| CPU → Queue | `CpuEconomyAiManager` / `CpuMilitaryAiManager` が **全ての Sim 変更**を `CommandQueue.Enqueue` 経由に |
| PlayerId | 各 AI に `PlayerId` を保持（MVP: **Player1** = 既存 `UnitTeam.Enemy`）。`PlayerIdMapping.ToLegacyTeam` でチーム解決 |
| 複数 CPU 準備 | 同一コンポーネントを **PlayerId 指定で複数配置可能**（Phase 59 で 3 体 — 本 Phase はシーン 1 体のままで可） |
| Command 拡張 | CPU 専用に不足する Command を **最小追加**（例: チーム建築開始） |
| CommandLog | 発行者 `PlayerId` を記録（プレイヤー = Player0 / CPU = 各 PlayerId） |
| 回帰 | **1v1 CPU** の経済・攻撃波・勝敗が Phase 57 以前と同様 |

**やらないこと:** 4 隅スポーン / マッチ設定 UI / PlayerId 2〜3 の実スポーン / 2v2 / Fog / 大マップ / 全 Command の EntityId 化（Phase 57 未移行分はそのまま可）/ 決定論（Phase 63）/ リプレイ

---

## ② 現状（読み取り用）

### プレイヤー（CommandQueue 経由 ✅）

| 操作 | Command |
|------|---------|
| 移動 | `MoveCommand`（EntityId 化済み） |
| 攻撃ユニット | `AttackUnitCommand`（EntityId 化済み） |
| 採集・狩り等 | `GatherCommand` / `GatherFoodCommand` / `HuntFoodCommand` 等（Unit 参照） |
| 生産 | `TrainVillagerCommand` / `TrainMilitiaCommand` 等 |
| 建築確定 | `BuildConfirmCommand` |

### CPU（Manager 直接呼び出し ❌）

| 領域 | 直接呼び出し例 |
|------|----------------|
| 経済 AI | `GatherManager.IssueGatherCommand`, `FoodGatherManager.*`, `MineralGatherManager.*`, `BuildingPlacementManager.TryStartTeamConstruction`, `townCenter.TryQueueVillagerProduction` |
| 軍事 AI | `BuildingPlacementManager.TryStartTeamConstruction`, `barracks.TryQueue*`, `AttackManager.IssueAttack`, `FormationMoveManager.Register`, `GameSessionManager.TryAgeUpForTeam` |
| チーム固定 | `CpuAiCoordination.CpuTeam = UnitTeam.Enemy`（定数） |

### CommandQueue

- 単一 FIFO キュー。Tick 先頭で **全 pending を Execute** + `CommandLog.Record`
- `GameSessionManager.IsGameOver` 中は Enqueue 無視

**Phase 58 後:** CPU の Tick 評価も **Enqueue のみ** — Execute は既存 Queue と同じ Tick で処理されればよい（AI Tick 内 Enqueue → 同一 Tick または次 Tick は実装判断。1v1 回帰優先）。

---

## ③ 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| Queue | `CommandQueue.cs`, `IGameCommand.cs`, `CommandLog.cs` |
| Commands | `GameCommands.cs`（既存 Command 一覧） |
| 経済 AI | `CpuEconomyAiManager.cs`, `CpuAiCoordination.cs` |
| 軍事 AI | `CpuMilitaryAiManager.cs` |
| Player | `PlayerId.cs`, `PlayerIdMapping.cs` |
| 建築 | `BuildingPlacementManager.TryStartTeamConstruction` |
| ロードマップ | [10_M6_MULTIPLAYER_FOUNDATION.md](../10_M6_MULTIPLAYER_FOUNDATION.md) § Phase 58 |

---

## ④ 実装タスク

### 1. Command に発行者 PlayerId（小拡張）

```csharp
// 例 — 新規 interface 推奨（既存 Command 一括変更は最小に）
public interface IPlayerCommand
{
    PlayerId IssuingPlayerId { get; }
}
```

- プレイヤー UI から Enqueue する Command は **`PlayerId.Player0`**
- CPU から Enqueue する Command は **その AI の `PlayerId`**
- `CommandLog.Record` — `team` フィールドを `PlayerIdMapping.ToLegacyTeam(issuingPlayerId)` で記録（既存 struct 互換）

### 2. CPU 用 Command（不足分のみ追加）

| 優先 | 操作 | 方針 |
|------|------|------|
| 必須 | チーム建築開始 | 新規 `CpuStartTeamConstructionCommand`（`PlacedBuildingData` + position + builder EntityId or Unit 参照 — small diff 優先） |
| 必須 | 攻撃波前進 | 既存 `MoveCommand` を Enqueue（EntityId 化済み） |
| 必須 | 攻撃ユニット / 建築 | 既存 `AttackUnitCommand` / `AttackBuildingCommand` を Enqueue |
| 必須 | 採集・狩り・鉱 | 既存 `Gather*` / `HuntFoodCommand` を Enqueue |
| 必須 | Villager / 兵士生産 | 既存 `TrainVillagerCommand` / `TrainMilitiaCommand` 等を Enqueue |
| 任意 | Feudal 昇格 | `AgeUpCommand` or 薄い `CpuAgeUpCommand` — `GameSessionManager.TryAgeUpForTeam` をラップ |

**禁止:** AI Tick 内で `GatherManager` / `AttackManager` / `FormationMoveManager` を **新規に直接呼ばない**（移行した箇所は Enqueue のみ）。

### 3. CpuEconomyAiManager リファクタ（small diff）

- `[SerializeField] PlayerId cpuPlayerId = PlayerId.Player1;` を追加
- `UnitTeam Team => PlayerIdMapping.ToLegacyTeam(cpuPlayerId);` で既存 `CpuAiCoordination.CpuTeam` 参照を置換（`CpuAiCoordination` も PlayerId 引数化 or インスタンス化を検討）
- `AssignIdleVillagers` / `TryBuildHouse` / `TryTrainVillager` 等 — 直接 Manager 呼び出しを **対応 Command の Enqueue** に置換
- Debug.Log の `[CPU Economy]` は維持可

### 4. CpuMilitaryAiManager リファクタ（small diff）

- 同上 `cpuPlayerId`（デフォルト Player1）
- `LaunchAttackWaveInternal` — `AttackManager` / `FormationMoveManager` → `AttackUnitCommand` / `AttackBuildingCommand` / `MoveCommand` Enqueue
- 兵舎・弓兵舎・厩舎建設・生産キュー — Enqueue 経由
- `ForceDebugAttackWave` / `CpuAttackPace` / Relaxed grace — **挙動維持**

### 5. CpuAiCoordination

- 定数 `CpuTeam` のみに依存しないよう **`PlayerId` / `UnitTeam` を引数**で渡せる形に（static のまま拡張でも可 — 複数 CPU で競合しないこと）

### 6. Phase10 シーン

- 既存 **CpuEconomyAiManager / CpuMilitaryAiManager 1 組**のまま Play 可
- `cpuPlayerId = Player1` がシリアライズされていれば Editor 変更不要でも可
- 必要なら `Phase10SceneBuilder` に PlayerId デフォルト設定を 1 行追加

### 7. 回帰

- Phase 57 Entity ID / ミニマップ / Combat VFX / HUD
- CPU 経済（House / Villager / 採集）・軍事（Barracks / 攻撃波）・Feudal 昇格
- 勝敗 → **R** 再開
- Console エラーなし
- `CommandLog` に CPU 発行 Command が `Player1`（または legacy Enemy）として記録されること

---

## ⑤ 制約

- small diff only / rewrite 禁止
- NavMesh 禁止
- `GetInstanceID()` 禁止
- **4 人マッチのスポーン・UI は Phase 59**
- Simulation の新規 GameObject 参照 Command は避ける（Phase 57 方針継続）
- AI の **評価間隔・閾値定数**は変更しない（挙動差分を出さない）

---

## ⑥ Play 確認

1. `Phase10.unity` — Play
2. 放置で CPU が Villager 増産・House・採集を継続
3. CPU 攻撃波が来る（Relaxed / Aggressive 両方 spot check）
4. プレイヤー移動・攻撃が従来どおり
5. Console: `CommandLog` に Move / Gather / Train / Attack が CPU 分も記録
6. 敵 TC 破壊 → 勝利 → **R** 再開

---

## ⑦ 完了時ドキュメント

- [ ] `IMPLEMENTATION_STATUS.md` — Phase 58 ✅
- [ ] `10_M6_MULTIPLAYER_FOUNDATION.md` — Phase 58 ✅
- [ ] 本プロンプト — ✅
- [ ] `phase57-prompt.md` の「次」リンクがあれば `phase58-prompt.md` 参照を確認

---

Phase 58 のみ。**Phase 59（4 人マッチ）・Phase 60（2v2）には触れない。**

> **次:** Phase 59 Four-Player Match（人間1 + CPU3）

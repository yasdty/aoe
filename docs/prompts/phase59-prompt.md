# Phase 59 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜58 完了（Entity ID / PlayerId / CPU Command Queue）  
> **マイルストン:** M6 — **Four-Player Match**（人間1 + CPU3）  
> **ロードマップ:** [10_M6_MULTIPLAYER_FOUNDATION.md](../10_M6_MULTIPLAYER_FOUNDATION.md)  
> **関連:** Phase 57 `EntityRegistry` / `PlayerId` / Phase 58 `CpuAiCommandQueue` / `ResourceManager` / `ProductionManager` / `GameSessionManager`  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 59 実装（Four-Player Match）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 59 のみ実装。** **人間 1 + CPU 3** の 4 人 FFA マッチを Play 可能にする（rewrite 禁止）。

**前提:** 現状 **1 人間（Player0）vs 1 CPU（Player1 相当）**。`ResourceManager` は `playerWood` / `enemyWood` の **2 値**。`UnitTeam` は `Player` / `Enemy` の 2 値。CPU AI は **経済・軍事それぞれ 1 インスタンス**。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| マッチ | **FFA — 人間1 + CPU3**（Player0 = 人間、Player1〜3 = CPU） |
| スポーン | マップ **4 隅** に TC + 初期 Villager（各 3 体目安） |
| 経済 | Wood / Food / Gold / Stone / Pop を **PlayerId 単位** |
| AI | PlayerId 1〜3 それぞれに `CpuEconomyAiManager` + `CpuMilitaryAiManager` |
| 勝利 | **最後の敵 Player** の TC 全破壊で勝利（自 TC 破壊で敗北） |
| 互換 | 1v1 モードは **デフォルト維持** or マッチ設定で切替可 |

**やらないこと:** 2v2 / `TeamId`（Phase 60）/ 大型マップ拡張（Phase 61）/ 敵 HP 表示・修理・複数建設（Phase 62〜64）/ Fog（Phase 65）/ 決定論（Phase 66）/ 本格マッチメイキング UI

---

## ② 現状（読み取り用）

### PlayerId / Team

| 項目 | 値 |
|------|-----|
| `PlayerId` | Player0〜3 定義済み |
| `PlayerIdMapping` | Player0 → `UnitTeam.Player`、他 → `UnitTeam.Enemy`（**2 値マッピング**） |
| `EntityRegistry` | Unit / Building / Resource に int ID。Move / AttackUnit は ID 化済み |

### 経済・人口

| 項目 | 値 |
|------|-----|
| `ResourceManager` | `playerWood/Food/Gold/Stone` + `enemyWood/...` の **2 セットのみ** |
| `PopulationManager` | `UnitTeam` キー（2 チーム） |
| `ProductionManager` | `GetTownCenterForTeam(UnitTeam)` — チームごと最初の TC |

### AI（Phase 58）

| 項目 | 値 |
|------|-----|
| `CpuEconomyAiManager` | `[SerializeField] PlayerId cpuPlayerId` — Command Queue 経由 |
| `CpuMilitaryAiManager` | 同上 + `opponentPlayerId = Player0` |
| シーン | Phase10 に **各 1 インスタンス** |

### 勝敗

| 項目 | 値 |
|------|-----|
| `GameSessionManager` | TC 破壊 → `UnitTeam.Player` 敗北 / `UnitTeam.Enemy` 勝利 の **2 陣営判定** |

### シーン（Phase10）

| 要素 | 配置 |
|------|------|
| Player TC | (0, 0, 0) |
| CPU TC | (0, 0, -60) |
| 地面 | Sandbox scale 18×18（Phase 35） |

**Phase 59 後:** 4 隅スポーン + CPU×3 が同時に経済・軍事を回ること。

---

## ③ 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| Player | `PlayerId.cs`, `PlayerIdMapping.cs`, `EntityRegistry.cs` |
| 経済 | `ResourceManager.cs`, `PopulationManager.cs` |
| 生産 | `ProductionManager.cs`, `TownCenter.cs` |
| 勝敗 | `GameSessionManager.cs`, `BuildingHealth.cs` |
| AI | `CpuEconomyAiManager.cs`, `CpuMilitaryAiManager.cs`, `CpuAiCommandQueue.cs` |
| ユニット | `Unit.cs`（`Team` / `PlayerId` 所有権） |
| シーン | `Phase10SceneBuilder.cs` |
| ロードマップ | [10_M6_MULTIPLAYER_FOUNDATION.md](../10_M6_MULTIPLAYER_FOUNDATION.md) § Phase 59 |

---

## ④ 実装タスク

### 1. マッチ設定（MVP）

```csharp
// 例 — Core に薄い設定で可
public enum MatchMode { OneVsOneCpu, FourPlayerFfa }

public static class MatchSettings
{
    public static MatchMode Mode { get; }
    public static bool IsCpu(PlayerId id);
    public static IReadOnlyList<PlayerId> ActivePlayers { get; }
}
```

- **デフォルト:** `FourPlayerFfa`（本 Phase の検証対象）
- Debug メニュー or 簡易 HUD トグルで `OneVsOneCpu` に戻せると回帰しやすい
- `GameSessionManager` / `Phase10SceneBuilder` がモードを参照

### 2. PlayerId 単位の経済・人口

**方針（small diff 優先）:** `ResourceManager` / `PopulationManager` を **PlayerId キーの配列 or Dictionary** に拡張。

| API 例 | 備考 |
|--------|------|
| `GetWood(PlayerId id)` | 既存 `GetWood(UnitTeam)` は `PlayerIdMapping` 経由で委譲 |
| `TrySpendWood(PlayerId id, float amount)` | CPU 3 体分が独立 |
| `GetCurrentPopulation(PlayerId id)` | Pop cap も PlayerId 単位 |

- 既存 `playerWood` / `enemyWood` フィールドは **Player0 / Player1** にマッピングし、Player2/3 は新フィールド
- HUD: 人間は Player0 のみ表示。CPU 情報は Debug HUD or 折りたたみ（MVP: Player1 のみ表示でも可 — **3 CPU 全員分が Sim 上独立していれば OK**）

### 3. Unit / Building の PlayerId 所有権

- `Unit` に `PlayerId OwnerId`（SerializeField or スポーン時設定）を追加、既存 `Team` と同期
- `TownCenter` / 配置建築も `PlayerId` を保持（`UnitTeam` 併用可）
- `PlayerIdMapping.ToLegacyTeam` — **暫定:** Player0 = Player、Player1〜3 = Enemy（Phase 60 で Team 細分化）

### 4. 4 隅スポーン（Phase10SceneBuilder）

| Player | TC 位置（例 — 地面 scale に合わせ調整） |
|--------|------------------------------------------|
| Player0 | 南西 `(−80, 0, −80)` |
| Player1 | 北東 `(80, 0, 80)` |
| Player2 | 北西 `(−80, 0, 80)` |
| Player3 | 南東 `(80, 0, −80)` |

- 各 TC 周辺に初期 Villager 3 体 + 近傍に木・Berry（最低限 — 大規模再配置は Phase 61）
- カメラ初期焦点: Player0 TC 付近 or マップ中心
- `MatchMode.OneVsOneCpu` 時は **従来の 2 TC 配置**を維持

### 5. CPU AI ×3

- `Phase10SceneBuilder` — `CpuEconomyAiManager` / `CpuMilitaryAiManager` を **PlayerId 1, 2, 3 用に 3 組**配置
- 各 `cpuPlayerId` をシリアライズ
- `opponentPlayerId` — MVP: **全 CPU が Player0 を主目標**（互いに攻撃は `UnitTeam.Enemy` 同士で可）
- `CpuMilitaryAiManager` の攻撃波 — 各 CPU が独立タイマー（同時多発 wave は許容）

### 6. 勝利条件の拡張

- `GameSessionManager.OnTownCenterDestroyed` — **PlayerId** ベースに
- Player0 の TC 全滅 → Defeat
- **全敵 PlayerId（1〜3）の TC 全滅** → Victory
- 途中で 1 CPU が脱落しても、残り CPU がいれば継続

### 7. 戦闘・選択の暫定ルール

- 人間は **自軍（Player0）のみ** 選択・命令（現状維持）
- CPU 同士・CPU vs 人間の戦闘は Sim 上動作すれば OK
- 敵ユニット選択は **Phase 62** — 本 Phase では触れない

### 8. 回帰

- `MatchMode.OneVsOneCpu` で Phase 58 以前と同等の 1v1 Play
- `FourPlayerFfa` で 4 隅 + CPU×3 経済・攻撃波
- CommandLog に `player=Player1/2/3` が記録されること
- 勝敗 → **R** 再開
- Console エラーなし

---

## ⑤ 制約

- small diff only / rewrite 禁止
- NavMesh 禁止
- `GetInstanceID()` 禁止
- **2v2 / TeamId は Phase 60**
- **大型マップ・資源の本格再配置は Phase 61**（4 隅が Play 可能なら最小資源で可）
- Simulation の新規 GameObject 参照 Command は避ける（Phase 57 方針継続）
- AI の **評価間隔・閾値定数**は変更しない（挙動差分を出さない）

---

## ⑥ Play 確認

1. `AoE → Setup Phase10 Scene` を再実行
2. `Phase10.unity` — Play（`FourPlayerFfa`）
3. 4 隅に TC + Villager が存在
4. 放置で CPU×3 が Villager 増産・採集・House を継続
5. CPU×3 から攻撃波（時間差でも可）
6. 1 CPU の TC を破壊 → ゲーム継続
7. 全敵 TC 破壊 → **VICTORY**
8. `MatchMode.OneVsOneCpu` に切替 → 従来 1v1 回帰

---

## ⑦ 完了時ドキュメント

- [x] `IMPLEMENTATION_STATUS.md` — Phase 59 ✅
- [x] `10_M6_MULTIPLAYER_FOUNDATION.md` — Phase 59 ✅
- [x] 本プロンプト — ✅
- [x] `README.md` — M6 表（必要時）

---

Phase 59 のみ。**Phase 60（2v2）・Phase 61（大マップ）・Phase 62〜64（敵HP/修理/複数建設）には触れない。**

> **次:** [phase60-prompt.md](phase60-prompt.md) — Team & 2v2（未作成）

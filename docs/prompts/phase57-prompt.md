# Phase 57 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜56 完了（M5 Visual / UI / Combat VFX）  
> **マイルストン:** M6 — **Entity ID & PlayerId**（4 人対戦の土台）  
> **ロードマップ:** [10_M6_MULTIPLAYER_FOUNDATION.md](../10_M6_MULTIPLAYER_FOUNDATION.md)  
> **関連:** Phase 16 `IGameCommand` / `CommandQueue` / `UnitTeam` / `ResourceManager` チーム別資源  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 57 実装（Entity ID & PlayerId）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 57 のみ実装。** int **Entity ID** と **PlayerId 0〜3** の基盤（rewrite 禁止）。

**前提:** 現状は **1 人間（Player）vs 1 CPU（Enemy）** の 2 陣営。`IGameCommand` は **GameObject / Unit 参照**を保持。Phase 59〜60 で 4 人・2v2 を入れるため、**先に ID 基盤だけ**作る。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| Entity ID | `EntityRegistry` — Unit / Building / Resource（+ 必要なら ConstructionSite）に **一意 int ID** |
| 解決 API | `EntityRegistry.Get<T>(id)` / `TryGet` / 登録・解除は Spawn / Pool Return / Destroy と同期 |
| PlayerId | `PlayerId` 0〜3 — 憲法 4 チーム目標。enum or readonly struct |
| マッピング | `PlayerId` ↔ 既存 `UnitTeam`（当面 **Player0 = Player, 他 = Enemy 系** で可 — 全 Manager 一括置換は Phase 59） |
| Command | **新規 Command** は Entity ID 参照。既存 Command は **adapter** or 段階移行（1〜2 種の移行例で十分） |
| 禁止 | `GetInstanceID()` — 憲法違反 |

**やらないこと:** 4 隅スポーン / マッチ設定 UI / 2v2 / Fog / CPU×3 / Phase 58 CPU Command 化 / 決定論（Phase 63）/ リプレイ

---

## ② 現状（読み取り用）

| 項目 | 値 |
|------|-----|
| Command | `IGameCommand` — `Execute()` のみ。`MoveCommand` 等は `IReadOnlyList<Unit>` 保持 |
| Queue | `CommandQueue` — Tick 先頭で実行 + `CommandLog.Record` |
| Team | `UnitTeam` — `Player`, `Enemy` の 2 値 |
| 資源 | `ResourceManager` — チーム別 Wood / Food / Gold / Stone（`UnitTeam` キー） |
| 参照 | Selection / Attack / Gather Command が **Unit / Building の直接参照** |

**Phase 57 後も Play は 1v1 のまま** — ID が付き、新 Command が ID 経由で動けば OK。

---

## ③ 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| Command | `IGameCommand.cs`, `CommandQueue.cs`, `MoveCommand.cs`, `CommandLog.cs` |
| Unit | `Unit.cs`, `UnitPool.cs`, `UnitManager.cs` |
| Building | `BuildingHealth.cs`, `TownCenter.cs`, `ProductionManager` |
| Team / 資源 | `UnitTeam.cs`, `ResourceManager.cs` |
| ロードマップ | [11_DEFERRED_EXTENSION_DESIGN.md](../11_DEFERRED_EXTENSION_DESIGN.md) § Entity ID |

---

## ④ 実装タスク

### 1. PlayerId（Core）

```csharp
// 例 — 配置は Core 推奨
public enum PlayerId { Player0 = 0, Player1 = 1, Player2 = 2, Player3 = 3 }

public static class PlayerIdMapping
{
    public static bool IsLocalHuman(PlayerId id); // MVP: Player0 のみ true
    public static UnitTeam ToLegacyTeam(PlayerId id); // 移行用
    public static PlayerId FromLegacyTeam(UnitTeam team); // 移行用
}
```

### 2. EntityRegistry

```csharp
public static class EntityRegistry
{
    public static int Register(Unit unit);
    public static int Register(BuildingHealth building);
    // Unregister on pool return / destroy
    public static bool TryGetUnit(int entityId, out Unit unit);
    public static bool TryGetBuilding(int entityId, out BuildingHealth building);
}
```

- ID は **単調増加 int**（再利用は Pool 返却時に Unregister → 再 Register で新 ID でも可 — 方針をコメントで明示）
- **GameObject 非依存**で Command / 将来 Replay が参照できる形

### 3. Unit / Building に EntityId 公開

- `Unit.EntityId` / `BuildingHealth.EntityId`（readonly）
- `UnitPool.Rent` / `PrepareForSpawn` で Register
- `UnitPool.Return` / 破壊で Unregister

### 4. Command 段階移行（small diff）

| 優先 | Command | 変更 |
|------|---------|------|
| 必須例 | 新規 `MoveUnitsByEntityIdCommand` or `MoveCommand` 内部で ID リスト保持 | Selection → Enqueue 時に Unit → EntityId 変換 |
| 任意 | `AttackUnitCommand` 1 種 | ターゲットを EntityId |

- 既存 `MoveCommand` を即削除しない — **並行 or 内部リファクタ**で最小 diff
- `CommandLog` に EntityId を記録できるよう拡張（リプレイ再生はオプション — 記録だけで可）

### 5. Editor / Phase10

- `Phase10SceneBuilder` 変更不要でも可（ランタイム Register で足りる）
- Play: 1v1 回帰 — 移動・攻撃・採集・生産・CPU・勝敗

### 6. 回帰

- Phase 56 Combat VFX / Phase 55 アニメ / HUD / ミニマップ
- Console エラーなし
- **1v1 CPU 戦**が Phase 56 以前と同様に動作

---

## ⑤ 制約

- small diff only / rewrite 禁止
- Simulation が `GameObject` を Command フィールドに **新規追加しない**（移行中の既存 Command は段階的に）
- NavMesh 禁止
- `GetInstanceID()` 禁止
- **全 Manager の UnitTeam → PlayerId 置換は Phase 57 では不要**（マッピング層で十分）

---

## ⑥ Play 確認

1. `Phase10.unity` — Play（Setup 変更があれば Edit モードで実行 → Ctrl+S）
2. Villager 移動・採集・建築 — 従来どおり
3. Militia 攻撃・CPU 攻撃波 — 従来どおり
4. 勝敗 → **R** 再開（ミニマップエラーなし）
5. Console に Entity 登録/解除の Debug（任意・1 行）— 重いログは避ける
6. 新 ID ベース Command（移行した 1 種）が動作

---

## ⑦ 完了時ドキュメント

- [x] `IMPLEMENTATION_STATUS.md` — Phase 57 ✅
- [x] `10_M6_MULTIPLAYER_FOUNDATION.md` — Phase 57 ✅
- [x] 本プロンプト — ✅

---

Phase 57 のみ。**Phase 58（CPU Command）・Phase 59（4人マッチ）には触れない。**

> **次:** Phase 58 CPU Command Queue — [phase58-prompt.md](phase58-prompt.md)

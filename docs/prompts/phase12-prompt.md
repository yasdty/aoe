# Phase 12 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜11 完了（PoC + Foundation Phase 11）  
> **ロードマップ:** [FOUNDATION_PHASES.md](../FOUNDATION_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/RTS_IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 12 実装（Object Pool）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜11 は完了済み。Phase 12 のみ実装すること。**

---

## ① Foundation 方針（必読・遵守）

[FOUNDATION_PHASES.md](../FOUNDATION_PHASES.md) の最重要方針を厳守:

- **AoE 機能を増やさない** — Archer / Food / Age Up 等は禁止
- **small diff** — 1 Phase = 1 目的（Object Pool のみ）
- **既存ゲームを壊さない** — 完了時 `Phase10.unity` でコアループ + **Victory / Defeat** が動くこと
- **Simulation 優先** — 見た目の polish より Spawn / Despawn 経路の置き換え

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止
- Manager 更新方式を維持
- **Unity アセット手書き禁止**: Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- Setup メニューは **Edit モード専用**
- **`GetInstanceID()` 禁止**

---

## ② Phase 11 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **Victory / Defeat** — Enemy / Player TC 破壊で終了 UI、`GameSessionManager.IsGameOver`
- **建物 HP** — TC 400 / House 150 / Barracks 200（テスト向け低 HP）
- **建物攻撃** — Militia が TC / House / Barracks を外縁スタンド位置から攻撃
- **Player House 建築** — builder は **Player チームのみ**（未選択時は CPU 村人を使わない）
- **ゴースト** — Prefab 実寸スケール（footprint 二重スケールなし）
- **Phase 10 コアループ** — 採集・建築・生産・CPU 経済 / 軍事 AI

### 現状のギャップ（Phase 12 で解消）

| 項目 | 現状 |
|------|------|
| ユニット Spawn | `UnitSpawner` → 毎回 `EntityVisualBuilder.CreateUnitShell` + `AddComponent<Unit>`（実質新規生成） |
| ユニット死亡 | `Unit.Die()` → **`Destroy(gameObject)`** |
| 建物 Spawn | `RuntimeBuildingFactory` → 毎回新規 Shell 生成 |
| 建物破壊 | `BuildingHealth.Die()` → **`Destroy(gameObject)`**（TC 含む） |
| Pool | **なし** |

主要ファイル（**実装前に必ず開いて読む**）:

- `Assets/Scripts/Units/UnitSpawner.cs`
- `Assets/Scripts/Units/Unit.cs`（`Die` / `SetData` / `OnEnable` / `OnDisable`）
- `Assets/Scripts/Units/UnitManager.cs`
- `Assets/Scripts/Buildings/ProductionManager.cs`（Spawn 呼び出し）
- `Assets/Scripts/Buildings/BarracksProductionManager.cs`
- `Assets/Scripts/Buildings/RuntimeBuildingFactory.cs`
- `Assets/Scripts/Buildings/BuildingHealth.cs`
- `Assets/Scripts/Visuals/EntityVisualBuilder.cs`
- `Assets/Scripts/Core/GameSessionManager.cs`
- `Assets/Scripts/Editor/Phase10SceneBuilder.cs`
- `Assets/Scenes/Phase10.unity`

---

## ③ Phase 12 目的

**大量ユニット生成に備える** — Villager / Militia の Spawn / Death サイクルで **`Instantiate` / `Destroy` を Pool 再利用に置き換える**。

### 今回実装するもの

1. **`UnitPool`**（または `UnitPoolManager`）— Villager / Militia 用
2. **`BuildingPool`** — House / Barracks 用（可能なら同 Phase で。工数が大きければ Unit のみ先行し README に明記）
3. **`UnitSpawner.Spawn`** — Pool から取得 → 状態リセット → 返却
4. **`Unit.Die`** — `Destroy` 禁止 → **`UnitPool.Return(unit)`**
5. **建物完成時** — `RuntimeBuildingFactory` を Pool 経由に（BuildingPool 実装時）
6. **Prewarm（任意）** — 初回 Play 時の GC スパイク低減（例: Villager 4 / Militia 4）
7. **Pool 統計（MVP）** — `SpawnCount` / `ReuseCount` を Console または Debug ログで確認可能に
8. **`Phase10SceneBuilder` 更新** — Pool Manager を Systems に追加
9. **README / `docs/FOUNDATION_PHASES.md` 更新**

### Unit Pool（MVP — 必須）

| 要件 | 内容 |
|------|------|
| 対象 | **Villager** / **Militia**（`UnitData` または `PlaceholderVisualKind` で分岐） |
| 取得 | Pool に空き → 再利用 / なければ新規作成（初回のみ `Instantiate` 相当） |
| 返却 | `SetActive(false)` + 親を Pool ルートへ / コンポーネント状態リセット |
| リセット項目 | HP、`isDead`、moveTarget、選択状態、Attack / Gather / Build ジョブ解除、`UnitManager` 登録 |

**`Unit` 再利用時の注意（必須）:**

```csharp
// Die() 内で Return する前に:
GatherManager.CancelForUnit(this);
BuildingPlacementManager.AbortConstructionForUnit(this);
AttackManager.CancelJobsForUnit(this);
SelectionManager.HandleUnitDied(this);
UnitManager.Unregister(this); // OnDisable でも呼ばれるが二重安全
```

返却後 `OnEnable` で再 Register される流れを維持。

### Building Pool（MVP — 推奨）

| 要件 | 内容 |
|------|------|
| 対象 | **House** / **Barracks** |
| 非対象 | **TownCenter** — 勝敗に直結。Phase 12 では **TC は Destroy のまま** で可 |
| 破壊 | House / Barracks の `BuildingHealth.Die()` → Pool Return（TC 以外） |
| リセット | `BuildingHealth` HP 復帰、Team、`House` / `Barracks` コンポーネント状態 |

### 禁止（Phase 12 範囲外）

- Benchmark シーン（Phase 13）
- Spatial Hash / Fixed Tick / Command Queue
- 新ユニット種別・新資源
- **TownCenter の Pool 化**（勝敗・シーン初期配置との整合が複雑 — 明示的に除外）
- Tree / ResourceNode の Pool
- 全プロジェクトの `Destroy` 一括置換（**Spawn / Death 経路のみ**）
- 新シーン `Phase12.unity`（**`Phase10.unity` 継続**）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 12-1 | `UnitPool` — Get / Return / Prewarm / 統計カウンタ |
| 12-2 | `Unit` — `ResetForSpawn()` / `Die()` → Return |
| 12-3 | `UnitSpawner.Spawn` — Pool 経由に差し替え |
| 12-4 | 動作確認 — 生産・死亡・再 생産で Reuse 増加 |
| 12-5 | `BuildingPool` — House / Barracks（任意だが推奨） |
| 12-6 | `RuntimeBuildingFactory` + `BuildingHealth.Die`（TC 除外） |
| 12-7 | `Phase10SceneBuilder` + README / FOUNDATION_PHASES 更新 |

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**（新規 / 変更 / 削除）
2. **新規追加ファイル一覧**
3. **UnitPool と BuildingPool の責務分担**
4. **Unit 状態リセット一覧**（漏れがあると再利用時バグ）
5. **影響範囲**（Production / BarracksProduction / Attack / Gather / Selection）
6. **リスク**（二重 Register、Dead Unit 残留、Victory 後 Return）
7. **ロールバック方法**
8. **完了条件**（下記チェックリスト）
9. **テスト手順**

---

## ⑥ 実装後に必ず出力

1. **変更内容サマリ**
2. **変更ファイル一覧**
3. **Pool 統計**（Spawn vs Reuse の例）
4. **テスト結果**
5. **既知の制限**（TC Destroy 残存、Tree 非 Pool 等）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### UnitPool 例

```csharp
public class UnitPool : MonoBehaviour
{
    static UnitPool instance;
    readonly Dictionary<PlaceholderVisualKind, Stack<Unit>> pools = new();

    public static Unit Rent(UnitData data, Vector3 position, UnitTeam team) { /* ... */ }
    public static void Return(Unit unit) { /* ... */ }
}
```

- Shell は `EntityVisualBuilder.CreateUnitShell` で **初回のみ** 作成し、以降 `Unit` コンポーネント付き GO を再利用
- Pool ルートは `DontDestroyOnLoad` **不要**（シーン内 `Systems/UnitPool` で十分）
- **`GameSessionManager.IsGameOver` 後も Return は安全** — 新規 Spawn は既に停止済み

### Spawn 経路（現状 → 変更後）

```
ProductionManager / BarracksProductionManager
  → UnitSpawner.Spawn
    → [Phase 12] UnitPool.Rent
    → [旧] CreateUnitShell + AddComponent
```

### 完了判定用ログ例

```
UnitPool: spawn=2 reuse=14 (Villager)
UnitPool: spawn=1 reuse=8 (Militia)
```

Play 中に Villager を何度か生産・死亡させ、**reuse が増え spawn が頭打ち** になれば OK。

### Phase10SceneBuilder

```csharp
GameObject poolObject = new GameObject("UnitPool");
poolObject.transform.SetParent(systems.transform);
poolObject.AddComponent<UnitPool>();
// BuildingPool 同様
```

### Victory / Defeat 後の R キー再起動

`VictoryDefeatHudView` が `SceneManager.LoadScene` — Pool はシーン再読込で自然リセット。`static` カウンタは `Awake` で 0 リセット。

---

## ⑧ シーン / Editor

- 検証シーン: **`Assets/Scenes/Phase10.unity`**（継続使用）
- `AoE → Setup Phase10 Scene` — Pool Manager 配線を追加
- Phase 1〜10.5 シーン・メニューは壊さない

---

## ⑨ 完了条件（Phase 12 MVP）

- [ ] Villager / Militia 生産が **UnitPool 経由**
- [ ] ユニット死亡が **`Destroy` ではなく Return**
- [ ] 2 回目以降の生産で **Reuse カウントが増える**（Console ログで確認）
- [ ] **Victory / Defeat** が Phase 11 同様に動作
- [ ] **Phase 10 コアループ** — 採集・建築・生産・戦闘・CPU が壊れていない
- [ ] Console に **Null 参照・二重 Register エラーなし**
- [ ] （推奨）House / Barracks が BuildingPool 経由
- [ ] `docs/FOUNDATION_PHASES.md` Phase 12 を ✅ に更新

---

## ⑩ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene` → Play
2. TC から Villager を **3 体以上** 生産
3. CPU 攻撃波で Villager / Militia を **死亡** させる
4. 再度 Villager / Militia を **生産**
5. Console で **UnitPool reuse 増加** を確認（spawn 乱立しない）
6. 敵 TC 破壊 → **VICTORY** → R で再起動 → 再 Play 可能
7. 自 TC 破壊 → **DEFEAT** も確認（任意）
8. Console エラーなし

Phase 12 のみ実装。Phase 13 以降（Benchmark 等）に触れない。

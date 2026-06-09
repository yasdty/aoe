# AoE RTS Engine — 意図的スコープ外機能の拡張設計

> **用途:** AoE2 フル機能のうち **意図的に後回し（カテゴリ C）** とした項目を、将来 **rewrite なしで挿入**できるよう設計指針を定める。  
> **関連:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) §8 Missing Features / [CONSTITUTION.md](../CONSTITUTION.md)

---

## カテゴリ C（意図的スコープ外）

| 機能 | 導入タイミング候補 | 拡張フック |
|------|-------------------|------------|
| **マルチプレイ（LAN/オンライン）** | M6 Phase 54〜58 | Entity ID, Command Queue, Deterministic Sim |
| **リプレイ / セーブ** | M6 Phase 57 | Command Log + Snapshot |
| **船・漁業・Trade Cart** | M8 以降 | `BuildingData` + `UnitData` + `GatherManager` 分岐 |
| **修道院 / Monk / 変換** | M8 以降 | `AbilityData` on UnitData |
| **攻城兵器** | M7 以降 | `AttackManager` siege 分岐 + `BuildingData` Siege Workshop |
| **Castle / Wonder** | M4 後拡張 | `AgeData` + `BuildingData` 追加のみ |
| **複数勝利条件** | M8 以降 | `VictoryConditionData` ScriptableObject |
| **ランダムマップ** | M8 / P3 | `MapGenerator` — 地形は現 Plane の上に Heightmap |
| **Fog of War** | M7 以降 | `VisionManager` + `TeamVisionMask` |
| **40+ 文明 / Extreme AI** | 長期 | `CivilizationData` + `AiProfileData` |
| **パスファインディング** | M7 | `IPathfinder` — NavMesh 禁止の代替 |
| **GPU Instancing 本格** | M5〜M7 | `UnitRenderBatch` — View 層 |

---

## 共通拡張パターン（全 Phase で維持）

### 1. Data 駆動（ScriptableObject）

```
UnitData        → 新兵種は asset 追加のみ
BuildingData    → 新建築は asset + ProductionRecipe
TechnologyData  → 新研究は asset 追加
AgeData         → 新時代は asset 追加
CivilizationData→ 新文明は asset 追加
```

**禁止:** 兵種ごとに `if (unitType == Archer)` が Manager に散在 → **Strategy / Component** へ。

### 2. Command パターン

全プレイヤー操作（将来 CPU 含む）は `IGameCommand`：

```csharp
// 目標形（M6 までに段階移行）
public interface IGameCommand
{
    int Tick { get; }
    int PlayerId { get; }
    void Execute(SimulationContext ctx);
}
```

新能力（Garrison, Patrol, Monk Convert）は **新 Command クラス追加**で対応。

### 3. Entity ID（M6 Phase 54）

Command / AI / Replay は **GameObject 参照禁止**。`EntityRegistry.Get<T>(id)` で解決。

### 4. 時代・建築アンロック（M4 Phase 42）

```csharp
// BuildingData
public AgeId requiredAge;
public TechnologyId[] requiredTechs;
```

Castle / Dock / Monastery は `BuildingData` を増やすだけ。

### 5. 能力システム（将来）

```csharp
// UnitData — Monk 変換等
public AbilityData[] abilities;
```

`AttackManager` を肥大化させず `IUnitAbility` で拡張。

### 6. 勝利条件（将来）

```csharp
public interface IVictoryCondition
{
    bool IsMet(SimulationContext ctx, PlayerId player);
}
```

TC 破壊は既存実装を 1 条件としてラップ可能。

---

## 新機能追加チェックリスト

新しい AoE2 機能を Phase に入れる前に確認:

1. [ ] ScriptableObject で定義できるか
2. [ ] `IGameCommand` で表現できるか
3. [ ] Entity ID で参照できるか（M6 以降必須）
4. [ ] 既存 Manager に `if civilization == X` を足していないか
5. [ ] View（OnGUI/uGUI）と Simulation が分離されているか（M5 以降）

---

## M0〜M5 各マイルストンでの「挿入余地」

| Milestone | 挿入しやすい拡張 |
|-----------|------------------|
| M3 | 新兵種・新軍事建築（Archery Range パターン） |
| M4 | 新时代・新研究・新文明・防御建築 |
| M5 | 新 UI パネル・新アニメセット |
| M6 | 新 Command 種・PlayerId |
| M7+ | Pathfinding / Map / Fog / Navy |

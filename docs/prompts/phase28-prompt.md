# Phase 28 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜27 完了（PoC + Foundation + M2 Economy + M2.5 Phase 21〜27）  
> **マイルストン:** M2.5 Economy Polish  
> **ロードマップ:** [04_M2_5_ECONOMY_POLISH_PHASES.md](../04_M2_5_ECONOMY_POLISH_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 28 実装（Sheep Herding + Animal Locomotion）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜27 は完了済み。Phase 28 のみ実装すること。**

---

## ① M2.5 Economy Polish 方針（必読・遵守）

Phase 27 で Mill（Food Drop-off）が完成。Phase 28 は **羊誘導 + 動物徘徊** — Phase 24 で省略した動物 UX。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | Neutral Sheep 発見・誘導 + Deer 徘徊 |
| **small diff** | `SheepResource` / `DeerResource` **拡張** — 統合リファクタ禁止 |
| **既存パターン再利用** | `UnitManager` 移動 Tick / `SelectionManager` / `HuntFoodCommand` / `FoodGatherManager` |
| **既存ゲームを壊すな** | Boar / Mill / Berry / Farm / 4 資源 / Info Panel / CPU |
| **Foundation 維持** | Command Queue / Fixed Tick 20 TPS / Pool / Spatial Hash |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- 動物移動は **`ISimulationTickable` + transform.position**（個別 `Update` 禁止）
- **`.meta` は 32 文字 GUID**

---

## ② Phase 27 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **Food Drop-off** — TC + **Mill**（`MillRegistry`）
- **狩り** — Deer / Sheep / Boar（死体）→ TC / Mill 搬入 → リピート
- **Sheep** — **無所属なし**。誰でも即 `HuntFoodCommand` 可能。静止
- **Deer** — 静止。Food 直減り
- **Boar** — HP/Food 分離 + 反撃（Phase 26）

### 現状のギャップ（Phase 28 で解消）

| 項目 | 現状 |
|------|------|
| Sheep 所属 | なし — 初期から狩れる |
| 羊誘導 | なし — 右クリック移動不可 |
| 発見 | なし |
| Deer 徘徊 | なし — 完全静止 |
| Info Panel | Sheep に Team / 所属表示なし |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| 被動動物 | `Assets/Scripts/Economy/SheepResource.cs` / `DeerResource.cs` |
| 狩り | `Assets/Scripts/Economy/FoodGatherManager.cs` — `huntJobs` |
| 反撃動物参考 | `Assets/Scripts/Economy/BoarAggroManager.cs` — 移動 Tick |
| ユニット移動 | `Assets/Scripts/Units/UnitManager.cs` / `Unit.TickMovement` |
| 選択 | `Assets/Scripts/Selection/SelectionManager.cs` |
| 命令 | `Assets/Scripts/Commands/GameCommands.cs` — `MoveCommand` / `HuntFoodCommand` |
| Info Panel | `Assets/Scripts/Selection/SelectionInfoPanelView.cs` |
| Editor | `Assets/Scripts/Editor/Phase10SceneBuilder.cs` — Sheep 配置 |

---

## ③ Phase 28 目的

**AoE2 Dark Age の羊スカウト経済** + 動物の見た目 polish。

### AoE2 参考（MVP スコープ）

| 動物 | MVP 挙動 |
|------|----------|
| **Sheep** | 初期 **Neutral（無所属）** → 自軍ユニット **接触/近接で発見** → **Player 所属** → 左クリ選択 + 右クリ移動（TC へ誘導）→ 狩り可 |
| **Sheep 追従** | 所属後、近くの **自軍村民** が通ると **ゆっくり追従**（optional 簡易版可） |
| **Deer** | **ランダム短距離 wander**（停止 ↔ 移動）。被狩り時の **逃走は optional / 後回し可** |
| **Boar** | **変更最小** — 既存反撃維持。徘徊追加は **不要** |

### Neutral Sheep — 狩りルール（MVP 推奨）

```
Neutral Sheep → HuntFoodCommand 不可（または右クリ無視）
自軍ユニット接触（半径 3m）→ Discover(team) → 所属 Sheep
所属 Sheep → HuntFoodCommand 可（既存フロー）
```

**発見トリガー:** 村民/Militia が Sheep に **一定距離以内** で Tick 検出（`AnimalDiscoveryManager` 推奨）。

### 羊誘導フロー

```
所属 Sheep を左クリック選択
    ↓
右クリック（Ground）→ SheepMoveCommand（新規）または MoveCommand 拡張
    ↓
Sheep が目的地へ低速移動（Herding）
    ↓
TC 近傍に置いて Hunt / 自動追従
```

**MVP:** Sheep は **Unit ではなく Resource** のまま。`SheepResource` に `moveTarget` + Tick 移動。

---

## ④ 今回実装するもの

### 1. `AnimalOwnership` / Team 拡張

- `UnitTeam` に **`Neutral`** を追加 **または** `SheepResource` 専用 `AnimalTeam { Neutral, Player, Enemy }`
- **small diff 推奨:** `SheepResource` 内 `bool isDiscovered` + `UnitTeam? ownerTeam`（null = Neutral）

### 2. `SheepResource` 拡張

- `OwnerTeam` / `IsNeutral` / `IsDiscovered`
- `Discover(UnitTeam team)` — 発見時に色変更（自軍色ティント）
- `SetMoveTarget(Vector3)` / `ClearMoveTarget()` / `TickMovement(deltaTime)`
- 移動速度: 2〜2.5（村民より遅い）
- `IsHuntable` — Neutral なら `false`（`IHuntableFoodResource` は維持、Manager 側でガード可）

### 3. `AnimalDiscoveryManager.cs`（新規）

- `ISimulationTickable` — 20 TPS
- 全 `SheepResource` を走査（Registry 推奨: `SheepRegistry` または static list）
- 自軍 `Unit` が `discoverRadius` 内 → `sheep.Discover(unit.Team)`
- CPU ユニットも発見可能（Phase 30 準備）

### 4. `SheepMoveCommand` + Selection 拡張

- **羊のみ**選択時（1 体）→ Ground 右クリック → `SheepMoveCommand`
- 混合選択（村民+羊）は **既存 MoveCommand のみ**（羊は動かさない — MVP OK）
- `SelectionManager` — Neutral Sheep **左クリック選択可**（Info: Neutral / Food）
- 所属 Sheep 左クリック → Info: `Sheep` + Food + `Owner: Player`

### 5. `PassiveAnimalLocomotionManager.cs`（新規）

- Deer（+ optional 未移動中の Sheep）の **wander**
- ジョブ: `{ DeerResource deer, float pauseTimer, Vector3 wanderTarget }`
- 状態: Idle → PickRandomOffset(2〜4m) → Move → Pause(2〜4s) → ループ
- **狩り中（FoodGatherManager が Hunter 設定中）は wander 停止** — Deer は `TakeFood` 中停止で OK

### 6. `DeerResource` — wander フック

- `TickWander(deltaTime)` を `PassiveAnimalLocomotionManager` から呼ぶ
- または Deer に `bool pauseWander` フラグ（狩り中は Manager が pause）

### 7. 狩り・命令ガード

- `SelectionManager` — Neutral Sheep 右クリック → **Hunt 発行しない**
- `FoodGatherManager.IssueHuntCommand` — Neutral Sheep スキップ
- 所属 Sheep / Deer — **既存通り**

### 8. Editor / シーン

- Phase10 の Sheep を **Neutral 初期化**（`CreateSheep` 後に `isDiscovered=false`）
- **`AoE → Reset Sheep to Neutral (Phase10)`** optional メニュー
- Deer 配置は現状維持（wander は Play 時自動）

---

## ⑤ 今回やらないこと

- Boar 逃亡 / 徘徊変更
- Deer / Sheep **HP モデル統一**（optional — 現状 Food 直減り維持）
- CPU 羊誘導 AI（**Phase 30**）
- Militia Aggro（**Phase 29**）
- Sheep を `Unit` コンポーネント化
- 本格 Pathfinding / RVO
- 複数 Sheep 隊列フォーメーション

---

## ⑥ 実装ステップ（推奨）

| Step | 内容 |
|------|------|
| 28-1 | `SheepResource` 拡張 — 所属 / 移動 / Discover |
| 28-2 | `AnimalDiscoveryManager` + Sheep Registry |
| 28-3 | `SheepMoveCommand` + SelectionManager 羊選択/移動 |
| 28-4 | `PassiveAnimalLocomotionManager` — Deer wander |
| 28-5 | Hunt ガード + Info Panel + Phase10 + ドキュメント |

---

## ⑦ 技術メモ

### SheepRegistry（案）

```csharp
// SheepResource OnEnable/OnDisable で Register
// AnimalDiscoveryManager / PassiveAnimalLocomotionManager が参照
```

### Discover

```csharp
public void Discover(UnitTeam team)
{
    if (isDiscovered) return;
    isDiscovered = true;
    ownerTeam = team;
    UpdateVisual();
}
```

### SelectionManager — Sheep 右クリック

```csharp
SheepResource sheep = hit.collider.GetComponentInParent<SheepResource>();
if (sheep != null && !sheep.IsDepleted)
{
    if (sheep.IsNeutral) return; // or no-op after discover attempt
    if (OnlySheepSelected())
        CommandQueue.Enqueue(new SheepMoveCommand(selectedSheep, hit.point));
    else
        CommandQueue.Enqueue(new HuntFoodCommand(selectedUnits, sheep));
    return;
}
```

### Deer wander（案）

```csharp
// Pick random point within 3m of spawn anchor or current pos
// MoveSpeed 1.5, pause 3s at destination
```

### Cancel 連携

- 村民 Move → 既存 `FoodGatherManager.CancelForUnits` — Deer/Sheep 狩り解除
- Sheep Move → 新 `SheepMoveCommand` は他ジョブ Cancel 不要（Resource のみ）

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`
- **Setup:** `AoE → Setup Phase10 Scene` — Sheep Neutral 初期化込み
- 既存シーン: Setup 再実行推奨

---

## ⑨ 完了条件（Phase 28 MVP）

- [ ] **Neutral Sheep** — 発見前は狩れない
- [ ] **発見** — 村民が近づく → Player 所属 → 狩り可
- [ ] **誘導** — 所属 Sheep 選択 → 右クリック → TC 方向へ移動
- [ ] **追従（optional）** — 村民近傍で Sheep がゆっくり追従
- [ ] **Deer 徘徊** — ランダム短距離 wander（停止含む）
- [ ] **狩りリピート** — 所属 Sheep / Deer → Mill/TC 搬入（Phase 21 回帰）
- [ ] **Info Panel** — Neutral: Food only / 所属: Food + Owner
- [ ] Boar / Mill / Berry / 4 資源 回帰
- [ ] Console エラーなし
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `04_M2_5` Phase 28 を ✅

---

## ⑩ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene` → Play
2. **Neutral Sheep** — 村民 → Sheep 右クリック → **狩り開始しない**
3. 村民を Sheep に近づける → **発見**（色変化）→ 右クリックで **狩り開始**
4. **誘導** — 所属 Sheep 左クリック → 地面右クリック → TC 方向へ移動
5. **Deer** — Play 中に短距離ランダム移動を確認
6. **Boar / Berry / Mill** 回帰
7. Console エラーなし

Phase 28 のみ実装。**Phase 29 以降（Militia Aggro 等）** に触れない。

# Phase 26 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜25 完了（PoC + Foundation + M2 Economy + M2.5 Phase 21〜25）  
> **マイルストン:** M2.5 Economy Polish  
> **ロードマップ:** [04_M2_5_ECONOMY_POLISH_PHASES.md](../04_M2_5_ECONOMY_POLISH_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 26 実装（Boar — 反撃狩り）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜25 は完了済み。Phase 26 のみ実装すること。**

---

## ① M2.5 Economy Polish 方針（必読・遵守）

Phase 25 で Selection Info Panel が完成。Phase 26 は **Boar（反撃する Food 源）** のみ。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | Boar 狩り + 反撃 + Militia 攻撃 |
| **small diff** | **Deer / Sheep 狩りパターンを拡張** — rewrite / 統合リファクタ禁止 |
| **既存パターン再利用** | `DeerResource` / `FoodGatherManager.huntJobs` / `HuntFoodCommand` / `AttackManager` |
| **既存ゲームを壊さない** | Deer/Sheep / Info Panel / 4 資源 / Mining Camp / CPU + Foundation 全機能 |
| **Foundation 維持** | Command Queue / Fixed Tick 20 TPS / Pool / Spatial Hash |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- Setup メニューは **Edit モード専用**（本 Phase で **Phase10SceneBuilder に Boar 配置を追加**）
- **`.meta` は 32 文字 GUID**（`MonoImporter` 任意。`LumberCamp.cs.meta` 形式で OK）

---

## ② Phase 25 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **狩り（被動）** — Deer / Sheep → `HuntFoodCommand` → TC 搬入 → リピート
- **Selection Info Panel** — 左下 HP / Attack / 資源残量（`SelectionInfoPanelView`）
- **村民 HP** — 1 体選択時は左下 Info Panel（中央 HP バーは複数選択時のみ）
- **Combat** — Militia 右クリック → `AttackUnitCommand` / `AttackBuildingCommand`（`AttackManager`）
- **Boar** — **未実装**

### 現状のギャップ（Phase 26 で解消）

| 項目 | 現状 |
|------|------|
| Boar | **未実装** |
| 反撃 AI | なし（Deer/Sheep は被動） |
| Militia vs 動物 | Militia は Unit/Building のみ攻撃 — Boar 不可 |
| Info Panel | Boar 用 HP / Attack 表示なし |

**実装前に必ず開いて読むファイル（Deer 狩りテンプレート）:**

| 領域 | ファイル |
|------|----------|
| 被動狩り | `Assets/Scripts/Economy/DeerResource.cs` / `IHuntableFoodResource.cs` |
| 狩りジョブ | `Assets/Scripts/Economy/FoodGatherManager.cs` — `huntJobs` / `TickHuntJobs` |
| 命令 | `Assets/Scripts/Commands/GameCommands.cs` — `HuntFoodCommand` |
| 右クリック | `Assets/Scripts/Selection/SelectionManager.cs` — ResourceMask |
| 戦闘 | `Assets/Scripts/Combat/AttackManager.cs` |
| Info Panel | `Assets/Scripts/Selection/SelectionInfoPanelView.cs` |
| Editor | `Assets/Scripts/Editor/Phase1SceneBuilder.cs` — `CreateDeer` |
| シーン | `Assets/Scripts/Editor/Phase10SceneBuilder.cs` — Deer 配置 |

---

## ③ Phase 26 目的

**Boar — 殴られると反撃する Food 源。** AoE2 Dark Age の「狩り + 軽い戦闘」橋渡し。

### AoE2 参考（MVP スコープ）

- Boar は **HP あり**（生きている間 Info Panel: HP + Attack）
- **殴られると反撃** — 近接で Hunter / Militia にダメージ
- **Militia** は Boar を攻撃 → **HP 減少** → 死亡
- 村民は狩れるが **被ダメージリスク** あり（狩り = HP ダメージ → 死体から Food 搬入）
- Boar 死亡後 Food 0 で採取不可・灰色化

### 変更後のフロー（村民）

```
村民 → Boar 右クリック
    ↓
MoveToBoar → Hunt（ApplyHuntDamage）— 初回 Hunt から Boar 反撃開始
    ↓
Boar 死亡 → 死体から TakeFood → MoveToDeposit → TC 搬入 → リピート
```

### 変更後のフロー（Militia）

```
Militia → Boar 右クリック
    ↓
AttackManager 相当で Boar の **HP** を削る
    ↓
Boar 反撃 → Militia が Boar を仕留め → 死体に Food → 村民が搬入可
```

**MVP 方針:** Boar は **HP（戦闘）と Food（死体）を分離**。Militia / 村民の狩りは **HP を削る** → 死亡後に `remainingFood = initialFood` の死体から `TakeFood`。Deer / Sheep は Phase 24 通り Food 直減り（Phase 28 で統一検討）。

---

## ④ 今回実装するもの

### 1. `BoarResource.cs`

- `IHuntableFoodResource` 実装（Deer コピー拡張）
- 追加: **反撃用 stat** — `attack` / `attackRange` / `attackCooldown`（SerializeField or FoodNodeData 拡張）
- `ApplyHuntDamage` — 村民狩り（HP 減少 + 反撃トリガー）
- `ApplyAttackDamage` — Militia 攻撃（HP 減少 + 反撃トリガー）
- 死亡時 `remainingFood = initialFood`、Aggro 解除
- 枯渇時 `UpdateVisual` 灰色化

### 2. `DefaultBoar.asset` — `FoodNodeData`

| 項目 | 値（案 — AoE2 参考で調整可） |
|------|------------------------------|
| displayName | Boar |
| initialFood | 340 |
| boarAttack（拡張） | 7（`FoodNodeData` に optional フィールド追加 or BoarResource SerializeField） |
| defaultColor | 暗めの灰色 `(0.35, 0.35, 0.38)` |

**small diff 推奨:** 攻撃力は `BoarResource` の SerializeField で OK（`FoodNodeData` 変更は optional）。

### 3. `BoarAggroManager.cs`（新規）

- `ISimulationTickable` — Fixed Tick 20 TPS
- Aggro ジョブ: `{ BoarResource boar, Unit target }`
- Tick: Boar が target に接近 → クールダウン後 `Unit.TakeDamage`
- target 死亡 / Boar 枯渇 / 距離外で長時間 → ジョブ解除
- **移動:** Boar 本体を `transform.position` で低速直線移動（`UnitManager` パターン。個別 `Update` 禁止）
- 速度案: 3〜4（村民よりやや速い）

### 4. 狩り・命令・選択の拡張

- **`HuntFoodCommand`** — Boar コンストラクタ追加（Deer/Sheep 同型）
- **`FoodGatherManager.IssueHuntCommand`** — Boar 対応（既存 `IHuntableFoodResource` ならそのまま可）
- **`SelectionManager.HandleMoveCommand`** — Deer/Sheep の後に Boar 判定
  - **村民**（`!CanAttack`）→ `HuntFoodCommand`
  - **Militia**（`CanAttack`）→ **Boar 攻撃命令**（新 `AttackBoarCommand` 推奨）
- **`SelectionInfoPanelView`** — Boar 左クリック → `Food: N` + **Attack: N**（Boar の反撃力）

### 5. Militia 攻撃 Boar

**推奨（small diff）:**

- `AttackBoarCommand` → 新 **`BoarAttackManager`** または **`AttackManager` 拡張**
- MVP: **`BoarAttackManager`** — Militia → Boar 接近 → クールダウン → `BoarResource.ApplyAttackDamage(attacker.AttackPower)`
- `AttackManager` の Unit/Building ジョブと **独立リスト** で OK（Phase 29 Aggro 前の簡易版）

**禁止:** `AttackManager` 全 rewrite。

### 6. Editor / シーン

- `Phase1SceneBuilder.CreateBoar` — Deer コピー（色・サイズ調整: やや大きめ Capsule）
- `Phase10SceneBuilder` — Player 近傍に Boar 1〜2 配置
- `GameAssetPaths.DefaultBoarData`
- **`.meta` GUID は 32 文字**

---

## ⑤ 今回やらないこと

- Mill（**Phase 27**）
- 羊誘導 / 動物徘徊（**Phase 28**）
- Boar 逃亡 AI / 複数 Boar 連携
- Militia 自動 Aggro（**Phase 29**）
- CPU Boar 狩り（**Phase 30**）
- Boar を `Unit` コンポーネント化（MVP 不要 — Resource + Aggro で十分）
- Melee/Pierce 装甲 2 種

---

## ⑥ 実装ステップ（推奨）

| Step | 内容 |
|------|------|
| 26-1 | `BoarResource` + Data + Editor Create + `.meta` |
| 26-2 | `BoarAggroManager` — 反撃 Tick |
| 26-3 | `HuntFoodCommand` / SelectionManager 村民狩り |
| 26-4 | `BoarAttackManager` + `AttackBoarCommand` + Militia 右クリック |
| 26-5 | `SelectionInfoPanelView` Boar 表示 + Phase10 配置 + ドキュメント |

---

## ⑦ 技術メモ

### SelectionManager — Boar 右クリック優先（案）

```csharp
// ResourceMask 内 — Deer/Sheep の前 or 後を統一
BoarResource boar = hit.collider.GetComponentInParent<BoarResource>();
if (boar != null && !boar.IsDepleted)
{
    if (TryIssueAttackBoarCommand(boar)) return;  // Militia
    CommandQueue.Enqueue(new HuntFoodCommand(selectedUnits, boar));  // Villager
    return;
}
```

### 反撃トリガー

```csharp
// BoarResource.TakeFood 内
public float TakeFood(float amount)
{
    // ... existing ...
    if (taken > 0f && currentHunter != null)
        BoarAggroManager.NotifyHunted(this, currentHunter);
    return taken;
}
```

`currentHunter` は `FoodGatherManager` の Hunt 状態から Set/Clear（`IssueHuntCommand` / job 終了時）。

### Militia vs Boar ダメージ

```csharp
// BoarResource — Militia の攻撃も Food プールを減らす
public void ApplyAttackDamage(float damage, Unit attacker)
{
    if (IsDepleted || damage <= 0f) return;
    remainingFood = Mathf.Max(0f, remainingFood - damage);
    BoarAggroManager.NotifyAttacked(this, attacker);
    UpdateVisual();
}
```

### Cancel 連携

- 村民 Move / 新 Hunt / Attack 系命令 → 既存 `FoodGatherManager.CancelForUnits` + Boar Aggro 対象更新
- Militia Boar 攻撃中に Move → `BoarAttackManager.CancelForUnits`

### Info Panel

```csharp
case BoarResource boar:
    title = "Boar";
    lines.Add($"Food: {Mathf.FloorToInt(boar.RemainingFood)}");
    lines.Add($"Attack: {boar.AttackPower:0}");  // 反撃力
    break;
```

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`
- **Setup:** `AoE → Setup Phase10 Scene` 再実行 **または** `CreateBoar` を既存シーン追加メニュー
- Deer/Sheep と同様 **`AoE → Add Huntable Animals`** は Boar 非含 — 本 Phase で Boar 専用配置を追加

---

## ⑨ 完了条件（Phase 26 MVP）

- [ ] **Boar** — 村民右クリック → 狩り → TC 搬入 → Food 増加
- [ ] **反撃** — 狩り中 Boar が村民にダメージ（HP 減少）
- [ ] **Militia** — Boar **右クリック** → **HP 減少** → 死亡
- [ ] **枯渇** — Food 0 で灰色化 / 採取・攻撃不可
- [ ] **採取リピート** — 搬入後 Boar へ復帰（Phase 21 回帰）
- [ ] **Info Panel** — 生きている Boar 左クリック → HP + Attack / 死体 → Food
- [ ] Deer / Sheep / Berry / Farm / 4 資源 / Info Panel 回帰
- [ ] Console エラーなし
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `04_M2_5` Phase 26 を ✅

---

## ⑩ テスト手順（Play チェックリスト）

1. `AoE → Setup Phase10 Scene`（または Boar 追加）→ Play
2. **村民 → Boar 右クリック** — 狩り開始 → Boar HP 減少・反撃で **村民 HP 減少** → 死亡後 Food 搬入
3. **Militia → Boar 右クリック** — 攻撃 → **Boar HP 減少**（左クリックで Panel 確認）
4. **Info Panel** — 生きている Boar 左クリック → HP / Attack、死体 → Food
5. **リピート** — 1 村民で複数往復
6. **枯渇** — Food 0 まで → 灰色化
7. Deer / Sheep / Berry / Selection Info Panel 回帰
8. Console エラーなし

Phase 26 のみ実装。**Phase 27 以降（Mill 等）** に触れない。

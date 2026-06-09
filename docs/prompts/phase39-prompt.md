# Phase 39 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜38 完了（M3 Stable + Cavalry + Scout 含む）  
> **マイルストン:** M3 Military — **Counter System（Melee / Pierce 装甲・ボーナスダメージ）**  
> **ロードマップ:** [07_M3_MILITARY_PHASES.md](../07_M3_MILITARY_PHASES.md)  
> **Balance 方針:** [12_GAMEPLAY_BALANCE_MODE.md](../12_GAMEPLAY_BALANCE_MODE.md) — 本 Phase では **GameplayBalance 層は触らない**（Data に MVP 値を直接書く）  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 39 実装（Counter System）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 39 のみ実装。** 既存 `AttackManager` のダメージ 1 行を **Resolver 経由**に差し替え、Data / Info Panel を拡張する（rewrite 禁止）。

**ユーザー確定（2026-06）:**

- **装甲:** 単一 `armor` を廃止し **Melee / Pierce 2 種**へ移行（Phase 37 で保留していた項目）
- **相性 MVP:** Spearman 対 Cavalry 系 + Archer の Pierce 弱点利用が **Play で体感できる**最小表
- **新 Input / 新建築 / CPU ロジック変更:** 本 Phase では **不要**

---

## ① 目的

AoE2 準拠の **rock-paper-scissors 下地**を入れる。Phase 37/38 で先送りした Spearman 対騎兵ボーナスを有効化し、Info Panel で装甲種別を表示する。

| 項目 | MVP |
|------|-----|
| 攻撃種別 | **Melee**（Militia / Spearman / Cavalry / Scout）/ **Pierce**（Archer） |
| 防御 | `meleeArmor` + `pierceArmor`（ユニット・建築） |
| 相性 | 静的ボーナス表 — **Spearman → Cavalry 系** が主役 |
| ダメージ式 | `max(1, attack - 対応装甲) + bonus` |
| UI | `SelectionInfoPanelView` — Melee/Pierce 装甲・攻撃種別表示 |
| 適用経路 | `AttackManager`（必須）/ `BoarAggroManager`（Melee vs meleeArmor で統一） |

**やらないこと:** Stance / Attack-Move（Phase 40）/ Formation（Phase 41）/ 弾丸飛翔 / 全 AoE2 装甲クラス網羅 / Balance Mode 本体 / CPU 相性 AI / 建築 armor class ボーナス

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| 現状ダメージ | `AttackManager.cs` — `ProcessAttackCycle` 内 `Mathf.Max(1f, attack - targetArmor)` |
| Boar 戦闘 | `BoarAggroManager.cs` — 同上パターン |
| Data | `UnitData.cs`, `Phase1SceneBuilder.Sync*Stats`（Militia〜Scout） |
| 建築 HP | `BuildingHealth.cs`, `PlacedBuildingData.armor` |
| Info Panel | `SelectionInfoPanelView.cs` — Phase 25 単一 Armor 表示 |
| 先送り明記 | `phase37-prompt.md` / `phase38-prompt.md` — 「対騎兵ボーナスは Phase 39」 |

---

## ③ 実装タスク（small diff）

### 1. Combat 型定義（新規）

`Assets/Scripts/Combat/` に追加（名称はこれで可、既存命名に合わせて調整可）:

```csharp
public enum AttackDamageType { Melee, Pierce }

public enum UnitArmorClass
{
    None = 0,
    Infantry,   // Militia, Spearman
    Cavalry,    // Cavalry, Scout
    // 将来: Archer, Siege, Building …
}
```

- `CombatDamageResolver`（static）— **唯一のダメージ計算入口**
  - `float Resolve(Unit attacker, Unit target)`
  - `float Resolve(Unit attacker, BuildingHealth target)` — 建築は `UnitArmorClass.None`、ボーナス 0
  - 内部: 攻撃種別に応じて `meleeArmor` / `pierceArmor` を選択
  - ボーナス表（MVP ハードコードで可）:

| Attacker `displayName` | Target `UnitArmorClass` | Bonus |
|------------------------|-------------------------|-------|
| Spearman | Cavalry | **+12** |

  - 将来 M4 で ScriptableObject 化しやすいよう、表は **1 メソッド / 1 配列**に集約

### 2. UnitData 拡張

`UnitData` に追加（`armor` は **削除または Obsolete 化** — Sync 関数から完全移行）:

```csharp
public AttackDamageType attackDamageType = AttackDamageType.Melee;
public float meleeArmor;
public float pierceArmor;
public UnitArmorClass armorClass = UnitArmorClass.None;
```

`Unit.cs` — プロパティ追加:

- `MeleeArmor` / `PierceArmor` / `AttackDamageType` / `ArmorClass`

### 3. Ensure* Sync 値（MVP 暫定 — Play で相性が分かる値）

| Unit | attack | attackDamageType | melee / pierce armor | armorClass |
|------|--------|------------------|----------------------|------------|
| Militia | 4 | Melee | 0 / 0 | Infantry |
| Spearman | 3 | Melee | 0 / 0 | Infantry |
| Archer | 4 | **Pierce** | 0 / 0 | Infantry |
| Cavalry | 6 | Melee | 0 / **2** | **Cavalry** |
| Scout | 3 | Melee | 0 / **1** | **Cavalry** |
| Villager | — | Melee | 0 / 0 | None |
| Enemy Dummy | 0 | Melee | 0 / 0 | None |

- Cavalry 系の **pierceArmor > 0** により Archer が騎兵に有利、Spearman は **+12 ボーナス**で近接カウンター — 両方試せる
- `SyncMilitiaStats` 等 **5 関数すべて**更新。`armor` フィールド参照をプロジェクトから除去

### 4. 建築 Data

- `PlacedBuildingData` / `BuildingData` — `armor` → `meleeArmor` + `pierceArmor`（既存値は **両方に同値コピー**で移行）
- `BuildingHealth.Configure(hp, meleeArmor, pierceArmor, team, …)` — シグネチャ拡張（呼び出し元を追随）
- `RuntimeBuildingFactory` / 各建築 `Configure` 経路を更新

### 5. AttackManager / BoarAggroManager

- `ProcessAttackCycle` — `targetArmor` 引数を廃止し、`CombatDamageResolver.Resolve(attacker, target)` を呼ぶ
- Debug.Log に **(Melee/Pierce)** と **bonus 内訳**（任意: `3+12=15 dmg`）を出すと Play 確認しやすい
- `BoarAggroManager` — Villager への Boar 攻撃も Resolver 経由（Boar は Melee 固定で可）

### 6. SelectionInfoPanelView

Phase 25 拡張 — 単体ユニット選択時:

```
Attack: 3 (Melee)
Melee Armor: 0
Pierce Armor: 0
```

- Pierce 攻撃は `Attack: 4 (Pierce)`
- 装甲 0 でも **両方表示**（相性学習用）。建築も同様
- 旧 `Armor: X` 単一行は **削除**

### 7. Phase10 SceneBuilder

- 変更不要想定（Ensure* Sync が走れば Data 更新される）
- 念のため `AoE → Setup Phase10 Scene` を Play 手順に含める

---

## ④ 制約

- rewrite 禁止 / small diff only
- Unity アセット手書き禁止 — Editor API（`Sync*Stats`）
- `.meta` 手書き禁止
- Militia / Spearman / Archer / Cavalry / Scout / Stable / Archery Range **生産・移動・Aggro 既存挙動を壊さない**
- **GameplayBalance 層は触らない**
- ボーナス表の AoE2 完全再現は **不要** — MVP 表のみ
- CPU は相性を意識しない（Phase 41 で攻撃波混在のみ）

---

## ⑤ Play 確認

1. `AoE → Setup Phase10 Scene` → Play
2. **Spearman vs Cavalry:** Console ダメージが Militia より大きい（+12 ボーナス）。Cavalry HP 45 が Spearman で短時間で削れる
3. **Archer vs Cavalry:** Pierce 装甲 2 減算 + Pierce 攻撃 — Militia 近接より効率が良い
4. **Militia vs Cavalry:** ボーナスなし — Spearman / Archer より時間がかかる（相性差の確認）
5. Info Panel — 各ユニットで Melee/Pierce 装甲・攻撃種別が表示される
6. 建築攻撃・Boar・TC 戦闘が従来通り動く
7. Phase 31〜38 回帰（Q/E 生産 / Rally / CPU 攻撃波 / Idle `,`）

**簡易手動セットアップ（任意）:** Stable で Cavalry 2、Barracks で Spearman 2 を造り、CPU Cavalry または Sandbox 上で互いに Attack 命令。

---

## ⑥ 完了時ドキュメント

- [x] `IMPLEMENTATION_STATUS.md` — Phase 39 ✅
- [x] `07_M3_MILITARY_PHASES.md` — Phase 39 ✅
- [x] 本プロンプト — ✅

---

Phase 39 のみ。**Phase 40 Stance / Attack-Move には触れない。**

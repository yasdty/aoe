# Phase 43 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜42 完了（M4 Gameplay Balance + Age Up）  
> **マイルストン:** M4 AoE Gameplay — **Blacksmith & Tech**  
> **ロードマップ:** [08_M4_GAMEPLAY_PHASES.md](../08_M4_GAMEPLAY_PHASES.md)  
> **Balance 方針:** [12_GAMEPLAY_BALANCE_MODE.md](../12_GAMEPLAY_BALANCE_MODE.md) — 研究コスト・時間も **GameplayBalance 層経由**  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 43 実装（Blacksmith & Tech）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 43 のみ実装。** 既存 TC / Barracks **生産キューパターン**を研究キューに流用（rewrite 禁止）。

**前提:** Phase 42 で `Blacksmith` の `PlacedBuildingData`（Feudal 必要）と `GameplayBalance` が導入済み。本 Phase で **建築配置 + 研究 1 系統** を完成させる。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| 建築 | **Blacksmith** — Feudal 以降、村民が建築可能 |
| 研究 | **1 系統** — 例: Militia → Man-at-Arms（`TechnologyData`） |
| キュー | Blacksmith 選択時 OnGUI — 研究ボタン + 進行表示（TC 生産パネル同型） |
| 効果 | 研究完了後、Barracks から **アップグレード済みユニット** を訓練可能 |
| Balance | 研究 Food/Gold コスト・時間 — `GameplayBalance.Scale*` 経由 |
| CPU | 研究は **MVP 任意**（プレイヤー優先。CPU は Phase 44 以降でも可） |

**やらないこと:** 全テックツリー / 大学 / 複数同時研究 / 市場・壁（44-45）/ uGUI 本格 HUD / 弾丸

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| 建築 Data | `Phase1SceneBuilder.EnsureBlacksmithData()`、`PlacedBuildingData.requiredAge` |
| 生産キュー | `ProductionManager` / `BarracksProductionManager` — Job 構造・Tick |
| パネル UI | `BarracksPanelView` / `ProductionPanelView` — OnGUI MVP |
| 配置 | `BuildingPlacementManager` — 新 `PlacedBuildingKind.Blacksmith` 配線 |
| Balance | `GameplayBalance.cs`、`PlacedBuildingData.Scaled*` |
| ユニット | `UnitData`、`UnitSpawner`、`EnsureMilitiaData()` |

---

## ③ 実装タスク

### 1. `TechnologyData`（新規 ScriptableObject）

```csharp
public enum TechnologyKind { InfantryUpgrade } // MVP 1 種

// displayName, foodCost, goldCost, researchTimeSeconds
// prerequisiteAge = Feudal
// outputUnitData — 研究後に訓練可能になる UnitData（例: Man-at-Arms）
// replacesUnitData — 置き換え対象（例: Militia）— Barracks プライマリ訓練の差し替え
```

- `Phase1SceneBuilder.EnsureManAtArmsData()` + `EnsureInfantryUpgradeTech()`
- AoE2 基準値 + Debug Balance で短縮

### 2. `Blacksmith` + `BlacksmithResearchManager`

- `Blacksmith.cs` — Barracks / ArcheryRange と同型（Team, Data, Register）
- `BlacksmithResearchManager` — 単一研究ジョブ or 小キュー（MVP は **1 件のみ** で可）
- 完了時: チームに `TechnologyState`（静的 or `GameSessionManager`）でフラグ記録

### 3. Barracks 訓練の差し替え

- プレイヤー/CPU が `InfantryUpgrade` 研究済みなら `trainUnitData` を Man-at-Arms に
- `BarracksPanelView` ボタン表示名を更新（`Militia` → `Man-at-Arms`）

### 4. 配置・HUD

- `BuildingPlacementManager.EnterBlacksmithPlacementMode`
- `ResourceHudView` — Feudal 以降、Blacksmith 建築ボタン
- `BlacksmithPanelView` — 研究ボタン OnGUI

### 5. Phase10 配線

- `Phase10SceneBuilder` — Blacksmith Data、Manager、Panel、HUD 参照
- Feudal 未満は配置不可（Phase 42 `requiredAge` パターン踏襲）

---

## ④ 制約

- rewrite 禁止 / small diff only
- `.meta` 手書き禁止
- 研究コストは **GameplayBalance のみ**（Debug 用 duplicate Data 禁止）
- OnGUI MVP
- Phase 42 の Age Up / Balance / CPU Relaxed ペースを **壊さない**

---

## ⑤ Play 確認

1. `Phase10.unity` — **Debug + CPU Relaxed**（2分猶予、波 5分、各兵種最大2体）
2. Feudal 昇格 → **Blacksmith 建築可能**
3. Blacksmith 建設 → 研究開始 → 完了
4. Barracks で **Man-at-Arms**（または UP 後ユニット）訓練可能
5. 研究コスト表示 = Balance 後の値
6. M3 軍事（Formation / Stance / Attack-Move）回帰
7. Console エラーなし

---

## ⑥ 完了時ドキュメント

- [ ] `IMPLEMENTATION_STATUS.md` — Phase 43 ✅
- [ ] `08_M4_GAMEPLAY_PHASES.md` — Phase 43 ✅
- [ ] 本プロンプト — Play 確認待ち → ✅

---

Phase 43 のみ。**Phase 44 Defense（壁・塔）には触れない。**

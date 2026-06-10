# Phase 46 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜45 完了（M4 Market）  
> **マイルストン:** M4 AoE Gameplay — **Civilization**  
> **ロードマップ:** [08_M4_GAMEPLAY_PHASES.md](../08_M4_GAMEPLAY_PHASES.md)  
> **拡張設計:** [11_DEFERRED_EXTENSION_DESIGN.md](../11_DEFERRED_EXTENSION_DESIGN.md) — `CivilizationData` asset 追加で文明拡張  
> **Balance 方針:** [12_GAMEPLAY_BALANCE_MODE.md](../12_GAMEPLAY_BALANCE_MODE.md) — ボーナスは **基準値への乗算/加算**（Balance 層の上）  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 46 実装（Civilization）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 46 のみ実装。** 既存 **Data 駆動パターン**（`TechnologyData` / `GameplayBalance`）を文明ボーナスに流用（rewrite 禁止）。

**前提:** Phase 42〜45 で `GameSessionManager` / `UnitTeam` / 資源・生産・Market が整備済み。本 Phase で **1 文明のボーナス MVP** を Data で差し替え可能にする。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| Data | **`CivilizationData`** ScriptableObject — 表示名 + ボーナス 1 種 |
| ボーナス例（どちらか 1 種で可） | **経済:** 村民採集速度 +10% / **軍事:** 歩兵 HP +10% |
| 適用 | `GameSessionManager`（または `CivilizationState`）で **Player / Enemy チーム**に紐付け |
| 既定 | Phase10 — Player = ボーナスあり文明、CPU = 無ボーナス（または別 asset） |
| Inspector / メニュー | `Phase1SceneBuilder.Ensure*CivilizationData()` + Phase10 配線 |
| CPU | ボーナスは Data 適用のみ（専用 AI 不要） |

**やらないこと:** 40 文明 / 固有ユニット / チームボーナス / 文明選択 UI / uGUI 本格 HUD / Second TC（47）/ 壁 Shift+ドラッグ（48）

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| Data パターン | `TechnologyData.cs`、`Phase1SceneBuilder.EnsureInfantryUpgradeTech()` |
| Balance 層 | `GameplayBalance.cs` — 乗算ヘルパーの追加可否 |
| 採集 | `GatherManager` / `FoodGatherManager` / `MineralGatherManager` — 速度適用ポイント |
| 戦闘 | `Unit.cs` / `UnitData` — HP 初期化・`CombatDamageResolver` |
| セッション | `GameSessionManager.cs` — チーム状態・Age Up |
| 拡張設計 | `11_DEFERRED_EXTENSION_DESIGN.md` §1 — `if civilization == X` 散在禁止 |

---

## ③ 実装タスク

### 1. `CivilizationData` + `CivilizationKind`

```csharp
public enum CivilizationBonusKind { GatherRate, InfantryHp } // MVP 1 種ずつ定義可

// displayName, bonusKind, gatherRateMultiplier (1.1f), infantryHpMultiplier (1.1f)
```

- `Phase1SceneBuilder.EnsureDefaultPlayerCivilizationData()` / `EnsureDefaultCpuCivilizationData()`（CPU は 1.0 倍）
- `GameAssetPaths.DefaultPlayerCivilizationData` 等

### 2. 適用レイヤ（`CivilizationBonusUtility` 推奨）

- **採集:** 搬入量 or 採集 tick 速度に `GetGatherRateMultiplier(team)` を乗算
- **歩兵 HP:** Militia / Spearman / Man-at-Arms スポーン時に `ScaledMaxHp` 適用（Archer 除外）
- Manager 内に文明名の `if` を書かず、**Utility + Data** 経由のみ

### 3. `GameSessionManager` 配線

- `[SerializeField] CivilizationData playerCivilization` / `enemyCivilization`
- `GetCivilization(UnitTeam team)` 静的アクセス
- Phase10 既定: Player = ボーナス文明、Enemy = 中立（1.0）

### 4. Phase10 / メニュー

- `Phase10SceneBuilder` — `GameSessionManager` に Data 参照
- `AoE → Add Civilization (Phase46)` パッチメニュー（Phase 45 パターン）
- `AoE → Sync AoE2 Game Data` に `Ensure*CivilizationData` 追加

### 5. 確認用ログ（任意）

- 初回スポーン / 初回採集で `[Civilization] Player gather x1.10` 等（Play 目視困難時）

---

## ④ 制約

- rewrite 禁止 / small diff only
- `.meta` 手書き禁止
- **文明ごとの Manager 分岐禁止** — Data + Utility のみ
- OnGUI 文明選択 UI は作らない（Inspector 既定で可）
- Phase 42 Balance / Market / CPU Relaxed を **壊さない**

---

## ⑤ Play 確認

1. `Phase10.unity` — **Debug + CPU Relaxed**
2. Player 村民の Wood 採集速度が CPU より速い（Gather ボーナス時）
3. または Player Militia の Max HP が CPU より高い（Infantry HP ボーナス時）
4. CPU は 1.0 倍のまま
5. Console エラーなし

---

## ⑥ 完了時ドキュメント

- [ ] `IMPLEMENTATION_STATUS.md` — Phase 46 ✅
- [ ] `08_M4_GAMEPLAY_PHASES.md` — Phase 46 ✅
- [ ] 本プロンプト — Play 確認待ち → ✅

---

Phase 46 のみ。**Phase 47 Second TC には触れない。**

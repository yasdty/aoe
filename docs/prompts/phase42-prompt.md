# Phase 42 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜41 完了（**M3 Military 完了**）  
> **マイルストン:** M4 AoE Gameplay — **Gameplay Balance Mode + Age Up**  
> **ロードマップ:** [08_M4_GAMEPLAY_PHASES.md](../08_M4_GAMEPLAY_PHASES.md)  
> **Balance 方針:** [12_GAMEPLAY_BALANCE_MODE.md](../12_GAMEPLAY_BALANCE_MODE.md) §6 — **本 Phase 先頭で GameplayBalance 実装**  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 42 実装（Gameplay Balance + Age Up）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 42 のみ実装。** 実装順は **確定** — ① GameplayBalance 層 → ② Data AoE2 正本化 → ③ Age Up MVP。

**M4 開始:** M3 完了後の最初の Phase。Debug Play（Phase10 既定）を維持しつつ、AoE2 基準 Data + Balance 倍率の二層を導入する。

---

## ① 目的（2 段階・同一 Phase）

### A. Gameplay Balance Mode（Phase 42 **先頭・必須**）

| 項目 | MVP |
|------|-----|
| モード | **Debug**（既定）/ **AoE2** |
| Debug 倍率 | buildTime **×0.1** / 全コスト **×0.3**（Wood/Food/Gold/Stone） |
| 適用 | 建築配置・訓練・HUD 表示・CPU コスト判定 — **同一 Scale 経由** |
| CPU 遅延 | `barracksBuildDelay` 等 — Debug 時 **×0.1** |
| 切替 | `GameSessionManager` Inspector + **`AoE → Balance Mode → Debug / AoE2`** |
| Data | ScriptableObject に **AoE2 基準値**を Sync（Debug 用別アセット **禁止**） |

### B. Age Up（Balance 導入後）

| 項目 | MVP |
|------|-----|
| 時代 | **Dark Age**（既定）→ **Feudal Age** |
| 昇格 | Town Center でボタン / コマンド — Food+Gold コスト（Balance 層経由） |
| アンロック | `BuildingData.requiredAge` — Feudal で Archery Range / Stable / Blacksmith 等 |
| UI | OnGUI MVP（TC 選択時 Age Up ボタン — M5 uGUI 移行は後回し） |

**やらないこと:** Blacksmith 研究本体（Phase 43）/ 壁・Market（44-45）/ Castle / Imperial / 全文明 / 弾丸 / uGUI 本格 HUD

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| Balance 設計 | `12_GAMEPLAY_BALANCE_MODE.md` §2, §6 |
| 建築コスト | `BuildingPlacementManager`, `PlacedBuildingData`, `PlacedBuildingDataResolver` |
| 訓練 | `BarracksProductionManager`, `ProductionManager`, `*PanelView` コスト表示 |
| CPU 遅延 | `CpuMilitaryAiManager` — `barracksBuildDelaySeconds`, `attackWaveIntervalSeconds` |
| セッション | `GameSessionManager`, `Phase10SceneBuilder` |
| Data Sync | `Phase1SceneBuilder.Ensure*Data()` — 既存 Sync パターン |
| TC | `TownCenter.cs`, `ProductionManager` |

---

## ③ GameplayBalance 実装タスク

### 1. `GameplayBalance`（新規）

- `Assets/Scripts/Core/` 等 — static または `GameSessionManager` 配下
- `enum GameplayBalanceMode { Debug, AoE2 }`
- `ScaleBuildTime(float baseSeconds)`, `ScaleWoodCost(int base)`, … 全資源
- Debug: ×0.1 / ×0.3（§6-A）。AoE2: ×1.0
- `ScaleCpuDelaySeconds(float base)` — Debug ×0.1

### 2. 適用箇所（grep で `woodCost` / `buildTime` / `trainTime` を洗い出し）

- `BuildingPlacementManager` — 配置 Wood 消費・建築完了時間
- 各 `*ProductionManager` — 訓練 Food/Wood/Gold 消費・`trainTime`
- HUD / Panel — ボタン表示コスト（プレイヤーが見る値 = Scale 後）
- `CpuMilitaryAiManager` / `CpuEconomyAiManager` — 閾値判定 + 遅延秒
- `PlacedBuildingDataResolver` fallback — **AoE2 基準**に揃える

### 3. Data AoE2 正本化（§6-D）

`Phase1SceneBuilder.Ensure*Data()` で軍事 + 経済建築を AoE2 基準へ（例）:

| 建築 | Wood | buildTime |
|------|------|-----------|
| Barracks | 175 | 50s |
| Archery Range | 175 | 50s |
| Stable | 175 | 50s |

- ユニット訓練コスト・時間も Liquipedia 等の代表値へ（MVP — 主要ユニットのみで可）
- **Debug Play 体感:** 50s → 5s、175 Wood → 52 — Phase10 で従来と近い速度を維持

### 4. Phase10 配線

- `GameSessionManager` — `[SerializeField] GameplayBalanceMode balanceMode = Debug`
- `Phase10SceneBuilder` — 既定 Debug
- Editor メニュー `AoE/Balance Mode/Debug` / `AoE/Balance Mode/AoE2`

---

## ④ Age Up 実装タスク

### 1. `AgeData` + enum

```csharp
public enum GameAge { Dark, Feudal } // MVP 2 時代のみ
```

- ScriptableObject `AgeData` — `displayName`, `upgradeFoodCost`, `upgradeGoldCost`, `upgradeTimeSeconds`
- Feudal 昇格要件 MVP: **Dark Age TC のみ**（建築数要件は Phase 48 以降可）

### 2. `BuildingData.requiredAge`

- 既定 Dark: TC, House, Barracks（Dark 可のもの）
- Feudal: Archery Range, Stable, Blacksmith（Phase 43 用データ下地）
- `BuildingPlacementManager` / Panel — 時代不足時は配置・ボタン **無効 or 非表示**

### 3. `AgeUpCommand` + TC UI

- TC 選択時 OnGUI — `Age Up to Feudal (Food X / Gold Y)` ボタン
- 実行: 資源消費（Balance 層）→ 昇格タイマー or 即時 MVP → `GameSessionManager.CurrentAge = Feudal`
- `CommandLog` — `AgeUp` 記録

### 4. 既存 Phase10 回帰

- Debug モードで **従来と同等の Play 速度**（Balance 導入で極端に遅く/安くならない）
- M3 軍事（Barracks / Range / Stable / Formation / Stance）が Feudal 昇格前後で破綻しない
- CPU ウェーブ・経済 AI が動作

---

## ⑤ 制約

- rewrite 禁止 / small diff only
- `.meta` 手書き禁止
- **Debug 用 duplicate Data 禁止** — 倍率は GameplayBalance のみ
- OnGUI MVP（Age Up ボタン）
- Blacksmith 研究ロジック（Phase 43）には触れない — **建築 Data のみ Feudal 下地**

---

## ⑥ Play 確認

1. `AoE → Setup Phase10 Scene` → Play（**Debug 既定**）
2. **Balance 回帰** — Barracks 建築 ~5s、コスト体感は Phase 41 前後と同程度
3. **`AoE → Balance Mode → AoE2`** → Play — Barracks 50s 級（遅い）ことを確認（開発確認用）
4. **Age Up** — TC 選択 → Feudal 昇格 → Archery Range / Stable が **配置可能**に
5. Dark Age 中 — Feudal 専建築は **不可**
6. CPU 攻撃波・Formation・Attack-Move 回帰
7. Console エラーなし

---

## ⑦ 完了時ドキュメント

- [ ] `IMPLEMENTATION_STATUS.md` — Phase 42 ✅、Balance Mode ✅、M4 開始
- [ ] `08_M4_GAMEPLAY_PHASES.md` — Phase 42 ✅
- [ ] `12_GAMEPLAY_BALANCE_MODE.md` — §7 チェックリスト更新
- [ ] `07_M3_MILITARY_PHASES.md` — M3 完了条件（Data 正本化）✅
- [ ] 本プロンプト — Play 確認待ち → ✅

---

Phase 42 のみ。**Phase 43 Blacksmith 研究には触れない。**

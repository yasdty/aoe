# AoE RTS Engine — Gameplay Balance Mode 設計

> **状態:** 方針確定（§6 決定済 2026-06）/ 実装は **Phase 42 先頭**（M3 Phase 41 完了後）  
> **関連:** [07_M3_MILITARY_PHASES.md](07_M3_MILITARY_PHASES.md) / [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) / [CONSTITUTION.md](../CONSTITUTION.md)

---

## 1. 目的

- **正（Source of Truth）:** ScriptableObject（`PlacedBuildingData` / `UnitData` 等）に **AoE2 基準のコスト・建築時間・訓練時間** を保持する。
- **開発用:** **Debug モード** では、上記基準値に倍率を掛けて **コスト・建築時間を短縮** し、Phase Play 確認を高速化する。
- **本番想定 Play:** AoE2 モード（倍率 1.0）でバランス検証できる状態にする。

現状（Phase 37 時点）は Barracks **5 秒** / Archery Range **40 秒** など **MVP 暫定値** が混在している。M3 軍事建築が揃った時点で AoE2 基準へ寄せ、Debug 分離を実装する。

---

## 2. 二層アーキテクチャ（確定方針）

```
┌─────────────────────────────────────┐
│  Data Layer（ScriptableObject）      │
│  AoE2 基準の woodCost / buildTime 等 │
└──────────────┬──────────────────────┘
               │ 読み取り時
               ▼
┌─────────────────────────────────────┐
│  GameplayBalance（Runtime 1 箇所）   │
│  Mode: AoE2 | Debug                 │
│  ScaleBuildTime / ScaleWoodCost …   │
└──────────────┬──────────────────────┘
               │
               ▼
  BuildingPlacement / Production / HUD / CPU（消費・表示・判定）
```

### 原則

| 原則 | 内容 |
|------|------|
| **データは 1 系統** | AoE2 値をアセットに直接書く。Debug 用の別アセットセットは作らない |
| **倍率は 1 箇所** | `GameplayBalance` がモードと multiplier を保持 |
| **全経路で適用** | 資源 Spend・建築開始可否・HUD 表示・訓練キュー enqueue が同じ Scale を通る |
| **Editor Sync** | `Phase1SceneBuilder.Ensure*Data()` が AoE2 基準を Sync（既存パターン踏襲） |
| **Resolver fallback** | `PlacedBuildingDataResolver` のランタイム fallback も AoE2 基準に揃える（二重定義防止） |

### モード（確定）

| モード | 用途 | 倍率 |
|--------|------|------|
| **AoE2** | バランス検証・本番想定 Play | buildTime **×1.0** / 全コスト **×1.0** |
| **Debug** | 開発・Phase Play 確認 | buildTime **×0.1** / 全コスト **×0.3** |

**Debug 倍率の適用対象:** Wood / Food / Gold / Stone（訓練・建築・時代昇格コスト等、将来追加資源も同倍率）。

---

## 3. 実装タイミング

| タイミング | 内容 |
|------------|------|
| **Phase 36〜41（M3 実装中）** | 新ユニット・新建築は **既存 MVP パターン** で追加可。Balance 分離は **必須にしない** |
| **M3 完了（Phase 41 後）** | Data 正本化の準備 — 軍事 + 経済建築を **AoE2 基準値**へ一括移行（§6-D） |
| **Phase 42 先頭** | ① `GameplayBalance` 導入 ② Debug 切替 UI（Inspector + `AoE` メニュー）③ CPU AI 遅延の Debug 倍率 |
| **Phase 42 以降（Age Up 等）** | 時代・テック・文明ボーナスは **基準値 + 補正** として Balance 層の上に載せる |

M3 ロードマップ [07_M3_MILITARY_PHASES.md](07_M3_MILITARY_PHASES.md) の **完了条件** に「Balance Mode 設計どおり Data 正本化（§6-D）」を含める。**GameplayBalance 実装本体は Phase 42 先頭**（[08_M4_GAMEPLAY_PHASES.md](08_M4_GAMEPLAY_PHASES.md)）。

---

## 4. 適用箇所（実装時チェックリスト）

- [x] `BuildingPlacementManager` — 配置開始時 Wood 消費・建築 `buildTime`
- [x] `ProductionManager` / `BarracksProductionManager` / `ArcheryRangeProductionManager` / `StableProductionManager` 等 — 訓練コスト・`trainTime`
- [x] `ResourceHudView` / 各 `*PanelView` — ボタン表示コスト
- [x] `PlacedBuildingDataResolver` — fallback 値（AoE2 基準）
- [x] `CpuMilitaryAiManager` / `CpuEconomyAiManager` — AI コスト閾値 **および** 建築遅延秒 — Debug 時 **×0.1**
- [x] `GameSessionManager` + `Phase10SceneBuilder` — シーン既定モード **Debug**
- [x] Editor — `AoE → Balance Mode → Debug / AoE2` メニュー

---

## 5. AoE2 参考（軍事建築 — 確定情報）

Liquipedia / Fandom 等の定番値。**Barracks と Archery Range は建築時間とも 50 秒（ゲーム内）**。

| 建築 | Wood | 建築時間（1 村民） | 備考 |
|------|------|-------------------|------|
| Barracks | 175 | 50s | Dark Age から |
| Archery Range | 175 | 50s | Feudal / Barracks 後 |
| Stable | 175 | 50s | Feudal / Barracks 後 |

経済建築・ユニット訓練コストの AoE2 一覧は **M3 完了時（Phase 41 後）の Data 移行** で表化する（§6-D — 軍事 + 経済建築を一括 AoE2 化）。

---

## 6. 決定事項（2026-06 確定）

| # | 項目 | 決定 |
|---|------|------|
| A | **Debug 倍率** | 建築時間 **×0.1** / コスト **×0.3**（Wood / Food / Gold / Stone すべて同倍率） |
| B | **Editor 既定モード** | **Phase10 = Debug** — `GameSessionManager` の SerializeField + `Phase10SceneBuilder` 配線 |
| C | **CPU AI 遅延** | `barracksBuildDelay` 等も Debug 時 **×0.1**（建築時間のみ短縮にしない — AI 待ち時間も開発向けに短縮） |
| D | **Data AoE2 移行** | **M3 完了時（Phase 41 後）** — 軍事 + 経済建築を AoE2 基準へ **一括移行**（Debug 倍率で短縮） |
| E | **実装 Phase** | **Phase 42 先頭** — `GameplayBalance` 導入・切替 UI・CPU 遅延倍率（Age Up 本体の前） |
| F | **切替 UI** | **`GameSessionManager` Inspector ドロップダウン** + **`AoE` メニュー**（Play 中 OnGUI トグル **なし**） |

### 実装メモ（Phase 42 用）

- `GameplayBalance`（static または `GameSessionManager` 配下シングルトン）が `Mode` と multiplier を保持
- Debug 例: Barracks 建築 50s → **5s**、175 Wood → **52**（端数は `CeilToInt` 等で統一）
- `AoE → Balance Mode → Debug` / `AoE → Balance Mode → AoE2` — Edit Mode で次回 Play の既定を切替
- Data 移行（D）は Phase 41 完了時点の Ensure* Sync で行い、Phase 42 先頭で Balance 層を通す

---

## 7. 関連ドキュメント更新ルール

Balance Mode **実装完了時** に更新:

- [x] 本ファイル — §6 決定事項（2026-06）
- [ ] `IMPLEMENTATION_STATUS.md` — §Data Model / Technical Debt
- [ ] `07_M3_MILITARY_PHASES.md` — M3 完了条件（Data 正本化）
- [ ] `08_M4_GAMEPLAY_PHASES.md` — Phase 42 先頭タスクとして Balance Mode を明記
- [ ] `prompts/phase42-prompt.md` — Balance Mode 実装タスクを先頭セクションに記載

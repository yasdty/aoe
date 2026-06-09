# AoE RTS Engine — Gameplay Balance Mode 設計

> **状態:** 方針確定 / 実装は **M3 完了時（Phase 41 完了後）** を目安  
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
| **倍率は 1 箇所** | `GameplayBalance`（名称 TBD）がモードと multiplier を保持 |
| **全経路で適用** | 資源 Spend・建築開始可否・HUD 表示・訓練キュー enqueue が同じ Scale を通る |
| **Editor Sync** | `Phase1SceneBuilder.Ensure*Data()` が AoE2 基準を Sync（既存パターン踏襲） |
| **Resolver fallback** | `PlacedBuildingDataResolver` のランタイム fallback も AoE2 基準に揃える（二重定義防止） |

### モード（案）

| モード | 用途 |
|--------|------|
| **AoE2** | 基準値そのまま（倍率 1.0） |
| **Debug** | 開発・Phase Play 確認（コスト・建築時間を短縮） |

---

## 3. 実装タイミング

| タイミング | 内容 |
|------------|------|
| **Phase 36〜41（M3 実装中）** | 新ユニット・新建築は **既存 MVP パターン** で追加可。Balance 分離は **必須にしない** |
| **M3 完了（Phase 41 後）** | ① AoE2 基準値を Data / Ensure* に一括反映 ② `GameplayBalance` 導入 ③ Debug 切替 UI / Editor メニュー |
| **M4 以降** | 時代・テック・文明ボーナスは **基準値 + 補正** として Balance 層の上に載せる |

M3 ロードマップ [07_M3_MILITARY_PHASES.md](07_M3_MILITARY_PHASES.md) の **完了条件** に「Balance Mode 設計どおり Data 正本化」を含める（実装 Phase は 41 完了後でも可）。

---

## 4. 適用箇所（実装時チェックリスト）

- [ ] `BuildingPlacementManager` — 配置開始時 Wood 消費・建築 `buildTime`
- [ ] `ProductionManager` / `BarracksProductionManager` / `ArcheryRangeProductionManager` 等 — 訓練コスト・`trainTime`
- [ ] `ResourceHudView` / 各 `*PanelView` — ボタン表示コスト
- [ ] `PlacedBuildingDataResolver` — fallback 値
- [ ] `CpuMilitaryAiManager` / `CpuEconomyAiManager` — AI が参照するコスト閾値（**要決定:** 建築遅延秒も Debug 倍率対象か）
- [ ] `Phase10SceneBuilder` — シーン既定モード（**要決定**）

---

## 5. AoE2 参考（軍事建築 — 確定情報）

Liquipedia / Fandom 等の定番値。**Barracks と Archery Range は建築時間とも 50 秒（ゲーム内）**。

| 建築 | Wood | 建築時間（1 村民） | 備考 |
|------|------|-------------------|------|
| Barracks | 175 | 50s | Dark Age から |
| Archery Range | 175 | 50s | Feudal / Barracks 後 |
| Stable | 175 | 50s | Feudal / Barracks 後 |

経済建築・ユニット訓練コストの AoE2 一覧は **M3 完了時の Data 移行 Phase で表化** する（現時点では軍事建築のみ記載）。

---

## 6. 要決定事項（ユーザー確認待ち）

以下は **方針ドキュメントに値を書かない**。決定後に本節を更新し、実装 Phase プロンプトへ反映する。

| # | 項目 | 選択肢の例 |
|---|------|------------|
| A | **Debug 倍率** | 建築時間 0.1x / コスト 0.3x 等 — 具体値 |
| B | **Editor 既定モード** | Phase10 Play 時デフォルト Debug vs AoE2 |
| C | **CPU AI 遅延**（Barracks 90s 等） | Debug でも短縮する / 建築時間のみ短縮 |
| D | **経済建築の AoE2 移行** | M3 完了時に一括 / 軍事のみ先 / 比率のみ AoE2 |
| E | **実装 Phase 番号** | Phase 41 内タスク / Phase 42 先頭 / 独立 Phase |
| F | **切替 UI** | Inspector のみ / `AoE` メニュー / Play 中 OnGUI トグル |

---

## 7. 関連ドキュメント更新ルール

Balance Mode **実装完了時** に更新:

- [ ] 本ファイル — §6 要決定を確定値に差し替え
- [ ] `IMPLEMENTATION_STATUS.md` — §Data Model / Technical Debt
- [ ] `07_M3_MILITARY_PHASES.md` — M3 完了条件
- [ ] 該当 Phase プロンプト（TBD）

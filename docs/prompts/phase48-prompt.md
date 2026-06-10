# Phase 48 実行プロンプト

> **状態:** ✅ 完了（Play 確認済 — 壁 #5 は部分達成、本格壁は Phase 49）  
> **前提:** Phase 1〜47 完了（M4 Second TC）  
> **マイルストン:** M4 AoE Gameplay — **RTS UX Polish**  
> **ロードマップ:** [08_M4_GAMEPLAY_PHASES.md](../08_M4_GAMEPLAY_PHASES.md)  
> **次:** [phase49-prompt.md](phase49-prompt.md) — Wall & Gate

---

## Play 確認結果（2026-06）

| # | 項目 | 結果 |
|---|------|------|
| 1 | TC キュー取消 + 返金 | ✅ |
| 2 | Barracks Shift+Q ×5 | ✅ |
| 3 | House 破壊 → Pop -5（選択 → K） | ✅ |
| 4 | H / B 建築ホットキー | ✅ |
| 5 | 壁 Shift+配置 | △ Shift+**クリック**連続のみ。ドラッグ・遮断・Gate **未達** → Phase 49 |
| 6 | キュー表示名（Unit 問題） | ✅ |
| 7 | Console エラーなし | ✅ |

**Play シーン:** `Phase10.unity` / Debug + CPU Relaxed / `Sync Input Actions` 後 Ctrl+S

---

# 依頼: AoE RTS Engine — Phase 48 実装（RTS UX Polish）

（以下は実装時の参照用 — **実装済み**）

## 実装サマリー

| 項目 | 実装 |
|------|------|
| キュー取消 + 返金 | `ProductionQueueRefundUtility` + 各 `*ProductionManager.TryCancelQueueItem` |
| キュー UI | `ProductionQueuePanelUi` + `ProductionQueueDisplayUtility` |
| Shift+5 | `ProductionQueuePanelUi.ShiftBatchQueueCount` |
| House Pop | `PopulationManager.RemoveHousing` + `BuildingHealth.Die` |
| H / B | `BuildHouse` / `BuildBarracks` Input Actions |
| 壁 Shift | `BuildingPlacementManager` — Shift 中配置維持（部分） |
| 表示名 | `UnitDisplayNameUtility` / `UnitDataResolver` |
| Debug K | `DebugPlaytestInput` — 選択 TC または自軍 Placed Building |

## 意図的に Phase 49 へ

- 壁通行遮断 / AoE2 型ドラッグ / Gate
- 時代別壁グレード → Phase 50
- 日本語 i18n → Phase 51

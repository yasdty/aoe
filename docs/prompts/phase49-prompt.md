# Phase 49 実行プロンプト

> **状態:** ✅ 完了（Play 確認済）  
> **前提:** Phase 1〜48 完了  
> **マイルストン:** M5 — **Wall & Gate System**  
> **ロードマップ:** [09_M5_VISUAL_UI_PHASES.md](../09_M5_VISUAL_UI_PHASES.md)  
> **次:** [phase50-prompt.md](phase50-prompt.md) — Wall Age Grades

---

## Play 確認結果（2026-06）

| # | 項目 | 結果 |
|---|------|------|
| 1 | Palisade **ドラッグ** → 壁列（工事中基礎 → 順次完成） | ✅ |
| 2 | 完成壁で村民 **すり抜けない** | ✅ |
| 3 | Gate（壁隣接）自軍通過 | ✅（要再確認可） |
| 4 | Stone Wall ドラッグ | ✅ |
| 5 | **ドラッグ中の列ゴースト** | ⬜ **Phase 52〜53 へ繰越**（gameplay 優先で Phase 49 スコープ外） |

---

## 実装サマリー

| 項目 | 実装 |
|------|------|
| 通行遮断 | `WallOccupancyRegistry` + `Unit.TickMovement` パス判定 |
| ドラッグ連続配置 | `BuildingPlacementManager.UpdateWallDragInput` → `TryConfirmWallDragLine` |
| セグメント列 | `WallPlacementUtility` + `TryQueueWallSegmentAt` + `queuedBuilder` 順次建築 |
| Gate | `Gate` / `GateData` / HUD / 自軍通過 |
| 単体ゴースト | ポインタ先 1 セグメントのみ（既存 `PlacementGhost`） |

**Editor:** `AoE → Sync AoE2 Game Data` / `Add Defense (Phase44)` → Ctrl+S

---

Phase 49 のみ。**Phase 50（壁 Age グレード）・Phase 51（i18n）には触れない。**

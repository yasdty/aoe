# AoE RTS Engine — ドキュメント索引

> **読む順番:** 下表の **#** 列の昇順。Phase 実装・引き継ぎ時はこの順で参照する。

| # | ファイル | Phase | 内容 |
|---|----------|-------|------|
| — | [../CONSTITUTION.md](../CONSTITUTION.md) | — | プロジェクト憲法（技術制約・AI ルール） |
| — | [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) | 1〜61 | **実装状況の正本**（機能一覧・技術負債・依存関係）。**Phase 完了ごとに更新** |
| 01 | [01_M0_POC_PHASES.md](01_M0_POC_PHASES.md) | 1〜10.5 | **Milestone 0 — PoC** ロードマップ |
| 02 | [02_M1_FOUNDATION_PHASES.md](02_M1_FOUNDATION_PHASES.md) | 11〜16 | **Milestone 1 — Foundation** ロードマップ ✅ 完了 |
| 03 | [03_M2_ECONOMY_PHASES.md](03_M2_ECONOMY_PHASES.md) | 17〜20 | **Milestone 2 — Economy** ✅ 完了 |
| 04 | [04_M2_5_ECONOMY_POLISH_PHASES.md](04_M2_5_ECONOMY_POLISH_PHASES.md) | 21〜30 | **Milestone 2.5 — Economy Polish** ✅ 完了 |
| 05 | [05_M2_6_RTS_UX_PHASES.md](05_M2_6_RTS_UX_PHASES.md) | 31〜34 | **Milestone 2.6 — RTS UX** ✅ 完了 |
| 06 | [06_M2_7_SANDBOX_PHASES.md](06_M2_7_SANDBOX_PHASES.md) | 35 | **Milestone 2.7 — Sandbox** ✅ 完了 |
| 07 | [07_M3_MILITARY_PHASES.md](07_M3_MILITARY_PHASES.md) | 36〜41 | **Milestone 3 — Military** ✅ 完了 |
| 08 | [08_M4_GAMEPLAY_PHASES.md](08_M4_GAMEPLAY_PHASES.md) | 42〜48 | **Milestone 4 — AoE Gameplay** ✅ 完了 |
| 09 | [09_M5_VISUAL_UI_PHASES.md](09_M5_VISUAL_UI_PHASES.md) | 49〜56 | **Milestone 5 — Gameplay Polish & Visual / UI** ⬜ 未着手 |
| 10 | [10_M6_MULTIPLAYER_FOUNDATION.md](10_M6_MULTIPLAYER_FOUNDATION.md) | 57〜61 | **Milestone 6 — Multiplayer Foundation** ⬜ 未着手 |
| 11 | [11_DEFERRED_EXTENSION_DESIGN.md](11_DEFERRED_EXTENSION_DESIGN.md) | — | **意図的スコープ外の拡張設計** |
| 12 | [12_GAMEPLAY_BALANCE_MODE.md](12_GAMEPLAY_BALANCE_MODE.md) | — | **Gameplay Balance Mode**（AoE2 正本 + Debug 短縮） |
| — | [prompts/](prompts/) | — | 各 Phase の Agent 実行プロンプト |

---

## 各ファイルの役割

### `IMPLEMENTATION_STATUS.md`（正本）

- **何か:** コードベース全体の「今どこまでできているか」を 1 ファイルに集約した実装状況書
- **含むもの:** 機能一覧、Data Model、Technical Debt、Performance、Multiplayer Readiness、Dependency Graph、完成度投影
- **いつ更新:** **各 Phase 完了時**（README の Phase 節とあわせて）。Phase 実装で触れない横断項目（Input 一覧・Missing Features 等）も **M 移行前に棚卸し**

### `0N_M*_*.md`（マイルストンロードマップ）

- **命名規則:** `{順番}_M{マイルストン番号}_{名称}_PHASES.md`
- **17 以降:** `03_M2` → `04_M2_5` → `05_M2_6` → `06_M2_7` → `07_M3` → `08_M4` → `09_M5` → `10_M6`

### `prompts/phaseN-prompt.md`

- Agent への実装依頼文。ロードマップ MD を `@` 添付して使用。

---

## マイルストン一覧

| Milestone | ファイル | Phase | 状態 |
|-----------|----------|-------|------|
| M0 PoC | `01_M0_POC_PHASES.md` | 1〜10.5 | ✅ 完了 |
| M1 Foundation | `02_M1_FOUNDATION_PHASES.md` | 11〜16 | ✅ 完了 |
| M2 Economy | `03_M2_ECONOMY_PHASES.md` | 17〜20 | ✅ 完了 |
| M2.5 Economy Polish | `04_M2_5_ECONOMY_POLISH_PHASES.md` | 21〜30 | ✅ 完了 |
| M2.6 RTS UX | `05_M2_6_RTS_UX_PHASES.md` | 31〜34 | ✅ 完了 |
| M2.7 Sandbox | `06_M2_7_SANDBOX_PHASES.md` | 35 | ✅ 完了 |
| M3 Military | `07_M3_MILITARY_PHASES.md` | 36〜41 | ✅ 完了 |
| M4 AoE Gameplay | `08_M4_GAMEPLAY_PHASES.md` | 42〜48 | ✅ 完了 |
| M5 Gameplay Polish & Visual / UI | `09_M5_VISUAL_UI_PHASES.md` | 49〜56 | ⬜ 未着手 — **次: Phase 49 Wall & Gate** |
| M6 Multiplayer Foundation | `10_M6_MULTIPLAYER_FOUNDATION.md` | 57〜61 | ⬜ 未着手 |

---

## Phase 実装後の更新チェックリスト

1. [ ] `IMPLEMENTATION_STATUS.md` — §2 Progress、§3 Features、§7〜8、§12、Dependency Graph、Quick Guide
2. [ ] 該当 `0N_M*_PHASES.md` — Phase 行を ✅
3. [ ] [../README.md](../README.md) — セットアップ・Phase 節（必要時）
4. [ ] `prompts/phaseN-prompt.md` — 状態を ✅

**M 移行前の追加棚卸し:** Input アクション一覧、Missing Features、AoE2 Coverage、Milestone 状態行

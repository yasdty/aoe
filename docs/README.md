# AoE RTS Engine — ドキュメント索引

> **読む順番:** 下表の **#** 列の昇順。Phase 実装・引き継ぎ時はこの順で参照する。

| # | ファイル | Phase | 内容 |
|---|----------|-------|------|
| — | [../CONSTITUTION.md](../CONSTITUTION.md) | — | プロジェクト憲法（技術制約・AI ルール） |
| — | [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) | 1〜58 | **実装状況の正本**（機能一覧・技術負債・依存関係）。**Phase 完了ごとに更新** |
| 01 | [01_M0_POC_PHASES.md](01_M0_POC_PHASES.md) | 1〜10.5 | **Milestone 0 — PoC** ロードマップ |
| 02 | [02_M1_FOUNDATION_PHASES.md](02_M1_FOUNDATION_PHASES.md) | 11〜16 | **Milestone 1 — Foundation** ロードマップ ✅ 完了 |
| 03 | [03_M2_ECONOMY_PHASES.md](03_M2_ECONOMY_PHASES.md) | 17〜20 | **Milestone 2 — Economy** ✅ 完了 |
| 04 | [04_M2_5_ECONOMY_POLISH_PHASES.md](04_M2_5_ECONOMY_POLISH_PHASES.md) | 21〜30 | **Milestone 2.5 — Economy Polish** ✅ 完了 |
| 05 | [05_M2_6_RTS_UX_PHASES.md](05_M2_6_RTS_UX_PHASES.md) | 31〜34 | **Milestone 2.6 — RTS UX** ✅ 完了 |
| 06 | [06_M2_7_SANDBOX_PHASES.md](06_M2_7_SANDBOX_PHASES.md) | 35 | **Milestone 2.7 — Sandbox** ⬜ 未着手 |
| 07 | [07_M3_MILITARY_PHASES.md](07_M3_MILITARY_PHASES.md) | 36〜41 | **Milestone 3 — Military** ⬜ 未着手 |
| 08 | [08_M4_GAMEPLAY_PHASES.md](08_M4_GAMEPLAY_PHASES.md) | 42〜48 | **Milestone 4 — AoE Gameplay** ⬜ 未着手 |
| 09 | [09_M5_VISUAL_UI_PHASES.md](09_M5_VISUAL_UI_PHASES.md) | 49〜53 | **Milestone 5 — Visual / UI** ⬜ 未着手 |
| 10 | [10_M6_MULTIPLAYER_FOUNDATION.md](10_M6_MULTIPLAYER_FOUNDATION.md) | 54〜58 | **Milestone 6 — Multiplayer Foundation** ⬜ 未着手 |
| 11 | [11_DEFERRED_EXTENSION_DESIGN.md](11_DEFERRED_EXTENSION_DESIGN.md) | — | **意図的スコープ外の拡張設計** |
| — | [prompts/](prompts/) | — | 各 Phase の Agent 実行プロンプト |

---

## 各ファイルの役割

### `IMPLEMENTATION_STATUS.md`（正本）

- **何か:** コードベース全体の「今どこまでできているか」を 1 ファイルに集約した実装状況書
- **含むもの:** 機能一覧、Data Model、Technical Debt、Performance、Multiplayer Readiness、Dependency Graph、完成度投影
- **いつ更新:** **各 Phase 完了時**（README の Phase 節とあわせて）

### `0N_M*_*.md`（マイルストンロードマップ）

- **命名規則:** `{順番}_M{マイルストン番号}_{名称}_PHASES.md`（または `11_DEFERRED_...`）
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
| M2.7 Sandbox | `06_M2_7_SANDBOX_PHASES.md` | 35 | ⬜ 未着手 |
| M3 Military | `07_M3_MILITARY_PHASES.md` | 36〜41 | ⬜ 未着手 |
| M4 AoE Gameplay | `08_M4_GAMEPLAY_PHASES.md` | 42〜48 | ⬜ 未着手 |
| M5 Visual / UI | `09_M5_VISUAL_UI_PHASES.md` | 49〜53 | ⬜ 未着手 |
| M6 Multiplayer Foundation | `10_M6_MULTIPLAYER_FOUNDATION.md` | 54〜58 | ⬜ 未着手 |

---

## Phase 実装後の更新チェックリスト

1. [ ] `IMPLEMENTATION_STATUS.md` — §2 Progress、§3 Features、§9〜11、Dependency Graph
2. [ ] 該当 `0N_M*_PHASES.md` — Phase 行を ✅
3. [ ] [../README.md](../README.md) — セットアップ・Phase 節
4. [ ] `prompts/phaseN-prompt.md` — 状態を ✅

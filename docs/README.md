# AoE RTS Engine — ドキュメント索引

> **読む順番:** 下表の **#** 列の昇順。Phase 実装・引き継ぎ時はこの順で参照する。

| # | ファイル | Phase | 内容 |
|---|----------|-------|------|
| — | [../CONSTITUTION.md](../CONSTITUTION.md) | — | プロジェクト憲法（技術制約・AI ルール） |
| — | [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) | 1〜43 | **実装状況の正本**（機能一覧・技術負債・依存関係）。**Phase 完了ごとに更新** |
| 01 | [01_M0_POC_PHASES.md](01_M0_POC_PHASES.md) | 1〜10.5 | **Milestone 0 — PoC** ロードマップ |
| 02 | [02_M1_FOUNDATION_PHASES.md](02_M1_FOUNDATION_PHASES.md) | 11〜16 | **Milestone 1 — Foundation** ロードマップ ✅ 完了 |
| 03 | [03_M2_ECONOMY_PHASES.md](03_M2_ECONOMY_PHASES.md) | 17〜20 | **Milestone 2 — Economy** ✅ 完了 |
| 04 | [04_M2_5_ECONOMY_POLISH_PHASES.md](04_M2_5_ECONOMY_POLISH_PHASES.md) | 21〜28 | **Milestone 2.5 — Economy Polish** 🔄 21 ✅ |
| 05 | [05_M2_6_RTS_UX_PHASES.md](05_M2_6_RTS_UX_PHASES.md) | 29〜32 | **Milestone 2.6 — RTS UX** ⬜ M3 前 |
| 06 | [06_M3_MILITARY_PHASES.md](06_M3_MILITARY_PHASES.md) | 33〜38 | **Milestone 3 — Military** ⬜ 未着手 |
| 07 | [07_M4_GAMEPLAY_PHASES.md](07_M4_GAMEPLAY_PHASES.md) | 39〜43 | **Milestone 4 — AoE Gameplay** ⬜ 未着手 |
| — | [prompts/](prompts/) | — | 各 Phase の Agent 実行プロンプト |

---

## 各ファイルの役割

### `IMPLEMENTATION_STATUS.md`（正本）

- **何か:** コードベース全体の「今どこまでできているか」を 1 ファイルに集約した実装状況書
- **含むもの:** 機能一覧、Data Model、Technical Debt、Performance、Multiplayer Readiness、Dependency Graph
- **いつ更新:** **各 Phase 完了時**（README の Phase 節とあわせて）

### `0N_M*_*.md`（マイルストンロードマップ）

- **命名規則:** `{順番}_M{マイルストン番号}_{名称}_PHASES.md`
- **17 以降:** `03_M2_...`, `04_M2_5_...`, `05_M2_6_...`, `06_M3_...`, `07_M4_...`

### `prompts/phaseN-prompt.md`

- Agent への実装依頼文。ロードマップ MD を `@` 添付して使用。

---

## マイルストン一覧

| Milestone | ファイル | Phase | 状態 |
|-----------|----------|-------|------|
| M0 PoC | `01_M0_POC_PHASES.md` | 1〜10.5 | ✅ 完了 |
| M1 Foundation | `02_M1_FOUNDATION_PHASES.md` | 11〜16 | ✅ 完了 |
| M2 Economy | `03_M2_ECONOMY_PHASES.md` | 17〜20 | ✅ 完了 |
| M2.5 Economy Polish | `04_M2_5_ECONOMY_POLISH_PHASES.md` | 21〜28 | 🔄 Phase 21 ✅ / 22〜28 ⬜ |
| M2.6 RTS UX | `05_M2_6_RTS_UX_PHASES.md` | 29〜32 | ⬜ M3 前 |
| M3 Military | `06_M3_MILITARY_PHASES.md` | 33〜38 | ⬜ 未着手 |
| M4 AoE Gameplay | `07_M4_GAMEPLAY_PHASES.md` | 39〜43 | ⬜ 未着手 |

---

## Phase 実装後の更新チェックリスト

1. [ ] `IMPLEMENTATION_STATUS.md` — §2 Progress、§3 Features、§9〜11、Dependency Graph
2. [ ] 該当 `0N_M*_PHASES.md` — Phase 行を ✅
3. [ ] [../README.md](../README.md) — セットアップ・Phase 節
4. [ ] `prompts/phaseN-prompt.md` — 状態を ✅

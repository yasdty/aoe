# AoE RTS Engine — ドキュメント索引

> **読む順番:** 下表の **#** 列の昇順。Phase 実装・引き継ぎ時はこの順で参照する。

| # | ファイル | Phase | 内容 |
|---|----------|-------|------|
| — | [../CONSTITUTION.md](../CONSTITUTION.md) | — | プロジェクト憲法（技術制約・AI ルール） |
| — | [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md) | 1〜16 | **実装状況の正本**（機能一覧・技術負債・依存関係）。**Phase 完了ごとに更新** |
| 01 | [01_M0_POC_PHASES.md](01_M0_POC_PHASES.md) | 1〜10.5 | **Milestone 0 — PoC** ロードマップ |
| 02 | [02_M1_FOUNDATION_PHASES.md](02_M1_FOUNDATION_PHASES.md) | 11〜16 | **Milestone 1 — Foundation** ロードマップ ✅ 完了 |
| 03 | [03_M2_ECONOMY_PHASES.md](03_M2_ECONOMY_PHASES.md) | 17〜20 | **Milestone 2 — Economy**（Phase 17 ✅） |
| — | [prompts/](prompts/) | — | 各 Phase の Agent 実行プロンプト |

---

## 各ファイルの役割

### `IMPLEMENTATION_STATUS.md`（正本）

- **何か:** コードベース全体の「今どこまでできているか」を 1 ファイルに集約した実装状況書
- **含むもの:** 機能一覧、Data Model、Technical Debt、Performance、Multiplayer Readiness、Dependency Graph
- **いつ更新:** **各 Phase 完了時**（README の Phase 節とあわせて）
- **旧ファイル:** `RTS_IMPLEMENTATION_STATUS.md` は統合済み（リダイレクト用スタブは廃止）

### `0N_M*_*.md`（マイルストンロードマップ）

- **命名規則:** `{順番}_M{マイルストン番号}_{名称}_PHASES.md`
- **例:** `01_M0_POC_PHASES.md` = 最初に読む PoC ロードマップ（Phase 1〜10.5）
- **17 以降:** マイルストンごとに `03_M2_...`, `04_M3_...` を追加

### `prompts/phaseN-prompt.md`

- Agent への実装依頼文。ロードマップ MD を `@` 添付して使用。

---

## マイルストン一覧

| Milestone | ファイル | Phase | 状態 |
|-----------|----------|-------|------|
| M0 PoC | `01_M0_POC_PHASES.md` | 1〜10.5 | ✅ 完了 |
| M1 Foundation | `02_M1_FOUNDATION_PHASES.md` | 11〜16 | ✅ 完了 |
| M2 Economy | `03_M2_ECONOMY_PHASES.md` | 17〜20 | △ Phase 17 ✅ |
| M3 Military | `04_M3_MILITARY_PHASES.md` | 21〜26 | ⬜ 未着手 |
| M4 AoE Gameplay | `05_M4_GAMEPLAY_PHASES.md` | 27〜31 | ⬜ 未着手 |

---

## Phase 実装後の更新チェックリスト

1. [ ] `IMPLEMENTATION_STATUS.md` — §2 Progress、§3 Features、§9〜11、Dependency Graph
2. [ ] 該当 `0N_M*_PHASES.md` — Phase 行を ✅
3. [ ] [../README.md](../README.md) — セットアップ・Phase 節
4. [ ] `prompts/phaseN-prompt.md` — 状態を ✅

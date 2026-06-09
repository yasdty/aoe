# AoE RTS Engine — Milestone 2.7: Sandbox Phase ロードマップ（Phase 35）

> **Milestone:** M2.7 — Phase10 サンドボックス拡張  
> **前提:** [05_M2_6_RTS_UX_PHASES.md](05_M2_6_RTS_UX_PHASES.md)（Phase 31〜34）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **前:** M2.6 RTS UX / **次:** [07_M3_MILITARY_PHASES.md](07_M3_MILITARY_PHASES.md)（Phase 36〜41）

> **2026-06 追記:** M2.6 付録だった「サンドボックス拡張」を **正式 Phase 35 / M2.7** に昇格。M3 は **Phase 36** から開始。

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 35 | Phase10 Sandbox | マップ拡大・資源増・CPU バランス調整 | ⬜ 未着手 |

**M2.7 完了条件:**

- `AoE → Setup Phase10 Scene` 再実行後、Player が **10 分以上** 4 資源経済を回せる
- CPU ラッシュが **即死レベルでない**
- Phase 31〜34 の操作 UX（キュー / Idle / Rally / Control Group）が **回帰しない**

---

## 背景

現状 `Phase10.unity` は Player / CPU TC 間 **約 35m**、木 **28 本前後**、Gold / Stone **各陣営 1 鉱山** と狭く、Phase 30 以降の 4 資源経済 + CPU 軍事 AI では **資源枯渇・早期ラッシュ** が起きやすい。M2.6 の UX 検証は狭いマップで足りるため、**環境拡張は M3 直前にまとめて実施**する。

---

## Phase 35 — Phase10 Sandbox ⬜

**目的:** M3（兵種拡張）入り前に、広い固定マップで経済・軍事を本格テストできる環境を整える。

**触る場所（主）:**

| 項目 | ファイル / 対象 |
|------|----------------|
| TC 間隔・資源配置 | `Phase10SceneBuilder.cs` — Tree / Berry / Deer / Sheep / Boar / Gold / Stone 配列 |
| TC 座標 | `PlayerTownCenterPosition` / `CpuTownCenterPosition` |
| カメラ | `CameraFocus` — 両 TC の中間 |
| 地面 | Plane スケール（Setup メニューで再生成） |
| 資源量 | ScriptableObject（`DefaultTree` 等）— 必要時のみ |
| CPU 難易度 | `DefaultAttackWaveIntervalSeconds` / `DefaultBarracksBuildDelaySeconds` |

**MVP 目安:**

| 要素 | 現状 | 拡張目安 |
|------|------|----------|
| TC 間隔 | ~35m | **55〜70m** |
| 木 | 28 本 | **40〜50 本**（両陣営に分散） |
| Berry Bush | 各 3 | **各 4〜5** |
| Gold / Stone | 各陣営 1 | **各 2** |
| Deer / Sheep | 少数 | **各 +1〜2** |

**やらないこと:**

- ランダムマップ生成 → [11_DEFERRED_EXTENSION_DESIGN.md](11_DEFERRED_EXTENSION_DESIGN.md)（M4 後 / P3）
- 新ゲームシステム・新ユニット種
- CPU AI ロジックの rewrite

**プロンプト:** [prompts/phase35-prompt.md](prompts/phase35-prompt.md)

---

## 進め方

1. Phase 34 Play 確認 OK
2. Phase 35 実装 → `Phase10.unity` Play（10 分経済テスト）
3. [07_M3_MILITARY_PHASES.md](07_M3_MILITARY_PHASES.md) Phase 36 へ

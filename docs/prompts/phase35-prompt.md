# Phase 35 — Phase10 Sandbox（M2.7）実装プロンプト

> **状態:** ✅ 完了（Play 確認済み）

> **Phase:** 35  
> **Milestone:** M2.7 — Sandbox  
> **ロードマップ:** [06_M2_7_SANDBOX_PHASES.md](../06_M2_7_SANDBOX_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

## 依頼

Phase 35（Phase10 サンドボックス拡張）のみ実装してください。

**目的:** M3 兵種拡張前に、広い固定マップで 4 資源経済 + CPU 対戦を **10 分以上** 回せるテスト環境を整える。

---

## 制約

| 項目 | 内容 |
|------|------|
| **small diff** | `Phase10SceneBuilder.cs` の配置定数・地面スケール・CPU タイミング調整が中心 — rewrite 禁止 |
| **シーン** | `Phase10.unity` のみ（`AoE → Setup Phase10 Scene` で再生成） |
| **新システム禁止** | 新ユニット種・新 Manager・新ゲームルールは追加しない |
| **CPU AI rewrite 禁止** | 数値定数の調整のみ |
| **`.meta` は 32 文字 GUID** | 新規 .cs なし想定。追加する場合は Editor 生成 |

---

## 変更対象

### `Assets/Scripts/Editor/Phase10SceneBuilder.cs`

| 定数 | 現状目安 | 目標 |
|------|----------|------|
| `CpuTownCenterPosition.z` | -35 | **-55 〜 -70** |
| `CameraFocus` | TC 中間 | 新 TC 間隔の中間 |
| `TreePositions` | ~28 本 | **40〜50 本**（両陣営側に分散） |
| Berry / Deer / Sheep / Boar | 現状 | **各 +1〜2** |
| Gold / Stone 鉱山 | 各陣営 1 | **各 2**（遠方追加可） |
| Ground Plane scale | 現状 | TC 間隔に合わせ拡大 |
| `DefaultAttackWaveIntervalSeconds` | 30 | **45〜60**（即死ラッシュ回避） |
| `DefaultBarracksBuildDelaySeconds` | 60 | 必要なら **90** 前後 |

資源 **量**（`DefaultTree` 等）は枯渇が早すぎる場合のみ ScriptableObject 調整。

---

## やらないこと

- ランダムマップ
- M3 兵種（Archer 等）
- Phase 31〜34 のロジック変更
- 新 prefab / 手書き Unity アセット

---

## Play 確認

1. `AoE → Setup Phase10 Scene` 実行 → シーン保存
2. Play — Player が **10 分以上** Wood / Food / Gold / Stone 経済を継続できる
3. CPU ラッシュで **即死しない**（調整後再確認）
4. Phase 31〜34 回帰: Q キュー / `.` Idle / Rally / Ctrl+1 グループ

---

## 完了時ドキュメント更新

- [x] `docs/IMPLEMENTATION_STATUS.md` — Phase 35 ✅
- [x] `docs/06_M2_7_SANDBOX_PHASES.md` — Phase 35 ✅
- [x] 本プロンプト — Play 確認済み ✅

---

Phase 35 のみ実装。**M3 Phase 36 には触れない。**

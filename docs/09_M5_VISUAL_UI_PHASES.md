# AoE RTS Engine — Milestone 5: Visual & UI Phase ロードマップ（Phase 49〜53）

> **Milestone:** M5 — Visual / UI Polish  
> **前提:** [08_M4_GAMEPLAY_PHASES.md](08_M4_GAMEPLAY_PHASES.md)（Phase 42〜48）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **前:** M4 AoE Gameplay / **次:** [10_M6_MULTIPLAYER_FOUNDATION.md](10_M6_MULTIPLAYER_FOUNDATION.md)（Phase 54〜58）

> **背景:** M0〜M4 は OnGUI MVP + 色変化ビジュアルで進めてきた。本マイルストンで **本番スケール UI** と **最低限のアニメーション** を導入する。マルチプレイそのものは M6。

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 49 | View Layer Split | Simulation / View 分離の土台 + uGUI Canvas シェル | ⬜ 未着手 |
| 50 | HUD Migration | 資源・生産・選択パネルを uGUI / UI Toolkit へ移行 | ⬜ 未着手 |
| 51 | Minimap | 俯瞰ミニマップ + TC / 主要建築アイコン | ⬜ 未着手 |
| 52 | Unit Animation | Animator MVP（歩行・採集・攻撃）— Editor API 生成 | ⬜ 未着手 |
| 53 | Combat VFX & Audio | 弾丸・ヒットフィードバック・SE フック | ⬜ 未着手 |

**M5 完了条件:**

- OnGUI 依存 HUD が **主要パネルから除去**（SelectionBox 等は残存可）
- 1280×720 以上で **レイアウト崩れなし**
- ミニマップで両 TC 位置が把握できる
- Villager / Militia / Archer が **状態に応じたアニメ**（ループ 3 種以上）
- 遠距離攻撃に **視覚的フィードバック**あり

---

## Phase 49 — View Layer Split ⬜

**目的:** Manager 内 OnGUI を段階的に剥がす。マルチプレイ時に **各クライアントが独立 View** を持てる構造にする。

**実装方針:**

- `IHudPresenter` / `ISelectionView` 等の薄いインターフェース
- Simulation Manager は **View を直接参照しない**（イベント or 読み取り専用 Snapshot）
- uGUI Canvas を Editor API（`Phase10SceneBuilder`）で生成 — prefab 手書き禁止

**マルチプレイ関連:** この Phase 完了で「表示層の差し替え」が可能。**ネットワーク自体は M6**。

**プロンプト:** [prompts/phase49-prompt.md](prompts/phase49-prompt.md)（未作成）

---

## Phase 50 — HUD Migration ⬜

**移行対象:** `ResourceHudView`, `ProductionPanelView`, `SelectionInfoPanelView`, `IdleUnitHudView`, `VictoryDefeatHudView`

**プロンプト:** [prompts/phase50-prompt.md](prompts/phase50-prompt.md)（未作成）

---

## Phase 51 — Minimap ⬜

**実装:** 右上ミニマップ RenderTexture or 簡易 ortho カメラ。TC / Barracks / Archery Range / Stable アイコン。

**プロンプト:** [prompts/phase51-prompt.md](prompts/phase51-prompt.md)（未作成）

---

## Phase 52 — Unit Animation ⬜

**制約:** Animator Controller は **Editor API 生成**（CONSTITUTION 準拠）

**対象:** Villager（Idle / Walk / Gather）、Militia / Archer（Idle / Walk / Attack）

**プロンプト:** [prompts/phase52-prompt.md](prompts/phase52-prompt.md)（未作成）

---

## Phase 53 — Combat VFX & Audio ⬜

**実装:** 矢弾トレイル、ヒットフラッシュ、攻撃 SE フック（AudioClip は Editor 生成 or 空参照）

**プロンプト:** [prompts/phase53-prompt.md](prompts/phase53-prompt.md)（未作成）

---

## M5 完了時の位置づけ

| 観点 | 見込み |
|------|--------|
| **AoE2 機能全体** | **約 50〜55%**（§IMPLEMENTATION_STATUS 投影 — M5 完了後セクション参照） |
| **コアループ（1v1 CPU）** | **約 85〜90%** |
| **マルチプレイ準備度** | **約 55〜60%**（View 分離まで。同期基盤は M6） |
| **憲法性能目標（800 体）** | **約 25〜35%**（Instancing / Pathfinding は未着手） |

---

## 進め方

1. M4 全 Phase 完了を確認
2. Phase 49 から順に — **1 Phase ごとに 1 パネル移行**（big bang 禁止）

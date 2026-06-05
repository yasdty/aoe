# AoE RTS Engine — Phase ロードマップ

[CONSTITUTION.md](../CONSTITUTION.md) に基づく、Phase 1〜10 の概要。  
**進め方:** MVP → 動作確認 → 次 Phase。Phase を飛ばさない。

| Phase | 内容 | シーン | 状態 |
|-------|------|--------|------|
| 1 | RTS カメラ、地面、ユニット 1 体、選択、移動 | `Phase1.unity` | ✅ 完了 |
| 2 | ドラッグ選択、複数選択、グループ移動 | `Phase2.unity` | ✅ 完了 |
| 3 | TownCenter、Villager 生産 | `Phase3.unity` | ✅ 完了 |
| 4 | 木材採集 | `Phase4.unity` | ✅ 完了 |
| 5 | House 建築 | `Phase5.unity` | ✅ 完了 |
| 6 | 人口システム | `Phase6.unity` | ✅ 完了 |
| 7 | Barracks、Militia | `Phase7.unity` | ✅ 完了 |
| 8 | 戦闘（死亡、HP 表示） | `Phase8.unity` | ✅ 完了 |
| 9 | CPU AI（経済） | `Phase9.unity` | ✅ 完了 |
| 10 | CPU 攻撃波、簡易 RTS 完成 | `Phase10.unity` | 📋 未実装 |

---

## 全体像

```
Phase 1–2  操作基盤（カメラ・選択・移動）
    ↓
Phase 3–6  経済・建築・人口（AoE 開始〜RTS 成立）
    ↓
Phase 7–8  軍事・戦闘
    ↓
Phase 9–10 CPU AI → 簡易 RTS 完成
```

**最終目標:** AoE2 ライクな economy / build order / 大規模戦闘を、Low-Spec（MacBook Air 16GB 級・4 チーム × 200 ユニット）で動かす RTS エンジン。

**共通制約:** Unity 6 / URP / New Input System / NavMesh 禁止 / Manager 更新方式 / Unity アセット手書き禁止（Editor API のみ）。

---

## Phase 1 — 操作基盤（最小プレイ環境）

**目的:** RTS の最低限のプレイ環境を作る。

### 実装内容

- **RTS カメラ:** WASD、マウス端スクロール、ホイールズーム（回転不要）
- **地面:** Plane 100×100 程度
- **ユニット 1 体:** Capsule、`UnitData`、HP 保持
- **選択:** 左クリック、選択時色変更
- **移動:** 地面右クリック → `MoveTarget` → 直線移動

### 主要システム

`RTSCameraController`, `Unit`, `UnitManager`, `SelectionManager`, `RTSInputReader`, `Phase1SceneBuilder`

### 範囲外

AI、建築、資源、NavMesh

### セットアップ

`AoE → Setup Phase1 Scene` → `Assets/Scenes/Phase1.unity`

---

## Phase 2 — 複数ユニット操作

**目的:** AoE らしい複数ユニット操作。

### 実装内容

- 左ドラッグ矩形選択（SelectionBox UI）
- 複数選択、Shift 追加選択
- グループ移動（右クリック → 3×3 / 4×4 等グリッド整列）
- `UnitManager` 拡張（全ユニット列挙）

### 範囲外

フォーメーション最適化、回避処理、建築・資源・AI

### セットアップ

`AoE → Setup Phase2 Scene` → `Assets/Scenes/Phase2.unity`

---

## Phase 3 — TownCenter と村人生産

**目的:** AoE 開始状態（市庁舎から村人を生産）。

### 実装内容

- **TownCenter:** 静止建築、Building レイヤー、選択可能
- **BuildingData**（ScriptableObject、Editor 生成）
- **生産キュー:** 1 スロット、タイマー（例: 5 秒）
- **ProductionManager:** 建築ごと Update 禁止、一括 Tick
- **UI:** TownCenter 選択時「Create Villager (Q)」+ 進捗表示
- 完成後 TownCenter 付近に Villager スポーン（既存 `Unit` / 選択・移動可）

### 範囲外

資源コスト、人口上限、House 建築、採集

### セットアップ（予定）

`AoE → Setup Phase3 Scene` → `Assets/Scenes/Phase3.unity`

---

## Phase 4 — 木材採集

**目的:** 経済開始（Wood のみ）。

### 実装内容

- **Tree / ResourceNode / GatherTask**
- 村人状態機械: Move → Gather → Carry → Deposit
- 右クリックで木へ移動、TownCenter へ運搬、Wood 増加

### 推奨サブステップ

| サブ | 内容 |
|------|------|
| 4-1 | Tree 配置 |
| 4-2 | Gather |
| 4-3 | Carry |
| 4-4 | Deposit |

### 範囲外

Food / Gold / Stone、LumberCamp 建築

### セットアップ

`AoE → Setup Phase4 Scene` → `Assets/Scenes/Phase4.unity`

---

## Phase 5 — House 建築

**目的:** 建築配置の開始。

### 実装内容

- **BuildingPlacement / GhostPreview / House**
- 流れ: House ボタン → 配置モード → クリック → 村人移動 → 建築開始
- Cost: Wood 25、BuildTime: 10 秒

### 範囲外

人口制限、文明差

### セットアップ

`AoE → Setup Phase5 Scene` → `Assets/Scenes/Phase5.unity`

---

## Phase 6 — 人口システム

**目的:** RTS としての基本成立（人口キャップ）。

**状態:** ✅ 完了

### 実装内容

- **PopulationManager** — 現在人口（`UnitManager` 集計）/ 上限（初期 5、House 完成 +5）
- HUD **Pop: N/M**、上限時 TC 生産拒否 + UI 無効

### 範囲外

House 破壊時の cap 減少、建築 Wood 返金・建築再開

### セットアップ

`AoE → Setup Phase6 Scene` → `Assets/Scenes/Phase6.unity`

---

## Phase 7 — Barracks と Militia

**目的:** 軍事の開始。

**状態:** ✅ 完了

### 実装内容

- **Barracks** 建築（50 Wood、5 秒）+ **Militia** 生産（20 Wood、3 秒）
- **AttackManager** — 右クリック攻撃、近接ダメージ（死亡は Phase 8）
- Phase7 シーンにテスト用 **Enemy Dummy** 1〜2 体

### 範囲外

死亡・HP バー、敵 AI・反撃、遠距離攻撃

### セットアップ

`AoE → Setup Phase7 Scene` → `Assets/Scenes/Phase7.unity`

---

## Phase 8 — 戦闘成立

**目的:** ユニット戦闘の完成。

**状態:** ✅ 完了

### 実装内容

- **死亡** — HP ≦ 0 → `Unit.Die()`（Destroy + Unregister）
- **UnitState** — Idle / Move / Attack / Dead
- **HP バー** — 選択ユニット（`UnitHpBarView`）
- 攻撃中ビジュアル、死亡時 Attack/Gather ジョブ解除

### 範囲外

敵 AI・反撃、Animator 本格実装

### セットアップ

`AoE → Setup Phase8 Scene` → `Assets/Scenes/Phase8.unity`

---

## Phase 9 — CPU AI（経済）

**目的:** AoE の核（CPU が経済を回す）。

**状態:** ✅ 完了

### 実装内容

- **チーム別経済** — Wood / Pop を `UnitTeam` 単位に分離（Player / CPU）
- **CPU 1 チーム**（`UnitTeam.Enemy`）: 木採集、House 建築、Villager 生産
- **`CpuEconomyAiManager`** — ルールベース経済 AI（Manager 方式）
- 人間プレイヤーと同一マップで共存（共有の木、CPU TC は反対側）

### 範囲外

軍事 AI（Barracks / Militia / 攻撃波 → Phase 10）

### セットアップ

`AoE → Setup Phase9 Scene` → `Assets/Scenes/Phase9.unity`

---

## Phase 10 — 簡易 RTS 完成

**目的:** AoE プロトタイプ完成。

**状態:** 📋 未実装

### 実装内容

- **CPU 軍事 AI** — Barracks 建築、Militia 生産（`UnitTeam.Enemy`）
- **CPU 攻撃波** — 例: 5 分毎に Player 方向へ Militia 攻撃
- **`CpuMilitaryAiManager`** + `Barracks` / `BarracksProductionManager` の Team 対応
- ゲームループ完成: **採集 → 建築 → 生産 → 戦闘**

### 範囲外

勝敗 UI、4 チーム、NavMesh / RVO、本格 HUD

### セットアップ

`AoE → Setup Phase10 Scene` → `Assets/Scenes/Phase10.unity`

---

## AI 開発の進め方

1. [CONSTITUTION.md](../CONSTITUTION.md) を読ませる
2. 既存ソース（`Assets/Scripts/`）を読ませる
3. [prompts/](prompts/) の該当 Phase プロンプトを渡す
4. 実装前設計 → 小さな diff → Play 確認 → 次 Phase

Phase 4 以降は **30〜60 分単位のサブステップ** に細分化すると、移動・採集の複雑さで途中が壊れにくい。

---

## プロンプト一覧

| Phase | ファイル | 備考 |
|-------|----------|------|
| 1 | [prompts/phase1-prompt.md](prompts/phase1-prompt.md) | 初回キックオフ（プロジェクト全体文脈含む） |
| 2 | [prompts/phase2-prompt.md](prompts/phase2-prompt.md) | |
| 3 | [prompts/phase3-prompt.md](prompts/phase3-prompt.md) | ✅ 完了 |
| 4 | [prompts/phase4-prompt.md](prompts/phase4-prompt.md) | ✅ 完了 |
| 5 | [prompts/phase5-prompt.md](prompts/phase5-prompt.md) | ✅ 完了 |
| 6 | [prompts/phase6-prompt.md](prompts/phase6-prompt.md) | ✅ 完了 |
| 7 | [prompts/phase7-prompt.md](prompts/phase7-prompt.md) | ✅ 完了 |
| 8 | [prompts/phase8-prompt.md](prompts/phase8-prompt.md) | ✅ 完了 |
| 9 | [prompts/phase9-prompt.md](prompts/phase9-prompt.md) | ✅ 完了 |
| 10 | [prompts/phase10-prompt.md](prompts/phase10-prompt.md) | |

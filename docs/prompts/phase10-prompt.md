# Phase 10 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜9 完了  
> **使い方:** `@CONSTITUTION.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 10 実装

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜9 は完了済み。Phase 10 のみ実装すること。**

---

## ① プロジェクト憲法（必読・遵守）

リポジトリの `CONSTITUTION.md` を読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止 / クリック時以外の Raycast 乱用禁止
- Manager 更新方式を維持
- **Unity アセット手書き禁止**: Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- Setup メニューは **Edit モード専用**
- **OnGUI は左上原点、Input System Pointer は左下原点** — `GameUiInput.GuiRectToScreenRect`
- **`GetInstanceID()` 禁止** — スロット等は `Unit.StandSlot` 等の自前 ID を使う

---

## ② Phase 1〜9 完了状態（現状）

動作確認済み（Phase9.unity）:

- **プレイヤー** — 採集・House・TC 生産・Barracks・Militia・近接攻撃・死亡・HP バー
- **チーム別経済** — `ResourceManager` / `PopulationManager` が `UnitTeam` 対応（Player API 後方互換）
- **CPU 経済 AI** — `CpuEconomyAiManager`（`UnitTeam.Enemy`）
  - 木採集、House 建築（Pop 上限時）、Villager 生産（目標 **6 体**）
  - 右上 **CPU Wood / CPU Pop**（`CpuHudView`）
- **CPU TC** — マップ南側 `(0, 0, -35)`、初期 Villager 3 体
- **Player TC** — 中央 `(0, 0, 0)`
- **共有の木**、CPU 建築 API（`TryStartTeamConstruction` / `TryFindPlacementNear`）
- **立ち位置オフセット** — `UnitPositionOffsets`（採集・搬入・建築・TC スポーン）

### Phase 9 から Phase 10 以降へ回す既知課題（今回必須ではない）

- HP バー OnGUI の微調整（下端見切れ等）— **本格 UI は範囲外**
- ユニット移動中の物理回避・押し合い（Phase 2 範囲外）
- 建築 Wood 返金、House 破壊時 cap 減少
- CPU 複数 House / Villager 上限の拡張経済
- 勝敗画面・リプレイ・セーブ
- 4 チーム、Food / Gold、NavMesh / RVO

主要ファイル（**実装前に必ず開いて読む**）:

- `Assets/Scripts/AI/CpuEconomyAiManager.cs`
- `Assets/Scripts/Buildings/Barracks.cs` / `BarracksProductionManager.cs`
- `Assets/Scripts/Buildings/BuildingPlacementManager.cs`
- `Assets/Scripts/Combat/AttackManager.cs`
- `Assets/Scripts/Economy/ResourceManager.cs` / `PopulationManager.cs`
- `Assets/Scripts/Units/Unit.cs` / `UnitTeam.cs`
- `Assets/Scripts/Editor/Phase9SceneBuilder.cs`
- `Assets/Scenes/Phase9.unity`

---

## ③ Phase 10 目的

**簡易 RTS 完成** — CPU が軍事を整え、定期的にプレイヤーへ攻撃波を送る。  
ゲームループ **採集 → 建築 → 生産 → 戦闘** が一連で回るプロトタイプ完成。

### 今回実装するもの

1. **CPU 軍事 AI** — Barracks 建築 + Militia 生産（`UnitTeam.Enemy`）
2. **CPU 攻撃波** — 例: **5 分毎**（300 秒）に Militia 群が Player ユニット / TC 方向へ攻撃
3. **`Barracks` / `BarracksProductionManager` の Team 対応** — Phase 9 と同様に CPU Wood / Pop を参照
4. **`CpuMilitaryAiManager`**（または `CpuEconomyAiManager` 拡張 — **小 diff 優先**）
5. **Phase10 シーン** — `AoE → Setup Phase10 Scene` → `Assets/Scenes/Phase10.unity`
6. **README / `docs/01_M0_POC_PHASES.md` 更新** — Phase 10 完了、最終プロトタイプ到達点を明記

### CPU 軍事 AI（MVP ルール）

**評価間隔:** 経済 AI と同様 **2 秒**ごと（毎フレーム不要）

```
前提: CPU Wood / Pop に余裕がある（経済 AI が回っている）

1. CPU Barracks が未建設 かつ Wood ≧ 50 かつ Pop 余裕
   → CPU TC 近くに Barracks 配置（Villager 1 体 builder、既存 API 再利用）
2. CPU Barracks 完成 かつ Militia 数 < 目標（例: 8 体） かつ Wood ≧ 20 かつ Pop 余裕
   → Militia 生産キュー
3. 攻撃波タイマー満了 かつ CPU Militia ≧ 3 体
   → 全 CPU Militia（または idle な Militia）に Player 側ターゲットへ AttackManager.IssueAttack
```

- **Barracks 配置** — House と同様 `TryFindPlacementNear` + `TryStartTeamConstruction`
- **攻撃ターゲット優先度（MVP）** — 最寄りの **Player ユニット** → なければ **Player TC** 付近の代表点（ユニットがいなくても Militia を前進させる簡易手段で可）
- **攻撃波間隔** — `[SerializeField] float attackWaveIntervalSeconds = 300f`（Inspector / SceneBuilder で調整可）
- **初回攻撃** — Play 開始 **5 分後**（即ラッシュは Phase 10 範囲外）

### 攻撃波の挙動（MVP）

- `AttackManager.IssueAttack(cpuMilitiaList, targetUnit)` を使用（プレイヤー右クリック攻撃と同じ経路）
- ターゲット死亡後は AttackManager 既存ロジックでジョブ解除
- CPU Militia は **プレイヤー選択不可**（既存 Team フィルタ）
- プレイヤーは Militia で反撃可能（Phase 7〜8 機能）

### Team / 経済の拡張（Phase 9 パターン踏襲）

| システム | Phase 9 状態 | Phase 10 で必要 |
|---------|-------------|----------------|
| `BarracksProductionManager` | Player Wood / Pop 固定 | **Barracks の Team** で Wood / Pop / Spawn |
| `Barracks` | Team 属性なし | **`UnitTeam` 追加**（CPU 色分け） |
| `SelectionManager` | CPU TC 選択不可 | **CPU Barracks 選択不可** |
| `ResourceManager.TrySpendWood` | Team 対応済 | Barracks 生産で `TrySpendWood(team, ...)` |
| Militia Spawn | Player 固定 | **`barracks.Team`** |

### 禁止（Phase 10 範囲外）

- 勝敗 UI・ゲームオーバー画面（Console ログ or 簡易 OnGUI 1 行通知程度は可）
- CPU 経済 AI の rewrite（**軍事ルール追加は OK**、経済ロジックは壊さない）
- 遠距離・攻城・複数 CPU チーム
- NavMesh / 経路探索ライブラリ
- 本格 HP バー / HUD リデザイン
- Phase 1〜9 シーンの破壊的変更

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 10-1 | `Barracks` に `UnitTeam` + ビジュアル（TownCenter と同パターン） |
| 10-2 | `BarracksProductionManager` — Team 別 Wood / Pop / Spawn |
| 10-3 | `BuildingPlacementManager` — Barracks 完成時 Team 整合（必要なら） |
| 10-4 | `SelectionManager` — CPU Barracks 選択不可 |
| 10-5 | `CpuMilitaryAiManager` — Barracks 建築 + Militia 生産ルール |
| 10-6 | 攻撃波タイマー + `IssueAttack` 一括命令 |
| 10-7 | `Phase10SceneBuilder`（Phase9 ベース）+ README / PHASES 更新 |

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更ファイル一覧**（新規 / 変更 / 削除）
2. **CpuEconomyAiManager との分担**（別 Manager か統合か）
3. **攻撃波ターゲット選定ロジック**
4. **後方互換** — Phase 1〜9 シーンが壊れないか

---

## ⑥ 実装後に必ず出力

1. **テスト手順**（チェックリスト 1〜12 程度）
2. **想定動作**（5 分後の攻撃波、CPU Barracks / Militia 増加）
3. **残課題**（UI  polish、勝敗、4 チーム等）
4. Unity メニュー手順（`AoE → Setup Phase10 Scene`）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### BarracksProductionManager 拡張例

```csharp
// CanTrainUnit(barracks.Team)
// TrySpendWood(barracks.Team, woodCost)
// UnitSpawner.Spawn(..., barracks.Team)
```

### 攻撃波

```csharp
// CpuMilitaryAiManager
float waveTimer;
void Update() {
  waveTimer -= Time.deltaTime;
  if (waveTimer > 0f) return;
  waveTimer = attackWaveIntervalSeconds;
  LaunchAttackWave();
}
```

- Player ユニット列挙: `UnitManager.CopyUnitsTo` + `Team == Player` + `CanAttack` または全 Player Unit
- Militia 列挙: `Team == Enemy` + `CanAttack`
- TC をターゲットにできない場合: Player TC 位置へ `SetMoveTarget` だけでも可（MVP）

### Phase10SceneBuilder

- `Phase9SceneBuilder` をコピー拡張
- `CpuMilitaryAiManager` を Systems に追加
- テスト用 **Player 初期 Villager 0**、CPU 経済は Phase 9 同様
- 攻撃波確認用に `attackWaveIntervalSeconds` を SceneBuilder から **60 秒**等に短縮する **開発用オプション** は README に明記しても可（デフォルト 300 秒）

### CpuEconomyAiManager との関係

- **推奨:** `CpuMilitaryAiManager` を別 MonoBehaviour に分離（経済 Tick と軍事 Tick を独立）
- 同一 GameObject でも可。`Update` 乱立は Manager 2 つ程度なら許容

---

## ⑧ シーン / Editor

- `Assets/Scenes/Phase10.unity` を新規作成
- Phase 1〜9 シーン・メニューは壊さない
- 初回 or ピンク地面時: `AoE → Fix Render Pipeline`

---

## ⑨ 完了条件（Phase 10 MVP — 簡易 RTS 完成）

- [ ] **Player 操作** — Phase 9 までの機能が壊れていない
- [ ] CPU が **Barracks を建築**できる（Wood 50、Villager builder）
- [ ] CPU Barracks から **Militia が生産**される（Enemy チーム、青系）
- [ ] Play **約 5 分**（または README 記載の短縮テスト間隔）後、**CPU Militia が Player 方向へ攻撃**する
- [ ] プレイヤー Militia で **CPU Militia と戦闘**できる（死亡・HP バー動作）
- [ ] CPU Wood / Pop / 経済 AI が **軍事 AI と共存**する（経済が止まらない）
- [ ] CPU Barracks / Militia は **プレイヤー選択不可**
- [ ] Console にエラーなし
- [ ] **ゲームループ** 採集 → 建築 → 生産 → 戦闘 が Phase10 シーンで一連体験できる

Phase 10 のみ実装。Phase 10 以降の拡張（4 チーム・大規模最適化等）に触れない。

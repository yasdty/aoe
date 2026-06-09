# Phase 36 実行プロンプト

> **状態:** ✅ 完了（M3 先頭）  
> **前提:** Phase 1〜35 完了（M2.7 Sandbox 含む）  
> **マイルストン:** M3 Military — **Archery Range + Archer**  
> **ロードマップ:** [07_M3_MILITARY_PHASES.md](../07_M3_MILITARY_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 36 実装（Archery Range + Archer）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 36 のみ実装。** Barracks / Militia パターンを **複製・拡張**（rewrite 禁止）。

---

## ① 目的

AoE2 準拠で **Archery Range 建築**から **Archer 遠距離ユニット**を生産・戦闘させる。

| 項目 | MVP |
|------|-----|
| 建築 | Archery Range（村民建築・配置ゴースト） |
| 生産 | Q キー + 生産キュー（Barracks と同型 FIFO 15） |
| 戦闘 | 射程内なら攻撃。弾丸は **即時ヒット MVP** 可（飛翔体は Phase 53 へ） |
| Rally | Phase 33 パターン — Range 選択 + 右クリック |
| CPU | 既存 Barracks 後に Range 建設・Archer 生産（単純ルール） |

**やらないこと:** Spearman / Stable / 時代昇格 / 相性表本格化 / Animator

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| Barracks 建築 | `Barracks.cs`, `BarracksProductionManager.cs`, `PlacedBuildingData`, `PlacedBuildingKind` |
| 配置 | `BuildingPlacementManager.cs` — `EnterBarracksPlacementMode` |
| Editor Data | `Phase1SceneBuilder.EnsureBarracksData`, `EnsureMilitiaData` |
| 戦闘 | `AttackManager.cs` — 近接ジョブのみ現状 |
| 選択・生産 UI | `SelectionManager.cs`, `BarracksPanelView.cs`, `ResourceHudView.cs` |
| Rally | `SetRallyPointCommand`, `ProductionRallyApplier` |
| CPU 軍事 | `CpuMilitaryAiManager.cs` |
| Scene | `Phase10SceneBuilder.cs` |

---

## ③ 実装タスク（small diff）

### 1. Data

- `PlacedBuildingKind` に `ArcheryRange` 追加
- `Phase1SceneBuilder.EnsureArcheryRangeData(UnitData archerData)` — Editor API で ScriptableObject 生成  
  - 目安: Wood 150 / 建築 40s / HP 300 / train Food+Wood コストは Militia より高め（例: 25 Wood + 25 Food）
- `Phase1SceneBuilder.EnsureArcherData()` — 遠距離用 `UnitData`  
  - 目安: HP 30 / 攻撃 4 / Pierce 系は Phase 39 まで単一 armor / **attackRange 5〜7** / attackCooldown 2s / 移動速度 Militia 同等

### 2. 建築・生産

- `ArcheryRange.cs` — `Barracks.cs` をベースにリネーム・最小差分
- `ArcheryRangeProductionManager.cs` — `BarracksProductionManager` 同型（別 Manager で可。共通化は Phase 36 では不要）
- `BuildingPlacementManager.EnterArcheryRangePlacementMode`
- `RuntimeBuildingFactory` / `BuildingPool` — ArcheryRange スポーン対応（既存パターン踏襲）

### 3. 戦闘（遠距離）

- `AttackManager` — `UnitData.attackRange > 0` で遠距離分岐  
  - 射程内: クールダウン後ダメージ適用（即時）  
  - 射程外: 攻撃接近移動（既存近接接近ロジック再利用）
- 右クリック攻撃命令は既存 `AttackCommand` 経由で動くこと

### 4. 選択・UI・入力

- `SelectionManager` — ArcheryRange クリック選択、右クリック Rally
- `ArcheryRangePanelView` — OnGUI MVP（`BarracksPanelView` 同型、Q 生産）
- `ResourceHudView` — 建築ボタン追加（Archery Range 配置モード）
- 生産ホットキー Q — Range 選択時は Archer キュー（TC/Barracks と競合しないよう Selection 状態で分岐）

### 5. Rally / キュー / Idle

- `ProductionRallyApplier` — Archer スポーン後 Rally 適用
- 生産キューは `ProductionManager` / Barracks パターンと同じ Spend-on-enqueue
- `UnitIdleTracker` — Archer の Attack 中は Idle 除外（既存 Militia 同様）

### 6. CPU

- `CpuMilitaryAiManager` — Barracks 建設後、Wood 余裕時に Archery Range 建設  
  - Archer 目標数（例: 4）まで Q 相当キュー  
  - 攻撃波に Archer を含める（Militia と混在で可）

### 7. Phase10 SceneBuilder

- `EnsureArcheryRangeData` / `EnsureArcherData` 呼び出し
- Managers / Panel / Placement 配線
- **シーン再生成:** `AoE → Setup Phase10 Scene`

---

## ④ 制約

- rewrite 禁止 / small diff only
- Unity アセット手書き禁止 — `.animator` / prefab は Editor API
- `.meta` は 32 文字 GUID
- Militia / Barracks 既存挙動を壊さない

---

## ⑤ Play 確認

1. `AoE → Setup Phase10 Scene` → Play
2. Archery Range 建築 → Q で Archer 複数キュー → スポーン
3. Archer 右クリック敵ユニット → **射程外は接近、射程内で攻撃**
4. Rally 右クリック → スポーン後移動
5. CPU が Range + Archer を生産（ログ or 観察）
6. Phase 31〜35 回帰（キュー / Idle / Control Group / 広いマップ）

---

## ⑥ 完了時ドキュメント

- [x] `IMPLEMENTATION_STATUS.md` — Phase 36 ✅
- [x] `07_M3_MILITARY_PHASES.md` — Phase 36 ✅
- [x] 本プロンプト — ✅

---

Phase 36 のみ。**Phase 37 Spearman には触れない。**

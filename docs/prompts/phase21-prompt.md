# Phase 21 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜20 完了（PoC + Foundation + M2 Economy）  
> **マイルストン:** M2.5 Economy Polish **開始 Phase**  
> **ロードマップ:** [04_M2_5_ECONOMY_POLISH_PHASES.md](../04_M2_5_ECONOMY_POLISH_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 21 実装（Gather Repeat）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜20 は完了済み。Phase 21 のみ実装すること。**

---

## ① M2.5 Economy Polish 方針（必読・遵守）

M2（Phase 17〜20）で 4 資源の採集は成立したが、**搬入 1 回でジョブ終了**しており AoE2 の「村民 1 命令 = 長期作業」と異なる。Phase 21 は **採取リピートのみ**。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | 搬入後の採取ループ復帰のみ |
| **small diff** | 3 GatherManager の **Deposit 完了処理** を中心に変更。rewrite / 統合リファクタ禁止 |
| **既存パターン維持** | ジョブ構造・Command 種類・Tick 登録はそのまま |
| **既存ゲームを壊さない** | Wood / Food / Farm / Gold / Stone / Lumber Camp / 建築・生産・CPU + Foundation 全機能 |
| **Foundation 維持** | Command Queue / Fixed Tick 20 TPS / Pool / Spatial Hash |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止
- Manager 更新方式を維持
- Setup メニューは **Edit モード専用**（本 Phase で Scene 変更は **不要**）
- Tick ループ中の **リスト一括削除禁止**（Phase 18 Farm 枯渇バグ参照）

---

## ② Phase 20 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **Wood** — `GatherManager` → TC / Lumber Camp 搬入 → **ジョブ削除で停止**
- **Food** — Berry / Farm → `FoodGatherManager` → TC 搬入 → **ジョブ削除で停止**
- **Gold / Stone** — `MineralGatherManager` → TC 搬入 → **ジョブ削除で停止**
- **Command Queue** — `GatherCommand` / `GatherFoodCommand` / `GatherFarmFoodCommand` / `GatherGoldCommand` / `GatherStoneCommand`
- **CPU** — 木採集のみ（Phase 28 で 4 資源対応予定）

### 現状のギャップ（Phase 21 で解消）

| 項目 | 現状 |
|------|------|
| 採取リピート | **なし** — 全 Manager が `TickMoveToDeposit` 完了時に `RemoveAt` |
| AoE2 村民 UX | 右クリックのたびに再命令が必要 |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| Wood 採集 | `Assets/Scripts/Economy/GatherManager.cs` — `TickMoveToDeposit`（約 L211） |
| Food 採集 | `Assets/Scripts/Economy/FoodGatherManager.cs` — Berry `TickMoveToDeposit` / Farm `TickFarmMoveToDeposit` |
| Mineral 採集 | `Assets/Scripts/Economy/MineralGatherManager.cs` — Gold/Stone `TickMove*ToDeposit` |
| 命令 Cancel | `Assets/Scripts/Commands/GameCommands.cs` |
| ロードマップ | `docs/04_M2_5_ECONOMY_POLISH_PHASES.md` |

---

## ③ Phase 21 目的

**搬入完了後、同じ採取対象へ自動復帰** — AoE2 と同様、村民は枯渇・中断・死亡まで採取を継続する。

### 変更後の採取ループ（全資源共通）

```
右クリック Gather*Command（1 回）
    ↓
MoveToResource → Gather → MoveToDeposit → AddResource
    ↓
対象が有効なら MoveToResource へ（リピート）
    ↓
枯渇 / 新命令 / 死亡 / Game Over でジョブ終了
```

Wood / Food / Gold / Stone **すべて**同じ振る舞いにする。

### 今回実装するもの

1. **`GatherManager.TickMoveToDeposit`** — Wood 搬入後、`job.tree` が未枯渇なら `job.state = MoveToTree`, `carriedWood = 0`, 採取位置へ再移動
2. **`FoodGatherManager.TickMoveToDeposit`** — Berry 搬入後、同 Bush へ復帰
3. **`FoodGatherManager.TickFarmMoveToDeposit`** — Farm 搬入後、`IsFarmGatherTargetValid(farm)` なら `MoveToFarm` へ復帰
4. **`MineralGatherManager.TickMoveGoldToDeposit` / `TickMoveStoneToDeposit`** — 搬入後、Mine 未枯渇なら `MoveToMine` へ復帰

### ジョブ終了条件（リピートしない場合）

| 条件 | 動作 |
|------|------|
| Tree / Berry / Farm / Mine **枯渇** | 搬入済みなら終了。搬行前なら Deposit へ（既存） |
| **新 Gather 命令** | 既存 `RemoveJobForUnit` / `Issue*` が上書き（変更不要） |
| **Move / Attack 命令** | 既存 Cancel がジョブ解除（変更不要） |
| **Unit 死亡** | 既存 Tick 先頭チェック（変更不要） |
| **Deposit 先なし**（TC 破壊等） | 既存どおりジョブ削除 |
| **carried == 0 で Deposit** | 既存どおりジョブ削除 |

### 禁止（Phase 21 範囲外）

- Farm 1 人制限（**Phase 22**）
- Mining Camp / Mill / 狩り（**Phase 23〜26**）
- Militia Aggro（**Phase 27**）
- CPU 4 資源 AI（**Phase 28**）
- GatherManager 3 つを 1 クラスに統合
- 新 Command 種類の追加（既存 Command で十分）
- 新シーン（**検証は `Phase10.unity` のみ**）
- `Phase10SceneBuilder` 変更（本 Phase 不要）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 21-1 | `GatherManager` — Wood リピート + Play 確認 |
| 21-2 | `FoodGatherManager` — Berry リピート |
| 21-3 | `FoodGatherManager` — Farm リピート（枯渇 Tick 安全） |
| 21-4 | `MineralGatherManager` — Gold / Stone リピート |
| 21-5 | 全資源回帰 + ドキュメント更新 |

各サブステップ後に **他資源採集が壊れていないこと** を確認すること。

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**（新規 / 変更 / 削除）
2. **4 Manager それぞれのリピート復帰ロジック**（疑似コード）
3. **Farm 枯渇時の Tick 安全**（Phase 18 バグ再発防止）
4. **影響範囲**（CPU 木採集 / Command Cancel）
5. **リスク**（Deposit 直後の index ずれ / 枯渇ノードへの復帰）
6. **ロールバック方法**
7. **完了条件**（下記チェックリスト）
8. **テスト手順**

---

## ⑥ 実装後に必ず出力

1. **変更内容サマリ**
2. **変更ファイル一覧**
3. **テスト結果**（資源ごとのリピート確認）
4. **既知の制限**（Farm 複数人可 / CPU 4 資源未対応 等 — Phase 22/28）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### Wood — `GatherManager.TickMoveToDeposit`（例）

```csharp
if (job.unit.IsNear(depositPosition, DepositReachDistance))
{
    ResourceManager.AddWood(job.unit.Team, job.carriedWood);
    job.carriedWood = 0f;

    if (job.tree != null && !job.tree.IsDepleted)
    {
        job.state = GatherState.MoveToTree;
        job.unit.SetMoveTarget(GetGatherPosition(job.tree, job.unit));
        jobs[index] = job;
        return;
    }

    job.unit.ClearMoveTarget();
    jobs.RemoveAt(index);
    return;
}
```

### Berry — `FoodGatherManager.TickMoveToDeposit`

- Wood と同パターン。`job.bush != null && !job.bush.IsDepleted` で `MoveToBush` へ

### Farm — `FoodGatherManager.TickFarmMoveToDeposit`

- `IsFarmGatherTargetValid(job.farm)` で `FarmGatherState.MoveToFarm` へ
- Farm 枯渇は `TakeFood` 側で処理 — **RemoveAt 中に別 Tick が走らないよう** 既存インデックスガードを維持

### Gold / Stone — `MineralGatherManager`

- Gold: `TickMoveGoldToDeposit` — `job.mine != null && !job.mine.IsDepleted` → `MoveToMine`
- Stone: `TickMoveStoneToDeposit` — 同左

### ヘルパー抽出について

- 4 ファイルに同型コードが並ぶのは **許容**（small diff / rewrite 禁止）
- 共通化リファクタは **Phase 21 ではしない**

### CPU 経済 AI

- `CpuEconomyAiManager` が `GatherManager.IssueGatherCommand` を呼ぶ既存フローは **そのままリピートの恩恵を受ける**
- CPU 側のコード変更は **不要**（任意スモーク確認のみ）

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`（主）
- **Setup 再実行:** 不要（ロジックのみ）
- Phase 1〜20 Setup メニューは壊さない

---

## ⑨ 完了条件（Phase 21 MVP）

- [ ] **Wood** — 木 1 回右クリック → 複数往復搬入 → 木枯渇まで継続
- [ ] **Berry** — 同 Bush へ複数往復
- [ ] **Farm** — 同 Farm へ複数往復 → 枯渇で Farm 消滅 + ジョブ終了
- [ ] **Gold / Stone** — 同 Mine へ複数往復 → 枯渇で停止
- [ ] 搬入後 **再右クリックなし** で採取継続
- [ ] 採集中 **地面右クリック** → キャンセル + 移動（既存）
- [ ] **Lumber Camp** Wood 搬入リピート（最寄り Drop-off 維持）
- [ ] **Command Queue** 経由は従来どおり（新 Command 不要）
- [ ] Console に **Null 参照・例外なし**
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `04_M2_5_ECONOMY_POLISH_PHASES.md` Phase 21 を ✅

### Victory 確認について

M2.5 Economy Phase では **毎回 Victory まで確認不要**。Phase 21 完了時は以下で十分:

- 各資源のリピート主パス
- Move キャンセル
- Console エラーなし

---

## ⑩ テスト手順（Play チェックリスト）

1. `Phase10.unity` → Play
2. 村民 → **木 右クリック 1 回** → TC/Lumber Camp へ **2 往復以上** 搬入（再右クリック不要）
3. 村民 → **Berry 右クリック 1 回** → 複数往復 → Bush 枯渇で停止
4. **Farm 建築** → Farm 右クリック 1 回 → 複数往復 → Farm 枯渇で停止
5. 村民 → **Gold / Stone 右クリック** → 複数往復
6. 採取中 **地面右クリック** → 移動し採取停止
7. **Build Lumber Camp** → 森近く Wood リピート（回帰）
8. Militia 生産・CPU 攻撃波スモーク
9. Console エラーなし

Phase 21 のみ実装。**Phase 22 以降（M2.5）** に触れない。

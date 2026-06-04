# Phase 8 実行プロンプト

> **状態:** 📋 未実行（プロンプト作成済み）  
> **前提:** Phase 1〜7 完了  
> **使い方:** `@CONSTITUTION.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 8 実装

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜7 は完了済み。Phase 8 のみ実装すること。**

---

## ① プロジェクト憲法（必読・遵守）

リポジトリの `CONSTITUTION.md` を読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止 / クリック時以外の Raycast 乱用禁止
- Manager 更新方式を維持（`UnitManager` / `AttackManager` / `GatherManager` 等）
- **Unity アセット手書き禁止**: Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- Setup メニューは **Edit モード専用**
- **OnGUI は左上原点、Input System Pointer は左下原点** — `GameUiInput.GuiRectToScreenRect`

---

## ② Phase 1〜7 完了状態（現状）

動作確認済み（Phase7.unity）:

- 経済・建築・人口（Phase 3〜6）— Wood 採集、House、Pop 上限
- **Barracks** 建築（50 Wood、5 秒）+ **Militia** 生産（20 Wood、3 秒）
- **AttackManager** — Militia 右クリック → 敵へ接近 → 近接攻撃（1 秒 CD）
- ダメージ式: `max(1, attack - targetArmor)`、`Unit.TakeDamage` で HP 減少
- **HP 0 でもユニットは消えない**（Phase 7 仕様。Phase 8 で死亡を実装）
- **UnitTeam** — Player / Enemy。自軍のみ選択可（`SelectionManager.IsPlayerUnit`）
- 敵 Dummy 1〜2 体（反撃なし、AI なし）
- Console にダメージログ出力

**Phase 7 から Phase 8 以降へ回す既知課題（今回必須ではない）:**

- 建築中断時の Wood 返金・建築再開
- Villager スポーン重なり
- House 破壊時の cap 減少
- 敵 AI・反撃
- 遠距離攻撃
- 本格アニメーション（Animator アセット大量生成）

主要ファイル（**実装前に必ず開いて読む**）:

- `Assets/Scripts/Units/Unit.cs` / `UnitData.cs` / `UnitManager.cs` / `UnitTeam.cs`
- `Assets/Scripts/Combat/AttackManager.cs`
- `Assets/Scripts/Economy/PopulationManager.cs` / `GatherManager.cs`
- `Assets/Scripts/Selection/SelectionManager.cs`
- `Assets/Scripts/Editor/Phase7SceneBuilder.cs`
- `Assets/Scenes/Phase7.unity`

---

## ③ Phase 8 目的

**戦闘の完成** — HP 0 で死亡、人口から除外、最小 HP 表示、攻撃状態の整理。

### 今回実装するもの

1. **死亡処理** — HP ≦ 0 で `Unit` 破棄 + `UnitManager.Unregister`
2. **人口更新** — 死亡時 `PopulationManager` が正しく減る（`UnitManager.UnitCount` 連動）
3. **攻撃ターゲット整理** — 死亡ユニットへの `AttackManager` ジョブ解除
4. **UnitState（最小）** — Idle / Move / Attack / Dead（Dead は破棄前の短い状態でも可）
5. **HP 表示（最小）** — 選択時 or 常時 OnGUI バー（World Space 不要。選択ユニットのみで可）
6. **攻撃中の見た目（最小）** — 色変更・スケールパルス等（Animator 不要）
7. **Phase8 シーン** — `AoE → Setup Phase8 Scene` → `Assets/Scenes/Phase8.unity`

### ルール（MVP）

| 項目 | 値 |
|------|-----|
| 死亡条件 | `CurrentHp <= 0` |
| 死亡後 | GameObject Destroy、選択解除、攻撃/Gather ジョブ解除 |
| 人口 | 死亡で CurrentPopulation 減少（Unregister ベース） |
| HP バー | 選択ユニット上方 or 画面下部パネル（OnGUI 最小） |
| UnitState | 移動中=Move、攻撃ジョブ中=Attack、それ以外=Idle |
| 敵死亡 | 同様に Destroy（テスト Dummy も消える） |

- **Dead 状態** — Destroy 直前にセットするか、即 Destroy で可（実装者判断、一貫性を README に明記）
- **Villager 死亡** — 採集中なら Gather ジョブも解除
- **Militia 死亡** — Attack ジョブ解除（攻撃者・被攻撃者双方）
- **リスポーン / 墓石** — Phase 8 範囲外

### 攻撃・移動との連携

```
TakeDamage → HP <= 0 → Die()
  ↓
AttackManager.CancelJobsForUnit(dead)
GatherManager.CancelForUnit(dead)
SelectionManager から選択解除
UnitManager.Unregister → Destroy(gameObject)
```

- 攻撃者のターゲットが死んだら、その AttackJob を削除
- HP 0 のユニットは攻撃・移動・選択不可

### UI（MVP）

- **選択ユニット HP バー** — `HP: 32/40` テキスト + 簡易バー（OnGUI）
- またはユニット選択時のみ画面下部に表示（TC / Barracks パネルと競合しない位置）
- HP バーは **3D 頭上 UI 不要**（Phase 8 MVP）

### 禁止（Phase 8 範囲外）

- 敵 AI・反撃・攻撃波
- 遠距離・攻城
- 経済拡張（Food / Gold）
- `AttackManager` / `UnitManager` / `GatherManager` の rewrite（**拡張・小 diff**）
- Animator Controller 手書き / prefab 手書き

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 8-1 | `UnitState` enum + `Unit` に状態プロパティ（Idle/Move/Attack/Dead） |
| 8-2 | `Unit.Die()` — Unregister、ジョブ解除、Destroy |
| 8-3 | `TakeDamage` → HP ≦ 0 で `Die()` |
| 8-4 | `AttackManager` — ターゲット/攻撃者死亡時ジョブ削除 |
| 8-5 | `GatherManager` — 死亡ユニットの Gather 解除（既存 Cancel 拡張） |
| 8-6 | `SelectionManager` — 死亡ユニットを選択リストから除外 |
| 8-7 | `UnitHpBarView` または選択パネル拡張 — HP 表示 |
| 8-8 | 攻撃中の最小ビジュアル（色・MaterialPropertyBlock） |
| 8-9 | `Phase8SceneBuilder` + README / `docs/PHASES.md` 更新 |

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更ファイル一覧**（新規 / 変更 / 削除）
2. **影響範囲**（Units / Combat / Economy / Selection / UI / Editor）
3. **パフォーマンス影響**（Destroy 頻度、GC、OnGUI コスト）
4. **save / multiplayer 将来互換**（`UnitDiedEvent` 化しやすい API か）

---

## ⑥ 実装後に必ず出力

1. **テスト手順**（チェックリスト 1〜12 程度）
2. **想定動作**
3. **残課題**（Phase 9 へ回すもの）
4. Unity メニュー手順（`AoE → Setup Phase8 Scene`）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

- 死亡: `UnitManager` に `KillUnit(Unit)` を足すか、`Unit.Die()` 内で `Unregister` + `Destroy`（既存 Register パターンに合わせる）
- 人口: `PopulationManager.CurrentPopulation` は `UnitManager.UnitCount` 参照のため Unregister で自動減る — 追加ロジック不要のはず（要 Play 確認）
- AttackManager: ジョブループ内で `target.CurrentHp <= 0` チェック
- Move 状態: `HasMoveTarget` / AttackManager にジョブがあるかで判定
- HP バー: `SelectionManager.SelectedUnits` を OnGUI で走査
- Phase7 の `Phase7SceneBuilder` をコピー拡張して Phase8 用に
- 死亡後 Collider 残り防止 — Destroy で十分

---

## ⑧ シーン / Editor

- `Assets/Scenes/Phase8.unity` を新規作成（Phase7 と同配置で可）
- Phase1〜7 シーン・メニューは壊さない
- 初回 or ピンク地面時: `AoE → Fix Render Pipeline`

---

## ⑨ 完了条件（Phase 8 MVP）

- [ ] Militia が敵を攻撃し **HP 0 で敵が消える**
- [ ] 敵が消えた後、攻撃ジョブが残らず Console エラーなし
- [ ] **Pop** が死亡で減る（例: 5 体 → 1 体死亡 → Pop 4/5）
- [ ] 自軍ユニット死亡も同様（テスト用に Villager を攻撃させる必要はない — 任意）
- [ ] 選択ユニットに **HP 表示** がある
- [ ] 攻撃中であることが分かる最小フィードバック（色等）
- [ ] Phase 7 機能（Barracks、採集、House、TC 生産）が壊れていない
- [ ] Console にエラーなし

Phase 8 のみ実装。Phase 9 以降に触れない。

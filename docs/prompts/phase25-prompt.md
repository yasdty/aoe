# Phase 25 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜24 完了（PoC + Foundation + M2 Economy + M2.5 Phase 21〜24）  
> **マイルストン:** M2.5 Economy Polish  
> **ロードマップ:** [04_M2_5_ECONOMY_POLISH_PHASES.md](../04_M2_5_ECONOMY_POLISH_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 25 実装（Selection Info Panel）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜24 は完了済み。Phase 25 のみ実装すること。**

---

## ① M2.5 Economy Polish 方針（必読・遵守）

Phase 24 で静止 Deer/Sheep 狩りが完成。Phase 25 は **選択オブジェクトの詳細パネル + 資源ノード左クリック選択** のみ。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | AoE2 風 Info Panel（HP / 攻撃 / 資源残量） |
| **small diff** | 既存 OnGUI パターン（`ResourceHudView` / `UnitHpBarView`）を拡張 |
| **既存ゲームを壊さない** | 採取 / 戦闘 / 建築 / CPU + Foundation 全機能 |
| **Foundation 維持** | Command Queue / Fixed Tick 20 TPS / Pool / Spatial Hash |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- Setup メニューは **Edit モード専用**（本 Phase で SceneBuilder に View 追加のみ。フル Setup 再実行は **不要**）

---

## ② Phase 24 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **狩り** — Deer / Sheep 右クリック → TC 搬入 → Food 増加（リピート）
- **選択** — 左クリック: 自軍 Unit / Player TC / Player Barracks のみ
- **資源ノード** — 左クリック選択 **なし**（右クリック命令のみ）
- **HP バー** — `UnitHpBarView` が選択ユニット上部に HP バー（複数選択時）
- **詳細パネル** — **なし**（名前 / 攻撃 / 資源残量が見えない）

### 現状のギャップ（Phase 25 で解消）

| 項目 | 現状 |
|------|------|
| 左下 Info Panel | **未実装** |
| 資源残量表示 | Tree / Berry / Deer 等をクリックしても残量不明 |
| Militia 攻撃力 | データは `UnitData.attack` あるが UI 非表示 |
| 建物 HP 詳細 | TC/Barracks 選択時に数値パネルなし |

### Phase 24 から後回しにした項目（本 Phase では触らない）

| 項目 | 先送り先 |
|------|----------|
| 羊の無所属 → 所属 + 誘導 | **Phase 28** |
| Deer / Sheep 徘徊 | **Phase 28** |
| Boar 反撃狩り | **Phase 26** |
| Melee / Pierce 装甲 2 種 | **Phase 38** Counter System |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| 選択入口 | `Assets/Scripts/Selection/SelectionManager.cs` — `HandleClickSelect` |
| HP バー参考 | `Assets/Scripts/Selection/UnitHpBarView.cs` |
| HUD 参考 | `Assets/Scripts/Selection/ResourceHudView.cs` / `GameUiInput.cs` |
| ユニット stat | `Assets/Scripts/Units/Unit.cs` / `UnitData.cs` |
| 建物 HP | `Assets/Scripts/Buildings/BuildingHealth.cs` / `TownCenter.cs` / `Barracks.cs` |
| 資源残量 | `TreeResource` / `BerryBushResource` / `DeerResource` / `SheepResource` / `GoldMineResource` / `StoneMineResource` |
| レイヤー | `Assets/Scripts/Core/GameLayers.cs` — `ResourceMask` |
| SceneBuilder | `Assets/Scripts/Editor/Phase10SceneBuilder.cs` — HUD View 追加パターン |

---

## ③ Phase 25 目的

**AoE2 風の選択詳細パネル。** 左クリックでオブジェクトを選び、**左下** に名前・HP・攻撃・資源残量を表示。

### AoE2 参考（MVP スコープ）

- 村民: 名前 + HP（攻撃力は **非表示** — 非戦闘）
- 軍事ユニット: 名前 + HP + 攻撃力
- 装甲: **単一値** `armor` のみ。0 なら行を省略
- 資源: `Wood: 342` / `Food: 140` / `Gold: 800` 等
- 建物: 名前 + HP（装甲 0 なら省略）

---

## ④ 今回実装するもの

### 1. 選択状態の拡張

`SelectionManager` に **単体非ユニット選択** を追加（既存 `selectedUnits` / TC / Barracks と排他）:

- `ISelectionInfoTarget` インターフェース（案）— 表示名 + 行テキスト生成、または型別分岐
- 選択種別: **Resource ノード** / **追加建物**（Farm / House / LumberCamp / MiningCamp）
- **複数ユニット選択時** — 既存どおり `UnitHpBarView` 優先。Info Panel は **単体選択時のみ** 表示（複数 Unit 選択中は従来 HP バーのみで OK）

### 2. 左クリック — Resource レイヤー

`HandleClickSelect` に `GameLayers.ResourceMask` Raycast を追加（Building の後、地面クリアの前）:

| コンポーネント | 選択 |
|----------------|------|
| `TreeResource` | Wood 残量 |
| `BerryBushResource` | Food 残量 |
| `DeerResource` / `SheepResource` | Food 残量 |
| `GoldMineResource` / `StoneMineResource` | Gold / Stone 残量 |

**右クリック命令は変更しない。**

### 3. 左クリック — 建物拡張（Player 自軍のみ）

現状 TC / Barracks のみ選択可。以下も **左クリック単体選択** + Info 表示:

- `Farm` / `House` / `LumberCamp` / `MiningCamp`（`BuildingHealth` + `Team == Player`）

既存 TC / Barracks 選択・生産パネル（`ProductionPanelView` / `BarracksPanelView`）は **維持**。

### 4. `SelectionInfoPanelView`（新規 OnGUI）

- 配置: **画面左下**（`ResourceHudView` 右上と `UnitHpBarView` 下部中央と干渉しない）
- `SelectionManager` から現在の Info 対象を参照
- 表示例:

```
Villager
HP: 40 / 40
```

```
Militia
HP: 40 / 40
Attack: 4
```

```
Tree
Wood: 500
```

```
Berry Bush
Food: 250
```

```
Town Center
HP: 400 / 400
```

- 数値は **整数表示**（`Mathf.FloorToInt` または `:0`）
- 枯渇資源: 残量 0 + 灰色化済みオブジェクトも選択可

### 5. SceneBuilder 配線

- `Phase10SceneBuilder`（および必要なら `Phase1SceneBuilder` パターン）に `SelectionInfoPanelView` を `SelectionManager` オブジェクトへ AddComponent
- `[SerializeField] SelectionManager selectionManager` を SerializedObject で配線

---

## ⑤ 今回やらないこと

- 羊の無所属 / 誘導（**Phase 28**）
- 動物徘徊（**Phase 28**）
- Boar（**Phase 26**）
- Mill（**Phase 27**）
- uGUI / UI Toolkit 移行
- Melee / Pierce 装甲 2 種
- 敵ユニット / CPU 建物の選択
- ミニマップ・アイコン

---

## ⑥ 実装ステップ（推奨）

| Step | 内容 |
|------|------|
| 25-1 | `SelectionInfoPanelView` スケルトン + 単体 Unit 表示 |
| 25-2 | `SelectionManager` — Resource / 追加建物クリック選択 |
| 25-3 | 資源残量・建物 HP・Militia 攻撃表示 |
| 25-4 | Phase10SceneBuilder 配線 + ドキュメント更新 |

---

## ⑦ 技術メモ

### SelectionManager — クリック優先（案）

```
Unit（自軍）→ Building（自軍 TC/Barracks/Farm/House/Camp）→ Resource → クリア
```

既存 Unit 優先を維持。Resource は **左クリック専用**（右クリック Gather フローと独立）。

### 攻撃力表示ルール

```csharp
// Villager: UnitData.attack == 0 → Attack 行を出さない
// Militia: attack > 0 → Attack: N
if (unit.CanAttack)
    lines.Add($"Attack: {unit.AttackPower:0}");
```

### 装甲表示ルール

```csharp
if (armor > 0f)
    lines.Add($"Armor: {armor:0}");
```

### 複数選択との共存

- `selectedUnits.Count > 1` → Info Panel **非表示**（`UnitHpBarView` のみ）
- `selectedUnits.Count == 1` → Info Panel + HP バー **両方 OK**（重ならない Y 配置）

### GameUiInput

HUD ヒット判定に Info Panel 矩形を **含めない**（ワールドクリックをブロックしない）。Panel は表示専用（ボタンなし）なので `ResourceHudView.IsPointerOverHud` 拡張は **不要**（Panel 上クリックは地面に抜けてよい MVP）。

---

## ⑧ シーン / Editor

- **検証シーン:** `Phase10.unity`
- **Setup 再実行:** **不要** — Edit モードで SceneBuilder 配線追加後、既存シーンを開いて Play
- 手動: 既存 `Phase10.unity` に `SelectionInfoPanelView` を AddComponent しても可

---

## ⑨ 完了条件（Phase 25 MVP）

- [ ] **Villager** 左クリック → 左下に名前 + HP（攻撃力なし）
- [ ] **Militia** 左クリック → 名前 + HP + Attack
- [ ] **Tree** 左クリック → `Wood: N`
- [ ] **Berry / Deer / Sheep** → `Food: N`
- [ ] **Gold / Stone Mine** → `Gold: N` / `Stone: N`
- [ ] **TC / Barracks / Farm / House / Camp** → 名前 + HP
- [ ] 複数ユニット矩形選択 → 従来どおり（Info Panel は非表示で OK）
- [ ] 右クリック Gather / Attack / Move 回帰
- [ ] Console エラーなし
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `04_M2_5` Phase 25 を ✅

---

## ⑩ テスト手順（Play チェックリスト）

1. `Phase10.unity` → Play
2. **Villager** 左クリック → Info Panel に HP
3. **Militia** 左クリック → HP + Attack
4. **Tree / Berry / Deer / Sheep / Mine** 左クリック → 残量表示
5. **TC** 左クリック → HP + 生産パネル（既存）共存
6. 村民 → 木右クリック採取 / 動物狩り / 移動 — 回帰
7. 矩形で複数村民選択 → HP バーのみ（Info Panel 非表示）
8. Console エラーなし

Phase 25 のみ実装。**Phase 26 以降（Boar 等）** に触れない。

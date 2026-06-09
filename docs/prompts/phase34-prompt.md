# Phase 34 実行プロンプト

> **状態:** ✅ 実装済み（Play 確認待ち）  
> **前提:** Phase 1〜33 完了（PoC + Foundation + M2 Economy + M2.5 + M2.6 Phase 31〜33）  
> **マイルストン:** M2.6 RTS UX（**最終 Phase**）  
> **ロードマップ:** [05_M2_6_RTS_UX_PHASES.md](../05_M2_6_RTS_UX_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 34 実装（Control Groups）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜33 は完了済み。Phase 34 のみ実装すること。**

---

## ① M2.6 RTS UX 方針（必読・遵守）

Phase 33 で Rally Point が完成。Phase 34 は **M2.6 最終** — **Ctrl+1〜9** でユニットグループ保存・呼び出し。

| 方針 | 内容 |
|------|------|
| **1 Phase = 1 目的** | 9 スロットの Control Group + ホットキー |
| **small diff** | **`ControlGroupManager` 新規** + `SelectionManager` 最小 API + Input — rewrite 禁止 |
| **既存パターン再利用** | `SelectionManager.SelectUnits` / `SelectSingleUnit` / Shift 追加選択 |
| **既存ゲームを壊さない** | Rally / 生産キュー / Idle UX / 採集 / Aggro / CPU / Victory |
| **Player のみ** | Control Group は **Player ユニット** のみ保存 |

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- **`.meta` は 32 文字 GUID**

---

## ② Phase 33 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **選択** — 単体 / 矩形 / Shift 追加選択（`SelectionManager`）
- **公開 API** — `SelectSingleUnit(Unit)` / `SelectUnits(IReadOnlyList<Unit>)`（置換選択）
- **Shift** — `RTSInputReader.IsShiftHeld` — 矩形・クリック追加選択で使用中
- **Ctrl** — **未実装** — Control Group 用の Ctrl 検知なし
- **数字キー** — Input アクション **なし**（1〜9 未バインド）
- **Control Group** — **なし**

### 現状のギャップ（Phase 34 で解消）

| 項目 | 現状 |
|------|------|
| Ctrl+1〜9 保存 | なし |
| 1〜9 呼び出し | なし |
| Shift+1〜9 追加選択 | なし |
| 死亡ユニット除去 | N/A |
| スロット表示 HUD | なし（任意） |

**実装前に必ず開いて読むファイル:**

| 領域 | ファイル |
|------|----------|
| 選択 | `Assets/Scripts/Selection/SelectionManager.cs` — `SelectUnits` / `ToggleUnitSelection` / `HandleUnitDied` |
| 入力 | `Assets/Scripts/Input/RTSInputReader.cs` / `RTSInputActionsBuilder.cs` / `RTSInputActionsFactory.cs` |
| Idle ホットキー参考 | `Assets/Scripts/Selection/IdleUnitSelectionController.cs` |
| ユニット | `Assets/Scripts/Units/UnitManager.cs` / `Unit.cs` |
| Editor | `Assets/Scripts/Editor/Phase10SceneBuilder.cs` |

---

## ③ Phase 34 目的 — Control Groups（MVP）

**AoE2 の Control Group — 編成を数字キーで保存・即呼び出し**

### MVP 挙動

```
Player が Villager + Militia を複数選択
    ↓ Ctrl+2
スロット 2 に現在選択を保存（Player ユニットのみ）
    ↓ 別ユニットを選択
    ↓ 2 キー
スロット 2 のユニットを **置換選択**
    ↓ Shift+2
スロット 2 のユニットを **追加選択**（既存選択に加える）
```

### スロット仕様

| 項目 | 値 |
|------|-----|
| スロット数 | **9**（キー 1〜9） |
| 保存対象 | **Player** かつ **Alive** ユニットのみ |
| 空スロット保存 | Ctrl+N で選択 0 体 → **スロットクリア**（AoE2 準拠） |
| 呼び出し時 | 死亡済み参照は **除外**（リストからも prune） |
| 建物選択 | Control Group 操作時は **ユニット選択のみ** — 建物は保存しない |

---

## ④ 今回実装するもの

### 1. `ControlGroupManager.cs`（新規）

```csharp
public class ControlGroupManager : MonoBehaviour
{
    const int SlotCount = 9;
    readonly List<Unit>[] slots = new List<Unit>[SlotCount];

    public void SaveGroup(int slotIndex, IReadOnlyList<Unit> units); // 0-based index
    public void RecallGroup(int slotIndex, bool additive);
    public void HandleUnitDied(Unit unit); // 全スロットから除去
}
```

- **SaveGroup** — Player + Alive のみコピー。0 体ならスロット Clear
- **RecallGroup** — 生存ユニットのみ `SelectionManager` へ
  - `additive == false` → `SelectUnits(buffer)`
  - `additive == true` → `SelectUnitsAdditive(buffer)`（新 API、下記）
- **HandleUnitDied** — `SelectionManager.HandleUnitDied` と同様に static フック or `SelectionManager` から委譲

### 2. `SelectionManager` — 追加選択 API

Phase 32 で `SelectUnits`（置換）は実装済み。**Shift+数字** 用に追加:

```csharp
public void SelectUnitsAdditive(IReadOnlyList<Unit> units);
```

- 既存選択を **維持**し、units を追加（重複スキップ）
- 建物 / 資源選択は **クリア**（ユニット追加選択に合わせる — 既存 Shift クリックと同型）
- 0 体追加時は何もしない

### 3. `ControlGroupInputController.cs`（新規）

`IdleUnitSelectionController` と同パターン — `Update` でキー検知。

| 入力 | 動作 |
|------|------|
| **Ctrl + 1〜9** | 現在 `SelectedUnits` をスロットに保存 |
| **1〜9**（Ctrl なし、Shift なし） | スロット呼び出し（置換） |
| **Shift + 1〜9** | スロット呼び出し（追加） |

**Ctrl 検知（推奨）:**

```csharp
bool IsCtrlHeld =>
    Keyboard.current != null &&
    (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);
```

**数字キー検知:**

- **Option A（推奨）** — `Keyboard.current` で `digit1Key`〜`digit9Key` の `wasPressedThisFrame` を直接 poll（Input アセット変更不要）
- **Option B** — `RTSInputActionsBuilder` に 9 アクション追加 + Factory 更新

**競合回避:**

- Ctrl+数字は **保存** — Recall より先に判定
- Q / `.` / `,` 等の既存キーと **数字キーは競合しない**
- TC / Barracks 選択中に数字 → **ユニット Recall のみ**（建物選択はクリアしてユニット選択）

### 4. `SelectionManager.HandleUnitDied` 連携

```csharp
public static void HandleUnitDied(Unit unit)
{
    ...
    ControlGroupManager.HandleUnitDied(unit); // static 委譲
}
```

### 5. Phase10 SceneBuilder

- `ControlGroupManager` + `ControlGroupInputController` を SelectionManager オブジェクトに AddComponent
- SerializeField 参照（`selectionManager`, `controlGroupManager`）配線

### 6. HUD（任意）

- `ControlGroupHudView` — `Group 1: 3` 等（最小 1 行でも可）
- **MVP 省略可** — ホットキー動作が確認できれば OK

---

## ⑤ 今回やらないこと

- 10 スロット（0 キー）
- ダブルタップでカメラジャンプ
- Control Group 名 / ラベル編集
- CPU Control Group
- Phase10 マップ拡張（M2.6 完了後）
- M3 Archer 等（**Phase 35**）

---

## ⑥ 実装ステップ（推奨）

| Step | 内容 |
|------|------|
| 34-1 | `ControlGroupManager` — 9 スロット Save / Recall / Prune |
| 34-2 | `SelectionManager.SelectUnitsAdditive` + `HandleUnitDied` 連携 |
| 34-3 | `ControlGroupInputController` — Ctrl / Shift / 数字キー |
| 34-4 | Phase10 SceneBuilder 配線 |
| 34-5 | Play 確認 + ドキュメント — **M2.6 完了** |

---

## ⑦ 技術メモ

### Save — Ctrl+2 例

```csharp
if (IsCtrlHeld && digit2.wasPressedThisFrame)
{
    controlGroupManager.SaveGroup(1, selectionManager.SelectedUnits);
    return;
}
```

### Recall — 2 キー例

```csharp
if (!IsCtrlHeld && !input.IsShiftHeld && digit2.wasPressedThisFrame)
{
    controlGroupManager.RecallGroup(1, additive: false);
    return;
}
```

### Prune on Recall

```csharp
for (int i = slot.Count - 1; i >= 0; i--)
{
    Unit unit = slot[i];
    if (unit == null || !unit.IsAlive || unit.Team != UnitTeam.Player)
        slot.RemoveAt(i);
}
```

### SelectUnitsAdditive イメージ

```csharp
ClearBuildingSelection();
ClearInfoSelection();
for each unit in units:
    if IsPlayerUnit(unit) && !selectedUnits.Contains(unit):
        selectedUnits.Add(unit);
        unit.SetSelected(true);
```

---

## ⑧ 完了条件（Phase 34 MVP）

- [ ] **Ctrl+1〜9** — 現在選択をスロット保存（Player ユニットのみ）
- [ ] **Ctrl+N + 0 体選択** — スロットクリア
- [ ] **1〜9** — スロット呼び出し（置換選択）
- [ ] **Shift+1〜9** — スロット追加選択
- [ ] 死亡ユニット — Recall 時除外 + スロットから prune
- [ ] Rally / 生産キュー / Idle UX / 採集 / Aggro **回帰**
- [ ] `docs/IMPLEMENTATION_STATUS.md` / `05_M2_6` Phase 34 ✅ — **M2.6 完了**

---

## ⑨ テスト手順（Play チェックリスト）

1. `Phase10.unity` → Play
2. Villager 3 体を矩形選択 → **Ctrl+1** → 別ユニット選択 → **1** → 3 体が再選択
3. Militia 2 体選択 → **Ctrl+2** → **2** → Militia 2 体選択
4. Villager 1 体 + **Shift+1** → Villager 4 体選択（1 + 3）
5. Ctrl+1 で 0 体（何も選択せず）→ **1** → 何も選択されない（スロット空）
6. Militia を戦闘で死亡 → **2** → 生存 Militia のみ選択
7. Rally / Q キュー / `.` Idle — **回帰**
8. Console エラーなし

Phase 34 のみ実装。**Phase10 マップ拡張 / M3 Phase 35** に触れない。

**M2.6 全 Phase 完了後:** [06_M2_7_SANDBOX_PHASES.md](../06_M2_7_SANDBOX_PHASES.md) Phase 35 → M3 Phase 36 へ。

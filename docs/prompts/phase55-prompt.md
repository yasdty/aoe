# Phase 55 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜54 完了（View Split / uGUI HUD / Minimap）  
> **マイルストン:** M5 — **Unit Animation**  
> **ロードマップ:** [09_M5_VISUAL_UI_PHASES.md](../09_M5_VISUAL_UI_PHASES.md)  
> **関連:** Phase 52 View 分離 / `EntityVisualBuilder` / `PlaceholderVisualKind` / `Unit.State` / Gather Managers  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 55 実装（Unit Animation）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 55 のみ実装。** ユニット状態に応じた **Animator MVP**（rewrite 禁止）。

**前提:** ユニット見た目は `EntityVisualBuilder` → `Visual` 子（Capsule fallback or `Resources/PlaceholderVisuals/*`）。**現状は `Unit.UpdateVisual()` の色変更のみ** — 攻撃中オレンジ tint 含む。Simulation（移動・戦闘・採集）は **既存 Manager** が担当済み。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| 対象兵種 | **Villager** / **Militia** / **Archer** — 3 種以上の状態アニメ（M5 完了条件） |
| 状態 | **Idle** / **Walk** / **Gather** / **Attack** / **Dead**（Dead は非表示 or 倒れ — 最小で可） |
| 方式 | Unity **Animator** + 簡易 **AnimationClip**（プレースホルダー Capsule/Mesh の transform キー — ボーンリグ不要） |
| 分離 | Phase 52 方針 — **`UnitAnimationView`**（View 層）が Animator を駆動。**`Unit` 本体に Animator 参照を直結しない** |
| 状態取得 | `Unit.State` + `GatherManager` / `FoodGatherManager` / `MineralGatherManager.IsUnitGathering` — **Gather は Move より優先** |
| 向き | 移動・攻撃時に **Visual または root** を XZ 方向へ向ける（View 層 — simulation の `TickMovement` は変更最小） |
| プール | `UnitPool` 再利用時に Animator 状態リセット |
| Editor | `AoE → Setup Unit Animations (Phase55)` — Controller / Clip 生成 or Prefab へ Animator 追加 |

**やらないこと:** 本格スケルタルリグ / Mixamo / 全兵種（Scout / Cavalry 等）/ 建築アニメ / Phase 56 VFX・SE / NavMesh / M6 同期

---

## ② 現状（読み取り用）

| 項目 | 値 |
|------|-----|
| `UnitState` | `Idle`, `Move`, `Attack`, `Dead` — **Gather なし**（View で Gather 判定を追加） |
| `Unit.State` | Attack → `AttackManager` / `BoarAttackManager`、Move → `HasMoveTarget` |
| Visual 種別 | `PlaceholderVisualKind.Villager` / `Militia` — `EntityVisualBuilder.GetUnitVisualKind`（Archer も Militia 扱い） |
| Prefab | `Assets/Resources/PlaceholderVisuals/VillagerVisual.prefab` 等 — **Animator 未設定** |
| 色 tint | `Unit.UpdateVisual()` — 選択・敵色・攻撃 tint。**Phase 55 では維持**（Animator と併用可） |

**Archer 方針:** 同一 Militia メッシュでも **`UnitAnimationProfile.Archer`** で Walk/Attack クリップ速度 or パラメータ差分 — 別 Prefab 必須ではない。

---

## ③ 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| ユニット Sim | `Unit.cs` — `State`, `TickMovement`, `NotifyStateChanged`, `UpdateVisual` |
| Visual 生成 | `EntityVisualBuilder`, `UnitPool`, `PlaceholderVisualCatalog` |
| View 分離 | `PlacementPreviewView`, `IPlacementPreviewView` — 同パターンで View コンポーネント |
| 採集 | `GatherManager`, `FoodGatherManager`, `MineralGatherManager` — `IsUnitGathering` |
| 戦闘 | `AttackManager.IsUnitAttacking` |
| Editor | `Phase10SceneBuilder`, `Phase1SceneBuilder.CreateUnit` |

---

## ④ 実装タスク

### 1. UnitVisualState（View 用）

```csharp
// 例 — 配置は View 名前空間推奨
public enum UnitVisualState { Idle, Walk, Gather, Attack, Dead }
public static class UnitVisualStateResolver
{
    public static UnitVisualState Resolve(Unit unit);
}
```

- Gather: 3 つの GatherManager のいずれかが true なら **Gather**（Move より優先）
- それ以外は `Unit.State` をマッピング

### 2. UnitAnimationView（`Assets/Scripts/View/` or `Visuals/`）

- `UnitAnimationView : MonoBehaviour` — `Visual` 子（または unit root）にアタッチ
- `Initialize(Unit unit)` or `Awake` で unit 参照
- `LateUpdate` — `UnitVisualStateResolver` → Animator パラメータ更新
- 推奨パラメータ（名前は実装で統一）:
  - `Speed` (float) — Walk ブレンド / 0 = Idle
  - `IsGathering` (bool)
  - `IsAttacking` (bool)
  - `IsDead` (bool)
- **向き:** `HasMoveTarget` or 攻撃対象方向から XZ `LookRotation`（slerp 可）

### 3. Animator Controller MVP

| 方式 | 内容 |
|------|------|
| A（推奨） | **Editor スクリプト**で AnimationClip をコード生成（Visual.localPosition Y bob / 軽い scale pulse）+ AnimatorController を `Assets/Resources/UnitAnimation/` に保存 |
| B | Runtime のみ — `Animator` なしで `UnitAnimationView` が transform を直接揺らす（**Animator MVP 要件を満たさない — 非推奨**） |

**Phase 55 デフォルト: A** — Villager / Militia（+ Archer プロファイル）各 4〜5 状態

### 4. スポーン配線

- `EntityVisualBuilder.AttachVisualOrFallback` 後 or `UnitPool.CreateFreshUnit` で `UnitAnimationView` を Ensure
- `Unit.NotifyStateChanged()` から View へイベント不要（LateUpdate ポーリングで MVP 可 — small diff 優先）
- Pool 返却時: Animator `Rebind()` or パラメータリセット

### 5. PlaceholderVisualKind / Profile（任意・小 diff）

```csharp
public enum UnitAnimationProfile { Villager, Militia, Archer }
public static UnitAnimationProfile GetProfile(UnitData data);
```

- Villager: `!CanAttack`
- Militia: 近接 `CanAttack` + melee range
- Archer: `CanAttack` + pierce / range > 3 等 — 既存 `UnitData` フィールドで判定

### 6. Editor / Phase10

- `[MenuItem("AoE/Setup Unit Animations (Phase55)")]` — Clip + Controller 生成、既存 Prefab に Animator 追加
- `BuildPhase10Scene` 変更は **最小**（メニュー実行で足りれば可）
- Play: Villager 採集・Militia/Archer 戦闘で状態が切り替わること

### 7. 回帰

- Phase 54 ミニマップ / Phase 53 HUD
- 選択色・敵色 tint（`UpdateVisual`）維持
- 800 体目標 — **全ユニット LateUpdate 禁止** → 距離カリング or `UnitManager` バッチ更新は **Phase 55 では近距離のみ** or 既存ユニット数で十分ならそのまま（過剰最適化不要）
- Console エラーなし

---

## ⑤ 制約

- small diff only / rewrite 禁止
- Simulation Manager が `Animator` 型を直接参照しない
- NavMesh 禁止
- 外部アセット（Mixamo 等）インポート禁止 — **コード生成 Clip のみ**
- Prefab 手編集最小 — Editor メニューで自動付与
- `Unit.TickMovement` / 戦闘ロジックの振る舞い変更禁止（向きのみ View で）

---

## ⑥ Play 確認

1. `Phase10.unity` — `AoE → Setup Unit Animations (Phase55)` → Ctrl+S
2. **Villager** — 木/食料/石採集で **Gather**、移動で **Walk**、待機で **Idle**
3. **Militia** — 敵へ移動 **Walk**、接敵 **Attack**
4. **Archer** — 射程外移動 **Walk**、射撃 **Attack**（Militia と差別化が分かる程度で可）
5. 死亡 — ユニット非表示 or Dead 状態（既存 `Unit` 死亡処理に合わせる）
6. Phase 54 回帰 — ミニマップ・HUD
7. Console エラーなし

---

## ⑦ 完了時ドキュメント

- [x] `IMPLEMENTATION_STATUS.md` — Phase 55 ✅ / Unit Animation ✅
- [x] `09_M5_VISUAL_UI_PHASES.md` — Phase 55 ✅
- [x] 本プロンプト — ✅

---

Phase 55 のみ。**Phase 56（Combat VFX & Audio）・M6 には触れない。**

> **次:** Phase 56 Combat VFX & Audio

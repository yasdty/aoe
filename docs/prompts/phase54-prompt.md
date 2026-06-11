# Phase 54 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜53 完了（uGUI HUD — `GameplayCanvas` / `HudUiFactory` / `GameUiInput`）  
> **マイルストン:** M5 — **Minimap**  
> **ロードマップ:** [09_M5_VISUAL_UI_PHASES.md](../09_M5_VISUAL_UI_PHASES.md)  
> **関連:** Phase 52 View 分離 / Phase 53 `HudRoot` レイアウト / `RTSCameraController` / Phase10 サンドボックス地面  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 54 実装（Minimap）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 54 のみ実装。** 俯瞰ミニマップ + TC / 主要建築アイコン（rewrite 禁止）。

**前提:** Phase 53 で `GameplayCanvas` + `HudRoot` に uGUI HUD が配置済み。`CpuHudView` は OnGUI 右上のまま — ミニマップは **その下 or 左** に uGUI で追加。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| 表示 | **RawImage** ベースのミニマップ（RenderTexture または CPU テクスチャ塗り — 小 diff 優先） |
| 位置 | **右上**（`CpuHudView` OnGUI と重ならない — anchor top-right, margin） |
| アイコン | **Player TC** / **Enemy TC**（必須）— 色分け（例: 青 / 赤） |
| 任意アイコン | Barracks / Market 等は **Phase 54 では TC のみでも可** — 拡張しやすい Registry 推奨 |
| カメラ | 現在の **視野範囲** を矩形で表示（ワールド XZ → ミニマップ UV） |
| クリック | ミニマップクリックで **カメラ focus 移動**（`RTSCameraController.ApplyOverviewView` 相当 or XZ pan） |
| 座標 | Phase10 地面 bounds を **単一真実源**（`MapBounds` static or ScriptableObject） |
| HUD 統合 | `HudUiFactory.SetupScreenPanel` / `GameUiInput.RegisterHudPanel` |
| i18n | ツールチップ等あれば `Localization` — ラベルなし MVP 可 |

**やらないこと:** 全ユニットドット（55 以降任意）/ Fog of War / 回転ミニマップ / M6 同期 / RenderTexture 高解像度 polish

---

## ② Phase10 マップ前提（読み取り用）

| 項目 | 値（Editor `Phase10SceneBuilder`） |
|------|-------------------------------------|
| 地面 scale | `(18, 1, 18)` — Unity Plane 10×10 → **約 180×180** |
| 地面 position | `(0, 0, -30)` |
| Player TC | `(0, 0, 0)` |
| CPU TC | `(0, 0, -60)` |
| カメラ | `RTSCameraController` — pitch/yaw 固定、XZ パン + ズーム |

**MapBounds 例:** `minX=-90, maxX=90, minZ=-120, maxZ=60`（地面 AABB から算出 — ハードコード可、Editor 同期推奨）

---

## ③ 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| uGUI 配置 | `HudUiFactory`, `GameUiInput`, `ResourceHudView.TryBuildUi` |
| カメラ | `RTSCameraController` — `ApplyOverviewView`, 移動・ズーム |
| TC | `TownCenter`, `UnitTeam`, `Phase10SceneBuilder` 配置座標 |
| 建築 | `BuildingHealth`, `PlacedBuildingKind` — アイコン拡張用 |
| HUD ヒット | `SelectionManager` — ミニマップ上クリックを地面選択に流さない |
| OnGUI CPU HUD | `CpuHudView` — 右上占有を避ける Y オフセット |

---

## ④ 実装タスク

### 1. MapBounds（Core or Spatial）

```csharp
// 例 — 名前・配置は実装に合わせて調整可
public static class MapBounds
{
    public static Vector2 WorldToNormalized01(Vector3 worldPosition);
    public static Vector3 Normalized01ToWorld(Vector2 uv, float y = 0f);
    public static Rect WorldXZRect { get; }
}
```

- Phase10 地面 Transform から AABB 算出 or 定数 — **ワールド↔UV 変換の単一真実源**
- ミニマップ View / カメラクリックの両方が参照

### 2. MinimapView（`Assets/Scripts/Selection/` or `Visuals/`）

- `MinimapView : MonoBehaviour` — Presenter + uGUI 構築
- `TryBuildUi()` — Phase 53 パターン（`HudRoot` 配下、`MinimapPanel` host）
- **右上:** `SetupScreenPanel` with top-right anchor helper（`HudUiFactory` に `SetAnchoredTopRight` 追加可）
- 子要素:
  - `RawImage` — 背景（単色 or 簡易 RenderTexture）
  - **アイコン** — `Image` プール（TC ×2 最小）
  - **カメラ矩形** — 細線 `Image` 4辺 or 1つの Outline 付き Rect
- `LateUpdate` — TC 位置を `MapBounds.WorldToNormalized01` で更新
- カメラ視野: Main Camera 地面交差 4 点 → UV bounding rect

### 3. クリックでカメラ移動

- `IPointerClickHandler` or `Button` + `RectTransformUtility` で UV 取得
- UV → ワールド XZ → `RTSCameraController` に focus 移動（public メソッド追加可）
- `GameUiInput.RegisterHudPanel` — 配置・選択クリックと競合しないこと

### 4. 簡易地形表示（MVP いずれか）

| 方式 | 内容 |
|------|------|
| A（推奨） | 単色 RawImage + アイコンのみ — **最小 diff** |
| B | 1×1 RenderTexture + 上から Ortho カメラ（culling 地面のみ） |
| C | CPU で 128×128 Texture2D に ground 色塗り（1 回 or 低頻度更新） |

**Phase 54 デフォルト: A** — 地形テクスチャは Phase 55 以降任意

### 5. Editor / Phase10

- `Phase10SceneBuilder.EnsureMinimap()` + `[MenuItem("AoE/Add Minimap (Phase54)")]`
- `EnsureHudUi()` に Minimap host 追加（空 shell のみ）
- `BuildPhase10Scene` に組込
- Play: 両 TC がミニマップ上で識別可能

### 6. 回帰

- Phase 53 HUD レイアウト維持（左上資源・上中央時間・左下生産）
- Phase 49〜52 壁・Gate・ゴースト
- ミニマップクリック後も選択・移動 OK
- Console エラーなし

---

## ⑤ 制約

- small diff only / rewrite 禁止
- NavMesh 禁止
- Prefab 手書き禁止 — `HudUiFactory` コード生成
- Simulation Manager が uGUI 具体型を直接参照しない
- `Find` 乱用禁止 — TC は `TownCenter` 列挙 or 既存 Registry（起動時キャッシュ可）
- 毎フレーム全 Scene 走査禁止 — TC 2 体 + カメラ 1 程度なら MVP 可

---

## ⑥ Play 確認

1. `Phase10.unity` — `AoE → Add Minimap (Phase54)` → Ctrl+S（または Play 時 runtime 生成）
2. **Player TC**（中央付近）と **CPU TC**（北側）がミニマップ上で色分け表示
3. カメラパン — 視野矩形が追従
4. ミニマップクリック — カメラがその付近へ移動
5. HUD / 地面クリック — ミニマップヒット時は地面命令に落ちない
6. Phase 53 回帰 — 資源 HUD・`L` 言語切替
7. Console エラーなし

---

## ⑦ 完了時ドキュメント

- [ ] `IMPLEMENTATION_STATUS.md` — Phase 54 ✅ / Minimap ✅
- [ ] `09_M5_VISUAL_UI_PHASES.md` — Phase 54 ✅
- [ ] 本プロンプト — ✅

---

Phase 54 のみ。**Phase 55（Unit Animation）・Phase 56（Combat VFX）には触れない。**

> **次:** Phase 55 Unit Animation

# Phase 52 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜51 完了（M5 i18n — LanguageMap + 主要 OnGUI HUD）  
> **マイルストン:** M5 — **View Layer Split**  
> **ロードマップ:** [09_M5_VISUAL_UI_PHASES.md](../09_M5_VISUAL_UI_PHASES.md)  
> **関連:** Phase 49 繰越の **壁ドラッグ列ゴースト** / Phase 53 で OnGUI → uGUI 本格移行 / Phase 51 `Localization` API を View から利用  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 52 実装（View Layer Split）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 52 のみ実装。** Simulation と View を **interface 経由**で分離し、uGUI Canvas シェルを追加（rewrite 禁止）。

**前提:** Phase 51 で `Localization`（Core 層）と OnGUI HUD が動作済み。Phase 49/50 で壁ドラッグ列配置・単体ゴースト（GameObject）あり。**列ゴーストは未実装** — 本 Phase で `IPlacementPreviewView` に集約。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| View インターフェース | `IPlacementPreviewView`（必須）+ `IHudLayoutProvider` or 既存 `GameUiInput` 拡張 |
| 配置プレビュー分離 | `BuildingPlacementManager` からゴースト描画を剥がし **Presenter / View 実装**へ |
| 壁ドラッグ列ゴースト | ドラッグ中に **全セグメント**を有効/無効色で表示（Phase 49 繰越） |
| uGUI シェル | Screen Space Overlay Canvas + EventSystem — Editor（`Phase10SceneBuilder`）で生成 |
| Simulation 純度 | Manager は **interface のみ**参照 — 具体 View 型（MonoBehaviour 名）を直接 `Find` しない |
| OnGUI HUD | **Phase 52 では残す** — 資源・生産パネルの uGUI 移行は Phase 53 |
| i18n | View 新規文言は `Localization.Get` 必須 |

**やらないこと:** 全 HUD の uGUI 移行（Phase 53）/ Minimap（54）/ Animation（55）/ M6 Entity ID / ネットコード

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| 配置・ゴースト | `BuildingPlacementManager` — `CreateGhost`, `UpdateGhostVisual`, `RefreshGhostFromPointer`, `TryConfirmWallDragLine` |
| 壁列 | `WallPlacementUtility.GetWallLinePositions`, `wallDragTracking`, `SelectionManager` スキップ |
| HUD ヒット | `GameUiInput`, `ResourceHudView.IsPointerOverHud` |
| 既存 View | `ResourceHudView`, `SelectionManager` |
| Editor 配線 | `Phase10SceneBuilder` — SelectionManager オブジェクトへの Component 追加パターン |
| i18n | `Localization`, `LanguageMapBootstrap` |

---

## ③ 実装タスク

### 1. View インターフェース（`Assets/Scripts/View/` 新規）

```csharp
// 例 — 名前・シグネチャは実装に合わせて調整可
public interface IPlacementPreviewView
{
    void ShowSinglePreview(PlacementPreviewState state);
    void ShowWallLinePreview(IReadOnlyList<PlacementPreviewState> segments);
    void HidePreview();
}

public readonly struct PlacementPreviewState
{
    public PlacedBuildingData data;
    public Vector3 groundPosition;
    public float wallOrientationY;
    public bool valid;
}
```

- Simulation 側は **DTO（PlacementPreviewState）** のみ渡す — Renderer / Material を Manager に置かない
- 登録: `PlacementPreviewViewRegistry` static or `BuildingPlacementManager` が `[SerializeField] MonoBehaviour` 1 つ（interface キャスト）— **Find 禁止**

### 2. `BuildingPlacementManager` リファクタ（small diff）

- `ghostObject` / `UpdateGhostVisual` を **IPlacementPreviewView 実装**へ移動
- `RefreshGhostFromPointer` → View に `ShowSinglePreview` を通知
- **壁ドラッグ中**（`wallDragTracking == true`）:
  - ポインタ start/end から `WallPlacementUtility.GetWallLinePositions`
  - 各セグメントの `CanPlaceBuildingAt`（または同等）で valid 判定
  - `ShowWallLinePreview` — 資源不足で打ち切るセグメントは invalid 色
- 配置確定・キャンセル時 `HidePreview`
- gameplay ロジック（`TryConfirmWallDragLine`, コスト消費, 占有）は **変更最小**

### 3. デフォルト View 実装

- `PlacementPreviewView`（MonoBehaviour）— 既存 `EntityVisualBuilder.CreateGhostVisual` を再利用
- 単体: 現行と同等の 1 ゴースト
- 列: セグメント数分の子オブジェクト or プール（Destroy 連打を避ける簡易プール推奨）
- Layer: `Ignore Raycast` 維持

### 4. uGUI Canvas シェル

- `Phase10SceneBuilder` に `EnsureGameplayCanvas()`:
  - Canvas（Screen Space Overlay, scaler 1280×720 基準）
  - EventSystem
  - 空の `HudRoot` RectTransform（Phase 53 用プレースホルダ）
- OnGUI と **共存** — Phase 53 まで両方動作

### 5. Phase10 / ドキュメント

- Play: 既存単体ゴースト OK + **壁ドラッグ中に列プレビュー**
- Phase 49/50 回帰: ドラッグ確定・通行遮断・Gate・時代別壁
- Phase 51 回帰: `L` で JA 切替
- Console エラーなし

---

## ④ 制約

- small diff only / rewrite 禁止
- NavMesh 禁止
- Simulation Manager が uGUI / OnGUI を直接触らない
- Phase 53 用に interface を public かつ Core/View フォルダ分離
- `Localization` は Core — View から参照可

---

## ⑤ Play 確認

1. `Phase10.unity` — Debug + CPU Relaxed — `AoE → Sync` / シーン保存
2. House 配置 — 単体ゴースト（有効/無効色）従来どおり
3. Palisade **ドラッグ** — ドラッグ中に **列全体**がプレビュー表示 → 離して確定
4. 無効マス（重複・資源外）— 列プレビューが途中で赤/無効色
5. Gate / Stone Wall — 単体・列ともプレビュー OK
6. `L` — 日本語 HUD 維持
7. Console エラーなし

---

## ⑥ 完了時ドキュメント

- [ ] `IMPLEMENTATION_STATUS.md` — Phase 52 ✅ / View Split ✅ / 列ゴースト ✅
- [ ] `09_M5_VISUAL_UI_PHASES.md` — Phase 52 ✅、繰越表の列ゴーストを ✅
- [ ] 本プロンプト — ✅

---

Phase 52 のみ。**Phase 53（HUD Migration）・Phase 54（Minimap）には触れない。**

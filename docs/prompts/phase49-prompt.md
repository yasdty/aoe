# Phase 49 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜48 完了（M4 RTS UX Polish）  
> **マイルストン:** M5 — **Wall & Gate System**  
> **ロードマップ:** [09_M5_VISUAL_UI_PHASES.md](../09_M5_VISUAL_UI_PHASES.md)  
> **関連:** Phase 44/48 の壁は **配置・HP のみ**。本 Phase で **AoE2 同型の防衛 gameplay** を完成させる。  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 49 実装（Wall & Gate System）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 49 のみ実装。** NavMesh 禁止。Manager 集中 Tick。rewrite 禁止。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| **通行遮断** | Palisade / Stone Wall が **ユニット移動をブロック**（Grid 占有 or StaticObstacle — 方針は CONSTITUTION 準拠） |
| **ドラッグ連続配置** | マウス **ドラッグ**で壁セグメント列（Shift+クリック連打ではない） |
| **セグメント接続** | 隣接スナップ・角の最低限接続（見た目は Placeholder 可） |
| **Gate（門）** | 新 `PlacedBuildingKind.Gate` — **自チームのみ通過**（将来同盟はフックのみ） |
| Phase 48 壁 UX | Shift+クリック連続は **置き換え or 統合**（AoE2 型ドラッグを正とする） |

**やらないこと:** 時代別壁グレード（**Phase 50**）/ i18n（**Phase 51**）/ uGUI 移行 / Castle

**参考（AoE2 Wiki 日本語）:** フェンス（Palisade）/ 石の城壁 / **門**（Gate）

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| 壁配置 | `BuildingPlacementManager` — `IsWallKind` / `TryWallShiftDragPlacement` |
| 占有・衝突 | `CanPlaceAt` / footprint / `UnitManager.TickMovement` |
| 建築 Data | `PlacedBuildingData` / `PlacedBuildingKind` |
| 完成生成 | `RuntimeBuildingFactory` / `BuildingPool` |
| Phase 44 | Palisade / Stone Wall / Watch Tower Data |

---

## ③ 実装タスク

### 1. グリッド占有 / 通行遮断

- 壁完成時に **占有セル**を登録（`WallOccupancyRegistry` 等 — 1 箇所）
- `UnitManager` 移動 Tick で占有セルへ **進入不可**（押し出し or 停止）
- Gate セルは **通過許可ルール**（自 Team）

### 2. AoE2 型ドラッグ配置

- 左ボタン **押下→ドラッグ→離す** でセグメント列
- グリッドに沿った Bresenham 風 or 直交優先スナップ
- 資源不足・無効タイルで打ち切り
- Esc / 右クリックでキャンセル（既存）

### 3. Gate

- `GateData` ScriptableObject（Feudal 以降 MVP 可 — Phase 44 Stone Wall と同 Age で可）
- 配置: 壁列上 or 壁に隣接（AoE2 近似で可）
- 通過: `Unit.Team == gate.Team` のみ（Ray or セル中心判定）

### 4. Phase10 / Editor

- `AoE → Sync AoE2 Game Data` / Gate Data 追加
- HUD: 壁/Gate ボタン or 既存 Defense ボタン拡張
- **`AoE → Sync Input Actions`** — Phase10 優先

---

## ④ 制約

- small diff only / Data 駆動
- Watch Tower 既存攻撃を壊さない
- Phase 42 Balance / CPU AI を壊さない

---

## ⑤ Play 確認

1. `Phase10.unity` — Debug + CPU Relaxed
2. Palisade を **ドラッグ** → 直線/折れ列が配置される
3. 村民が **壁をすり抜けない**（迂回 or 停止）
4. **Gate** 配置 → 自軍のみ通過、敵軍は不可
5. Stone Wall も同様
6. Console エラーなし

---

## ⑥ 完了時ドキュメント

- [ ] `IMPLEMENTATION_STATUS.md` — Phase 49 ✅ / 壁遮断・Gate
- [ ] `09_M5_VISUAL_UI_PHASES.md` — Phase 49 ✅
- [ ] 本プロンプト — ✅

---

Phase 49 のみ。**Phase 50（壁 Age グレード）・Phase 51（i18n）には触れない。**

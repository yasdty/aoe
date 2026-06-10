# Phase 44 実行プロンプト

> **状態:** ✅ 実装済み（Play 確認推奨）  
> **前提:** Phase 1〜43 完了（M4 Blacksmith & Tech）  
> **マイルストン:** M4 AoE Gameplay — **Defense**  
> **ロードマップ:** [08_M4_GAMEPLAY_PHASES.md](../08_M4_GAMEPLAY_PHASES.md)  
> **Balance 方針:** [12_GAMEPLAY_BALANCE_MODE.md](../12_GAMEPLAY_BALANCE_MODE.md) — 建築コスト・時間は **GameplayBalance 層経由**  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 44 実装（Defense）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 44 のみ実装。** 既存 **建築配置パターン**（House / Barracks / Blacksmith）を壁・塔に流用（rewrite 禁止）。

**前提:** Phase 42〜43 で `requiredAge` / `GameplayBalance` / `BuildingPlacementManager` / `RuntimeBuildingFactory` が整備済み。本 Phase で **防衛建築 MVP** を完成させる。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| 柵 | **Palisade Wall** — Dark Age から建築可能（Wood） |
| 石壁 | **Stone Wall** — Feudal 以降（Stone + Wood または Stone のみ — AoE2 基準で 1 種に絞る） |
| 塔 | **Watch Tower** — Feudal 以降、遠距離攻撃（Archer 相当の簡易攻撃で可） |
| 配置 | 既存ゴースト配置 — 細長フットプリント or セグメント 1 マス MVP |
| HP | `PlacedBuildingData.maxHp` + `BuildingHealth` |
| 建築 | 村民が建築サイトを建てる（既存 `ConstructionSite` パターン） |
| Balance | コスト・建築時間 — `PlacedBuildingData.Scaled*` / `GameplayBalance` |
| CPU | 壁・塔建設は **MVP 任意**（プレイヤー優先） |

**やらないこと:** 城壁の連結自動補完 / Gate（扉）/ 攻城兵器 / 複数塔種 / Market（45）/ uGUI 本格 HUD / 弾丸ビジュアル

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| 建築 Kind | `PlacedBuildingKind`、`PlacedBuildingData` |
| 配置 | `BuildingPlacementManager` — `Enter*PlacementMode` / `CompleteConstruction` |
| ファクトリ | `RuntimeBuildingFactory`、`PlacedBuildingDataResolver` |
| Data Sync | `Phase1SceneBuilder.Ensure*Data()` |
| HUD | `ResourceHudView` — Feudal 判定は `GameSessionManager.CanBuild` |
| 攻撃 | `AttackManager`、Archer 遠距離 — 塔は簡易 `BuildingAttack` or 既存コンポーネント再利用 |
| Phase 43 | `Blacksmith.cs` — 選択・パネル不要（壁は選択 Info のみで可） |

---

## ③ 実装タスク

### 1. `PlacedBuildingKind` 拡張 + Data

```csharp
// 例: PalisadeWall, StoneWall, WatchTower
```

- `Phase1SceneBuilder.EnsurePalisadeWallData()` / `EnsureStoneWallData()` / `EnsureWatchTowerData()`
- AoE2 基準値 + `GameplayBalance` で Debug 短縮
- 壁は **footprintWidth/Depth** を細長に（例: 1×4）— 連結は後 Phase

### 2. コンポーネント

- `PalisadeWall` / `StoneWall` — `House` 同型（Team, Data, HP、訓練なし）
- `WatchTower` — 静止遠距離攻撃（射程内の敵ユニットを `AttackManager` または専用 Tick で攻撃）
- `RuntimeBuildingFactory.Create*` + `CompleteConstruction` 分岐

### 3. 配置・HUD

- `EnterPalisadeWallPlacementMode` / `EnterStoneWallPlacementMode` / `EnterWatchTowerPlacementMode`
- `ResourceHudView` ボタン（Stone Wall / Tower は Feudal 以降表示）
- 配置コストは Wood / Stone を既存 `ResourceManager` 経由

### 4. Phase10 配線

- `Phase10SceneBuilder` — Data 参照・Placement・HUD
- 既存シーン向け `AoE → Add Defense (Phase44)` パッチメニュー（Phase 43 パターン）

---

## ④ 制約

- rewrite 禁止 / small diff only
- `.meta` 手書き禁止
- コストは **GameplayBalance のみ**（Debug 用 duplicate Data 禁止）
- OnGUI MVP
- Phase 42 Balance / CPU Relaxed（2分猶予・5分波）/ Phase 43 Blacksmith を **壊さない**

---

## ⑤ Play 確認

1. `Phase10.unity` — **Debug + CPU Relaxed**
2. Palisade を Wood で配置 → 建築完了 → HP 表示
3. Feudal 昇格後 Stone Wall / Watch Tower 配置可能
4. 塔が近くの敵 CPU ユニットを攻撃 — Console に `[WatchTower]` ログ（Relaxed 猶予中は CPU 攻撃波なし。塔射程 7m 内の敵のみ）
5. コスト表示 = Balance 後の値
6. Console エラーなし（`AudioListener` 警告は Main Camera から除去済み）

---

## ⑥ 完了時ドキュメント

- [x] `IMPLEMENTATION_STATUS.md` — Phase 44 ✅
- [x] `08_M4_GAMEPLAY_PHASES.md` — Phase 44 ✅
- [x] 本プロンプト — Play 確認待ち → ✅

---

Phase 44 のみ。**Phase 45 Market には触れない。**

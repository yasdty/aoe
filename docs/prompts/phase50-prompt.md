# Phase 50 実行プロンプト

> **状態:** ⬜ 未着手  
> **前提:** Phase 1〜49 完了（M5 Wall & Gate — 通行遮断・ドラッグ列・Gate）  
> **マイルストン:** M5 — **Wall Age Grades**  
> **ロードマップ:** [09_M5_VISUAL_UI_PHASES.md](../09_M5_VISUAL_UI_PHASES.md)  
> **関連:** Phase 44 で Palisade（Dark）/ Stone Wall（Feudal）/ Gate（Feudal）の Data は存在。本 Phase は **時代に応じた壁種の整理・自動差替え・（任意）グレードアップ**  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 50 実装（Wall Age Grades）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 50 のみ実装。** 既存 **壁・Gate・Age Up** を Data 駆動で拡張（rewrite 禁止）。

**前提:** Phase 49 で壁ドラッグ列配置・`WallOccupancyRegistry`・Gate 通過は動作済み。Phase 42 で `GameSessionManager.GetAge` / `CanBuild(requiredAge)` あり。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| Dark Age | **Palisade（フェンス）** のみ — Wood |
| Feudal Age | **Stone Wall（石の城壁）** + **Gate** が利用可能（既存 Data の `requiredAge` を正しく反映） |
| 配置 UX | Feudal 昇格後、壁ボタンが **時代に応じて適切な種**を出す（Palisade 継続可 or Stone 追加 — AoE2 近似） |
| グレードアップ | **任意 MVP:** 既存 Palisade セグメントを選択 → アップグレードで Stone Wall に置換（HP/占有更新）。**未実装でも可** — HUD で Stone のみ追加なら Phase 50 完了可 |
| Gate 種 | Feudal: **Palisade Gate / Stone Gate** の 2 種は **Nice to have** — MVP は既存 `GateData`（Feudal）維持で可 |
| Balance | コスト・HP — `PlacedBuildingData` + `GameplayBalance.Scaled*` |
| CPU | 壁種の時代判定は `CanBuild` 準拠（CPU 壁建設は任意） |

**やらないこと:** Castle 壁 / Imperial 要塞 / 海軍 / i18n（Phase 51）/ ドラッグ列ゴースト（Phase 52〜53）/ uGUI 本格 HUD

**参考（AoE2 Wiki 日本語）:** [フェンス](https://ageofempires.fandom.com/ja/wiki/フェンス) / [石の城壁](https://ageofempires.fandom.com/ja/wiki/石の城壁) / [門](https://ageofempires.fandom.com/ja/wiki/門)

---

## ② 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| 時代 | `GameSessionManager` — `GetAge`, `CanBuild`, `TryAgeUp` |
| 壁 Data | `PalisadeWallData`, `StoneWallData`, `GateData` — `requiredAge` |
| 配置 | `BuildingPlacementManager`, `ResourceHudView` — ボタン表示条件 |
| 占有 | `WallOccupancyRegistry`, `OrientedWallSegment`, `WallOccupancyRegistration` |
| Phase 49 | `WallPlacementUtility`, `TryConfirmWallDragLine` |
| Sync | `Phase1SceneBuilder.EnsurePalisadeWallData` / `EnsureStoneWallData` / `EnsureGateData` |

---

## ③ 実装タスク

### 1. Data / Age 整合

- `PalisadeWallData.requiredAge = Dark`, `StoneWallData` / `GateData` = Feudal（既存確認・不足なら修正）
- AoE2 基準 HP/コストを `Ensure*Data` に反映（Balance 層経由）
- `AoE → Sync AoE2 Game Data` でアセット更新

### 2. HUD / 配置モード

- Dark: **Build Palisade** のみ（Stone / Gate は gray + 時代ラベル — 既存パターン）
- Feudal: **Palisade + Stone Wall + Gate**（または Stone のみ強調 — 設計メモを `09_M5` に 1 行追記可）
- `GameSessionManager.CanBuild` を単一の真実源に — ボタンと `Enter*PlacementMode` で二重判定しない

### 3. （任意）Palisade → Stone グレードアップ

- 自軍 Palisade 選択時 OnGUI に **Upgrade to Stone Wall**（Stone コスト消費）
- 実装: 既存 `PalisadeWall` を `Destroy` + 同位置・同 orientation で `CreateStoneWall` + `WallOccupancyRegistry` 再登録
- 未選択時はスキップ可 — **優先度低**

### 4. Phase10 / ドキュメント

- Play: Dark で Palisade のみ / Feudal 昇格後 Stone+Gate 建築可
- 既存 Phase 49 壁列・遮断・Gate 通過を **壊さない**

---

## ④ 制約

- small diff only / rewrite 禁止
- Phase 49 の `WallOccupancyRegistry` API を維持（Unregister → 再 Register）
- NavMesh 禁止

---

## ⑤ Play 確認

1. `Phase10.unity` — Debug + CPU Relaxed
2. **Dark Age** — Stone Wall / Gate ボタン不可、Palisade のみ配置可
3. **Feudal 昇格** — Stone Wall + Gate 配置可
4. Phase 49 回帰: ドラッグ列・通行遮断・Gate 自軍通過
5. Console エラーなし

---

## ⑥ 完了時ドキュメント

- [ ] `IMPLEMENTATION_STATUS.md` — Phase 50 ✅ / 時代別壁 ✅
- [ ] `09_M5_VISUAL_UI_PHASES.md` — Phase 50 ✅
- [ ] 本プロンプト — ✅

---

Phase 50 のみ。**Phase 51（i18n）・Phase 52（View Split）には触れない。**

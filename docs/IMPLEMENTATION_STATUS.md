# AoE RTS Engine — Implementation Status

> **用途:** このファイル単体を AI に渡すことで、現状の実装範囲・未実装・AoE2 との差分・技術構成・拡張方針を把握できる。
>
> **最終更新:** Phase 59 ✅（Four-Player Match）。**M6 進行中 — 次: Phase 60 Team & 2v2。**
>
> **関連:** [CONSTITUTION.md](../CONSTITUTION.md) / [README.md](../README.md) / [docs/README.md](README.md)  
> **ロードマップ:** [05_M2_6](05_M2_6_RTS_UX_PHASES.md) / [06_M2_7](06_M2_7_SANDBOX_PHASES.md) / [07_M3](07_M3_MILITARY_PHASES.md) / [08_M4](08_M4_GAMEPLAY_PHASES.md) / [09_M5](09_M5_VISUAL_UI_PHASES.md) / [10_M6](10_M6_MULTIPLAYER_FOUNDATION.md) / [11 拡張設計](11_DEFERRED_EXTENSION_DESIGN.md) / [12 Balance Mode](12_GAMEPLAY_BALANCE_MODE.md)
>
> **更新ルール:** 各 Phase 完了時に本ファイルを更新する（[docs/README.md](README.md) チェックリスト参照）。

---

## 1. Project Overview

| 項目 | 内容 |
|------|------|
| **プロジェクト名** | AoE RTS Engine |
| **目的** | Age of Empires II ライクな Low-Spec RTS エンジンのプロトタイプ。MacBook Air 16GB 級で大規模戦闘を目指す基盤を Phase 単位で構築する |
| **技術スタック** | Unity 6 / C# / URP / New Input System |
| **対象スペック** | MacBook Air 16GB 級（将来: 4 チーム × 200 ユニット + 建築多数 + 大規模戦闘） |
| **対戦想定** | **4人 FFA（人間1 + CPU3）** または **1v1 CPU**（`MatchMode` / **M** キー切替・シーン再生成で反映）。**M6 目標: 2v2・大マップ・敵HP/修理/複数建設・Fog**（Phase 60〜65） |
| **設計方針** | Manager 集中更新 / NavMesh 禁止 / Unit 個別 Update 禁止 / Asset Store 購入禁止 / Unity アセット手書き禁止（Editor API のみ）/ Phase 単位の small diff / マルチプレイ将来互換を意識した simulation 分離 |

### アーキテクチャ原則（CONSTITUTION より）

- **禁止:** Rigidbody 大量利用、Unit ごとの Update 乱立、Unit ごとの Raycast 乱立、Dynamic Shadow 乱用
- **推奨:** Manager 更新方式、Object Pooling、GPU Instancing、Shared Material、Fixed Tick ✅（20 TPS）
- **マルチプレイ:** 現時点でネットコード実装禁止。Fixed Tick ✅ / Command Queue ✅（プレイヤー操作）/ CPU は直接 Manager 呼び出し

---

## 2. Current Progress

| Phase | 内容 | シーン | 状態 |
|-------|------|--------|------|
| 1 | RTS カメラ、地面、ユニット 1 体、選択、移動 | `Phase1.unity` | ✅ 実装済み |
| 2 | ドラッグ選択、複数選択、グループ移動 | `Phase2.unity` | ✅ 実装済み |
| 3 | TownCenter、Villager 生産 | `Phase3.unity` | ✅ 実装済み |
| 4 | 木材採集（Wood のみ） | `Phase4.unity` | ✅ 実装済み |
| 5 | House 建築（配置・ゴースト・建築時間） | `Phase5.unity` | ✅ 実装済み |
| 6 | 人口システム（Pop cap / House +5） | `Phase6.unity` | ✅ 実装済み |
| 7 | Barracks 建築、Militia 生産、近接攻撃 | `Phase7.unity` | ✅ 実装済み |
| 8 | 戦闘成立（死亡、HP バー、攻撃ビジュアル） | `Phase8.unity` | ✅ 実装済み |
| 9 | CPU 経済 AI（採集・House・Villager 増産） | `Phase9.unity` | ✅ 実装済み |
| 10 | CPU 軍事 AI（Barracks・Militia・攻撃波）、簡易 RTS 完成 | `Phase10.unity` | ✅ 実装済み |
| 10.5 | Visual Placeholder Upgrade（GLB 差し替え・ロジック不変） | `Phase10.unity` | ✅ 実装済み |
| 11 | Victory & Defeat（TC 破壊で勝敗・ゲーム終了） | `Phase10.unity` | ✅ 実装済み |
| 12 | Object Pool（Unit / Building） | `Phase10.unity` | ✅ 実装済み |
| 13 | Benchmark Infrastructure | `Benchmark.unity` | ✅ 実装済み |
| 14 | Spatial Hash（Unit / Tree 索引） | `Phase10.unity` | ✅ 実装済み |
| 15 | Fixed Tick（20 TPS Simulation） | `Phase10.unity` | ✅ 実装済み |
| 16 | Command Queue（プレイヤー操作） | `Phase10.unity` | ✅ 実装済み |
| 17 | Food（Berry Bush 採集 + Villager コスト） | `Phase10.unity` | ✅ 実装済み |
| 18 | Farm（建築 + 継続 Food 採集） | `Phase10.unity` | ✅ 実装済み |
| 19 | Lumber Camp（Wood Drop-off） | `Phase10.unity` | ✅ 実装済み |
| 20 | Gold + Stone（採掘 + TC 搬入） | `Phase10.unity` | ✅ 実装済み |
| 21 | Gather Repeat（搬入後採取継続） | `Phase10.unity` | ✅ 実装済み |
| 22 | Farm 1 人制限 + Spawn グリッド | `Phase10.unity` | ✅ 実装済み |
| 23 | Mining Camp（Gold/Stone Drop-off） | `Phase10.unity` | ✅ 実装済み |
| 24 | Hunting（Deer / Sheep） | `Phase10.unity` | ✅ 実装済み |
| 25 | Selection Info Panel | `Phase10.unity` | ✅ 実装済み |
| 26 | Boar（反撃狩り） | `Phase10.unity` | ✅ 実装済み |
| 27 | Mill（Food Drop-off） | `Phase10.unity` | ✅ 実装済み |
| 28 | Sheep Herding + Animal Locomotion | `Phase10.unity` | ✅ 実装済み |
| 29 | Militia Basic Aggro | `Phase10.unity` | ✅ 実装済み |
| 30 | CPU 4 Resources | `Phase10.unity` | ✅ 実装済み |
| 31 | Unit Production Queue（TC / Barracks） | `Phase10.unity` | ✅ 実装済み |
| 32 | Idle Unit UX | `Phase10.unity` | ✅ 実装済み |
| 33 | Rally Point | `Phase10.unity` | ✅ 実装済み |
| 34 | Control Groups | `Phase10.unity` | ✅ 実装済み |
| 35 | Phase10 Sandbox | `Phase10.unity` | ✅ 実装済み |
| 36 | Archery Range + Archer | `Phase10.unity` | ✅ 実装済み |
| 37 | Spearman | `Phase10.unity` | ✅ 実装済み |
| 38 | Stable + Cavalry + Scout | `Phase10.unity` | ✅ 完了（M3） |
| 39 | Counter System | `Phase10.unity` | ✅ 完了（M3） |
| 40 | Stance & Attack-Move | `Phase10.unity` | ✅ 完了（M3） |
| 41 | Formation | `Phase10.unity` | ✅ 完了（M3） |
| 42 | Age Up + Gameplay Balance | `Phase10.unity` | ✅ 完了（M4） |
| 43 | Blacksmith & Tech | `Phase10.unity` | ✅ 完了（M4） |
| 44 | Defense | `Phase10.unity` | ✅ 完了（M4） |
| 45 | Market | `Phase10.unity` | ✅ 完了（M4） |
| 46 | Civilization | `Phase10.unity` | ✅ 完了（M4） |
| 47 | Second TC | `Phase10.unity` | ✅ 完了（M4） |
| 48 | RTS UX Polish | `Phase10.unity` | ✅ 完了（M4） |
| 49 | Wall & Gate System | `Phase10.unity` | ✅ 完了（M5）— 列ゴースト Phase 52 ✅ |
| 50 | Wall Age Grades | `Phase10.unity` | ✅ 完了（M5） |
| 51 | Localization (i18n) | `Phase10.unity` | ✅ 完了（M5） |
| 52 | View Layer Split | `Phase10.unity` | ✅ 完了（M5） |
| 53 | HUD Migration | `Phase10.unity` | ✅ 完了（M5）— uGUI 主要 HUD |
| 54 | Minimap | `Phase10.unity` | ✅ 完了（M5）— TC アイコン / 視野 / クリック移動 |
| 55 | Unit Animation | `Phase10.unity` | ✅ 完了（M5）— Animator MVP / Idle・Walk・Gather・Attack |
| 56 | Combat VFX & Audio | `Phase10.unity` | ✅ 完了（M5）— 弾丸 / ヒット VFX / SE / 死亡 puff |
| 57 | Entity ID & PlayerId | `Phase10.unity` | ✅ 完了（M6）— `EntityRegistry` / `PlayerId` / Move・Attack Command ID 化 |
| 58 | CPU Command Queue | `Phase10.unity` | ✅ 完了（M6）— CPU AI → `CommandQueue` / `PlayerId` 対応 |
| 59 | Four-Player Match（1H + 3CPU） | `Phase10.unity` | ✅ 完了（M6） |
| 60 | Team & 2v2 | `Phase10.unity` | ⬜ 未着手（M6）— **次** |
| 61 | Large Map | `Phase10.unity` | ⬜ 未着手（M6） |
| 62 | Enemy HP Display（敵選択・HP バー） | `Phase10.unity` | ⬜ 未着手（M6） |
| 63 | Building Repair（建物修理） | `Phase10.unity` | ⬜ 未着手（M6） |
| 64 | Multi-Villager Build（複数人建設加速） | `Phase10.unity` | ⬜ 未着手（M6） |
| 65 | Fog of War | `Phase10.unity` | ⬜ 未着手（M6） |
| 66 | Deterministic Sim（LAN 準備） | `Phase10.unity` | ⬜ 未着手（M6・後回し可） |

**ゲームループ:** 採集 → 建築 → 生産 → 戦闘 → **勝敗判定**

**Foundation Milestone 1:** ✅ 完了（Phase 11〜16）

**Milestone 2 Economy:** ✅ 完了（Phase 17〜20 — Wood / Food / Gold / Stone）

**Milestone 2.5 Economy Polish:** ✅ 完了（Phase 21〜30）

**Milestone 2.6 RTS UX:** ✅ 完了（Phase 31〜34）

**Milestone 2.7 Sandbox:** ✅ 完了（Phase 35）

**Milestone 3 Military:** ✅ 完了（Phase 36〜41）

**Milestone 4 AoE Gameplay:** ✅ 完了（Phase 42〜48）

**Milestone 5 Gameplay Polish & Visual / UI:** ✅ 完了（Phase 49〜56）

**Milestone 6 — 4-Player & World Scale:** ⬜ 進行中（Phase 60〜65 が本体 / 66 は LAN 前 — **次: Phase 60**）

---

## 3. Implemented Features

### Camera

| 機能 | 状態 | 備考 |
|------|------|------|
| WASD カメラ移動 | ✅ | `RTSCameraController` |
| 画面端マウススクロール | ✅ | 12px ボーダー |
| マウスホイールズーム | ✅ | 高さ 12〜80 |
| 俯瞰開始視点 | ✅ | Phase 5 以降、TC 中心 |
| カメラ回転（自由操作） | ❌ | 固定ピッチ・ヨー |

### Input

| 機能 | 状態 | 備考 |
|------|------|------|
| New Input System | ✅ | `RTSInputActions`（Editor API 生成） |
| 左クリック選択 | ✅ | Unit / TownCenter / Barracks / Archery Range / Stable |
| 右クリック命令 | ✅ | 木 / Berry / Farm / 移動 / 攻撃（CommandQueue 経由） |
| ドラッグ矩形選択 | ✅ | Phase 2 以降 |
| Shift 追加選択 | ✅ | |
| Q キー Villager 生産 | ✅ | TownCenter 選択時 — **Q 連打でキュー追加**（最大 15） |
| Shift+Q Villager ×5 | ✅ | Phase 48 — TC 選択時 |
| Shift+Q Militia ×5 | ✅ | Phase 48 — Barracks 選択時 |
| 生産キュー取消 | ✅ | Phase 48 — キュー行クリック + `ProductionQueueRefundUtility` |
| H キー House 配置 | ✅ | Phase 48 — 村民選択時 `BuildHouse` |
| B キー Barracks 配置 | ✅ | Phase 48 — 村民選択時 `BuildBarracks` |
| Q キー Militia 生産 | ✅ | Barracks 選択時 Q |
| E キー Spearman 生産 | ✅ | Barracks 選択時 E（Phase 37） |
| Q/E Stable 生産 | ✅ | Cavalry / Scout（Phase 38） |
| Q キー Archer 生産 | ✅ | Archery Range 選択時 Q（Phase 36） |
| ユニット生産キュー | ✅ | TC / Barracks / Archery Range — FIFO 最大 15、先頭のみ Tick |
| Esc / 右クリックで配置キャンセル | ✅ | House / Barracks / Archery Range 配置モード |
| ゲームパッド | ❌ | 未対応 |

### Selection

| 機能 | 状態 | 備考 |
|------|------|------|
| 単体選択 | ✅ | |
| 複数選択・矩形選択 | ✅ | `SelectionBoxView`（OnGUI） |
| 建築選択（TC / Barracks） | ✅ | |
| CPU ユニット・建築の選択不可 | ✅ | `UnitTeam.Enemy` フィルタ — **Phase 62 で敵選択・HP 表示** |
| 選択時色変更 | ✅ | MaterialPropertyBlock |
| グループ移動グリッド整列 | ✅ | `GroupMoveFormation`（√n グリッド） |
| 建物スポーン周囲グリッド | ✅ | Phase 22 — `BuildingSpawnFormation`（TC / Barracks、16 スロット √n グリッド） |
| フォーメーション / 隊列維持 | ✅ | Phase 41 — `FormationMoveManager` + `UnitSeparation` |
| ホットキーグループ（Ctrl+数字） | ✅ | Phase 34 — Ctrl+1〜9 保存 / 数字 Recall / Shift+数字追加 |
| Idle Villager 表示・選択 | ✅ | Phase 32 — HUD カウント + `.` / Shift+. |
| 待機軍 `,` 選択 | ✅ | Phase 32 — 次の待機 Militia |
| Rally Point（集合地点） | ✅ | Phase 33 — TC / Barracks / Archery Range 右クリック + Spawn 後適用 |
| 選択詳細パネル（HP / 攻撃 / 資源残量） | ✅ | Phase 25 — `SelectionInfoPanelView` |
| 資源ノード左クリック選択 | ✅ | Phase 25 — Tree / Berry / Deer / Sheep / Boar / Mine |

### Movement

| 機能 | 状態 | 備考 |
|------|------|------|
| 直線移動 | ✅ | `UnitManager.TickSimulation` → 全 Unit `TickMovement` |
| 目標到達で停止 | ✅ | |
| 障害物回避 | ❌ | |
| NavMesh / Pathfinding | ❌ | 憲法で禁止 |
| RVO / ユニット間押し出し | ❌ | `UnitPositionOffsets` で軽微な散開のみ |
| 地形高低差 | ❌ | 平面 Ground のみ |

### Economy

| 機能 | 状態 | 備考 |
|------|------|------|
| Wood 資源 | ✅ | チーム別 `ResourceManager` |
| Food 資源 | ✅ | Berry Bush / Farm → `FoodGatherManager` → TC 搬入 |
| Gold / Stone | ✅ | Gold/Stone Mine → `MineralGatherManager` → TC 搬入 |
| 木（Tree）採集 | ✅ | `GatherManager` + `GatherCommand` → 最寄り Drop-off（TC / Lumber Camp） |
| 採取リピート（搬入後継続） | ✅ | Phase 21 — Wood / Berry / Farm / Gold / Stone |
| Berry Bush 採集 | ✅ | `FoodGatherManager` + `GatherFoodCommand` |
| Farm 採集 | ✅ | `FoodGatherManager` + `GatherFarmFoodCommand` |
| Farm 1 村民制限 | ✅ | Phase 22 — `FoodGatherManager.IsFarmOccupiedByOther` |
| 狩り（Deer / Sheep / Boar） | ✅ | Phase 24 Deer/Sheep — Phase 26 Boar — Phase 28 Neutral Sheep 発見 + 所属後狩り |
| Gold 採集 | ✅ | `MineralGatherManager` + `GatherGoldCommand` |
| Stone 採集 | ✅ | `MineralGatherManager` + `GatherStoneCommand` |
| TownCenter への搬入 | ✅ | チーム別 TC |
| Lumber Camp Drop-off | ✅ | Wood |
| Mining Camp Drop-off | ✅ | Phase 23 — Gold/Stone → 最寄り TC / Mining Camp |
| Mill Drop-off | ✅ | Phase 27 — Food → 最寄り TC / Mill |
| 羊の無所属・誘導 | ✅ | Phase 28 — Neutral 発見 → 所属 / `SheepMoveCommand` / 村民追従 |
| 動物徘徊（Deer） | ✅ | Phase 28 — `PassiveAnimalLocomotionManager` wander |
| 資源ノード枯渇 | ✅ | 色変化・採集不可 |
| 共有木の競合採集 | ✅ | Phase 9/10（先に切った側が取得） |
| Lumber Camp | ✅ | 100 Wood / 6 秒 / Wood Drop-off 拠点 |
| 市場・交易 | ✅ | Phase 45 — Market + Food/Wood/Stone ↔ Gold 固定レート |
| 文明ボーナス | ✅ | Phase 46 — `CivilizationData` + Player 採集 +10%（既定）/ 歩兵 HP ボーナス対応 |
| 2 台目 TC | ✅ | Phase 47 — Feudal 以降 275W+100S / チーム最大 2 基 / 最寄り TC 搬入 |
| CPU ペース切替 | ✅ | Play 中 `P` または HUD ボタンで Relaxed ↔ Aggressive |
| Debug テスト | ✅ | Debug 時 **K**=選択 TC **または** 自軍 Placed Building に 150dmg / **Shift+K**=CPU ウェーブ強制 |
| 農業（Farm） | ✅ | 60 Wood / 8 秒 / 250 Food 容量・枯渇で Pool 返却 |
| 漁業 | ❌ | |

### Building

| 機能 | 状態 | 備考 |
|------|------|------|
| TownCenter | ✅ | Villager 生産（1 キュー） |
| House | ✅ | 25 Wood / 3 秒 / +5 Pop |
| Barracks | ✅ | 50 Wood / 5 秒 |
| Farm | ✅ | 60 Wood / 8 秒 / HP 100 / Pop +0 |
| Lumber Camp | ✅ | 100 Wood / 6 秒 / HP 400 / Pop +0 |
| Mining Camp | ✅ | 100 Wood / 6 秒 / Gold+Stone Drop-off 拠点 |
| Mill | ✅ | 100 Wood / 6 秒 / Food Drop-off 拠点 |
| 配置ゴーストプレビュー | ✅ | 有効/無効色 |
| Villager による建築 | ✅ | 現場移動 → 建築タイマー（**1 人/サイト** — Phase 64 で複数人加速） |
| 建築中断（移動命令） | ✅ | Wood 返金なし |
| 建物修理 | ❌ | Phase 63 予定 |
| CPU 自動建築 | ✅ | House / Barracks（AI） |
| 建築破壊・Pop 減少 | ✅ | Phase 48 — House 破壊時 `PopulationManager.RemoveHousing` |
| 壁・塔（配置・HP） | ✅ | Phase 44 — Palisade / Stone Wall / Watch Tower |
| 壁通行遮断 | ✅ | Phase 49 — `WallOccupancyRegistry` |
| 壁 AoE2 型ドラッグ連続 | ✅ | Phase 49 — ドラッグ確定でセグメント列 |
| 壁ドラッグ列ゴースト | ✅ | Phase 52 — `IPlacementPreviewView` ドラッグ中プレビュー |
| 城門（Gate） | ✅ | Phase 49 — 自軍通過可 / 敵軍ブロック |
| 時代別壁グレード | ✅ | Phase 50 — Dark: Palisade / Feudal: Stone Wall + Gate（`CanBuild`） |
| 複数建築タイプ（修道院等） | ❌ | |

### Combat

| 機能 | 状態 | 備考 |
|------|------|------|
| 近接攻撃（右クリック） | ✅ | `AttackManager` |
| ダメージ計算（攻撃力 − 装甲） | ✅ | 最低 1 |
| 攻撃クールダウン | ✅ | Militia 1 秒 |
| 攻撃接近リング散開 | ✅ | `UnitPositionOffsets` |
| 死亡（Pool 返却） | ✅ | HP ≦ 0 → `UnitPool.Return`（Destroy 禁止） |
| HP バー（選択時） | ✅ | `UnitHpBarView`（OnGUI）— **自軍のみ** / 敵は Phase 62 |
| 攻撃中色変化 | ✅ | オレンジ系ティント |
| 建築 HP / TC 破壊 | ✅ | `BuildingHealth`（Phase 11） |
| 遠距離攻撃（弓・投石） | ❌ | |
| 自動反撃 / 警戒 AI | ✅ | Phase 29 — Idle Militia 自動攻撃（検知 5m、`[AutoAggro]` ログ、Player / CPU 対称） |
| スプラッシュ・貫通 | ❌ | |
| ユニットアップグレード | ❌ | |
| 勝敗判定 UI | ✅ | `VictoryDefeatHudView`（VICTORY / DEFEAT、R で再読み込み） |

### AI

| 機能 | 状態 | 備考 |
|------|------|------|
| CPU 経済 AI | ✅ | `CpuEconomyAiManager`（2 秒評価間隔） |
| CPU Villager 木採集自動割当 | ✅ | |
| CPU 4 資源経済 | ✅ | Phase 30 — Berry/Hunt/Gold/Stone + Mill/Mining Camp/Farm、`CpuAiCoordination` |
| CPU House 建築 | ✅ | Wood 余裕・Pop 逼迫時 |
| CPU Villager 増産 | ✅ | 目標 6 体 |
| CPU 軍事 AI | ✅ | `CpuMilitaryAiManager` |
| CPU Barracks 建築 | ✅ | Relaxed: 90秒後 / Aggressive: Debug 倍率 |
| CPU Militia 生産 | ✅ | 目標 8 体（拠点待機。攻撃波は兵種ごと最大2体） |
| CPU 攻撃波 | ✅ | Relaxed: **2分猶予** → **5分毎**、各兵種最大 **2体** / `AoE → CPU Attack Pace` |
| 難易度・文明差 | ❌ | |
| ビルドオーダー最適化 | ❌ | ルールベース MVP |
| スカウト・ラッシュ判断 | ❌ | |

### UI / HUD（横断）

| 機能 | 状態 | 備考 |
|------|------|------|
| Wood / Pop 表示 | ✅ | `ResourceHudView` |
| Food 表示 | ✅ | `ResourceHudView` / `CpuHudView` |
| Gold 表示 | ✅ | `ResourceHudView` / `CpuHudView` |
| Stone 表示 | ✅ | `ResourceHudView` / `CpuHudView` |
| CPU Wood / Pop | ✅ | `CpuHudView`（Phase 9/10） |
| ゲーム時間・波カウントダウン | ✅ | `GameTimeHudView`（Phase 10） |
| TC / Barracks 生産パネル | ✅ | OnGUI — Q キー + キュー行クリック取消 |
| 生産キュー UI | ✅ | Phase 31 + Phase 48 取消 — 先頭プログレス + キュー長 + 返金 |
| ユニット表示名 | ✅ | Phase 48 — `UnitDisplayNameUtility` / `ProductionQueueDisplayUtility` |
| 文明ラベル | ✅ | Phase 46 — HUD 文明名 |
| Idle カウント HUD | ✅ | Phase 32 — `IdleUnitHudView` |
| 選択詳細パネル | ✅ | Phase 25 |
| 日本語 Localization | ✅ | Phase 51 — LanguageMap + `L` キー切替 |
| 本格 UI（uGUI / UI Toolkit） | ✅ | 主要 HUD uGUI ✅ Phase 53 — OnGUI 残: 選択ボックス / HP バー / CPU Debug |

### Engine Foundation（Phase 11〜16）

| 機能 | 状態 | 備考 |
|------|------|------|
| Victory / Defeat | ✅ | TC 破壊、`GameSessionManager` |
| UnitPool / BuildingPool | ✅ | 死亡・Despawn は Return。TC / Tree は除外 |
| Benchmark シーン | ✅ | `Benchmark.unity`、50〜800 体、FPS / GC HUD |
| Spatial Hash | ✅ | `UnitSpatialIndex` / `TreeSpatialIndex` |
| Fixed Tick | ✅ | `SimulationTick` 20 TPS、`ISimulationTickable` |
| Command Queue | ✅ | プレイヤー操作 11 種（GatherGold / GatherStone 含む）。`CommandLog` 記録 |
| CPU AI Command 化 | ❌ | CPU は Manager 直接呼び出し（将来 Phase） |
| Replay 再生 | ❌ | CommandLog のみ。ファイル保存・再生なし |
| Entity ID | ❌ | GameObject 参照のまま |

---

## 4. Systems Overview

### システム一覧

```
Input / HUD（可変フレーム）
    ↓ Enqueue
CommandQueue ── Tick 先頭 Execute ──→ GatherManager / AttackManager / ...
    ↓
SimulationTick（20 TPS）
    ↓
┌─────────────────┼─────────────────┐
↓                 ↓                 ↓
UnitManager   GatherManager    AttackManager
(移動)         (採集)            (戦闘)
↓                 ↓
ProductionManager  BuildingPlacementManager
BarracksProductionManager
↓
ResourceManager / PopulationManager
↓
CpuEconomyAiManager / CpuMilitaryAiManager（直接 Manager 呼び出し）
```

---

### Camera System

| 項目 | 内容 |
|------|------|
| **目的** | RTS 俯瞰操作（移動・ズーム・初期視点） |
| **主要クラス** | `RTSCameraController`, `RTSInputReader` |
| **関連データ** | なし（SerializeField パラメータ） |
| **データフロー** | `RTSInputReader` → キーボード/端スクロール/ズーム → `transform` 更新 |

---

### Input System

| 項目 | 内容 |
|------|------|
| **目的** | 選択・命令・カメラ・生産ショートカットの入力集約 |
| **主要クラス** | `RTSInputReader`, `RTSInputActionsBuilder`, `RTSInputActionsFactory` |
| **関連データ** | `Assets/Input/RTSInputActions.inputactions`（Editor 生成） |
| **データフロー** | InputActionAsset → `RTSInputReader` プロパティ → Selection / Camera / UI |

**アクション:** Select, Command, MoveCamera, Zoom, PointerPosition, TrainVillager, BuildHouse, BuildBarracks

---

### Selection System

| 項目 | 内容 |
|------|------|
| **目的** | ユニット・建築の選択、右クリック命令の振り分け |
| **主要クラス** | `SelectionManager`, `SelectionBoxView`, `GroupMoveFormation` |
| **関連データ** | レイヤーマスク `GameLayers` |
| **データフロー** | Raycast でターゲット解決 → `CommandQueue.Enqueue` → Tick 先頭で Manager 実行 |

**命令優先:** 木（Resource）→ 敵 Unit（攻撃）→ 地面（移動）

---

### Unit System

| 項目 | 内容 |
|------|------|
| **目的** | ユニット状態・HP・移動・ビジュアル・生死 |
| **主要クラス** | `Unit`, `UnitManager`, `UnitPool`, `UnitData`, `UnitTeam`, `UnitState`, `UnitPositionOffsets`, `UnitSpawner` |
| **関連データ** | `UnitData`（DefaultUnit=Villager, MilitiaData, EnemyDummyData） |
| **データフロー** | `UnitManager.TickSimulation` → 全 Unit `TickMovement` / 死亡時 `Die()` → `UnitPool.Return` |

**重要:** `Unit` に `Update()` なし。移動は Manager 一括。

---

### Economy System

| 項目 | 内容 |
|------|------|
| **目的** | 資源・採集・人口の管理 |
| **主要クラス** | `ResourceManager`, `GatherManager`, `LumberCampRegistry`, `PopulationManager`, `TreeResource`, `ResourceNodeData` |
| **関連データ** | `ResourceNodeData`（DefaultTree） |
| **データフロー** | 採集命令 → GatherJob（MoveToTree → Gather → MoveToDeposit）→ `ResourceManager.AddWood(team)` |

**定数:** 搬送量 10、採集速度 2.5/秒

---

### Building & Production System

| 項目 | 内容 |
|------|------|
| **目的** | 建築配置・建設・TC/Barracks からのユニット生産 |
| **主要クラス** | `BuildingPlacementManager`, `ProductionManager`, `BarracksProductionManager`, `TownCenter`, `House`, `Barracks`, `Farm`, `LumberCamp`, `LumberCampRegistry`, `RuntimeBuildingFactory`, `PlacedBuildingDataResolver` |
| **関連データ** | `BuildingData`（TC）, `PlacedBuildingData`（House, Barracks, Farm, LumberCamp） |
| **データフロー** | HUD ボタン → 配置モード → クリック確定 → ConstructionSite → 完成時 Factory で建築生成 / Pop 加算 |

**生産キュー:** TC・Barracks 等 **FIFO 最大 15**（Phase 31）。Phase 48 で **取消 + 返金**（`ProductionQueueRefundUtility`）

**Editor:** `AoE → Sync Input Actions` — Phase10 優先で Input 配線

---

### Combat System

| 項目 | 内容 |
|------|------|
| **目的** | 攻撃ジョブ管理・ダメージ・接近移動・スタンス・攻撃移動 |
| **主要クラス** | `AttackManager`, `CombatDamageResolver`, `UnitAggroManager`, `AttackMoveManager`, `UnitStancePanelView` |
| **関連データ** | `UnitData`（attack, attackDamageType, meleeArmor, pierceArmor, armorClass, attackRange, attackCooldown） |
| **データフロー** | `IssueAttack` → AttackJob → `CombatDamageResolver.Resolve` → ダメージ（Melee/Pierce 装甲 + ボーナス） |

**ログ形式:** `[Player/CPU] 攻撃者 → [Player/CPU] 対象: 15 (3+12) (Melee) (HP x/y)` — Phase 39 Counter System

**Phase 40:** `UnitCombatStance`（Aggressive / Defensive / Stand Ground）— Stand Ground = 射程内 Aggro のみ・追撃なし。攻撃移動 = **A ホールド + 右クリック地面** → `AttackMoveCommand` → 移動中も Aggro。

**Phase 41:** `FormationMoveManager` — ソフト隊列（移動中スロット維持）。`UnitSeparation` — CanAttack 移動中ユニットの軽量押し出し。CPU 攻撃波 = 兵種内訳ログ + 隊列前進。

---

### AI System

| 項目 | 内容 |
|------|------|
| **目的** | CPU の経済・軍事をルールベースで自動化 |
| **主要クラス** | `CpuEconomyAiManager`, `CpuMilitaryAiManager` |
| **関連データ** | `PlacedBuildingData`（house, barracks） |
| **データフロー** | 定期 Evaluate → Wood/Pop/ユニット数チェック → Gather/Build/Train/Attack 命令 |

| AI | 評価間隔 | 主な定数 |
|----|----------|----------|
| 経済 | 2 秒 | Villager 目標 6、House 半径 8〜24 |
| 軍事 | 2 秒（建築・生産）、波 30 秒 | Militia 目標 8、Barracks 開始 60 秒、波最低 1 体 |

---

### UI System

| 項目 | 内容 |
|------|------|
| **目的** | 資源・生産・HP・CPU 情報・ゲーム時間の表示 |
| **主要クラス** | `ResourceHudView`, `ProductionPanelView`, `BarracksPanelView`, `UnitHpBarView`, `CpuHudView`, `GameTimeHudView`, `GameUiInput` |
| **関連データ** | `PlacedBuildingData` 参照（ランタイム Resolver で補完） |
| **データフロー** | OnGUI 毎フレーム描画 / ボタン → Manager API 呼び出し |

---

## 5. Scene Structure

各 Phase シーンは **累積的に機能を追加**する検証用ステージ。`AoE → Setup PhaseN Scene`（Editor）で再生成可能。

| シーン | 役割 | 主な固有要素 |
|--------|------|-------------|
| **Phase1** | 操作基盤の最小検証 | ユニット 1 体、カメラ、選択、移動 |
| **Phase2** | 複数ユニット操作 | 複数 Villager、矩形選択、グループ移動 |
| **Phase3** | 生産開始 | Player TownCenter、Villager 生産 |
| **Phase4** | 経済開始 | 木 8 本、GatherManager、Wood HUD |
| **Phase5** | 建築開始 | House 配置、BuildingPlacementManager |
| **Phase6** | 人口 | PopulationManager、Pop HUD |
| **Phase7** | 軍事開始 | Barracks、Militia、Enemy Dummy 2 体（静止テスト用） |
| **Phase8** | 戦闘完成 | 死亡・HP バー（Phase 7 構成 + 戦闘完成） |
| **Phase9** | CPU 経済 | CPU TC（z=-35）+ Villager 3、CpuEconomyAiManager、CpuHudView、共有木 |
| **Phase10** | **最終統合プレイ** | Phase 9 + CpuMilitaryAiManager + GameTimeHudView + 木 28 本前後 |

### Phase10 マップ構成（代表値）

| 要素 | 配置 |
|------|------|
| Player TownCenter | (0, 0, 0) |
| CPU TownCenter | (0, 0, -60) |
| カメラ焦点 | (0, 0, -30) 俯瞰 |
| 開始資源（Classic） | 両チーム Food **200** / Wood **200** / Gold **0** / Stone **0** |
| Player 初期 Villager | **3 体**（Player TC 付近） |
| CPU 初期 Villager | **3 体**（CPU TC 付近） |

### Systems オブジェクト（Phase 10）

`UnitManager`, `AttackManager`, `PopulationManager`, `ProductionManager`, `BarracksProductionManager`, `ResourceManager`, `GatherManager`, `BuildingPlacementManager`, `CpuEconomyAiManager`, `CpuMilitaryAiManager`, `SelectionManager`（各種 View コンポーネント付き）

---

## 6. Data Model

### ScriptableObject 一覧

| 型 | アセット例 | 役割 |
|----|-----------|------|
| **UnitData** | `DefaultUnit`（Villager）, `MilitiaData`, `EnemyDummyData` | HP・速度・攻撃・装甲・射程・色 |
| **BuildingData** | `TownCenterData` | TC 表示名・Villager 生産時間・スポーン位置 |
| **PlacedBuildingData** | `HouseData`, `BarracksData`, `FarmData`, `LumberCampData` | 配置建築のコスト・建築時間・フットプリント・訓練ユニット・Farm foodCapacity |
| **ResourceNodeData** | `DefaultTree` | 木の初期 Wood 量・色 |

### 主要パラメータ（現行バランス）

| データ | 値 |
|--------|-----|
| Villager | HP 100, 速度 5, 非戦闘 |
| Militia | HP 40, 攻撃 4, 装甲 0, 射程 2, CD 1 秒, コスト 20 Wood / 3 秒 |
| Enemy Dummy | Militia 相当（Phase 7/8 テスト用） |
| House | 25 Wood, 3 秒, +5 Pop |
| Barracks | 50 Wood, 5 秒 |
| Farm | 60 Wood, 8 秒, Food 容量 250, HP 100 |
| Lumber Camp | 100 Wood, 6 秒, HP 400 |
| Tree | 初期 Wood 100 |
| 初期 Pop cap | 5（TC 分） |

### ランタイム解決

シーン上の `houseData` / `barracksData` 参照が `{fileID: 0}` の場合、`PlacedBuildingDataResolver` が `Assets/Data/` からロードして補完する。

### チームモデル

```csharp
enum UnitTeam { Player = 0, Enemy = 1 }
```

建築・資源・人口はチーム別。4 チーム拡張は未実装。

---

## 7. AoE2 Feature Coverage

| 機能 | AoE2 | 本プロジェクト | ロードマップ |
|------|------|----------------|--------------|
| **資源** | Wood, Food, Gold, Stone | 4 資源 + Drop-off ✅ | M2.5 完了 |
| **建築** | 多数・時代進化 | TC / House / Barracks / Farm / Camps / Mill ✅ | M4 |
| **人口** | Pop cap / House | Pop cap ✅ / 破壊時減少 ✅（Phase 48） | M4 完了 |
| **文明** | 各国固有ボーナス | `CivilizationData` + 採集/歩兵 HP ボーナス MVP ✅（Phase 46） | M4 Phase 47+ |
| **研究** | 鍛冶屋・大学 | Blacksmith + Infantry Upgrade ✅（Phase 43）/ 大学 ❌ | M4 Phase 44+ |
| **軍事** | 歩兵・弓・騎兵・攻城等 | Militia 1 種 ✅ | M3 Phase 36〜41 |
| **AI** | 経済・軍事・難易度 | CPU 4 資源 + Militia 波 ✅ | M3 CPU 拡張 |
| **戦闘** | 遠近・装甲・相性 | 近接/遠距離 + Melee/Pierce 装甲 + Spearman 対騎兵 ✅（Phase 39） | M4+ |
| **フォーメーション** | 隊列・スタンス | スタンス + 攻撃移動 ✅（Phase 40）/ 隊列 ✅（Phase 41） | M4 Phase 42+ |
| **壁** | 石壁・塔・Gate | 遮断・Gate・ドラッグ列 ✅ / 列ゴースト ✅ Phase 52 / 時代別 ✅ Phase 50 |
| **Localization** | 多言語 UI | ✅ Phase 51 — EN/JA LanguageMap |
| **船** | 海上戦・貿易 | ❌ | [11_DEFERRED](11_DEFERRED_EXTENSION_DESIGN.md) |
| **市場** | 資源交易 | Market + 固定レート売買 ✅（Phase 45） | M4 Phase 46+ |
| **テクノロジー** | Dark→Imperial | Feudal 昇格 ✅（Phase 42）/ Blacksmith 研究 1 系統 ✅（Phase 43） | M4 Phase 44+ |
| **マルチプレイ** | LAN/オンライン | ❌（基盤 40〜50%） | M6 Phase 58〜66 |
| **4 人 / 2v2** | 1H+3CPU / 同盟 | ❌ | M6 Phase 59〜60 |
| **敵 HP 表示** | 敵選択・識別 | ❌（敵選択不可） | M6 Phase 62 |
| **建物修理** | Villager 修理 | ❌ | M6 Phase 63 |
| **複数人建設** | 建築速度加速 | ❌（1人/サイト） | M6 Phase 64 |
| **Fog of War** | 視界制限 | ❌ | M6 Phase 65 |
| **大型マップ** | 4 隅スポーン | △ Phase10 小 | M6 Phase 61 |
| **マップ** | ランダムマップ | 固定 Plane（Phase 35 で拡大） | ✅ Sandbox / ランダムは M8 |
| **勝敗** | 征服・遺跡等 | TC 破壊 ✅ | 拡張フック定義済み |
| **リプレイ** | あり | CommandLog のみ △ | オプション（後回し） |
| **UI** | 本格 HUD・ミニマップ | OnGUI MVP ✅ / i18n ❌ | M5 Phase 51〜54 |

**総合（現状）:** Dark Age 経済 + 最小 RTS 操作 + Militia 戦。**M5 完了で AoE2 全体の約 50〜55%** を目標（§AoE2 Completion Analysis 投影表）。

---

## 8. Missing Features

### Critical（ゲームとして成立に必須）

| 機能 | 状態 |
|------|------|
| 勝敗条件 | ✅ Phase 11 |
| 4 資源経済 | ✅ M2〜M2.5 |
| Object Pooling | ✅ Phase 12 |
| Fixed Tick Simulation | ✅ Phase 15 |
| パスファインディング / 衝突回避 | ❌ 大規模時に詰まる（M7 候補） |

### Important（AoE2 らしさ・拡張性）

| 機能 | 説明 |
|------|------|
| 壁通行遮断・Gate | ✅ Phase 49 / **列ゴースト** ✅ Phase 52 |
| 時代別壁グレード | ✅ Phase 50 — Dark 柵 / Feudal 石壁+Gate |
| 日本語 UI | ✅ Phase 51 — LanguageMap + HUD 主要パネル |
| CPU 難易度・戦略バリエーション | 現状は Relaxed / Aggressive のみ |
| 本格 HUD（uGUI） | ✅ Phase 53 — 資源・生産・選択・勝敗・Idle / Canvas ✅ Phase 52 |
| ミニマップ | ✅ Phase 54 — `MinimapView` / `MapBounds` / TC アイコン / 視野矩形 / クリック移動 |
| ユニットアニメ | ✅ Phase 55 — `UnitAnimationView` / Animator MVP（Villager・Militia・Archer） |
| 4 チーム対応 | 憲法目標だが enum は 2 チームのみ — Phase 59 |
| 敵 HP 表示 | Phase 62 — 敵選択 + HP バー |
| 建物修理 | Phase 63 — `RepairManager` + 木材コスト |
| 複数 Villager 建設 | Phase 64 — `ConstructionSite` 複数 builder |
| Castle / Wonder | [11_DEFERRED](11_DEFERRED_EXTENSION_DESIGN.md) |

### Nice To Have

| 機能 | 説明 |
|------|------|
| 騎兵・攻城兵器 | ✅ 騎兵 MVP（Phase 38）/ 攻城 ❌ |
| 壁・塔・Gate | 遮断・Gate ✅ / 列ゴースト ✅ Phase 52 |
| 船・水上 | |
| マップ生成 | |
| セーブ / ロード | |
| リプレイ | |
| Animator 本格実装 | ✅ Phase 55 MVP — Villager / Militia / Archer（Idle・Walk・Gather・Attack） |
| ミニマップ | ✅ Phase 54 |
| 音声・BGM | |

---

## 9. Technical Debt

| 負債 | 原因 | 影響範囲 | 状態 / 改善案 |
|------|------|----------|-------------|
| **MVP 暫定バランス** | Phase 7〜 開発速度優先（例: Barracks 5s / Archery 40s） | コスト・建築時間の AoE2 非準拠 | △ [12_GAMEPLAY_BALANCE_MODE.md](12_GAMEPLAY_BALANCE_MODE.md) — M3 完了後に AoE2 正本 + Debug 分離 |
| **OnGUI ベース UI** | Phase 3〜 で迅速 MVP | 全 HUD・パネル。解像度・見切れ | ❌ 未解消 → uGUI / UI Toolkit 移行 |
| **Fixed Tick** | — | Simulation 全般 | ✅ Phase 15 完了（20 TPS） |
| **Instantiate / Destroy** | Phase 1 からの単純実装 | ユニット・建築 | ✅ Phase 12 Pool 導入済み |
| **Unit 移動の直線のみ** | NavMesh 禁止 | 障害物・建築回り | ❌ グリッド A* または Flow Field |
| **Static Singleton Manager** | 実装速度優先 | 全システム。テスト・マルチ非互換 | ❌ DI or Simulation World |
| **シーン参照 `{fileID: 0}`** | SceneBuilder / 手動生成混在 | 配置データの不安定さ | ❌ Setup メニュー統一 |
| **CPU AI の木探索** | 簡易実装 | 木追加・削除時 | △ Phase 14 `TreeSpatialIndex` で改善 |
| **リスト線形探索** | 小規模 MVP | AI・選択 | ✅ Phase 14 Spatial Hash 完了 |
| **GPU Instancing 未活用** | Placeholder Mesh + MaterialPropertyBlock | 描画コール増 | ❌ 同一メッシュ Instancing |
| **Command Queue（CPU 未対応）** | Phase 16 はプレイヤーのみ | Replay / Lockstep | △ プレイヤー ✅ / CPU ❌ |
| **Entity ID 未導入** | GameObject 参照 | ネットワーク・Replay 再生 | ❌ int ID 化 |
| **決定論性** | float 演算 | Lockstep | ❌ 固定小数 or 整数 Tick |

---

## 10. Performance Review

### Update 使用（`MonoBehaviour.Update` — 非 Simulation）

| クラス | 役割 |
|--------|------|
| `SimulationTick` | Fixed Tick ドライバ（accumulator → 各 `ISimulationTickable`） |
| `BuildingPlacementManager` | 建築ゴースト追従（建築進行は Tick） |
| `SelectionManager` | 入力・選択処理 |
| `RTSCameraController` | カメラ |
| `ProductionPanelView` | Q キー監視 |
| `VictoryDefeatHudView` | R キー再読み込み |
| `BenchmarkMetricsView` | 計測 HUD |

### TickSimulation 使用（`ISimulationTickable` — Simulation）

| クラス | 役割 |
|--------|------|
| `CommandQueue` | **Tick 先頭** — キュー消化 → Manager 呼び出し |
| `UnitManager` | 全 Unit 移動 + Spatial 更新 |
| `AttackManager` | 攻撃ジョブ |
| `GatherManager` | 採集ジョブ |
| `ProductionManager` | TC 生産タイマー |
| `BarracksProductionManager` | Barracks 生産タイマー |
| `BuildingPlacementManager` | 建築進行 Tick |
| `CpuEconomyAiManager` | 2 秒評価間隔 AI |
| `CpuMilitaryAiManager` | 攻撃波 + 2 秒評価 AI |

**Unit 個別 Update:** なし ✅（憲法準拠）

**OnGUI（毎フレーム描画）:** `ResourceHudView`, `ProductionPanelView`, `BarracksPanelView`, `SelectionBoxView`, `UnitHpBarView`, `CpuHudView`, `GameTimeHudView`

### Manager 構成

- 10+ の static singleton Manager が並列稼働
- ジョブは `List<T>` で保持（AttackJob, GatherJob, ProductionJob, ConstructionSite）
- ユニット列挙は `UnitManager.CopyUnitsTo` → 再利用 `List` バッファ（AI 側）

### GC 発生ポイント

| 箇所 | 内容 |
|------|------|
| `UnitPool.Return` | アクティブ Unit の非アクティブ化（Destroy なし） |
| 初回 Pool miss | `CreateFreshUnit`（Prewarm 後は reuse 増） |
| OnGUI | 内部アロケーション（レイアウト・Skin） |
| `CommandLog` / `Debug.Log` | 命令ログ文字列 |

### 将来のボトルネック（200 ユニット × 4 チーム想定）

1. **UnitManager** — O(n) 全 Unit 移動毎 Tick
2. **AttackManager / GatherManager** — O(jobs) 線形走査
3. **描画** — Unit ごと MaterialPropertyBlock（Instancing 未使用）
4. **OnGUI** — 本番スケール不向き
5. **AI** — Spatial Hash 経由だが 800 体級は未実測

**現状の実用規模:** 数十ユニット程度なら問題なし。憲法目標の 800 ユニット級には構造変更が必要。

**FPS 実測値:** 未計測 — [Performance Benchmark](#performance-benchmark) を参照。

---

## 11. Multiplayer Readiness

| 要素 | 状態 | 評価 |
|------|------|------|
| **Fixed Tick Simulation** | ✅ | 20 TPS。Game Over 中停止 |
| **Command Queue（プレイヤー）** | ✅ | 9 種 Command。Tick 先頭 Execute |
| **Command Log** | △ | `CommandLog` 記録あり（EntityId 付き — Move / AttackUnit）。再生・保存なし |
| **Command Queue（CPU）** | ✅ | `CpuEconomyAiManager` / `CpuMilitaryAiManager` → `CpuAiCommandQueue` |
| **Deterministic Simulation** | ❌ | float 演算・Tickable 登録順 |
| **Turn / Lockstep** | ❌ | 未設計 |
| **Replay 再生** | ❌ | 未実装 |
| **State Snapshot** | ❌ | セーブデータ構造なし |
| **Network Layer** | ❌ | 憲法で現時点禁止 |
| **Team / Player ID** | △ | `PlayerId` 0〜3 + per-player 資源/人口/TC（Phase 59）。`UnitTeam` は Player/Enemy 2 値のまま |
| **Entity ID** | ✅ | `EntityRegistry` — Unit / Building / Resource。Move・AttackUnit は ID 参照 |
| **Simulation / View 分離** | △ | Manager に OnGUI View 混在 |

**現状評価:** ローカル 1v1 CPU として動作。**M6 は 4 人・2v2・大マップ・Fog を優先**（リプレイ・ホットシートは後回し）。詳細は [10_M6_MULTIPLAYER_FOUNDATION.md](10_M6_MULTIPLAYER_FOUNDATION.md)。

---

## 12. Future Roadmap

Phase 11 以降の候補（優先度順）。

| 優先度 | 候補 | 状態 |
|--------|------|------|
| P0 | 勝敗 UI・ゲーム終了条件 | ✅ Phase 11 |
| P0 | Fixed Tick + Command Queue 基盤 | ✅ Phase 15〜16 |
| P0 | Object Pooling | ✅ Phase 12 |
| P1 | Food 資源 + 農場 | ✅ Phase 17〜18（M2） |
| P1 | 採取リピート + Drop-off + 狩り + 選択 UI | ✅ M2.5 |
| P1 | RTS UX（キュー・Idle・Rally・Control Group） | ✅ M2.6 |
| P1 | CPU 4 資源経済 | ✅ Phase 30 |
| P1 | Phase10 サンドボックス拡張 | ✅ Phase 35 |
| P1 | 弓兵・騎兵・相性（Archery Range / Stable） | ✅ M3 Phase 36〜41 |
| P1 | 壁・Gate + i18n + 本格 HUD | ⬜ M5 Phase 49〜54 |
| P2 | 時代昇格 / 鍛冶屋 / 市場 / 文明 | ✅ M4 Phase 42〜48 |
| P2 | 壁時代グレード | ✅ M5 Phase 50 |
| P1 | Entity ID & PlayerId | ✅ M6 Phase 57 |
| P2 | CPU Command 化 | ✅ M6 Phase 58 |
| P3 | 4 人（1H+3CPU） | ✅ M6 Phase 59 |
| P3 | 2v2 同盟 | M6 Phase 60 — **次** |
| P4 | 大型マップ | M6 Phase 61 |
| P4 | 敵 HP 表示 | M6 Phase 62 |
| P4 | 建物修理 | M6 Phase 63 |
| P4 | 複数 Villager 建設 | M6 Phase 64 |
| P4 | Fog of War | M6 Phase 65 |
| P5 | 決定論（LAN 前） | M6 Phase 66 |
| — | リプレイ / ホットシート | オプション（後回し） |
| — | 本格オンラインマルチ | M9 以降 |

---

## Performance Benchmark

> **目的:** 現在の性能状態と、今後ベンチマークを取るべき規模を AI が判断できるようにする。
>
> **現状:** Phase 13 で **Benchmark シーン・計測 HUD** を追加済み。FPS 数値は **Editor で `Benchmark.unity` を Play して取得**（リポジトリ内の固定値は未記載）。

### Test Environment

| 項目 | 値 |
|------|-----|
| **Unity Version** | Unity 6（README 記載）。具体パッチ: **TBD**（例: 6000.x） |
| **CPU** | TBD |
| **RAM** | TBD（目標スペック: MacBook Air **16 GB** — [CONSTITUTION.md](../CONSTITUTION.md)） |
| **GPU** | TBD |
| **OS** | TBD |
| **Test Scene** | **`Assets/Scenes/Benchmark.unity`**（`AoE → Setup Benchmark Scene`） |
| **Render Pipeline** | URP（プロジェクト標準） |
| **VSync** | TBD |
| **Quality Settings** | TBD |

### Test Method（Phase 13 — Benchmark シーン）

1. **セットアップ:** `AoE → Setup Benchmark Scene`（Edit モード）
2. **シーン:** `Benchmark.unity` を Play
3. **負荷:** 左上 HUD の **50 / 100 / 200 / 500 / 800** ボタンで Villager を Idle 一括スポーン
4. **計測:** 画面上 **FPS / Frame ms / GC KB per frame**（30 フレーム移動平均 + `ProfilerRecorder`）
5. **Clear:** Pool 返却後、別プリセットで再計測
6. **Mixed（実プレイ）:** `Phase10.unity` — 従来どおり手動プレイ + Profiler

### FPS Table

| Unit Count | Idle FPS | Move FPS | Combat FPS | Mixed FPS | 備考 |
| ---------- | -------- | -------- | ---------- | --------- | ---- |
| 50         | TBD      | TBD      | TBD        | TBD       | 現 Phase 10 自然プレイはおおむねこの程度 |
| 100        | TBD      | TBD      | TBD        | TBD       | |
| 200        | TBD      | TBD      | TBD        | TBD       | 憲法目標 **1 チームあたり** の上限 |
| 500        | TBD      | TBD      | TBD        | TBD       | 中間スケールストレステスト |
| 800        | TBD      | TBD      | TBD        | TBD       | 憲法目標 **4 チーム × 200** 相当 |

### どの規模で性能検証を行うべきか

| 優先度 | 規模 | 理由（コード・ドキュメント根拠） |
|--------|------|----------------------------------|
| **P0** | **50〜100** | Phase 10 実プレイ（CPU + プレイヤー合計で数十体）の快適性確認。現状の Manager 数（§10）で十分意味がある |
| **P1** | **200 / チーム** | [CONSTITUTION.md](../CONSTITUTION.md) の明示目標「4 チーム × 200 ユニット」の **1 チーム分**。`UnitManager.Update` の O(n) 移動がボトルネック化し始める規模（§10） |
| **P1** | **800 合計** | 憲法の最終目標規模。4 チーム分の Pop・Wood・AI 走査が重なる |
| **P2** | **500** | 200→800 の中間。Pooling / Instancing 導入前後の比較用 |
| **P2** | Mixed パターン | `CpuEconomyAiManager` / `CpuMilitaryAiManager` の `UnitManager.CopyUnitsTo` 全走査（§10）を含む **実戦相当** 負荷 |

**注意:** Benchmark シーンは Phase 13 で実装済み。FPS 数値は Editor Play で取得し、下表に記録する（現状 TBD）。

---

## Core Dependency Graph

> **目的:** AI が変更の影響範囲を把握できるようにする。
>
> **根拠:** `Assets/Scripts/**/*.cs` 内の **static メソッド呼び出し・型参照** のみ。矢印は「呼び出し側 → 被呼び出し側」。

### 凡例

- **Manager** — `MonoBehaviour` + static singleton API
- **View** — OnGUI / 入力表示（Simulation ではないが命令発行元）
- **Entity** — シーン上の `Unit` / `TownCenter` / `Barracks` 等

---

### Input → Selection → Command 系統

```
RTSInputReader
└─ SelectionManager
   ├─ UnitSpatialIndex                   QueryInWorldBounds（矩形選択）
   ├─ CommandQueue                       Enqueue（Move / Attack / Gather / BuildConfirm）
   └─ BuildingPlacementManager           CancelPlacementMode（UI 即時）

ResourceHudView
└─ BuildingPlacementManager             EnterHouse/BarracksPlacementMode（UI 状態）

ProductionPanelView / BarracksPanelView
└─ CommandQueue                         TrainVillager / TrainMilitia

CommandQueue（Tick 先頭）
└─ GatherManager | AttackManager | GroupMoveFormation | BuildingPlacementManager
   | ProductionManager（Train 経由 TownCenter/Barracks）
```

---

### Simulation Managers（ゲームロジック中核）

```
SimulationTick（20 TPS）
├─ CommandQueue                        Tick 先頭
├─ UnitManager                         TickMovement + UnitSpatialIndex 更新
├─ AttackManager
├─ GatherManager
├─ ProductionManager
├─ BarracksProductionManager
├─ BuildingPlacementManager            建築進行
├─ CpuEconomyAiManager
└─ CpuMilitaryAiManager

Unit                                    （Entity）
├─ UnitManager                         Register / Unregister
├─ UnitPool                            Return（Die 時）
├─ AttackManager                       IsUnitAttacking
├─ GatherManager                       CancelForUnit（Die 時）
├─ BuildingPlacementManager            AbortConstructionForUnit（Die 時）
└─ SelectionManager                    HandleUnitDied（Die 時）

AttackManager
├─ GatherManager                       CancelForUnits（攻撃開始時）
└─ Unit                                IsNear, SetMoveTarget, TakeDamage

GatherManager
├─ ProductionManager                   GetTownCenterForTeam
└─ ResourceManager                     AddWood（搬入時）

ProductionManager / BarracksProductionManager
├─ PopulationManager                   CanTrainUnit
├─ ResourceManager                     TrySpendWood（Barracks）
└─ UnitSpawner → UnitPool.Rent         Spawn（生産完了時）

BuildingPlacementManager
├─ ResourceManager                     TrySpendWood
├─ BuildingPool                        House / Barracks 完成
└─ PopulationManager                   AddHousing（House 完成時）

PopulationManager
└─ UnitManager                         GetUnitCountForTeam
```

---

### AI → Economy → Combat 系統

```
CpuEconomyAiManager
├─ ProductionManager                   GetTownCenterForTeam, GetQueueCount, IsProducing
├─ GatherManager                       IssueGatherCommand
├─ ResourceManager                     GetWood
├─ PopulationManager                   GetCurrentPopulation, GetMaxPopulation, CanTrainUnit
├─ BuildingPlacementManager            HasActiveConstructionForTeam, TryFindPlacementNear,
│                                    TryStartTeamConstruction, IsUnitBuilding
├─ UnitManager                         CopyUnitsTo
└─ TreeResource                        FindObjectsByType（木キャッシュ）

CpuMilitaryAiManager
├─ ProductionManager                   GetTownCenterForTeam
├─ BarracksProductionManager           HasBarracksForTeam, GetBarracksForTeam, GetQueueCount, IsProducing
├─ BuildingPlacementManager            HasActiveBarracksConstructionForTeam, TryFindPlacementNear,
│                                    TryStartTeamConstruction, IsUnitBuilding
├─ ResourceManager                     GetWood
├─ PopulationManager                   CanTrainUnit
├─ AttackManager                       IssueAttack（攻撃波）
├─ UnitManager                         CopyUnitsTo
└─ Barracks.TryQueueMilitiaProduction  → BarracksProductionManager
```

**CPU 攻撃波フロー（コード上）:**

```
CpuMilitaryAiManager.Update（waveTimer）
  → CollectCpuMilitia（UnitManager.CopyUnitsTo）
  → AttackManager.IssueAttack(cpuMilitia, nearestPlayerUnit)
    → GatherManager.CancelForUnits
    → AttackJob リスト追加
```

---

### View / HUD（読取専用依存）

```
CpuHudView
├─ ResourceManager                     GetWood(Enemy)
└─ PopulationManager                   GetCurrentPopulation, GetMaxPopulation

GameTimeHudView
├─ CpuMilitaryAiManager                Instance, WaveTimerRemaining, BarracksBuildDelayRemaining 等
├─ ResourceManager                     GetWood(Enemy)
├─ BarracksProductionManager         HasBarracksForTeam（static）
└─ BuildingPlacementManager            HasActiveBarracksConstructionForTeam

UnitHpBarView
└─ SelectionManager                    SelectedUnits

SelectionBoxView                        （SelectionManager から Rect 受取。Manager 非依存）
```

---

### 依存関係マトリクス（Manager 間・直接 static 呼び出し）

| 呼び出し元 ↓ / 先 → | UnitMgr | Attack | Gather | Prod | BarrProd | BuildPlace | Resource | Pop | UnitSpawn | BuildFactory |
|---------------------|---------|--------|--------|------|----------|------------|----------|-----|-----------|--------------|
| **SelectionManager** | ✓ | ✓ | ✓ | — | — | ✓ | — | — | — | — |
| **AttackManager** | — | — | ✓ | — | — | — | — | — | — | — |
| **GatherManager** | — | — | — | ✓ | — | — | ✓ | — | — | — |
| **ProductionManager** | — | — | — | — | — | — | — | ✓ | ✓ | — |
| **BarracksProductionManager** | — | — | — | — | — | — | ✓ | ✓ | ✓ | — |
| **BuildingPlacementManager** | ✓ | — | ✓ | — | — | — | ✓ | ✓ | — | ✓ |
| **PopulationManager** | ✓ | — | — | — | — | — | — | — | — | — |
| **CpuEconomyAiManager** | ✓ | — | ✓ | ✓ | — | ✓ | ✓ | ✓ | — | — |
| **CpuMilitaryAiManager** | ✓ | ✓ | — | ✓ | ✓ | ✓ | ✓ | ✓ | — | — |
| **Unit（Die 時）** | ✓ | — | ✓ | — | — | ✓ | — | — | — | — |

**AI 変更時の典型影響範囲:**

| 変更対象 | 直接影響 | 間接影響 |
|----------|----------|----------|
| `ResourceManager` | 全 Spend/Add Wood | Production, Placement, AI, HUD |
| `UnitManager` | 移動・Pop 集計・AI 列挙 | Selection, Population, 両 CPU AI |
| `AttackManager` | 戦闘・CPU 攻撃波 | Selection 命令, Unit ビジュアル |
| `BuildingPlacementManager` | 建築・CPU 建設 | Gather 中断, Pop, AI 建築判断 |

---

## AoE2 Completion Analysis

> **目的:** 「現在どこまで AoE2 に近いか」をカテゴリ別に量化する。
>
> **算出方法:** §3 Implemented Features と §7 AoE2 Feature Coverage の **✅/❌ 項目数** をベースに、各カテゴリ内で「実装済み項目 / AoE2 相当の主要項目」を比率化。主観の単独推定は避け、下表の **根拠列** に §3 参照項目を明示する。

### カテゴリ別 Completion

| Category | Completion | 根拠（§3 / §7 / コードから） |
| -------- | ---------- | --------------------------- |
| **Economy** | **45%** | Wood / Food / Gold / Stone 採集・搬入 ✅。市場・交易 ❌ |
| **Buildings** | **22%** | TC / House / Barracks / Farm / Lumber / Mining / Mill ✅。時代・防衛・軍事建築（Range/Stable）❌ |
| **Military** | **10%** | Militia + 簡易 Aggro ✅。弓・騎兵・相性・攻城 ❌ → M3 |
| **AI** | **15%** | CPU 4 資源 + Militia 波 ✅。新兵種・時代・難易度 ❌ |
| **Technology** | **0%** | 研究・時代昇格・鍛冶屋/大学 ❌（§3 / §7 該当項目すべて ❌） |
| **Civilizations** | **15%** | `CivilizationData` + チームボーナス 1 種 ✅（Phase 46）/ 固有ユニット・文明選択 UI ❌ |
| **Multiplayer** | **35%** | Fixed Tick ✅ / プレイヤー Command ✅ / CPU Command ❌ / Replay 再生 ❌ |
| **UX/UI** | **25%** | 選択・OnGUI HUD・キュー・Idle・Rally・Control Group・勝敗 ✅ → M5 で本格 UI |
| **Performance Foundation** | **65%** | Pool ✅ / Spatial Hash ✅ / Fixed Tick ✅ / Benchmark ✅ / Instancing ❌ |

### Overall Completion

```
Overall Completion（現状 Phase 34）: 15%
```

**算出式（カテゴリ均等重み 9 分の 1）— Phase 34 時点:**

| Category | % |
|----------|---|
| Economy | 45 |
| Buildings | 22 |
| Military | 10 |
| AI | 15 |
| Technology | 0 |
| Civilizations | 15 |
| Multiplayer | 35 |
| UX/UI | 25 |
| Performance Foundation | 65 |

`(45+22+10+15+0+0+35+25+65) / 9 ≈ 24%` — コアループ偏重で **実用体感 15%** と表記（Technology / Civ 未着手の重み付け）。

### マイルストン完了時の投影

| マイルストン | AoE2 全体 | コアループ（1v1 CPU） | マルチ準備度 | 憲法性能目標 |
|--------------|-----------|----------------------|--------------|--------------|
| **現状（M2.6）** | **~15%** | ~85% | 30〜40% | ~15% |
| M2.7 完了 | ~16% | ~88% | 30〜40% | ~15% |
| M3 完了 | ~22% | ~90% | 35% | ~18% |
| M4 完了 | ~38% | ~92% | 40% | ~20% |
| **M5 完了** | **~50〜55%** | **~90%** | **55〜60%** | **~30%** |
| M6 完了（57〜65） | ~62% | ~96% | **4人ローカル** | ~40% |

**M5 完了で 50% 前後**の内訳: Economy / Military / Technology / UX が大幅向上。未着手が大きい領域は **海軍・攻城・全文明・ランダムマップ・Fog・本格オンライン MP・800 体性能**。

### 解釈

| 観点 | 評価 |
|------|------|
| **コアループ** | 採集 → 建築 → 生産 → 戦闘は **垂直スライスとして成立** |
| **AoE2 全体** | M5 までで **約半分**。残り半分は海軍・攻城・全時代・マップ・本格 MP |
| **最も近い領域** | Economy、RTS 操作 UX |
| **最も遠い領域** | 海軍、全文明、ランダムマップ、本格オンライン MP |
| **UI と MP** | **UI（M5）≠ マルチプレイ可能**。M6 同期基盤が別途必要 |

### 現時点の位置づけ（M4 完了 / Phase 48）

```
AoE2 フル機能 ████████░░░░░░░░░░░░  ~38%
コアループ     ██████████████████░░  ~92%
マルチ準備     ████████░░░░░░░░░░░░  ~40%
憲法性能目標   ████░░░░░░░░░░░░░░░░  ~20%
```

---

## Appendix A: ソースディレクトリ

```
Assets/Scripts/
  AI/           CpuEconomyAiManager, CpuMilitaryAiManager
  Buildings/    Placement, Production, ProductionQueue*, BuildingHealth
  Economy/      Resource, Gather, ProductionQueueRefundUtility, PopulationManager
  Input/        RTSInputReader, RTSInputActionsBuilder
  Selection/    HUD Views, ProductionQueuePanelUi, SelectionManager
  Units/        Unit, UnitManager, UnitDisplayNameUtility, UnitDataResolver
  Core/         EntityRegistry, PlayerId, GameSessionManager
  Commands/     IGameCommand, CommandQueue, MoveCommand (EntityId), AttackUnitCommand (EntityId)
  Editor/       Phase SceneBuilder, Sync Input Actions
```

## Appendix B: AI への推奨読み方

1. 本ファイルで全体像を把握
2. [CONSTITUTION.md](../CONSTITUTION.md) で制約を確認
3. 変更対象 Phase の `docs/prompts/phaseN-prompt.md` を読む
4. `Assets/Scripts/` の該当 Manager を読んでから small diff

## Appendix C: クイック判断ガイド

| 質問 | 答え |
|------|------|
| AoE2 にどれくらい近い？ | 4 資源・Feudal 経済・多兵種・1 CPU — **Dark〜Feudal 垂直スライス**（全体 ~38%） |
| 何が一番足りない？ | 兵種多様性・時代・本格 UI・マルチ同期基盤 |
| 次に何を作るべき？ | **Phase 60 Team & 2v2** — [10_M6](10_M6_MULTIPLAYER_FOUNDATION.md) |
| M6 のゴールは？ | **人間1+CPU3・2v2・大マップ・敵HP/修理/複数建設・Fog**（57→58→59→60→61→62→63→64→65） |
| リプレイ・ホットシートは？ | **M6 スコープ外（後回し）** |
| M5 完了時の全体完成度？ | **約 50〜55%**（§AoE2 Completion Analysis 投影表） |
| プレイ用シーンは？ | **`Phase10.unity`** |
| 自軍は自動反撃？ | **する**（Phase 29 — 待機 Militia が近接敵を自動攻撃。Move 中はしない） |
| 性能ベンチマークは？ | **未計測（TBD）** — §Performance Benchmark 参照 |
| 変更の影響範囲は？ | §Core Dependency Graph 参照 |
| AoE2 全体の完成度は？ | **現状 ~38%（M4 完了）** / **M5 後 ~50〜55%** — §AoE2 Completion Analysis |

---

*Document generated from codebase state at Phase 10 completion. §Performance Benchmark FPS values are TBD until measured.*

# AoE RTS Engine — Implementation Status

> **用途:** このファイル単体を AI に渡すことで、現状の実装範囲・未実装・AoE2 との差分・技術構成・拡張方針を把握できる。
>
> **最終更新:** Phase 17 完了（Food 資源）。Foundation Milestone 1 完了。M2 Economy 開始。
>
> **関連:** [CONSTITUTION.md](../CONSTITUTION.md) / [README.md](../README.md) / [docs/README.md](README.md)  
> **ロードマップ:** [01_M0_POC_PHASES.md](01_M0_POC_PHASES.md) / [02_M1_FOUNDATION_PHASES.md](02_M1_FOUNDATION_PHASES.md) / [03_M2_ECONOMY_PHASES.md](03_M2_ECONOMY_PHASES.md)
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
| **対戦想定** | 現状は **1 人間（Player）vs 1 CPU（Enemy）** のローカルシングルプレイ。マルチプレイは未実装 |
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

**ゲームループ:** 採集 → 建築 → 生産 → 戦闘 → **勝敗判定**

**Foundation Milestone 1:** ✅ 完了（Phase 11〜16）

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
| 左クリック選択 | ✅ | Unit / TownCenter / Barracks |
| 右クリック命令 | ✅ | 木 / Berry / 移動 / 攻撃（CommandQueue 経由） |
| ドラッグ矩形選択 | ✅ | Phase 2 以降 |
| Shift 追加選択 | ✅ | |
| Q キー Villager 生産 | ✅ | TownCenter 選択時 |
| Esc / 右クリックで配置キャンセル | ✅ | House / Barracks 配置モード |
| ゲームパッド | ❌ | 未対応 |

### Selection

| 機能 | 状態 | 備考 |
|------|------|------|
| 単体選択 | ✅ | |
| 複数選択・矩形選択 | ✅ | `SelectionBoxView`（OnGUI） |
| 建築選択（TC / Barracks） | ✅ | |
| CPU ユニット・建築の選択不可 | ✅ | `UnitTeam.Enemy` フィルタ |
| 選択時色変更 | ✅ | MaterialPropertyBlock |
| グループ移動グリッド整列 | ✅ | `GroupMoveFormation`（√n グリッド） |
| フォーメーション / 隊列維持 | ❌ | 移動先でグリッド配置のみ |
| ホットキーグループ（Ctrl+数字） | ❌ | |

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
| Food 資源 | ✅ | Berry Bush → `FoodGatherManager` → TC 搬入 |
| Gold / Stone | ❌ | Phase 20 予定 |
| 木（Tree）採集 | ✅ | `GatherManager` + `GatherCommand` |
| Berry Bush 採集 | ✅ | `FoodGatherManager` + `GatherFoodCommand` |
| TownCenter への搬入 | ✅ | チーム別 TC |
| 資源ノード枯渇 | ✅ | 色変化・採集不可 |
| 共有木の競合採集 | ✅ | Phase 9/10（先に切った側が取得） |
| Lumber Camp | ❌ | |
| 市場・交易 | ❌ | |
| 農業・漁業 | ❌ | |

### Building

| 機能 | 状態 | 備考 |
|------|------|------|
| TownCenter | ✅ | Villager 生産（1 キュー） |
| House | ✅ | 25 Wood / 3 秒 / +5 Pop |
| Barracks | ✅ | 50 Wood / 5 秒 |
| 配置ゴーストプレビュー | ✅ | 有効/無効色 |
| Villager による建築 | ✅ | 現場移動 → 建築タイマー |
| 建築中断（移動命令） | ✅ | Wood 返金なし |
| CPU 自動建築 | ✅ | House / Barracks（AI） |
| 建築破壊・Pop 減少 | ❌ | House 破壊時 cap 減なし |
| 壁・塔・城門 | ❌ | |
| 複数建築タイプ（修道院等） | ❌ | |

### Combat

| 機能 | 状態 | 備考 |
|------|------|------|
| 近接攻撃（右クリック） | ✅ | `AttackManager` |
| ダメージ計算（攻撃力 − 装甲） | ✅ | 最低 1 |
| 攻撃クールダウン | ✅ | Militia 1 秒 |
| 攻撃接近リング散開 | ✅ | `UnitPositionOffsets` |
| 死亡（Pool 返却） | ✅ | HP ≦ 0 → `UnitPool.Return`（Destroy 禁止） |
| HP バー（選択時） | ✅ | `UnitHpBarView`（OnGUI） |
| 攻撃中色変化 | ✅ | オレンジ系ティント |
| 建築 HP / TC 破壊 | ✅ | `BuildingHealth`（Phase 11） |
| 遠距離攻撃（弓・投石） | ❌ | |
| 自動反撃 / 警戒 AI | ❌ | 自軍 Militia は手動右クリックのみ |
| スプラッシュ・貫通 | ❌ | |
| ユニットアップグレード | ❌ | |
| 勝敗判定 UI | ✅ | `VictoryDefeatHudView`（VICTORY / DEFEAT、R で再読み込み） |

### AI

| 機能 | 状態 | 備考 |
|------|------|------|
| CPU 経済 AI | ✅ | `CpuEconomyAiManager`（2 秒評価間隔） |
| CPU Villager 木採集自動割当 | ✅ | |
| CPU House 建築 | ✅ | Wood 余裕・Pop 逼迫時 |
| CPU Villager 増産 | ✅ | 目標 6 体 |
| CPU 軍事 AI | ✅ | `CpuMilitaryAiManager` |
| CPU Barracks 建築 | ✅ | 開始 60 秒後 + Wood 50 |
| CPU Militia 生産 | ✅ | 目標 8 体 |
| CPU 攻撃波 | ✅ | 30 秒毎、全 Militia に攻撃命令 |
| 難易度・文明差 | ❌ | |
| ビルドオーダー最適化 | ❌ | ルールベース MVP |
| スカウト・ラッシュ判断 | ❌ | |

### UI / HUD（横断）

| 機能 | 状態 | 備考 |
|------|------|------|
| Wood / Pop 表示 | ✅ | `ResourceHudView` |
| Food 表示 | ✅ | `ResourceHudView` / `CpuHudView` |
| CPU Wood / Pop | ✅ | `CpuHudView`（Phase 9/10） |
| ゲーム時間・波カウントダウン | ✅ | `GameTimeHudView`（Phase 10） |
| TC / Barracks 生産パネル | ✅ | OnGUI ボタン |
| 本格 UI（uGUI / UI Toolkit） | ❌ | すべて OnGUI MVP |

### Engine Foundation（Phase 11〜16）

| 機能 | 状態 | 備考 |
|------|------|------|
| Victory / Defeat | ✅ | TC 破壊、`GameSessionManager` |
| UnitPool / BuildingPool | ✅ | 死亡・Despawn は Return。TC / Tree は除外 |
| Benchmark シーン | ✅ | `Benchmark.unity`、50〜800 体、FPS / GC HUD |
| Spatial Hash | ✅ | `UnitSpatialIndex` / `TreeSpatialIndex` |
| Fixed Tick | ✅ | `SimulationTick` 20 TPS、`ISimulationTickable` |
| Command Queue | ✅ | プレイヤー操作 8 種（GatherFood 含む）。`CommandLog` 記録 |
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

**アクション:** Select, Command, MoveCamera, Zoom, PointerPosition, TrainVillager

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
| **主要クラス** | `ResourceManager`, `GatherManager`, `PopulationManager`, `TreeResource`, `ResourceNodeData` |
| **関連データ** | `ResourceNodeData`（DefaultTree） |
| **データフロー** | 採集命令 → GatherJob（MoveToTree → Gather → MoveToDeposit）→ `ResourceManager.AddWood(team)` |

**定数:** 搬送量 10、採集速度 2.5/秒

---

### Building & Production System

| 項目 | 内容 |
|------|------|
| **目的** | 建築配置・建設・TC/Barracks からのユニット生産 |
| **主要クラス** | `BuildingPlacementManager`, `ProductionManager`, `BarracksProductionManager`, `TownCenter`, `House`, `Barracks`, `RuntimeBuildingFactory`, `PlacedBuildingDataResolver` |
| **関連データ** | `BuildingData`（TC）, `PlacedBuildingData`（House, Barracks） |
| **データフロー** | HUD ボタン → 配置モード → クリック確定 → ConstructionSite → 完成時 Factory で建築生成 / Pop 加算 |

**生産キュー:** TC・Barracks とも **1 スロット**

---

### Combat System

| 項目 | 内容 |
|------|------|
| **目的** | 攻撃ジョブ管理・ダメージ・接近移動 |
| **主要クラス** | `AttackManager` |
| **関連データ** | `UnitData`（attack, armor, attackRange, attackCooldown） |
| **データフロー** | `IssueAttack(attackers, target)` → AttackJob リスト → Update で接近・クールダウン・ダメージ |

**ログ形式:** `[Player/CPU] 攻撃者 → [Player/CPU] 対象: N dmg (HP x/y)`

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
| CPU TownCenter | (0, 0, -35) |
| カメラ焦点 | (0, 0, -17) 俯瞰 |
| CPU 初期 Villager | 3 体（CPU TC 付近） |

### Systems オブジェクト（Phase 10）

`UnitManager`, `AttackManager`, `PopulationManager`, `ProductionManager`, `BarracksProductionManager`, `ResourceManager`, `GatherManager`, `BuildingPlacementManager`, `CpuEconomyAiManager`, `CpuMilitaryAiManager`, `SelectionManager`（各種 View コンポーネント付き）

---

## 6. Data Model

### ScriptableObject 一覧

| 型 | アセット例 | 役割 |
|----|-----------|------|
| **UnitData** | `DefaultUnit`（Villager）, `MilitiaData`, `EnemyDummyData` | HP・速度・攻撃・装甲・射程・色 |
| **BuildingData** | `TownCenterData` | TC 表示名・Villager 生産時間・スポーン位置 |
| **PlacedBuildingData** | `HouseData`, `BarracksData` | 配置建築のコスト・建築時間・フットプリント・訓練ユニット |
| **ResourceNodeData** | `DefaultTree` | 木の初期 Wood 量・色 |

### 主要パラメータ（現行バランス）

| データ | 値 |
|--------|-----|
| Villager | HP 100, 速度 5, 非戦闘 |
| Militia | HP 40, 攻撃 4, 装甲 0, 射程 2, CD 1 秒, コスト 20 Wood / 3 秒 |
| Enemy Dummy | Militia 相当（Phase 7/8 テスト用） |
| House | 25 Wood, 3 秒, +5 Pop |
| Barracks | 50 Wood, 5 秒 |
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

| 機能 | AoE2 | 本プロジェクト |
|------|------|----------------|
| **資源** | Wood, Food, Gold, Stone | Wood のみ ✅ / 他 ❌ |
| **建築** | 多数の建築物・時代進化 | TC, House, Barracks のみ ✅ |
| **人口** | Pop cap / House | 基本 Pop cap ✅ / 破壊時減少 ❌ |
| **文明** | 各国固有ボーナス・ユニット | ❌ 単一ルール |
| **研究** | 大学・鍛冶屋等のテクノロジー | ❌ |
| **軍事** | 歩兵・弓・騎兵・攻城等 | Militia（近接 1 種）のみ ✅ |
| **AI** | 経済・軍事・難易度 | ルールベース CPU 経済+軍事 ✅（単純） |
| **戦闘** | 遠近・装甲・相性・地形 | 近接・単純ダメージ ✅ / 相性 ❌ |
| **フォーメーション** | 隊列・スタンス | 移動時グリッドのみ △ |
| **壁** | 石壁・城壁 | ❌ |
| **船** | 海上戦・貿易 | ❌ |
| **市場** | 資源交易 | ❌ |
| **テクノロジー** | 時代昇格（Dark→Imperial） | ❌ |
| **マルチプレイ** | LAN/オンライン | ❌ |
| **マップ** | ランダムマップ生成 | 固定 Plane ❌ |
| **勝敗** | 征服・遺跡等 | TC 破壊 ✅ / その他 ❌ |
| **リプレイ** | あり | ❌ |
| **エディタ** | シナリオエディタ | Phase SceneBuilder のみ △ |

**総合:** AoE2 の **コアループの極小サブセット**（1 資源・3 建築・1 歩兵・1 CPU）を再現。時代・文明・多資源・多兵種は未着手。

---

## 8. Missing Features

### Critical（ゲームとして成立に必須）

| 機能 | 説明 |
|------|------|
| 勝敗条件 | TC 破壊・征服等のゲーム終了 |
| 複数資源 or 食料 | 経済の深みが Wood のみで不足 |
| パスファインディング / 衝突回避 | 大規模ユニット時に詰まる（NavMesh 禁止のため代替手法が必要） |
| 建築・ユニット Object Pooling | 大量戦闘時の GC・Instantiate コスト |
| Fixed Tick Simulation | フレームレート依存・将来マルチ非互換 |

### Important（AoE2 らしさ・拡張性）

| 機能 | 説明 |
|------|------|
| 遠距離ユニット（弓兵） | 戦闘の幅 |
| テクノロジー / 時代 | ユニット・建築のアンロック |
| 追加建築（農場、採石場、市場等） | 経済多様化 |
| CPU 難易度・戦略バリエーション | 現状は単一ルール |
| 本格 HUD（uGUI） | OnGUI はスケール・見切れ問題 |
| 4 チーム対応 | 憲法目標だが enum は 2 チームのみ |
| 自動警戒 / 反撃 | 現状は全手動攻撃 |
| House 破壊時 Pop cap 減少 | |

### Nice To Have

| 機能 | 説明 |
|------|------|
| 騎兵・攻城兵器 | |
| 壁・塔 | |
| 船・水上 | |
| マップ生成 | |
| セーブ / ロード | |
| リプレイ | |
| Animator 本格実装 | 現状は色変化のみ |
| ミニマップ | |
| 音声・BGM | |

---

## 9. Technical Debt

| 負債 | 原因 | 影響範囲 | 状態 / 改善案 |
|------|------|----------|-------------|
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
| **Command Queue（プレイヤー）** | ✅ | 7 種 Command。Tick 先頭 Execute |
| **Command Log** | △ | `CommandLog` 記録あり。再生・保存なし |
| **Command Queue（CPU）** | ❌ | CPU は Manager 直接呼び出し |
| **Deterministic Simulation** | ❌ | float 演算・Tickable 登録順 |
| **Turn / Lockstep** | ❌ | 未設計 |
| **Replay 再生** | ❌ | 未実装 |
| **State Snapshot** | ❌ | セーブデータ構造なし |
| **Network Layer** | ❌ | 憲法で現時点禁止 |
| **Team / Player ID** | △ | `UnitTeam` 2 値のみ |
| **Simulation / View 分離** | △ | Manager に OnGUI View 混在 |

**現状評価:** ローカルプロトタイプとして動作。**マルチプレイ準備度は中程度（30〜40%）**。Fixed Tick + プレイヤー Command 基盤あり。Replay / Lockstep には Entity ID・CPU Command 化・決定論 RNG が必要。

---

## 12. Future Roadmap

Phase 11 以降の候補（優先度順）。

| 優先度 | 候補 | 状態 |
|--------|------|------|
| P0 | 勝敗 UI・ゲーム終了条件 | ✅ Phase 11 |
| P0 | Fixed Tick + Command Queue 基盤 | ✅ Phase 15〜16 |
| P0 | Object Pooling | ✅ Phase 12 |
| P1 | Food 資源 + 農場 | ⬜ Phase 17〜18（M2） |
| P1 | 弓兵（遠距離戦闘） | ⬜ Phase 21（M3） |
| P1 | 本格 HUD 移行 | ⬜ |
| P1 | Benchmark 数値記録 | △ シーンあり / FPS 表 TBD |
| P2 | テクノロジー / 時代昇格 | AoE2 コア体験 |
| P2 | 文明差（ボーナス 1 つから） | リプレイ性 |
| P2 | 騎兵・追加歩兵 | 兵種カウンター |
| P2 | 壁・防御建築 | 拠点防衛 |
| P3 | グリッド Pathfinding（NavMesh 代替） | 大規模移動 |
| P3 | マップ生成 | リプレイ性 |
| P3 | 4 チーム対応 | 憲法目標 |
| P3 | CPU 難易度・戦略テンプレ | AI 品質 |
| P4 | セーブ / ロード | |
| P4 | リプレイ | |
| P4 | マルチプレイ原型 | |

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
├─ ProductionManager                   GetTownCenterForTeam, IsProducing
├─ GatherManager                       IssueGatherCommand
├─ ResourceManager                     GetWood
├─ PopulationManager                   GetCurrentPopulation, GetMaxPopulation, CanTrainUnit
├─ BuildingPlacementManager            HasActiveConstructionForTeam, TryFindPlacementNear,
│                                    TryStartTeamConstruction, IsUnitBuilding
├─ UnitManager                         CopyUnitsTo
└─ TreeResource                        FindObjectsByType（木キャッシュ）

CpuMilitaryAiManager
├─ ProductionManager                   GetTownCenterForTeam
├─ BarracksProductionManager           HasBarracksForTeam, GetBarracksForTeam, IsProducing
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
| **Economy** | **20%** | Wood 採集・TC 搬入・チーム別 Wood ✅（§3 Economy 4/20 項目）。Food/Gold/Stone・農場・市場・交易 ❌ |
| **Buildings** | **10%** | TC / House / Barracks の 3 種のみ ✅（§7）。時代建築・防衛・生産建築多数 ❌。`PlacedBuildingKind` は House + Barracks の 2 値のみ |
| **Military** | **8%** | Militia 近接 1 種 + 基本戦闘 ✅（§3 Combat 6/15 項目）。弓・騎兵・攻城・海軍・ユニット UP ❌ |
| **AI** | **12%** | `CpuEconomyAiManager` + `CpuMilitaryAiManager` ルールベース ✅（§3 AI 8/12 項目）。スカウト・時代・難易度・ビルドオーダー ❌ |
| **Technology** | **0%** | 研究・時代昇格・鍛冶屋/大学 ❌（§3 / §7 該当項目すべて ❌） |
| **Civilizations** | **0%** | 文明ボーナス・固有ユニット ❌。`UnitTeam` は Player/Enemy の 2 値のみ |
| **Multiplayer** | **35%** | Fixed Tick ✅ / プレイヤー Command ✅ / CPU Command ❌ / Replay 再生 ❌ |
| **UX/UI** | **20%** | 選択・OnGUI HUD・生産パネル・HP バー・**勝敗画面** ✅ |
| **Performance Foundation** | **65%** | Pool ✅ / Spatial Hash ✅ / Fixed Tick ✅ / Benchmark ✅ / Instancing ❌ |

### Overall Completion

```
Overall Completion: 10%
```

**算出式（カテゴリ均等重み 9 分の 1）:**

```
(20 + 10 + 8 + 12 + 0 + 0 + 0 + 15 + 22) / 9 = 87 / 9 ≈ 9.7% → 10%（四捨五入）
```

### 解釈

| 観点 | 評価 |
|------|------|
| **コアループ** | 採集 → 建築 → 生産 → 戦闘は **垂直スライスとして成立**（Phase 10） |
| **AoE2 全体** | 資源・兵種・文明・時代・マルチの大部分が未着手のため **約 1 割** |
| **最も近い領域** | Economy（Wood ループ）、UX/UI（操作可能な最小 HUD） |
| **最も遠い領域** | Technology / Civilizations / Multiplayer（実装 0） |

### Phase 10 完了時点の位置づけ

```
AoE2 フル機能 ████░░░░░░░░░░░░░░░░  ~10%
コアループ     ████████████████░░░░  ~80%  （1 資源・3 建築・1 兵種・1 CPU 内）
憲法性能目標   ███░░░░░░░░░░░░░░░░░  ~15%  （Manager 方式のみ。800 体未検証）
```

---

## Appendix A: ソースディレクトリ

```
Assets/Scripts/
  AI/           CpuEconomyAiManager, CpuMilitaryAiManager
  Buildings/    TC, House, Barracks, Placement, Production
  Camera/       RTSCameraController
  Combat/       AttackManager
  Core/         GameLayers, GameAssetPaths
  Economy/      Resource, Gather, Population, Tree
  Input/        RTSInputReader, InputActions 生成
  Selection/    Selection, HUD Views, Formation
  Units/        Unit, UnitManager, UnitData
  Editor/       Phase1〜10 SceneBuilder, 各種 Setup
```

## Appendix B: AI への推奨読み方

1. 本ファイルで全体像を把握
2. [CONSTITUTION.md](../CONSTITUTION.md) で制約を確認
3. 変更対象 Phase の `docs/prompts/phaseN-prompt.md` を読む
4. `Assets/Scripts/` の該当 Manager を読んでから small diff

## Appendix C: クイック判断ガイド

| 質問 | 答え |
|------|------|
| AoE2 にどれくらい近い？ | 1 資源・3 建築・1 兵種・1 CPU の **垂直スライス** |
| 何が一番足りない？ | 多資源・時代・兵種・本格 UI |
| 次に何を作るべき？ | **Phase 17 Food**（Milestone 2 Economy）— [docs/README.md](README.md) |
| プレイ用シーンは？ | **`Phase10.unity`** |
| 自軍は自動反撃？ | **しない**（Militia 右クリックのみ） |
| 性能ベンチマークは？ | **未計測（TBD）** — §Performance Benchmark 参照 |
| 変更の影響範囲は？ | §Core Dependency Graph 参照 |
| AoE2 全体の完成度は？ | **約 10%** — §AoE2 Completion Analysis 参照 |

---

*Document generated from codebase state at Phase 10 completion. §Performance Benchmark FPS values are TBD until measured.*

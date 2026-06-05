# AoE RTS Engine — Implementation Status

> **用途:** このファイル単体を AI に渡すことで、現状の実装範囲・未実装・AoE2 との差分・技術構成・拡張方針を把握できる。
>
> **最終更新基準:** Phase 1〜10 完了時点（`Phase10.unity` が最終プレイシーン）
>
> **関連:** [CONSTITUTION.md](../CONSTITUTION.md) / [PHASES.md](PHASES.md) / [README.md](../README.md)
>
> **注:** 本ファイルパスは `docs/RTS_IMPLEMENTATION_STATUS.md`（Implementation Status ドキュメント）

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
- **推奨:** Manager 更新方式、Object Pooling、GPU Instancing、Shared Material、Fixed Tick（※ Fixed Tick は未導入）
- **マルチプレイ:** 現時点でネットコード実装禁止。将来の command queue / fixed tick / simulation 分離を意識

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

**ゲームループ（Phase 10 完成形）:** 採集 → 建築 → 生産 → 戦闘

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
| 右クリック命令 | ✅ | 移動 / 採集 / 攻撃 |
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
| 直線移動 | ✅ | `UnitManager` 一括 `TickMovement` |
| 目標到達で停止 | ✅ | |
| 障害物回避 | ❌ | |
| NavMesh / Pathfinding | ❌ | 憲法で禁止 |
| RVO / ユニット間押し出し | ❌ | `UnitPositionOffsets` で軽微な散開のみ |
| 地形高低差 | ❌ | 平面 Ground のみ |

### Economy

| 機能 | 状態 | 備考 |
|------|------|------|
| Wood 資源 | ✅ | チーム別 `ResourceManager` |
| Food / Gold / Stone | ❌ | |
| 木（Tree）採集 | ✅ | Move → Gather → Carry → Deposit |
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
| 死亡（Destroy） | ✅ | HP ≦ 0 |
| HP バー（選択時） | ✅ | `UnitHpBarView`（OnGUI） |
| 攻撃中色変化 | ✅ | オレンジ系ティント |
| 遠距離攻撃（弓・投石） | ❌ | |
| 自動反撃 / 警戒 AI | ❌ | 自軍 Militia は手動右クリックのみ |
| スプラッシュ・貫通 | ❌ | |
| ユニットアップグレード | ❌ | |
| 勝敗判定 UI | ❌ | |

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
| CPU Wood / Pop | ✅ | `CpuHudView`（Phase 9/10） |
| ゲーム時間・波カウントダウン | ✅ | `GameTimeHudView`（Phase 10） |
| TC / Barracks 生産パネル | ✅ | OnGUI ボタン |
| 本格 UI（uGUI / UI Toolkit） | ❌ | すべて OnGUI MVP |

---

## 4. Systems Overview

### システム一覧

```
Input → Selection → Command Issue
                          ↓
        ┌─────────────────┼─────────────────┐
        ↓                 ↓                 ↓
   UnitManager      GatherManager      AttackManager
   (移動)            (採集)              (戦闘)
        ↓                 ↓
   ProductionManager  BuildingPlacementManager
   BarracksProductionManager
        ↓
   ResourceManager / PopulationManager
        ↓
   CpuEconomyAiManager / CpuMilitaryAiManager
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
| **データフロー** | 左クリック/ドラッグ → 選択リスト更新 → 右クリック Raycast → 採集/攻撃/移動/建築中断 |

**命令優先:** 木（Resource）→ 敵 Unit（攻撃）→ 地面（移動）

---

### Unit System

| 項目 | 内容 |
|------|------|
| **目的** | ユニット状態・HP・移動・ビジュアル・生死 |
| **主要クラス** | `Unit`, `UnitManager`, `UnitData`, `UnitTeam`, `UnitState`, `UnitPositionOffsets`, `UnitSpawner` |
| **関連データ** | `UnitData`（DefaultUnit=Villager, MilitiaData, EnemyDummyData） |
| **データフロー** | `UnitManager.Update` → 全 Unit `TickMovement` / 死亡時 `Die()` → Manager からジョブ解除 |

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
| **勝敗** | 征服・遺跡等 | ❌ |
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

| 負債 | 原因 | 影響範囲 | 将来の改善案 |
|------|------|----------|-------------|
| **OnGUI ベース UI** | Phase 3〜 で迅速 MVP | 全 HUD・パネル。解像度・見切れ（HP バー下端） | uGUI / UI Toolkit 移行 |
| **Fixed Tick 未導入** | `Time.deltaTime` で簡易実装 | 全 Manager の戦闘・生産・採集タイミング | Simulation 層を fixed tick 化 |
| **Instantiate / Destroy** | Phase 1 からの単純実装 | ユニット死亡・生産・建築完成 | Object Pool + 再利用 |
| **Unit 移動の直線のみ** | NavMesh 禁止 | 障害物・建築回り | グリッド A* または Flow Field |
| **Static Singleton Manager** | 実装速度優先 | 全システム。テスト・マルチ非互換 | DI or Simulation World |
| **シーン参照 `{fileID: 0}`** | SceneBuilder / 手動生成混在 | 配置データの不安定さ | Setup メニュー統一 or 参照の完全シリアライズ |
| **Phase10.unity 生成経路が二重** | Unity バッチ不可時に Python 生成 | シーンと SceneBuilder の乖離 | CI / Editor からの単一生成 |
| **CPU AI の `FindObjectsByType`** | 木リストキャッシュ簡易実装 | 木追加・削除時の同期 | 登録型 ResourceRegistry |
| **リスト線形探索** | 小規模 MVP | 200 ユニット時の CPU 負荷 | チーム別インデックス・Spatial Hash |
| **GPU Instancing 未活用** | Capsule + MaterialPropertyBlock | 描画コール増 | 同一メッシュの Instancing |
| **攻撃・採集のフレーム依存** | Manager Update 分散 | リプレイ再現不可 | Command Queue 化 |

---

## 10. Performance Review

### Update 使用（`MonoBehaviour.Update`）

| クラス | 役割 |
|--------|------|
| `UnitManager` | 全 Unit 移動 Tick |
| `AttackManager` | 攻撃ジョブ Tick |
| `GatherManager` | ※ `LateUpdate` で採集 Tick |
| `ProductionManager` | TC 生産タイマー |
| `BarracksProductionManager` | Barracks 生産タイマー |
| `BuildingPlacementManager` | 配置モード（建築 Tick は `LateUpdate`） |
| `SelectionManager` | 入力・選択処理 |
| `RTSCameraController` | カメラ |
| `CpuEconomyAiManager` | 2 秒間隔 AI |
| `CpuMilitaryAiManager` | 波タイマー + 2 秒間隔 AI |
| `ProductionPanelView` | Q キー監視 |

**Unit 個別 Update:** なし ✅（憲法準拠）

**OnGUI（毎フレーム描画）:** `ResourceHudView`, `ProductionPanelView`, `BarracksPanelView`, `SelectionBoxView`, `UnitHpBarView`, `CpuHudView`, `GameTimeHudView`

### Manager 構成

- 10+ の static singleton Manager が並列稼働
- ジョブは `List<T>` で保持（AttackJob, GatherJob, ProductionJob, ConstructionSite）
- ユニット列挙は `UnitManager.CopyUnitsTo` → 再利用 `List` バッファ（AI 側）

### GC 発生ポイント

| 箇所 | 内容 |
|------|------|
| `Unit.Die()` | `Destroy(gameObject)` |
| ユニット/建築スポーン | `new GameObject` / Instantiate 相当 |
| `FindObjectsByType<TreeResource>()` | CPU AI の木キャッシュ更新 |
| OnGUI | 内部アロケーション（レイアウト・Skin） |
| `Debug.Log` 文字列結合 | 攻撃毎ログ |

### 将来のボトルネック（200 ユニット × 4 チーム想定）

1. **UnitManager** — O(n) 全 Unit 移動毎フレーム
2. **AttackManager / GatherManager** — O(jobs) 線形走査
3. **SelectionManager** — ドラッグ時の Unit 全走査
4. **描画** — Unit ごと MaterialPropertyBlock（Instancing 未使用）
5. **AI** — `FindNearestPlayerUnit` 等の全 Unit 走査
6. **OnGUI** — 本番スケール不向き

**現状の実用規模:** 数十ユニット程度なら問題なし。憲法目標の 800 ユニット級には構造変更が必要。

**FPS 実測値:** 未計測 — [Performance Benchmark](#performance-benchmark) を参照。

---

## 11. Multiplayer Readiness

| 要素 | 状態 | 評価 |
|------|------|------|
| **Deterministic Simulation** | ❌ | `Time.deltaTime`・浮動小数・Update 順序依存 |
| **Command Queue** | ❌ | 入力が直接 Manager を呼ぶ（即時実行） |
| **Turn / Lockstep** | ❌ | 未設計 |
| **Replay** | ❌ | コマンドログなし |
| **State Snapshot** | ❌ | セーブデータ構造なし |
| **Network Layer** | ❌ | 憲法で現時点禁止 |
| **Team / Player ID** | △ | `UnitTeam` 2 値のみ。Player Index なし |
| **Simulation / View 分離** | △ | Manager に View（OnGUI）が混在 |

### マルチプレイ対応に必要な追加要素

1. **入力 → Command 化** — `MoveCommand`, `AttackCommand`, `BuildCommand` 等をキューに積む
2. **Fixed Tick World** — 例: 20 TPS で全シミュレーションを進める
3. **決定論的 RNG** — シード付き乱数（マップ・クリティカル等用）
4. **エンティティ ID** — GameObject 参照ではなく int ID で参照
5. **状態同期 or ロックステップ** — コマンドストリームの共有
6. **Pool ベース Spawn** — ネットワーク上の生成/破壊の一貫性

**現状評価:** ローカルプロトタイプとして動作。**マルチプレイ準備度は低い（10〜20%）**。将来互換を意識した設計思想はあるが、実装は未着手。

---

## 12. Future Roadmap

Phase 11 以降の候補（優先度順）。

| 優先度 | 候補 | 理由 |
|--------|------|------|
| P0 | 勝敗 UI・ゲーム終了条件 | 1 ゲームとしての完結 |
| P0 | Fixed Tick + Command Queue 基盤 | 大規模化・マルチの前提 |
| P1 | Food 資源 + 農場 | 経済の奥行き |
| P1 | 弓兵（遠距離戦闘） | 戦闘バリエーション |
| P1 | 本格 HUD 移行 | UX・見切れ解消 |
| P1 | Object Pooling | パフォーマンス |
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
> **現状:** リポジトリ内に FPS 計測結果・ベンチマークシーン・Profiler ログは **存在しない**。以下は計測待ち（TBD）のテンプレート。

### Test Environment

| 項目 | 値 |
|------|-----|
| **Unity Version** | Unity 6（README 記載）。具体パッチ: **TBD**（例: 6000.x） |
| **CPU** | TBD |
| **RAM** | TBD（目標スペック: MacBook Air **16 GB** — [CONSTITUTION.md](../CONSTITUTION.md)） |
| **GPU** | TBD |
| **OS** | TBD |
| **Test Scene** | TBD（推奨: `Phase10.unity` または専用 Benchmark シーン） |
| **Render Pipeline** | URP（プロジェクト標準） |
| **VSync** | TBD |
| **Quality Settings** | TBD |

### Test Method（推奨手順 — 未実施）

以下は **ドキュメント・憲法から導出した推奨方法**。実測値ではない。

1. **シーン:** `Phase10.unity` をベースに、指定 Unit 数まで Villager / Militia をスポーン（Editor スクリプトまたは Phase SceneBuilder 拡張）
2. **カメラ:** 俯瞰固定（`RTSCameraController` 初期視点相当）
3. **負荷パターン（各 Unit Count で計測）:**
   - **Idle:** 全ユニット静止
   - **Move:** 全ユニットにランダム移動命令（`UnitManager.TickMovement` 負荷）
   - **Combat:** 半数が Attack ジョブ（`AttackManager` 負荷）
   - **Mixed:** Phase 10 相当（Gather + CPU AI + 攻撃波）— **実プレイに最も近い**
4. **計測:** Unity Profiler（CPU / GC Alloc）+ 画面上 FPS（最低 60 秒平均）
5. **合格目安（憲法目標から逆算）:** 4 チーム × 200 ユニット = **800 ユニット** で playable FPS（目標値 **TBD**、要定義）

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

**注意:** 現コードベースには N 体スポーン用ベンチマークツールは **未実装**。計測前に Editor ベンチマークシーンの追加が必要。

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
RTSInputReader                          （入力読取のみ。他 Manager 非依存）
└─ RTSCameraController                  （CameraMove, ZoomDelta）

RTSInputReader
└─ SelectionManager
   ├─ UnitManager                       CopyUnitsTo（選択・矩形判定）
   ├─ BuildingPlacementManager          IsPlacementModeActive, TryConfirmPlacement,
   │                                    CancelPlacementMode, AbortConstructionForUnits
   ├─ GatherManager                     IssueGatherCommand, CancelForUnits
   ├─ AttackManager                     IssueAttack, CancelForUnits
   └─ GroupMoveFormation                AssignMoveTargets（static・Manager 外）

ResourceHudView                         （View → 命令入口）
├─ SelectionManager                     SelectedUnits
├─ ResourceManager                      Wood 参照
├─ PopulationManager                    Pop 参照
└─ BuildingPlacementManager             EnterHousePlacementMode, EnterBarracksPlacementMode

ProductionPanelView
├─ SelectionManager                     SelectedTownCenter
├─ ProductionManager                    IsProducing, GetRemainingSeconds, GetTotalSeconds
├─ PopulationManager                    CanTrainUnit
└─ TownCenter.TryQueueProduction        → ProductionManager.TryQueueProduction

BarracksPanelView
├─ SelectionManager                     SelectedBarracks
├─ BarracksProductionManager            IsProducing, GetRemainingSeconds, GetTotalSeconds
├─ ResourceManager                      Wood 参照
├─ PopulationManager                    CanTrainUnit
└─ Barracks.TryQueueMilitiaProduction   → BarracksProductionManager.TryQueueProduction
```

**プレイヤー命令フロー（コード上の経路）:**

```
右クリック / HUD ボタン
  → SelectionManager または ResourceHudView
    → GatherManager | AttackManager | BuildingPlacementManager | GroupMoveFormation
      → Unit.SetMoveTarget / AttackJob / GatherJob / ConstructionSite
```

---

### Simulation Managers（ゲームロジック中核）

```
UnitManager
└─ Unit.TickMovement                   （Update 内。他 Manager 非呼び出し）

Unit                                    （Entity・双方向コールバック）
├─ UnitManager                         Register / Unregister
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

ProductionManager
├─ PopulationManager                   CanTrainUnit
└─ UnitSpawner                         Spawn（生産完了時）

BarracksProductionManager
├─ PopulationManager                   CanTrainUnit
├─ ResourceManager                     TrySpendWood
└─ UnitSpawner                         Spawn（生産完了時）

BuildingPlacementManager
├─ ResourceManager                     TrySpendWood
├─ GatherManager                       CancelForUnits
├─ PopulationManager                   AddHousing（House 完成時）
├─ UnitManager                         CopyUnitsTo
└─ RuntimeBuildingFactory              CreateHouse, CreateBarracks, GetSharedLitMaterial

PopulationManager
└─ UnitManager                         GetUnitCountForTeam

ResourceManager                         （末端。UnitTeam 別 Wood 保持のみ）

TownCenter                              （Entity）
└─ ProductionManager                   Register, TryQueueProduction

Barracks                                  （Entity）
└─ BarracksProductionManager           Register, TryQueueProduction
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
| **Multiplayer** | **0%** | ネットコード・Command Queue・Replay ❌（§11 すべて ❌） |
| **UX/UI** | **15%** | 選択・OnGUI HUD・生産パネル・HP バー ✅（§3 UI 6/10 項目）。ミニマップ・本格 UI・勝敗画面 ❌ |
| **Performance Foundation** | **22%** | Unit 個別 Update なし ✅、`UnitManager` 集中移動 ✅（§10）。Fixed Tick / Pooling / Instancing / Pathfinding ❌（§9 Technical Debt） |

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
| 何が一番足りない？ | 多資源・時代・兵種・勝敗・パフォーマンス基盤 |
| 次に何を作るべき？ | 勝敗 UI → Fixed Tick → Food/農場 or 弓兵（目的による） |
| プレイ用シーンは？ | **`Phase10.unity`** |
| 自軍は自動反撃？ | **しない**（Militia 右クリックのみ） |
| 性能ベンチマークは？ | **未計測（TBD）** — §Performance Benchmark 参照 |
| 変更の影響範囲は？ | §Core Dependency Graph 参照 |
| AoE2 全体の完成度は？ | **約 10%** — §AoE2 Completion Analysis 参照 |

---

*Document generated from codebase state at Phase 10 completion. §Performance Benchmark FPS values are TBD until measured.*

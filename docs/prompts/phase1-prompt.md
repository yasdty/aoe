# Phase 1 実行プロンプト

> **状態:** ✅ 実行済み（初回キックオフ）  
> **使い方:** 空ワークスペースから Phase 1 を実装するときにそのまま Agent へ貼り付け。  
> **注記:** プロジェクト全体の設計文脈 + Phase 1 指示 + Phase 2〜10 の予告を含む初回プロンプト。

---

Phase1

目標
# Low-Spec RTS Engine Project (AoE2-inspired)

あなたは：

* シニア Unity RTS エンジニア
* C# 熟練者
* Unity 6 熟練者
* AoE2 / StarCraft / Warcraft3 のゲーム構造理解者
* low-spec最適化専門

目的：

Age of Empires 2 ライクなRTSを
Unityで構築する。

==================================================

# 技術スタック

==================================================

継続利用：

* Unity 6
* C#
* URP
* New Input System
* Cursor
* Git

無料アセットのみ利用可能。

Asset Store購入禁止。

==================================================

# 最重要目標

==================================================

MacBook Air 16GB級でも：

* 4チーム
* 各200ユニット
* 建築多数
* 大規模戦闘

が動作すること。

==================================================

# 目標ゲーム

==================================================

AoE2ライク。

重要：

* 経済
* 村人管理
* 資源採集
* 建築配置
* ユニット生産
* 大軍戦
* 文明差
* CPU AI
* マイクロ操作

不要：

* campaign
* cinematic
* story
* ultra graphics
* physics destruction

==================================================

# マルチプレイ考慮

==================================================

最初は：

single player only

将来：

* Human vs Human
* Human vs CPU
* CPU vs CPU

対応可能構造。

ネットコード実装禁止。

ただし：

* fixed tick
* command queue
* simulation分離

を意識。

==================================================

# マップ

==================================================

Phase1：

* 平地
* グリッド
* 高低差なし

後で追加可能：

* 森
* 川
* 崖
* choke point

==================================================

# パスファインディング

==================================================

重要。

必要：

* unit movement
* formation movement
* building avoidance
* crowd avoidance

推奨：

* Grid A*
* Flow Field

Unity NavMesh依存禁止。

==================================================

# ユニット

==================================================

初期実装：

* Villager
* Militia

後で追加：

* Archer
* Scout
* Spearman
* Knight

==================================================

# 資源

==================================================

必要：

* Food
* Wood
* Gold
* Stone

==================================================

# 村人

==================================================

必要：

* 採集
* 運搬
* 建築
* 修理

==================================================

# 建築

==================================================

Phase順：

1 TownCenter
2 House
3 Barracks
4 LumberCamp
5 MiningCamp
6 Farm

必要：

* placement preview
* blocked tile
* construction progress

==================================================

# 文明

==================================================

初期：

* Franks
* Britons
* Mongols

ScriptableObjectで管理。

data-driven設計。

==================================================

# AI

==================================================

Phase順：

1 採集
2 build order
3 軍生産
4 攻撃波
5 文明別AI

==================================================

# 戦闘

==================================================

AoE2寄り。

必要：

* melee
* ranged
* projectile
* attack cooldown
* armor
* bonus damage

不要：

* physics combat

==================================================

# Selection

==================================================

必要：

* drag select
* right click move
* right click attack
* control group
* multi select

==================================================

# Formation

==================================================

必要：

* group move
* soft formation
* separation

==================================================

# Animation

==================================================

必要：

* idle
* walk
* attack
* gather
* build
* death

無料モデル利用可。

==================================================

# Save System

==================================================

将来対応。

必要：

* replay考慮
* command log考慮

==================================================

# low-spec最適化

==================================================

超重要。

禁止：

* Rigidbody大量利用
* UnitごとのUpdate乱立
* UnitごとのRaycast乱立
* Dynamic Shadow乱用
* 高解像度テクスチャ

推奨：

* Manager更新方式
* Object Pooling
* GPU Instancing
* Shared Material
* Fixed Tick

==================================================

# デバッグ方針

==================================================

毎Phase：

* regression check
* performance check
* memory leak check
* unit count check

==================================================

# AI実装ルール

==================================================

超重要。

* rewrite禁止
* 一括リファクタ禁止
* phase単位
* small diff only
* 必ず既存コード確認

推測禁止。

==================================================

# 実装前に必ず出力

==================================================

* 変更ファイル一覧
* 影響範囲
* performance影響
* memory影響
* save影響
* multiplayer将来互換性

==================================================

# 最重要

==================================================

Phaseを飛ばさない。

必ず：

MVP
↓
動作確認
↓
次Phase

で進める。

巨大機能を一気に追加禁止。

==================================================

# 最終目標

==================================================

AoE2ライクな：

* economy
* build order
* civilization strategy
* large scale battle

を持つ

Low-Spec RTS Engine。

RTSの最低限の土台

# Phase 1

目的

RTSの最低限のプレイ環境を作る。

今回実装するもの

- RTSカメラ
- 地面
- ユニット1体
- 左クリック選択
- 右クリック移動

必要機能

■ RTS Camera

- WASD移動
- マウス端スクロール
- ホイールズーム
- 回転不要

■ Ground

- Plane配置
- 100x100程度

■ Unit

- Capsuleで可
- UnitData作成
- HPを保持

■ Selection

- 左クリックで選択
- 選択時色変更

■ Move

- 地面右クリック
- MoveTarget設定
- 目標地点へ移動

禁止

- AI
- 建築
- 資源
- NavMesh

実装前に必ず出力

- 変更ファイル一覧
- クラス図
- パフォーマンス影響

実装後に出力

- テスト手順
- 残課題
Phase2

目標

AoEらしい操作感

# Phase 2

目的

複数ユニット操作を実装。

今回実装

- ドラッグ選択
- 複数選択
- Shift追加選択
- グループ移動

必要

- SelectionBox UI
- UnitManager

移動

- 複数選択状態で右クリック

到達点

選択ユニットを

3x3
4x4

等で整列移動。

禁止

- フォーメーション最適化
- 回避処理

実装前に必ず出力

- 変更ファイル一覧
- 影響範囲

実装後

- テスト手順
Phase3

目標

AoE開始状態

# Phase 3

目的

TownCenterと村人生成。

今回実装

- TownCenter
- Villager
- 生産キュー

必要

- BuildingData
- UnitData

UI

- TownCenter選択

ボタン

Create Villager

生産時間

5秒

完成後

TownCenter横へ出現

禁止

- 建築
- 採集
Phase4

目標

経済開始

# Phase 4

目的

木材採集。

今回実装

- Tree
- ResourceNode
- GatherTask

村人

右クリックで木へ移動

状態

Move
↓
Gather
↓
Carry
↓
Deposit

TownCenterへ運搬

木材増加

必要資源

Woodのみ

禁止

Food
Gold
Stone
Phase5

目標

建築開始

# Phase 5

目的

House建築。

今回実装

- BuildingPlacement
- GhostPreview
- House

流れ

Houseボタン
↓
配置モード
↓
クリック
↓
村人移動
↓
建築開始

House

Cost
Wood 25

BuildTime
10秒

禁止

人口制限
文明差
Phase6

目標

RTSとして成立

# Phase 6

目的

人口システム。

実装

- Population
- Housing

初期

5/5

House

+5

人口上限超過時

ユニット生産不可
Phase7

目標

軍事開始

# Phase 7

目的

Barracksと兵士。

実装

- Barracks
- Militia

Militia

HP
Attack
Armor

右クリック攻撃

実装

- AttackTarget
- AttackCooldown

禁止

遠距離攻撃
Phase8

目標

戦闘成立

# Phase 8

目的

ユニット戦闘。

実装

- 死亡
- HP表示
- 攻撃モーション

必要

UnitState

Idle
Move
Attack
Dead
Phase9

目標

AoEの核完成

# Phase 9

目的

CPU AI。

実装

- 木採集
- House建築
- Villager生産

CPU1チーム

人間と共存

禁止

軍事AI
Phase10

目標

AoEプロトタイプ完成

# Phase 10

目的

簡易RTS完成。

実装

- Barracks生産
- Militia軍団
- CPU攻撃波

CPU

5分毎攻撃

到達目標

AoE風ゲームループ

採集
↓
建築
↓
生産
↓
戦闘

Cursorに渡す実運用としては、

① プロジェクト憲法
② 現在のソースコードを読ませる
③ Phase指示

の順番が良いです。

さらにAI開発成功率を上げるなら、各Phaseをさらに

Phase 4-1 Tree配置
Phase 4-2 Gather
Phase 4-3 Carry
Phase 4-4 Deposit

のように30〜60分単位へ細分化すると、途中で壊れにくくなります。AoE系は特に「移動」と「採集」が複雑なので、Phase4以降は小さく刻むのがおすすめです。
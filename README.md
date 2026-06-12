# AoE RTS Engine

Age of Empires 2 ライクな Low-Spec RTS を Unity 6 で構築するプロジェクト。

## セットアップ

1. Unity Hub で **Unity 6** をインストール
2. 本リポジトリを Unity Hub の「Add project from disk」で開く
3. 初回インポート完了後、メニュー **AoE → Setup Phase1 Scene** を実行
4. `Assets/Scenes/Phase1.unity` を開いて Play

**Phase 2:** メニュー **AoE → Setup Phase2 Scene** → `Assets/Scenes/Phase2.unity` を Play

**Phase 3:** メニュー **AoE → Setup Phase3 Scene** → `Assets/Scenes/Phase3.unity` を Play

**Phase 4:** メニュー **AoE → Setup Phase4 Scene** → `Assets/Scenes/Phase4.unity` を Play

**Phase 5:** メニュー **AoE → Setup Phase5 Scene** → `Assets/Scenes/Phase5.unity` を Play

**Phase 6:** メニュー **AoE → Setup Phase6 Scene** → `Assets/Scenes/Phase6.unity` を Play

**Phase 7:** メニュー **AoE → Setup Phase7 Scene** → `Assets/Scenes/Phase7.unity` を Play

**Phase 8:** メニュー **AoE → Setup Phase8 Scene** → `Assets/Scenes/Phase8.unity` を Play

**Phase 9:** メニュー **AoE → Setup Phase9 Scene** → `Assets/Scenes/Phase9.unity` を Play

**Phase 10（メイン検証シーン）:** `Assets/Scenes/Phase10.unity` を Play

**Phase 10.5（見た目）:** 初回のみ以下を実行してから Play

1. `py Tools/generate_placeholder_glbs.py`
2. メニュー **AoE → Setup Phase10.5 Scene**

（Visual Prefab のみ更新する場合は **AoE → Setup Phase10.5 Visual Prefabs**）

**Phase 11（Foundation — 勝敗）:** メニュー **AoE → Setup Phase10 Scene** を再実行してから `Phase10.unity` を Play

- **勝利:** 敵 Town Center 破壊 → `VICTORY`
- **敗北:** 自 Town Center 破壊 → `DEFEAT`
- 終了後は **R** でシーン再読み込み
- Town Center HP: 400 / House: 150 / Barracks: 200（`AoE → Setup Phase10 Scene` で Data アセットも更新）

**Phase 12（Foundation — Object Pool）:** 上記と同じく **AoE → Setup Phase10 Scene** を再実行してから Play

- Villager / Militia の生産・死亡は `UnitPool` 経由（Console: `UnitPool: spawn=X reuse=Y`）
- House / Barracks 完成・破壊は `BuildingPool` 経由（Town Center は Pool 対象外）
- 初回 Play 時 Prewarm: Villager 4 / Militia 4

**Phase 13（Foundation — Benchmark）:** メニュー **AoE → Setup Benchmark Scene** → `Assets/Scenes/Benchmark.unity` を Play

- 左上 HUD: FPS / FrameTime / GC
- ボタン **50 / 100 / 200 / 500 / 800** で Villager 一括スポーン（`UnitPool.Rent` 経由）
- **Clear** で Pool 返却
- ゲーム本体の検証は引き続き `Phase10.unity`

**Phase 14（Foundation — Spatial Hash）:** **AoE → Setup Phase10 Scene** を再実行してから Play

- `UnitSpatialIndex` / `TreeSpatialIndex` が Systems に追加される
- CPU 木・攻撃目標探索、矩形選択が Grid 経由

**Phase 15（Foundation — Fixed Tick）:** **AoE → Setup Phase10 Scene** を再実行してから Play

- `SimulationTick` — **20 TPS**（1 Tick = 0.05s）
- 移動・攻撃・採集・生産・建築・CPU AI は Fixed Tick 駆動
- Input / Camera / UI は可変フレームのまま

**Phase 16（Foundation — Command Queue）:** **AoE → Setup Phase10 Scene** を再実行してから Play

- プレイヤー操作（移動・攻撃・採集・建築確定・生産）は `CommandQueue` 経由
- 各 Simulation Tick 先頭で命令実行（Console: `CommandLog: tick=N ...`）
- CPU AI は従来どおり Manager 直接呼び出し

**Phase 17（M2 Economy — Food）:** **AoE → Setup Phase10 Scene** を再実行してから Play

- **Berry Bush** 右クリックで Food 採集 → TownCenter 搬入
- HUD: **Food: N**（開始 200）/ Wood / Pop
- Villager 生産（Q）: **50 Food** 消費

**M5（Visual / UI）✅ 完了:** Phase 49〜56 — 壁・i18n・uGUI HUD・ミニマップ・アニメ・戦闘 VFX  
セットアップ例: **AoE → Setup Combat VFX (Phase56)**（初回のみ）

**M6（次）— 4 人・2v2・大マップ・Fog:** [docs/10_M6_MULTIPLAYER_FOUNDATION.md](docs/10_M6_MULTIPLAYER_FOUNDATION.md)

| Phase | 内容 |
|-------|------|
| 57 | Entity ID & PlayerId — **次** |
| 58 | CPU Command Queue |
| 59 | 4 人マッチ（人間1 + CPU3） |
| 60 | 2v2（人間+CPU vs CPU×2） |
| 61 | 大型マップ |
| 62 | Fog of War |
| 63 | 決定論（LAN 前・後回し可） |

ロードマップ索引: [docs/README.md](docs/README.md)  
実装状況（正本）: [docs/IMPLEMENTATION_STATUS.md](docs/IMPLEMENTATION_STATUS.md)  
Foundation: [docs/02_M1_FOUNDATION_PHASES.md](docs/02_M1_FOUNDATION_PHASES.md)  
Economy: [docs/03_M2_ECONOMY_PHASES.md](docs/03_M2_ECONOMY_PHASES.md)〜[docs/08_M4_GAMEPLAY_PHASES.md](docs/08_M4_GAMEPLAY_PHASES.md)  
Visual / UI（M5）: [docs/09_M5_VISUAL_UI_PHASES.md](docs/09_M5_VISUAL_UI_PHASES.md)

入力エラーが出る場合は **AoE → Fix Phase1 Input References** を実行してから Play してください。

地面がピンクの場合は **AoE → Fix Render Pipeline** を実行してから **Setup Phase10 Scene** をやり直してください。

`RTSInputActions` を **Project-wide Input Actions** に割り当てないでください（EditorBuildSettings の不正参照の原因になります）。Phase1 はシーン上の `RTSInputReader` 参照のみ使用します。

> Input System の有効化ダイアログが出た場合は **Yes** を選択してください。

## Phase 1 操作

| 操作 | 入力 |
|------|------|
| カメラ移動 | WASD / 画面端にマウス |
| ズーム | マウスホイール |
| ユニット選択 | 左クリック |
| 移動命令 | 右クリック（地面） |

### Phase 2 操作（Phase2.unity）

| 操作 | 入力 |
|------|------|
| 矩形選択 | 左ドラッグ |
| 単体選択 | 左クリック |
| 追加選択 | Shift + 左クリック / Shift + ドラッグ |
| グループ移動 | 複数選択中に右クリック |

### Phase 3 操作（Phase3.unity）

| 操作 | 入力 |
|------|------|
| TownCenter 選択 | 左クリック |
| Villager 生産 | TownCenter 選択中に **Create Villager (Q)** または **Q**（**3 秒**） |
| ユニット選択・移動 | Phase 2 と同様 |

### Phase 4 操作（Phase4.unity）

| 操作 | 入力 |
|------|------|
| 木材採集 | Villager 選択中に木を右クリック |
| Wood 表示 | 画面左上（OnGUI） |
| TownCenter 搬入 | 採集後、自動で TC へ運搬 |
| その他 | Phase 2〜3 と同様 |

### Phase 5 操作（Phase5.unity）

| 操作 | 入力 |
|------|------|
| House 建築 | **Build House (25 Wood)** → 地面左クリックで配置 |
| 配置キャンセル | Esc / 右クリック |
| 建築 | Villager 選択中に配置 → 現場へ移動 → **3 秒**で完成 |
| 建築中断 | 建築中に右クリック移動 → 中断（Wood 返金なし） |
| 開始カメラ | TC 中心・俯瞰（Play 開始時に自動） |
| その他 | Phase 2〜4 と同様 |

### Phase 6 操作（Phase6.unity）

| 操作 | 入力 |
|------|------|
| 人口表示 | 画面左上 **Pop: N/M** |
| 生産上限 | 人口 ≧ 上限のとき TC で Villager 生産不可 |
| House 完成 | 上限 **+5**（建築中は加算しない） |
| その他 | Phase 2〜5 と同様 |

### Phase 7 操作（Phase7.unity）

| 操作 | 入力 |
|------|------|
| Barracks 建築 | **Build Barracks (50 Wood)** → 配置 → Villager 建築（**5 秒**） |
| Militia 生産 | Barracks 選択 → **Create Militia (20 Wood)**（**3 秒**） |
| 攻撃 | Militia 選択 → 敵 Unit を右クリック（近接・1 秒クールダウン） |
| Militia ステータス | HP 40 / Attack 4 / Armor 0 |
| その他 | Phase 2〜6 と同様 |

### Phase 8 操作（Phase8.unity）

| 操作 | 入力 |
|------|------|
| 死亡 | HP ≦ 0 でユニット消滅（敵・自軍とも） |
| HP 表示 | ユニット選択時、画面下部中央に HP バー |
| 攻撃中 | Militia がオレンジ系に色変化 |
| 人口 | 死亡で **Pop** 減少 |
| その他 | Phase 2〜7 と同様 |

### Phase 9 操作（Phase9.unity）

| 操作 | 内容 |
|------|------|
| CPU 経済 | 青系 TC + Villager 3 体が自動で木採集・House 建築・Villager 生産 |
| CPU 表示 | 画面右上 **CPU Wood / CPU Pop** |
| プレイヤー | Phase 8 と同様（CPU ユニット・TC は選択不可） |
| 木 | 両陣営で共有（先に切った側が取得） |

### Phase 10 操作（Phase10.unity）— 簡易 RTS 完成

| 操作 | 内容 |
|------|------|
| Food 採集 | Berry Bush または **Farm** を右クリック → TC 搬入 |
| Gold / Stone 採集 | **Gold Mine / Stone Mine** を右クリック → TC 搬入 |
| Farm 建築 | **Build Farm (60 Wood)** → 配置 → Villager 建築（**8 秒**） |
| Lumber Camp 建築 | **Build Lumber Camp (100 Wood)** → 森近くに配置 → Wood Drop-off |
| CPU 経済 | Phase 9 と同様（木採集・House・Villager） |
| CPU 軍事 | 自動で Barracks 建築 → Militia 生産（青系） |
| CPU 攻撃波 | **30 秒毎**に CPU Militia **全員**が最寄りの自軍へ攻撃（3 体未満でも 1 体から波開始） |
| ゲーム時間 | 画面上部中央 **Time / Next wave / Barracks after** |
| CPU Barracks | **開始 1 分後**（60 秒）+ Wood 50 から建築開始 |
| 反撃 | **自動ではない**。自軍 Militia を選択 → CPU Militia を**右クリック**で攻撃命令 |

## ディレクトリ

```
Assets/
  Input/          Input Actions
  Scripts/
    Camera/       RTS カメラ
    Core/         レイヤー定数など
    Input/        入力リーダー
    Combat/       攻撃管理
    AI/           CPU 経済・軍事 AI
    Selection/    選択管理
    Buildings/    建築・生産
    Economy/      資源・採集
    Units/        ユニット・UnitData
    Editor/       シーン構築ツール
  Scenes/         ゲームシーン
  Data/           ScriptableObject データ
```

## 開発ルール

[CONSTITUTION.md](CONSTITUTION.md) を参照。

- `*.inputactions` / `*.prefab` / `*.mat` 等は **手書き禁止**。Editor API または **AoE** メニューから生成すること。

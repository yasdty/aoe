# AoE RTS Engine

Age of Empires 2 ライクな Low-Spec RTS を Unity 6 で構築するプロジェクト。

## セットアップ

1. Unity Hub で **Unity 6** をインストール
2. 本リポジトリを Unity Hub の「Add project from disk」で開く
3. 初回インポート完了後、メニュー **AoE → Setup Phase1 Scene** を実行
4. `Assets/Scenes/Phase1.unity` を開いて Play

**Phase 2:** メニュー **AoE → Setup Phase2 Scene** → `Assets/Scenes/Phase2.unity` を Play

入力エラーが出る場合は **AoE → Fix Phase1 Input References** を実行してから Play してください。

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

## ディレクトリ

```
Assets/
  Input/          Input Actions
  Scripts/
    Camera/       RTS カメラ
    Core/         レイヤー定数など
    Input/        入力リーダー
    Selection/    選択管理
    Units/        ユニット・UnitData
    Editor/       シーン構築ツール
  Scenes/         ゲームシーン
  Data/           ScriptableObject データ
```

## 開発ルール

[CONSTITUTION.md](CONSTITUTION.md) を参照。

- `*.inputactions` / `*.prefab` / `*.mat` 等は **手書き禁止**。Editor API または **AoE** メニューから生成すること。

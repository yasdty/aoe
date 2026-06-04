# AoE RTS Engine — プロジェクト憲法

Low-Spec RTS Engine（AoE2 インスパイア）の開発ルール。

## 技術スタック

- Unity 6 / C# / URP / New Input System
- Asset Store 購入禁止（無料アセットのみ）
- NavMesh 依存禁止

## 最重要目標

MacBook Air 16GB 級で 4 チーム × 200 ユニット + 建築多数 + 大規模戦闘を動作させる。

## 最適化方針

**禁止:** Rigidbody 大量利用、Unit ごとの Update 乱立、Unit ごとの Raycast 乱立、Dynamic Shadow 乱用

**推奨:** Manager 更新方式、Object Pooling、GPU Instancing、Shared Material、Fixed Tick

## マルチプレイ将来互換

- ネットコード実装禁止（現時点）
- fixed tick / command queue / simulation 分離を意識した設計

## Unity アセット生成ルール（必須）

**Unity アセットを手書き（テキスト直書き・推測 YAML/JSON）してリポジトリに置かない。**

次の拡張子は、必ず **Unity Editor API** または **Editor 拡張（`Assets/Scripts/Editor/`）** から生成すること。

| 拡張子 | 例 |
|--------|-----|
| `*.inputactions` | `RTSInputActionsFactory` → `InputActionAsset` API + `ToJson()` + `AssetDatabase.ImportAsset` |
| `*.animator` | `AnimatorController` API / Editor メニュー |
| `*.controller` | 同上 |
| `*.prefab` | `PrefabUtility.SaveAsPrefabAsset` 等 |
| `*.mat` | `new Material` + `AssetDatabase.CreateAsset` 等 |

**禁止:**

- 上記ファイルの JSON/YAML を Cursor や外部エディタで直接作成してコミットすること
- 対応する `*.meta` の手書き（Unity がインポート時に自動生成させる）

**許可:**

- `ScriptableObject` を `AssetDatabase.CreateAsset` で生成（例: `UnitData`）
- シーンを `EditorSceneManager.SaveScene` で保存（例: **AoE → Setup Phase1 Scene**）
- Editor 実行時のみの一時ファイル書き込み（公式 API の出力を `ImportAsset` する場合）

## AI 実装ルール

- rewrite 禁止
- 一括リファクタ禁止
- Phase 単位、small diff only
- 必ず既存コード確認、推測禁止

## Phase 進行

MVP → 動作確認 → 次 Phase。Phase を飛ばさない。

## Phase 一覧

| Phase | 内容 |
|-------|------|
| 1 | RTS カメラ、地面、ユニット 1 体、選択、移動 |
| 2 | ドラッグ選択、複数選択、グループ移動 |
| 3 | TownCenter、Villager 生産 |
| 4 | 木材採集 |
| 5 | House 建築 |
| 6 | 人口システム |
| 7 | Barracks、Militia |
| 8 | 戦闘（死亡、HP 表示） |
| 9 | CPU AI（経済） |
| 10 | CPU 攻撃波、簡易 RTS 完成 |

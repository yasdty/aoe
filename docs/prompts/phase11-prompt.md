# Phase 11 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜10.5 完了（PoC + Visual Placeholder）  
> **ロードマップ:** [FOUNDATION_PHASES.md](../FOUNDATION_PHASES.md)  
> **使い方:** `@CONSTITUTION.md` と `@docs/RTS_IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 11 実装（Victory & Defeat）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
Low-Spec RTS（AoE2 インスパイア）。**Phase 1〜10.5 は完了済み。Phase 11 のみ実装すること。**

---

## ① Foundation 方針（必読・遵守）

[FOUNDATION_PHASES.md](../FOUNDATION_PHASES.md) の最重要方針を厳守:

- **AoE 機能を増やさない** — Archer / Food / Age Up 等は禁止
- **small diff** — 1 Phase = 1 目的（勝敗のみ）
- **既存ゲームを壊さない** — 完了時 `Phase10.unity` でコアループが動くこと
- **Simulation 優先** — 見た目の polish より終了条件・HP・攻撃対象拡張

リポジトリの `CONSTITUTION.md` も読み、以下を厳守:

- rewrite 禁止 / 一括リファクタ禁止 / small diff only
- 実装前に既存コードを読む（推測禁止）
- NavMesh 禁止 / Unit ごとの Update 乱立禁止
- Manager 更新方式を維持
- **Unity アセット手書き禁止**: Editor API または `Assets/Scripts/Editor/` から生成
- `*.meta` 手書き禁止
- Setup メニューは **Edit モード専用**
- **OnGUI は左上原点** — `GameUiInput.GuiRectToScreenRect`
- **`GetInstanceID()` 禁止**

---

## ② Phase 10.5 完了状態（現状）

`Phase10.unity` で動作確認済み:

- **プレイヤー** — 採集・House・TC 生産・Barracks・Militia・近接攻撃（ユニット対ユニット）・死亡・HP バー
- **CPU 経済 AI** — 木採集 / House / Villager 生産（目標 6 体）
- **CPU 軍事 AI** — Barracks 建築（60 秒後）/ Militia 生産 / **30 秒毎**攻撃波
- **Visual** — `EntityVisualBuilder` + Placeholder Prefab（Phase 10.5）
- **チーム** — Player（緑）/ Enemy CPU（青）

### 現状のギャップ（Phase 11 で解消）

| 項目 | 現状 |
|------|------|
| 建物 HP | **なし** — `TownCenter` / `House` / `Barracks` はダメージ不可 |
| 建物への攻撃 | **なし** — `AttackManager` は `Unit` ターゲットのみ |
| CPU → Player TC | Militia は TC **付近へ移動するのみ**（攻撃不可） |
| 勝敗 | **なし** — ゲーム終了条件・UI なし |

主要ファイル（**実装前に必ず開いて読む**）:

- `Assets/Scripts/Buildings/TownCenter.cs`
- `Assets/Scripts/Buildings/House.cs` / `Barracks.cs`
- `Assets/Scripts/Buildings/BuildingData.cs` / `PlacedBuildingData.cs`
- `Assets/Scripts/Combat/AttackManager.cs`
- `Assets/Scripts/AI/CpuMilitaryAiManager.cs`
- `Assets/Scripts/Selection/SelectionManager.cs`（右クリック攻撃）
- `Assets/Scripts/Units/Unit.cs`（`TakeDamage` / `Die` パターン参考）
- `Assets/Scripts/Selection/ResourceHudView.cs`（OnGUI HUD パターン参考）
- `Assets/Scripts/Editor/Phase10SceneBuilder.cs`
- `Assets/Scenes/Phase10.unity`

---

## ③ Phase 11 目的

**ゲームを終了可能にする** — Town Center 破壊で Victory / Defeat を表示し、以降の入力・AI を停止する。

### 今回実装するもの

1. **Building HP** — 共通コンポーネントまたは TC 専用 HP（**TownCenter 必須**）
2. **建物への攻撃** — Militia が TC（および可能なら House / Barracks）にダメージ
3. **TownCenter 破壊** — HP 0 で Destroy + 登録解除
4. **勝敗判定** — Enemy TC 破壊 → **Victory** / Player TC 破壊 → **Defeat**
5. **Victory / Defeat UI** — OnGUI オーバーレイ（既存 HUD と同パターン）
6. **ゲーム終了時の入力停止** — 選択・移動・攻撃・建築・生産を無効化
7. **`Phase10SceneBuilder` 更新** — `GameSessionManager`（名称は任意）を Systems に追加
8. **README / `docs/FOUNDATION_PHASES.md` 更新** — Phase 11 完了を明記

### 勝利 / 敗北条件（MVP — 厳守）

| 結果 | 条件 |
|------|------|
| **Victory** | **Enemy TownCenter** が破壊された |
| **Defeat** | **Player TownCenter** が破壊された |

- House / Barracks 破壊では **ゲーム終了しない**
- 引き分け条件は不要（同時破壊は MVP 範囲外。先着 or ログのみで可）

### 建物 HP（MVP 数値 — Inspector / ScriptableObject で調整可）

| 建物 | 推奨 Max HP | 備考 |
|------|-------------|------|
| TownCenter | **2400** | 勝敗に直結 |
| House | 900 | 攻撃可能にするなら |
| Barracks | 1200 | 同上 |

- TC の HP は `BuildingData` に追加が自然
- House / Barracks は `PlacedBuildingData` に追加
- **Phase 11 必須は TC のみ**。House / Barracks HP は small diff なら同 Phase で可、時間が足りなければ TC のみ先行

### 攻撃拡張（MVP）

- `AttackManager` に **建物ターゲット**対応を追加（`Unit` 攻撃は壊さない）
- プレイヤー: 選択 Militia + **右クリックで Enemy TC**（Collider ヒット）→ 攻撃命令
- CPU: 攻撃波で Player ユニットがいなければ **Player TC を攻撃ターゲット**に（移動のみから変更）
- ダメージ式: ユニット攻撃と同様 `Max(1, AttackPower - Armor)`。建物 Armor は 0 または Data フィールド

### ゲーム終了 UI（MVP）

- 画面中央に **「VICTORY」** または **「DEFEAT」**（大きめテキスト）
- サブテキスト例: `Press R to Restart` または `Reload scene to play again`（Restart 実装は任意）
- 終了後は **HUD は残してよい**（Wood / Pop 表示可）
- 本格 UI リデザインは範囲外

### 禁止（Phase 11 範囲外）

- Object Pool（Phase 12）
- Fixed Tick / Command Queue（Phase 15〜16）
- Spatial Hash / Benchmark
- 新ユニット・新資源・Age Up
- Phase 12 用の `ReturnToPool`（**TC 破壊時は MVP では `Destroy` 可**）
- リプレイ / セーブ / ネットワーク
- 新シーン `Phase11.unity` の必須化（**`Phase10.unity` を継続使用**）

---

## ④ 推奨実装順（30〜60 分単位）

| サブステップ | 内容 |
|-------------|------|
| 11-1 | `BuildingHealth`（または同等）— `TakeDamage` / `Die` / `MaxHp` / イベント |
| 11-2 | `BuildingData` / `PlacedBuildingData` に `maxHp` 追加 + 既存 Data アセット更新（Editor） |
| 11-3 | `TownCenter` に HP 連携 + 破壊時 `ProductionManager.Unregister` |
| 11-4 | `AttackManager` — 建物ターゲット Job 追加（Unit 攻撃と共存） |
| 11-5 | `SelectionManager` — 右クリックで建物（TC）攻撃 |
| 11-6 | `CpuMilitaryAiManager` — ユニット不在時 TC 攻撃 |
| 11-7 | `GameSessionManager` — 勝敗状態 / `IsGameOver` / TC 破壊イベント購読 |
| 11-8 | `VictoryDefeatHudView` — OnGUI + 入力ブロック連携 |
| 11-9 | `Phase10SceneBuilder` + ドキュメント更新 |

---

## ⑤ 実装前に必ず出力（コードを書く前）

1. **変更対象ファイル一覧**（新規 / 変更 / 削除）
2. **新規追加ファイル一覧**
3. **影響範囲**（Attack / Selection / AI / Production / HUD）
4. **リスク**（AttackManager 拡張、TC 破壊後の Null 参照、CPU AI 暴走）
5. **ロールバック方法**（git revert 単位、SceneBuilder 再実行）
6. **完了条件**（下記チェックリスト）
7. **テスト手順**（手動 Play 手順）

---

## ⑥ 実装後に必ず出力

1. **変更内容サマリ**
2. **変更ファイル一覧**
3. **テスト結果**（チェックリスト各項目）
4. **既知の制限**（Pool 未導入、House 破壊時 Pop 減少なし等）

---

## ⑦ 技術メモ（設計のヒント・強制ではない）

### BuildingHealth 例

```csharp
public class BuildingHealth : MonoBehaviour
{
    public float MaxHp { get; private set; }
    public float CurrentHp { get; private set; }
    public bool IsAlive => CurrentHp > 0f;
    public event Action<BuildingHealth> Destroyed;

    public void TakeDamage(float amount) { /* HP 0 → Destroyed → Destroy(gameObject) */ }
}
```

- TC 破壊時: `ProductionManager.Unregister` は `OnDisable` で既に呼ばれる — **追加で AI が null TC を参照しないよう `GameSessionManager` で早期 return**

### AttackManager 拡張方針（small diff）

- **案 A（推奨）:** `AttackJob` に `Unit targetUnit` **または** `BuildingHealth targetBuilding`（nullable）
- **案 B:** `ICombatTarget` インターフェース — 差分が大きくなりがちなので Phase 11 では慎重に

```csharp
public static void IssueAttack(IReadOnlyList<Unit> attackers, BuildingHealth building) { /* ... */ }
```

### SelectionManager — 建物攻撃

- 右クリック Raycast で `Unit` が取れなければ `TownCenter` / `BuildingHealth` を試す
- **自軍 TC への誤攻撃は禁止**（Team チェック）

### GameSessionManager

```csharp
public enum MatchState { Playing, Victory, Defeat }
public static bool IsGameOver => state != Playing;
```

- `Update` 先頭で `IsGameOver` なら `CpuEconomyAiManager` / `CpuMilitaryAiManager` / プレイヤー入力を止める
- 各 Manager に `if (GameSessionManager.IsGameOver) return;` を **最小箇所**に追加

### TC HP バー（任意）

- `UnitHpBarView` と同パターンの `BuildingHpBarView` は **任意**（時間があれば）
- MVP 完了には **不要**

### テスト加速

- SceneBuilder または Inspector で TC `maxHp = 500` 等の **開発用低 HP** を README に記載しても可
- デフォルトは上表の推奨値

### Phase10SceneBuilder

- `GameSessionManager` を Systems GameObject に AddComponent
- `VictoryDefeatHudView` を SelectionManager オブジェクト等に AddComponent（既存 HUD と同所）
- **新シーン不要** — `AoE → Setup Phase10 Scene` 再実行で配線

---

## ⑧ シーン / Editor

- 検証シーン: **`Assets/Scenes/Phase10.unity`**（継続使用）
- `AoE → Setup Phase10 Scene` — GameSession / Victory HUD 配線を追加
- Phase 1〜10.5 シーン・メニューは壊さない

---

## ⑨ 完了条件（Phase 11 MVP）

- [ ] **Player TC** に HP があり、ダメージを受けられる
- [ ] **Enemy TC** に HP があり、ダメージを受けられる
- [ ] プレイヤー Militia で **Enemy TC を攻撃**できる（右クリック）
- [ ] CPU Militia が Player ユニット不在時 **Player TC を攻撃**できる（移動のみから改善）
- [ ] **Enemy TC 破壊 → VICTORY 表示** + ゲーム操作停止
- [ ] **Player TC 破壊 → DEFEAT 表示** + ゲーム操作停止
- [ ] 勝敗後も **Console エラーなし**（Null 参照なし）
- [ ] **Phase 10 コアループ** — 勝敗前の採集・建築・生産・戦闘・CPU が壊れていない
- [ ] `docs/FOUNDATION_PHASES.md` Phase 11 を ✅ に更新

---

## ⑩ テスト手順（Play チェックリスト）

1. `Phase10.unity` を Play
2. 通常プレイ — 採集 / House / Villager / Barracks / Militia が Phase 10 同様に動く
3. プレイヤー Militia を選択 → Enemy TC を右クリック → TC HP が減る
4. TC HP 0 → Enemy TC 消滅 → **VICTORY** 表示 → 移動・攻撃・建築不可
5. シーン再 Play → CPU 攻撃波で Player TC を削る（または開発用低 HP）
6. Player TC 破壊 → **DEFEAT** 表示
7. Console にエラーなし

Phase 11 のみ実装。Phase 12 以降（Pool / Benchmark 等）に触れない。

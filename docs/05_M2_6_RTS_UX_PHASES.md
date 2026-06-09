# AoE RTS Engine — Milestone 2.6: RTS UX ロードマップ（Phase 31〜34）

> **Milestone:** M2.6 — RTS UX（AoE2 操作感の基盤）  
> **前提:** [04_M2_5_ECONOMY_POLISH_PHASES.md](04_M2_5_ECONOMY_POLISH_PHASES.md)（Phase 21〜30）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **前:** M2.5 Economy Polish / **次:** [06_M3_MILITARY_PHASES.md](06_M3_MILITARY_PHASES.md)（Phase 35〜40）

> **番号変更（2026-06）:** M2.5 に Phase 25（Info Panel）・Phase 28（Sheep Herding）挿入に伴い、旧 Phase 29〜32 を **Phase 31〜34** に繰り下げ。

M2.5 で経済ループが揃ったあと、**M3 Military（弓兵等）に入る前**に AoE2 の操作基盤を整える。

---

## 用語整理 — 「生産キュー」と「建物キュー」は別物

| 用語 | AoE2 での意味 | 本プロジェクト Phase |
|------|---------------|---------------------|
| **ユニット生産キュー** | TC / Barracks 等で **Villager・Militia 等を複数予約**（Q 連打、Shift+5 体）。資源は **キュー追加時に消費** | **Phase 31** ← 今回 M2.6 の主題 |
| **集合地点（Rally / Gather Point）** | 生産建物を選択 → 右クリックで **生まれたユニットの行き先**（資源・前線） | **Phase 33** |
| **建築配置** | House / Farm 等を **地面に配置**（ゴースト → 村民が建築）。1 村民 = 1 現場 | 既存 `BuildingPlacementManager`（Phase 5〜） |
| **建築キュー（厳密には存在しない）** | AoE2 に「TC で House を 5 件予約」のような **建物専用キューはない**。Shift+連続配置は **配置の連打** であり、ユニット生産キューとは別 | **M2.6 対象外**（将来 Shift 連続配置は任意） |

**結論:** ユーザーが言う「Villager / Militia の作成予約」= **ユニット生産キュー（Phase 31）**。建物そのものを並べて予約する機能ではない。

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 31 | Unit Production Queue | TC / Barracks で複数ユニット予約 + Barracks Q | ✅ 実装済み |
| 32 | Idle Unit UX | 待機村民・軍の表示 + 選択ホットキー | ✅ 実装済み |
| 33 | Rally Point | 生産建物の集合地点（右クリック） | ⬜ 未着手 |
| 34 | Control Groups | Ctrl+数字で選択グループ保存 | ⬜ 未着手 |
| — | **Phase10 サンドボックス拡張** | マップ拡大・資源増・CPU バランス（**M2.6 完了後**） | ⬜ 未着手 |

**M2.6 完了条件（Phase 31〜34）:**

- TC で Q 連打 → Villager が **キューに積まれる**（現状 1 体のみ → 複数）
- Barracks 選択中 Q → Militia **キュー**（選択が外れにくい）
- HUD に **待機村民数**、ホットキーで待機ユニットへジャンプ
- TC / Barracks に **Rally Point** を設定し、生産ユニットが自動移動
- **Ctrl+1〜9** でユニットグループ保存・呼び出し

---

## Phase 31 — Unit Production Queue ✅

**目的:** AoE2 の **ユニット生産キュー**。1 建物あたり複数ユニットを予約し、順次生産。

**実装済み（Phase 31）:**

- `ProductionManager` / `BarracksProductionManager` — 建物ごと FIFO、最大 **15**、先頭のみ Tick
- キュー追加時に Food / Wood Spend + Pop チェック
- `ProductionPanelView` / `BarracksPanelView` — `Queue: N`、生産中も Q / ボタン有効
- Barracks **Q キー**（`TrainVillager` アクション流用）
- CPU AI — `IsProducing` ガード削除（キュー空きなら 2 秒周期で追加）

**現状ギャップ（Phase 31 以前）:**

- `ProductionManager` / `BarracksProductionManager` が `IsProducing()` で **2 体目を拒否**
- Barracks に **Q ホットキーなし**（TC のみ Q 対応）
- 生産中パネルが無効化 → 連続生産 UX が悪い

**実装方針:**

1. **`ProductionManager`** — `activeJobs` を **建物ごとの FIFO キュー**（最大 **15**、AoE2 原版準拠）
2. **`BarracksProductionManager`** — 同パターン
3. **資源** — キュー **追加時に即 Spend**（Pop / Food / Wood チェック）。キャンセル時返金は Phase 31 では **任意**（MVP はキャンセル UI なしでも可）
4. **Tick** — 先頭ジョブ完了 → Spawn → 次ジョブ開始（待機中も `remainingSeconds` で 1 本だけ Tick）
5. **HUD** — キュー長表示（例: `Queue: 3`）、生産中は先頭のプログレスバー
6. **Barracks `BarracksPanelView`** — TC と同様 **Q キー**で `TrainMilitiaCommand`（Barracks 選択時）
7. **選択維持** — 生産命令で `SelectionManager` の Barracks 選択を **クリアしない**（既存がそうなら変更不要）

**AoE2 参考（Phase 31 では省略可）:**

- Shift+Q → 5 体一括キュー
- 複数 Barracks 選択 → 最短キューへ分散

**プロンプト:** [prompts/phase31-prompt.md](prompts/phase31-prompt.md)

---

## Phase 32 — Idle Unit UX ✅

**目的:** 待機中の Villager / Militia が **一目でわかる** + 素早く選択。

**実装済み（Phase 32）:**

- `UnitIdleTracker` — 村民 / 軍事の待機判定（Gather / Build / Attack 除外）
- `IdleUnitHudView` — `Idle Villagers: N` / `Idle Military: M` + ボタン
- `IdleUnitSelectionController` — `.` / Shift+. / `,` ホットキー + カメラジャンプ
- `SelectionManager.SelectSingleUnit` / `SelectUnits` 公開 API
- Input — `SelectNextIdleVillager`（period）/ `SelectNextIdleMilitary`（comma）

**AoE2 参考:**

- 画面上部 **Idle Villager カウント** + ボタン
- **Shift+.** — 全待機村民選択 / **.** — 次の待機村民
- **,** — 次の待機軍事ユニット（全待機軍のみ選択は vanilla になし）

**実装方針（MVP）:**

1. **`UnitIdleTracker`** または `UnitManager` 拡張 — `UnitState.Idle` かつ Gather/Attack/Build ジョブなし = 待機
2. **HUD** — `Idle Villagers: N`（ResourceHudView 付近）
3. **ホットキー** — `.` = 次の待機村民を選択（カメラジャンプ任意）
4. **ビジュアル（任意）** — 待機村民の小さなアイコン / ティント

**プロンプト:** [prompts/phase32-prompt.md](prompts/phase32-prompt.md)

---

## Phase 33 — Rally Point ⬜

**目的:** 生産建物の **Gather Point / Rally Point**。生まれたユニットが自動で指定地点へ。

**AoE2 参考:**

- 建物選択 → **地面右クリック** で旗設置
- TC Rally を **木 / 金** に向けると村民が自動採集開始

**実装方針:**

1. **`TownCenter` / `Barracks`** — `Vector3? rallyPoint` フィールド
2. **SelectionManager** — 建物選択中の地面右クリック → Rally 設定（移動命令より優先度要設計）
3. **Spawn 後** — `UnitSpawner` または Production 完了時に `SetMoveTarget(rally)` 
4. **TC + 資源 Rally** — Rally 先が Tree 等なら `GatherCommand` 発行（任意 / サブステップ）

**プロンプト:** [prompts/phase33-prompt.md](prompts/phase33-prompt.md)

---

## Phase 34 — Control Groups ⬜

**目的:** **Ctrl+1〜9** で選択ユニットを保存・呼び出し。M3 弓兵混合編成の前提。

**実装方針:**

1. **`ControlGroupManager`** — 9 スロット × ユニット参照リスト
2. **Ctrl+数字** — 現在選択をスロットに保存
3. **数字** — スロット呼び出し（選択置換）
4. **Shift+数字** — 追加選択（既存 Shift 選択と整合）

**プロンプト:** [prompts/phase34-prompt.md](prompts/phase34-prompt.md)（未作成）

---

## Phase 34 完了後 — Phase10 サンドボックス拡張 ⬜

> **位置づけ:** M2.6（Phase 31〜34）**完了後**、M3 Military（Phase 35）**直前**のバランス調整。正式 Phase 番号は付けない（M3 は Phase 35 から継続）。

**背景:** 現状 `Phase10.unity` は Player / CPU TC 間 **約 35m**、木 **28 本前後**、Gold / Stone **各陣営 1 鉱山** と狭く、Phase 30 以降の 4 資源経済 + CPU 軍事 AI では **資源枯渇・早期ラッシュ** が起きやすい。M2.6 の操作 UX 検証は狭いマップでも足りるため、**マップ拡張は M2.6 後にまとめて行う**。

**目的:**

- プレイ時間を延ばし、4 資源 + 複数建物 + 軍事の **本格テスト環境** を整える
- M3（Archer / Cavalry 等）入り前に **広い固定マップ** で経済・軍事を試せるようにする

**触る場所（主）:**

| 項目 | ファイル / 対象 |
|------|----------------|
| TC 間隔・資源配置 | `Assets/Scripts/Editor/Phase10SceneBuilder.cs` — `TreePositions` / Berry / Deer / Sheep / Boar / Gold / Stone 配列 |
| TC 座標 | `PlayerTownCenterPosition` / `CpuTownCenterPosition`（例: CPU z を `-55` 〜 `-70`） |
| カメラ | `CameraFocus` — 両 TC の中間 |
| 地面 | Plane スケール（Setup メニューで再生成） |
| 資源 **量** | ScriptableObject（`DefaultTree` Wood 量、Berry Food 量等）— 必要時のみ |
| CPU 難易度 | `DefaultAttackWaveIntervalSeconds` / `DefaultBarracksBuildDelaySeconds` / 初期 Villager 数 |

**MVP 目安（調整可能）:**

| 要素 | 現状 | 拡張目安 |
|------|------|----------|
| TC 間隔 | ~35m | **55〜70m** |
| 木 | 28 本 | **40〜50 本**（Player / CPU 側に分散） |
| Berry Bush | 各 3 | **各 4〜5** |
| Gold / Stone | 各陣営 1 | **各 2**（遠方に追加可） |
| Deer / Sheep | 少数 | **各 +1〜2** |

**やらないこと:**

- ランダムマップ生成（M4 / P3 以降）
- 新ゲームシステム・新ユニット種
- CPU AI ロジックの rewrite

**完了条件:**

- `AoE → Setup Phase10 Scene` 再実行後、Player が **10 分以上** 4 資源経済を回せる
- CPU ラッシュが **即死レベルでない**（間隔・距離の調整で確認）
- Phase 31〜34 で追加した操作 UX が **回帰しない**

**進行:** Phase 34 Play 確認 OK → 本項目 → M3 Phase 35 へ。

---

## M2.6 で意図的に後回し

| 項目 | 先送り先 |
|------|----------|
| Shift+5 体一括キュー | Phase 31 拡張 or M4 |
| キューから個別キャンセル（クリックで返金） | Phase 31 拡張 |
| 複数建物分散キュー | M3 以降 |
| Shift+連続建築配置 | 建築 UX 別 Phase |
| Minimap / 本格 uGUI | M4 以降 |
| 全待機軍事ユニット一括選択 | AoE2 にも弱い — Phase 32 拡張 |
| Phase10 マップ拡張・資源増 | **M2.6 完了後**（上記セクション）— M2.6 中は混ぜない |

---

## 進め方

1. M2.5 全 Phase 完了を確認
2. [CONSTITUTION.md](../CONSTITUTION.md) + [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)
3. **Phase 31 から順に** small diff → `Phase10.unity` Play 確認
4. Phase 34 完了 → **Phase10 サンドボックス拡張**（マップ・資源・CPU バランス）
5. 上記完了後 → [06_M3_MILITARY_PHASES.md](06_M3_MILITARY_PHASES.md) Phase 35 へ

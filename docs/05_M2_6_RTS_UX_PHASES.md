# AoE RTS Engine — Milestone 2.6: RTS UX ロードマップ（Phase 31〜34）

> **Milestone:** M2.6 — RTS UX（AoE2 操作感の基盤）  
> **前提:** [04_M2_5_ECONOMY_POLISH_PHASES.md](04_M2_5_ECONOMY_POLISH_PHASES.md)（Phase 21〜30）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **前:** M2.5 Economy Polish / **次:** [06_M2_7_SANDBOX_PHASES.md](06_M2_7_SANDBOX_PHASES.md)（Phase 35）

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
| 33 | Rally Point | 生産建物の集合地点（右クリック） | ✅ 実装済み |
| 34 | Control Groups | Ctrl+数字で選択グループ保存 | ✅ 実装済み |
| — | **Phase10 サンドボックス拡張** | → **M2.7 Phase 35** へ移行 | 参照 [06_M2_7](06_M2_7_SANDBOX_PHASES.md) |

**M2.6 完了条件（Phase 31〜34）:**

- TC で Q 連打 → Villager が **キューに積まれる**（現状 1 体のみ → 複数）
- Barracks 選択中 Q → Militia **キュー**（選択が外れにくい）
- HUD に **待機村民数**、ホットキーで待機ユニットへジャンプ
- TC / Barracks に **Rally Point** を設定し、生産ユニットが自動移動
- **Ctrl+1〜9** でユニットグループ保存・呼び出し

**→ 上記すべて ✅ 完了（Phase 31〜34）**

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

## Phase 33 — Rally Point ✅

**目的:** 生産建物の **Gather Point / Rally Point**。生まれたユニットが自動で指定地点へ。

**実装済み（Phase 33）:**

- `ProductionRallyPoint` — Ground / Tree / Berry / Farm / Gold / Stone
- `TownCenter` / `Barracks` — `SetRally` / `HasRally`
- `SetRallyPointCommand` + `SelectionManager.TrySetProductionRallyFromClick`
- `ProductionRallyApplier` — Spawn 直後に Move / Gather 適用
- Info Panel — `Rally: Set` / `Rally: None`

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

## Phase 34 — Control Groups ✅

**目的:** **Ctrl+1〜9** で選択ユニットを保存・呼び出し。M3 弓兵混合編成の前提。

**実装済み（Phase 34）:**

- `ControlGroupManager` — 9 スロット Save / Recall / 死亡 prune
- `ControlGroupInputController` — Ctrl+1〜9 保存 / 1〜9 Recall / Shift+1〜9 追加
- `SelectionManager.SelectUnitsAdditive` — Shift+数字用
- `HandleUnitDied` — 全スロットから除去

**実装方針:**

1. **`ControlGroupManager`** — 9 スロット × ユニット参照リスト
2. **Ctrl+数字** — 現在選択をスロットに保存
3. **数字** — スロット呼び出し（選択置換）
4. **Shift+数字** — 追加選択（既存 Shift 選択と整合）

**プロンプト:** [prompts/phase34-prompt.md](prompts/phase34-prompt.md)

---

## Phase 34 完了後

**次:** [06_M2_7_SANDBOX_PHASES.md](06_M2_7_SANDBOX_PHASES.md)（Phase 35 — マップ拡大・資源増・CPU バランス）→ M3 Phase 36 へ。

---

## M2.6 で意図的に後回し

| 項目 | 先送り先 |
|------|----------|
| Shift+5 体一括キュー | Phase 31 拡張 or M4 |
| キューから個別キャンセル（クリックで返金） | Phase 31 拡張 |
| 複数建物分散キュー | M3 以降 |
| Shift+連続建築配置（壁ドラッグ） | **M4 Phase 48** — [08_M4](08_M4_GAMEPLAY_PHASES.md) |
| Minimap / 本格 uGUI | M5 Phase 49〜51 |
| 全待機軍事ユニット一括選択 | AoE2 にも弱い — Phase 32 拡張 |
| Phase10 マップ拡張・資源増 | **M2.7 Phase 35** — [06_M2_7](06_M2_7_SANDBOX_PHASES.md) |

---

## 進め方

1. M2.5 全 Phase 完了を確認
2. [CONSTITUTION.md](../CONSTITUTION.md) + [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)
3. **Phase 31 から順に** small diff → `Phase10.unity` Play 確認
4. Phase 34 完了 → [06_M2_7_SANDBOX_PHASES.md](06_M2_7_SANDBOX_PHASES.md) Phase 35
5. M2.7 完了後 → [07_M3_MILITARY_PHASES.md](07_M3_MILITARY_PHASES.md) Phase 36 へ

# AoE RTS Engine — Milestone 2.5: Economy Polish ロードマップ（Phase 21〜30）

> **Milestone:** M2.5 — Economy Polish（AoE2 経済ループの完成）  
> **前提:** [03_M2_ECONOMY_PHASES.md](03_M2_ECONOMY_PHASES.md)（Phase 17〜20）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **前:** M2 Economy（Phase 17〜20） / **次:** [05_M2_6_RTS_UX_PHASES.md](05_M2_6_RTS_UX_PHASES.md)（Phase 31〜34）→ [06_M3_MILITARY_PHASES.md](06_M3_MILITARY_PHASES.md)（Phase 35〜40）

> **番号変更（2026-06）:** Phase 25 に **Selection Info Panel**、Phase 28 に **Sheep Herding + 動物徘徊** を挿入。旧 Phase 25〜28（Boar / Mill / Aggro / CPU）は **+1〜+2** 繰り下げ。M2.6 は **31〜34**、M3 は **35〜40** へ。

M2 で 4 資源の「存在」は完成したが、AoE2 の Dark Age 経済体験（採取ループ・Drop-off・狩り・CPU 対等性）が不足している。**M3 Military（弓兵等）に入る前に本マイルストンを完了する。**

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 21 | Gather Repeat | 搬入後に同ノードへ自動復帰（採取リピート） | ✅ 実装済み |
| 22 | Farm + Spawn | Farm 1 人制限 + 建物スポーン周囲グリッド | ✅ 実装済み |
| 23 | Mining Camp | Gold / Stone の Drop-off 拠点 | ✅ 実装済み |
| 24 | Hunting | Deer / Sheep（被動 Food 源・静止狩り） | ✅ 実装済み |
| 25 | Selection Info Panel | 左クリック選択時の詳細 + 資源残量表示 | ✅ 実装済み |
| 26 | Boar | 反撃する Food 源 + 狩り戦闘 | ⬜ 未着手 |
| 27 | Mill | Food Drop-off 拠点（Farm 近く搬入） | ⬜ 未着手 |
| 28 | Sheep Herding | 羊の無所属→所属・誘導 + 動物徘徊 | ⬜ 未着手 |
| 29 | Militia Aggro | 近接軍の簡易自動攻撃（射程内の敵） | ⬜ 未着手 |
| 30 | CPU 4 Resources | CPU 経済 AI が 4 資源 + 狩りを利用 | ⬜ 未着手 |

**M2.5 完了条件:**

- 村民が右クリック 1 回で **枯渇 or 中断まで採取ループ** する
- Farm は **1 枚 1 村民**、建物生産ユニットは **周囲グリッド** に出現
- Wood / Food / Gold / Stone すべて **Camp / Mill 含む Drop-off** が機能
- **Berry → 狩り → Farm** の序盤 Food ルートが揃う
- **選択オブジェクトの HP / 攻撃 / 資源残量** が AoE2 風に表示される
- Militia が **近接自動攻撃**（簡易版）する
- CPU が **4 資源経済** を回せる

---

## Phase 21 — Gather Repeat ✅

**目的:** 搬入完了後、同じ Tree / Berry / Farm / Gold / Stone へ戻り採取を継続。AoE2 の「村民 1 命令 = 長期作業」を実現。

**実装:** `GatherManager` / `FoodGatherManager` / `MineralGatherManager` — Deposit 完了後に `MoveTo*` 状態へ復帰

**プロンプト:** [prompts/phase21-prompt.md](prompts/phase21-prompt.md)

---

## Phase 22 — Farm One-Worker + Spawn Grid ✅

**目的:** AoE2 準拠の Farm 占有（1 Farm = 同時 1 村民）と、TC / Barracks 生産時の周囲グリッド配置。

**実装:**

- `FoodGatherManager.IsFarmOccupiedByOther` — 他村民が採取中の Farm は拒否
- `FoodGatherManager.HasAssignableFarmGatherers` — `SelectionManager` が 1 人も割当不可なら false
- `BuildingSpawnFormation` — TC / Barracks 出口前 √n グリッド（16 スロット循環）

**完了条件:**

- 2 人目が同一 Farm を右クリックしても採取開始しない
- 連続生産でユニットが建物周囲に重なりにくい

**プロンプト:** [prompts/phase22-prompt.md](prompts/phase22-prompt.md)

---

## Phase 23 — Mining Camp ✅

**目的:** Lumber Camp と同パターンで Gold / Stone の Drop-off 拠点。鉱山近くへの搬入で採掘効率改善。

**実装:**

- `MiningCamp` / `MiningCampRegistry` / `MiningCampData`
- `MineralGatherManager.GetDepositPosition` — 最寄り TC / Mining Camp
- HUD **Build Mining Camp** + `BuildingPlacementManager.EnterMiningCampPlacementMode`

**完了条件:**

- Gold / Stone 搬入先が最寄り Camp または TC
- 100 Wood / 6 秒 / HP 400

**プロンプト:** [prompts/phase23-prompt.md](prompts/phase23-prompt.md)

---

## Phase 24 — Hunting（Deer / Sheep）✅

**目的:** 被動動物から Food 採集。Berry Bush に次ぐ序盤 Food 源。**MVP は静止カプセル + 狩りジョブのみ。**

**実装:**

- `DeerResource` / `SheepResource` + `IHuntableFoodResource`
- `FoodGatherManager.huntJobs` + `HuntFoodCommand`
- `Phase10SceneBuilder` Deer / Sheep 配置

**Phase 24 で意図的に省略（後続 Phase へ）:**

| 省略項目 | 先送り先 |
|----------|----------|
| 羊の無所属 → 発見チーム所属 + 移動誘導 | **Phase 28** |
| Deer / Sheep の徘徊・停止・逃げ | **Phase 28** |
| 選択時の詳細 / 資源残量 UI | **Phase 25** |

**プロンプト:** [prompts/phase24-prompt.md](prompts/phase24-prompt.md)

---

## Phase 25 — Selection Info Panel ✅

**目的:** AoE2 風の **選択オブジェクト詳細パネル**。Play テスト・Boar 実装前の横断 UX 基盤。

**実装:**

- `SelectionInfoPanelView` — 左下 OnGUI パネル
- `SelectionManager` — Resource / Farm / House / Camp 左クリック選択
- `AoE → Add Selection Info Panel (Phase10)` — 既存シーンへ追加

**プロンプト:** [prompts/phase25-prompt.md](prompts/phase25-prompt.md)

---

## Phase 26 — Boar ⬜

**目的:** 攻撃してくる Boar。狩り + 軽い戦闘の橋渡し（M3 弓兵前の combat 拡張）。

**実装方針:**

- `BoarUnit` or `BoarResource` — HP あり、殴られると反撃
- 村民は狩れるが被ダメージリスク
- Militia は Boar を攻撃可能（既存 AttackManager 流用）
- Food 搬入は Deer と同経路
- **Phase 25 パネル** で Boar HP / 攻撃力を選択表示

**プロンプト:** [prompts/phase26-prompt.md](prompts/phase26-prompt.md)

---

## Phase 27 — Mill ⬜

**目的:** Food の Drop-off 拠点。Farm / Berry / 狩り肉を TC 以外に搬入可能。

**実装方針:**

- `Mill` / `MillData` / `MillRegistry`（Lumber Camp パターン）
- `FoodGatherManager.GetDepositPosition` — 最寄り TC / Mill
- HUD **Build Mill**

**パラメータ（MVP）:**

| 項目 | 値（案） |
|------|----------|
| コスト | 100 Wood |
| 建築時間 | 6 秒 |
| HP | 400 |

**プロンプト:** [prompts/phase27-prompt.md](prompts/phase27-prompt.md)（未作成）

---

## Phase 28 — Sheep Herding + Animal Locomotion ⬜

**目的:** AoE2 の **羊スカウト経済** と動物の見た目 polish。Phase 24 で省略した動物 UX をここで完結。

**Phase 24 からの引き継ぎ（必須スコープ）:**

| 項目 | 内容 |
|------|------|
| **Sheep 無所属** | 初期 `UnitTeam` なし / Neutral。誰の資源でもない |
| **発見 → 所属** | 自軍ユニットが視界 or 接触で **発見チームに所属**（CPU も同様） |
| **Sheep 移動** | 所属後 **左クリック選択 + 右クリック移動**（TC へ誘導 = AoE2 Herding） |
| **Sheep 追従（MVP）** | 村民が近くを通るとゆっくり追従（簡易。本格 Shepherd AI は optional） |
| **Deer 徘徊** | ランダム短距離移動 → 停止を繰り返す（**逃げる**は optional / Phase 28 サブ） |
| **狩りとの整合** | 所属 Sheep も `HuntFoodCommand` 対象。Neutral Sheep は **発見前は狩れない or 発見トリガー**（実装時に AoE2 準拠で決定） |

**共通基盤（推奨）:**

- `AnimalLocomotion` or `PassiveAnimalBehaviour` — Tick ベースの低速 wander（`UnitManager` パターン参考、個別 Update 禁止）
- `SheepResource` 拡張 — `ownerTeam`, `isDiscovered`, 移動ターゲット

**Phase 28 で意図的に後回し:**

| 項目 | 先送り先 |
|------|----------|
| Deer 被狩り時の逃走 | M3 以降 or Phase 28 拡張 |
| 本格 Pathfinding 回避 | M3 Phase 40 Formation / P3 |
| CPU 羊誘導 AI | **Phase 30** CPU 4 Resources と連携 |

**プロンプト:** [prompts/phase28-prompt.md](prompts/phase28-prompt.md)（未作成）

---

## Phase 29 — Militia Basic Aggro ⬜

**目的:** 命令待ち Militia が射程内の敵を自動攻撃。M3 弓兵の遠距離 Aggro 設計の土台。

**現状:** Militia は右クリック攻撃のみ。

**実装方針:**

- `AttackManager` または `UnitAggroManager` — Idle 近接ユニットが `UnitSpatialIndex` で最寄り敵を検索
- **簡易版のみ:** 移動命令中は Aggro しない / Stand Ground UI は Phase 39（M3）へ

**完了条件:**

- 待機 Militia が敵 Villager / Militia 進入時に自動攻撃
- プレイヤー Move 命令は Aggro より優先（または Move で Aggro 解除）

**プロンプト:** [prompts/phase29-prompt.md](prompts/phase29-prompt.md)（未作成）

---

## Phase 30 — CPU 4 Resources ⬜

**目的:** `CpuEconomyAiManager` が Wood / Food / Gold / Stone + 狩り / Farm / Camp を利用。

**現状:** CPU は主に Wood 採集 + House + Villager。Berry / Farm / Gold / Stone 未対応。

**実装方針:**

- 評価ループに Food / Gold / Stone 需要判断
- Villager 割当: Berry → 狩り → Farm / Lumber Camp / Mining Camp 近傍
- Mining Camp / Mill / Lumber Camp 建築判断（Wood 余裕時）
- **Phase 28 依存:** CPU 羊発見・誘導（optional サブタスク）
- **Command 化は不要**（既存 CPU 方針: Manager 直接呼び出し）

**完了条件:**

- CPU が 4 資源を増やし Villager / Militia 生産を継続
- プレイヤーと同様に採取リピート（Phase 21 依存）が CPU にも効く

**プロンプト:** [prompts/phase30-prompt.md](prompts/phase30-prompt.md)（未作成）

---

## M2.5 で意図的に後回し

| 項目 | 理由 | 先送り先 |
|------|------|----------|
| 羊の無所属・誘導・追従 | Phase 24 は静止狩り MVP | **Phase 28 Sheep Herding** |
| Deer / Sheep 徘徊・逃げ | 動物 AI + 経路。Boar 戦闘と設計分離 | **Phase 28**（逃げは optional） |
| Melee / Pierce 装甲 2 種 | MVP は `armor` 1 値 | M3 **Phase 36** Counter System |
| 選択詳細 / 資源残量 UI | Phase 24 スコープ外 | **Phase 25 Selection Info Panel** |
| 移動時ユニット押し出し / RVO | 性能・工数大 | M3 Phase 40 Formation / P3 Pathfinding |
| Stand Ground / Defensive スタンス UI | Phase 29 は簡易 Aggro のみ | M3 Phase 39 |
| ユニット生産キュー / Idle / Rally | AoE2 操作基盤 | **M2.6 Phase 31〜33** |
| 市場・交易 | AoE2 中盤コンテンツ | M4 |
| 時代昇格 | 大規模 | M4 |

---

## 進め方

1. [CONSTITUTION.md](../CONSTITUTION.md)
2. [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)
3. 本ファイル → 該当 [prompts/phaseN-prompt.md](prompts/)
4. **Phase 21 から順番に** small diff → `Phase10.unity` Play 確認
5. M2.5 全完了後 → [05_M2_6_RTS_UX_PHASES.md](05_M2_6_RTS_UX_PHASES.md) Phase 31 へ

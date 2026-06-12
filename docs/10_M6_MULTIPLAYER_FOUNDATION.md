# AoE RTS Engine — Milestone 6: 4-Player & World Scale ロードマップ（Phase 57〜66）

> **Milestone:** M6 — **4 人対戦基盤** + **2v2** + **大型マップ** + **ゲームプレイ補完** + **Fog of War**  
> **前提:** [09_M5_VISUAL_UI_PHASES.md](09_M5_VISUAL_UI_PHASES.md)（Phase 49〜56）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **憲法:** 現時点では **ネットコード実装禁止**。本マイルストンは **ローカル 4 人（人間1 + CPU3）** と **2v2** を優先。

---

## 2026-06 方針変更（ロードマップ再編）

| 旧 M6 | 新方針 |
|-------|--------|
| Phase 59 決定論を最優先 | **Phase 66** に後ろ倒し（LAN 直前まで不要） |
| Phase 60 リプレイ | **後回し（オプション）** — 現プロジェクトでは不要 |
| Phase 61 ホットシート | **後回し（オプション）** — 現プロジェクトでは不要 |
| 1v1 CPU のみ | **Phase 59〜65** で **4人・2v2・大マップ・Fog** を追加 |
| Fog 直前にゲームプレイ不足 | **Phase 62〜64** で **敵 HP 表示・建物修理・複数 Villager 建設** を挿入 |

**プレイ目標（M6 完了時）:**

- **FFA:** 人間 1 + CPU 3（4 隅スポーン）
- **2v2:** 人間 + CPU 同盟 vs CPU 2（チーム視界共有は Fog と連携）
- **大型マップ** — 4 陣営向けに地面・`MapBounds`・カメラ・ミニマップ拡張
- **敵 HP 表示** — 敵ユニット・敵建物の選択と HP 確認
- **建物修理** — Villager による損傷建物の修理（木材消費）
- **複数 Villager 建設** — 同一建設現場への複数人割当・速度加速
- **Fog of War** — 自軍視界のみ（同盟はチーム視界共有）

---

## 進め方（推奨順）

```
57 Entity ID & PlayerId     … 4 プレイヤー枠・Entity 参照の土台 ✅
58 CPU Command Queue        … CPU×3 を同一 Command パイプラインへ ✅
59 Four-Player Match        … 1 人間 + CPU 3・マッチ設定・4 隅スポーン
60 Team & 2v2               … 同盟チーム・味方攻撃不可・2v2 勝利条件
61 Large Map                … 地面拡大・資源配置・カメラ/ミニマップ
62 Enemy HP Display         … 敵ユニット・敵建物の選択と HP 表示
63 Building Repair          … Villager 修理コマンド・木材コスト
64 Multi-Villager Build     … 同一サイト複数 builder・建築速度スケール
65 Fog of War               … VisionManager・未探索/霧表示
66 Deterministic Sim        … LAN 直前（任意タイミングで実施可）
```

**スキップ（オプション・M6 完了条件外）:** 旧 Phase 60 リプレイ / 旧 Phase 61 ホットシート / 本格オンライン（M9 以降）

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 57 | Entity ID & PlayerId | int Entity ID / `PlayerId` 0〜3 | ✅ 完了 |
| 58 | CPU Command Queue | CPU AI を `IGameCommand` 経由に統一 | ✅ 完了 |
| 59 | Four-Player Match | 人間1 + CPU3・マッチ設定・4 隅 TC / 資源 per Player | ⬜ 未着手 — **次** |
| 60 | Team & 2v2 | `TeamId` / 同盟・人間+CPU vs CPU×2 | ⬜ 未着手 |
| 61 | Large Map | 大型地面・`MapBounds`・スポーン・カメラ範囲 | ⬜ 未着手 |
| 62 | Enemy HP Display | 敵ユニット・敵建物の選択と HP 表示 | ⬜ 未着手 |
| 63 | Building Repair | Villager による建物修理（木材消費） | ⬜ 未着手 |
| 64 | Multi-Villager Build | 同一建設現場に複数 Villager・速度加速 | ⬜ 未着手 |
| 65 | Fog of War | `VisionManager` + ミニマップ/ワールド未視認 | ⬜ 未着手 |
| 66 | Deterministic Sim | Tick 順固定・RNG・float 削減（LAN 準備） | ⬜ 未着手 |

---

## Phase 57 — Entity ID & PlayerId ✅

**目的:** GameObject 参照から脱却し、**4 プレイヤー**をデータ上表現できるようにする。

| 項目 | 実装 |
|------|------|
| Registry | `EntityRegistry` — Unit / Building / Resource に int ID |
| Command | `IGameCommand` を ID 参照へ段階移行（adapter で旧 Command 維持可） |
| Player | `PlayerId` 0〜3（憲法 4 チーム目標） |
| 互換 | 既存 `UnitTeam.Player` / `Enemy` は **当面維持**し `PlayerId` とマッピング |

**プロンプト:** [prompts/phase57-prompt.md](prompts/phase57-prompt.md)

**やらないこと:** 4 隅スポーン / 2v2 / Fog（Phase 59〜65）

---

## Phase 58 — CPU Command Queue ✅

**目的:** `CpuEconomyAiManager` / `CpuMilitaryAiManager` が `CommandQueue.Enqueue` を使用。CPU を **PlayerId ごとに複数インスタンス化** 可能に。

**プロンプト:** [prompts/phase58-prompt.md](prompts/phase58-prompt.md)

**やらないこと:** 4 人スポーン / 2v2 / Fog（Phase 59〜65）

---

## Phase 59 — Four-Player Match ⬜

**目的:** **人間 1 + CPU 3** の 4 人マッチ（ローカル FFA）。

| 項目 | MVP |
|------|-----|
| マッチ設定 | 簡易 UI or Debug メニュー — 「人間1 + CPU3」 |
| スポーン | マップ 4 隅に TC + 初期 Villager |
| 経済 | Wood / Food / Gold / Stone / Pop を **PlayerId 単位** |
| AI | PlayerId 1〜3 それぞれに CPU 経済・軍事 |
| 勝利 | 敵 **Player** の TC 全破壊（2v2 は Phase 60） |

**プロンプト:** [prompts/phase59-prompt.md](prompts/phase59-prompt.md)

---

## Phase 60 — Team & 2v2 ⬜

**目的:** **人間 + CPU 同盟 vs CPU 2**。

| 項目 | MVP |
|------|-----|
| Team | `TeamId` — 例: Team0 = {Player0, Player1}, Team1 = {Player2, Player3} |
| 戦闘 | 同盟への攻撃命令不可 |
| 勝利 | 敵チームの TC 全破壊 |
| Fog 準備 | 同盟は **同一チーム視界**（Phase 65 で実装） |

---

## Phase 61 — Large Map ⬜

**目的:** 4 陣営向け **大型マップ**。

- Ground scale 拡大（Phase10 の約 2〜4 倍目安 — 実装で調整）
- `MapBounds` / ミニマップ / `RTSCameraController` パン範囲
- 木・鉱山・ベリーを 4 陣営周辺に再配置

---

## Phase 62 — Enemy HP Display ⬜

**目的:** 戦闘・攻城時に **敵の残 HP を確認**できるようにする（現状は敵選択不可）。

| 項目 | MVP |
|------|-----|
| 選択 | 敵ユニット・敵建物を左クリック選択可能 |
| HP 表示 | `SelectionInfoPanelView` / `UnitHpBarView` で敵 HP バー・数値 |
| 制限 | 敵ユニットは命令不可（移動・攻撃命令は自軍のみ） |
| 識別 | 兵種名・建物名・装甲等の識別情報も表示 |

**現状:** `SelectionManager` が `UnitTeam.Player` のみ選択許可。敵は右クリック攻撃のみ。

---

## Phase 63 — Building Repair ⬜

**目的:** AoE2 的 **建物修理** — 損傷建物を Villager で修復。

| 項目 | MVP |
|------|-----|
| 命令 | 自軍 Villager 選択 → 損傷自軍建物を右クリックで修理 |
| コスト | 木材消費（建物種別ごとに比率 — MVP は固定レート可） |
| Sim | `RepairManager` or `BuildingPlacementManager` 拡張 — 修理ジョブ |
| 中断 | 移動命令で修理中断（建築と同様） |

**現状:** `repair` 関連コード未実装。

---

## Phase 64 — Multi-Villager Build ⬜

**目的:** **同一建設現場に複数 Villager** を載せ、人数に応じて建築速度を上げる。

| 項目 | MVP |
|------|-----|
| データ | `ConstructionSite` — 単一 `builder` から **複数 builder リスト**へ |
| 速度 | `remainingTime` 減算を builder 人数でスケール（上限あり — 例: 最大 5 人） |
| 割当 | 建設中サイトへ追加 Villager を右クリック or 自動割当 |
| CPU | CPU 建築も複数人対応（Command 経由） |

**現状:** `BuildingPlacementManager.ConstructionSite` は **1 サイト = 1 Villager** 固定。

---

## Phase 65 — Fog of War ⬜

**目的:** AoE2 的 **戦場の霧** — 自軍（+ 同盟）視界外は未表示 or グレーアウト。

| 項目 | 実装 |
|------|------|
| Sim | `VisionManager` — ユニット・建築の視界半径 |
| View | 地形・ユニット・建築の表示/非表示（View 層） |
| ミニマップ | 未探索エリアの暗転 |
| 2v2 | **同盟チーム視界共有** |

---

## Phase 66 — Deterministic Sim ⬜

**目的:** 将来 LAN / Lockstep のため **同一 Command → 同一結果**。

- `SimulationTick` 登録順の明示化
- `SimulationRng`（シード固定）
- 移動・距離判定の整数化（段階的）

**ローカル 1+3 CPU のみなら Phase 66 は後回し可。** オンライン着手直前に実施。

---

## オプション（M6 完了条件外・後回し）

| 旧 Phase | 内容 | 備考 |
|----------|------|------|
| — | Replay & Snapshot | Command Log ファイル I/O — 必要になったら別 Phase |
| — | Hotseat | 同一 PC 交互操作 — 不要 |
| M9+ | Online MP | 本格 Lockstep + Transport |

---

## M6 完了条件（改訂）

- [x] `PlayerId` 0〜3 + `EntityRegistry` が Command / Sim の参照基盤（Phase 57 — Move / AttackUnit は ID 化済み）
- [x] CPU が Command パイプライン経由（PlayerId ごと — Phase 58、1 CPU インスタンス）
- [ ] **人間1 + CPU3** で 4 隅マッチが Play 可能
- [ ] **2v2**（人間+CPU vs CPU×2）が Play 可能
- [ ] **大型マップ** + 4 隅スポーン
- [ ] **敵 HP 表示**（敵選択 + HP バー）
- [ ] **建物修理** + **複数 Villager 建設**
- [ ] **Fog of War**（同盟視界共有含む）
- [ ] Phase 10 コアループ回帰（採集・建築・戦闘・勝敗）

---

## M6 以降（候補）

| 候補 | 内容 |
|------|------|
| M7 Pathfinding | グリッド A* — 大規模戦闘・障害物回り |
| M8 Map Gen | ランダムマップ生成 |
| M9 Online MP | Phase 66 + Transport + 切断・再同期 |

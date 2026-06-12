# AoE RTS Engine — Milestone 6: 4-Player & World Scale ロードマップ（Phase 57〜63）

> **Milestone:** M6 — **4 人対戦基盤** + **2v2** + **大型マップ** + **Fog of War**  
> **前提:** [09_M5_VISUAL_UI_PHASES.md](09_M5_VISUAL_UI_PHASES.md)（Phase 49〜56）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **憲法:** 現時点では **ネットコード実装禁止**。本マイルストンは **ローカル 4 人（人間1 + CPU3）** と **2v2** を優先。

---

## 2026-06 方針変更（ロードマップ再編）

| 旧 M6 | 新方針 |
|-------|--------|
| Phase 59 決定論を最優先 | **Phase 63** に後ろ倒し（LAN 直前まで不要） |
| Phase 60 リプレイ | **後回し（オプション）** — 現プロジェクトでは不要 |
| Phase 61 ホットシート | **後回し（オプション）** — 現プロジェクトでは不要 |
| 1v1 CPU のみ | **Phase 59〜62** で **4人・2v2・大マップ・Fog** を追加 |

**プレイ目標（M6 完了時）:**

- **FFA:** 人間 1 + CPU 3（4 隅スポーン）
- **2v2:** 人間 + CPU 同盟 vs CPU 2（チーム視界共有は Fog と連携）
- **大型マップ** — 4 陣営向けに地面・`MapBounds`・カメラ・ミニマップ拡張
- **Fog of War** — 自軍視界のみ（同盟はチーム視界共有）

---

## 進め方（推奨順）

```
57 Entity ID & PlayerId     … 4 プレイヤー枠・Entity 参照の土台
58 CPU Command Queue        … CPU×3 を同一 Command パイプラインへ
59 Four-Player Match        … 1 人間 + CPU 3・マッチ設定・4 隅スポーン
60 Team & 2v2               … 同盟チーム・味方攻撃不可・2v2 勝利条件
61 Large Map                … 地面拡大・資源配置・カメラ/ミニマップ
62 Fog of War               … VisionManager・未探索/霧表示
63 Deterministic Sim        … LAN 直前（任意タイミングで実施可）
```

**スキップ（オプション・M6 完了条件外）:** 旧 Phase 60 リプレイ / 旧 Phase 61 ホットシート / 本格オンライン（M9 以降）

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 57 | Entity ID & PlayerId | int Entity ID / `PlayerId` 0〜3 | ✅ 完了 |
| 58 | CPU Command Queue | CPU AI を `IGameCommand` 経由に統一 | ⬜ 未着手 — **次** |
| 59 | Four-Player Match | 人間1 + CPU3・マッチ設定・4 隅 TC / 資源 per Player | ⬜ 未着手 |
| 60 | Team & 2v2 | `TeamId` / 同盟・人間+CPU vs CPU×2 | ⬜ 未着手 |
| 61 | Large Map | 大型地面・`MapBounds`・スポーン・カメラ範囲 | ⬜ 未着手 |
| 62 | Fog of War | `VisionManager` + ミニマップ/ワールド未視認 | ⬜ 未着手 |
| 63 | Deterministic Sim | Tick 順固定・RNG・float 削減（LAN 準備） | ⬜ 未着手 |

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

**やらないこと:** 4 隅スポーン / 2v2 / Fog（Phase 59〜62）

---

## Phase 58 — CPU Command Queue ⬜

**目的:** `CpuEconomyAiManager` / `CpuMilitaryAiManager` が `CommandQueue.Enqueue` を使用。CPU を **PlayerId ごとに複数インスタンス化** 可能に。

**プロンプト:** [prompts/phase58-prompt.md](prompts/phase58-prompt.md)

**やらないこと:** 4 人スポーン / 2v2 / Fog（Phase 59〜62）

---

## Phase 59 — Four-Player Match ⬜

**目的:** **人間 1 + CPU 3** の 4 人マッチ（ローカル）。

| 項目 | MVP |
|------|-----|
| マッチ設定 | 簡易 UI or Debug メニュー — 「人間1 + CPU3」 |
| スポーン | マップ 4 隅に TC + 初期 Villager |
| 経済 | Wood / Food / Gold / Stone / Pop を **PlayerId 単位** |
| AI | PlayerId 1〜3 それぞれに CPU 経済・軍事 |
| 勝利 | 敵 **Player** の TC 全破壊（2v2 は Phase 60） |

---

## Phase 60 — Team & 2v2 ⬜

**目的:** **人間 + CPU 同盟 vs CPU 2**。

| 項目 | MVP |
|------|-----|
| Team | `TeamId` — 例: Team0 = {Player0, Player1}, Team1 = {Player2, Player3} |
| 戦闘 | 同盟への攻撃命令不可 |
| 勝利 | 敵チームの TC 全破壊 |
| Fog 準備 | 同盟は **同一チーム視界**（Phase 62 で実装） |

---

## Phase 61 — Large Map ⬜

**目的:** 4 陣営向け **大型マップ**。

- Ground scale 拡大（Phase10 の約 2〜4 倍目安 — 実装で調整）
- `MapBounds` / ミニマップ / `RTSCameraController` パン範囲
- 木・鉱山・ベリーを 4 陣営周辺に再配置

---

## Phase 62 — Fog of War ⬜

**目的:** AoE2 的 **戦場の霧** — 自軍（+ 同盟）視界外は未表示 or グレーアウト。

| 項目 | 実装 |
|------|------|
| Sim | `VisionManager` — ユニット・建築の視界半径 |
| View | 地形・ユニット・建築の表示/非表示（View 層） |
| ミニマップ | 未探索エリアの暗転 |
| 2v2 | **同盟チーム視界共有** |

---

## Phase 63 — Deterministic Sim ⬜

**目的:** 将来 LAN / Lockstep のため **同一 Command → 同一結果**。

- `SimulationTick` 登録順の明示化
- `SimulationRng`（シード固定）
- 移動・距離判定の整数化（段階的）

**ローカル 1+3 CPU のみなら Phase 63 は後回し可。** オンライン着手直前に実施。

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
- [ ] CPU が Command パイプライン経由（PlayerId ごと）
- [ ] **人間1 + CPU3** で 4 隅マッチが Play 可能
- [ ] **2v2**（人間+CPU vs CPU×2）が Play 可能
- [ ] **大型マップ** + 4 隅スポーン
- [ ] **Fog of War**（同盟視界共有含む）
- [ ] Phase 10 コアループ回帰（採集・建築・戦闘・勝敗）

---

## M6 以降（候補）

| 候補 | 内容 |
|------|------|
| M7 Pathfinding | グリッド A* — 大規模戦闘・障害物回り |
| M8 Map Gen | ランダムマップ生成 |
| M9 Online MP | Phase 63 + Transport + 切断・再同期 |

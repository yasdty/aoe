# AoE RTS Engine — Milestone 6: Multiplayer Foundation ロードマップ（Phase 54〜58）

> **Milestone:** M6 — Multiplayer / Replay Foundation  
> **前提:** [09_M5_VISUAL_UI_PHASES.md](09_M5_VISUAL_UI_PHASES.md)（Phase 49〜53）完了  
> **実装状況:** [IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)  
> **憲法:** 現時点では **ネットコード実装禁止**。本マイルストンは **ローカルで検証可能な同期基盤**まで。

---

## 重要: UI だけではマルチプレイにならない

| 誤解 | 実際 |
|------|------|
| M5 で UI ができれば LAN 対戦できる | **ならない** — UI は表示層。同期には **同一シミュレーション入力** が必要 |
| Fixed Tick があればマルチ OK | **不十分** — 決定論性・Entity ID・全プレイヤー Command 化が必要 |
| CPU を人間に置き換えればよい | **不十分** — 入力遅延・順序・チェックサム・切断処理が別途必要 |

**マルチプレイに必要な層（下から順）:**

```
1. Fixed Tick Simulation          ✅ Phase 15
2. IGameCommand + Command Queue     ✅ Phase 16（Player のみ）
3. Entity ID（GameObject 非依存）   ⬜ Phase 54
4. 決定論シミュレーション           ⬜ Phase 56
5. CPU も Command 経由              ⬜ Phase 55
6. Command Log 保存 / 再生          ⬜ Phase 57
7. 複数 PlayerId / Team 拡張        ⬜ Phase 54
8. Network Transport（Lockstep）    ⬜ Phase 58 以降
9. View 分離（クライアント別 UI）   ⬜ Phase 49
```

M5 完了時点の **マルチプレイ準備度: 約 55〜60%**。  
M6 完了時点の **ローカル同期プロトタイプ: 約 75〜80%**（ホットシート or リプレイ再生まで）。  
**実際のオンライン対戦**は M6 以降の Transport Phase が別途必要。

---

## 現状のマルチプレイ互換設計の評価

| 要素 | 状態 | マルチ向きか |
|------|------|--------------|
| Fixed Tick 20 TPS | ✅ | ◎ |
| Player Command Queue | ✅ | ◎ |
| CommandLog 記録 | △ 記録のみ | ○ |
| CPU 直接 Manager 呼び出し | ❌ | × リプレイ非互換 |
| float 演算 / 登録順依存 | ❌ | × Lockstep 非決定論 |
| GameObject 参照 in Command | ❌ | × ネットワーク非互換 |
| UnitTeam 2 値 enum | △ | △ PlayerId 拡張要 |
| OnGUI in Manager | ❌ → M5 で解消 | △ |
| Static Singleton Manager | ❌ | △ テスト・複数 World 困難 |

**結論:** **意図はあるが、現状は「マルチ前提の設計思想」段階（30〜40%）**。M5 + M6 完了で **技術的にホットシート / リプレイ可能な土台**には到達するが、**UI 完成 ≠ マルチプレイ可能**。

---

## Phase 一覧

| Phase | 名称 | 目的 | 状態 |
|-------|------|------|------|
| 54 | Entity ID & PlayerId | int Entity ID / PlayerId 4 チーム拡張 | ⬜ 未着手 |
| 55 | CPU Command Queue | CPU AI を `IGameCommand` 経由に統一 | ⬜ 未着手 |
| 56 | Deterministic Sim | Tick 順序固定・RNG シード・float 削減 | ⬜ 未着手 |
| 57 | Replay & Snapshot | Command Log ファイル I/O + 状態スナップショット | ⬜ 未着手 |
| 58 | Hotseat / Net Shell | ローカル 2 人交互 or ネットワーク層スタブ（送受信のみ） | ⬜ 未着手 |

**M6 完了条件:**

- 同一 Command Log から **同一結果の Replay 再生**ができる
- CPU と Player が **同一 Command パイプライン**
- 2 PlayerId で **ホットシート 1v1** が可能（UI は簡易で可）
- ネットワーク層は **スタブ**（実 LAN は M6 後の別 Phase）

---

## Phase 54 — Entity ID & PlayerId ⬜

**実装:**

- `EntityRegistry` — Unit / Building / Resource に int ID
- `IGameCommand` は ID 参照に移行（段階的。旧 Command は adapter）
- `PlayerId` 0〜3（憲法 4 チーム目標）

---

## Phase 55 — CPU Command Queue ⬜

**実装:** `CpuEconomyAiManager` / `CpuMilitaryAiManager` が `CommandQueue.Enqueue` を使用

---

## Phase 56 — Deterministic Sim ⬜

**実装:** `SimulationTick` 登録順固定 / `SimulationRng` / 移動・戦闘の整数化検討

---

## Phase 57 — Replay & Snapshot ⬜

**実装:** Tick 番号付き Command Log JSON / 簡易 Save-Load

---

## Phase 58 — Hotseat / Net Shell ⬜

**実装:** PlayerId 切替 UI / `INetworkTransport` インターフェース（localhost echo のみ）

---

## M6 以降（ロードマップ外・候補）

| 候補 | 内容 |
|------|------|
| M7 Pathfinding | グリッド A* — 大規模戦闘の前提 |
| M8 Map Gen | ランダムマップ |
| M9 Online MP | 本格 Lockstep + 切断・再同期 |

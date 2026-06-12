# Phase 56 実行プロンプト

> **状態:** ✅ 完了  
> **前提:** Phase 1〜55 完了（View Split / uGUI HUD / Minimap / Unit Animation）  
> **マイルストン:** M5 — **Combat VFX & Audio**（M5 最終 Phase）  
> **ロードマップ:** [09_M5_VISUAL_UI_PHASES.md](../09_M5_VISUAL_UI_PHASES.md)  
> **関連:** Phase 52 View 分離 / Phase 55 `UnitAnimationView` / `AttackManager` / `WatchTowerDefenseManager`  
> **使い方:** `@CONSTITUTION.md` と `@docs/IMPLEMENTATION_STATUS.md` と `Assets/Scripts/` を添付したうえで、以下を Agent へ貼り付け。

---

# 依頼: AoE RTS Engine — Phase 56 実装（Combat VFX & Audio）

あなたはシニア Unity RTS エンジニア。Unity 6 / C# / URP / New Input System。  
**Phase 56 のみ実装。** 戦闘の **見た目・音フィードバック MVP**（rewrite 禁止）。

**前提:** ダメージ適用は **即時**（`AttackManager` / `BoarAttackManager` / `WatchTowerDefenseManager` が `TakeDamage`）。**弾道シミュレーションは Simulation に入れない** — VFX は View 層の演出のみ。

---

## ① 目的

| 項目 | MVP |
|------|-----|
| 分離 | Phase 52 方針 — **`CombatFeedbackView`**（View 層）が Particle / Mesh / Audio を再生。Simulation Manager は **イベント DTO のみ発行** |
| 遠距離 | **Archer** / **Watch Tower** 等 — 攻撃命中時に **コスメティック弾丸**（attacker → target へ lerp、ダメージタイミングは既存 Sim のまま） |
| 近接 | **Militia** / Boar 等 — 命中時 **ヒットフラッシュ**（短い Particle or scale pulse — View のみ） |
| 建築被弾 | Unit → Building 攻撃時も **同一ヒット VFX**（建築 root 位置） |
| 死亡 | 命中で Kill した場合 — **短い死亡 puff / 色フェード**（0.2〜0.4s）→ 既存 `UnitPool.Return`。**Sim の即 Return は維持可** — View が corpse 演出を挟むなら **delay Return を View 側コールバックで 1 箇所に集約**（small diff 優先） |
| SE | **フック API** + プレースホルダー Clip（Editor 生成 or 空 AudioClip — Asset Store 禁止）— Melee Hit / Ranged Hit / Death の **3 種以上** |
| プール | 弾丸 Mesh / Particle / AudioSource を **Object Pool** — GC 回避 |
| Editor | `AoE → Setup Combat VFX (Phase56)` — Prefab / Material / ParticleSystem / 空 SE を Editor API で生成 |

**やらないこと:** 本格 VFX アセットパック / 音楽 BGM / 採集・建築・UI SE / 弾速に連動したダメージ遅延 / NavMesh / M6 同期 / Phase 57 Entity ID 移行

---

## ② 現状（読み取り用）

| 項目 | 値 |
|------|-----|
| 攻撃 Sim | `AttackManager.TickSimulation` — cooldown 満了で即 `TakeDamage` |
| Boar | `BoarAttackManager` — 同上 |
| 塔 | `WatchTowerDefenseManager` — Pierce 即時ダメージ |
| 遠距離判定 | `UnitData.attackDamageType == Pierce` かつ `attackRange > 3`（Archer パターン — Phase 55 `UnitAnimationProfile.Archer` と同基準） |
| 近接 | Melee range ≈ 1.5 |
| View 先例 | `UnitAnimationView`, `PlacementPreviewView`, `MinimapView` — LateUpdate / イベント駆動 |
| VFX / Audio | **未実装** — grep ヒットなし |

**Phase 55 との関係:** `UnitAnimationView` の Attack 状態アニメは維持。Phase 56 は **命中瞬間の追加演出**（弾丸・火花・SE）。

---

## ③ 実装前に必ず読むファイル

| 領域 | 参考 |
|------|------|
| 戦闘 Sim | `AttackManager.cs`, `BoarAttackManager.cs`, `WatchTowerDefenseManager.cs` |
| ダメージ | `CombatDamageResolver.cs`, `AttackDamageType` |
| View 分離 | `PlacementPreviewView`, `IPlacementPreviewView` — DTO + View パターン |
| ユニット View | `UnitAnimationView`, `UnitPool` |
| 建築 | `BuildingHealth` — 被弾位置 |
| Editor | `UnitAnimationSetupPhase55.cs`, `PlaceholderVisualMeshFactory.cs` — Editor API 生成先例 |

---

## ④ 実装タスク

### 1. CombatFeedbackEvent（DTO）

```csharp
// 例 — 名前空間は View or Combat.Events 推奨
public struct CombatFeedbackEvent
{
    public Vector3 sourceWorldPosition;
    public Vector3 targetWorldPosition;
    public CombatFeedbackKind kind; // MeleeHit, RangedHit, BuildingHit, UnitDeath, ...
    public bool targetWasKilled;
}
```

- Simulation から **struct + static Raise** または **interface `ICombatFeedbackSink`** で View へ通知
- **禁止:** Manager 内で `ParticleSystem` / `AudioSource` を直接 `GetComponent`

### 2. Simulation 側フック（最小 diff）

| 箇所 | タイミング |
|------|------------|
| `AttackManager` | `TakeDamage` 直前 or 直後 — attacker / target 位置、Melee vs Ranged 判定 |
| `BoarAttackManager` | boar へダメージ適用時 |
| `WatchTowerDefenseManager` | 塔 → unit ダメージ時（Ranged） |

- 既存 Debug.Log は維持可
- **ダメージ計算・cooldown ロジックは変更しない**

### 3. CombatFeedbackView（`Assets/Scripts/View/`）

- シーン singleton or `GameplayCanvas` 子 — `Awake` でイベント購読
- **Ranged:** プールから弾丸 Visual（Sphere / Capsule mesh）を rent → lerp 0.15〜0.35s → hit VFX → return
- **Melee / BuildingHit:** ターゲット位置で短い Particle burst or MaterialPropertyBlock フラッシュ
- **Death（任意 MVP）:** `targetWasKilled` で puff → `Unit.Die` の Return タイミング調整（**1 フレーム以上 Dead アニメが見える**程度で可 — Phase 55 改善）

### 4. CombatAudioView（同 View 層 or 同一コンポーネント）

```csharp
public static class CombatAudioHooks
{
    public static void PlayMeleeHit(Vector3 worldPosition);
    public static void PlayRangedHit(Vector3 worldPosition);
    public static void PlayUnitDeath(Vector3 worldPosition);
}
```

- `AudioSource` プール — 同時再生数上限（例: 8）
- Clip は Editor メニューで **空 or 短い procedural WAV** を `Resources/CombatAudio/` に生成
- 音量・距離減衰は MVP 最小（2D でも可）

### 5. Editor / Phase10

- `[MenuItem("AoE/Setup Combat VFX (Phase56)")]` — Particle prefab、弾丸 material、Audio clip placeholder
- `Phase10SceneBuilder` — `CombatFeedbackView` を Systems or GameplayCanvas 配下に Ensure（既存 Ensure パターン）
- Play 前にメニュー 1 回実行

### 6. 回帰

- Phase 55 アニメ / Phase 54 ミニマップ / Phase 53 HUD
- 戦闘ダメージ値・cooldown **不変**
- Console エラーなし
- 800 体目標 — 画面外 VFX はスキップ or 距離カリング（過剰最適化不要、カメラ近傍のみで可）

---

## ⑤ 制約

- small diff only / rewrite 禁止
- Simulation が `ParticleSystem` / `AudioClip` 型を **フィールド保持しない**
- NavMesh 禁止
- Asset Store / 外部 VFX・SE パック禁止 — **Editor API 生成 or コード生成のみ**
- Prefab 手編集最小
- **弾丸 VFX はダメージタイミングを遅延しない**（演出と Sim 乖離 OK — AoE2 も表示上の矢は cosmetic）

---

## ⑥ Play 確認

1. `Phase10.unity` — `AoE → Setup Combat VFX (Phase56)` → Ctrl+S
2. **Archer** → 敵 Militia — **矢（球）が飛び、命中でヒット VFX + SE**
3. **Militia** 近接 — **火花/フラッシュ + Melee SE**（弾丸なし）
4. **Watch Tower** — 敵接近時 **塔から弾丸 + ヒット**
5. **Boar 狩り** — 近接ヒット VFX
6. **建築攻撃** — 壁/TC 被弾位置にヒット VFX
7. **死亡** — 一瞬 puff（実装した場合）→ 消える
8. Phase 54/55 回帰 — ミニマップ・アニメ・HUD
9. Console エラーなし

---

## ⑦ 完了時ドキュメント

- [x] `IMPLEMENTATION_STATUS.md` — Phase 56 ✅ / M5 完了
- [x] `09_M5_VISUAL_UI_PHASES.md` — Phase 56 ✅
- [x] 本プロンプト — ✅

---

Phase 56 のみ。**M6（Phase 57 Entity ID）には触れない。**

> **次:** M6 Phase 57 Entity ID & PlayerId — [phase57-prompt.md](phase57-prompt.md) / [10_M6_MULTIPLAYER_FOUNDATION.md](../10_M6_MULTIPLAYER_FOUNDATION.md)

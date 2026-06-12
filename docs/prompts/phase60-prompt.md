# Phase 60 — CPU Difficulty System

> **状態:** ✅ 完了  
> **ロードマップ:** [10_M6_MULTIPLAYER_FOUNDATION.md](../10_M6_MULTIPLAYER_FOUNDATION.md)

## 目的

CPU を「APM 無限の即時反応」から **AoE2 風の人間らしいプレイ**へ変更する。

## 実装サマリー

| コンポーネント | 役割 |
|----------------|------|
| `CpuDifficulty` / `CpuDifficultyProfile` | Easy / Normal / Hard / Hardest パラメータ |
| `CpuDifficultySettings.EffectiveDifficulty` | **Debug モード時は常に Easy** |
| `CpuAiActionQueue` | `reactionDelay` + 行動種別ランダム遅延後に Command 実行 |
| `CpuEconomyAiManager` | `decisionInterval` / `maxActionsPerCycle` / `villagerTarget` |
| `CpuMilitaryAiManager` | 軍量ベース攻撃（wave タイマー廃止） |

## 難易度パラメータ（正本）

| | Easy | Normal | Hard | Hardest |
|---|------|--------|------|---------|
| reactionDelay | 3.0s | 1.5s | 0.75s | 0.25s |
| decisionInterval | 5.0s | 3.0s | 2.0s | 1.0s |
| maxActionsPerCycle | 1 | 2 | 3 | 5 |
| villagerTarget | 20 | 40 | 60 | 80 |
| armyRatio | 0.20 | 0.30 | 0.40 | 0.50 |
| attackThreshold | 8 | 15 | 25 | dynamic |
| attackConfidence | 1.5 | 1.2 | 1.0 | 0.8 |

## 操作

- **AoE → CPU Difficulty** メニュー（シーン保存）
- Play 中 **P キー** / HUD ボタンで難易度サイクル（Debug 時は Easy 固定）
- Debug Balance モードでは敵は常に Easy

## 完了条件

- [x] 条件成立時に即 Command 実行しない（遅延キュー経由）
- [x] 1 サイクルあたり行動数上限
- [x] 攻撃がタイマー波ではなく軍量ベース
- [x] Debug モードで CPU = Easy

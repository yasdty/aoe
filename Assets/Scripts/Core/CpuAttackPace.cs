namespace AoE.RTS.Core
{
    /// <summary>
    /// Phase10 テスト用 CPU 攻撃ペース。Balance Mode（Debug/AoE2）とは独立。
    /// </summary>
    public enum CpuAttackPace
    {
        /// <summary>約2分は攻撃波・CPU 自動反撃なし（既定）</summary>
        Relaxed = 0,
        /// <summary>Debug 倍率付きの従来どおりの早攻め（戦闘テスト用）</summary>
        Aggressive = 1
    }
}

namespace RePuzzleKnights.Scripts.Domain.Enums
{
    /// <summary>
    /// 敵に付与される状態異常（デバフ）のタイプ
    /// </summary>
    public enum StatusEffectType
    {
        None = 0,
        Burn,       // 火傷：継続ダメージ
        Slow,       // 減速：移動速度低下
        Stun,       // スタン：行動不能
        DefDebuff   // 防御力デバフ：物理防御力低下
    }
}

public class Define
{
    public enum UIEvent
    {
        Click,
        Pressed,
        PointerDown,
        PointerUp,
        Drag,
        BeginDrag,
        EndDrag,
        OnPointerExit,
        PointerEnter,
    }

    public enum EnemyType
    {
        Basic,
        Tank,
        Runner,
        Split,
        Revive,
        MiddleBoss,
        Boss,
    }

    public enum TowerType
    {
        Basic,
        Cannon,
        Slow,
        Sniper,
        Poison,
        Lightning,
    }

    public enum SkillType
    {
        Block,
        ArrowRain,
        Freeze
    }

    public enum StatType
    {
        Damage,
        AttackSpeed,
        Range,
        Speed,
        CriticalChance,
        CiriticalDamage,
    }

    public enum ModifierType
    {
        Flat,
        Percent,
    }

    public enum CardCategory { A, B, C, D }
    public enum CardEffectType
    {
        //A
        DamageUp, AttackSpeedUp, RangeUp, CriticalChanceUp,
        //B
        GoldInstant, KillRewardUp, BuildCostDown, WaveBonusDouble,
        //C
        FreeTower, EnemyHpDown, CoreHpUp, SynergyAmp,
        //D
        SkillSelect, SkillPointUp
    }
}


public class StatModifier
{
    public Define.StatType StatType { get; private set; }
    public float Value { get; private set; }
    public Define.ModifierType ModifierType { get; private set; } = Define.ModifierType.Percent;

    public StatModifier(Define.StatType statType, float value, Define.ModifierType modifierType)
    {
        StatType = statType;
        Value = value;
        ModifierType = modifierType;
    }

    public float Apply(float baseValue)
    {
        return ModifierType switch
        {
            Define.ModifierType.Flat => baseValue + Value,
            Define.ModifierType.Percent => baseValue * Value,
            _ => throw new System.ArgumentOutOfRangeException()
        };
    }
}

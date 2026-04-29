using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffHandler : MonoBehaviour, ITickable
{
    /// <summary>
    /// 현재 활성화 목록
    /// </summary>
    /// <returns></returns>
    private readonly List<IBuff> _effects = new();
    /// <summary>
    /// 현재 적용중인 배율, 더하기 목록
    /// </summary>
    /// <returns></returns>
    private readonly List<StatModifier> _modifiers = new();
    /// <summary>
    /// 만료된 효과(임시보관 -> 재사용가능)
    /// </summary>
    /// <returns></returns>
    private readonly List<IBuff> _expiredBuffer = new();
    // 타워, enemy가 구독해서 스탯 재계산하는 이벤트
    public event System.Action OnModifiersChanged;

    void OnEnable() => Managers.UpdateM.Register(this);
    void OnDisable() => Managers.UpdateM.UnRegister(this);

    /// <summary>
    /// 만료된 효과 제거
    /// </summary>
    /// <param name="_deltaTime"></param>
    public void Tick(float _deltaTime)
    {
        foreach (var effect in _effects)
        {
            effect.Tick(_deltaTime);
            if (effect.IsExpired) _expiredBuffer.Add(effect);
        }

        foreach (var expired in _expiredBuffer)
        {
            expired.OnRemove(this);
            _effects.Remove(expired);
        }
        _expiredBuffer.Clear();

    }
    /// <summary>
    /// 버프 /디버프 효과 추가, 중복확인 후 Apply호출
    /// </summary>
    /// <param name="effect"></param>
    public void AddEffect(BuffEffect effect)
    {
        if (!effect.AllowStack)
        {
            foreach (var e in _effects)
            {
                if (e is BuffEffect existing && existing.EffectType == effect.EffectType)
                {
                    existing.Refresh();
                    return;
                }
            }
        }

        _effects.Add(effect);
        effect.OnApply(this);
    }


    /// <summary>
    /// _modifiers 순회해서 최종스탯을 반환해줌
    /// </summary>
    /// <param name="statType"></param>
    /// <param name="baseValue"></param>
    /// <returns></returns>
    public float GetStat(Define.StatType statType, float baseValue)
    {
        foreach (var m in _modifiers)
        {
            if (m.StatType == statType)
            {
                baseValue = m.Apply(baseValue);
            }
        }

        return baseValue;
    }

    /// <summary>
    /// 리스트 추가, 제거
    /// </summary>
    /// <param name="modifier"></param>
    public void AddModifier(StatModifier modifier)
    {
        _modifiers.Add(modifier);
        OnModifiersChanged?.Invoke();
    }

    public void RemoveModifier(StatModifier modifier)
    {
        _modifiers.Remove(modifier);
        OnModifiersChanged?.Invoke();
    }

    public bool HasEffect<T>() where T : BuffEffect
    {
        foreach (var e in _effects)
            if (e is T) return true;
        return false;
    }
}

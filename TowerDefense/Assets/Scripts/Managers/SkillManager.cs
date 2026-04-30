using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SkillManager
{
    private const int MAX_SLOTS = 3;

    private SkillData[] _slots = new SkillData[MAX_SLOTS];
    private float[] _cooldowns = new float[MAX_SLOTS];
    private CancellationTokenSource[] _cooldownCts = new CancellationTokenSource[MAX_SLOTS];
    private Dictionary<Define.SkillType, int> _skillLevels = new();

    public SkillData PendingReplaceSkill { get; set; }
    public int SkillPoints { get; private set; }

    private int _pendingTargetSlot = -1;
    public bool IsTargeting => _pendingTargetSlot >= 0;
    public float TargetingRange { get; private set; }
    public SkillData PendingSkill => IsTargeting ? _slots[_pendingTargetSlot] : null;

    public event Action<int, SkillData> OnSlotChanged;
    public event Action<int, float> OnCooldownChanged;  // (slotIndex, ratio 0~1)
    public event Action<int> OnSkillPointsChanged;      // (skillPoints)
    public event Action<float> OnTargetingStarted;      // (range)
    public event Action OnTargetingCancelled;

    // ─── 조회 ─────────────────────────────────────────────────────────────────

    public SkillData GetSlot(int index) => _slots[index];

    public float GetCooldownRemaining(int index) => _cooldowns[index];

    public float GetCooldownRatio(int index)
    {
        float effective = GetEffectiveCooldown(index);
        if (effective <= 0f) return 0f;
        return _cooldowns[index] / effective;
    }

    private float GetEffectiveCooldown(int slotIndex)
    {
        var skill = _slots[slotIndex];
        if (skill == null) return 0f;
        if (skill.upgradeSteps == null || skill.upgradeSteps.Length == 0) return skill.cooldown;
        int level = Mathf.Clamp(GetSkillLevel(skill) - 1, 0, skill.upgradeSteps.Length - 1);
        return Mathf.Max(0f, skill.cooldown - skill.upgradeSteps[level].cooldownReduction);
    }

    public bool CanActivate(int index) => _slots[index] != null && _cooldowns[index] <= 0f;

    public bool HasSkill(Define.SkillType skillType)
    {
        foreach (var slot in _slots)
            if (slot != null && slot.skillType == skillType) return true;
        return false;
    }

    public int GetSkillLevel(SkillData skill)
    {
        if (skill == null) return 0;
        _skillLevels.TryGetValue(skill.skillType, out int level);
        return level + 1; // 1-based
    }

    public bool CanUpgrade(int slotIndex)
    {
        var skill = _slots[slotIndex];
        if (skill == null || skill.upgradeSteps == null || skill.upgradeSteps.Length == 0) return false;
        _skillLevels.TryGetValue(skill.skillType, out int level);
        return level < skill.upgradeSteps.Length - 1;
    }

    public void UpgradeSkill(int slotIndex)
    {
        var skill = _slots[slotIndex];
        if (!CanUpgrade(slotIndex)) return;
        var type = skill.skillType;
        _skillLevels.TryGetValue(type, out int level);
        _skillLevels[type] = level + 1;
        OnSlotChanged?.Invoke(slotIndex, skill);
    }

    public void AddSkillPoint()
    {
        SkillPoints++;
        OnSkillPointsChanged?.Invoke(SkillPoints);
    }

    public void ConsumeSkillPoint()
    {
        if (SkillPoints <= 0) return;
        SkillPoints--;
        OnSkillPointsChanged?.Invoke(SkillPoints);
    }

    // ─── 슬롯 관리 ────────────────────────────────────────────────────────────

    public bool TryAddSkill(SkillData skillData)
    {
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            if (_slots[i] == null)
            {
                SetSlot(i, skillData);
                Activate(i);
                return true;
            }
        }
        return false;
    }

    public void SetSlot(int slotIndex, SkillData skillData)
    {
        CancelCooldown(slotIndex);
        _slots[slotIndex] = skillData;
        OnSlotChanged?.Invoke(slotIndex, skillData);
    }

    public void RemoveSlot(int slotIndex)
    {
        CancelCooldown(slotIndex);
        _slots[slotIndex] = null;
        OnSlotChanged?.Invoke(slotIndex, null);
    }

    private void CancelCooldown(int slotIndex)
    {
        _cooldownCts[slotIndex]?.Cancel();
        _cooldownCts[slotIndex]?.Dispose();
        _cooldownCts[slotIndex] = null;
        _cooldowns[slotIndex] = 0f;
        OnCooldownChanged?.Invoke(slotIndex, 0f);
    }

    // ─── 스킬 발동 ────────────────────────────────────────────────────────────

    public void Activate(int slotIndex)
    {
        if (!CanActivate(slotIndex) || IsTargeting) return;

        var skill = _slots[slotIndex];

        if (skill.isTargeted)
        {
            _pendingTargetSlot = slotIndex;
            int level = Mathf.Clamp(GetSkillLevel(skill) - 1, 0, skill.upgradeSteps.Length - 1);
            TargetingRange = skill.baseRange + skill.upgradeSteps[level].rangeBonus;
            OnTargetingStarted?.Invoke(TargetingRange);
        }
        else
        {
            StartCooldownAndExecute(slotIndex, Vector3.zero);
        }
    }

    public void ExecuteAt(Vector3 worldPos)
    {
        if (!IsTargeting) return;
        int slot = _pendingTargetSlot;
        _pendingTargetSlot = -1;
        StartCooldownAndExecute(slot, worldPos);
    }

    public void CancelTargeting()
    {
        if (!IsTargeting) return;
        _pendingTargetSlot = -1;
        OnTargetingCancelled?.Invoke();
    }

    private void StartCooldownAndExecute(int slotIndex, Vector3 targetPos)
    {
        var skill = _slots[slotIndex];
        float effectiveCooldown = GetEffectiveCooldown(slotIndex);

        _cooldownCts[slotIndex]?.Cancel();
        _cooldownCts[slotIndex]?.Dispose();
        _cooldownCts[slotIndex] = new CancellationTokenSource();

        _cooldowns[slotIndex] = effectiveCooldown;
        ExecuteSkill(skill, targetPos);
        CooldownAsync(slotIndex, effectiveCooldown, _cooldownCts[slotIndex].Token).Forget();
    }

    private async UniTaskVoid CooldownAsync(int slotIndex, float duration, CancellationToken token)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _cooldowns[slotIndex] = Mathf.Max(0f, duration - elapsed);
            OnCooldownChanged?.Invoke(slotIndex, _cooldowns[slotIndex] / duration);
            bool cancelled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();
            if (cancelled) return;
        }

        _cooldowns[slotIndex] = 0f;
        OnCooldownChanged?.Invoke(slotIndex, 0f);
    }

    private void ExecuteSkill(SkillData skill, Vector3 targetPos)
    {
        int level = Mathf.Clamp(GetSkillLevel(skill) - 1, 0, skill.upgradeSteps.Length - 1);
        SkillUpgradeStep step = skill.upgradeSteps[level];

        float damage = skill.baseDamage * step.damageMultiplier;
        float range = skill.baseRange + step.rangeBonus;
        float duration = skill.baseDuration > 0f ? step.skillDuration : 0f;

        switch (skill.skillType)
        {
            case Define.SkillType.ArrowRain:
                var hits = Physics.OverlapSphere(targetPos, range, LayerMask.GetMask("Enemy"));
                Debug.Log($"[ArrowRain] targetPos={targetPos}, range={range}, hits={hits.Length}");
                foreach (var hit in hits)
                    hit.GetComponent<IDamageable>()?.TakeDamage(damage, false);
                break;

            case Define.SkillType.Block:
                GridNode blockNode = Managers.Grid?.GetNode(targetPos);
                if (blockNode == null || blockNode.NodeType != NodeType.Road || !blockNode.CanWalk) break;

                GameObject blockGo = Managers.PoolM.Pop(skill.skillPrefabkey);
                if (blockGo == null) break;

                blockGo.transform.position = blockNode.WorldPosition;
                if (blockGo.TryGetComponent(out BlockObstacle obstacle))
                    obstacle.Init(duration > 0f ? duration : 10f);

                Managers.Grid.SetOccupied(blockNode.WorldPosition, true);
                break;

            case Define.SkillType.Freeze:
                var freezeHits = Physics.OverlapSphere(targetPos, range, LayerMask.GetMask("Enemy"));
                Debug.Log($"[Freeze] targetPos={targetPos}, range={range}, hits={freezeHits.Length}");
                foreach (var hit in freezeHits)
                    hit.GetComponent<BuffHandler>()?.AddEffect(new FreezeEffect(duration));
                break;

            case Define.SkillType.LightningStorm:
                var stormHits = Physics.OverlapSphere(targetPos, range, LayerMask.GetMask("Enemy"));
                foreach (var hit in stormHits)
                    hit.GetComponent<IDamageable>()?.TakeDamage(damage, false);
                break;

            case Define.SkillType.PoisonMist:
                GameObject mistGo = Managers.PoolM.Pop(skill.skillPrefabkey);
                if (mistGo == null) break;
                mistGo.transform.position = targetPos;
                if (mistGo.TryGetComponent(out PoisonMistZone mistZone))
                    mistZone.Init(range, skill.effectValue, duration > 0f ? duration : 10f);
                break;
        }
    }

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public void Clear()
    {
        if (IsTargeting)
        {
            _pendingTargetSlot = -1;
            OnTargetingCancelled?.Invoke();
        }

        for (int i = 0; i < MAX_SLOTS; i++)
        {
            _cooldownCts[i]?.Cancel();
            _cooldownCts[i]?.Dispose();
            _cooldownCts[i] = null;
            _slots[i] = null;
            _cooldowns[i] = 0f;
        }
        _skillLevels.Clear();
        PendingReplaceSkill = null;
        SkillPoints = 0;
    }
}

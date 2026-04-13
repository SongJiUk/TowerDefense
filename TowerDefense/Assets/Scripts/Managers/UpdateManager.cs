using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ITickable
{
    void Tick(float _deltaTime);
}

public interface IUnScaledTickable
{
    void UnScaledTick(float _unscaledDeltaTime);
}
public class UpdateManager : MonoBehaviour
{
    private readonly HashSet<ITickable> tickables = new();
    private readonly HashSet<ITickable> toAdd = new();
    private readonly HashSet<ITickable> toRemove = new();


    private readonly HashSet<IUnScaledTickable> unScaledTickables = new();
    private readonly HashSet<IUnScaledTickable> unScaledToAdd = new();
    private readonly HashSet<IUnScaledTickable> unScaledToRemove = new();


    public void Register(ITickable _tickable = null, IUnScaledTickable _unScaledTickable = null)
    {
        if (_tickable != null) toAdd.Add(_tickable);
        if (_unScaledTickable != null) unScaledToAdd.Add(_unScaledTickable);
    }

    public void UnRegister(ITickable _tickable = null, IUnScaledTickable _unScaledTickable = null)
    {
        if (_tickable != null) toRemove.Add(_tickable);
        if (_unScaledTickable != null) unScaledToRemove.Add(_unScaledTickable);
    }


    private void Update()
    {
        float deltaTime = Time.deltaTime;
        float unscaledDeltaTime = Time.unscaledDeltaTime;


        foreach (var t in toAdd)
            tickables.Add(t);
        toAdd.Clear();
        foreach (var t in toRemove) tickables.Remove(t);
        toRemove.Clear();

        foreach (var t in unScaledToAdd)
            unScaledTickables.Add(t);
        unScaledToAdd.Clear();
        foreach (var t in unScaledToRemove) unScaledTickables.Remove(t);
        unScaledToRemove.Clear();


        foreach (var tick in tickables)
        {
            if (tick is Component component && component == null)
            {
                toRemove.Add(tick);
                continue;
            }
            tick.Tick(deltaTime);
        }


        foreach (var tick in unScaledTickables)
        {
            if (tick is Component component && component == null)
            {
                unScaledToRemove.Add(tick);
                continue;
            }
            tick.UnScaledTick(unscaledDeltaTime);
        }
    }
}

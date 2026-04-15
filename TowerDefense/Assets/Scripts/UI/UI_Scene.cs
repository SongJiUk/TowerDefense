using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class UI_Scene : UI_Base
{
    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;
        return true;
    }
}

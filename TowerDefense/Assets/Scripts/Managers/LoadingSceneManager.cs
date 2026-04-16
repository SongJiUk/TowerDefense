using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class LoadingSceneManager : MonoBehaviour
{

    [SerializeField] UI_LoadScene _ui;
    async void Start()
    {
        await Managers.ResourceM.LoadGroupAsync<GameObject>("PrevLoad", (key, current, total) =>
        {
            _ui.UpdateProgress((float)current / total, key);
        });

        _ui.SetStartButton(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class UI_LoadScene : UI_Base
{
    enum Buttons
    {
        StartButton
    }
    async void Start()
    {
        await Init();
    }
    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindButton(typeof(Buttons));

        GetButton(typeof(Buttons), (int)Buttons.StartButton).gameObject.BindEvent(OnClickStartButton);
        GetButton(typeof(Buttons), (int)Buttons.StartButton).gameObject.SetActive(false);
        return true;
    }


    void OnClickStartButton()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void UpdateProgress(float ratio, string currentKey)
    {
        //TODO: 로딩바, 텍스트 만들기
        Debug.Log($"ratio : {ratio}");
        Debug.Log($"currentKey : {currentKey}");
    }

    public void SetStartButton(bool active)
    {
        GetButton(typeof(Buttons), (int)Buttons.StartButton).gameObject.SetActive(active);
    }
}

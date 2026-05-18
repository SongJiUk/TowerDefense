using UnityEngine;
using Cysharp.Threading.Tasks;

public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] UI_LoadScene _ui;
    [SerializeField] UI_Login     _loginUI;

    async void Start()
    {
        await Managers.ResourceM.LoadGroupAsync<Object>("PrevLoad", (key, current, total) =>
        {
            _ui.UpdateProgress((float)current / total, key);
        });

        Managers.GameM.LevelData = Managers.ResourceM.Load<LevelData>("LevelData");
        Managers.CardM.Init();

        string stageKey = $"Stage{Managers.SelectedStage}";
        StageData stageData = Managers.ResourceM.Load<StageData>(stageKey);
        Managers.WaveM.Init(stageData);

        await InitFirebase();

        _ui.SetStartButton(true);
    }

    private async UniTask InitFirebase()
    {
        await Managers.FirebaseM.Init();

        if (Managers.FirebaseM.IsLoggedIn())
        {
            await Managers.FirebaseM.ReadData();
            return;
        }

        // 이전에 로그인한 적 없으면 로그인 UI 표시
        if (_loginUI == null) return;

        bool done = false;
        _loginUI.OnLoginSuccess = () => done = true;
        _loginUI.gameObject.SetActive(true);
        await _loginUI.Init();

        await Cysharp.Threading.Tasks.UniTask.WaitUntil(() => done);
    }
}

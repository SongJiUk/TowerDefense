using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 씬 HUD — 골드 · 웨이브 표시.
/// 타워 선택 UI는 UI_TowerSelectPopup이 담당.
/// </summary>
public class UI_GameScene : UI_Scene
{
    enum Texts   { Text_Gold, Text_Wave }
    enum Buttons { Btn_StartWave }

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    async void Start()
    {
        await Init();
    }

    void OnDestroy()
    {
        Managers.OnGoldChanged     -= RefreshGold;
        Managers.WaveM.OnWaveStart -= RefreshWave;
    }

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));

        GetButton(typeof(Buttons), (int)Buttons.Btn_StartWave)
            .onClick.AddListener(OnStartWaveClicked);

        Managers.OnGoldChanged     += RefreshGold;
        Managers.WaveM.OnWaveStart += RefreshWave;
        Managers.WaveM.OnWaveComplete += OnWaveComplete;

        RefreshGold(Managers.Gold);
        RefreshWave(Managers.WaveM.CurrentWave);

        return true;
    }

    // ─── 갱신 ─────────────────────────────────────────────────────────────────

    private void RefreshGold(int gold)
    {
        GetText(typeof(Texts), (int)Texts.Text_Gold).text = $"골드: {gold}G";
    }

    private void RefreshWave(int wave)
    {
        GetText(typeof(Texts), (int)Texts.Text_Wave).text =
            $"웨이브: {wave} / {Managers.WaveM.TotalWaves}";
    }

    // ─── 버튼 핸들러 ─────────────────────────────────────────────────────────

    private void OnStartWaveClicked()
    {
        Managers.WaveM.StartNextWave();
    }

    private void OnWaveComplete(int wave)
    {
        // autoStart=false일 때 다음 웨이브 버튼 활성화 등 추가 가능
    }
}

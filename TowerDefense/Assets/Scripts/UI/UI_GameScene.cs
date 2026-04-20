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
    enum Texts { Text_Gold, Text_Wave, Text_HP, Text_Level }
    enum Buttons { StartWave_Button }
    enum Images { Image_LevelFill }

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    async void Start()
    {
        await Init();
    }

    void OnDestroy()
    {
        Managers.GameM.OnGoldChanged -= RefreshGold;
        Managers.GameM.OnExpChanged -= RefreshExp;
        Managers.GameM.OnLevelUp -= LevelUp;
        Managers.WaveM.OnWaveStart -= RefreshWave;
    }

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));

        GetButton(typeof(Buttons), (int)Buttons.StartWave_Button)
            .onClick.AddListener(OnStartWaveClicked);

        Managers.GameM.OnGoldChanged += RefreshGold;
        Managers.GameM.OnExpChanged += RefreshExp;
        Managers.GameM.OnLevelUp += LevelUp;
        Managers.WaveM.OnWaveStart += RefreshWave;
        Managers.WaveM.OnWaveComplete += OnWaveComplete;

        RefreshGold(Managers.GameM.Gold);
        RefreshWave(Managers.WaveM.CurrentWave);

        return true;
    }

    // ─── 갱신 ─────────────────────────────────────────────────────────────────

    private void RefreshGold(int gold)
    {
        GetText(typeof(Texts), (int)Texts.Text_Gold).text = $"{gold}";
    }

    private void RefreshWave(int wave)
    {
        GetText(typeof(Texts), (int)Texts.Text_Wave).text =
            $"{wave} / {Managers.WaveM.TotalWaves}";
    }

    private void RefreshExp(int exp, int maxExp)
    {
        float amount = (float)exp / maxExp;
        GetImage(typeof(Images), (int)Images.Image_LevelFill).fillAmount = amount;
    }

    private void LevelUp(int level, int currentExp)
    {
        GetText(typeof(Texts), (int)Texts.Text_Level).text = level.ToString();
        int required = Managers.GameM.LevelData.GetRequiredExp(level);
        float amount = required > 0 ? (float)currentExp / required : 0f;
        GetImage(typeof(Images), (int)Images.Image_LevelFill).fillAmount = amount;

        Managers.ObjectM.SpawnUI<UI_LevelUpPopup>("UI_LevelUpPopup", transform);
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

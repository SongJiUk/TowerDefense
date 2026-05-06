using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 웨이브 시작 전 예고 패널.
/// 웨이브 타입(일반/중간보스/최종보스)에 따라 Text_Wave·Text_Type·
/// Image_StartEarly_BG·Image_StartEarly_Border 색상을 변경한다.
/// </summary>
public class UI_NextWavePanel : UI_Base
{
    enum Texts
    {
        Text_WaveNumber, Text_Wave, Text_Type,
        Text_RemainTime, Text_RewardGold, Text_EstimatedTime,
        Text_NormalCount, Text_TankCount, Text_RunnerCount, Text_SplitCount,
        Text_ReviveCount, Text_MiddleBossCount, Text_BossCount
    }
    enum Buttons  { Button_StartEarly, Button_Close }
    enum Images   { Image_StartEarly_BG, Image_StartEarly_Border }
    enum GameObjects
    {
        Content_Normal, Content_Tank, Content_Runner, Content_Split,
        Content_Revive, Content_MiddleBoss, Content_Boss
    }

    // ─── 웨이브 타입별 색상 ────────────────────────────────────────────────────
    private static readonly Color TEXT_NORMAL    = new Color(0.420f, 0.690f, 0.804f); // #6BB0CD
    private static readonly Color BORDER_NORMAL  = new Color(0.529f, 0.820f, 0.894f); // #87D1E4
    private static readonly Color BG_NORMAL      = new Color(0.502f, 0.847f, 1.000f); // #80D8FF

    private static readonly Color TEXT_MIDBOSS   = new Color(1.000f, 0.702f, 0.000f); // #FFB300
    private static readonly Color BORDER_MIDBOSS = new Color(0.427f, 0.275f, 0.000f); // #6D4600
    private static readonly Color BG_MIDBOSS     = new Color(1.000f, 0.702f, 0.000f); // #FFB300

    private static readonly Color TEXT_BOSS      = new Color(0.800f, 0.118f, 0.090f); // #CC1E17
    private static readonly Color BORDER_BOSS    = new Color(0.588f, 0.153f, 0.145f); // #962725
    private static readonly Color BG_BOSS        = new Color(0.800f, 0.133f, 0.200f); // #CC2233

    private bool _bound;
    private CancellationTokenSource _cts;

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public override async UniTask<bool> Init()
    {
        if (_bound) return true;
        if (!await base.Init()) return false;
        _bound = true;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindImage(typeof(Images));
        BindObject(typeof(GameObjects));

        GetButton(typeof(Buttons), (int)Buttons.Button_StartEarly).onClick.AddListener(OnStartEarlyClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Close)     .onClick.AddListener(ForceClose);

        return true;
    }

    // ─── 데이터 적용 ──────────────────────────────────────────────────────────

    public void SetInfo(WavePreview preview)
    {
        bool isBoss      = preview.IsBossWave;
        bool isMidBoss   = !isBoss && HasEnemyType(preview, Define.EnemyType.MiddleBoss);

        Color textColor, borderColor, bgColor;
        string typeLabel;

        if (isBoss)
        {
            textColor = TEXT_BOSS;   borderColor = BORDER_BOSS;   bgColor = BG_BOSS;
            typeLabel = "최종 보스";
        }
        else if (isMidBoss)
        {
            textColor = TEXT_MIDBOSS; borderColor = BORDER_MIDBOSS; bgColor = BG_MIDBOSS;
            typeLabel = "중간 보스";
        }
        else
        {
            textColor = TEXT_NORMAL;  borderColor = BORDER_NORMAL;  bgColor = BG_NORMAL;
            typeLabel = "일반 웨이브";
        }

        ApplyTheme(textColor, borderColor, bgColor);

        GetText(typeof(Texts), (int)Texts.Text_WaveNumber) .text = preview.WaveNumber.ToString();
        GetText(typeof(Texts), (int)Texts.Text_Type)       .text = typeLabel;
        GetText(typeof(Texts), (int)Texts.Text_RewardGold) .text = $"+{preview.BaseRewardGold}";
        GetText(typeof(Texts), (int)Texts.Text_EstimatedTime).text = $"{preview.EstimatedSeconds}초";

        RefreshContents(preview);
        StartCountdown(preview.WaveDelay);
    }

    // ─── 색상 ─────────────────────────────────────────────────────────────────

    private void ApplyTheme(Color text, Color border, Color bg)
    {
        GetText (typeof(Texts),  (int)Texts.Text_Wave)           .color = text;
        GetText (typeof(Texts),  (int)Texts.Text_Type)           .color = text;
        GetImage(typeof(Images), (int)Images.Image_StartEarly_BG)    .color = bg;
        GetImage(typeof(Images), (int)Images.Image_StartEarly_Border).color = border;
    }

    // ─── 적 목록 ──────────────────────────────────────────────────────────────

    private void RefreshContents(WavePreview preview)
    {
        // 전체 숨기기
        for (int i = 0; i < System.Enum.GetValues(typeof(GameObjects)).Length; i++)
            GetObject(typeof(GameObjects), i).SetActive(false);

        if (preview.EnemyGroups == null) return;

        foreach (var (data, count) in preview.EnemyGroups)
        {
            int goIdx   = EnemyTypeToGoIndex(data.enemyType);
            int textIdx = EnemyTypeToCountIndex(data.enemyType);
            if (goIdx < 0) continue;

            GetObject(typeof(GameObjects), goIdx).SetActive(true);
            if (textIdx >= 0)
                GetText(typeof(Texts), textIdx).text = $"x{count}";
        }
    }

    private static int EnemyTypeToGoIndex(Define.EnemyType type) => type switch
    {
        Define.EnemyType.Basic      => (int)GameObjects.Content_Normal,
        Define.EnemyType.Tank       => (int)GameObjects.Content_Tank,
        Define.EnemyType.Runner     => (int)GameObjects.Content_Runner,
        Define.EnemyType.Split      => (int)GameObjects.Content_Split,
        Define.EnemyType.Revive     => (int)GameObjects.Content_Revive,
        Define.EnemyType.MiddleBoss => (int)GameObjects.Content_MiddleBoss,
        Define.EnemyType.Boss       => (int)GameObjects.Content_Boss,
        _                           => -1
    };

    private static int EnemyTypeToCountIndex(Define.EnemyType type) => type switch
    {
        Define.EnemyType.Basic      => (int)Texts.Text_NormalCount,
        Define.EnemyType.Tank       => (int)Texts.Text_TankCount,
        Define.EnemyType.Runner     => (int)Texts.Text_RunnerCount,
        Define.EnemyType.Split      => (int)Texts.Text_SplitCount,
        Define.EnemyType.Revive     => (int)Texts.Text_ReviveCount,
        Define.EnemyType.MiddleBoss => (int)Texts.Text_MiddleBossCount,
        Define.EnemyType.Boss       => (int)Texts.Text_BossCount,
        _                           => -1
    };

    // ─── 카운트다운 ───────────────────────────────────────────────────────────

    private void StartCountdown(float seconds)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        CountdownAsync(seconds, _cts.Token).Forget();
    }

    private async UniTaskVoid CountdownAsync(float seconds, CancellationToken token)
    {
        var txt = GetText(typeof(Texts), (int)Texts.Text_RemainTime);
        float remaining = seconds;
        try
        {
            while (remaining > 0f)
            {
                txt.text = Mathf.CeilToInt(remaining).ToString();
                await UniTask.Delay(100, cancellationToken: token);
                remaining -= 0.1f;
            }
            txt.text = "0";
        }
        catch (System.OperationCanceledException) { }
    }

    // ─── 버튼 ─────────────────────────────────────────────────────────────────

    public void ForceClose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        Managers.PoolM.Push(gameObject);
    }

    private void OnStartEarlyClicked()
    {
        Managers.WaveM.RequestEarlyStart();
        ForceClose();
    }

    // ─── 유틸 ─────────────────────────────────────────────────────────────────

    private static bool HasEnemyType(WavePreview preview, Define.EnemyType type)
    {
        if (preview.EnemyGroups == null) return false;
        foreach (var (data, _) in preview.EnemyGroups)
            if (data.enemyType == type) return true;
        return false;
    }
}

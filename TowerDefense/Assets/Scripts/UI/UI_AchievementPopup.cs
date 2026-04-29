using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 업적 달성 알림 팝업. 화면 하단에서 올라왔다가 자동으로 사라짐.
/// 오브젝트 이름: Text_Title, Text_Desc
/// </summary>
public class UI_AchievementPopup : UI_Base
{
    enum Texts { Text_Title, Text_Desc }

    private bool _initialized;
    private RectTransform _rect;

    public override async UniTask<bool> Init()
    {
        if (_initialized) return true;
        if (!await base.Init()) return false;
        _initialized = true;

        BindText(typeof(Texts));
        _rect = GetComponent<RectTransform>();
        return true;
    }

    public void Show(AchievementData data)
    {
        if (data == null) return;
        GetText(typeof(Texts), (int)Texts.Text_Title).text = data.title;
        GetText(typeof(Texts), (int)Texts.Text_Desc).text = data.description;
        AnimateAsync().Forget();
    }

    private async UniTaskVoid AnimateAsync()
    {
        if (_rect == null) _rect = GetComponent<RectTransform>();

        Vector2 hiddenPos = new Vector2(0f, -200f);
        Vector2 shownPos  = new Vector2(0f, 60f);

        _rect.anchoredPosition = hiddenPos;
        gameObject.SetActive(true);

        _rect.DOAnchorPos(shownPos, 0.4f).SetEase(Ease.OutBack).SetUpdate(true);
        await UniTask.Delay(2800, cancellationToken: destroyCancellationToken);
        _rect.DOAnchorPos(hiddenPos, 0.3f).SetEase(Ease.InBack).SetUpdate(true);
        await UniTask.Delay(300, cancellationToken: destroyCancellationToken);

        Managers.PoolM.Push(gameObject);
    }
}

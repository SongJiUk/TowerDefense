using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 난이도 선택 팝업의 카드 1개.
/// Unity 오브젝트 이름 규칙:
///   Text_BestRecord, Button_Card, Button_Start
///   Object_Bottom, Object_Lock, Image_Border
/// </summary>
public class UI_DifficultyCard : UI_Base
{
    enum Texts       { Text_BestRecord }
    enum Buttons     { Button_Card, Button_Start }
    enum GameObjects { Object_Bottom, Object_Lock }
    enum Images      { Image_Border }

    private bool  _initialized;
    private Color _borderDefaultColor;

    public override async UniTask<bool> Init()
    {
        if (_initialized) return true;
        if (!await base.Init()) return false;
        _initialized = true;

        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));

        _borderDefaultColor = GetImage(typeof(Images), (int)Images.Image_Border).color;
        GetObject(typeof(GameObjects), (int)GameObjects.Object_Bottom).SetActive(false);

        return true;
    }

    public void SetData(bool unlocked, string bestRecord)
    {
        GetText(typeof(Texts), (int)Texts.Text_BestRecord).text = bestRecord;
        GetObject(typeof(GameObjects), (int)GameObjects.Object_Lock).SetActive(!unlocked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Card).interactable = unlocked;
    }

    public void SetOnClick(System.Action onClick)
    {
        var btn = GetButton(typeof(Buttons), (int)Buttons.Button_Card);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClick?.Invoke());
    }

    public void SetOnStartClick(System.Action onStart)
    {
        var btn = GetButton(typeof(Buttons), (int)Buttons.Button_Start);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onStart?.Invoke());
    }

    public void SetSelected(bool selected)
    {
        GetObject(typeof(GameObjects), (int)GameObjects.Object_Bottom).SetActive(selected);

        var border = GetImage(typeof(Images), (int)Images.Image_Border);
        border.color = selected
            ? new Color(_borderDefaultColor.r + 0.4f, _borderDefaultColor.g + 0.4f, _borderDefaultColor.b + 0.4f, 1f)
            : _borderDefaultColor;
    }
}

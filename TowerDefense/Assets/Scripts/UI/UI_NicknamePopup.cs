using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UI_NicknamePopup : UI_Base
{
    enum Buttons     { Button_Confirm, Button_Cancel, Button_Random }
    enum Texts       { Text_Count, Text_Validation }
    enum InputFields { InputField_Nickname }
    enum Images      { Image_InputBorder }

    private static readonly Color _colorValid   = new Color(0.4f, 0.8f, 0.4f); // 초록
    private static readonly Color _colorInvalid = new Color(0.9f, 0.2f, 0.2f); // 빨강
    private static readonly Color _colorDefault = new Color(1f,   0.7f, 0.2f); // 기본(주황)

    private const int MAX_LENGTH = 12;
    private const int MIN_LENGTH = 2;

    public System.Action OnComplete;

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindInputField(typeof(InputFields));
        BindImage(typeof(Images));

        var input = GetInputField(typeof(InputFields), (int)InputFields.InputField_Nickname);
        input.characterLimit = MAX_LENGTH;
        input.text = "";
        input.onValueChanged.AddListener(OnInputChanged);

        OnInputChanged("");

        GetButton(typeof(Buttons), (int)Buttons.Button_Confirm).onClick.AddListener(OnConfirm);
        GetButton(typeof(Buttons), (int)Buttons.Button_Cancel).onClick.AddListener(OnCancel);
        GetButton(typeof(Buttons), (int)Buttons.Button_Random).onClick.AddListener(OnRandom);

        return true;
    }

    private void OnInputChanged(string value)
    {
        int len = value.Length;
        bool valid = len >= MIN_LENGTH;

        GetText(typeof(Texts), (int)Texts.Text_Count).text = $"{len} / {MAX_LENGTH}";

        var validationText = GetText(typeof(Texts), (int)Texts.Text_Validation);
        var border         = GetImage(typeof(Images), (int)Images.Image_InputBorder);
        var confirmBtn     = GetButton(typeof(Buttons), (int)Buttons.Button_Confirm);

        if (len == 0)
        {
            validationText.text  = "";
            border.color         = _colorDefault;
            confirmBtn.interactable = false;
        }
        else if (valid)
        {
            validationText.text  = "✓ 사용 가능";
            validationText.color = _colorValid;
            border.color         = _colorValid;
            confirmBtn.interactable = true;
        }
        else
        {
            validationText.text  = $"2~12자 사이로 입력하세요";
            validationText.color = _colorInvalid;
            border.color         = _colorInvalid;
            confirmBtn.interactable = false;
        }
    }

    private void OnRandom()
    {
        GetInputField(typeof(InputFields), (int)InputFields.InputField_Nickname).text
            = NicknameGenerator.Generate();
    }

    private void OnConfirm()
    {
        string nickname = GetInputField(typeof(InputFields), (int)InputFields.InputField_Nickname)
            .text.Trim();

        if (nickname.Length < MIN_LENGTH) return;

        var data = Managers.SaveM?.Data;
        if (data != null)
        {
            data.PlayerName = nickname;
            Managers.SaveM.SaveCurrent();
        }

        Managers.AchievementM?.AddProgress("set_nickname");
        OnComplete?.Invoke();
        Managers.ResourceM.Destroy(gameObject);
    }

    private void OnCancel()
    {
        Managers.ResourceM.Destroy(gameObject);
    }
}

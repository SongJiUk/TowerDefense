using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_NicknamePopup : UI_Base
{
    enum Buttons { Button_Confirm, Button_Cancel }

    [SerializeField] private TMP_InputField _inputField;

    private const int MAX_LENGTH = 12;
    private const int MIN_LENGTH = 2;

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindButton(typeof(Buttons));
        GetButton(typeof(Buttons), (int)Buttons.Button_Confirm).onClick.AddListener(OnConfirm);
        GetButton(typeof(Buttons), (int)Buttons.Button_Cancel).onClick.AddListener(OnCancel);

        if (_inputField != null)
        {
            _inputField.characterLimit = MAX_LENGTH;
            _inputField.text = Managers.SaveM?.Data?.PlayerName ?? "";
        }

        return true;
    }

    private void OnConfirm()
    {
        string nickname = _inputField != null ? _inputField.text.Trim() : "";

        if (nickname.Length < MIN_LENGTH)
        {
            Debug.LogWarning($"[NicknamePopup] 닉네임은 {MIN_LENGTH}자 이상이어야 합니다.");
            return;
        }

        var data = Managers.SaveM?.Data;
        if (data != null)
        {
            data.PlayerName = nickname;
            Managers.SaveM.SaveCurrent();
        }

        gameObject.SetActive(false);
    }

    private void OnCancel()
    {
        gameObject.SetActive(false);
    }
}

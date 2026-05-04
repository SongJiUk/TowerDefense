using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 난이도 선택 팝업.
/// Unity 오브젝트 이름 규칙:
///   버튼 : Button_Back
///   카드  : Content_Cards 하위에 Easy→Normal→Hard→Hell 순서대로 배치
///           (각 카드에 UI_DifficultyCard 컴포넌트 + Button 컴포넌트 필요)
/// </summary>
public class UI_DifficultySelectPopup : UI_Base
{
    enum Buttons { Button_Back }

    private bool _initialized;
    private UI_DifficultyCard[] _cards;
    private Define.Difficulty? _selected = null;

    public override async UniTask<bool> Init()
    {
        if (_initialized) return true;
        if (!await base.Init()) return false;
        _initialized = true;

        _cards = GetComponentsInChildren<UI_DifficultyCard>(true);
        foreach (var card in _cards)
            if (!await card.Init()) return false;

        BindButton(typeof(Buttons));
        GetButton(typeof(Buttons), (int)Buttons.Button_Back).onClick.AddListener(OnBackClicked);

        Refresh();
        return true;
    }

    private void Refresh()
    {
        var saveData = Managers.SaveM.Data;

        for (int i = 0; i < _cards.Length; i++)
        {
            var d = (Define.Difficulty)i;
            bool unlocked = Managers.DifficultyM.IsUnlocked(d);
            string best = saveData.BestWave > 0 && (int)saveData.BestDifficulty == i
                ? $"최고기록: 웨이브 {saveData.BestWave}"
                : "최고기록: -";

            _cards[i].SetData(unlocked, best);
            _cards[i].SetSelected(_selected.HasValue && d == _selected.Value);

            int captured = i;
            _cards[i].SetOnClick(() => OnCardSelected((Define.Difficulty)captured));
            _cards[i].SetOnStartClick(OnStartClicked);
        }
    }

    private void OnCardSelected(Define.Difficulty d)
    {
        if (!Managers.DifficultyM.IsUnlocked(d)) return;

        if (_selected == d)
            _selected = null;   // 같은 카드 다시 클릭 → 비활성화
        else
        {
            _selected = d;
            Managers.DifficultyM.Select(d);
        }
        Refresh();
    }

    private void OnBackClicked()
    {
        Managers.UIM.ClosePopup();
    }

    private void OnStartClicked()
    {
        Managers.SelectedStage = 1;
        Managers.GameM.Reset();
        Managers.CardM.Clear();
        Managers.Clear();
        SceneManager.LoadScene("GameScene");
    }
}

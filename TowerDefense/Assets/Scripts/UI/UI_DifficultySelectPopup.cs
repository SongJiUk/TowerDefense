using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 난이도 선택 팝업. 타이틀/로딩씬에 배치.
/// 버튼 4개(Easy/Normal/Hard/Hell) 각각에 DifficultyButton 컴포넌트를 연결하거나
/// 이 스크립트에 직접 참조로 연결해 사용.
/// </summary>
public class UI_DifficultySelectPopup : MonoBehaviour
{
    [System.Serializable]
    public struct DifficultySlot
    {
        public Button button;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descText;
        public GameObject lockIcon;
        public GameObject selectedMark;
    }

    [SerializeField] private DifficultySlot[] _slots; // 길이 4, 순서: Easy Normal Hard Hell

    private static readonly string[] NAMES = { "Easy", "Normal", "Hard", "Hell" };
    private static readonly string[] DESCS =
    {
        "적 HP -20% / 골드 +20%\n코어 HP x1.5",
        "기본 난이도",
        "적 HP +30% / 골드 -10%\n코어 HP x0.8",
        "적 HP +70% / 골드 -25%\n코어 HP x0.5",
    };

    void OnEnable() => Refresh();

    public void Refresh()
    {
        if (Managers.DifficultyM == null) return;

        for (int i = 0; i < _slots.Length; i++)
        {
            var d = (Define.Difficulty)i;
            bool unlocked = Managers.DifficultyM.IsUnlocked(d);
            bool selected = Managers.DifficultyM.Selected == d;

            var slot = _slots[i];
            if (slot.nameText != null) slot.nameText.text = NAMES[i];
            if (slot.descText != null) slot.descText.text = DESCS[i];
            if (slot.lockIcon != null) slot.lockIcon.SetActive(!unlocked);
            if (slot.selectedMark != null) slot.selectedMark.SetActive(selected);
            if (slot.button != null) slot.button.interactable = unlocked;

            int captured = i;
            slot.button?.onClick.RemoveAllListeners();
            slot.button?.onClick.AddListener(() => OnSelect((Define.Difficulty)captured));
        }
    }

    private void OnSelect(Define.Difficulty d)
    {
        Managers.DifficultyM.Select(d);
        Refresh();
    }
}

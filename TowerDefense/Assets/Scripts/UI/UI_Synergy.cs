using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 시너지 목록 패널. Canvas에 항상 배치되는 HUD.
/// SynergyData 에셋은 PrevLoad 레이블로 미리 로드되어 있어야 함.
/// Button_Open → Object_Bottom(시너지 목록) 표시 / Button_Close → 숨김.
/// </summary>
public class UI_Synergy : UI_Base
{
    enum Images      { Image_Synergy_BG, Image_Synergy_Border, Image_SynergyCountBG }
    enum Texts       { Text_Synergy , Text_SynergyCount }
    enum Buttons     { Button_Open, Button_Close }
    enum GameObjects { Object_Bottom, Object_Parent }

    private UI_SynergyItem[] _items;

    // ─── Unity 생명주기 ───────────────────────────────────────────────────────

    async void Start()
    {
        await Init();
    }

    void OnDestroy()
    {
        if (Managers.SynergyM != null)
            Managers.SynergyM.OnSynergyChanged -= RefreshAll;
    }

    // ─── 초기화 ───────────────────────────────────────────────────────────────

    public override async UniTask<bool> Init()
    {
        if (!await base.Init()) return false;

        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        BindObject(typeof(GameObjects));

        GetButton(typeof(Buttons), (int)Buttons.Button_Open).onClick.AddListener(OnOpenClicked);
        GetButton(typeof(Buttons), (int)Buttons.Button_Close).onClick.AddListener(OnCloseClicked);

        var synergies = Managers.ResourceM.GetAllLoaded<SynergyData>();
        if (synergies.Count == 0)
            Debug.LogWarning("[UI_Synergy] SynergyData가 없습니다. PrevLoad 레이블 확인 필요.");

        var container = GetObject(typeof(GameObjects), (int)GameObjects.Object_Parent).transform;

        _items = new UI_SynergyItem[synergies.Count];
        for (int i = 0; i < synergies.Count; i++)
        {
            var item = Managers.ObjectM.SpawnUI<UI_SynergyItem>("UI_SynergyItem", container);
            await item.Init();
            item.SetDef(synergies[i]);
            _items[i] = item;
        }

        GetImage(typeof(Images), (int)Images.Image_SynergyCountBG).gameObject.SetActive(false);

        Managers.SynergyM.OnSynergyChanged += RefreshAll;
        RefreshAll();

        SetPanelOpen(false);

        ApplyTheme(Managers.WaveM?.CurrentStage);
        return true;
    }

    public override void ApplyTheme(StageData stage)
    {
        if (stage == null || !isInit) return;
        GetImage(typeof(Images), (int)Images.Image_Synergy_BG).color    = stage.uiBGColor;
        GetImage(typeof(Images), (int)Images.Image_Synergy_Border).color = stage.uiBorderColor;
        GetText(typeof(Texts),   (int)Texts.Text_Synergy).color          = stage.uiBorderColor;
    }

    // ─── 갱신 ─────────────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        if (_items == null) return;

        int activeCount = 0;
        foreach (var item in _items)
        {
            item?.Refresh();
            if (item != null && item.IsActive) activeCount++;
        }

        bool hasAny = activeCount > 0;
        GetImage(typeof(Images), (int)Images.Image_SynergyCountBG).gameObject.SetActive(hasAny);
        GetText(typeof(Texts), (int)Texts.Text_SynergyCount).text = activeCount.ToString();
    }

    // ─── 버튼 ─────────────────────────────────────────────────────────────────

    private void OnOpenClicked()  => SetPanelOpen(true);
    private void OnCloseClicked() => SetPanelOpen(false);

    private void SetPanelOpen(bool open)
    {
        GetObject(typeof(GameObjects), (int)GameObjects.Object_Bottom).SetActive(open);
        GetButton(typeof(Buttons), (int)Buttons.Button_Open).gameObject.SetActive(!open);
        GetButton(typeof(Buttons), (int)Buttons.Button_Close).gameObject.SetActive(open);
    }
}

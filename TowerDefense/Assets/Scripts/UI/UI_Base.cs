using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 모든 UI 클래스의 베이스. Enum 기반 Bind 패턴으로 자식 오브젝트를 자동 연결한다.
///
/// Bind 패턴 사용법:
///   1. Enum 이름을 자식 오브젝트 이름과 동일하게 작성
///   2. Init()에서 BindButton(typeof(Buttons)) 등 호출
///   3. GetButton(typeof(Buttons), (int)Buttons.Btn_Start)로 접근
///
/// 이벤트 바인딩:
///   BindEvent(go, () => OnClick(), Define.UIEvent.Click)
///   BindEvent(go, () => OnHover(), Define.UIEvent.PointerEnter)
///   BindEvent(go, null, (_) => OnExit(), Define.UIEvent.OnPointerExit)
/// </summary>
public class UI_Base : MonoBehaviour
{
    /// <summary>Init()이 완료됐는지 여부. 중복 초기화 방지용.</summary>
    protected bool isInit = false;

    /// <summary>
    /// 초기화. 서브클래스에서 override해서 Bind 호출.
    /// isInit이 이미 true면 false를 반환해 중복 실행을 막는다.
    /// </summary>
    public virtual async UniTask<bool> Init()
    {
        if (isInit) return false;
        isInit = true;
        return true;
    }

    public virtual void SetInfo() { }

    /// <summary>
    /// 스테이지 테마 색상 적용. Init() 완료 후 호출.
    /// 서브클래스에서 override해 바인딩된 Image·Text에 색상 적용.
    /// </summary>
    public virtual void ApplyTheme(StageData stage) { }

    #region 바인딩

    protected Dictionary<Type, UnityEngine.Object[]> objs_Dic = new();

    /// <summary>
    /// Enum의 각 이름과 일치하는 자식 오브젝트를 찾아 objs_Dic에 저장.
    /// T 타입으로 컴포넌트를 찾으며, GameObject 타입이면 오브젝트 자체를 저장.
    /// 이름이 일치하는 오브젝트가 없으면 에러 로그 출력.
    /// </summary>
    protected void Bind<T>(Type _type) where T : UnityEngine.Object
    {
        string[] names = Enum.GetNames(_type);
        UnityEngine.Object[] objs = new UnityEngine.Object[names.Length];
        objs_Dic.Add(_type, objs);

        for (int i = 0; i < names.Length; i++)
        {
            if (typeof(T) == typeof(GameObject))
                objs[i] = Utils.FindChild(gameObject, names[i], true);
            else
                objs[i] = Utils.FindChild<T>(gameObject, names[i], true);

            if (objs[i] == null) Debug.LogError($"[UI_Base] Bind 실패: {names[i]}");
        }
    }

    protected void BindObject(Type _type) => Bind<GameObject>(_type);
    protected void BindImage(Type _type)  => Bind<Image>(_type);
    protected void BindText(Type _type)   => Bind<TextMeshProUGUI>(_type);
    protected void BindButton(Type _type) => Bind<Button>(_type);
    protected void BindToggle(Type _type) => Bind<Toggle>(_type);
    protected void BindSlider(Type _type) => Bind<Slider>(_type);

    #endregion

    #region Get

    protected T Get<T>(Type _type, int _index) where T : UnityEngine.Object
    {
        if (!objs_Dic.TryGetValue(_type, out UnityEngine.Object[] objs)) return null;
        return objs[_index] as T;
    }

    protected GameObject      GetObject(Type _type, int _index) => Get<GameObject>(_type, _index);
    protected Image           GetImage(Type _type, int _index)  => Get<Image>(_type, _index);
    protected TextMeshProUGUI GetText(Type _type, int _index)   => Get<TextMeshProUGUI>(_type, _index);
    protected Button          GetButton(Type _type, int _index) => Get<Button>(_type, _index);
    protected Toggle          GetToggle(Type _type, int _index) => Get<Toggle>(_type, _index);
    protected Slider          GetSlider(Type _type, int _index) => Get<Slider>(_type, _index);

    #endregion

    #region 이벤트 바인딩

    /// <summary>
    /// _go에 UI_EventHandler를 자동 추가하고 이벤트 타입에 맞는 핸들러를 등록.
    /// 중복 등록 방지를 위해 -= 후 += 처리.
    /// </summary>
    /// <param name="_go">이벤트를 등록할 오브젝트</param>
    /// <param name="_action">Click·PointerEnter 등 단순 Action</param>
    /// <param name="_dragAction">Drag·PointerExit 등 BaseEventData가 필요한 Action</param>
    public static void BindEvent(GameObject _go, Action _action = null, Action<BaseEventData> _dragAction = null, Define.UIEvent _type = Define.UIEvent.Click)
    {
        UI_EventHandler eh = _go.GetOrAddComponent<UI_EventHandler>();

        switch (_type)
        {
            case Define.UIEvent.Click:         eh.OnClickHandler -= _action;            eh.OnClickHandler += _action;            break;
            case Define.UIEvent.Pressed:       eh.OnPressHandler -= _action;            eh.OnPressHandler += _action;            break;
            case Define.UIEvent.PointerDown:   eh.OnPointerDownHandler -= _action;      eh.OnPointerDownHandler += _action;      break;
            case Define.UIEvent.PointerUp:     eh.OnPointerUpHandler -= _action;        eh.OnPointerUpHandler += _action;        break;
            case Define.UIEvent.Drag:          eh.OnDragHandler -= _dragAction;         eh.OnDragHandler += _dragAction;         break;
            case Define.UIEvent.BeginDrag:     eh.OnBeginDragHandler -= _dragAction;    eh.OnBeginDragHandler += _dragAction;    break;
            case Define.UIEvent.EndDrag:       eh.OnEndDragHandler -= _dragAction;      eh.OnEndDragHandler += _dragAction;      break;
            case Define.UIEvent.OnPointerExit: eh.OnPointerExitHandler -= _dragAction;  eh.OnPointerExitHandler += _dragAction;  break;
            case Define.UIEvent.PointerEnter:  eh.OnPointerEnterHandler -= _action;     eh.OnPointerEnterHandler += _action;     break;
        }
    }

    public virtual void OnDrag(BaseEventData _eventData)      { }
    public virtual void OnBeginDrag(BaseEventData _eventData) { }
    public virtual void OnEndDrag(BaseEventData _eventData)   { }

    #endregion
}

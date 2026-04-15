using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Unity EventSystem 인터페이스를 Action으로 래핑하는 컴포넌트.
/// UI_Base.BindEvent()에서 GetOrAddComponent로 자동 추가된다.
/// 직접 추가하지 않고 BindEvent를 통해 사용하는 것을 권장.
///
/// 지원 이벤트: Click, PointerDown, PointerUp, Drag, BeginDrag, EndDrag, PointerExit, PointerEnter
/// Pressed: PointerDown 상태에서 매 프레임 OnPressHandler 호출 (버튼 홀드 감지용)
/// </summary>
public class UI_EventHandler : MonoBehaviour,
    IPointerClickHandler, IPointerDownHandler, IPointerUpHandler,
    IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerExitHandler, IPointerEnterHandler
{
    public Action                   OnClickHandler        = null;
    public Action                   OnPressHandler        = null;
    public Action                   OnPointerDownHandler  = null;
    public Action                   OnPointerUpHandler    = null;
    public Action<BaseEventData>    OnDragHandler         = null;
    public Action<BaseEventData>    OnBeginDragHandler    = null;
    public Action<BaseEventData>    OnEndDragHandler      = null;
    public Action<BaseEventData>    OnPointerExitHandler  = null;
    public Action                   OnPointerEnterHandler = null;

    bool isPressed = false;

    void Update()
    {
        if (isPressed) OnPressHandler?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)  => OnClickHandler?.Invoke();
    public void OnPointerDown(PointerEventData eventData)   { isPressed = true;  OnPointerDownHandler?.Invoke(); }
    public void OnPointerUp(PointerEventData eventData)     { isPressed = false; OnPointerUpHandler?.Invoke(); }
    public void OnDrag(PointerEventData eventData)          { isPressed = true;  OnDragHandler?.Invoke(eventData); }
    public void OnBeginDrag(PointerEventData eventData)     => OnBeginDragHandler?.Invoke(eventData);
    public void OnEndDrag(PointerEventData eventData)       { isPressed = false; OnEndDragHandler?.Invoke(eventData); }
    public void OnPointerExit(PointerEventData eventData)   { isPressed = false; OnPointerExitHandler?.Invoke(eventData); }
    public void OnPointerEnter(PointerEventData eventData)  => OnPointerEnterHandler?.Invoke();
}

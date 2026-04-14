using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 누르는 동안 카메라를 조작하는 버튼.
/// Button 컴포넌트 대신 이 스크립트를 UI 오브젝트에 붙여 사용.
/// </summary>
public class CameraHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum Type { Left, Right, ZoomIn, ZoomOut }

    [SerializeField] private Type _type;
    [SerializeField] private CameraController _cameraController;

    public void OnPointerDown(PointerEventData _eventData)
    {
        switch (_type)
        {
            case Type.Left:    _cameraController.SetMoveDir(-1f); break;
            case Type.Right:   _cameraController.SetMoveDir(1f);  break;
            case Type.ZoomIn:  _cameraController.SetZoomDir(1f);  break;
            case Type.ZoomOut: _cameraController.SetZoomDir(-1f); break;
        }
    }

    public void OnPointerUp(PointerEventData _eventData)
    {
        switch (_type)
        {
            case Type.Left:
            case Type.Right:   _cameraController.SetMoveDir(0f); break;
            case Type.ZoomIn:
            case Type.ZoomOut: _cameraController.SetZoomDir(0f); break;
        }
    }
}

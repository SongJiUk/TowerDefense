using System;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Utils
{
    public static T GetOrAddComponent<T>(this GameObject _go) where T : Component
    {
        if (_go == null) return null;
        T component = _go.GetComponent<T>();
        if (component == null)
            component = _go.AddComponent<T>();
        return component;
    }

    public static GameObject FindChild(GameObject _go, string _name = null, bool _recursive = false)
    {
        Transform tr = FindChild<Transform>(_go, _name, _recursive);
        if (tr == null) return null;
        return tr.gameObject;
    }

    public static T FindChild<T>(GameObject _go, string _name = null, bool _recursive = false) where T : UnityEngine.Object
    {
        if (_go == null) return null;

        if (!_recursive)
        {
            for (int i = 0; i < _go.transform.childCount; i++)
            {
                Transform tr = _go.transform.GetChild(i);
                if (string.IsNullOrEmpty(_name) || tr.name == _name)
                {
                    T component = tr.GetComponent<T>();
                    if (component != null) return component;
                }
            }
        }
        else
        {
            foreach (T component in _go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(_name) || component.name == _name)
                    return component;
            }
        }

        return null;
    }

    public static bool IsValid(this GameObject _go)
    {
        return _go != null && _go.activeSelf;
    }

    public static bool IsValid(this UI_Base _ui)
    {
        return _ui != null && _ui.isActiveAndEnabled;
    }

    public static void BindEvent(this GameObject _go, Action _action = null, Action<BaseEventData> _dragAction = null, Define.UIEvent _type = Define.UIEvent.Click)
    {
        UI_Base.BindEvent(_go, _action, _dragAction, _type);
    }
}

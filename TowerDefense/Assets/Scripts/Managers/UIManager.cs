using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIManager
{
    public readonly Stack<UI_Base> popupStack = new();

    UI_Scene sceneUI = null;
    public UI_Scene SceneUI => sceneUI;

    GameObject root = null;
    public GameObject Root
    {
        get
        {
            if (root != null) return root;

            root = GameObject.Find("@UI_Root");
            if (root == null)
            {
                root = new GameObject { name = "@UI_Root" };
                Canvas canvas = root.AddComponent<Canvas>();
                root.AddComponent<CanvasScaler>();
                root.AddComponent<GraphicRaycaster>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            return root;
        }
    }

    public T MakeSubItem<T>(Transform _parent = null, string _name = null, bool _pooling = true) where T : UI_Base
    {
        if (string.IsNullOrEmpty(_name)) _name = typeof(T).Name;
        GameObject go = Managers.ResourceM.Instantiate(_name, _parent, _pooling);
        if (_parent != null)
            go.transform.SetParent(_parent, false);

        return go.GetOrAddComponent<T>();
    }

    public T ShowScene<T>(string _name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(_name)) _name = typeof(T).Name;

        GameObject go = Managers.ResourceM.Instantiate(_name);
        T ui = go.GetOrAddComponent<T>();
        sceneUI = ui;
        go.transform.SetParent(Root.transform);

        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        return ui;
    }

    #region Popup

    public T ShowPopup<T>(string _name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(_name)) _name = typeof(T).Name;

        GameObject go = Managers.ResourceM.Instantiate(_name, _pooling: true);
        if (go == null)
        {
            Debug.LogError($"[UIManager] ShowPopup: '{_name}' 생성 실패");
            return null;
        }

        T popup = go.GetOrAddComponent<T>();
        go.transform.SetParent(Root.transform);

        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localPosition = Vector3.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        popupStack.Push(popup);
        go.transform.SetAsLastSibling();
        popup.Init().Forget();

        return popup;
    }

    public void ClosePopup()
    {
        if (popupStack.Count == 0) return;
        UI_Base popup = popupStack.Pop();
        Managers.ResourceM.Destroy(popup.gameObject);
    }

    public void CloseAllPopup()
    {
        while (popupStack.Count > 0) ClosePopup();
    }

    public int GetPopupCount() => popupStack.Count;

    #endregion

    public void Clear()
    {
        CloseAllPopup();
        if (sceneUI != null)
        {
            Managers.ResourceM.Destroy(sceneUI.gameObject);
            sceneUI = null;
        }
        root = null;
    }
}

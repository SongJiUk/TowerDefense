using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class SceneFader
{
    private static Image _overlay;

    public static async UniTask FadeOut(float duration = 0.35f)
    {
        EnsureOverlay(0f);
        var tcs = new UniTaskCompletionSource();
        _overlay.DOFade(1f, duration).SetUpdate(true).OnComplete(() => tcs.TrySetResult());
        await tcs.Task;
    }

    public static async UniTask FadeIn(float duration = 0.35f)
    {
        if (_overlay == null) return;
        var tcs = new UniTaskCompletionSource();
        _overlay.DOFade(0f, duration).SetUpdate(true).OnComplete(() => tcs.TrySetResult());
        await tcs.Task;
        Object.Destroy(_overlay.transform.root.gameObject);
        _overlay = null;
    }

    private static void EnsureOverlay(float startAlpha)
    {
        if (_overlay != null)
        {
            _overlay.color = new Color(0f, 0f, 0f, startAlpha);
            return;
        }

        var root = new GameObject("SceneFader");
        Object.DontDestroyOnLoad(root);

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();

        var imgGo = new GameObject("Image");
        imgGo.transform.SetParent(root.transform, false);
        _overlay = imgGo.AddComponent<Image>();
        _overlay.color = new Color(0f, 0f, 0f, startAlpha);
        _overlay.raycastTarget = true;

        var rect = imgGo.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}

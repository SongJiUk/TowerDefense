using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 적 피격/사망 시 월드 좌표에 숫자를 띄우고 올라가며 사라짐.
/// ObjectPool로 재사용. Managers.FloatingTextM.Show(pos, text, color)로 호출.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class FloatingText : MonoBehaviour
{
    private TextMeshPro _tmp;
    private Camera _cam;

    void Awake()
    {
        _tmp = GetComponent<TextMeshPro>();
        _cam = Camera.main;
    }

    void LateUpdate()
    {
        if (_cam != null)
            transform.rotation = _cam.transform.rotation;
    }

    public async UniTaskVoid Show(Vector3 worldPos, string text, Color color)
    {
        transform.position = worldPos;
        _tmp.text = text;
        _tmp.color = color;

        var c = color;
        c.a = 1f;
        _tmp.color = c;

        transform.localScale = Vector3.one;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(worldPos + Vector3.up * 1.5f, 0.8f).SetEase(Ease.OutCubic));
        seq.Join(transform.DOScale(1.3f, 0.15f).SetLoops(2, LoopType.Yoyo));
        seq.Append(_tmp.DOFade(0f, 0.3f));

        await seq.AsyncWaitForCompletion();

        Managers.PoolM.Push(gameObject);
    }
}

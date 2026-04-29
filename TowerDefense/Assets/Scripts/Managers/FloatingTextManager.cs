using UnityEngine;

/// <summary>
/// 데미지 숫자 표시 전담. Managers.FloatingTextM으로 접근.
/// </summary>
public class FloatingTextManager
{
    private static readonly Color COLOR_NORMAL   = Color.white;
    private static readonly Color COLOR_CRITICAL = new Color(1f, 0.9f, 0f);
    private static readonly Color COLOR_POISON   = new Color(0.4f, 1f, 0.2f);
    private static readonly Color COLOR_SLOW     = new Color(0.4f, 0.8f, 1f);
    private static readonly Color COLOR_SPLASH   = new Color(1f, 0.5f, 0.1f);

    public void ShowDamage(Vector3 pos, float damage, bool isCritical = false)
    {
        string text = isCritical ? $"!{(int)damage}!" : ((int)damage).ToString();
        Color color = isCritical ? COLOR_CRITICAL : COLOR_NORMAL;
        Spawn(pos, text, color);
    }

    public void ShowPoison(Vector3 pos, float damage)
        => Spawn(pos, ((int)damage).ToString(), COLOR_POISON);

    public void ShowSlow(Vector3 pos)
        => Spawn(pos, "SLOW", COLOR_SLOW);

    public void ShowSplash(Vector3 pos, float damage)
        => Spawn(pos, ((int)damage).ToString(), COLOR_SPLASH);

    private const float HEAD_OFFSET_Y = 2.2f;

    private void Spawn(Vector3 pos, string text, Color color)
    {
        GameObject go = Managers.PoolM.Pop("FloatingText");
        if (go == null) return;

        pos += new Vector3(Random.Range(-0.5f, 0.5f), HEAD_OFFSET_Y, 0f);

        if (go.TryGetComponent(out FloatingText ft))
            ft.Show(pos, text, color).Forget();
    }
}

using System.Collections.Generic;

/// <summary>
/// 다음 웨이브 예고 데이터. WaveManager.PrepareNextWave()에서 생성.
/// </summary>
public class WavePreview
{
    public int  WaveNumber;
    public bool IsBossWave;
    public System.Collections.Generic.List<(EnemyData data, int count)> EnemyGroups;
    public int  BaseRewardGold;
    public int  EstimatedSeconds;
    public float WaveDelay; // WaveStarter._nextWaveDelay — 카운트다운 표시용
}

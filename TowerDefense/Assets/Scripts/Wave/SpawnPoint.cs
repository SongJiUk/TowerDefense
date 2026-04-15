using UnityEngine;

/// <summary>
/// 적 스폰 위치를 Managers에 등록하는 마커 컴포넌트.
/// WaveManager.SpawnEnemy()에서 Managers.SpawnPoint.transform.position으로 스폰 좌표를 읽는다.
/// Road 타일 위에 배치해야 적이 경로를 올바르게 탐색한다.
/// </summary>
public class SpawnPoint : MonoBehaviour
{
    void OnEnable()  => Managers.SpawnPoint = this;
    void OnDisable() => Managers.SpawnPoint = null;
}

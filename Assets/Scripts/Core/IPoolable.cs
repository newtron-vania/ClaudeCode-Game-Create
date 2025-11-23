/// <summary>
/// 풀링 가능한 오브젝트 인터페이스
/// PoolManager에서 관리되는 오브젝트는 이 인터페이스를 구현해야 합니다.
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// 풀에서 스폰될 때 호출됩니다 (OnEnable 대신 사용)
    /// </summary>
    void OnSpawnedFromPool();

    /// <summary>
    /// 풀로 반환될 때 호출됩니다 (OnDisable 대신 사용)
    /// </summary>
    void OnReturnedToPool();
}

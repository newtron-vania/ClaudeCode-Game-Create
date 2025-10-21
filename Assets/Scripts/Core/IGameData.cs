/// <summary>
/// 게임 데이터 인터페이스
/// 각 미니 게임은 이 인터페이스를 상속받아 고유한 데이터 구조 정의
/// </summary>
public interface IGameData
{
    /// <summary>
    /// 게임 데이터 초기화
    /// </summary>
    void Initialize();

    /// <summary>
    /// 게임 데이터 리셋
    /// </summary>
    void Reset();

    /// <summary>
    /// 게임 데이터 검증
    /// </summary>
    /// <returns>데이터가 유효하면 true</returns>
    bool Validate();
}

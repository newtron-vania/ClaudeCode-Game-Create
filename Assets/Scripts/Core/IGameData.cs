/// <summary>
/// 게임 데이터 인터페이스
/// 각 미니 게임은 이 인터페이스를 상속받아 고유한 데이터 구조 정의
/// GameManager가 게임의 구체적인 데이터 타입을 모르고도 저장/로드할 수 있도록 합니다.
/// </summary>
public interface IGameData
{
    /// <summary>
    /// 게임 데이터 초기화
    /// 새 게임 시작 시 호출됩니다.
    /// </summary>
    void Initialize();

    /// <summary>
    /// 게임 데이터 리셋
    /// 게임 재시작 시 호출됩니다.
    /// </summary>
    void Reset();

    /// <summary>
    /// 게임 데이터 검증
    /// 데이터 무결성을 확인합니다.
    /// </summary>
    /// <returns>데이터가 유효하면 true</returns>
    bool Validate();

    /// <summary>
    /// 현재 게임 데이터를 저장
    /// JSON 직렬화 또는 PlayerPrefs 등을 활용하여 구현합니다.
    /// </summary>
    void SaveState();

    /// <summary>
    /// 저장된 게임 데이터를 로드
    /// JSON 역직렬화 또는 PlayerPrefs 등을 활용하여 구현합니다.
    /// </summary>
    void LoadState();
}

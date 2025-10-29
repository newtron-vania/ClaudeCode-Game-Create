/// <summary>
/// 게임별 데이터 제공자 인터페이스
/// 각 미니게임은 이 인터페이스를 구현하여 고유한 데이터를 관리합니다.
/// DataManager가 게임의 구체적인 데이터 구조를 모르고도 데이터를 로드/언로드할 수 있도록 합니다.
/// </summary>
public interface IGameDataProvider
{
    /// <summary>
    /// 게임 ID (예: "UndeadSurvivor", "Tetris")
    /// </summary>
    string GameID { get; }

    /// <summary>
    /// 데이터 로드 완료 여부
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// 데이터 제공자 초기화
    /// 딕셔너리 생성 등 기본 설정을 수행합니다.
    /// </summary>
    void Initialize();

    /// <summary>
    /// 게임 데이터 로드
    /// ScriptableObject, JSON 등에서 데이터를 읽어 메모리에 캐싱합니다.
    /// </summary>
    void LoadData();

    /// <summary>
    /// 게임 데이터 언로드
    /// 메모리에서 데이터를 해제합니다.
    /// </summary>
    void UnloadData();

    /// <summary>
    /// 특정 키로 데이터 조회
    /// </summary>
    /// <typeparam name="T">데이터 타입</typeparam>
    /// <param name="key">데이터 키</param>
    /// <returns>데이터 (없으면 null)</returns>
    T GetData<T>(string key) where T : class;

    /// <summary>
    /// 특정 키의 데이터 존재 여부 확인
    /// </summary>
    /// <param name="key">데이터 키</param>
    /// <returns>데이터가 존재하면 true</returns>
    bool HasData(string key);
}

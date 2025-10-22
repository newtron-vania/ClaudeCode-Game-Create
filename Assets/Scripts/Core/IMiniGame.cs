/// <summary>
/// 모든 미니게임이 반드시 구현해야 하는 인터페이스
/// 이 인터페이스를 통해 GameManager는 게임의 구체적인 내용을 모르고도 생명주기를 관리할 수 있습니다.
/// OCP(개방-폐쇄 원칙) 준수: 새로운 게임 추가 시 이 인터페이스만 구현하면 됩니다.
/// </summary>
public interface IMiniGame
{
    /// <summary>
    /// 게임 초기화 - 공용 데이터 주입
    /// GameManager가 LoadGame 시 최초 1회 호출합니다.
    /// </summary>
    /// <param name="commonData">모든 게임이 공유하는 플레이어 데이터 (레벨, 골드 등)</param>
    void Initialize(CommonPlayerData commonData);

    /// <summary>
    /// 게임 시작
    /// Initialize 직후 호출되며, 이 시점에서 InputManager를 구독해야 합니다.
    /// 예: InputManager.Instance.OnInputEvent += HandleInput;
    /// </summary>
    void StartGame();

    /// <summary>
    /// 매 프레임 게임 로직 실행
    /// GameManager의 Update 루프에서 호출됩니다.
    /// </summary>
    /// <param name="deltaTime">이전 프레임과의 시간 차이</param>
    void Update(float deltaTime);

    /// <summary>
    /// 게임 종료 및 정리
    /// 다른 게임으로 전환되거나 앱 종료 시 호출됩니다.
    /// InputManager 구독을 반드시 해제해야 합니다.
    /// 예: InputManager.Instance.OnInputEvent -= HandleInput;
    /// </summary>
    void Cleanup();

    /// <summary>
    /// 게임 고유 데이터 반환
    /// GameManager가 현재 게임의 데이터를 저장/로드하기 위해 호출합니다.
    /// </summary>
    /// <returns>이 게임의 IGameData 구현 객체</returns>
    IGameData GetData();
}

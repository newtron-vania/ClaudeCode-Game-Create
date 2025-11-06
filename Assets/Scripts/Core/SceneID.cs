/// <summary>
/// 씬 ID 열거형
/// 모든 게임 씬을 구분하기 위한 고유 식별자
/// </summary>
public enum SceneID
{
    /// <summary>메인 메뉴 씬</summary>
    MainMenu = 0,

    /// <summary>테트리스 게임 씬</summary>
    Tetris = 1,

    /// <summary>스도쿠 게임 씬</summary>
    Sudoku = 2,

    /// <summary>슬라이딩 퍼즐 게임 씬</summary>
    SlidingPuzzle = 3,

    /// <summary>언데드 서바이버 초기 화면 (게임 시작, 설정, 게임 종료)</summary>
    UndeadSurvivor = 4,

    /// <summary>언데드 서바이버 캐릭터 선택 씬</summary>
    UndeadSurvivorCharacterSelectionScene = 5,

    /// <summary>언데드 서바이버 게임 플레이 씬 (실제 전투)</summary>
    UndeadSurvivorGameScene = 6,

    /// <summary>로딩 씬</summary>
    Loading = 10,

    /// <summary>설정 씬</summary>
    Settings = 20
}

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

    /// <summary>로딩 씬</summary>
    Loading = 10,

    /// <summary>설정 씬</summary>
    Settings = 20
}

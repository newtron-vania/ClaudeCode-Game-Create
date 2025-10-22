using UnityEngine;

/// <summary>
/// 입력 이벤트 타입
/// InputManager가 발생시키는 다양한 입력 종류를 구분합니다.
/// </summary>
public enum InputType
{
    /// <summary>키보드 키가 눌렸을 때</summary>
    KeyDown,

    /// <summary>키보드 키가 떼어졌을 때</summary>
    KeyUp,

    /// <summary>마우스 버튼이 눌렸을 때</summary>
    PointerDown,

    /// <summary>마우스 버튼이 떼어졌을 때</summary>
    PointerUp,

    /// <summary>마우스/터치가 이동 중일 때</summary>
    PointerMove,

    /// <summary>마우스 휠 스크롤</summary>
    PointerScroll,

    /// <summary>터치가 시작되었을 때</summary>
    TouchBegan,

    /// <summary>터치가 이동 중일 때</summary>
    TouchMoved,

    /// <summary>터치가 종료되었을 때</summary>
    TouchEnded
}

/// <summary>
/// 입력 이벤트 데이터 구조체
/// InputManager가 OnInputEvent를 통해 전송하는 입력 정보를 담습니다.
/// 키보드, 마우스(3버튼+휠), 터치 입력을 모두 포함할 수 있는 통합 구조체입니다.
/// </summary>
[System.Serializable]
public struct InputEventData
{
    /// <summary>
    /// 입력 이벤트 타입
    /// </summary>
    public InputType Type;

    /// <summary>
    /// 키보드 입력 시 눌린 키 코드
    /// KeyDown/KeyUp 타입에서만 유효합니다.
    /// </summary>
    public KeyCode KeyCode;

    /// <summary>
    /// 마우스/터치 입력 위치 (스크린 좌표)
    /// Pointer/Touch 타입 이벤트에서 유효합니다.
    /// </summary>
    public Vector2 PointerPosition;

    /// <summary>
    /// 터치 ID (멀티터치 지원)
    /// -1: 터치 아님, 0~: 터치 인덱스
    /// </summary>
    public int TouchID;

    /// <summary>
    /// 마우스 버튼 인덱스
    /// 0: 왼쪽 버튼, 1: 오른쪽 버튼, 2: 중간 버튼, -1: 버튼 없음
    /// </summary>
    public int PointerButton;

    /// <summary>
    /// 마우스 휠 스크롤 델타
    /// x: 수평 스크롤, y: 수직 스크롤
    /// </summary>
    public Vector2 ScrollDelta;

    /// <summary>
    /// 키보드 입력 이벤트 생성자
    /// </summary>
    /// <param name="type">입력 타입 (KeyDown 또는 KeyUp)</param>
    /// <param name="keyCode">눌린 키 코드</param>
    public InputEventData(InputType type, KeyCode keyCode)
    {
        Type = type;
        KeyCode = keyCode;
        PointerPosition = Vector2.zero;
        TouchID = -1;
        PointerButton = -1;
        ScrollDelta = Vector2.zero;
    }

    /// <summary>
    /// 마우스 입력 이벤트 생성자 (기존 호환성 유지)
    /// </summary>
    /// <param name="type">입력 타입 (PointerDown, PointerUp, PointerMove)</param>
    /// <param name="position">마우스 위치 (스크린 좌표)</param>
    public InputEventData(InputType type, Vector2 position)
    {
        Type = type;
        KeyCode = KeyCode.None;
        PointerPosition = position;
        TouchID = -1;
        PointerButton = 0; // 기본값: 왼쪽 버튼
        ScrollDelta = Vector2.zero;
    }

    /// <summary>
    /// 마우스 버튼 입력 이벤트 생성자 (확장)
    /// </summary>
    /// <param name="type">입력 타입 (PointerDown, PointerUp)</param>
    /// <param name="position">마우스 위치 (스크린 좌표)</param>
    /// <param name="pointerButton">마우스 버튼 (0: 왼쪽, 1: 오른쪽, 2: 중간)</param>
    public InputEventData(InputType type, Vector2 position, int pointerButton)
    {
        Type = type;
        KeyCode = KeyCode.None;
        PointerPosition = position;
        TouchID = -1;
        PointerButton = pointerButton;
        ScrollDelta = Vector2.zero;
    }

    /// <summary>
    /// 마우스 스크롤 입력 이벤트 생성자
    /// </summary>
    /// <param name="scrollDelta">스크롤 델타 (x: 수평, y: 수직)</param>
    public InputEventData(Vector2 scrollDelta)
    {
        Type = InputType.PointerScroll;
        KeyCode = KeyCode.None;
        PointerPosition = Input.mousePosition;
        TouchID = -1;
        PointerButton = -1;
        ScrollDelta = scrollDelta;
    }

    /// <summary>
    /// 터치 입력 이벤트 생성자
    /// </summary>
    /// <param name="type">입력 타입 (TouchBegan, TouchMoved, TouchEnded)</param>
    /// <param name="position">터치 위치 (스크린 좌표)</param>
    /// <param name="touchID">터치 ID</param>
    public static InputEventData CreateTouchEvent(InputType type, Vector2 position, int touchID)
    {
        InputEventData data;
        data.Type = type;
        data.KeyCode = KeyCode.None;
        data.PointerPosition = position;
        data.TouchID = touchID;
        data.PointerButton = -1;
        data.ScrollDelta = Vector2.zero;
        return data;
    }

    /// <summary>
    /// 디버깅용 문자열 변환
    /// </summary>
    public override string ToString()
    {
        if (Type == InputType.KeyDown || Type == InputType.KeyUp)
        {
            return $"InputEvent [{Type}] Key: {KeyCode}";
        }
        else if (Type == InputType.PointerScroll)
        {
            return $"InputEvent [{Type}] ScrollDelta: {ScrollDelta}";
        }
        else if (Type == InputType.PointerDown || Type == InputType.PointerUp)
        {
            string buttonName = PointerButton == 0 ? "Left" : PointerButton == 1 ? "Right" : "Middle";
            return $"InputEvent [{Type}] Button: {buttonName}, Position: {PointerPosition}";
        }
        else if (Type == InputType.TouchBegan || Type == InputType.TouchMoved || Type == InputType.TouchEnded)
        {
            return $"InputEvent [{Type}] TouchID: {TouchID}, Position: {PointerPosition}";
        }
        else
        {
            return $"InputEvent [{Type}] Position: {PointerPosition}";
        }
    }
}

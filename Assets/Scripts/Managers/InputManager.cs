using UnityEngine;
using System;

/// <summary>
/// 입력 관리자 (싱글톤)
/// 키보드, 마우스(3버튼+휠), 터치 입력을 통합 관리하고 OnInputEvent를 통해 브로드캐스트합니다.
/// 모든 미니게임은 이 매니저의 OnInputEvent를 구독하여 입력을 처리합니다.
/// </summary>
public class InputManager : Singleton<InputManager>
{
    /// <summary>
    /// 입력 이벤트 발생 시 브로드캐스트
    /// 모든 구독자에게 InputEventData를 전달합니다.
    /// </summary>
    public event Action<InputEventData> OnInputEvent;

    /// <summary>
    /// 입력 감지 활성화 여부
    /// false로 설정하면 입력을 무시합니다 (UI 팝업 표시 시 등)
    /// </summary>
    public bool IsInputEnabled { get; set; } = true;

    private Vector2 _previousMousePosition;

    protected override void Awake()
    {
        base.Awake();
        _previousMousePosition = Input.mousePosition;
    }

    private void Update()
    {
        if (!IsInputEnabled)
        {
            return;
        }

        HandleKeyboardInput();
        HandleMouseInput();
        HandleTouchInput();
    }

    /// <summary>
    /// 키보드 입력 처리
    /// 모든 키 입력을 감지하여 KeyDown/KeyUp 이벤트 발생
    /// </summary>
    private void HandleKeyboardInput()
    {
        // anyKey는 성능 최적화를 위한 조기 종료
        if (!Input.anyKey)
        {
            return;
        }

        // 모든 KeyCode를 순회하며 입력 감지
        foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                InputEventData eventData = new InputEventData(InputType.KeyDown, keyCode);
                OnInputEvent?.Invoke(eventData);
                Debug.Log($"[INFO] InputManager::HandleKeyboardInput - {eventData}");
            }

            if (Input.GetKeyUp(keyCode))
            {
                InputEventData eventData = new InputEventData(InputType.KeyUp, keyCode);
                OnInputEvent?.Invoke(eventData);
                Debug.Log($"[INFO] InputManager::HandleKeyboardInput - {eventData}");
            }
        }
    }

    /// <summary>
    /// 마우스 입력 처리
    /// 3개 버튼(왼쪽/오른쪽/중간), 휠 스크롤, 마우스 이동 감지
    /// </summary>
    private void HandleMouseInput()
    {
        Vector2 currentMousePosition = Input.mousePosition;

        // 마우스 버튼 입력 처리 (0: 왼쪽, 1: 오른쪽, 2: 중간)
        for (int button = 0; button < 3; button++)
        {
            if (Input.GetMouseButtonDown(button))
            {
                InputEventData eventData = new InputEventData(
                    InputType.PointerDown,
                    currentMousePosition,
                    button
                );
                OnInputEvent?.Invoke(eventData);
                Debug.Log($"[INFO] InputManager::HandleMouseInput - {eventData}");
            }

            if (Input.GetMouseButtonUp(button))
            {
                InputEventData eventData = new InputEventData(
                    InputType.PointerUp,
                    currentMousePosition,
                    button
                );
                OnInputEvent?.Invoke(eventData);
                Debug.Log($"[INFO] InputManager::HandleMouseInput - {eventData}");
            }
        }

        // 마우스 이동 감지
        if (Vector2.Distance(currentMousePosition, _previousMousePosition) > 0.1f)
        {
            InputEventData eventData = new InputEventData(InputType.PointerMove, currentMousePosition);
            OnInputEvent?.Invoke(eventData);
            _previousMousePosition = currentMousePosition;
        }

        // 마우스 휠 스크롤 감지
        Vector2 scrollDelta = Input.mouseScrollDelta;
        if (scrollDelta != Vector2.zero)
        {
            InputEventData eventData = new InputEventData(scrollDelta);
            OnInputEvent?.Invoke(eventData);
            Debug.Log($"[INFO] InputManager::HandleMouseInput - {eventData}");
        }
    }

    /// <summary>
    /// 터치 입력 처리 (모바일 환경)
    /// 멀티터치 지원 (최대 10개 동시 터치)
    /// </summary>
    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
        {
            return;
        }

        foreach (Touch touch in Input.touches)
        {
            InputType touchType;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchType = InputType.TouchBegan;
                    break;
                case TouchPhase.Moved:
                    touchType = InputType.TouchMoved;
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    touchType = InputType.TouchEnded;
                    break;
                default:
                    continue; // Stationary는 무시
            }

            InputEventData eventData = InputEventData.CreateTouchEvent(
                touchType,
                touch.position,
                touch.fingerId
            );
            OnInputEvent?.Invoke(eventData);
            Debug.Log($"[INFO] InputManager::HandleTouchInput - {eventData}");
        }
    }

    /// <summary>
    /// 입력 시스템 활성화
    /// UI 팝업이 닫힐 때 등 호출
    /// </summary>
    public void EnableInput()
    {
        IsInputEnabled = true;
        Debug.Log("[INFO] InputManager::EnableInput - Input system enabled");
    }

    /// <summary>
    /// 입력 시스템 비활성화
    /// UI 팝업이 표시될 때 등 호출
    /// </summary>
    public void DisableInput()
    {
        IsInputEnabled = false;
        Debug.Log("[INFO] InputManager::DisableInput - Input system disabled");
    }

    /// <summary>
    /// 모든 이벤트 구독 제거 (씬 전환 시 등)
    /// </summary>
    public void ClearAllSubscribers()
    {
        OnInputEvent = null;
        Debug.Log("[INFO] InputManager::ClearAllSubscribers - All event subscribers cleared");
    }
}

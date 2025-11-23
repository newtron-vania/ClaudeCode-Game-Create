using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스도쿠 숫자 입력 패드 UI
/// 1-9 숫자 버튼, 지우기 버튼 제공
/// </summary>
public class NumPadUI : MonoBehaviour
{
    [Header("Number Buttons (1-9)")]
    [SerializeField] private Button[] _numberButtons = new Button[9];

    [Header("Control Buttons")]
    [SerializeField] private Button _clearButton;  // 지우기 버튼

    [Header("Visual Settings")]
    [SerializeField] private Color _normalColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color _highlightColor = new Color(0.7f, 0.9f, 1f);
    [SerializeField] private Color _disabledColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("Auto Setup")]
    [SerializeField] private bool _autoSetupButtons = true;
    [SerializeField] private Transform _numberButtonsContainer;
    [SerializeField] private Transform _controlButtonsContainer;

    // 게임 참조
    private SudokuGame _game;

    // 이벤트
    public event Action<int> OnNumberInput;  // 1-9 입력
    public event Action OnClearInput;        // 지우기 (0 입력)

    private void Awake()
    {
        // 자동 버튼 찾기
        if (_autoSetupButtons)
        {
            AutoSetupButtons();
        }

        // 버튼 이벤트 등록
        RegisterButtonEvents();
    }

    private void OnDestroy()
    {
        // 버튼 이벤트 해제
        UnregisterButtonEvents();
    }

    /// <summary>
    /// NumPad 초기화
    /// </summary>
    /// <param name="game">스도쿠 게임 인스턴스</param>
    public void Initialize(SudokuGame game)
    {
        _game = game;

        Debug.Log("[INFO] NumPadUI::Initialize - NumPad initialized");
    }

    /// <summary>
    /// 자동으로 버튼 찾기
    /// </summary>
    private void AutoSetupButtons()
    {
        // 숫자 버튼 자동 찾기
        if (_numberButtonsContainer != null)
        {
            Button[] foundButtons = _numberButtonsContainer.GetComponentsInChildren<Button>();
            for (int i = 0; i < Mathf.Min(foundButtons.Length, 9); i++)
            {
                _numberButtons[i] = foundButtons[i];
            }
        }

        // 지우기 버튼 자동 찾기
        if (_controlButtonsContainer != null && _clearButton == null)
        {
            Button[] controlButtons = _controlButtonsContainer.GetComponentsInChildren<Button>();
            if (controlButtons.Length > 0)
            {
                _clearButton = controlButtons[0];
            }
        }
    }

    #region 버튼 이벤트

    private void RegisterButtonEvents()
    {
        // 숫자 버튼 (1-9)
        for (int i = 0; i < _numberButtons.Length; i++)
        {
            if (_numberButtons[i] != null)
            {
                int number = i + 1;  // 1-9
                _numberButtons[i].onClick.AddListener(() => OnNumberButtonClicked(number));
            }
        }

        // 지우기 버튼
        if (_clearButton != null)
        {
            _clearButton.onClick.AddListener(OnClearButtonClicked);
        }
    }

    private void UnregisterButtonEvents()
    {
        // 숫자 버튼
        for (int i = 0; i < _numberButtons.Length; i++)
        {
            if (_numberButtons[i] != null)
            {
                _numberButtons[i].onClick.RemoveAllListeners();
            }
        }

        // 지우기 버튼
        if (_clearButton != null)
        {
            _clearButton.onClick.RemoveAllListeners();
        }
    }

    private void OnNumberButtonClicked(int number)
    {
        Debug.Log($"[INFO] NumPadUI::OnNumberButtonClicked - Number: {number}");
        
        // 게임에 직접 입력
        if (_game != null)
        {
            _game.InputNumber(number);
        }

        // 이벤트 발생
        OnNumberInput?.Invoke(number);
    }

    private void OnClearButtonClicked()
    {
        Debug.Log("[INFO] NumPadUI::OnClearButtonClicked - Clear input");
        // 게임에 직접 입력 (0 = 삭제)
        if (_game != null)
        {
            _game.InputNumber(0);
        }

        // 이벤트 발생
        OnClearInput?.Invoke();
    }

    #endregion

    #region 버튼 상태 관리

    /// <summary>
    /// 특정 숫자 버튼 활성화/비활성화
    /// </summary>
    /// <param name="number">숫자 (1-9)</param>
    /// <param name="enabled">활성화 여부</param>
    public void SetNumberButtonEnabled(int number, bool enabled)
    {
        if (number < 1 || number > 9) return;

        int index = number - 1;
        if (_numberButtons[index] != null)
        {
            _numberButtons[index].interactable = enabled;

            // 색상 변경
            Image buttonImage = _numberButtons[index].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = enabled ? _normalColor : _disabledColor;
            }
        }
    }

    /// <summary>
    /// 모든 숫자 버튼 활성화/비활성화
    /// </summary>
    /// <param name="enabled">활성화 여부</param>
    public void SetAllButtonsEnabled(bool enabled)
    {
        for (int i = 1; i <= 9; i++)
        {
            SetNumberButtonEnabled(i, enabled);
        }

        if (_clearButton != null)
        {
            _clearButton.interactable = enabled;
        }
    }

    /// <summary>
    /// 특정 숫자 버튼 하이라이트
    /// </summary>
    /// <param name="number">숫자 (1-9)</param>
    /// <param name="highlighted">하이라이트 여부</param>
    public void HighlightNumberButton(int number, bool highlighted)
    {
        if (number < 1 || number > 9) return;

        int index = number - 1;
        if (_numberButtons[index] != null)
        {
            Image buttonImage = _numberButtons[index].GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = highlighted ? _highlightColor : _normalColor;
            }
        }
    }

    /// <summary>
    /// 모든 하이라이트 제거
    /// </summary>
    public void ClearHighlights()
    {
        for (int i = 1; i <= 9; i++)
        {
            HighlightNumberButton(i, false);
        }
    }

    /// <summary>
    /// 숫자별 남은 개수에 따라 버튼 비활성화
    /// (선택 기능: 해당 숫자가 9개 모두 사용되면 버튼 비활성화)
    /// </summary>
    /// <param name="board">현재 보드</param>
    public void UpdateAvailableNumbers(SudokuBoard board)
    {
        if (board == null) return;

        for (int number = 1; number <= 9; number++)
        {
            int count = CountNumber(board, number);
            bool available = count < 9;  // 9개 미만이면 사용 가능

            SetNumberButtonEnabled(number, available);
        }
    }

    /// <summary>
    /// 보드에서 특정 숫자의 개수 세기
    /// </summary>
    private int CountNumber(SudokuBoard board, int number)
    {
        int count = 0;
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (board.GetCell(row, col) == number)
                {
                    count++;
                }
            }
        }
        return count;
    }

    #endregion

    #region 키보드 입력 지원

    /// <summary>
    /// 키보드 입력 처리 (외부에서 호출)
    /// </summary>
    /// <param name="keyCode">입력된 키</param>
    public void HandleKeyboardInput(KeyCode keyCode)
    {
        // 숫자 1-9
        if (keyCode >= KeyCode.Alpha1 && keyCode <= KeyCode.Alpha9)
        {
            int number = keyCode - KeyCode.Alpha0;
            OnNumberButtonClicked(number);
        }
        // Numpad 1-9
        else if (keyCode >= KeyCode.Keypad1 && keyCode <= KeyCode.Keypad9)
        {
            int number = keyCode - KeyCode.Keypad0;
            OnNumberButtonClicked(number);
        }
        // Backspace, Delete, 0
        else if (keyCode == KeyCode.Backspace || keyCode == KeyCode.Delete ||
                 keyCode == KeyCode.Alpha0 || keyCode == KeyCode.Keypad0)
        {
            OnClearButtonClicked();
        }
    }

    #endregion
}

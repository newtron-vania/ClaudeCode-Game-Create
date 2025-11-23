using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UndeadSurvivor;

/// <summary>
/// 레벨업 4지선다 UI 패널
/// Time.timeScale = 0으로 게임 일시정지 후 선택지 표시
/// </summary>
public class LevelUpUIPanel : UIPanel
{
    [Header("Option Buttons")]
    [SerializeField] private LevelUpOptionButton[] _optionButtons; // 4개의 선택지 버튼

    [Header("Panel Container")]
    [SerializeField] private GameObject _panelContainer; // UI 루트 컨테이너

    private Player _player;
    private LevelUpManager _levelUpManager;
    private List<LevelUpOption> _currentOptions;

    private bool _isWaitingForSelection = false;

    /// <summary>
    /// 선택 완료 이벤트 (UI 닫기용)
    /// </summary>
    public event Action OnSelectionComplete;

    #region Initialization

    protected override void Awake()
    {
        base.Awake();

        // 옵션 버튼 이벤트 구독
        if (_optionButtons != null)
        {
            for (int i = 0; i < _optionButtons.Length; i++)
            {
                int index = i; // 클로저 캡처 방지
                _optionButtons[i].OnButtonClicked += () => OnOptionSelected(index);
            }
        }
    }

    /// <summary>
    /// 초기화 (Player 및 LevelUpManager 연동)
    /// </summary>
    public void Initialize(Player player, LevelUpManager levelUpManager)
    {
        _player = player;
        _levelUpManager = levelUpManager;

        Debug.Log("[INFO] LevelUpUIPanel::Initialize - LevelUpUIPanel initialized");
    }

    #endregion

    #region Show/Hide

    /// <summary>
    /// 레벨업 UI 표시 (게임 일시정지)
    /// </summary>
    public void ShowLevelUpOptions()
    {
        if (_levelUpManager == null)
        {
            Debug.LogError("[ERROR] LevelUpUIPanel::ShowLevelUpOptions - LevelUpManager is null");
            return;
        }

        // 선택지 생성
        _currentOptions = _levelUpManager.GenerateLevelUpOptions();

        if (_currentOptions == null || _currentOptions.Count == 0)
        {
            Debug.LogError("[ERROR] LevelUpUIPanel::ShowLevelUpOptions - No options generated");
            return;
        }

        // UI 업데이트
        UpdateOptionButtons();

        // 게임 일시정지
        Time.timeScale = 0f;
        _isWaitingForSelection = true;

        // UI 표시
        Open();

        Debug.Log($"[INFO] LevelUpUIPanel::ShowLevelUpOptions - Showing {_currentOptions.Count} options, Game paused");
    }

    /// <summary>
    /// 패널 열기 오버라이드
    /// </summary>
    public override void Open()
    {
        base.Open();

        if (_panelContainer != null)
        {
            _panelContainer.SetActive(true);
        }
    }

    /// <summary>
    /// 패널 닫기 오버라이드
    /// </summary>
    public override void Close()
    {
        base.Close();

        if (_panelContainer != null)
        {
            _panelContainer.SetActive(false);
        }

        // 게임 재개
        Time.timeScale = 1f;
        _isWaitingForSelection = false;

        Debug.Log("[INFO] LevelUpUIPanel::Close - UI closed, Game resumed");
    }

    #endregion

    #region Option UI Update

    /// <summary>
    /// 선택지 버튼 UI 업데이트
    /// </summary>
    private void UpdateOptionButtons()
    {
        if (_optionButtons == null || _currentOptions == null)
        {
            Debug.LogError("[ERROR] LevelUpUIPanel::UpdateOptionButtons - Buttons or options are null");
            return;
        }

        int optionCount = Mathf.Min(_optionButtons.Length, _currentOptions.Count);

        for (int i = 0; i < _optionButtons.Length; i++)
        {
            if (i < optionCount)
            {
                // 옵션 데이터로 버튼 업데이트
                _optionButtons[i].SetOption(_currentOptions[i]);
                _optionButtons[i].gameObject.SetActive(true);
            }
            else
            {
                // 남는 버튼 비활성화
                _optionButtons[i].gameObject.SetActive(false);
            }
        }

        Debug.Log($"[INFO] LevelUpUIPanel::UpdateOptionButtons - Updated {optionCount} buttons");
    }

    #endregion

    #region Selection Handling

    /// <summary>
    /// 선택지 선택 시 호출
    /// </summary>
    private void OnOptionSelected(int optionIndex)
    {
        if (!_isWaitingForSelection)
        {
            Debug.LogWarning("[WARNING] LevelUpUIPanel::OnOptionSelected - Not waiting for selection");
            return;
        }

        if (optionIndex < 0 || optionIndex >= _currentOptions.Count)
        {
            Debug.LogError($"[ERROR] LevelUpUIPanel::OnOptionSelected - Invalid option index: {optionIndex}");
            return;
        }

        LevelUpOption selectedOption = _currentOptions[optionIndex];

        Debug.Log($"[INFO] LevelUpUIPanel::OnOptionSelected - Selected option {optionIndex}: {selectedOption.Title}");

        // 선택지 적용
        ApplySelection(selectedOption);

        // UI 닫기
        OnSelectionComplete?.Invoke();
        Close();

        // 플레이어 이동 재개
        if (_player != null)
        {
            _player.ResumeMovement();
        }
    }

    /// <summary>
    /// 선택지 적용 (플레이어에게 효과 적용)
    /// </summary>
    private void ApplySelection(LevelUpOption option)
    {
        if (_player == null || option == null)
        {
            Debug.LogError("[ERROR] LevelUpUIPanel::ApplySelection - Player or option is null");
            return;
        }

        // LevelUpOption의 Apply 메서드 호출
        option.Apply(_player);

        Debug.Log($"[INFO] LevelUpUIPanel::ApplySelection - Applied: {option.Title}");
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_optionButtons != null)
        {
            foreach (var button in _optionButtons)
            {
                if (button != null)
                {
                    button.OnButtonClicked -= null;
                }
            }
        }

        // 게임 재개 보장 (안전장치)
        if (Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
            Debug.LogWarning("[WARNING] LevelUpUIPanel::OnDestroy - Force resuming game (timeScale was 0)");
        }
    }

    #endregion
}

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스도쿠 셀 버튼
/// 개별 셀의 숫자 표시, 상태 관리, 클릭 이벤트 처리
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class SudokuCellButton : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _background;
    [SerializeField] private TextMeshProUGUI _numberText;

    [Header("Visual Settings")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _selectedColor = new Color(0.7f, 0.9f, 1f);
    [SerializeField] private Color _fixedColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color _errorColor = new Color(1f, 0.6f, 0.6f);
    [SerializeField] private Color _highlightColor = new Color(0.9f, 0.95f, 1f);

    [Header("Text Settings")]
    [SerializeField] private Color _normalTextColor = Color.black;
    [SerializeField] private Color _fixedTextColor = new Color(0.2f, 0.2f, 0.2f);
    [SerializeField] private Color _errorTextColor = Color.red;
    [SerializeField] private int _normalFontSize = 36;
    [SerializeField] private int _fixedFontSize = 40;

    // 셀 정보
    private int _row;
    private int _col;
    private int _value;

    // 상태
    private bool _isFixed;
    private bool _isSelected;
    private bool _hasError;
    private bool _isHighlighted;
    private Color _highlightTint = Color.white;

    // 이벤트
    public event Action<int, int> OnCellClicked;

    private void Awake()
    {
        // 컴포넌트 자동 찾기
        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_background == null)
        {
            _background = GetComponent<Image>();
        }

        if (_numberText == null)
        {
            _numberText = GetComponentInChildren<TextMeshProUGUI>();
        }

        // 버튼 클릭 이벤트 등록
        if (_button != null)
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        // 초기 상태
        _value = 0;
        _isFixed = false;
        _isSelected = false;
        _hasError = false;
        _isHighlighted = false;

        UpdateVisual();
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }
    }

    /// <summary>
    /// 셀 초기화
    /// </summary>
    /// <param name="row">행 (0-8)</param>
    /// <param name="col">열 (0-8)</param>
    public void Initialize(int row, int col)
    {
        _row = row;
        _col = col;
        _value = 0;
        _isFixed = false;
        _isSelected = false;
        _hasError = false;
        _isHighlighted = false;

        UpdateVisual();
    }

    /// <summary>
    /// 버튼 클릭 이벤트
    /// </summary>
    private void OnButtonClicked()
    {
        OnCellClicked?.Invoke(_row, _col);
    }

    #region 값 및 상태 설정

    /// <summary>
    /// 셀 값 설정
    /// </summary>
    /// <param name="value">숫자 (0-9, 0은 빈 셀)</param>
    public void SetValue(int value)
    {
        _value = Mathf.Clamp(value, 0, 9);
        UpdateVisual();
    }

    /// <summary>
    /// 고정 셀 여부 설정 (초기 힌트)
    /// </summary>
    /// <param name="isFixed">고정 여부</param>
    public void SetFixed(bool isFixed)
    {
        _isFixed = isFixed;
        UpdateVisual();
    }

    /// <summary>
    /// 선택 상태 설정
    /// </summary>
    /// <param name="isSelected">선택 여부</param>
    public void SetSelected(bool isSelected)
    {
        _isSelected = isSelected;
        UpdateVisual();
    }

    /// <summary>
    /// 에러 상태 설정
    /// </summary>
    /// <param name="hasError">에러 여부</param>
    public void SetError(bool hasError)
    {
        _hasError = hasError;
        UpdateVisual();
    }

    /// <summary>
    /// 하이라이트 설정 (동일 숫자 강조)
    /// </summary>
    /// <param name="isHighlighted">하이라이트 여부</param>
    /// <param name="color">하이라이트 색상</param>
    public void SetHighlight(bool isHighlighted, Color color)
    {
        _isHighlighted = isHighlighted;
        _highlightTint = color;
        UpdateVisual();
    }

    /// <summary>
    /// 버튼 상호작용 가능 여부 설정
    /// </summary>
    /// <param name="interactable">상호작용 가능 여부</param>
    public void SetInteractable(bool interactable)
    {
        if (_button != null)
        {
            _button.interactable = interactable;
        }
    }

    #endregion

    #region 시각 업데이트

    /// <summary>
    /// 시각 요소 업데이트
    /// </summary>
    private void UpdateVisual()
    {
        UpdateBackgroundColor();
        UpdateNumberText();
    }

    /// <summary>
    /// 배경 색상 업데이트
    /// </summary>
    private void UpdateBackgroundColor()
    {
        if (_background == null) return;

        Color targetColor = _normalColor;

        // 우선순위: 에러 > 선택 > 하이라이트 > 고정 > 일반
        if (_hasError)
        {
            targetColor = _errorColor;
        }
        else if (_isSelected)
        {
            targetColor = _selectedColor;
        }
        else if (_isHighlighted)
        {
            targetColor = _highlightTint;
        }
        else if (_isFixed)
        {
            targetColor = _fixedColor;
        }

        _background.color = targetColor;
    }

    /// <summary>
    /// 숫자 텍스트 업데이트
    /// </summary>
    private void UpdateNumberText()
    {
        if (_numberText == null) return;

        // 숫자 표시 (0이면 빈 문자열)
        _numberText.text = _value > 0 ? _value.ToString() : "";

        // 텍스트 색상
        Color textColor = _normalTextColor;
        if (_hasError)
        {
            textColor = _errorTextColor;
        }
        else if (_isFixed)
        {
            textColor = _fixedTextColor;
        }

        _numberText.color = textColor;

        // 폰트 크기 (고정 셀은 더 크게)
        _numberText.fontSize = _isFixed ? _fixedFontSize : _normalFontSize;

        // 폰트 스타일 (고정 셀은 Bold)
        _numberText.fontStyle = _isFixed ? FontStyles.Bold : FontStyles.Normal;
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// 행 인덱스
    /// </summary>
    public int Row => _row;

    /// <summary>
    /// 열 인덱스
    /// </summary>
    public int Col => _col;

    /// <summary>
    /// 현재 값
    /// </summary>
    public int Value => _value;

    /// <summary>
    /// 고정 셀 여부
    /// </summary>
    public bool IsFixed => _isFixed;

    /// <summary>
    /// 선택 상태
    /// </summary>
    public bool IsSelected => _isSelected;

    /// <summary>
    /// 에러 상태
    /// </summary>
    public bool HasError => _hasError;

    #endregion
}

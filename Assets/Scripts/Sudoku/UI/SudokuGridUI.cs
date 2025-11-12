using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스도쿠 9x9 그리드 UI 관리
/// SudokuCellButton 인스턴스들을 동적 생성 및 관리
/// 선택 상태, 에러 하이라이팅, 보드 데이터 동기화
/// </summary>
public class SudokuGridUI : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private GridLayoutGroup _gridLayout;
    [SerializeField] private RectTransform _gridContainer;
    [SerializeField] private GameObject _cellButtonPrefab;

    [Header("Visual Settings")]
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _selectedColor = new Color(0.7f, 0.9f, 1f);
    [SerializeField] private Color _fixedColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color _errorColor = new Color(1f, 0.6f, 0.6f);
    [SerializeField] private Color _sameNumberColor = new Color(0.9f, 0.95f, 1f);

    [Header("Box Lines")]
    [SerializeField] private float _normalLineWidth = 1f;
    [SerializeField] private float _boldLineWidth = 3f;
    [SerializeField] private Color _lineColor = Color.black;

    private SudokuGame _game;
    private SudokuBoard _board;
    private SudokuCellButton[,] _cellButtons = new SudokuCellButton[9, 9];
    private SudokuCellButton _selectedCell;

    private int _selectedRow = -1;
    private int _selectedCol = -1;

    /// <summary>
    /// 그리드 UI 초기화
    /// </summary>
    /// <param name="game">스도쿠 게임 인스턴스</param>
    public void Initialize(SudokuGame game)
    {
        _game = game;
        _board = _game.Board;

        // 그리드 레이아웃 설정
        SetupGridLayout();

        // 9x9 셀 버튼 생성
        CreateCellButtons();

        Debug.Log("[INFO] SudokuGridUI::Initialize - Grid initialized");
    }

    /// <summary>
    /// 그리드 레이아웃 설정
    /// </summary>
    private void SetupGridLayout()
    {
        if (_gridLayout == null)
        {
            _gridLayout = _gridContainer.GetComponent<GridLayoutGroup>();
            if (_gridLayout == null)
            {
                _gridLayout = _gridContainer.gameObject.AddComponent<GridLayoutGroup>();
            }
        }

        // 3x3 = 9칸
        _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        _gridLayout.constraintCount = 9;

        // 셀 크기 자동 계산
        float containerSize = _gridContainer.rect.width;
        float cellSize = (containerSize - _gridLayout.spacing.x * 8) / 9f;
        _gridLayout.cellSize = new Vector2(cellSize, cellSize);

        // 간격 설정
        _gridLayout.spacing = new Vector2(_normalLineWidth, _normalLineWidth);
        _gridLayout.padding = new RectOffset(5, 5, 5, 5);
    }

    /// <summary>
    /// 9x9 셀 버튼 동적 생성
    /// </summary>
    private void CreateCellButtons()
    {
        if (_cellButtonPrefab == null)
        {
            Debug.LogError("[ERROR] SudokuGridUI::CreateCellButtons - Cell button prefab is null!");
            return;
        }

        // 기존 셀 제거
        ClearCellButtons();

        // 9x9 셀 생성
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                CreateCellButton(row, col);
            }
        }

        Debug.Log("[INFO] SudokuGridUI::CreateCellButtons - Created 81 cell buttons");
    }

    /// <summary>
    /// 개별 셀 버튼 생성
    /// </summary>
    private void CreateCellButton(int row, int col)
    {
        GameObject cellObj = Instantiate(_cellButtonPrefab, _gridContainer);
        SudokuCellButton cellButton = cellObj.GetComponent<SudokuCellButton>();

        if (cellButton != null)
        {
            // 셀 초기화
            cellButton.Initialize(row, col);

            // 클릭 이벤트 등록
            cellButton.OnCellClicked += OnCellClicked;

            // 배열에 저장
            _cellButtons[row, col] = cellButton;

            // 3x3 박스 경계선 굵게 표시
            ApplyBoxBorders(cellButton, row, col);
        }
        else
        {
            Debug.LogError($"[ERROR] SudokuGridUI::CreateCellButton - SudokuCellButton component not found on prefab!");
            Destroy(cellObj);
        }
    }

    /// <summary>
    /// 3x3 박스 경계선 적용
    /// </summary>
    private void ApplyBoxBorders(SudokuCellButton cellButton, int row, int col)
    {
        // 3x3 박스 경계 (0, 3, 6 인덱스에서 굵은 선)
        bool boldTop = row % 3 == 0;
        bool boldLeft = col % 3 == 0;
        bool boldBottom = row == 8 || row % 3 == 2;
        bool boldRight = col == 8 || col % 3 == 2;

        // 셀 버튼에 경계선 정보 전달 (구현 필요)
        // cellButton.SetBorders(boldTop, boldLeft, boldBottom, boldRight, _boldLineWidth, _normalLineWidth, _lineColor);
    }

    /// <summary>
    /// 기존 셀 버튼 모두 제거
    /// </summary>
    private void ClearCellButtons()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (_cellButtons[row, col] != null)
                {
                    _cellButtons[row, col].OnCellClicked -= OnCellClicked;
                    Destroy(_cellButtons[row, col].gameObject);
                    _cellButtons[row, col] = null;
                }
            }
        }

        _selectedCell = null;
        _selectedRow = -1;
        _selectedCol = -1;
    }

    #region 보드 데이터 → UI 동기화

    /// <summary>
    /// 전체 보드를 UI에 반영
    /// </summary>
    public void UpdateBoard()
    {
        if (_board == null) return;

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                UpdateCell(row, col);
            }
        }

        Debug.Log("[INFO] SudokuGridUI::UpdateBoard - Board updated");
    }

    /// <summary>
    /// 특정 셀 업데이트
    /// </summary>
    public void UpdateCell(int row, int col)
    {
        if (_cellButtons[row, col] == null) return;

        int value = _board.GetCell(row, col);
        bool isFixed = _board.IsCellFixed(row, col);
        bool hasError = _board.HasCellError(row, col);

        _cellButtons[row, col].SetValue(value);
        _cellButtons[row, col].SetFixed(isFixed);
        _cellButtons[row, col].SetError(hasError);

        // 고정 셀은 편집 불가
        _cellButtons[row, col].SetInteractable(!isFixed);
    }

    /// <summary>
    /// 에러 하이라이팅 업데이트
    /// </summary>
    public void UpdateErrors()
    {
        if (_board == null) return;

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                bool hasError = _board.HasCellError(row, col);
                _cellButtons[row, col]?.SetError(hasError);
            }
        }
    }

    /// <summary>
    /// 동일 숫자 하이라이팅
    /// </summary>
    public void HighlightSameNumbers(int number)
    {
        if (number < 1 || number > 9) return;

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                bool isSame = _board.GetCell(row, col) == number;
                _cellButtons[row, col]?.SetHighlight(isSame, _sameNumberColor);
            }
        }
    }

    /// <summary>
    /// 모든 하이라이팅 제거
    /// </summary>
    public void ClearHighlights()
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                _cellButtons[row, col]?.SetHighlight(false, Color.white);
            }
        }
    }

    #endregion

    #region 셀 선택

    /// <summary>
    /// 셀 클릭 이벤트 핸들러
    /// </summary>
    private void OnCellClicked(int row, int col)
    {
        // 고정 셀은 선택 불가
        if (_board.IsCellFixed(row, col))
        {
            Debug.Log($"[INFO] SudokuGridUI::OnCellClicked - Fixed cell cannot be selected: ({row}, {col})");
            return;
        }

        SelectCell(row, col);
    }

    /// <summary>
    /// 셀 선택
    /// </summary>
    public void SelectCell(int row, int col)
    {
        // 이전 선택 해제
        if (_selectedCell != null)
        {
            _selectedCell.SetSelected(false);
        }

        // 새로운 셀 선택
        _selectedRow = row;
        _selectedCol = col;
        _selectedCell = _cellButtons[row, col];

        if (_selectedCell != null)
        {
            _selectedCell.SetSelected(true);

            // 동일 숫자 하이라이팅
            int value = _board.GetCell(row, col);
            if (value != 0)
            {
                HighlightSameNumbers(value);
            }
            else
            {
                ClearHighlights();
            }

            Debug.Log($"[INFO] SudokuGridUI::SelectCell - Cell selected: ({row}, {col})");
        }
    }

    /// <summary>
    /// 현재 선택된 셀에 숫자 입력
    /// </summary>
    /// <param name="number">입력할 숫자 (1-9, 0은 삭제)</param>
    public void InputNumber(int number)
    {
        if (_selectedRow < 0 || _selectedCol < 0)
        {
            Debug.LogWarning("[WARNING] SudokuGridUI::InputNumber - No cell selected");
            return;
        }

        // 고정 셀은 입력 불가
        if (_board.IsCellFixed(_selectedRow, _selectedCol))
        {
            Debug.LogWarning("[WARNING] SudokuGridUI::InputNumber - Cannot modify fixed cell");
            return;
        }

        // 보드에 숫자 설정
        _board.SetCell(_selectedRow, _selectedCol, number);

        // UI 업데이트
        UpdateCell(_selectedRow, _selectedCol);

        // 에러 체크 및 업데이트
        UpdateErrors();

        // 동일 숫자 하이라이팅
        if (number != 0)
        {
            HighlightSameNumbers(number);
        }
        else
        {
            ClearHighlights();
        }

        Debug.Log($"[INFO] SudokuGridUI::InputNumber - Input {number} at ({_selectedRow}, {_selectedCol})");
    }

    /// <summary>
    /// 선택 해제
    /// </summary>
    public void DeselectCell()
    {
        if (_selectedCell != null)
        {
            _selectedCell.SetSelected(false);
            _selectedCell = null;
        }

        _selectedRow = -1;
        _selectedCol = -1;

        ClearHighlights();
    }

    #endregion

    #region Public API

    /// <summary>
    /// 현재 선택된 행
    /// </summary>
    public int SelectedRow => _selectedRow;

    /// <summary>
    /// 현재 선택된 열
    /// </summary>
    public int SelectedCol => _selectedCol;

    /// <summary>
    /// 셀이 선택되어 있는지 여부
    /// </summary>
    public bool HasSelection => _selectedRow >= 0 && _selectedCol >= 0;

    #endregion

    private void OnDestroy()
    {
        // 모든 셀 버튼 이벤트 해제
        ClearCellButtons();
    }
}

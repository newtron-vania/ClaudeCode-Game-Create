using UnityEngine;

/// <summary>
/// 스도쿠 9x9 보드 상태를 관리하는 클래스
/// </summary>
public class SudokuBoard
{
    private const int SIZE = 9;
    private const int BOX_SIZE = 3;

    private int[,] _board;              // 현재 보드 상태 (0 = 빈 칸, 1-9 = 숫자)
    private int[,] _solution;           // 정답 보드
    private bool[,] _isFixed;           // 초기 힌트 셀 여부 (수정 불가능)
    private bool[,] _hasError;          // 에러 표시 (규칙 위반)

    /// <summary>
    /// 보드 크기
    /// </summary>
    public int Size => SIZE;

    /// <summary>
    /// 3x3 박스 크기
    /// </summary>
    public int BoxSize => BOX_SIZE;

    /// <summary>
    /// 현재 보드 상태
    /// </summary>
    public int[,] Board => _board;

    /// <summary>
    /// 정답 보드
    /// </summary>
    public int[,] Solution => _solution;

    /// <summary>
    /// 고정 셀 여부
    /// </summary>
    public bool[,] IsFixed => _isFixed;

    /// <summary>
    /// 에러 표시
    /// </summary>
    public bool[,] HasError => _hasError;

    /// <summary>
    /// 생성자
    /// </summary>
    public SudokuBoard()
    {
        _board = new int[SIZE, SIZE];
        _solution = new int[SIZE, SIZE];
        _isFixed = new bool[SIZE, SIZE];
        _hasError = new bool[SIZE, SIZE];

        ClearBoard();
    }

    /// <summary>
    /// 보드 초기화
    /// </summary>
    public void ClearBoard()
    {
        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                _board[row, col] = 0;
                _solution[row, col] = 0;
                _isFixed[row, col] = false;
                _hasError[row, col] = false;
            }
        }

        Debug.Log("[INFO] SudokuBoard::ClearBoard - Board cleared");
    }

    /// <summary>
    /// 정답 보드 설정
    /// </summary>
    /// <param name="solution">정답 보드 (9x9)</param>
    public void SetSolution(int[,] solution)
    {
        if (solution == null || solution.GetLength(0) != SIZE || solution.GetLength(1) != SIZE)
        {
            Debug.LogError("[ERROR] SudokuBoard::SetSolution - Invalid solution board size");
            return;
        }

        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                _solution[row, col] = solution[row, col];
            }
        }

        Debug.Log("[INFO] SudokuBoard::SetSolution - Solution board set");
    }

    /// <summary>
    /// 초기 힌트 셀 설정
    /// </summary>
    /// <param name="hints">힌트 배열 (true = 고정 셀)</param>
    public void SetInitialHints(bool[,] hints)
    {
        if (hints == null || hints.GetLength(0) != SIZE || hints.GetLength(1) != SIZE)
        {
            Debug.LogError("[ERROR] SudokuBoard::SetInitialHints - Invalid hints array size");
            return;
        }

        int hintCount = 0;
        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                _isFixed[row, col] = hints[row, col];
                if (_isFixed[row, col])
                {
                    _board[row, col] = _solution[row, col];
                    hintCount++;
                }
                else
                {
                    _board[row, col] = 0;
                }
            }
        }

        Debug.Log($"[INFO] SudokuBoard::SetInitialHints - Initial hints set ({hintCount} hints applied)");
    }

    /// <summary>
    /// 셀 값 설정
    /// </summary>
    /// <param name="row">행 (0-8)</param>
    /// <param name="col">열 (0-8)</param>
    /// <param name="value">값 (0-9, 0은 빈 칸)</param>
    /// <returns>설정 성공 여부</returns>
    public bool SetCell(int row, int col, int value)
    {
        if (!IsValidPosition(row, col))
        {
            Debug.LogError($"[ERROR] SudokuBoard::SetCell - Invalid position ({row}, {col})");
            return false;
        }

        if (_isFixed[row, col])
        {
            Debug.LogWarning($"[WARNING] SudokuBoard::SetCell - Cannot modify fixed cell ({row}, {col})");
            return false;
        }

        if (value < 0 || value > 9)
        {
            Debug.LogError($"[ERROR] SudokuBoard::SetCell - Invalid value {value}");
            return false;
        }

        _board[row, col] = value;
        return true;
    }

    /// <summary>
    /// 셀 값 가져오기
    /// </summary>
    /// <param name="row">행 (0-8)</param>
    /// <param name="col">열 (0-8)</param>
    /// <returns>셀 값 (0-9)</returns>
    public int GetCell(int row, int col)
    {
        if (!IsValidPosition(row, col))
        {
            Debug.LogError($"[ERROR] SudokuBoard::GetCell - Invalid position ({row}, {col})");
            return 0;
        }

        return _board[row, col];
    }

    /// <summary>
    /// 정답 셀 값 가져오기
    /// </summary>
    /// <param name="row">행 (0-8)</param>
    /// <param name="col">열 (0-8)</param>
    /// <returns>정답 값 (1-9)</returns>
    public int GetSolution(int row, int col)
    {
        if (!IsValidPosition(row, col))
        {
            Debug.LogError($"[ERROR] SudokuBoard::GetSolution - Invalid position ({row}, {col})");
            return 0;
        }

        return _solution[row, col];
    }

    /// <summary>
    /// 셀이 고정되어 있는지 확인
    /// </summary>
    /// <param name="row">행 (0-8)</param>
    /// <param name="col">열 (0-8)</param>
    /// <returns>고정 셀이면 true</returns>
    public bool IsCellFixed(int row, int col)
    {
        if (!IsValidPosition(row, col))
        {
            return false;
        }

        return _isFixed[row, col];
    }

    /// <summary>
    /// 셀에 에러가 있는지 확인
    /// </summary>
    /// <param name="row">행 (0-8)</param>
    /// <param name="col">열 (0-8)</param>
    /// <returns>에러가 있으면 true</returns>
    public bool HasCellError(int row, int col)
    {
        if (!IsValidPosition(row, col))
        {
            return false;
        }

        return _hasError[row, col];
    }

    /// <summary>
    /// 위치가 유효한지 확인
    /// </summary>
    /// <param name="row">행</param>
    /// <param name="col">열</param>
    /// <returns>유효하면 true</returns>
    public bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < SIZE && col >= 0 && col < SIZE;
    }

    /// <summary>
    /// 모든 셀이 채워졌는지 확인
    /// </summary>
    /// <returns>모든 셀이 채워졌으면 true</returns>
    public bool IsAllCellsFilled()
    {
        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                if (_board[row, col] == 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 보드가 정답과 일치하는지 확인
    /// </summary>
    /// <returns>정답이면 true</returns>
    public bool IsSolved()
    {
        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                if (_board[row, col] != _solution[row, col])
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 에러 표시 업데이트
    /// </summary>
    /// <param name="errors">에러 배열</param>
    public void UpdateErrors(bool[,] errors)
    {
        if (errors == null || errors.GetLength(0) != SIZE || errors.GetLength(1) != SIZE)
        {
            Debug.LogError("[ERROR] SudokuBoard::UpdateErrors - Invalid errors array size");
            return;
        }

        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                _hasError[row, col] = errors[row, col];
            }
        }
    }

    /// <summary>
    /// 초기 상태로 리셋 (힌트만 남김)
    /// </summary>
    public void ResetToInitial()
    {
        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                if (!_isFixed[row, col])
                {
                    _board[row, col] = 0;
                }
                _hasError[row, col] = false;
            }
        }

        Debug.Log("[INFO] SudokuBoard::ResetToInitial - Board reset to initial state");
    }

    /// <summary>
    /// 보드 상태를 문자열로 변환 (디버깅용)
    /// </summary>
    /// <returns>보드 상태 문자열</returns>
    public string ToString()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Sudoku Board:");

        for (int row = 0; row < SIZE; row++)
        {
            if (row % BOX_SIZE == 0 && row != 0)
            {
                sb.AppendLine("------+-------+------");
            }

            for (int col = 0; col < SIZE; col++)
            {
                if (col % BOX_SIZE == 0 && col != 0)
                {
                    sb.Append("| ");
                }

                int value = _board[row, col];
                if (value == 0)
                {
                    sb.Append(". ");
                }
                else
                {
                    sb.Append($"{value} ");
                }
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// 보드 복사
    /// </summary>
    /// <returns>복사된 보드</returns>
    public int[,] CopyBoard()
    {
        int[,] copy = new int[SIZE, SIZE];

        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                copy[row, col] = _board[row, col];
            }
        }

        return copy;
    }

    /// <summary>
    /// 정답 보드 복사
    /// </summary>
    /// <returns>복사된 정답 보드</returns>
    public int[,] CopySolution()
    {
        int[,] copy = new int[SIZE, SIZE];

        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                copy[row, col] = _solution[row, col];
            }
        }

        return copy;
    }
}

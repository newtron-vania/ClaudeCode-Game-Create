using UnityEngine;

/// <summary>
/// 스도쿠 규칙 검증 클래스
/// 행, 열, 3x3 박스의 중복 검사 및 유효성 검증
/// </summary>
public class SudokuValidator
{
    private const int SIZE = 9;
    private const int BOX_SIZE = 3;

    /// <summary>
    /// 보드가 유효한지 검증 (규칙 위반 여부)
    /// </summary>
    /// <param name="board">검증할 보드</param>
    /// <returns>유효하면 true</returns>
    public static bool IsValid(int[,] board)
    {
        if (board == null)
        {
            Debug.LogError("[ERROR] SudokuValidator::IsValid - Board is null");
            return false;
        }

        if (board.GetLength(0) != SIZE || board.GetLength(1) != SIZE)
        {
            Debug.LogError("[ERROR] SudokuValidator::IsValid - Invalid board size");
            return false;
        }

        // 모든 행 검사
        for (int row = 0; row < SIZE; row++)
        {
            if (!IsRowValid(board, row))
            {
                return false;
            }
        }

        // 모든 열 검사
        for (int col = 0; col < SIZE; col++)
        {
            if (!IsColumnValid(board, col))
            {
                return false;
            }
        }

        // 모든 3x3 박스 검사
        for (int boxRow = 0; boxRow < SIZE; boxRow += BOX_SIZE)
        {
            for (int boxCol = 0; boxCol < SIZE; boxCol += BOX_SIZE)
            {
                if (!IsBoxValid(board, boxRow, boxCol))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 특정 행이 유효한지 검증
    /// </summary>
    /// <param name="board">보드</param>
    /// <param name="row">행 번호</param>
    /// <returns>유효하면 true</returns>
    public static bool IsRowValid(int[,] board, int row)
    {
        bool[] used = new bool[SIZE + 1]; // 1-9 사용 여부

        for (int col = 0; col < SIZE; col++)
        {
            int value = board[row, col];

            if (value == 0) continue; // 빈 칸은 무시

            if (value < 1 || value > 9)
            {
                Debug.LogError($"[ERROR] SudokuValidator::IsRowValid - Invalid value {value} at ({row}, {col})");
                return false;
            }

            if (used[value])
            {
                Debug.LogWarning($"[WARNING] SudokuValidator::IsRowValid - Duplicate {value} in row {row}");
                return false;
            }

            used[value] = true;
        }

        return true;
    }

    /// <summary>
    /// 특정 열이 유효한지 검증
    /// </summary>
    /// <param name="board">보드</param>
    /// <param name="col">열 번호</param>
    /// <returns>유효하면 true</returns>
    public static bool IsColumnValid(int[,] board, int col)
    {
        bool[] used = new bool[SIZE + 1]; // 1-9 사용 여부

        for (int row = 0; row < SIZE; row++)
        {
            int value = board[row, col];

            if (value == 0) continue; // 빈 칸은 무시

            if (value < 1 || value > 9)
            {
                Debug.LogError($"[ERROR] SudokuValidator::IsColumnValid - Invalid value {value} at ({row}, {col})");
                return false;
            }

            if (used[value])
            {
                Debug.LogWarning($"[WARNING] SudokuValidator::IsColumnValid - Duplicate {value} in column {col}");
                return false;
            }

            used[value] = true;
        }

        return true;
    }

    /// <summary>
    /// 특정 3x3 박스가 유효한지 검증
    /// </summary>
    /// <param name="board">보드</param>
    /// <param name="startRow">박스 시작 행 (0, 3, 6)</param>
    /// <param name="startCol">박스 시작 열 (0, 3, 6)</param>
    /// <returns>유효하면 true</returns>
    public static bool IsBoxValid(int[,] board, int startRow, int startCol)
    {
        bool[] used = new bool[SIZE + 1]; // 1-9 사용 여부

        for (int row = startRow; row < startRow + BOX_SIZE; row++)
        {
            for (int col = startCol; col < startCol + BOX_SIZE; col++)
            {
                int value = board[row, col];

                if (value == 0) continue; // 빈 칸은 무시

                if (value < 1 || value > 9)
                {
                    Debug.LogError($"[ERROR] SudokuValidator::IsBoxValid - Invalid value {value} at ({row}, {col})");
                    return false;
                }

                if (used[value])
                {
                    Debug.LogWarning($"[WARNING] SudokuValidator::IsBoxValid - Duplicate {value} in box ({startRow}, {startCol})");
                    return false;
                }

                used[value] = true;
            }
        }

        return true;
    }

    /// <summary>
    /// 특정 위치에 값을 놓을 수 있는지 검증
    /// </summary>
    /// <param name="board">보드</param>
    /// <param name="row">행</param>
    /// <param name="col">열</param>
    /// <param name="value">놓으려는 값 (1-9)</param>
    /// <returns>놓을 수 있으면 true</returns>
    public static bool CanPlaceValue(int[,] board, int row, int col, int value)
    {
        if (value < 1 || value > 9)
        {
            return false;
        }

        // 같은 행에 이미 있는지 확인
        for (int c = 0; c < SIZE; c++)
        {
            if (board[row, c] == value)
            {
                return false;
            }
        }

        // 같은 열에 이미 있는지 확인
        for (int r = 0; r < SIZE; r++)
        {
            if (board[r, col] == value)
            {
                return false;
            }
        }

        // 같은 3x3 박스에 이미 있는지 확인
        int boxRow = (row / BOX_SIZE) * BOX_SIZE;
        int boxCol = (col / BOX_SIZE) * BOX_SIZE;

        for (int r = boxRow; r < boxRow + BOX_SIZE; r++)
        {
            for (int c = boxCol; c < boxCol + BOX_SIZE; c++)
            {
                if (board[r, c] == value)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 보드 전체에서 에러가 있는 셀 찾기
    /// </summary>
    /// <param name="board">보드</param>
    /// <returns>에러 배열 (true = 에러)</returns>
    public static bool[,] FindErrors(int[,] board)
    {
        bool[,] errors = new bool[SIZE, SIZE];

        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                int value = board[row, col];

                if (value == 0) continue; // 빈 칸은 에러 아님

                // 임시로 값을 제거하고 놓을 수 있는지 확인
                board[row, col] = 0;
                bool canPlace = CanPlaceValue(board, row, col, value);
                board[row, col] = value;

                if (!canPlace)
                {
                    errors[row, col] = true;
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// 보드가 완성되었는지 확인
    /// </summary>
    /// <param name="board">보드</param>
    /// <returns>완성되었으면 true (모든 칸이 채워지고 유효함)</returns>
    public static bool IsSolved(int[,] board)
    {
        // 빈 칸이 있는지 확인
        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                if (board[row, col] == 0)
                {
                    return false;
                }
            }
        }

        // 모든 칸이 채워졌으면 규칙 검증
        return IsValid(board);
    }

    /// <summary>
    /// 보드가 정답과 일치하는지 확인
    /// </summary>
    /// <param name="board">플레이어 보드</param>
    /// <param name="solution">정답 보드</param>
    /// <returns>일치하면 true</returns>
    public static bool MatchesSolution(int[,] board, int[,] solution)
    {
        if (board == null || solution == null)
        {
            Debug.LogError("[ERROR] SudokuValidator::MatchesSolution - Board or solution is null");
            return false;
        }

        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                if (board[row, col] != solution[row, col])
                {
                    return false;
                }
            }
        }

        return true;
    }
}

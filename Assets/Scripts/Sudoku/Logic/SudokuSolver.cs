using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스도쿠 솔버 - 백트래킹 알고리즘
/// 퍼즐 해법 찾기 및 유일 해 검증
/// </summary>
public class SudokuSolver
{
    private const int SIZE = 9;

    /// <summary>
    /// 스도쿠 보드를 풀기 (백트래킹)
    /// </summary>
    /// <param name="board">풀 보드 (0 = 빈 칸)</param>
    /// <returns>풀 수 있으면 true</returns>
    public static bool Solve(int[,] board)
    {
        if (board == null || board.GetLength(0) != SIZE || board.GetLength(1) != SIZE)
        {
            Debug.LogError("[ERROR] SudokuSolver::Solve - Invalid board");
            return false;
        }

        return SolveBacktracking(board);
    }

    /// <summary>
    /// 백트래킹으로 보드 풀기
    /// </summary>
    /// <param name="board">보드</param>
    /// <returns>풀렸으면 true</returns>
    private static bool SolveBacktracking(int[,] board)
    {
        // 빈 칸 찾기
        int row = -1;
        int col = -1;
        bool foundEmpty = false;

        for (int r = 0; r < SIZE && !foundEmpty; r++)
        {
            for (int c = 0; c < SIZE && !foundEmpty; c++)
            {
                if (board[r, c] == 0)
                {
                    row = r;
                    col = c;
                    foundEmpty = true;
                }
            }
        }

        // 빈 칸이 없으면 완성
        if (!foundEmpty)
        {
            return true;
        }

        // 1-9 시도
        for (int num = 1; num <= 9; num++)
        {
            if (SudokuValidator.CanPlaceValue(board, row, col, num))
            {
                board[row, col] = num;

                if (SolveBacktracking(board))
                {
                    return true;
                }

                // 백트래킹
                board[row, col] = 0;
            }
        }

        return false;
    }

    /// <summary>
    /// 해의 개수 세기 (유일 해 검증용)
    /// </summary>
    /// <param name="board">보드</param>
    /// <param name="limit">최대 카운트 (효율성)</param>
    /// <returns>해의 개수 (limit 도달 시 중단)</returns>
    public static int CountSolutions(int[,] board, int limit = 2)
    {
        if (board == null || board.GetLength(0) != SIZE || board.GetLength(1) != SIZE)
        {
            Debug.LogError("[ERROR] SudokuSolver::CountSolutions - Invalid board");
            return 0;
        }

        // 보드 복사
        int[,] boardCopy = new int[SIZE, SIZE];
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                boardCopy[r, c] = board[r, c];
            }
        }

        int count = 0;
        CountSolutionsRecursive(boardCopy, ref count, limit);
        return count;
    }

    /// <summary>
    /// 재귀적으로 해의 개수 세기
    /// </summary>
    /// <param name="board">보드</param>
    /// <param name="count">현재 카운트 (참조)</param>
    /// <param name="limit">최대 카운트</param>
    private static void CountSolutionsRecursive(int[,] board, ref int count, int limit)
    {
        // 이미 limit에 도달했으면 중단
        if (count >= limit)
        {
            return;
        }

        // 빈 칸 찾기
        int row = -1;
        int col = -1;
        bool foundEmpty = false;

        for (int r = 0; r < SIZE && !foundEmpty; r++)
        {
            for (int c = 0; c < SIZE && !foundEmpty; c++)
            {
                if (board[r, c] == 0)
                {
                    row = r;
                    col = c;
                    foundEmpty = true;
                }
            }
        }

        // 빈 칸이 없으면 해를 찾음
        if (!foundEmpty)
        {
            count++;
            return;
        }

        // 1-9 시도
        for (int num = 1; num <= 9; num++)
        {
            if (SudokuValidator.CanPlaceValue(board, row, col, num))
            {
                board[row, col] = num;
                CountSolutionsRecursive(board, ref count, limit);
                board[row, col] = 0;

                // 이미 limit에 도달했으면 중단
                if (count >= limit)
                {
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 유일 해인지 검증
    /// </summary>
    /// <param name="board">보드</param>
    /// <returns>유일 해이면 true</returns>
    public static bool HasUniqueSolution(int[,] board)
    {
        int solutionCount = CountSolutions(board, 2);
        return solutionCount == 1;
    }

    /// <summary>
    /// 특정 셀의 힌트 가져오기 (가능한 값들)
    /// </summary>
    /// <param name="board">보드</param>
    /// <param name="row">행</param>
    /// <param name="col">열</param>
    /// <returns>가능한 값 리스트</returns>
    public static List<int> GetPossibleValues(int[,] board, int row, int col)
    {
        List<int> possibleValues = new List<int>();

        if (board[row, col] != 0)
        {
            return possibleValues; // 이미 채워진 칸은 빈 리스트 반환
        }

        for (int num = 1; num <= 9; num++)
        {
            if (SudokuValidator.CanPlaceValue(board, row, col, num))
            {
                possibleValues.Add(num);
            }
        }

        return possibleValues;
    }

    /// <summary>
    /// 가장 쉬운 빈 칸 찾기 (가능한 값이 적은 칸)
    /// </summary>
    /// <param name="board">보드</param>
    /// <param name="row">찾은 행 (출력)</param>
    /// <param name="col">찾은 열 (출력)</param>
    /// <returns>빈 칸을 찾았으면 true</returns>
    public static bool FindEasiestEmptyCell(int[,] board, out int row, out int col)
    {
        row = -1;
        col = -1;
        int minPossibilities = 10;

        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                if (board[r, c] == 0)
                {
                    int possibilities = GetPossibleValues(board, r, c).Count;

                    if (possibilities < minPossibilities)
                    {
                        minPossibilities = possibilities;
                        row = r;
                        col = c;
                    }
                }
            }
        }

        return row != -1 && col != -1;
    }

    /// <summary>
    /// 힌트를 위한 최선의 다음 수 찾기
    /// </summary>
    /// <param name="board">현재 보드</param>
    /// <param name="solution">정답 보드</param>
    /// <param name="row">찾은 행 (출력)</param>
    /// <param name="col">찾은 열 (출력)</param>
    /// <param name="value">찾은 값 (출력)</param>
    /// <returns>찾았으면 true</returns>
    public static bool GetBestHint(int[,] board, int[,] solution, out int row, out int col, out int value)
    {
        row = -1;
        col = -1;
        value = 0;

        // 가장 쉬운 빈 칸 찾기 (가능한 값이 적은 칸)
        if (FindEasiestEmptyCell(board, out row, out col))
        {
            value = solution[row, col];
            return true;
        }

        return false;
    }

    /// <summary>
    /// 보드 유효성 및 해결 가능성 검증
    /// </summary>
    /// <param name="board">보드</param>
    /// <returns>유효하고 풀 수 있으면 true</returns>
    public static bool IsSolvable(int[,] board)
    {
        if (!SudokuValidator.IsValid(board))
        {
            return false;
        }

        // 보드 복사하여 풀기 시도
        int[,] boardCopy = new int[SIZE, SIZE];
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                boardCopy[r, c] = board[r, c];
            }
        }

        return Solve(boardCopy);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스도쿠 퍼즐 생성기
/// PRD 요구사항: 실시간 시드 기반 랜덤 생성 + 유일 해 보장
/// </summary>
public class SudokuGenerator
{
    private const int SIZE = 9;
    private const int BOX_SIZE = 3;

    private System.Random _random;

    /// <summary>
    /// 새 퍼즐 생성
    /// </summary>
    /// <param name="difficulty">난이도</param>
    /// <param name="hintCount">힌트 개수 (0이면 난이도에 따라 자동 결정)</param>
    /// <returns>생성된 퍼즐 (board, solution, hints)</returns>
    public PuzzleResult GeneratePuzzle(SudokuDifficulty difficulty, int hintCount = 0)
    {
        // PRD 요구사항: 실시간 기반 랜덤 시드
        int seed = (int)DateTime.Now.Ticks;
        _random = new System.Random(seed);

        Debug.Log($"[INFO] SudokuGenerator::GeneratePuzzle - Generating {difficulty} puzzle with seed: {seed}");

        // 1. 완전한 보드 생성
        int[,] solution = GenerateCompletedBoard();

        if (solution == null)
        {
            Debug.LogError("[ERROR] SudokuGenerator::GeneratePuzzle - Failed to generate completed board");
            return null;
        }

        // 2. 난이도에 따라 구멍 뚫기
        if (hintCount == 0)
        {
            hintCount = GetDefaultHintCount(difficulty);
        }

        var (puzzle, hints) = CreatePuzzle(solution, hintCount);

        // 3. 유일 해 검증 (PRD 필수 요구사항)
        if (!SudokuSolver.HasUniqueSolution(puzzle))
        {
            Debug.LogWarning("[WARNING] SudokuGenerator::GeneratePuzzle - Puzzle does not have unique solution, retrying...");
            // 재귀적으로 재시도 (최대 3회)
            return GeneratePuzzleWithRetry(difficulty, hintCount, 3);
        }

        Debug.Log($"[INFO] SudokuGenerator::GeneratePuzzle - Successfully generated puzzle with {hintCount} hints");

        return new PuzzleResult
        {
            Board = puzzle,
            Solution = solution,
            Hints = hints,
            Difficulty = difficulty,
            HintCount = hintCount,
            Seed = seed
        };
    }

    /// <summary>
    /// 재시도를 포함한 퍼즐 생성
    /// </summary>
    private PuzzleResult GeneratePuzzleWithRetry(SudokuDifficulty difficulty, int hintCount, int maxRetries)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            var result = GeneratePuzzle(difficulty, hintCount);
            if (result != null)
            {
                return result;
            }

            Debug.LogWarning($"[WARNING] SudokuGenerator::GeneratePuzzleWithRetry - Retry {i + 1}/{maxRetries}");
        }

        Debug.LogError("[ERROR] SudokuGenerator::GeneratePuzzleWithRetry - Failed to generate valid puzzle after retries");
        return null;
    }

    /// <summary>
    /// 완전한 스도쿠 보드 생성 (백트래킹)
    /// </summary>
    /// <returns>완성된 보드</returns>
    private int[,] GenerateCompletedBoard()
    {
        int[,] board = new int[SIZE, SIZE];

        // 빈 보드에서 시작
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                board[r, c] = 0;
            }
        }

        // 백트래킹으로 랜덤 완성 보드 생성
        if (FillBoardRandomly(board))
        {
            return board;
        }

        Debug.LogError("[ERROR] SudokuGenerator::GenerateCompletedBoard - Failed to fill board");
        return null;
    }

    /// <summary>
    /// 백트래킹으로 보드를 랜덤하게 채우기
    /// </summary>
    /// <param name="board">보드</param>
    /// <returns>성공하면 true</returns>
    private bool FillBoardRandomly(int[,] board)
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

        // 1-9를 랜덤 순서로 시도
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Shuffle(numbers);

        foreach (int num in numbers)
        {
            if (SudokuValidator.CanPlaceValue(board, row, col, num))
            {
                board[row, col] = num;

                if (FillBoardRandomly(board))
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
    /// 완성된 보드에서 퍼즐 생성 (구멍 뚫기)
    /// </summary>
    /// <param name="solution">완성된 보드</param>
    /// <param name="hintCount">남길 힌트 개수</param>
    /// <returns>(퍼즐 보드, 힌트 배열)</returns>
    private (int[,] puzzle, bool[,] hints) CreatePuzzle(int[,] solution, int hintCount)
    {
        int[,] puzzle = new int[SIZE, SIZE];
        bool[,] hints = new bool[SIZE, SIZE];

        // 솔루션 복사
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                puzzle[r, c] = solution[r, c];
                hints[r, c] = false;
            }
        }

        // 모든 위치를 리스트로 만들기
        List<(int row, int col)> positions = new List<(int, int)>();
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                positions.Add((r, c));
            }
        }

        // 랜덤하게 섞기
        Shuffle(positions);

        // hintCount개만 남기고 나머지 제거
        int cellsToRemove = SIZE * SIZE - hintCount;
        int removedCount = 0;

        foreach (var (row, col) in positions)
        {
            if (removedCount >= cellsToRemove)
            {
                break;
            }

            int temp = puzzle[row, col];
            puzzle[row, col] = 0;

            // 유일 해를 유지하는지 확인 (선택적 검증)
            // 이 검증은 시간이 걸리므로, 최종 검증은 GeneratePuzzle에서 수행
            removedCount++;
        }

        // 힌트 배열 생성 (0이 아닌 칸 = 힌트)
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                hints[r, c] = (puzzle[r, c] != 0);
            }
        }

        Debug.Log($"[INFO] SudokuGenerator::CreatePuzzle - Created puzzle with {hintCount} hints ({removedCount} cells removed)");

        return (puzzle, hints);
    }

    /// <summary>
    /// 난이도에 따른 기본 힌트 개수
    /// </summary>
    /// <param name="difficulty">난이도</param>
    /// <returns>힌트 개수</returns>
    private int GetDefaultHintCount(SudokuDifficulty difficulty)
    {
        switch (difficulty)
        {
            case SudokuDifficulty.Easy:
                return _random.Next(36, 41); // 36-40
            case SudokuDifficulty.Medium:
                return _random.Next(30, 35); // 30-34
            case SudokuDifficulty.Hard:
                return _random.Next(24, 28); // 24-27
            default:
                return 30;
        }
    }

    /// <summary>
    /// 리스트 섞기 (Fisher-Yates 알고리즘)
    /// </summary>
    /// <typeparam name="T">리스트 타입</typeparam>
    /// <param name="list">섞을 리스트</param>
    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = _random.Next(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    /// <summary>
    /// 퍼즐 생성 결과
    /// </summary>
    public class PuzzleResult
    {
        public int[,] Board;            // 퍼즐 보드 (0 = 빈 칸)
        public int[,] Solution;         // 정답 보드
        public bool[,] Hints;           // 힌트 배열 (true = 초기 힌트)
        public SudokuDifficulty Difficulty;
        public int HintCount;
        public int Seed;                // 생성 시드 (재현 가능)

        public override string ToString()
        {
            return $"Sudoku Puzzle (Difficulty: {Difficulty}, Hints: {HintCount}, Seed: {Seed})";
        }
    }
}

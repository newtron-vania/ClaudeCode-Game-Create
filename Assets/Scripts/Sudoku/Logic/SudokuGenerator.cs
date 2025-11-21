using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 스도쿠 퍼즐 생성기 (최적화 버전)
///
/// 주요 최적화:
/// 1. 대각선 박스 우선 채우기 (성능 향상)
/// 2. 비트마스크 기반 후보 관리 (메모리 효율)
/// 3. Human Solver 기반 난이도 측정 (논리 기술 요구도)
/// 4. 유일 해 보장 구멍 뚫기 (품질 보장)
/// </summary>
public class SudokuGenerator
{
    private const int SIZE = 9;
    private const int BOX_SIZE = 3;
    private const int ALL_CANDIDATES = 0x1FF; // 1-9 모든 비트 (0001_1111_1111)

    private System.Random _random;

    /// <summary>
    /// 새 퍼즐 비동기 생성 (추천)
    /// </summary>
    public async Task<PuzzleResult> GeneratePuzzleAsync(SudokuDifficulty difficulty, int hintCount = 0)
    {
        Stopwatch timer = Stopwatch.StartNew();
        UnityEngine.Debug.Log($"[INFO] SudokuGenerator::GeneratePuzzleAsync - Starting async puzzle generation for {difficulty}");

        // 백그라운드 스레드에서 퍼즐 생성 실행
        PuzzleResult result = await Task.Run(() => GeneratePuzzle(difficulty, hintCount));

        timer.Stop();
        UnityEngine.Debug.Log($"[INFO] SudokuGenerator::GeneratePuzzleAsync - Completed in {timer.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 새 퍼즐 생성 (동기)
    /// </summary>
    public PuzzleResult GeneratePuzzle(SudokuDifficulty difficulty, int hintCount = 0)
    {
        Stopwatch timer = Stopwatch.StartNew();

        // 실시간 기반 랜덤 시드
        int seed = (int)DateTime.Now.Ticks;
        _random = new System.Random(seed);

        UnityEngine.Debug.Log($"[INFO] SudokuGenerator::GeneratePuzzle - Generating {difficulty} puzzle (seed: {seed})");

        // 1. 완전한 보드 생성 (대각선 박스 최적화 적용)
        int[,] solution = GenerateCompletedBoard();
        if (solution == null)
        {
            UnityEngine.Debug.LogError("[ERROR] SudokuGenerator::GeneratePuzzle - Failed to generate completed board");
            return null;
        }

        // 2. 난이도에 따라 구멍 뚫기 (유일 해 보장)
        if (hintCount == 0)
        {
            hintCount = GetTargetHintCount(difficulty);
        }

        var (puzzle, hints) = CreatePuzzleWithUniqueness(solution, hintCount);

        // 3. Human Solver로 실제 난이도 측정
        SudokuDifficulty measuredDifficulty = EvaluateDifficulty(puzzle);

        timer.Stop();
        UnityEngine.Debug.Log($"[INFO] SudokuGenerator::GeneratePuzzle - Generated in {timer.ElapsedMilliseconds}ms " +
                             $"(Target: {difficulty}, Measured: {measuredDifficulty}, Hints: {hintCount})");

        return new PuzzleResult
        {
            Board = puzzle,
            Solution = solution,
            Hints = hints,
            Difficulty = difficulty,
            MeasuredDifficulty = measuredDifficulty,
            HintCount = hintCount,
            Seed = seed
        };
    }

    // ============================================================
    // Step 1: 완전한 보드 생성 (대각선 박스 최적화)
    // ============================================================

    /// <summary>
    /// 완전한 스도쿠 보드 생성
    /// 최적화: (0,0), (3,3), (6,6) 박스를 먼저 채워 성능 향상
    /// </summary>
    private int[,] GenerateCompletedBoard()
    {
        int[,] board = new int[SIZE, SIZE];

        // 대각선 3개 박스는 서로 독립적이므로 먼저 랜덤으로 채움
        FillDiagonalBoxes(board);

        // 나머지 빈 칸을 백트래킹으로 채움
        if (!SolveBoard(board))
        {
            UnityEngine.Debug.LogError("[ERROR] SudokuGenerator::GenerateCompletedBoard - Failed to fill board");
            return null;
        }

        return board;
    }

    /// <summary>
    /// 대각선 박스 3개 ((0,0), (3,3), (6,6)) 우선 채우기
    /// 이 박스들은 서로 영향을 주지 않으므로 독립적으로 채울 수 있음
    /// </summary>
    private void FillDiagonalBoxes(int[,] board)
    {
        for (int i = 0; i < SIZE; i += BOX_SIZE)
        {
            FillBox(board, i, i);
        }
    }

    /// <summary>
    /// 3x3 박스 내부를 1-9로 랜덤하게 채우기
    /// </summary>
    private void FillBox(int[,] board, int rowStart, int colStart)
    {
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Shuffle(numbers);

        int index = 0;
        for (int i = 0; i < BOX_SIZE; i++)
        {
            for (int j = 0; j < BOX_SIZE; j++)
            {
                board[rowStart + i, colStart + j] = numbers[index++];
            }
        }
    }

    /// <summary>
    /// 백트래킹으로 보드 완성
    /// </summary>
    private bool SolveBoard(int[,] board)
    {
        // 빈 칸 찾기
        int row = -1, col = -1;
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
        if (!foundEmpty) return true;

        // 1-9를 랜덤 순서로 시도
        List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        Shuffle(numbers);

        foreach (int num in numbers)
        {
            if (IsSafe(board, row, col, num))
            {
                board[row, col] = num;

                if (SolveBoard(board))
                {
                    return true;
                }

                // 백트래킹
                board[row, col] = 0;
            }
        }

        return false;
    }

    // ============================================================
    // Step 2: 구멍 뚫기 (유일 해 보장)
    // ============================================================

    /// <summary>
    /// 완성된 보드에서 유일 해를 보장하며 구멍 뚫기
    /// </summary>
    private (int[,] puzzle, bool[,] hints) CreatePuzzleWithUniqueness(int[,] solution, int targetHintCount)
    {
        int[,] puzzle = (int[,])solution.Clone();
        bool[,] hints = new bool[SIZE, SIZE];

        // 모든 위치를 랜덤하게 섞기
        List<(int row, int col)> positions = new List<(int, int)>();
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                positions.Add((r, c));
            }
        }
        Shuffle(positions);

        int currentHints = SIZE * SIZE;
        int cellsToRemove = SIZE * SIZE - targetHintCount;
        int removedCount = 0;

        // 각 위치를 시도하며 구멍 뚫기
        foreach (var (row, col) in positions)
        {
            if (removedCount >= cellsToRemove)
            {
                break;
            }

            int temp = puzzle[row, col];
            puzzle[row, col] = 0;

            // 유일한 해인지 검증
            if (!HasUniqueSolution(puzzle))
            {
                // 해가 2개 이상이면 복구
                puzzle[row, col] = temp;
            }
            else
            {
                removedCount++;
                currentHints--;
            }
        }

        // 힌트 배열 생성
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                hints[r, c] = (puzzle[r, c] != 0);
            }
        }

        UnityEngine.Debug.Log($"[INFO] SudokuGenerator::CreatePuzzleWithUniqueness - Created puzzle with {currentHints} hints ({removedCount} cells removed)");

        return (puzzle, hints);
    }

    /// <summary>
    /// 유일한 해를 가지는지 확인
    /// </summary>
    private bool HasUniqueSolution(int[,] board)
    {
        int[,] clone = (int[,])board.Clone();
        int solutionCount = 0;
        SolveAndCount(clone, ref solutionCount);
        return solutionCount == 1;
    }

    /// <summary>
    /// 해의 개수를 세기 (최대 2개까지만 확인)
    /// </summary>
    private void SolveAndCount(int[,] board, ref int count)
    {
        // 가지치기: 해가 2개 이상이면 즉시 중단
        if (count > 1) return;

        // 빈 칸 찾기
        int row = -1, col = -1;
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
            if (IsSafe(board, row, col, num))
            {
                board[row, col] = num;
                SolveAndCount(board, ref count);
                board[row, col] = 0;
            }
        }
    }

    // ============================================================
    // Step 3: 난이도 측정 (Human Solver 시뮬레이션)
    // ============================================================

    /// <summary>
    /// Human Solver로 난이도 측정
    /// 어떤 논리 기술이 필요한지에 따라 난이도 결정
    /// </summary>
    private SudokuDifficulty EvaluateDifficulty(int[,] board)
    {
        int[,] workBoard = (int[,])board.Clone();
        int[,] candidates = InitCandidates(workBoard);

        SudokuDifficulty currentDifficulty = SudokuDifficulty.Easy;
        bool changed = true;

        while (changed && !IsFull(workBoard))
        {
            changed = false;

            // Lv 1. Naked Single (후보가 1개인 칸 찾기)
            if (ApplyNakedSingle(workBoard, candidates))
            {
                changed = true;
                continue;
            }

            // Lv 2. Hidden Single (특정 줄에서 유일한 위치 찾기)
            if (ApplyHiddenSingle(workBoard, candidates))
            {
                currentDifficulty = MaxDifficulty(currentDifficulty, SudokuDifficulty.Medium);
                changed = true;
                continue;
            }

            // 더 이상 논리로 풀 수 없음 (고급 기술 또는 추측 필요)
            if (!IsFull(workBoard))
            {
                return SudokuDifficulty.Hard;
            }
        }

        return currentDifficulty;
    }

    /// <summary>
    /// 후보 비트마스크 초기화
    /// 각 칸의 가능한 숫자를 비트로 표현 (1-9 = bit 0-8)
    /// </summary>
    private int[,] InitCandidates(int[,] board)
    {
        int[,] candidates = new int[SIZE, SIZE];

        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                if (board[r, c] != 0)
                {
                    candidates[r, c] = 0; // 이미 채워진 칸
                }
                else
                {
                    int mask = ALL_CANDIDATES; // 1-9 모두 가능
                    for (int num = 1; num <= 9; num++)
                    {
                        if (!IsSafe(board, r, c, num))
                        {
                            // num이 불가능하면 해당 비트 끄기
                            mask &= ~(1 << (num - 1));
                        }
                    }
                    candidates[r, c] = mask;
                }
            }
        }

        return candidates;
    }

    /// <summary>
    /// Naked Single 기술 적용
    /// 후보가 딱 1개인 칸을 찾아 채우기
    /// </summary>
    private bool ApplyNakedSingle(int[,] board, int[,] candidates)
    {
        bool changed = false;

        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                if (board[r, c] == 0 && CountSetBits(candidates[r, c]) == 1)
                {
                    // 후보가 1개뿐인 칸 발견
                    int value = GetFirstSetBit(candidates[r, c]);
                    ConfirmCell(board, candidates, r, c, value);
                    changed = true;
                }
            }
        }

        return changed;
    }

    /// <summary>
    /// Hidden Single 기술 적용
    /// 특정 행/열/박스에서 숫자가 들어갈 수 있는 칸이 1개뿐인 경우 찾기
    /// </summary>
    private bool ApplyHiddenSingle(int[,] board, int[,] candidates)
    {
        bool changed = false;

        // 행 검사
        for (int r = 0; r < SIZE; r++)
        {
            for (int num = 1; num <= 9; num++)
            {
                int mask = 1 << (num - 1);
                int count = 0;
                int targetCol = -1;

                for (int c = 0; c < SIZE; c++)
                {
                    if (board[r, c] == 0 && (candidates[r, c] & mask) != 0)
                    {
                        count++;
                        targetCol = c;
                    }
                }

                if (count == 1)
                {
                    ConfirmCell(board, candidates, r, targetCol, num);
                    changed = true;
                }
            }
        }

        // 열 검사
        for (int c = 0; c < SIZE; c++)
        {
            for (int num = 1; num <= 9; num++)
            {
                int mask = 1 << (num - 1);
                int count = 0;
                int targetRow = -1;

                for (int r = 0; r < SIZE; r++)
                {
                    if (board[r, c] == 0 && (candidates[r, c] & mask) != 0)
                    {
                        count++;
                        targetRow = r;
                    }
                }

                if (count == 1)
                {
                    ConfirmCell(board, candidates, targetRow, c, num);
                    changed = true;
                }
            }
        }

        // 박스 검사
        for (int boxRow = 0; boxRow < SIZE; boxRow += BOX_SIZE)
        {
            for (int boxCol = 0; boxCol < SIZE; boxCol += BOX_SIZE)
            {
                for (int num = 1; num <= 9; num++)
                {
                    int mask = 1 << (num - 1);
                    int count = 0;
                    int targetR = -1, targetC = -1;

                    for (int r = boxRow; r < boxRow + BOX_SIZE; r++)
                    {
                        for (int c = boxCol; c < boxCol + BOX_SIZE; c++)
                        {
                            if (board[r, c] == 0 && (candidates[r, c] & mask) != 0)
                            {
                                count++;
                                targetR = r;
                                targetC = c;
                            }
                        }
                    }

                    if (count == 1)
                    {
                        ConfirmCell(board, candidates, targetR, targetC, num);
                        changed = true;
                    }
                }
            }
        }

        return changed;
    }

    /// <summary>
    /// 칸에 값을 확정하고 후보 전파
    /// </summary>
    private void ConfirmCell(int[,] board, int[,] candidates, int r, int c, int value)
    {
        board[r, c] = value;
        candidates[r, c] = 0;

        int mask = ~(1 << (value - 1)); // 해당 숫자 비트 끄기

        // 같은 행/열의 후보에서 제거
        for (int k = 0; k < SIZE; k++)
        {
            candidates[r, k] &= mask;
            candidates[k, c] &= mask;
        }

        // 같은 박스의 후보에서 제거
        int boxRow = r - r % BOX_SIZE;
        int boxCol = c - c % BOX_SIZE;
        for (int i = 0; i < BOX_SIZE; i++)
        {
            for (int j = 0; j < BOX_SIZE; j++)
            {
                candidates[boxRow + i, boxCol + j] &= mask;
            }
        }
    }

    // ============================================================
    // 헬퍼 메서드
    // ============================================================

    /// <summary>
    /// 행, 열, 박스에서 안전한지 확인
    /// </summary>
    private bool IsSafe(int[,] board, int row, int col, int num)
    {
        return IsSafeInRow(board, row, num) &&
               IsSafeInCol(board, col, num) &&
               IsSafeInBox(board, row - row % BOX_SIZE, col - col % BOX_SIZE, num);
    }

    private bool IsSafeInRow(int[,] board, int row, int num)
    {
        for (int c = 0; c < SIZE; c++)
        {
            if (board[row, c] == num) return false;
        }
        return true;
    }

    private bool IsSafeInCol(int[,] board, int col, int num)
    {
        for (int r = 0; r < SIZE; r++)
        {
            if (board[r, col] == num) return false;
        }
        return true;
    }

    private bool IsSafeInBox(int[,] board, int startRow, int startCol, int num)
    {
        for (int i = 0; i < BOX_SIZE; i++)
        {
            for (int j = 0; j < BOX_SIZE; j++)
            {
                if (board[startRow + i, startCol + j] == num) return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 보드가 완전히 채워졌는지 확인
    /// </summary>
    private bool IsFull(int[,] board)
    {
        foreach (int val in board)
        {
            if (val == 0) return false;
        }
        return true;
    }

    /// <summary>
    /// 비트마스크에서 켜진 비트 개수 세기
    /// </summary>
    private int CountSetBits(int mask)
    {
        int count = 0;
        while (mask > 0)
        {
            mask &= (mask - 1); // 가장 오른쪽 1 비트 제거
            count++;
        }
        return count;
    }

    /// <summary>
    /// 비트마스크에서 첫 번째로 켜진 비트의 값 가져오기 (1-9)
    /// </summary>
    private int GetFirstSetBit(int mask)
    {
        for (int num = 1; num <= 9; num++)
        {
            if ((mask & (1 << (num - 1))) != 0)
            {
                return num;
            }
        }
        return 0;
    }

    /// <summary>
    /// 두 난이도 중 더 높은 것 반환
    /// </summary>
    private SudokuDifficulty MaxDifficulty(SudokuDifficulty a, SudokuDifficulty b)
    {
        return a > b ? a : b;
    }

    /// <summary>
    /// 난이도별 목표 힌트 개수
    /// </summary>
    private int GetTargetHintCount(SudokuDifficulty difficulty)
    {
        switch (difficulty)
        {
            case SudokuDifficulty.Easy:
                return _random.Next(36, 41);   // 36-40 힌트
            case SudokuDifficulty.Medium:
                return _random.Next(32, 36);   // 32-35 힌트
            case SudokuDifficulty.Hard:
                return _random.Next(26, 30);   // 26-29 힌트
            default:
                return 30;
        }
    }

    /// <summary>
    /// Fisher-Yates 셔플 알고리즘
    /// </summary>
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

    // ============================================================
    // 퍼즐 생성 결과
    // ============================================================

    /// <summary>
    /// 퍼즐 생성 결과 데이터
    /// </summary>
    public class PuzzleResult
    {
        public int[,] Board;                        // 퍼즐 보드 (0 = 빈 칸)
        public int[,] Solution;                     // 정답 보드
        public bool[,] Hints;                       // 힌트 배열 (true = 초기 힌트)
        public SudokuDifficulty Difficulty;         // 목표 난이도
        public SudokuDifficulty MeasuredDifficulty; // Human Solver로 측정된 실제 난이도
        public int HintCount;                       // 힌트 개수
        public int Seed;                            // 생성 시드 (재현 가능)

        public override string ToString()
        {
            return $"Sudoku Puzzle (Target: {Difficulty}, Measured: {MeasuredDifficulty}, Hints: {HintCount}, Seed: {Seed})";
        }
    }
}

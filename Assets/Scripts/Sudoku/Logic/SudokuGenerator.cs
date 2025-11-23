using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

// =========================================================
// 난이도 등급
// =========================================================
public enum Difficulty { Easy, Medium, Hard }

// =========================================================
// 기술 종류 (난이도 판별용)
// =========================================================
public enum TechType
{
    None,
    NakedSingle,
    HiddenSingle,
    Pair,         // Naked Pair + Hidden Pair 통합
    Intersection, // Pointing + Claiming 통합
    XWing
}

// =========================================================
// 생성된 스도쿠 결과 데이터
// =========================================================
public class SudokuData
{
    public int[,] Board;                // 플레이용 보드 (0: 빈칸)
    public int[,] SolvedBoard;          // 정답 보드
    public Difficulty Diff;             // 설정된 난이도
    public List<TechType> UsedTechs;    // 풀이에 사용된 기술 목록
    public int Hints { get; set; } = 5; // 난이도 무관 힌트 5개 고정

    public override string ToString()
    {
        return $"Sudoku (Difficulty: {Diff}, UsedTechs: {string.Join(",", UsedTechs)}, Hints: {Hints})";
    }
}

// =========================================================
// 1. 공통 헬퍼 함수 (Common Helpers)
// =========================================================
/// <summary>
/// 스도쿠 공통 유틸리티 함수 모음
/// 비트마스크 연산, 후보 관리, 안전성 검사 등
/// </summary>
public static class SudokuUtils
{
    private const int SIZE = 9;
    private const int BOX_SIZE = 3;
    private const int ALL_CANDIDATES = 0x1FF; // 1-9 모든 비트 (0001_1111_1111)

    /// <summary>
    /// 켜진 비트 개수 반환 (후보 숫자 개수)
    /// </summary>
    public static int CountBits(int n)
    {
        int count = 0;
        while (n > 0) { n &= (n - 1); count++; }
        return count;
    }

    /// <summary>
    /// 비트마스크에서 유일한 숫자 값 추출 (예: 00100 -> 3)
    /// </summary>
    public static int GetSingleValue(int mask)
    {
        for (int k = 1; k <= 9; k++)
            if ((mask & (1 << (k - 1))) != 0) return k;
        return 0;
    }

    /// <summary>
    /// 숫자 확정 및 후보 제거 (Propagation)
    /// 행, 열, 박스의 모든 후보에서 확정된 숫자 제거
    /// </summary>
    public static void ConfirmCell(int[,] board, int[,] candidates, int r, int c, int val)
    {
        board[r, c] = val;
        candidates[r, c] = 0; // 확정된 칸 후보 삭제
        int mask = ~(1 << (val - 1)); // 제거할 마스크 (NOT)

        // 행, 열 전파
        for (int k = 0; k < SIZE; k++)
        {
            candidates[r, k] &= mask;
            candidates[k, c] &= mask;
        }

        // 박스 전파
        int startRow = (r / BOX_SIZE) * BOX_SIZE;
        int startCol = (c / BOX_SIZE) * BOX_SIZE;
        for (int i = 0; i < BOX_SIZE; i++)
            for (int j = 0; j < BOX_SIZE; j++)
                candidates[startRow + i, startCol + j] &= mask;
    }

    /// <summary>
    /// 후보 숫자 배열 초기화
    /// 각 빈 칸에 대해 가능한 숫자를 비트마스크로 표현
    /// </summary>
    public static int[,] InitCandidates(int[,] board)
    {
        int[,] candidates = new int[SIZE, SIZE];
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                if (board[r, c] != 0) candidates[r, c] = 0;
                else
                {
                    int mask = ALL_CANDIDATES; // 1~9 비트 ON
                    for (int k = 1; k <= 9; k++)
                        if (!IsSafe(board, r, c, k)) mask &= ~(1 << (k - 1));
                    candidates[r, c] = mask;
                }
            }
        }
        return candidates;
    }

    /// <summary>
    /// 안전성 검사 (행, 열, 박스에서 중복 여부)
    /// </summary>
    public static bool IsSafe(int[,] board, int row, int col, int num)
    {
        // 행 검사
        for (int i = 0; i < SIZE; i++)
            if (board[row, i] == num) return false;

        // 열 검사
        for (int i = 0; i < SIZE; i++)
            if (board[i, col] == num) return false;

        // 박스 검사
        int startRow = (row / BOX_SIZE) * BOX_SIZE;
        int startCol = (col / BOX_SIZE) * BOX_SIZE;
        for (int i = 0; i < BOX_SIZE; i++)
            for (int j = 0; j < BOX_SIZE; j++)
                if (board[startRow + i, startCol + j] == num) return false;

        return true;
    }

    /// <summary>
    /// 보드가 완전히 채워졌는지 확인
    /// </summary>
    public static bool IsFull(int[,] board)
    {
        foreach (int val in board) if (val == 0) return false;
        return true;
    }
}

// =========================================================
// 2. 난이도 판단 함수 (Technique Algorithms)
// =========================================================
/// <summary>
/// 스도쿠 풀이 기술 알고리즘 모음
/// Naked Single, Hidden Single, Pairs, Intersection, X-Wing
/// </summary>
public static class SudokuTechs
{
    private const int SIZE = 9;
    private const int BOX_SIZE = 3;

    /// <summary>
    /// [Lv 1] Naked Single
    /// 후보가 딱 1개인 칸을 찾아 채우기
    /// </summary>
    public static bool ApplyNakedSingle(int[,] board, int[,] candidates)
    {
        bool changed = false;
        for (int r = 0; r < SIZE; r++)
        {
            for (int c = 0; c < SIZE; c++)
            {
                if (board[r, c] == 0 && SudokuUtils.CountBits(candidates[r, c]) == 1)
                {
                    int val = SudokuUtils.GetSingleValue(candidates[r, c]);
                    SudokuUtils.ConfirmCell(board, candidates, r, c, val);
                    changed = true;
                }
            }
        }
        return changed;
    }

    /// <summary>
    /// [Lv 2] Hidden Single
    /// 특정 행/열/박스에서 숫자가 들어갈 수 있는 칸이 1개뿐인 경우 찾기
    /// </summary>
    public static bool ApplyHiddenSingle(int[,] board, int[,] candidates)
    {
        bool changed = false;

        // 행 검사
        for (int r = 0; r < SIZE; r++)
        {
            for (int num = 1; num <= 9; num++)
            {
                int mask = 1 << (num - 1);
                int count = 0, targetC = -1;
                for (int c = 0; c < SIZE; c++)
                {
                    if (board[r, c] == 0 && (candidates[r, c] & mask) != 0)
                    {
                        count++; targetC = c;
                    }
                }
                if (count == 1)
                {
                    SudokuUtils.ConfirmCell(board, candidates, r, targetC, num);
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
                int count = 0, targetR = -1;
                for (int r = 0; r < SIZE; r++)
                {
                    if (board[r, c] == 0 && (candidates[r, c] & mask) != 0)
                    {
                        count++; targetR = r;
                    }
                }
                if (count == 1)
                {
                    SudokuUtils.ConfirmCell(board, candidates, targetR, c, num);
                    changed = true;
                }
            }
        }

        // 박스 검사
        for (int b = 0; b < SIZE; b++)
        {
            int startRow = (b / BOX_SIZE) * BOX_SIZE;
            int startCol = (b % BOX_SIZE) * BOX_SIZE;

            for (int num = 1; num <= 9; num++)
            {
                int mask = 1 << (num - 1);
                int count = 0, targetR = -1, targetC = -1;

                for (int i = 0; i < BOX_SIZE; i++)
                {
                    for (int j = 0; j < BOX_SIZE; j++)
                    {
                        int r = startRow + i;
                        int c = startCol + j;
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
                    SudokuUtils.ConfirmCell(board, candidates, targetR, targetC, num);
                    changed = true;
                }
            }
        }

        return changed;
    }

    /// <summary>
    /// [Lv 3] Pair (Naked Pair)
    /// 같은 2개 후보를 가진 두 칸을 찾아, 다른 칸에서 해당 후보 제거
    /// </summary>
    public static bool ApplyPairs(int[,] board, int[,] candidates)
    {
        bool changed = false;

        // 행 검사
        for (int r = 0; r < SIZE; r++)
        {
            var cells = new List<(int c, int mask)>();
            for (int c = 0; c < SIZE; c++)
                if (board[r, c] == 0 && SudokuUtils.CountBits(candidates[r, c]) == 2)
                    cells.Add((c, candidates[r, c]));

            for (int i = 0; i < cells.Count; i++)
            {
                for (int j = i + 1; j < cells.Count; j++)
                {
                    if (cells[i].mask == cells[j].mask)
                    {
                        int mask = cells[i].mask;
                        for (int k = 0; k < SIZE; k++)
                        {
                            if (k != cells[i].c && k != cells[j].c && board[r, k] == 0)
                            {
                                if ((candidates[r, k] & mask) != 0)
                                {
                                    candidates[r, k] &= ~mask;
                                    changed = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        return changed;
    }

    /// <summary>
    /// [Lv 4] Intersection / Pointing
    /// 박스 내에서 특정 숫자가 같은 행/열에만 있으면, 해당 행/열의 박스 밖 후보 제거
    /// </summary>
    public static bool ApplyIntersection(int[,] board, int[,] candidates)
    {
        bool changed = false;

        for (int b = 0; b < SIZE; b++)
        {
            int startRow = (b / BOX_SIZE) * BOX_SIZE;
            int startCol = (b % BOX_SIZE) * BOX_SIZE;

            for (int num = 1; num <= 9; num++)
            {
                int mask = 1 << (num - 1);
                var possible = new List<(int r, int c)>();

                for (int i = 0; i < BOX_SIZE; i++)
                    for (int j = 0; j < BOX_SIZE; j++)
                        if (board[startRow + i, startCol + j] == 0 &&
                            (candidates[startRow + i, startCol + j] & mask) != 0)
                            possible.Add((startRow + i, startCol + j));

                if (possible.Count < 2 || possible.Count > 3) continue;

                // Row Alignment Check (Pointing)
                int firstRow = possible[0].r;
                if (possible.All(p => p.r == firstRow))
                {
                    for (int c = 0; c < SIZE; c++)
                    {
                        if ((c < startCol || c >= startCol + BOX_SIZE) &&
                            board[firstRow, c] == 0 &&
                            (candidates[firstRow, c] & mask) != 0)
                        {
                            candidates[firstRow, c] &= ~mask;
                            changed = true;
                        }
                    }
                }

                // Column Alignment Check (Claiming)
                int firstCol = possible[0].c;
                if (possible.All(p => p.c == firstCol))
                {
                    for (int r = 0; r < SIZE; r++)
                    {
                        if ((r < startRow || r >= startRow + BOX_SIZE) &&
                            board[r, firstCol] == 0 &&
                            (candidates[r, firstCol] & mask) != 0)
                        {
                            candidates[r, firstCol] &= ~mask;
                            changed = true;
                        }
                    }
                }
            }
        }
        return changed;
    }

    /// <summary>
    /// [Lv 5] X-Wing
    /// 두 행에서 특정 숫자가 정확히 같은 두 열에만 있으면, 다른 행의 해당 열 후보 제거
    /// </summary>
    public static bool ApplyXWing(int[,] board, int[,] candidates)
    {
        bool changed = false;

        for (int num = 1; num <= 9; num++)
        {
            int mask = 1 << (num - 1);
            var rows = new List<(int r, int c1, int c2)>();

            // 각 행에서 num 후보가 정확히 2개 열에만 있는 경우 수집
            for (int r = 0; r < SIZE; r++)
            {
                var cols = new List<int>();
                for (int c = 0; c < SIZE; c++)
                    if (board[r, c] == 0 && (candidates[r, c] & mask) != 0) cols.Add(c);

                if (cols.Count == 2) rows.Add((r, cols[0], cols[1]));
            }

            // X-Wing 패턴 찾기
            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = i + 1; j < rows.Count; j++)
                {
                    if (rows[i].c1 == rows[j].c1 && rows[i].c2 == rows[j].c2)
                    {
                        int c1 = rows[i].c1;
                        int c2 = rows[i].c2;

                        // 다른 행의 해당 열 후보 제거
                        for (int r = 0; r < SIZE; r++)
                        {
                            if (r != rows[i].r && r != rows[j].r)
                            {
                                if (board[r, c1] == 0 && (candidates[r, c1] & mask) != 0)
                                {
                                    candidates[r, c1] &= ~mask; changed = true;
                                }
                                if (board[r, c2] == 0 && (candidates[r, c2] & mask) != 0)
                                {
                                    candidates[r, c2] &= ~mask; changed = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        return changed;
    }
}

// =========================================================
// 3. 난이도 섞임 검사 및 생성 (Generator Logic)
// =========================================================
/// <summary>
/// 스도쿠 퍼즐 생성기
/// 완전한 보드 생성 → 구멍 뚫기 → 기술 분석 → 난이도 검증
/// </summary>
public static class SudokuGenerator
{
    private static System.Random _rng = new System.Random();
    private const int SIZE = 9;
    private const int BOX_SIZE = 3;

    /// <summary>
    /// 스도쿠 퍼즐 생성 (정적 메서드)
    /// </summary>
    public static SudokuData Generate(Difficulty difficulty)
    {
        UnityEngine.Debug.Log($"[INFO] SudokuGenerator::Generate - Starting generation for {difficulty}");

        (int minHole, int maxHole) = GetHoleRange(difficulty);

        int[,] solvedBoard = new int[SIZE, SIZE];
        int[,] puzzleBoard = new int[SIZE, SIZE];
        HashSet<TechType> usedTechs = new HashSet<TechType>();

        int maxRetries = 500;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            // 1. 정답 보드 생성 (대각선 박스 최적화)
            solvedBoard = GenerateFullBoard();
            if (solvedBoard == null)
            {
                UnityEngine.Debug.LogError("[ERROR] SudokuGenerator::Generate - Failed to generate full board");
                continue;
            }

            puzzleBoard = (int[,])solvedBoard.Clone();

            // 2. 구멍 뚫기 (난이도별 구멍 개수 적용)
            int targetHoles = _rng.Next(minHole, maxHole + 1);
            DigHoles(puzzleBoard, solvedBoard, targetHoles);

            // 3. 기술 분석
            usedTechs = AnalyzeUsedTechs(puzzleBoard);

            // 4. 조건 검사 (기술 섞임 확인)
            if (CheckTechCondition(difficulty, usedTechs))
            {
                UnityEngine.Debug.Log($"[INFO] SudokuGenerator::Generate - Success on attempt {attempt + 1}, " +
                                     $"Holes: {targetHoles}, Techs: {string.Join(",", usedTechs)}");

                return new SudokuData
                {
                    Board = puzzleBoard,
                    SolvedBoard = solvedBoard,
                    Diff = difficulty,
                    UsedTechs = usedTechs.ToList(),
                    Hints = 5 // 힌트 5개 고정
                };
            }
        }

        UnityEngine.Debug.LogWarning($"[WARNING] SudokuGenerator::Generate - Failed to meet tech conditions after {maxRetries} attempts");

        // 최대 재시도 후에도 실패하면 마지막 생성된 퍼즐 반환
        return new SudokuData
        {
            Board = puzzleBoard,
            SolvedBoard = solvedBoard,
            Diff = difficulty,
            UsedTechs = usedTechs.ToList(),
            Hints = 5
        };
    }

    /// <summary>
    /// 완전한 스도쿠 보드 생성 (대각선 박스 최적화)
    /// </summary>
    private static int[,] GenerateFullBoard()
    {
        int[,] board = new int[SIZE, SIZE];

        // 대각선 3개 박스는 서로 독립적이므로 먼저 랜덤으로 채움
        FillDiagonalBoxes(board);

        // 나머지 빈 칸을 백트래킹으로 채움
        if (!SolveBoard(board))
        {
            return null;
        }

        return board;
    }

    /// <summary>
    /// 대각선 박스 3개 ((0,0), (3,3), (6,6)) 우선 채우기
    /// </summary>
    private static void FillDiagonalBoxes(int[,] board)
    {
        for (int i = 0; i < SIZE; i += BOX_SIZE)
        {
            FillBox(board, i, i);
        }
    }

    /// <summary>
    /// 3x3 박스 내부를 1-9로 랜덤하게 채우기
    /// </summary>
    private static void FillBox(int[,] board, int rowStart, int colStart)
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
    private static bool SolveBoard(int[,] board)
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
            if (SudokuUtils.IsSafe(board, row, col, num))
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

    /// <summary>
    /// 난이도별 구멍 개수 범위 반환
    /// Easy: 15-26 holes (55-66 hints)
    /// Medium: 27-40 holes (41-54 hints)
    /// Hard: 41-50 holes (31-40 hints)
    /// </summary>
    private static (int minHole, int maxHole) GetHoleRange(Difficulty diff)
    {
        return diff switch
        {
            Difficulty.Easy => (25, 35),
            Difficulty.Medium => (25, 45),
            Difficulty.Hard => (50, 55),
            _ => (35, 45)
        };
    }

    /// <summary>
    /// 난이도별 기술 섞임 조건 검사
    /// </summary>
    private static bool CheckTechCondition(Difficulty diff, HashSet<TechType> techs)
    {
        if (techs.Count == 0) return false;

        switch (diff)
        {
            case Difficulty.Easy:
                // Naked Single, Hidden Single 중 하나 이상
                return techs.Contains(TechType.NakedSingle) ||
                       techs.Contains(TechType.HiddenSingle);

            case Difficulty.Medium:
                // Hidden Single, Pair, Intersection 중 하나 이상 포함
                return techs.Contains(TechType.HiddenSingle) ||
                       techs.Contains(TechType.Pair) ||
                       techs.Contains(TechType.Intersection);

            case Difficulty.Hard:
                // Pair, Intersection, X-Wing 중 하나 이상 포함
                return techs.Contains(TechType.Pair) ||
                       techs.Contains(TechType.Intersection) ||
                       techs.Contains(TechType.XWing);

            default: return false;
        }
    }

    /// <summary>
    /// 사용된 기술 분석기
    /// 퍼즐을 Human Solver처럼 풀면서 어떤 기술이 사용되었는지 추적
    /// </summary>
    private static HashSet<TechType> AnalyzeUsedTechs(int[,] board)
    {
        int[,] workBoard = (int[,])board.Clone();
        int[,] candidates = SudokuUtils.InitCandidates(workBoard);
        var techs = new HashSet<TechType>();

        bool changed = true;
        while (changed && !SudokuUtils.IsFull(workBoard))
        {
            changed = false;

            if (SudokuTechs.ApplyNakedSingle(workBoard, candidates))
            {
                techs.Add(TechType.NakedSingle); changed = true; continue;
            }
            if (SudokuTechs.ApplyHiddenSingle(workBoard, candidates))
            {
                techs.Add(TechType.HiddenSingle); changed = true; continue;
            }
            if (SudokuTechs.ApplyPairs(workBoard, candidates))
            {
                techs.Add(TechType.Pair); changed = true; continue;
            }
            if (SudokuTechs.ApplyIntersection(workBoard, candidates))
            {
                techs.Add(TechType.Intersection); changed = true; continue;
            }
            if (SudokuTechs.ApplyXWing(workBoard, candidates))
            {
                techs.Add(TechType.XWing); changed = true; continue;
            }
        }
        return techs;
    }

    /// <summary>
    /// 구멍 뚫기 헬퍼 (유일 해 보장)
    /// </summary>
    private static void DigHoles(int[,] board, int[,] solution, int targetHoles)
    {
        var positions = new List<(int r, int c)>();
        for (int i = 0; i < SIZE; i++)
            for (int j = 0; j < SIZE; j++)
                positions.Add((i, j));

        // Fisher-Yates 셔플
        int n = positions.Count;
        while (n > 1)
        {
            n--;
            int k = _rng.Next(n + 1);
            var temp = positions[k];
            positions[k] = positions[n];
            positions[n] = temp;
        }

        int holes = 0;
        foreach (var (r, c) in positions)
        {
            if (holes >= targetHoles) break;

            int temp = board[r, c];
            board[r, c] = 0;

            // 유일 해 검증
            if (!HasUniqueSolution(board, solution))
            {
                // 해가 2개 이상이면 복구
                board[r, c] = temp;
            }
            else
            {
                holes++;
            }
        }

        UnityEngine.Debug.Log($"[INFO] SudokuGenerator::DigHoles - Created {holes} holes (target: {targetHoles})");
    }

    /// <summary>
    /// 유일한 해를 가지는지 확인
    /// </summary>
    private static bool HasUniqueSolution(int[,] board, int[,] solution)
    {
        int[,] clone = (int[,])board.Clone();
        int solutionCount = 0;
        SolveAndCount(clone, solution, ref solutionCount);
        return solutionCount == 1;
    }

    /// <summary>
    /// 해의 개수를 세기 (최대 2개까지만 확인)
    /// </summary>
    private static void SolveAndCount(int[,] board, int[,] solution, ref int count)
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

        // 1-9 시도 (정답과 비교하여 빠른 종료)
        for (int num = 1; num <= 9; num++)
        {
            if (SudokuUtils.IsSafe(board, row, col, num))
            {
                board[row, col] = num;
                SolveAndCount(board, solution, ref count);
                board[row, col] = 0;
            }
        }
    }

    /// <summary>
    /// Fisher-Yates 셔플 알고리즘
    /// </summary>
    private static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = _rng.Next(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}

// =========================================================
// 기존 코드와의 호환성을 위한 래퍼 클래스
// =========================================================
/// <summary>
/// 기존 SudokuGenerator 인터페이스 유지용 래퍼 클래스
/// 기존 코드와의 호환성을 위해 비동기 메서드 제공
/// </summary>
public class SudokuGeneratorWrapper
{
    /// <summary>
    /// 새 퍼즐 비동기 생성 (기존 인터페이스 호환)
    /// </summary>
    public async Task<PuzzleResult> GeneratePuzzleAsync(SudokuDifficulty difficulty, int hintCount = 0)
    {
        Stopwatch timer = Stopwatch.StartNew();
        UnityEngine.Debug.Log($"[INFO] SudokuGeneratorWrapper::GeneratePuzzleAsync - Starting async puzzle generation for {difficulty}");

        // 백그라운드 스레드에서 퍼즐 생성 실행
        PuzzleResult result = await Task.Run(() => GeneratePuzzle(difficulty, hintCount));

        timer.Stop();
        UnityEngine.Debug.Log($"[INFO] SudokuGeneratorWrapper::GeneratePuzzleAsync - Completed in {timer.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 새 퍼즐 생성 (동기, 기존 인터페이스 호환)
    /// </summary>
    public PuzzleResult GeneratePuzzle(SudokuDifficulty difficulty, int hintCount = 0)
    {
        Stopwatch timer = Stopwatch.StartNew();

        // SudokuDifficulty → Difficulty 변환
        Difficulty newDifficulty = difficulty switch
        {
            SudokuDifficulty.Easy => Difficulty.Easy,
            SudokuDifficulty.Medium => Difficulty.Medium,
            SudokuDifficulty.Hard => Difficulty.Hard,
            _ => Difficulty.Medium
        };

        // 새로운 정적 생성기 호출
        SudokuData sudokuData = SudokuGenerator.Generate(newDifficulty);

        // SudokuData → PuzzleResult 변환
        bool[,] hints = new bool[9, 9];
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                hints[r, c] = (sudokuData.Board[r, c] != 0);
            }
        }

        timer.Stop();
        UnityEngine.Debug.Log($"[INFO] SudokuGeneratorWrapper::GeneratePuzzle - Generated in {timer.ElapsedMilliseconds}ms");

        return new PuzzleResult
        {
            Board = sudokuData.Board,
            Solution = sudokuData.SolvedBoard,
            Hints = hints,
            Difficulty = difficulty,
            MeasuredDifficulty = difficulty, // 동일 난이도로 설정
            HintCount = sudokuData.Hints,
            Seed = (int)DateTime.Now.Ticks
        };
    }

    /// <summary>
    /// 퍼즐 생성 결과 데이터 (기존 인터페이스 호환)
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

# Sudoku Algorithm: Generation, Digging, and Difficulty Analysis

이 문서는 스도쿠 퍼즐을 생성하고, 플레이 가능한 형태로 빈 칸을 뚫고, 최종적인 난이도를 사람이 푸는 방식(Logic)에 기반하여 측정하는 알고리즘을 설명합니다.

---

## 1. 알고리즘 개요 (Algorithm Overview)

스도쿠 생성 시스템은 크게 세 가지 단계로 구성됩니다.

### 1.1. 완전한 보드 생성 (Full Board Generation)
* **목표:** 규칙(행, 열, 3x3 박스 내 중복 없음)을 만족하는 9x9 보드를 생성합니다.
* **핵심 기법:** **Backtracking (백트래킹)**
* **최적화:** $(0,0), (3,3), (6,6)$ 위치의 3x3 박스는 서로 독립적이므로, 이들을 먼저 무작위로 채운 후 나머지를 탐색하면 연산 속도가 비약적으로 상승합니다.



### 1.2. 빈 칸 뚫기 (Hole Digging)
* **목표:** 정답 보드에서 숫자를 지워 퍼즐을 만듭니다.
* **제약 조건:** **유일한 해(Unique Solution)**가 보장되어야 합니다.
* **로직:**
    1.  임의의 좌표 숫자를 지웁니다.
    2.  `Solve()` 함수를 돌려 해가 2개 이상 나오는지 확인합니다.
    3.  해가 2개 이상이면 지운 숫자를 다시 복구합니다.
    4.  이를 목표 힌트 수에 도달하거나 모든 칸을 확인할 때까지 반복합니다.

### 1.3. 난이도 측정 (Difficulty Evaluation)
* **목표:** 단순히 빈 칸의 개수가 아닌, **"어떤 논리 기술이 필요한가"**를 기준으로 난이도를 판별합니다.
* **핵심 기법:** **Human Solver Simulation & Bitmask**
* **난이도 기준:**
    * **Easy:** `Naked Single` (후보가 1개인 칸 찾기)만으로 풀림.
    * **Medium:** `Hidden Single` (특정 줄에서 유일한 위치 찾기) 기술 필요.
    * **Hard/Expert:** `Pair`, `Pointing`, `X-Wing` 등 고급 기술 필요.



---

## 2. 통합 C# 코드 (SudokuGenerator.cs)

성능 최적화를 위해 후보 숫자 관리에 **비트마스크(Bitmask)**를 사용했습니다.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

public enum Difficulty { Easy, Medium, Hard, Expert, Extreme }

public class SudokuData
{
    public int[,] Board;       // 플레이용 퍼즐 (0은 빈 칸)
    public int[,] SolvedBoard; // 정답지
    public Difficulty Diff;    // 측정된 난이도
}

public class SudokuGenerator
{
    private System.Random rng = new System.Random();

    // ============================================================
    // Public API
    // ============================================================

    /// <summary>
    /// 목표 난이도에 근접한 스도쿠 퍼즐을 생성하여 반환합니다.
    /// </summary>
    public SudokuData CreatePuzzle(Difficulty targetDiff)
    {
        // 1. 완전한 보드 생성
        int[,] solvedBoard = GenerateSolvedBoard();
        int[,] puzzleBoard = (int[,])solvedBoard.Clone();

        // 2. 빈 칸 뚫기 (유일 해 보장)
        DigHoles(puzzleBoard, targetDiff);

        // 3. 최종 난이도 판별 (Human Solver 로직)
        Difficulty finalDiff = EvaluateDifficulty(puzzleBoard);

        return new SudokuData
        {
            Board = puzzleBoard,
            SolvedBoard = solvedBoard,
            Diff = finalDiff
        };
    }

    // ============================================================
    // Step 1: Full Board Generation
    // ============================================================

    private int[,] GenerateSolvedBoard()
    {
        int[,] board = new int[9, 9];
        // 최적화: 독립된 대각선 박스 3개를 먼저 채움
        FillDiagonalBoxes(board);
        // 나머지 빈 칸을 백트래킹으로 채움
        SolveBoard(board);
        return board;
    }

    private void FillDiagonalBoxes(int[,] board)
    {
        for (int i = 0; i < 9; i += 3)
            FillBox(board, i, i);
    }

    private void FillBox(int[,] board, int rowStart, int colStart)
    {
        int num;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                do
                {
                    num = rng.Next(1, 10);
                } while (!IsSafeInBox(board, rowStart, colStart, num));
                board[rowStart + i, colStart + j] = num;
            }
        }
    }

    private bool SolveBoard(int[,] board)
    {
        int row = -1, col = -1;
        bool isEmpty = true;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (board[i, j] == 0)
                {
                    row = i; col = j;
                    isEmpty = false;
                    break;
                }
            }
            if (!isEmpty) break;
        }

        if (isEmpty) return true;

        var numbers = Enumerable.Range(1, 9).OrderBy(x => rng.Next()).ToList();

        foreach (int num in numbers)
        {
            if (IsSafe(board, row, col, num))
            {
                board[row, col] = num;
                if (SolveBoard(board)) return true;
                board[row, col] = 0;
            }
        }
        return false;
    }

    // ============================================================
    // Step 2: Digging Holes (Uniqueness Check)
    // ============================================================

    private void DigHoles(int[,] board, Difficulty target)
    {
        List<(int r, int c)> positions = new List<(int, int)>();
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 9; j++)
                positions.Add((i, j));

        // Fisher-Yates Shuffle
        int n = positions.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var value = positions[k];
            positions[k] = positions[n];
            positions[n] = value;
        }

        // 난이도별 목표 힌트 수 (가이드라인)
        int minClues = target switch
        {
            Difficulty.Easy => 36,
            Difficulty.Medium => 32,
            Difficulty.Hard => 26,
            _ => 17
        };

        int clues = 81;

        foreach (var pos in positions)
        {
            if (clues <= minClues) break;

            int temp = board[pos.r, pos.c];
            board[pos.r, pos.c] = 0; // 구멍 뚫기 시도

            // 유일한 해인지 검증
            if (!HasUniqueSolution(board))
            {
                board[pos.r, pos.c] = temp; // 해가 2개 이상이면 복구
            }
            else
            {
                clues--;
            }
        }
    }

    private bool HasUniqueSolution(int[,] board)
    {
        int[,] clone = (int[,])board.Clone();
        int solutions = 0;
        SolveAndCount(clone, ref solutions);
        return solutions == 1;
    }

    private void SolveAndCount(int[,] board, ref int count)
    {
        if (count > 1) return; // 해가 2개 이상이면 즉시 중단 (가지치기)

        int row = -1, col = -1;
        bool isEmpty = true;

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (board[i, j] == 0)
                {
                    row = i; col = j;
                    isEmpty = false;
                    break;
                }
            }
            if (!isEmpty) break;
        }

        if (isEmpty)
        {
            count++;
            return;
        }

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
    // Step 3: Difficulty Evaluation (Human Solver)
    // ============================================================

    private Difficulty EvaluateDifficulty(int[,] board)
    {
        int[,] workBoard = (int[,])board.Clone();
        int[,] candidates = InitCandidates(workBoard); // 비트마스크 초기화

        Difficulty currentDiff = Difficulty.Easy;
        bool changed = true;

        while (changed && !IsFull(workBoard))
        {
            changed = false;

            // Lv 1. Naked Single
            if (ApplyNakedSingle(workBoard, candidates))
            {
                changed = true;
                continue;
            }

            // Lv 2. Hidden Single
            if (ApplyHiddenSingle(workBoard, candidates))
            {
                currentDiff = MaxDiff(currentDiff, Difficulty.Medium);
                changed = true;
                continue;
            }
            
            // Lv 3, 4... (Pair, Pointing 등 추가 가능)

            // 더 이상 논리로 풀 수 없음 (Expert/Guessing 필요)
            if (!IsFull(workBoard)) return Difficulty.Expert;
        }

        return currentDiff;
    }

    // --- Evaluation Logic Helpers (Bitmask) ---

    private int[,] InitCandidates(int[,] board)
    {
        int[,] candidates = new int[9, 9];
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (board[r, c] != 0)
                {
                    candidates[r, c] = 0;
                }
                else
                {
                    int mask = 0x1FF; // 1~9 모든 비트 켜기
                    for (int k = 1; k <= 9; k++)
                    {
                        if (!IsSafe(board, r, c, k)) mask &= ~(1 << (k - 1));
                    }
                    candidates[r, c] = mask;
                }
            }
        }
        return candidates;
    }

    private bool ApplyNakedSingle(int[,] board, int[,] candidates)
    {
        bool changed = false;
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                // 후보 비트가 딱 1개 켜져 있는 경우
                if (board[r, c] == 0 && CountSetBits(candidates[r, c]) == 1)
                {
                    int val = 0;
                    for (int k = 1; k <= 9; k++)
                    {
                        if ((candidates[r, c] & (1 << (k - 1))) != 0)
                        {
                            val = k; break;
                        }
                    }
                    ConfirmCell(board, candidates, r, c, val);
                    changed = true;
                }
            }
        }
        return changed;
    }

    private bool ApplyHiddenSingle(int[,] board, int[,] candidates)
    {
        bool changed = false;
        // 행(Row) 검사만 예시로 포함 (열, 박스 검사도 동일 로직 필요)
        for (int r = 0; r < 9; r++)
        {
            for (int num = 1; num <= 9; num++)
            {
                int mask = 1 << (num - 1);
                int count = 0;
                int targetCol = -1;

                for (int c = 0; c < 9; c++)
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
        return changed;
    }

    private void ConfirmCell(int[,] board, int[,] candidates, int r, int c, int val)
    {
        board[r, c] = val;
        candidates[r, c] = 0;
        int mask = ~(1 << (val - 1)); // 해당 숫자 비트 끄기 마스크

        // 행, 열 전파
        for (int k = 0; k < 9; k++)
        {
            candidates[r, k] &= mask;
            candidates[k, c] &= mask;
        }
        
        // 박스 전파
        int startRow = r - r % 3;
        int startCol = c - c % 3;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                candidates[startRow + i, startCol + j] &= mask;
    }

    // --- Common Helpers ---

    private bool IsSafe(int[,] board, int row, int col, int num)
    {
        return IsSafeInRow(board, row, num) &&
               IsSafeInCol(board, col, num) &&
               IsSafeInBox(board, row - row % 3, col - col % 3, num);
    }

    private bool IsSafeInRow(int[,] board, int row, int num)
    {
        for (int c = 0; c < 9; c++) if (board[row, c] == num) return false;
        return true;
    }

    private bool IsSafeInCol(int[,] board, int col, int num)
    {
        for (int r = 0; r < 9; r++) if (board[r, col] == num) return false;
        return true;
    }

    private bool IsSafeInBox(int[,] board, int startRow, int startCol, int num)
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (board[startRow + i, startCol + j] == num) return false;
        return true;
    }

    private bool IsFull(int[,] board)
    {
        foreach (int val in board) if (val == 0) return false;
        return true;
    }

    private int CountSetBits(int n)
    {
        int count = 0;
        while (n > 0) { n &= (n - 1); count++; }
        return count;
    }

    private Difficulty MaxDiff(Difficulty a, Difficulty b) => a > b ? a : b;
}
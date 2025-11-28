using System.Collections.Generic;

namespace ThreeMatch.Board
{
    /// <summary>
    /// 매치 감지 로직 (정적 유틸리티 클래스)
    /// 가로/세로 매치, 크로스 매치 감지 및 점수 계산
    /// </summary>
    public static class MatchDetector
    {
        // ========== 매치 감지 ==========

        /// <summary>
        /// 전체 보드에서 모든 매치 찾기
        /// </summary>
        public static List<Match> FindAllMatches(int[,] board, int width, int height)
        {
            List<Match> matches = new List<Match>();
            bool[,] processed = new bool[width, height];  // 중복 처리 방지

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (processed[x, y] || board[x, y] == 0)
                        continue;

                    Match match = FindMatchAt(board, x, y);

                    if (match.Positions != null && match.Positions.Count >= 3)
                    {
                        matches.Add(match);

                        // 매치된 위치들을 처리됨으로 표시
                        foreach (var pos in match.Positions)
                        {
                            if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                            {
                                processed[pos.x, pos.y] = true;
                            }
                        }
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// 특정 위치에서 매치 찾기 (가로/세로/크로스)
        /// </summary>
        public static Match FindMatchAt(int[,] board, int x, int y)
        {
            int pieceId = board[x, y];
            if (pieceId == 0)
            {
                return new Match { Positions = new List<Vector2Int>() };
            }

            // 가로 매치 찾기
            List<Vector2Int> horizontalMatch = FindHorizontalMatch(board, x, y);

            // 세로 매치 찾기
            List<Vector2Int> verticalMatch = FindVerticalMatch(board, x, y);

            // 크로스 매치 확인
            if (horizontalMatch.Count >= 3 && verticalMatch.Count >= 3)
            {
                return DetectCrossMatch(horizontalMatch, verticalMatch, pieceId);
            }

            // 가로 또는 세로 매치 중 더 긴 것 반환
            if (horizontalMatch.Count >= 3)
            {
                return CreateBasicMatch(horizontalMatch, pieceId);
            }

            if (verticalMatch.Count >= 3)
            {
                return CreateBasicMatch(verticalMatch, pieceId);
            }

            // 매치 없음
            return new Match { Positions = new List<Vector2Int>() };
        }

        // ========== 개별 방향 매치 ==========

        /// <summary>
        /// 가로 방향 매치 찾기
        /// </summary>
        private static List<Vector2Int> FindHorizontalMatch(int[,] board, int x, int y)
        {
            int pieceId = board[x, y];
            List<Vector2Int> match = new List<Vector2Int>();

            // 현재 위치 추가
            match.Add(new Vector2Int(x, y));

            // 왼쪽 탐색
            int left = x - 1;
            while (left >= 0 && board[left, y] == pieceId)
            {
                match.Insert(0, new Vector2Int(left, y));
                left--;
            }

            // 오른쪽 탐색
            int right = x + 1;
            int width = board.GetLength(0);
            while (right < width && board[right, y] == pieceId)
            {
                match.Add(new Vector2Int(right, y));
                right++;
            }

            return match;
        }

        /// <summary>
        /// 세로 방향 매치 찾기
        /// </summary>
        private static List<Vector2Int> FindVerticalMatch(int[,] board, int x, int y)
        {
            int pieceId = board[x, y];
            List<Vector2Int> match = new List<Vector2Int>();

            // 현재 위치 추가
            match.Add(new Vector2Int(x, y));

            // 아래쪽 탐색
            int down = y - 1;
            while (down >= 0 && board[x, down] == pieceId)
            {
                match.Insert(0, new Vector2Int(x, down));
                down--;
            }

            // 위쪽 탐색
            int up = y + 1;
            int height = board.GetLength(1);
            while (up < height && board[x, up] == pieceId)
            {
                match.Add(new Vector2Int(x, up));
                up++;
            }

            return match;
        }

        // ========== 크로스 매치 ==========

        /// <summary>
        /// 크로스 매치 생성 (가로 + 세로 매치가 교차하는 경우)
        /// </summary>
        private static Match DetectCrossMatch(List<Vector2Int> horizontal, List<Vector2Int> vertical, int pieceId)
        {
            // 중복 제거하고 모든 위치 합치기
            HashSet<Vector2Int> allPositions = new HashSet<Vector2Int>();
            foreach (var pos in horizontal)
            {
                allPositions.Add(pos);
            }
            foreach (var pos in vertical)
            {
                allPositions.Add(pos);
            }

            // 매치 타입 결정
            MatchType type = DetermineMatchType(horizontal.Count, vertical.Count);

            // 점수 계산 (크로스 매치는 콤보 배율 적용 전 기본 점수)
            int baseScore = GetBaseScore(type);

            return new Match
            {
                Type = type,
                Positions = new List<Vector2Int>(allPositions),
                PieceId = pieceId,
                Score = baseScore
            };
        }

        /// <summary>
        /// 기본 매치 생성 (일렬 매치)
        /// </summary>
        private static Match CreateBasicMatch(List<Vector2Int> positions, int pieceId)
        {
            MatchType type = DetermineBasicMatchType(positions.Count);
            int baseScore = GetBaseScore(type);

            return new Match
            {
                Type = type,
                Positions = positions,
                PieceId = pieceId,
                Score = baseScore
            };
        }

        // ========== 점수 계산 ==========

        /// <summary>
        /// 매치 점수 계산 (콤보 배율 적용)
        /// </summary>
        public static int CalculateScore(Match match, int comboMultiplier)
        {
            int baseScore = match.Score;
            int finalScore = baseScore * comboMultiplier;
            return finalScore;
        }

        /// <summary>
        /// 매치 타입에 따른 기본 점수
        /// </summary>
        private static int GetBaseScore(MatchType type)
        {
            switch (type)
            {
                case MatchType.Basic3:
                    return 100;
                case MatchType.Basic4:
                    return 500;
                case MatchType.Basic5:
                    return 1000;
                case MatchType.Cross33:
                    return 1000;
                case MatchType.Cross43:
                    return 1500;
                case MatchType.Cross53:
                    return 2000;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 크로스 매치 타입 결정
        /// </summary>
        private static MatchType DetermineMatchType(int horizontalCount, int verticalCount)
        {
            // 5+3 크로스
            if ((horizontalCount >= 5 && verticalCount >= 3) || (horizontalCount >= 3 && verticalCount >= 5))
            {
                return MatchType.Cross53;
            }

            // 4+3 크로스
            if ((horizontalCount >= 4 && verticalCount >= 3) || (horizontalCount >= 3 && verticalCount >= 4))
            {
                return MatchType.Cross43;
            }

            // 3+3 크로스
            if (horizontalCount >= 3 && verticalCount >= 3)
            {
                return MatchType.Cross33;
            }

            // 기본 매치 (크로스가 아닌 경우)
            int maxCount = System.Math.Max(horizontalCount, verticalCount);
            return DetermineBasicMatchType(maxCount);
        }

        /// <summary>
        /// 기본 매치 타입 결정 (일렬 매치)
        /// </summary>
        private static MatchType DetermineBasicMatchType(int count)
        {
            if (count >= 5)
                return MatchType.Basic5;
            else if (count >= 4)
                return MatchType.Basic4;
            else if (count >= 3)
                return MatchType.Basic3;
            else
                return MatchType.Basic3;  // 기본값
        }
    }
}

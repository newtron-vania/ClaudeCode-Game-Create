using System;
using System.Collections.Generic;

namespace ThreeMatch.Board
{
    /// <summary>
    /// 3-Match 게임의 순수 데이터 모델 (MonoBehaviour 상속 안 함)
    /// 데이터-View 분리 패턴: 데이터만 관리하고, 이벤트를 통해 View에 통지
    /// </summary>
    public class ThreeMatchBoard
    {
        // ========== 필드 ==========
        private int[,] _board;                    // 보드 상태 (0: 빈칸, 1~N: 퍼즐 ID)
        private int _width;                       // 보드 너비
        private int _height;                      // 보드 높이
        private int _pieceTypeCount;              // 퍼즐 종류 수

        // ========== 이벤트 (View가 구독) ==========
        public event Action<int, int, int> OnPieceChanged;              // (x, y, newPieceId)
        public event Action<int, int, int, int> OnPiecesSwapped;        // (x1, y1, x2, y2)
        public event Action<List<Match>> OnMatchesFound;                // 매치 발견
        public event Action<List<Vector2Int>> OnPiecesDestroyed;        // 퍼즐 파괴
        public event Action<List<PieceMove>> OnPiecesFalling;           // 중력 낙하
        public event Action OnBoardShuffled;                            // Shuffle 발생
        public event Action<bool> OnDeadlockDetected;                   // Deadlock 감지

        // ========== 프로퍼티 ==========
        public int Width => _width;
        public int Height => _height;
        public int PieceTypeCount => _pieceTypeCount;

        // ========== 초기화 ==========

        /// <summary>
        /// 보드 초기화
        /// </summary>
        public void Initialize(int width, int height, int pieceTypeCount)
        {
            _width = width;
            _height = height;
            _pieceTypeCount = pieceTypeCount;
            _board = new int[width, height];
        }

        /// <summary>
        /// 초기 보드 생성 (매치 없는 상태로)
        /// </summary>
        public void GenerateInitialBoard()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int pieceId = GetNoMatchRandomPiece(x, y);
                    _board[x, y] = pieceId;
                    OnPieceChanged?.Invoke(x, y, pieceId);
                }
            }
        }

        /// <summary>
        /// 초기 생성 시 매치가 발생하지 않는 퍼즐 ID 반환
        /// </summary>
        private int GetNoMatchRandomPiece(int x, int y)
        {
            Random random = new Random();
            List<int> availablePieces = new List<int>();

            // 1부터 pieceTypeCount까지 모든 퍼즐 타입
            for (int i = 1; i <= _pieceTypeCount; i++)
            {
                availablePieces.Add(i);
            }

            // 가로 방향 체크 (왼쪽 2개)
            if (x >= 2 && _board[x - 1, y] == _board[x - 2, y])
            {
                availablePieces.Remove(_board[x - 1, y]);
            }

            // 세로 방향 체크 (아래 2개)
            if (y >= 2 && _board[x, y - 1] == _board[x, y - 2])
            {
                availablePieces.Remove(_board[x, y - 1]);
            }

            // 사용 가능한 퍼즐 중 랜덤 선택
            if (availablePieces.Count > 0)
            {
                int randomIndex = random.Next(availablePieces.Count);
                return availablePieces[randomIndex];
            }

            // 예외 상황: 모든 퍼즐이 제외된 경우 (발생하지 않아야 함)
            return random.Next(1, _pieceTypeCount + 1);
        }

        // ========== 보드 조작 ==========

        /// <summary>
        /// 두 퍼즐 교체
        /// </summary>
        public void SwapPieces(int x1, int y1, int x2, int y2)
        {
            if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
                return;

            // 데이터 교체
            int temp = _board[x1, y1];
            _board[x1, y1] = _board[x2, y2];
            _board[x2, y2] = temp;

            // 이벤트 발생 (View가 후처리)
            OnPiecesSwapped?.Invoke(x1, y1, x2, y2);
        }

        /// <summary>
        /// 유효한 교체인지 확인 (인접 여부)
        /// </summary>
        public bool IsValidSwap(int x1, int y1, int x2, int y2)
        {
            if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
                return false;

            // 인접 확인 (상하좌우만)
            int dx = Math.Abs(x1 - x2);
            int dy = Math.Abs(y1 - y2);

            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        /// <summary>
        /// 특정 위치에 퍼즐 설정
        /// </summary>
        public void SetPieceAt(int x, int y, int pieceId)
        {
            if (!IsValidPosition(x, y))
                return;

            _board[x, y] = pieceId;
            OnPieceChanged?.Invoke(x, y, pieceId);
        }

        /// <summary>
        /// 특정 위치의 퍼즐 ID 가져오기
        /// </summary>
        public int GetPieceAt(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return 0;

            return _board[x, y];
        }

        /// <summary>
        /// 유효한 위치인지 확인
        /// </summary>
        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        // ========== 매치 감지 ==========

        /// <summary>
        /// 전체 보드에서 모든 매치 찾기
        /// </summary>
        public List<Match> FindAllMatches()
        {
            List<Match> matches = MatchDetector.FindAllMatches(_board, _width, _height);

            if (matches.Count > 0)
            {
                OnMatchesFound?.Invoke(matches);
            }

            return matches;
        }

        /// <summary>
        /// 특정 위치에 매치가 있는지 확인
        /// </summary>
        public bool HasMatchAt(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return false;

            Match match = MatchDetector.FindMatchAt(_board, x, y);
            return match.Positions != null && match.Positions.Count >= 3;
        }

        /// <summary>
        /// 매치된 퍼즐들 파괴 (빈칸으로 설정)
        /// </summary>
        public void DestroyMatches(List<Match> matches)
        {
            List<Vector2Int> destroyedPositions = new List<Vector2Int>();

            foreach (var match in matches)
            {
                foreach (var pos in match.Positions)
                {
                    _board[pos.x, pos.y] = 0;  // 빈칸으로 설정
                    destroyedPositions.Add(pos);
                }
            }

            if (destroyedPositions.Count > 0)
            {
                OnPiecesDestroyed?.Invoke(destroyedPositions);
            }
        }

        // ========== 중력 및 채우기 ==========

        /// <summary>
        /// 중력 적용 (빈칸 위의 퍼즐들을 아래로 이동)
        /// </summary>
        public List<PieceMove> ApplyGravity()
        {
            List<PieceMove> moves = new List<PieceMove>();

            // 각 열마다 아래부터 위로 스캔
            for (int x = 0; x < _width; x++)
            {
                int writeY = 0;  // 다음에 쓸 위치

                for (int readY = 0; readY < _height; readY++)
                {
                    if (_board[x, readY] != 0)  // 빈칸이 아닌 경우
                    {
                        if (readY != writeY)  // 위치가 다르면 이동 필요
                        {
                            moves.Add(new PieceMove
                            {
                                From = new Vector2Int(x, readY),
                                To = new Vector2Int(x, writeY),
                                PieceId = _board[x, readY]
                            });

                            _board[x, writeY] = _board[x, readY];
                            _board[x, readY] = 0;
                        }
                        writeY++;
                    }
                }
            }

            if (moves.Count > 0)
            {
                OnPiecesFalling?.Invoke(moves);
            }

            return moves;
        }

        /// <summary>
        /// 빈칸을 새로운 퍼즐로 채우기
        /// </summary>
        public List<Vector2Int> FillEmptyCells()
        {
            List<Vector2Int> filledPositions = new List<Vector2Int>();
            Random random = new Random();

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_board[x, y] == 0)
                    {
                        int newPieceId = random.Next(1, _pieceTypeCount + 1);
                        _board[x, y] = newPieceId;
                        filledPositions.Add(new Vector2Int(x, y));
                        OnPieceChanged?.Invoke(x, y, newPieceId);
                    }
                }
            }

            return filledPositions;
        }

        // ========== Deadlock 관리 ==========

        /// <summary>
        /// Deadlock 상태인지 확인 (가능한 이동이 하나도 없는 경우)
        /// </summary>
        public bool IsDeadlocked()
        {
            // 모든 위치에서 상하좌우 교체를 시뮬레이션해서 매치가 발생하는지 확인
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    // 오른쪽 교체
                    if (x < _width - 1)
                    {
                        if (WouldCreateMatch(x, y, x + 1, y))
                        {
                            OnDeadlockDetected?.Invoke(false);
                            return false;
                        }
                    }

                    // 아래쪽 교체
                    if (y < _height - 1)
                    {
                        if (WouldCreateMatch(x, y, x, y + 1))
                        {
                            OnDeadlockDetected?.Invoke(false);
                            return false;
                        }
                    }
                }
            }

            OnDeadlockDetected?.Invoke(true);
            return true;
        }

        /// <summary>
        /// 교체 시 매치가 발생하는지 시뮬레이션
        /// </summary>
        private bool WouldCreateMatch(int x1, int y1, int x2, int y2)
        {
            // 임시로 교체
            int temp = _board[x1, y1];
            _board[x1, y1] = _board[x2, y2];
            _board[x2, y2] = temp;

            // 매치 확인
            bool hasMatch = HasMatchAt(x1, y1) || HasMatchAt(x2, y2);

            // 원상복구
            temp = _board[x1, y1];
            _board[x1, y1] = _board[x2, y2];
            _board[x2, y2] = temp;

            return hasMatch;
        }

        /// <summary>
        /// 보드 Shuffle (Deadlock 해소)
        /// </summary>
        public void ShuffleBoard()
        {
            Random random = new Random();
            List<int> allPieces = new List<int>();

            // 현재 보드의 모든 퍼즐 수집
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    allPieces.Add(_board[x, y]);
                }
            }

            // Fisher-Yates Shuffle
            for (int i = allPieces.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = allPieces[i];
                allPieces[i] = allPieces[j];
                allPieces[j] = temp;
            }

            // Shuffle된 퍼즐을 보드에 재배치
            int index = 0;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _board[x, y] = allPieces[index++];
                    OnPieceChanged?.Invoke(x, y, _board[x, y]);
                }
            }

            OnBoardShuffled?.Invoke();
        }

        // ========== 유틸리티 ==========

        /// <summary>
        /// 보드 초기화 (모든 칸을 빈칸으로)
        /// </summary>
        public void ClearBoard()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _board[x, y] = 0;
                    OnPieceChanged?.Invoke(x, y, 0);
                }
            }
        }

        /// <summary>
        /// 보드 상태 복사본 반환 (디버깅용)
        /// </summary>
        public int[,] GetBoardCopy()
        {
            int[,] copy = new int[_width, _height];
            Array.Copy(_board, copy, _board.Length);
            return copy;
        }
    }

    // ========== 데이터 구조 ==========

    /// <summary>
    /// 매치 정보
    /// </summary>
    public struct Match
    {
        public MatchType Type;                    // 매치 타입
        public List<Vector2Int> Positions;        // 매치된 위치들
        public int PieceId;                       // 매치된 퍼즐 ID
        public int Score;                         // 점수
    }

    /// <summary>
    /// 퍼즐 이동 정보
    /// </summary>
    public struct PieceMove
    {
        public Vector2Int From;                   // 시작 위치
        public Vector2Int To;                     // 목표 위치
        public int PieceId;                       // 퍼즐 ID
    }

    /// <summary>
    /// 매치 타입
    /// </summary>
    public enum MatchType
    {
        Basic3,      // 3개 일렬 (100점)
        Basic4,      // 4개 일렬 (500점)
        Basic5,      // 5개 일렬 (1,000점)
        Cross33,     // 3+3 크로스 (1,000점)
        Cross43,     // 4+3 크로스 (1,500점)
        Cross53      // 5+3 크로스 (2,000점)
    }

    /// <summary>
    /// 2D 정수 벡터 (Unity.Vector2Int 대신 사용)
    /// </summary>
    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return $"({x}, {y})";
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2Int other)
            {
                return x == other.x && y == other.y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (x, y).GetHashCode();
        }

        public static bool operator ==(Vector2Int a, Vector2Int b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Vector2Int a, Vector2Int b)
        {
            return !(a == b);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.Board
{
    /// <summary>
    /// 3-Match 보드 View 레이어 (MonoBehaviour)
    /// 데이터-View 분리 패턴: 이벤트를 구독하고 UI만 업데이트
    /// </summary>
    public class ThreeMatchBoardView : MonoBehaviour
    {
        // ========== Inspector 설정 가능한 필드 ==========
        [Header("Board Layout")]
        [SerializeField] private Transform _boardContainer;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private float _spacing = 0.1f;
        [SerializeField] private Vector2 _boardOffset = Vector2.zero;

        [Header("Piece Prefab")]
        [SerializeField] private string _piecePrefabAddress = "Prefabs/ThreeMatch/Piece";

        [Header("Animation Durations")]
        [SerializeField] private float _swapDuration = 0.3f;
        [SerializeField] private float _destroyDuration = 0.4f;
        [SerializeField] private float _fallDuration = 0.5f;
        [SerializeField] private float _shuffleDuration = 1f;
        [SerializeField] private float _delayBetweenAnimations = 0.1f;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem _matchEffectPrefab;
        [SerializeField] private ParticleSystem _comboEffectPrefab;
        [SerializeField] private GameObject _deadlockWarningUI;

        [Header("Audio")]
        [SerializeField] private string _swapSoundPath = "Audio/SFX/ThreeMatch/Swap";
        [SerializeField] private string _matchSoundPath = "Audio/SFX/ThreeMatch/Match";
        [SerializeField] private string _comboSoundPath = "Audio/SFX/ThreeMatch/Combo";
        [SerializeField] private string _shuffleSoundPath = "Audio/SFX/ThreeMatch/Shuffle";

        [Header("Debug")]
        [SerializeField] private bool _showDebugLog = false;

        // ========== Private 필드 ==========
        private PuzzlePiece[,] _pieceViews;
        private ThreeMatchBoard _boardData;
        private bool _isAnimating;
        private int _width;
        private int _height;

        // ========== 프로퍼티 ==========
        public bool IsAnimating => _isAnimating;

        // ========== 초기화 ==========

        /// <summary>
        /// 보드 View 초기화
        /// </summary>
        public void Initialize(ThreeMatchBoard boardData, int width, int height)
        {
            _boardData = boardData;
            _width = width;
            _height = height;

            // PuzzlePiece 배열 생성
            _pieceViews = new PuzzlePiece[width, height];

            // 이벤트 구독
            SubscribeToEvents();

            // 초기 보드 시각화 (데이터 생성 후 이벤트로 처리됨)
            LogDebug($"ThreeMatchBoardView initialized: {width}x{height}");
        }

        /// <summary>
        /// View 생성 (PuzzlePiece 인스턴스화)
        /// </summary>
        public void CreatePieceViews()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    CreatePieceViewAt(x, y);
                }
            }

            LogDebug("PuzzlePiece views created");
        }

        /// <summary>
        /// 특정 위치에 PuzzlePiece 생성
        /// </summary>
        private void CreatePieceViewAt(int x, int y)
        {
            Vector3 worldPos = GridToWorldPosition(x, y);

            // PoolManager를 통해 인스턴스화
            ResourceManager.Instance.InstantiateAsync(_piecePrefabAddress, _boardContainer, (instance) =>
            {
                if (instance != null)
                {
                    PuzzlePiece piece = instance.GetComponent<PuzzlePiece>();
                    if (piece != null)
                    {
                        piece.transform.position = worldPos;
                        piece.SetGridPosition(x, y);
                        _pieceViews[x, y] = piece;

                        // 초기 퍼즐 ID 설정 (데이터에서 가져오기)
                        int pieceId = _boardData.GetPieceAt(x, y);
                        if (pieceId > 0)
                        {
                            UpdatePieceView(x, y, pieceId);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            if (_boardData == null)
                return;

            _boardData.OnPieceChanged += HandlePieceChanged;
            _boardData.OnPiecesSwapped += HandlePiecesSwapped;
            _boardData.OnMatchesFound += HandleMatchesFound;
            _boardData.OnPiecesDestroyed += HandlePiecesDestroyed;
            _boardData.OnPiecesFalling += HandlePiecesFalling;
            _boardData.OnBoardShuffled += HandleBoardShuffled;
            _boardData.OnDeadlockDetected += HandleDeadlockDetected;

            LogDebug("Events subscribed");
        }

        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (_boardData == null)
                return;

            _boardData.OnPieceChanged -= HandlePieceChanged;
            _boardData.OnPiecesSwapped -= HandlePiecesSwapped;
            _boardData.OnMatchesFound -= HandleMatchesFound;
            _boardData.OnPiecesDestroyed -= HandlePiecesDestroyed;
            _boardData.OnPiecesFalling -= HandlePiecesFalling;
            _boardData.OnBoardShuffled -= HandleBoardShuffled;
            _boardData.OnDeadlockDetected -= HandleDeadlockDetected;

            LogDebug("Events unsubscribed");
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        // ========== 이벤트 핸들러 (후처리) ==========

        /// <summary>
        /// 퍼즐 변경 이벤트 핸들러
        /// </summary>
        private void HandlePieceChanged(int x, int y, int newPieceId)
        {
            UpdatePieceView(x, y, newPieceId);
            LogDebug($"Piece changed at ({x}, {y}) to ID {newPieceId}");
        }

        /// <summary>
        /// 퍼즐 교체 이벤트 핸들러
        /// </summary>
        private void HandlePiecesSwapped(int x1, int y1, int x2, int y2)
        {
            StartCoroutine(SwapAnimation(new Vector2Int(x1, y1), new Vector2Int(x2, y2)));
            PlaySound(_swapSoundPath);
            LogDebug($"Pieces swapped: ({x1}, {y1}) <-> ({x2}, {y2})");
        }

        /// <summary>
        /// 매치 발견 이벤트 핸들러
        /// </summary>
        private void HandleMatchesFound(List<Match> matches)
        {
            foreach (var match in matches)
            {
                PlayMatchEffect(match);
            }
            PlaySound(_matchSoundPath);
            LogDebug($"Matches found: {matches.Count}");
        }

        /// <summary>
        /// 퍼즐 파괴 이벤트 핸들러
        /// </summary>
        private void HandlePiecesDestroyed(List<Vector2Int> positions)
        {
            StartCoroutine(DestroyAnimation(positions));
            LogDebug($"Pieces destroyed: {positions.Count}");
        }

        /// <summary>
        /// 퍼즐 낙하 이벤트 핸들러
        /// </summary>
        private void HandlePiecesFalling(List<PieceMove> moves)
        {
            StartCoroutine(FallAnimation(moves));
            LogDebug($"Pieces falling: {moves.Count}");
        }

        /// <summary>
        /// 보드 Shuffle 이벤트 핸들러
        /// </summary>
        private void HandleBoardShuffled()
        {
            StartCoroutine(ShuffleAnimation());
            PlaySound(_shuffleSoundPath);
            LogDebug("Board shuffled");
        }

        /// <summary>
        /// Deadlock 감지 이벤트 핸들러
        /// </summary>
        private void HandleDeadlockDetected(bool isDeadlocked)
        {
            if (isDeadlocked)
            {
                PlayDeadlockWarning();
                LogDebug("Deadlock detected!");
            }
        }

        // ========== 애니메이션 ==========

        /// <summary>
        /// 교체 애니메이션
        /// </summary>
        private IEnumerator SwapAnimation(Vector2Int pos1, Vector2Int pos2)
        {
            _isAnimating = true;

            PuzzlePiece piece1 = GetPieceViewAt(pos1.x, pos1.y);
            PuzzlePiece piece2 = GetPieceViewAt(pos2.x, pos2.y);

            if (piece1 != null && piece2 != null)
            {
                Vector3 targetPos1 = GridToWorldPosition(pos2.x, pos2.y);
                Vector3 targetPos2 = GridToWorldPosition(pos1.x, pos1.y);

                // 두 퍼즐을 동시에 이동
                Coroutine move1 = StartCoroutine(piece1.MoveToPosition(targetPos1, _swapDuration));
                Coroutine move2 = StartCoroutine(piece2.MoveToPosition(targetPos2, _swapDuration));

                // 두 애니메이션 모두 완료될 때까지 대기
                yield return move1;
                yield return move2;

                // View 배열에서 위치 교체
                _pieceViews[pos1.x, pos1.y] = piece2;
                _pieceViews[pos2.x, pos2.y] = piece1;

                piece1.SetGridPosition(pos2.x, pos2.y);
                piece2.SetGridPosition(pos1.x, pos1.y);
            }

            _isAnimating = false;
        }

        /// <summary>
        /// 파괴 애니메이션
        /// </summary>
        private IEnumerator DestroyAnimation(List<Vector2Int> positions)
        {
            _isAnimating = true;

            List<Coroutine> animations = new List<Coroutine>();

            foreach (var pos in positions)
            {
                PuzzlePiece piece = GetPieceViewAt(pos.x, pos.y);
                if (piece != null)
                {
                    piece.PlayMatchEffect();
                    Coroutine anim = StartCoroutine(piece.DestroyAnimation(_destroyDuration));
                    animations.Add(anim);
                }
            }

            // 모든 파괴 애니메이션 완료 대기
            foreach (var anim in animations)
            {
                yield return anim;
            }

            // 파괴된 퍼즐들을 풀로 반환
            foreach (var pos in positions)
            {
                PuzzlePiece piece = GetPieceViewAt(pos.x, pos.y);
                if (piece != null)
                {
                    ResourceManager.Instance.ReleaseInstance(piece.gameObject);
                    _pieceViews[pos.x, pos.y] = null;
                }
            }

            yield return new WaitForSeconds(_delayBetweenAnimations);
            _isAnimating = false;
        }

        /// <summary>
        /// 낙하 애니메이션
        /// </summary>
        private IEnumerator FallAnimation(List<PieceMove> moves)
        {
            _isAnimating = true;

            List<Coroutine> animations = new List<Coroutine>();

            foreach (var move in moves)
            {
                PuzzlePiece piece = GetPieceViewAt(move.From.x, move.From.y);
                if (piece != null)
                {
                    Vector3 targetPos = GridToWorldPosition(move.To.x, move.To.y);
                    Coroutine anim = StartCoroutine(piece.MoveToPosition(targetPos, _fallDuration));
                    animations.Add(anim);

                    // View 배열 업데이트
                    _pieceViews[move.To.x, move.To.y] = piece;
                    _pieceViews[move.From.x, move.From.y] = null;
                    piece.SetGridPosition(move.To.x, move.To.y);
                }
            }

            // 모든 낙하 애니메이션 완료 대기
            foreach (var anim in animations)
            {
                yield return anim;
            }

            yield return new WaitForSeconds(_delayBetweenAnimations);
            _isAnimating = false;
        }

        /// <summary>
        /// Shuffle 애니메이션
        /// </summary>
        private IEnumerator ShuffleAnimation()
        {
            _isAnimating = true;

            // 간단한 Shuffle 애니메이션: 모든 퍼즐을 빠르게 깜빡임
            for (int i = 0; i < 3; i++)
            {
                // 모든 퍼즐 숨기기
                SetAllPiecesVisible(false);
                yield return new WaitForSeconds(_shuffleDuration / 6f);

                // 모든 퍼즐 보이기
                SetAllPiecesVisible(true);
                yield return new WaitForSeconds(_shuffleDuration / 6f);
            }

            // 모든 퍼즐을 새 위치로 업데이트
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int pieceId = _boardData.GetPieceAt(x, y);
                    UpdatePieceView(x, y, pieceId);
                }
            }

            _isAnimating = false;
        }

        // ========== 시각 피드백 ==========

        /// <summary>
        /// 매치 이펙트 재생
        /// </summary>
        private void PlayMatchEffect(Match match)
        {
            if (_matchEffectPrefab == null || match.Positions == null || match.Positions.Count == 0)
                return;

            // 매치된 위치의 중심점 계산
            Vector3 centerPos = Vector3.zero;
            foreach (var pos in match.Positions)
            {
                centerPos += GridToWorldPosition(pos.x, pos.y);
            }
            centerPos /= match.Positions.Count;

            // 이펙트 생성 및 재생
            ParticleSystem effect = Instantiate(_matchEffectPrefab, centerPos, Quaternion.identity, _boardContainer);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        /// <summary>
        /// 콤보 이펙트 재생
        /// </summary>
        public void PlayComboEffect(int comboCount)
        {
            if (_comboEffectPrefab == null)
                return;

            Vector3 centerPos = _boardContainer.position;
            ParticleSystem effect = Instantiate(_comboEffectPrefab, centerPos, Quaternion.identity, _boardContainer);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);

            PlaySound(_comboSoundPath);
        }

        /// <summary>
        /// Deadlock 경고 UI 표시
        /// </summary>
        private void PlayDeadlockWarning()
        {
            if (_deadlockWarningUI != null)
            {
                _deadlockWarningUI.SetActive(true);
                StartCoroutine(HideDeadlockWarningAfterDelay(2f));
            }
        }

        private IEnumerator HideDeadlockWarningAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_deadlockWarningUI != null)
            {
                _deadlockWarningUI.SetActive(false);
            }
        }

        // ========== 유틸리티 ==========

        /// <summary>
        /// 그리드 좌표를 월드 좌표로 변환
        /// </summary>
        private Vector3 GridToWorldPosition(int x, int y)
        {
            float totalCellSize = _cellSize + _spacing;
            float worldX = x * totalCellSize + _boardOffset.x;
            float worldY = y * totalCellSize + _boardOffset.y;

            if (_boardContainer != null)
            {
                return _boardContainer.position + new Vector3(worldX, worldY, 0);
            }

            return new Vector3(worldX, worldY, 0);
        }

        /// <summary>
        /// 특정 위치의 PuzzlePiece View 가져오기
        /// </summary>
        private PuzzlePiece GetPieceViewAt(int x, int y)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                return _pieceViews[x, y];
            }
            return null;
        }

        /// <summary>
        /// PuzzlePiece View 업데이트 (스프라이트 변경)
        /// </summary>
        private void UpdatePieceView(int x, int y, int pieceId)
        {
            PuzzlePiece piece = GetPieceViewAt(x, y);
            if (piece != null)
            {
                // ThreeMatchDataProvider를 통해 스프라이트 로드
                string spritePath = $"Sprites/ThreeMatch/Piece_{pieceId}";
                ResourceManager.Instance.LoadAsync<Sprite>(spritePath, (sprite) =>
                {
                    if (sprite != null && piece != null)
                    {
                        piece.SetPieceType(pieceId, sprite);
                    }
                });
            }
        }

        /// <summary>
        /// 모든 퍼즐 가시성 설정
        /// </summary>
        private void SetAllPiecesVisible(bool visible)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    PuzzlePiece piece = GetPieceViewAt(x, y);
                    if (piece != null)
                    {
                        piece.gameObject.SetActive(visible);
                    }
                }
            }
        }

        /// <summary>
        /// 사운드 재생 (SoundManager 사용)
        /// </summary>
        private void PlaySound(string soundPath)
        {
            if (string.IsNullOrEmpty(soundPath))
                return;

            SoundManager.Instance.PlaySFX(soundPath);
        }

        /// <summary>
        /// 디버그 로그 출력
        /// </summary>
        private void LogDebug(string message)
        {
            if (_showDebugLog)
            {
                Debug.Log($"[ThreeMatchBoardView] {message}");
            }
        }
    }
}

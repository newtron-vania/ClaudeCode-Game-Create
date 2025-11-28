using System;
using UnityEngine;

namespace ThreeMatch.Board
{
    /// <summary>
    /// 3-Match 입력 처리 컨트롤러
    /// InputManager의 이벤트를 구독하고 퍼즐 선택/교체 로직 처리
    /// </summary>
    public class InputController : MonoBehaviour
    {
        // ========== Inspector 설정 가능한 필드 ==========
        [Header("Input Settings")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private LayerMask _pieceLayerMask = -1;
        [SerializeField] private float _dragThreshold = 0.3f;
        [SerializeField] private bool _enableTouchInput = true;
        [SerializeField] private bool _enableMouseInput = true;

        [Header("Visual Feedback")]
        [SerializeField] private Color _selectedHighlightColor = new Color(1f, 1f, 0.5f, 1f);
        [SerializeField] private float _highlightScale = 1.1f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugLog = false;

        // ========== Private 필드 ==========
        private Vector2Int? _selectedPiece;
        private ThreeMatchBoard _board;
        private ThreeMatchBoardView _boardView;
        private bool _isProcessing;

        private Vector2 _dragStartPosition;
        private bool _isDragging;

        private SpriteRenderer _highlightedPieceSpriteRenderer;
        private Color _originalHighlightColor;
        private Vector3 _originalHighlightScale;

        // ========== 이벤트 ==========
        public event Action<Vector2Int, Vector2Int> OnSwapRequested;

        // ========== 초기화 ==========

        private void Awake()
        {
            // 메인 카메라 자동 찾기
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        /// <summary>
        /// InputController 초기화
        /// </summary>
        public void Initialize(ThreeMatchBoard board, ThreeMatchBoardView boardView)
        {
            _board = board;
            _boardView = boardView;
            _selectedPiece = null;
            _isProcessing = false;

            LogDebug("InputController initialized");
        }

        private void OnEnable()
        {
            // InputManager 이벤트 구독
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInputEvent += HandleInputEvent;
            }
        }

        private void OnDisable()
        {
            // InputManager 이벤트 구독 해제
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnInputEvent -= HandleInputEvent;
            }
        }

        // ========== InputManager 이벤트 처리 ==========

        /// <summary>
        /// InputManager로부터 입력 이벤트 수신
        /// </summary>
        private void HandleInputEvent(InputEventData inputData)
        {
            if (_isProcessing || _boardView == null || _boardView.IsAnimating)
                return;

            // 마우스 입력 처리
            if (_enableMouseInput)
            {
                switch (inputData.Type)
                {
                    case InputType.PointerDown:
                        // 왼쪽 버튼만 처리 (PointerButton: 0 = Left, 1 = Right, 2 = Middle)
                        if (inputData.PointerButton == 0)
                        {
                            OnPointerDown(inputData.PointerPosition);
                        }
                        break;

                    case InputType.PointerUp:
                        if (inputData.PointerButton == 0)
                        {
                            OnPointerUp(inputData.PointerPosition);
                        }
                        break;
                }
            }

            // 터치 입력 처리
            if (_enableTouchInput)
            {
                switch (inputData.Type)
                {
                    case InputType.TouchBegan:
                        OnPointerDown(inputData.PointerPosition);
                        break;

                    case InputType.TouchEnded:
                        OnPointerUp(inputData.PointerPosition);
                        break;
                }
            }
        }

        // ========== 입력 처리 로직 ==========

        /// <summary>
        /// 포인터 다운 (클릭/터치 시작)
        /// </summary>
        private void OnPointerDown(Vector2 screenPosition)
        {
            Vector2Int? gridPos = ScreenToGridPosition(screenPosition);

            if (gridPos.HasValue)
            {
                _dragStartPosition = screenPosition;
                _isDragging = true;

                if (_selectedPiece == null)
                {
                    // 첫 번째 퍼즐 선택
                    SelectPiece(gridPos.Value);
                }
                else
                {
                    // 두 번째 퍼즐 선택 → 교체 시도
                    TrySwap(gridPos.Value);
                }

                LogDebug($"Pointer down at grid: {gridPos.Value}");
            }
        }

        /// <summary>
        /// 포인터 업 (클릭/터치 종료)
        /// </summary>
        private void OnPointerUp(Vector2 screenPosition)
        {
            if (_isDragging)
            {
                // 드래그 거리 확인
                float dragDistance = Vector2.Distance(_dragStartPosition, screenPosition);

                if (dragDistance > _dragThreshold)
                {
                    // 드래그 방향으로 교체 시도
                    HandleDragSwap(screenPosition);
                }

                _isDragging = false;
            }
        }

        /// <summary>
        /// 드래그로 교체 처리
        /// </summary>
        private void HandleDragSwap(Vector2 endPosition)
        {
            if (!_selectedPiece.HasValue)
                return;

            Vector2 dragDirection = (endPosition - _dragStartPosition).normalized;
            Vector2Int adjacentPos = GetAdjacentPositionFromDirection(_selectedPiece.Value, dragDirection);

            if (adjacentPos.x >= 0 && adjacentPos.y >= 0)
            {
                TrySwap(adjacentPos);
            }
        }

        /// <summary>
        /// 드래그 방향에서 인접 위치 계산
        /// </summary>
        private Vector2Int GetAdjacentPositionFromDirection(Vector2Int origin, Vector2 direction)
        {
            // 가장 큰 방향 성분 찾기
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // 가로 방향
                if (direction.x > 0)
                    return new Vector2Int(origin.x + 1, origin.y);  // 오른쪽
                else
                    return new Vector2Int(origin.x - 1, origin.y);  // 왼쪽
            }
            else
            {
                // 세로 방향
                if (direction.y > 0)
                    return new Vector2Int(origin.x, origin.y + 1);  // 위
                else
                    return new Vector2Int(origin.x, origin.y - 1);  // 아래
            }
        }

        // ========== 선택 로직 ==========

        /// <summary>
        /// 퍼즐 선택
        /// </summary>
        private void SelectPiece(Vector2Int gridPos)
        {
            _selectedPiece = gridPos;
            HighlightPiece(gridPos, true);
            LogDebug($"Piece selected: {gridPos}");
        }

        /// <summary>
        /// 교체 시도
        /// </summary>
        private void TrySwap(Vector2Int gridPos)
        {
            if (!_selectedPiece.HasValue)
            {
                SelectPiece(gridPos);
                return;
            }

            // 같은 퍼즐 선택 시 선택 해제
            if (_selectedPiece.Value == gridPos)
            {
                ClearSelection();
                return;
            }

            // 인접 확인
            if (IsAdjacent(_selectedPiece.Value, gridPos))
            {
                // 교체 요청 이벤트 발생
                OnSwapRequested?.Invoke(_selectedPiece.Value, gridPos);
                LogDebug($"Swap requested: {_selectedPiece.Value} <-> {gridPos}");
            }
            else
            {
                // 인접하지 않으면 새로운 퍼즐 선택
                ClearSelection();
                SelectPiece(gridPos);
            }
        }

        /// <summary>
        /// 인접 여부 확인
        /// </summary>
        private bool IsAdjacent(Vector2Int pos1, Vector2Int pos2)
        {
            int dx = Mathf.Abs(pos1.x - pos2.x);
            int dy = Mathf.Abs(pos1.y - pos2.y);

            // 상하좌우만 인접으로 인정 (대각선 제외)
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        // ========== 상태 관리 ==========

        /// <summary>
        /// 처리 중 상태 설정
        /// </summary>
        public void SetProcessing(bool processing)
        {
            _isProcessing = processing;
            LogDebug($"Processing: {processing}");
        }

        /// <summary>
        /// 선택 해제
        /// </summary>
        public void ClearSelection()
        {
            if (_selectedPiece.HasValue)
            {
                HighlightPiece(_selectedPiece.Value, false);
                _selectedPiece = null;
                LogDebug("Selection cleared");
            }
        }

        // ========== 시각 피드백 ==========

        /// <summary>
        /// 퍼즐 하이라이트
        /// </summary>
        private void HighlightPiece(Vector2Int gridPos, bool highlight)
        {
            PuzzlePiece piece = GetPieceAtGridPosition(gridPos);
            if (piece == null)
                return;

            SpriteRenderer spriteRenderer = piece.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                return;

            if (highlight)
            {
                // 하이라이트 적용
                _highlightedPieceSpriteRenderer = spriteRenderer;
                _originalHighlightColor = spriteRenderer.color;
                _originalHighlightScale = piece.transform.localScale;

                spriteRenderer.color = _selectedHighlightColor;
                piece.transform.localScale = _originalHighlightScale * _highlightScale;
            }
            else
            {
                // 하이라이트 제거
                if (_highlightedPieceSpriteRenderer == spriteRenderer)
                {
                    spriteRenderer.color = _originalHighlightColor;
                    piece.transform.localScale = _originalHighlightScale;
                    _highlightedPieceSpriteRenderer = null;
                }
            }
        }

        // ========== 유틸리티 ==========

        /// <summary>
        /// 스크린 좌표를 그리드 좌표로 변환
        /// </summary>
        private Vector2Int? ScreenToGridPosition(Vector2 screenPosition)
        {
            if (_mainCamera == null)
                return null;

            Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, 100f, _pieceLayerMask);

            if (hit.collider != null)
            {
                PuzzlePiece piece = hit.collider.GetComponent<PuzzlePiece>();
                if (piece != null)
                {
                    return piece.GridPosition;
                }
            }

            return null;
        }

        /// <summary>
        /// 그리드 위치의 PuzzlePiece 가져오기
        /// </summary>
        private PuzzlePiece GetPieceAtGridPosition(Vector2Int gridPos)
        {
            if (_boardView == null)
                return null;

            // BoardView를 통해 접근 (리플렉션 사용 대신 직접 접근이 필요하면 BoardView에 public 메서드 추가)
            // 현재는 Raycast로 찾는 방식 사용
            Vector3 worldPos = _boardView.transform.position + new Vector3(gridPos.x, gridPos.y, 0);
            Collider2D[] colliders = Physics2D.OverlapPointAll(worldPos, _pieceLayerMask);

            foreach (var collider in colliders)
            {
                PuzzlePiece piece = collider.GetComponent<PuzzlePiece>();
                if (piece != null && piece.GridPosition == gridPos)
                {
                    return piece;
                }
            }

            return null;
        }

        /// <summary>
        /// 디버그 로그 출력
        /// </summary>
        private void LogDebug(string message)
        {
            if (_showDebugLog)
            {
                Debug.Log($"[InputController] {message}");
            }
        }
    }
}

using System.Collections;
using UnityEngine;

namespace ThreeMatch.Board
{
    /// <summary>
    /// 개별 퍼즐 조각 View 컴포넌트
    /// MonoBehaviour 상속, IPoolable 구현 (풀링 지원)
    /// </summary>
    public class PuzzlePiece : MonoBehaviour, IPoolable
    {
        // ========== Inspector 설정 가능한 필드 ==========
        [Header("Components")]
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private ParticleSystem _matchEffect;

        [Header("Animation Settings")]
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _moveDuration = 0.3f;
        [SerializeField] private float _spawnDuration = 0.3f;
        [SerializeField] private float _destroyDuration = 0.4f;

        [Header("Spawn Animation")]
        [SerializeField] private AnimationCurve _spawnScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float _elasticPower = 0.3f;

        [Header("Debug")]
        [SerializeField] private bool _showGridGizmo = true;
        [SerializeField] private float _gizmoSize = 0.1f;

        // ========== Private 필드 ==========
        private int _pieceId;
        private Vector2Int _gridPosition;
        private bool _isAnimating;

        // ========== 프로퍼티 ==========
        public int PieceId => _pieceId;
        public Vector2Int GridPosition => _gridPosition;
        public bool IsAnimating => _isAnimating;

        // ========== 초기화 ==========

        private void Awake()
        {
            // SpriteRenderer가 Inspector에서 설정되지 않았다면 자동으로 찾기
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            // ParticleSystem이 없다면 자동으로 찾기 (선택적 컴포넌트)
            if (_matchEffect == null)
            {
                _matchEffect = GetComponentInChildren<ParticleSystem>();
            }
        }

        /// <summary>
        /// 퍼즐 타입 설정 (스프라이트 변경)
        /// </summary>
        public void SetPieceType(int pieceId, Sprite sprite)
        {
            _pieceId = pieceId;

            if (_spriteRenderer != null && sprite != null)
            {
                _spriteRenderer.sprite = sprite;
            }
        }

        /// <summary>
        /// 그리드 좌표 설정
        /// </summary>
        public void SetGridPosition(int x, int y)
        {
            _gridPosition = new Vector2Int(x, y);
        }

        // ========== 애니메이션 ==========

        /// <summary>
        /// 목표 위치로 이동 애니메이션
        /// </summary>
        public IEnumerator MoveToPosition(Vector3 targetPosition, float duration = -1f)
        {
            _isAnimating = true;
            Vector3 startPosition = transform.position;

            // duration이 지정되지 않으면 Inspector 설정값 사용
            float actualDuration = duration > 0 ? duration : _moveDuration;
            float elapsed = 0f;

            while (elapsed < actualDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / actualDuration);

                // SmoothStep 보간 (부드러운 가속/감속)
                float smoothT = t * t * (3f - 2f * t);

                transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
                yield return null;
            }

            // 정확한 위치로 설정
            transform.position = targetPosition;
            _isAnimating = false;
        }

        /// <summary>
        /// 매치 이펙트 재생
        /// </summary>
        public void PlayMatchEffect()
        {
            if (_matchEffect != null)
            {
                _matchEffect.Play();
            }
        }

        /// <summary>
        /// 생성 이펙트 재생 (새로운 퍼즐이 생성될 때)
        /// </summary>
        public void PlaySpawnEffect()
        {
            StartCoroutine(SpawnScaleAnimation());
        }

        private IEnumerator SpawnScaleAnimation()
        {
            Vector3 originalScale = Vector3.one;
            transform.localScale = Vector3.zero;

            float elapsed = 0f;

            while (elapsed < _spawnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _spawnDuration);

                // AnimationCurve 또는 Elastic 효과 사용
                float scale = _spawnScaleCurve != null && _spawnScaleCurve.length > 0
                    ? _spawnScaleCurve.Evaluate(t)
                    : ElasticEaseOut(t);

                transform.localScale = originalScale * scale;
                yield return null;
            }

            transform.localScale = originalScale;
        }

        /// <summary>
        /// Elastic Ease Out 보간 함수
        /// </summary>
        private float ElasticEaseOut(float t)
        {
            if (t == 0) return 0;
            if (t == 1) return 1;

            float p = _elasticPower;
            float s = p / 4f;
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - s) * (2 * Mathf.PI) / p) + 1;
        }

        /// <summary>
        /// 파괴 애니메이션 (페이드아웃 + 스케일 축소)
        /// </summary>
        public IEnumerator DestroyAnimation(float duration = -1f)
        {
            _isAnimating = true;
            Vector3 originalScale = transform.localScale;
            Color originalColor = _spriteRenderer != null ? _spriteRenderer.color : Color.white;

            // duration이 지정되지 않으면 Inspector 설정값 사용
            float actualDuration = duration > 0 ? duration : _destroyDuration;
            float elapsed = 0f;

            while (elapsed < actualDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / actualDuration);

                // 스케일 축소
                transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

                // 알파 감소
                if (_spriteRenderer != null)
                {
                    Color color = originalColor;
                    color.a = Mathf.Lerp(1f, 0f, t);
                    _spriteRenderer.color = color;
                }

                yield return null;
            }

            // 원상복구 (풀로 반환 시 재사용을 위해)
            transform.localScale = originalScale;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = originalColor;
            }

            _isAnimating = false;
        }

        // ========== IPoolable 구현 ==========

        /// <summary>
        /// 풀에서 생성될 때 호출
        /// </summary>
        public void OnSpawnFromPool()
        {
            gameObject.SetActive(true);
            _isAnimating = false;

            // 스프라이트 초기화
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = Color.white;
            }

            // 스케일 초기화
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// 풀로 반환될 때 호출
        /// </summary>
        public void OnReturnToPool()
        {
            // 실행 중인 코루틴 정지
            StopAllCoroutines();

            // 상태 초기화
            _isAnimating = false;
            _pieceId = 0;
            _gridPosition = new Vector2Int(-1, -1);

            // 이펙트 정지
            if (_matchEffect != null && _matchEffect.isPlaying)
            {
                _matchEffect.Stop();
            }

            gameObject.SetActive(false);
        }

        // ========== 디버깅 ==========

        private void OnDrawGizmos()
        {
            if (!_showGridGizmo)
                return;

            // 그리드 좌표 표시 (Scene 뷰에서)
            if (_gridPosition.x >= 0 && _gridPosition.y >= 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(transform.position, Vector3.one * _gizmoSize);
            }
        }
    }

    /// <summary>
    /// IPoolable 인터페이스 (풀링 지원)
    /// </summary>
    public interface IPoolable
    {
        void OnSpawnFromPool();
        void OnReturnToPool();
    }
}

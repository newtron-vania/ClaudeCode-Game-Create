using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 적 스폰 시스템
    /// 시간 기반으로 난이도가 증가하며 적을 생성합니다.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float _spawnInterval = 2f; // 스폰 주기 (초)
        [SerializeField] private float _spawnDistance = 15f; // 플레이어로부터 스폰 거리
        [SerializeField] private int _maxEnemies = 100; // 최대 적 수

        [Header("Difficulty Scaling")]
        [SerializeField] private float _difficultyIncreaseInterval = 30f; // 난이도 증가 주기 (초)
        [SerializeField] private float _difficultyMultiplierIncrease = 0.1f; // 난이도 증가량

        [Header("Prefabs")]
        [SerializeField] private GameObject _enemyPrefab; // 적 프리팹

        private Player _targetPlayer;
        private UndeadSurvivorDataProvider _dataProvider;

        private float _currentDifficultyMultiplier = 1.0f;
        private float _nextSpawnTime = 0f;
        private float _nextDifficultyIncreaseTime = 0f;

        private List<Enemy> _activeEnemies = new List<Enemy>();

        private bool _isSpawning = false;

        #region Unity Lifecycle

        private void Update()
        {
            if (!_isSpawning) return;

            // 난이도 증가
            if (Time.time >= _nextDifficultyIncreaseTime)
            {
                IncreaseDifficulty();
            }

            // 적 스폰
            if (Time.time >= _nextSpawnTime && _activeEnemies.Count < _maxEnemies)
            {
                SpawnEnemy();
                _nextSpawnTime = Time.time + _spawnInterval;
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 스포너 초기화
        /// </summary>
        /// <param name="targetPlayer">추적 대상 플레이어</param>
        /// <param name="dataProvider">몬스터 데이터 제공자</param>
        public void Initialize(Player targetPlayer, UndeadSurvivorDataProvider dataProvider)
        {
            _targetPlayer = targetPlayer;
            _dataProvider = dataProvider;

            _currentDifficultyMultiplier = 1.0f;
            _nextSpawnTime = Time.time + 1f; // 1초 후 첫 스폰
            _nextDifficultyIncreaseTime = Time.time + _difficultyIncreaseInterval;

            // Enemy 프리팹 로드 (Resources 폴더에서)
            if (_enemyPrefab == null)
            {
                _enemyPrefab = Resources.Load<GameObject>("Prefabs/Monster/UndeadSurvivor/Enemy");

                if (_enemyPrefab == null)
                {
                    Debug.LogError("[ERROR] EnemySpawner::Initialize - Enemy prefab not found at 'Prefabs/Monster/UndeadSurvivor/Enemy'");
                }
                else
                {
                    Debug.Log("[INFO] EnemySpawner::Initialize - Enemy prefab loaded successfully");
                }
            }

            Debug.Log("[INFO] EnemySpawner::Initialize - Spawner initialized");
        }

        /// <summary>
        /// 스폰 시작
        /// </summary>
        public void StartSpawning()
        {
            _isSpawning = true;
            Debug.Log("[INFO] EnemySpawner::StartSpawning - Spawning started");
        }

        /// <summary>
        /// 스폰 중지
        /// </summary>
        public void StopSpawning()
        {
            _isSpawning = false;
            Debug.Log("[INFO] EnemySpawner::StopSpawning - Spawning stopped");
        }

        #endregion

        #region Spawning

        /// <summary>
        /// 적 생성
        /// </summary>
        private void SpawnEnemy()
        {
            if (_targetPlayer == null || !_targetPlayer.IsAlive)
            {
                StopSpawning();
                return;
            }

            // Enemy 프리팹이 없으면 스폰 불가
            if (_enemyPrefab == null)
            {
                Debug.LogError("[ERROR] EnemySpawner::SpawnEnemy - Enemy prefab is null. Cannot spawn enemy.");
                _isSpawning = false; // 스폰 중지
                return;
            }

            // 랜덤 몬스터 선택 (ID 1-4)
            int randomMonsterId = Random.Range(1, 5);
            MonsterData monsterData = _dataProvider.GetMonsterData(randomMonsterId);

            if (monsterData == null)
            {
                Debug.LogError($"[ERROR] EnemySpawner::SpawnEnemy - Monster ID {randomMonsterId} not found");
                return;
            }

            // 스폰 위치 계산 (플레이어 주변 랜덤)
            Vector2 spawnPosition = GetRandomSpawnPosition();

            // 적 생성
            GameObject enemyObj = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
            Enemy enemy = enemyObj.GetComponent<Enemy>();

            if (enemy == null)
            {
                Debug.LogError("[ERROR] EnemySpawner::SpawnEnemy - Enemy component not found on prefab");
                Destroy(enemyObj);
                return;
            }

            // 적 초기화
            enemy.Initialize(monsterData, _currentDifficultyMultiplier, _targetPlayer);

            // 사망 이벤트 구독 (경험치 드롭)
            enemy.OnDeath += HandleEnemyDeath;

            // 활성 적 리스트에 추가
            _activeEnemies.Add(enemy);

            Debug.Log($"[INFO] EnemySpawner::SpawnEnemy - Spawned {monsterData.Name} at {spawnPosition} (Active: {_activeEnemies.Count}/{_maxEnemies})");
        }

        /// <summary>
        /// 랜덤 스폰 위치 계산 (플레이어 주변)
        /// </summary>
        private Vector2 GetRandomSpawnPosition()
        {
            Vector2 playerPos = _targetPlayer.transform.position;

            // 플레이어 기준 원형 범위에서 랜덤 각도
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            // 스폰 거리만큼 떨어진 위치
            Vector2 offset = new Vector2(
                Mathf.Cos(randomAngle) * _spawnDistance,
                Mathf.Sin(randomAngle) * _spawnDistance
            );

            return playerPos + offset;
        }

        #endregion

        #region Difficulty Scaling

        /// <summary>
        /// 난이도 증가
        /// </summary>
        private void IncreaseDifficulty()
        {
            _currentDifficultyMultiplier += _difficultyMultiplierIncrease;
            _nextDifficultyIncreaseTime = Time.time + _difficultyIncreaseInterval;

            Debug.Log($"[INFO] EnemySpawner::IncreaseDifficulty - Difficulty increased to {_currentDifficultyMultiplier:F2}x");
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 적 사망 처리 (경험치 드롭)
        /// </summary>
        private void HandleEnemyDeath(Enemy enemy)
        {
            // 활성 적 리스트에서 제거
            _activeEnemies.Remove(enemy);

            // 경험치 계산 (기본 10 * 경험치 배율)
            int expAmount = Mathf.RoundToInt(10 * enemy.ExpMultiplier);

            // 플레이어에게 경험치 지급
            if (_targetPlayer != null && _targetPlayer.IsAlive)
            {
                _targetPlayer.GainExp(expAmount);
            }

            Debug.Log($"[INFO] EnemySpawner::HandleEnemyDeath - {enemy.MonsterData.Name} died, dropped {expAmount} EXP (Active: {_activeEnemies.Count})");
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// 모든 적 제거
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemy in _activeEnemies)
            {
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            _activeEnemies.Clear();
            Debug.Log("[INFO] EnemySpawner::ClearAllEnemies - All enemies cleared");
        }

        private void OnDestroy()
        {
            ClearAllEnemies();
        }

        #endregion
    }
}

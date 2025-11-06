using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// Undead Survivor 게임 로직 구현
    /// IMiniGame 인터페이스를 통해 MiniGameManager와 통합됩니다.
    /// </summary>
    public class UndeadSurvivorGame : IMiniGame
    {
        private UndeadSurvivorGameData _gameData;
        private CommonPlayerData _commonData;
        private UndeadSurvivorDataProvider _dataProvider;
        private UndeadSurvivorInputAdapter _inputAdapter;

        private Player _player;
        private Transform _playerSpawnPoint;

        // 게임 진행 관련
        private float _gameTime;
        private bool _isInitialized;

        #region IMiniGame Implementation

        /// <summary>
        /// 게임 초기화
        /// </summary>
        public void Initialize(CommonPlayerData commonData)
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[WARNING] UndeadSurvivor::Game::Initialize - Already initialized");
                return;
            }

            _commonData = commonData;
            _gameData = new UndeadSurvivorGameData();
            _gameData.Initialize();

            // DataManager에서 UndeadSurvivorDataProvider 가져오기
            _dataProvider = DataManager.Instance.GetProvider<UndeadSurvivorDataProvider>("UndeadSurvivor");

            if (_dataProvider == null || !_dataProvider.IsLoaded)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::Game::Initialize - DataProvider not loaded");
                return;
            }

            // InputAdapter 찾기
            _inputAdapter = Object.FindObjectOfType<UndeadSurvivorInputAdapter>();
            if (_inputAdapter == null)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::Game::Initialize - UndeadSurvivorInputAdapter not found in scene");
                return;
            }

            _gameTime = 0f;
            _isInitialized = true;

            Debug.Log("[INFO] UndeadSurvivor::Game::Initialize - Game initialized");
        }

        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            if (!_isInitialized)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::Game::StartGame - Not initialized");
                return;
            }

            // 플레이어 생성 및 초기화
            SpawnPlayer();

            // 게임 상태를 Playing으로 변경
            _gameData.CurrentGameState = UndeadSurvivorGameData.GameState.Playing;

            // 게임 시작 이벤트 구독
            if (_player != null)
            {
                _player.OnPlayerDeath += HandlePlayerDeath;
                _player.OnPlayerLevelUp += HandlePlayerLevelUp;
            }

            // TODO Phase 2: 적 스폰 시작
            // TODO Phase 3: BGM 재생

            Debug.Log("[INFO] UndeadSurvivor::Game::StartGame - Game started");
        }

        /// <summary>
        /// 매 프레임 게임 로직 실행
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!_isInitialized || !_gameData.IsPlaying)
            {
                return;
            }

            // 게임 시간 업데이트
            _gameTime += deltaTime;
            _gameData.SurvivalTime = _gameTime;

            // TODO Phase 2: 적 스폰 로직
            // TODO Phase 2: 시간 기반 난이도 증가
            // TODO Phase 4: 보스 스폰 (30초, 1분, 2분, 3분, 5분)
        }

        /// <summary>
        /// 게임 정리
        /// </summary>
        public void Cleanup()
        {
            if (!_isInitialized)
            {
                return;
            }

            // 이벤트 구독 해제
            if (_player != null)
            {
                _player.OnPlayerDeath -= HandlePlayerDeath;
                _player.OnPlayerLevelUp -= HandlePlayerLevelUp;
            }

            // 플레이어 제거
            if (_player != null)
            {
                Object.Destroy(_player.gameObject);
                _player = null;
            }

            // TODO Phase 2: 모든 적 제거
            // TODO Phase 3: 사운드 정지

            _isInitialized = false;

            Debug.Log("[INFO] UndeadSurvivor::Game::Cleanup - Game cleaned up");
        }

        /// <summary>
        /// 게임 데이터 반환
        /// </summary>
        public IGameData GetData()
        {
            return _gameData;
        }

        #endregion

        #region Player Management

        /// <summary>
        /// 플레이어 생성 및 초기화
        /// </summary>
        private void SpawnPlayer()
        {
            // 플레이어 스폰 포인트 찾기 (없으면 (0, 0, 0))
            GameObject spawnPointObj = GameObject.Find("PlayerSpawnPoint");
            _playerSpawnPoint = spawnPointObj != null ? spawnPointObj.transform : null;
            Vector3 spawnPosition = _playerSpawnPoint != null ? _playerSpawnPoint.position : Vector3.zero;

            // 플레이어 프리팹 로드 및 생성
            // TODO: Addressables로 변경
            GameObject playerPrefab = Resources.Load<GameObject>("Prefabs/Player/UndeadSurvivor/Player");

            if (playerPrefab == null)
            {
                // 프리팹이 없으면 씬에서 찾기
                _player = Object.FindObjectOfType<Player>();

                if (_player == null)
                {
                    Debug.LogError("[ERROR] UndeadSurvivor::Game::SpawnPlayer - Player prefab not found and no Player in scene");
                    return;
                }

                Debug.Log("[INFO] UndeadSurvivor::Game::SpawnPlayer - Using existing Player from scene");
            }
            else
            {
                GameObject playerObj = Object.Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
                _player = playerObj.GetComponent<Player>();
            }

            // 캐릭터 데이터 로드 및 플레이어 초기화
            int characterId = _gameData.SelectedCharacterId;
            CharacterData characterData = _dataProvider.GetCharacterData(characterId);

            if (characterData == null)
            {
                Debug.LogError($"[ERROR] UndeadSurvivor::Game::SpawnPlayer - Character ID {characterId} not found");
                return;
            }

            _player.Initialize(characterData);

            // 게임 데이터에 플레이어 초기 정보 저장
            _gameData.PlayerLevel = _player.Level;
            _gameData.PlayerCurrentHp = _player.CurrentHp;
            _gameData.PlayerMaxHp = _player.MaxHp;

            Debug.Log($"[INFO] UndeadSurvivor::Game::SpawnPlayer - Player spawned at {spawnPosition} with character {characterData.Name}");
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 플레이어 사망 이벤트 핸들러
        /// </summary>
        private void HandlePlayerDeath()
        {
            _gameData.CurrentGameState = UndeadSurvivorGameData.GameState.GameOver;

            Debug.Log($"[INFO] UndeadSurvivor::Game::HandlePlayerDeath - Game Over! Survived: {_gameData.SurvivalTime:F1}s, Kills: {_gameData.KillCount}");

            // TODO: 게임 오버 UI 표시
            // TODO: 통계 표시
        }

        /// <summary>
        /// 플레이어 레벨업 이벤트 핸들러
        /// </summary>
        private void HandlePlayerLevelUp(int newLevel)
        {
            _gameData.CurrentGameState = UndeadSurvivorGameData.GameState.LevelUp;
            _gameData.PlayerLevel = newLevel;

            Debug.Log($"[INFO] UndeadSurvivor::Game::HandlePlayerLevelUp - Player reached level {newLevel}");

            // TODO: 레벨업 UI 표시 (4지선다)
            // TODO: Time.timeScale = 0 또는 이동 중지
        }

        /// <summary>
        /// 레벨업 선택 완료 (UI에서 호출)
        /// </summary>
        public void OnLevelUpChoiceSelected()
        {
            if (_gameData.CurrentGameState != UndeadSurvivorGameData.GameState.LevelUp)
            {
                return;
            }

            _gameData.CurrentGameState = UndeadSurvivorGameData.GameState.Playing;

            // 플레이어 이동 재개
            if (_player != null)
            {
                _player.ResumeMovement();
            }

            Debug.Log("[INFO] UndeadSurvivor::Game::OnLevelUpChoiceSelected - Resuming game after level up");
        }

        #endregion

        #region Public API

        /// <summary>
        /// 현재 플레이어 참조 가져오기
        /// </summary>
        public Player GetPlayer()
        {
            return _player;
        }

        /// <summary>
        /// 게임 데이터 참조 가져오기
        /// </summary>
        public UndeadSurvivorGameData GetGameData()
        {
            return _gameData;
        }

        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void PauseGame()
        {
            if (_gameData.IsPlaying)
            {
                _gameData.CurrentGameState = UndeadSurvivorGameData.GameState.Paused;
                Debug.Log("[INFO] UndeadSurvivor::Game::PauseGame - Game paused");
            }
        }

        /// <summary>
        /// 게임 재개
        /// </summary>
        public void ResumeGame()
        {
            if (_gameData.CurrentGameState == UndeadSurvivorGameData.GameState.Paused)
            {
                _gameData.CurrentGameState = UndeadSurvivorGameData.GameState.Playing;
                Debug.Log("[INFO] UndeadSurvivor::Game::ResumeGame - Game resumed");
            }
        }

        /// <summary>
        /// 적 처치 기록
        /// </summary>
        public void OnEnemyKilled(bool isBoss)
        {
            _gameData.AddKill(isBoss);
        }

        #endregion
    }
}

using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// Undead Survivor 게임 세션 데이터
    /// 현재 게임 진행 상황을 저장하고 관리합니다.
    /// </summary>
    [System.Serializable]
    public class UndeadSurvivorGameData : IGameData
    {
        /// <summary>
        /// 게임 상태
        /// </summary>
        public enum GameState
        {
            NotStarted,     // 게임 시작 전
            Playing,        // 게임 진행 중
            Paused,         // 일시정지
            LevelUp,        // 레벨업 선택 중
            GameOver,       // 게임 오버 (패배)
            Victory         // 승리
        }

        #region Fields

        // 게임 상태
        [SerializeField] private GameState _gameState;

        // 플레이어 정보
        [SerializeField] private int _selectedCharacterId;
        [SerializeField] private int _playerLevel;
        [SerializeField] private float _playerCurrentHp;
        [SerializeField] private float _playerMaxHp;

        // 게임 진행 정보
        [SerializeField] private float _survivalTime;          // 생존 시간 (초)
        [SerializeField] private int _killCount;               // 총 처치 수
        [SerializeField] private int _bossKillCount;           // 보스 처치 수
        [SerializeField] private int _expGained;               // 획득 경험치

        // 통계
        [SerializeField] private float _totalDamageDealt;      // 총 준 피해
        [SerializeField] private float _totalDamageTaken;      // 총 받은 피해
        [SerializeField] private int _totalHealing;            // 총 회복량

        #endregion

        #region Properties

        /// <summary>현재 게임 상태</summary>
        public GameState CurrentGameState
        {
            get => _gameState;
            set => _gameState = value;
        }

        /// <summary>선택한 캐릭터 ID</summary>
        public int SelectedCharacterId
        {
            get => _selectedCharacterId;
            set => _selectedCharacterId = value;
        }

        /// <summary>플레이어 레벨</summary>
        public int PlayerLevel
        {
            get => _playerLevel;
            set => _playerLevel = value;
        }

        /// <summary>플레이어 현재 체력</summary>
        public float PlayerCurrentHp
        {
            get => _playerCurrentHp;
            set => _playerCurrentHp = Mathf.Max(0f, value);
        }

        /// <summary>플레이어 최대 체력</summary>
        public float PlayerMaxHp
        {
            get => _playerMaxHp;
            set => _playerMaxHp = Mathf.Max(1f, value);
        }

        /// <summary>생존 시간 (초)</summary>
        public float SurvivalTime
        {
            get => _survivalTime;
            set => _survivalTime = Mathf.Max(0f, value);
        }

        /// <summary>처치 수</summary>
        public int KillCount
        {
            get => _killCount;
            set => _killCount = Mathf.Max(0, value);
        }

        /// <summary>보스 처치 수</summary>
        public int BossKillCount
        {
            get => _bossKillCount;
            set => _bossKillCount = Mathf.Max(0, value);
        }

        /// <summary>획득 경험치</summary>
        public int ExpGained
        {
            get => _expGained;
            set => _expGained = Mathf.Max(0, value);
        }

        /// <summary>총 준 피해</summary>
        public float TotalDamageDealt
        {
            get => _totalDamageDealt;
            set => _totalDamageDealt = Mathf.Max(0f, value);
        }

        /// <summary>총 받은 피해</summary>
        public float TotalDamageTaken
        {
            get => _totalDamageTaken;
            set => _totalDamageTaken = Mathf.Max(0f, value);
        }

        /// <summary>총 회복량</summary>
        public int TotalHealing
        {
            get => _totalHealing;
            set => _totalHealing = Mathf.Max(0, value);
        }

        /// <summary>게임이 진행 중인지 여부</summary>
        public bool IsPlaying => _gameState == GameState.Playing;

        /// <summary>게임이 종료되었는지 여부</summary>
        public bool IsGameOver => _gameState == GameState.GameOver || _gameState == GameState.Victory;

        #endregion

        #region IGameData Implementation

        /// <summary>
        /// 게임 데이터 초기화
        /// </summary>
        public void Initialize()
        {
            _gameState = GameState.NotStarted;
            _selectedCharacterId = 1; // 기본값: Knight

            _playerLevel = 1;
            _playerCurrentHp = 100f;
            _playerMaxHp = 100f;

            _survivalTime = 0f;
            _killCount = 0;
            _bossKillCount = 0;
            _expGained = 0;

            _totalDamageDealt = 0f;
            _totalDamageTaken = 0f;
            _totalHealing = 0;

            Debug.Log("[INFO] UndeadSurvivor::GameData::Initialize - Game data initialized");
        }

        /// <summary>
        /// 게임 데이터 리셋
        /// </summary>
        public void Reset()
        {
            Initialize();
            Debug.Log("[INFO] UndeadSurvivor::GameData::Reset - Game data reset");
        }

        /// <summary>
        /// 게임 데이터 유효성 검증
        /// </summary>
        public bool Validate()
        {
            bool isValid = true;

            if (_playerMaxHp <= 0f)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::GameData::Validate - PlayerMaxHp must be greater than 0");
                isValid = false;
            }

            if (_playerCurrentHp < 0f || _playerCurrentHp > _playerMaxHp)
            {
                Debug.LogError($"[ERROR] UndeadSurvivor::GameData::Validate - PlayerCurrentHp ({_playerCurrentHp}) out of range [0, {_playerMaxHp}]");
                isValid = false;
            }

            if (_playerLevel < 1)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::GameData::Validate - PlayerLevel must be at least 1");
                isValid = false;
            }

            if (_survivalTime < 0f)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::GameData::Validate - SurvivalTime cannot be negative");
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// 게임 상태 저장 (PlayerPrefs 사용)
        /// </summary>
        public void SaveState()
        {
            string json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString("UndeadSurvivor_GameData", json);
            PlayerPrefs.Save();

            Debug.Log("[INFO] UndeadSurvivor::GameData::SaveState - Game state saved");
        }

        /// <summary>
        /// 게임 상태 로드 (PlayerPrefs 사용)
        /// </summary>
        public void LoadState()
        {
            if (PlayerPrefs.HasKey("UndeadSurvivor_GameData"))
            {
                string json = PlayerPrefs.GetString("UndeadSurvivor_GameData");
                JsonUtility.FromJsonOverwrite(json, this);

                Debug.Log("[INFO] UndeadSurvivor::GameData::LoadState - Game state loaded");
            }
            else
            {
                Debug.LogWarning("[WARNING] UndeadSurvivor::GameData::LoadState - No saved game data found");
                Initialize();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 처치 수 증가
        /// </summary>
        /// <param name="isBoss">보스 여부</param>
        public void AddKill(bool isBoss = false)
        {
            _killCount++;
            if (isBoss)
            {
                _bossKillCount++;
            }
        }

        /// <summary>
        /// 경험치 획득 기록
        /// </summary>
        public void AddExpGained(int exp)
        {
            _expGained += exp;
        }

        /// <summary>
        /// 피해를 준 기록
        /// </summary>
        public void AddDamageDealt(float damage)
        {
            _totalDamageDealt += damage;
        }

        /// <summary>
        /// 피해를 받은 기록
        /// </summary>
        public void AddDamageTaken(float damage)
        {
            _totalDamageTaken += damage;
        }

        /// <summary>
        /// 회복 기록
        /// </summary>
        public void AddHealing(float healing)
        {
            _totalHealing += Mathf.RoundToInt(healing);
        }

        /// <summary>
        /// 게임 통계 문자열
        /// </summary>
        public string GetStatsSummary()
        {
            int minutes = Mathf.FloorToInt(_survivalTime / 60f);
            int seconds = Mathf.FloorToInt(_survivalTime % 60f);

            return $"Survival Time: {minutes:D2}:{seconds:D2}\n" +
                   $"Kills: {_killCount} (Boss: {_bossKillCount})\n" +
                   $"Level: {_playerLevel} | Exp: {_expGained}\n" +
                   $"Damage Dealt: {_totalDamageDealt:F0} | Taken: {_totalDamageTaken:F0}\n" +
                   $"Healing: {_totalHealing}";
        }

        #endregion
    }
}

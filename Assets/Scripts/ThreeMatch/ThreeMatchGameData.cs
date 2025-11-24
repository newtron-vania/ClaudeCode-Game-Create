using UnityEngine;
using ThreeMatch.Data;

namespace ThreeMatch
{
    /// <summary>
    /// ThreeMatch 런타임 게임 데이터 (IGameData 구현)
    /// 게임 진행 중 변경되는 상태 관리
    /// </summary>
    public class ThreeMatchGameData : IGameData
    {
        // 점수 관련
        public int Score { get; set; }
        public int TargetScore { get; set; }

        // 콤보 관련
        public int CurrentCombo { get; set; }
        public int MaxCombo { get; set; }

        // 게임 진행 관련
        public int RemainingMoves { get; set; }
        public float ElapsedTime { get; set; }
        public float TimeLimit { get; set; }

        // 게임 설정
        public DifficultyLevel CurrentDifficulty { get; set; }
        public GameMode CurrentMode { get; set; }

        // 보드 크기
        public int BoardWidth { get; set; }
        public int BoardHeight { get; set; }

        /// <summary>
        /// 초기화: 기본값 설정
        /// </summary>
        public void Initialize()
        {
            Score = 0;
            TargetScore = 0;
            CurrentCombo = 0;
            MaxCombo = 0;
            RemainingMoves = 0;
            ElapsedTime = 0f;
            TimeLimit = 0f;
            CurrentDifficulty = DifficultyLevel.Normal;
            CurrentMode = GameMode.Classic;
            BoardWidth = 7;
            BoardHeight = 7;

            Debug.Log("[ThreeMatchGameData] Initialized");
        }

        /// <summary>
        /// 리셋: 초기화와 동일
        /// </summary>
        public void Reset()
        {
            Initialize();
            Debug.Log("[ThreeMatchGameData] Reset");
        }

        /// <summary>
        /// 검증: 데이터 유효성 확인
        /// </summary>
        public bool Validate()
        {
            bool isValid = Score >= 0 &&
                           TargetScore > 0 &&
                           ElapsedTime >= 0f &&
                           BoardWidth > 0 &&
                           BoardHeight > 0;

            if (!isValid)
            {
                Debug.LogError("[ThreeMatchGameData] Validation failed!");
            }

            return isValid;
        }

        /// <summary>
        /// 상태 저장: PlayerPrefs에 하이스코어 및 최대 콤보 저장
        /// </summary>
        public void SaveState()
        {
            int currentHighScore = PlayerPrefs.GetInt("ThreeMatch_HighScore", 0);
            int currentMaxCombo = PlayerPrefs.GetInt("ThreeMatch_MaxCombo", 0);

            if (Score > currentHighScore)
            {
                PlayerPrefs.SetInt("ThreeMatch_HighScore", Score);
                Debug.Log($"[ThreeMatchGameData] New High Score: {Score}");
            }

            if (MaxCombo > currentMaxCombo)
            {
                PlayerPrefs.SetInt("ThreeMatch_MaxCombo", MaxCombo);
                Debug.Log($"[ThreeMatchGameData] New Max Combo: {MaxCombo}");
            }

            PlayerPrefs.Save();
            Debug.Log("[ThreeMatchGameData] State saved");
        }

        /// <summary>
        /// 상태 로드: PlayerPrefs에서 하이스코어 로드
        /// </summary>
        public void LoadState()
        {
            int highScore = PlayerPrefs.GetInt("ThreeMatch_HighScore", 0);
            int maxCombo = PlayerPrefs.GetInt("ThreeMatch_MaxCombo", 0);

            Debug.Log($"[ThreeMatchGameData] Loaded - High Score: {highScore}, Max Combo: {maxCombo}");
        }

        /// <summary>
        /// 하이스코어 조회
        /// </summary>
        public int GetHighScore()
        {
            return PlayerPrefs.GetInt("ThreeMatch_HighScore", 0);
        }

        /// <summary>
        /// 최대 콤보 조회
        /// </summary>
        public int GetMaxComboRecord()
        {
            return PlayerPrefs.GetInt("ThreeMatch_MaxCombo", 0);
        }

        /// <summary>
        /// 점수 추가 (콤보 배율 적용)
        /// </summary>
        public void AddScore(int baseScore)
        {
            int multiplier = Mathf.Max(1, CurrentCombo);
            int finalScore = baseScore * multiplier;
            Score += finalScore;

            Debug.Log($"[ThreeMatchGameData] Score added: {baseScore} x {multiplier} = {finalScore} (Total: {Score})");
        }

        /// <summary>
        /// 콤보 증가
        /// </summary>
        public void IncrementCombo()
        {
            CurrentCombo++;
            if (CurrentCombo > MaxCombo)
            {
                MaxCombo = CurrentCombo;
            }

            Debug.Log($"[ThreeMatchGameData] Combo: {CurrentCombo} (Max: {MaxCombo})");
        }

        /// <summary>
        /// 콤보 리셋
        /// </summary>
        public void ResetCombo()
        {
            CurrentCombo = 0;
            Debug.Log("[ThreeMatchGameData] Combo reset");
        }

        /// <summary>
        /// 시간 업데이트 (클래식 모드)
        /// </summary>
        public void UpdateTime(float deltaTime)
        {
            ElapsedTime += deltaTime;
        }

        /// <summary>
        /// 남은 시간 조회 (클래식 모드)
        /// </summary>
        public float GetRemainingTime()
        {
            return Mathf.Max(0, TimeLimit - ElapsedTime);
        }

        /// <summary>
        /// 시간 종료 여부 (클래식 모드)
        /// </summary>
        public bool IsTimeUp()
        {
            return CurrentMode == GameMode.Classic && ElapsedTime >= TimeLimit;
        }

        /// <summary>
        /// 이동 횟수 감소 (이동 횟수 모드)
        /// </summary>
        public void DecrementMoves()
        {
            if (RemainingMoves > 0)
            {
                RemainingMoves--;
                Debug.Log($"[ThreeMatchGameData] Moves remaining: {RemainingMoves}");
            }
        }

        /// <summary>
        /// 이동 횟수 소진 여부 (이동 횟수 모드)
        /// </summary>
        public bool IsMovesExhausted()
        {
            return CurrentMode == GameMode.MovesLimited && RemainingMoves <= 0;
        }

        /// <summary>
        /// 목표 달성 여부
        /// </summary>
        public bool IsGoalAchieved()
        {
            return Score >= TargetScore;
        }
    }
}

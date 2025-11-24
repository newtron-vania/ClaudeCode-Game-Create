using System.Collections.Generic;
using UnityEngine;

namespace ThreeMatch.Data
{
    /// <summary>
    /// ThreeMatch 게임 데이터 제공자 (IGameDataProvider 구현)
    /// ScriptableObject에서 게임 데이터를 로드하고 관리
    /// </summary>
    public class ThreeMatchDataProvider : IGameDataProvider
    {
        public string GameID => "ThreeMatch";
        public bool IsLoaded { get; private set; }

        // 데이터 딕셔너리
        private Dictionary<int, PieceTypeData> _pieceTypes;
        private Dictionary<DifficultyLevel, DifficultyConfig> _difficultyConfigs;
        private Dictionary<GameMode, GameModeConfig> _gameModeConfigs;

        /// <summary>
        /// 초기화: 데이터 구조 생성
        /// </summary>
        public void Initialize()
        {
            _pieceTypes = new Dictionary<int, PieceTypeData>();
            _difficultyConfigs = new Dictionary<DifficultyLevel, DifficultyConfig>();
            _gameModeConfigs = new Dictionary<GameMode, GameModeConfig>();
            IsLoaded = false;

            Debug.Log("[ThreeMatchDataProvider] Initialized");
        }

        /// <summary>
        /// 데이터 로드: Resources에서 ScriptableObject 로드
        /// </summary>
        public void LoadData()
        {
            if (IsLoaded)
            {
                Debug.LogWarning("[ThreeMatchDataProvider] Data already loaded");
                return;
            }

            // PieceTypeDataList 로드
            var pieceList = Resources.Load<PieceTypeDataList>("Data/ThreeMatch/ScriptableObjects/PieceTypeDataList");
            if (pieceList != null)
            {
                foreach (var piece in pieceList.PieceTypes)
                {
                    if (!_pieceTypes.ContainsKey(piece.PieceId))
                    {
                        _pieceTypes.Add(piece.PieceId, piece);
                    }
                }
                Debug.Log($"[ThreeMatchDataProvider] Loaded {_pieceTypes.Count} piece types");
            }
            else
            {
                Debug.LogError("[ThreeMatchDataProvider] PieceTypeDataList not found at Resources/Data/ThreeMatch/ScriptableObjects/PieceTypeDataList");
            }

            // DifficultyConfigList 로드
            var difficultyList = Resources.Load<DifficultyConfigList>("Data/ThreeMatch/ScriptableObjects/DifficultyConfigList");
            if (difficultyList != null)
            {
                foreach (var config in difficultyList.Configs)
                {
                    if (!_difficultyConfigs.ContainsKey(config.Level))
                    {
                        _difficultyConfigs.Add(config.Level, config);
                    }
                }
                Debug.Log($"[ThreeMatchDataProvider] Loaded {_difficultyConfigs.Count} difficulty configs");
            }
            else
            {
                Debug.LogError("[ThreeMatchDataProvider] DifficultyConfigList not found at Resources/Data/ThreeMatch/ScriptableObjects/DifficultyConfigList");
            }

            // GameModeConfigList 로드
            var modeList = Resources.Load<GameModeConfigList>("Data/ThreeMatch/ScriptableObjects/GameModeConfigList");
            if (modeList != null)
            {
                foreach (var mode in modeList.Modes)
                {
                    if (!_gameModeConfigs.ContainsKey(mode.Mode))
                    {
                        _gameModeConfigs.Add(mode.Mode, mode);
                    }
                }
                Debug.Log($"[ThreeMatchDataProvider] Loaded {_gameModeConfigs.Count} game mode configs");
            }
            else
            {
                Debug.LogError("[ThreeMatchDataProvider] GameModeConfigList not found at Resources/Data/ThreeMatch/ScriptableObjects/GameModeConfigList");
            }

            IsLoaded = true;
            Debug.Log("[ThreeMatchDataProvider] Data loaded successfully");
        }

        /// <summary>
        /// 데이터 언로드: 메모리 정리
        /// </summary>
        public void UnloadData()
        {
            _pieceTypes.Clear();
            _difficultyConfigs.Clear();
            _gameModeConfigs.Clear();
            IsLoaded = false;

            Debug.Log("[ThreeMatchDataProvider] Data unloaded");
        }

        /// <summary>
        /// 제네릭 데이터 접근 (IGameDataProvider 인터페이스 구현)
        /// </summary>
        public T GetData<T>(string key) where T : class
        {
            // ThreeMatch는 게임별 메서드 사용 권장
            Debug.LogWarning("[ThreeMatchDataProvider] Generic GetData is not recommended. Use specific methods instead.");
            return null;
        }

        /// <summary>
        /// 데이터 존재 확인 (IGameDataProvider 인터페이스 구현)
        /// </summary>
        public bool HasData(string key)
        {
            // ThreeMatch는 게임별 메서드 사용 권장
            return false;
        }

        // ========================================
        // 게임별 데이터 접근 메서드
        // ========================================

        /// <summary>
        /// 퍼즐 타입 데이터 조회 (ID로)
        /// </summary>
        public PieceTypeData GetPieceTypeData(int pieceId)
        {
            if (!IsLoaded)
            {
                Debug.LogError("[ThreeMatchDataProvider] Data not loaded. Call LoadData() first.");
                return null;
            }

            return _pieceTypes.TryGetValue(pieceId, out var data) ? data : null;
        }

        /// <summary>
        /// 난이도 설정 조회
        /// </summary>
        public DifficultyConfig GetDifficultyConfig(DifficultyLevel level)
        {
            if (!IsLoaded)
            {
                Debug.LogError("[ThreeMatchDataProvider] Data not loaded. Call LoadData() first.");
                return null;
            }

            return _difficultyConfigs.TryGetValue(level, out var config) ? config : null;
        }

        /// <summary>
        /// 게임 모드 설정 조회
        /// </summary>
        public GameModeConfig GetGameModeConfig(GameMode mode)
        {
            if (!IsLoaded)
            {
                Debug.LogError("[ThreeMatchDataProvider] Data not loaded. Call LoadData() first.");
                return null;
            }

            return _gameModeConfigs.TryGetValue(mode, out var config) ? config : null;
        }

        /// <summary>
        /// 모든 퍼즐 타입 데이터 조회
        /// </summary>
        public Dictionary<int, PieceTypeData> GetAllPieceTypes()
        {
            if (!IsLoaded)
            {
                Debug.LogError("[ThreeMatchDataProvider] Data not loaded. Call LoadData() first.");
                return null;
            }

            return _pieceTypes;
        }
    }
}

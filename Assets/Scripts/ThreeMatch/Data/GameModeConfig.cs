using UnityEngine;

namespace ThreeMatch.Data
{
    /// <summary>
    /// 게임 모드 열거형
    /// </summary>
    public enum GameMode
    {
        Classic,        // 시간 제한 모드
        MovesLimited,   // 이동 횟수 제한 모드
        Endless         // 무한 모드
    }

    /// <summary>
    /// 게임 모드 설정 데이터 (ScriptableObject)
    /// 각 게임 모드의 특성 정의
    /// </summary>
    [CreateAssetMenu(fileName = "GameModeConfig", menuName = "ThreeMatch/GameModeConfig")]
    public class GameModeConfig : ScriptableObject
    {
        [Header("Mode Information")]
        [Tooltip("게임 모드")]
        public GameMode Mode;

        [Tooltip("모드 이름 (UI 표시용)")]
        public string ModeName;

        [Tooltip("모드 설명")]
        [TextArea(2, 4)]
        public string Description;

        [Header("Mode Features")]
        [Tooltip("시간 제한 있음")]
        public bool HasTimeLimit;

        [Tooltip("시간 제한 (초)")]
        public float TimeLimit = 60f;

        [Tooltip("이동 횟수 제한 있음")]
        public bool HasMovesLimit;

        [Tooltip("이동 횟수 제한")]
        public int MovesLimit = 20;
    }
}

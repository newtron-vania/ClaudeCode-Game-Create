using UnityEngine;

namespace ThreeMatch.Data
{
    /// <summary>
    /// 난이도 레벨 열거형
    /// </summary>
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }

    /// <summary>
    /// 난이도 설정 데이터 (ScriptableObject)
    /// 난이도별 보드 크기, 퍼즐 종류, 목표 점수, 제한 시간/이동 정의
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "ThreeMatch/DifficultyConfig")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Difficulty Settings")]
        [Tooltip("난이도 레벨")]
        public DifficultyLevel Level;

        [Header("Board Settings")]
        [Tooltip("보드 가로 크기")]
        public int BoardWidth = 6;

        [Tooltip("보드 세로 크기")]
        public int BoardHeight = 6;

        [Tooltip("사용할 퍼즐 종류 수 (5~7)")]
        public int PieceTypeCount = 5;

        [Header("Game Goals")]
        [Tooltip("목표 점수 (클래식 모드)")]
        public int TargetScore = 1000;

        [Header("Limits")]
        [Tooltip("시간 제한 (초, 클래식 모드)")]
        public float TimeLimit = 90f;

        [Tooltip("이동 횟수 제한 (이동 횟수 모드)")]
        public int MovesLimit = 30;
    }
}

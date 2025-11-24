using UnityEngine;

namespace ThreeMatch.Data
{
    /// <summary>
    /// 퍼즐 타입 데이터 (ScriptableObject)
    /// 각 퍼즐의 ID, 이름, 스프라이트, 색상 정의
    /// </summary>
    [CreateAssetMenu(fileName = "PieceTypeData", menuName = "ThreeMatch/PieceTypeData")]
    public class PieceTypeData : ScriptableObject
    {
        [Header("Piece Information")]
        [Tooltip("퍼즐 고유 ID (1~7)")]
        public int PieceId;

        [Tooltip("퍼즐 이름 (Red, Blue, Green, etc.)")]
        public string PieceName;

        [Header("Visuals")]
        [Tooltip("퍼즐 스프라이트")]
        public Sprite PieceSprite;

        [Tooltip("퍼즐 색상 (UI 표시용)")]
        public Color PieceColor = Color.white;
    }
}

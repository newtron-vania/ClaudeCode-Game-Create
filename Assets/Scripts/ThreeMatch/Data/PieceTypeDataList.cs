using UnityEngine;
using System.Collections.Generic;

namespace ThreeMatch.Data
{
    /// <summary>
    /// 퍼즐 타입 데이터 리스트 (ScriptableObject)
    /// 모든 퍼즐 타입을 관리하는 컨테이너
    /// </summary>
    [CreateAssetMenu(fileName = "PieceTypeDataList", menuName = "ThreeMatch/PieceTypeDataList")]
    public class PieceTypeDataList : ScriptableObject
    {
        [Header("Piece Types")]
        [Tooltip("모든 퍼즐 타입 리스트 (7종)")]
        public List<PieceTypeData> PieceTypes = new List<PieceTypeData>();

        /// <summary>
        /// ID로 퍼즐 타입 데이터 조회
        /// </summary>
        public PieceTypeData GetPieceTypeById(int pieceId)
        {
            return PieceTypes.Find(p => p.PieceId == pieceId);
        }

        /// <summary>
        /// 이름으로 퍼즐 타입 데이터 조회
        /// </summary>
        public PieceTypeData GetPieceTypeByName(string pieceName)
        {
            return PieceTypes.Find(p => p.PieceName == pieceName);
        }
    }
}

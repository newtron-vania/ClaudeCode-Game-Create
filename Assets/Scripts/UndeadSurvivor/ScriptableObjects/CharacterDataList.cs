using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 캐릭터 데이터 리스트 ScriptableObject
    /// Unity 에디터에서 생성하여 Resources 폴더에 배치
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDataList", menuName = "UndeadSurvivor/CharacterDataList")]
    public class CharacterDataList : ScriptableObject
    {
        [SerializeField] private List<CharacterData> _characters = new List<CharacterData>();

        /// <summary>
        /// 캐릭터 데이터 리스트
        /// </summary>
        public List<CharacterData> Characters => _characters;
    }
}

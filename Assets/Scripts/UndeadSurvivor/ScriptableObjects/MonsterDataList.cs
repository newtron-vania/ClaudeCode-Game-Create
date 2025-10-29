using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 몬스터 데이터 리스트 ScriptableObject
    /// Unity 에디터에서 생성하여 Resources 폴더에 배치
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterDataList", menuName = "UndeadSurvivor/MonsterDataList")]
    public class MonsterDataList : ScriptableObject
    {
        [SerializeField] private List<MonsterData> _monsters = new List<MonsterData>();

        /// <summary>
        /// 몬스터 데이터 리스트
        /// </summary>
        public List<MonsterData> Monsters => _monsters;
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 무기 데이터 리스트 ScriptableObject
    /// Unity 에디터에서 생성하여 Resources 폴더에 배치
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponDataList", menuName = "UndeadSurvivor/WeaponDataList")]
    public class WeaponDataList : ScriptableObject
    {
        [SerializeField] private List<WeaponData> _weapons = new List<WeaponData>();

        /// <summary>
        /// 무기 데이터 리스트
        /// </summary>
        public List<WeaponData> Weapons => _weapons;
    }
}

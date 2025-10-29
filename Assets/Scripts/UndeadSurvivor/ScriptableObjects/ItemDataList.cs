using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 아이템 데이터 리스트 ScriptableObject
    /// Unity 에디터에서 생성하여 Resources 폴더에 배치
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDataList", menuName = "UndeadSurvivor/ItemDataList")]
    public class ItemDataList : ScriptableObject
    {
        [SerializeField] private List<ItemData> _items = new List<ItemData>();

        /// <summary>
        /// 아이템 데이터 리스트
        /// </summary>
        public List<ItemData> Items => _items;
    }
}

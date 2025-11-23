using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 아이템 타입
    /// </summary>
    public enum ItemType
    {
        Exp,        // 경험치
        Health,     // 체력 회복
        Magnet,     // 자석 (아이템 자동 획득)
        Box         // 아이템 상자
    }

    /// <summary>
    /// 아이템 데이터
    /// </summary>
    [System.Serializable]
    public class ItemData
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private string _description;
        [SerializeField] private ItemType _type;
        [SerializeField] private float _value;  // 체력 회복량, 경험치량 등

        /// <summary>
        /// 아이템 고유 ID
        /// </summary>
        public int Id => _id;

        /// <summary>
        /// 아이템 이름
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// 아이템 설명
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// 아이템 타입
        /// </summary>
        public ItemType Type => _type;

        /// <summary>
        /// 아이템 효과값
        /// - Exp: 경험치량
        /// - Health: 체력 회복량
        /// - Magnet: 흡수 범위
        /// - Box: 무작위 보상 개수
        /// </summary>
        public float Value => _value;

        /// <summary>
        /// 생성자
        /// </summary>
        public ItemData(int id, string name, string description, ItemType type, float value)
        {
            _id = id;
            _name = name;
            _description = description;
            _type = type;
            _value = value;
        }
    }
}

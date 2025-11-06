using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 몬스터 데이터 리스트 ScriptableObject
    /// Unity 에디터에서 생성하거나 JSON 파일로부터 로드
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterDataList", menuName = "UndeadSurvivor/MonsterDataList")]
    public class MonsterDataList : ScriptableObject
    {
        [SerializeField] private List<MonsterData> _monsters = new List<MonsterData>();

        /// <summary>
        /// 몬스터 데이터 리스트
        /// </summary>
        public List<MonsterData> Monsters => _monsters;

        /// <summary>
        /// JSON 파일로부터 몬스터 데이터 로드
        /// </summary>
        /// <param name="jsonPath">Resources 폴더 기준 경로 (확장자 제외)</param>
        public static MonsterDataList LoadFromJson(string jsonPath)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(jsonPath);
            if (jsonFile == null)
            {
                Debug.LogError($"[ERROR] MonsterDataList::LoadFromJson - File not found: {jsonPath}");
                return null;
            }

            MonsterDataWrapper wrapper = JsonUtility.FromJson<MonsterDataWrapper>(jsonFile.text);
            MonsterDataList dataList = CreateInstance<MonsterDataList>();
            dataList._monsters = wrapper.monsters;

            Debug.Log($"[INFO] MonsterDataList::LoadFromJson - Loaded {dataList._monsters.Count} monsters from JSON");
            return dataList;
        }

        /// <summary>
        /// ID로 몬스터 데이터 검색
        /// </summary>
        public MonsterData GetMonsterById(int id)
        {
            return _monsters.Find(m => m.Id == id);
        }

        /// <summary>
        /// JSON 직렬화용 래퍼 클래스
        /// </summary>
        [System.Serializable]
        private class MonsterDataWrapper
        {
            public List<MonsterData> monsters;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 몬스터 데이터 리스트 ScriptableObject
    /// Unity 에디터에서 생성하거나 JSON 파일로부터 동적으로 로드
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterDataList", menuName = "UndeadSurvivor/MonsterDataList")]
    public class MonsterDataList : ScriptableObject
    {
        [System.Serializable]
        public class MonsterDataWrapper
        {
            public List<MonsterDataJson> monsters = new List<MonsterDataJson>();
        }

        [System.Serializable]
        public class MonsterDataJson
        {
            public int id;
            public string name;
            public float maxHp;
            public float moveSpeed;
            public float damage;
            public float defense;
            public float expMultiplier;
        }

        [SerializeField] private List<MonsterData> _monsters = new List<MonsterData>();

        /// <summary>
        /// 몬스터 데이터 리스트
        /// </summary>
        public List<MonsterData> Monsters => _monsters;

        /// <summary>
        /// JSON 텍스트로부터 몬스터 데이터 로드
        /// </summary>
        /// <param name="jsonText">JSON 텍스트</param>
        public void LoadFromJson(string jsonText)
        {
            _monsters.Clear();

            try
            {
                MonsterDataWrapper wrapper = JsonUtility.FromJson<MonsterDataWrapper>(jsonText);

                if (wrapper == null || wrapper.monsters == null)
                {
                    Debug.LogError("[ERROR] UndeadSurvivor::MonsterDataList::LoadFromJson - Failed to parse JSON");
                    return;
                }

                foreach (var jsonData in wrapper.monsters)
                {
                    MonsterData monsterData = new MonsterData(
                        jsonData.id,
                        jsonData.name,
                        jsonData.maxHp,
                        jsonData.moveSpeed,
                        jsonData.damage,
                        jsonData.defense,
                        jsonData.expMultiplier
                    );

                    _monsters.Add(monsterData);
                }

                Debug.Log($"[INFO] UndeadSurvivor::MonsterDataList::LoadFromJson - Loaded {_monsters.Count} monsters");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ERROR] UndeadSurvivor::MonsterDataList::LoadFromJson - Exception: {e.Message}");
            }
        }

        /// <summary>
        /// ID로 몬스터 데이터 찾기
        /// </summary>
        /// <param name="id">몬스터 ID</param>
        /// <returns>몬스터 데이터 (없으면 null)</returns>
        public MonsterData GetMonsterById(int id)
        {
            return _monsters.Find(m => m.Id == id);
        }

        /// <summary>
        /// 이름으로 몬스터 데이터 찾기
        /// </summary>
        /// <param name="name">몬스터 이름</param>
        /// <returns>몬스터 데이터 (없으면 null)</returns>
        public MonsterData GetMonsterByName(string name)
        {
            return _monsters.Find(m => m.Name == name);
        }
    }
}

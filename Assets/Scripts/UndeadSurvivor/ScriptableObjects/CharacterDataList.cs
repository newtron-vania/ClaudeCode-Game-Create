using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 캐릭터 데이터 리스트 ScriptableObject
    /// Unity 에디터에서 생성하거나 JSON 파일로부터 동적으로 로드
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDataList", menuName = "UndeadSurvivor/CharacterDataList")]
    public class CharacterDataList : ScriptableObject
    {
        [System.Serializable]
        public class CharacterDataWrapper
        {
            public List<CharacterDataJson> characters = new List<CharacterDataJson>();
        }

        [System.Serializable]
        public class CharacterDataJson
        {
            public int id;
            public string name;
            public float maxHp;
            public float moveSpeed;
            public float damage;
            public float defense;
            public float cooldown;
            public int amount;
            public int startWeaponId;
        }

        [SerializeField] private List<CharacterData> _characters = new List<CharacterData>();

        /// <summary>
        /// 캐릭터 데이터 리스트
        /// </summary>
        public List<CharacterData> Characters => _characters;

        /// <summary>
        /// JSON 텍스트로부터 캐릭터 데이터 로드
        /// </summary>
        /// <param name="jsonText">JSON 텍스트</param>
        public void LoadFromJson(string jsonText)
        {
            _characters.Clear();

            try
            {
                CharacterDataWrapper wrapper = JsonUtility.FromJson<CharacterDataWrapper>(jsonText);

                if (wrapper == null || wrapper.characters == null)
                {
                    Debug.LogError("[ERROR] UndeadSurvivor::CharacterDataList::LoadFromJson - Failed to parse JSON");
                    return;
                }

                foreach (var jsonData in wrapper.characters)
                {
                    CharacterData characterData = new CharacterData(
                        jsonData.id,
                        jsonData.name,
                        jsonData.maxHp,
                        jsonData.moveSpeed,
                        jsonData.damage,
                        jsonData.defense,
                        jsonData.cooldown,
                        jsonData.amount,
                        jsonData.startWeaponId
                    );

                    _characters.Add(characterData);
                }

                Debug.Log($"[INFO] UndeadSurvivor::CharacterDataList::LoadFromJson - Loaded {_characters.Count} characters");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ERROR] UndeadSurvivor::CharacterDataList::LoadFromJson - Exception: {e.Message}");
            }
        }

        /// <summary>
        /// ID로 캐릭터 데이터 찾기
        /// </summary>
        /// <param name="id">캐릭터 ID</param>
        /// <returns>캐릭터 데이터 (없으면 null)</returns>
        public CharacterData GetCharacterById(int id)
        {
            return _characters.Find(c => c.Id == id);
        }

        /// <summary>
        /// 이름으로 캐릭터 데이터 찾기
        /// </summary>
        /// <param name="name">캐릭터 이름</param>
        /// <returns>캐릭터 데이터 (없으면 null)</returns>
        public CharacterData GetCharacterByName(string name)
        {
            return _characters.Find(c => c.Name == name);
        }
    }
}

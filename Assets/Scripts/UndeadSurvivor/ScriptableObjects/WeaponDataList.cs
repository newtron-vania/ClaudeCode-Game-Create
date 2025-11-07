using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 무기 데이터 리스트 ScriptableObject
    /// JSON 파일로부터 동적으로 로드 가능
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponDataList", menuName = "UndeadSurvivor/WeaponDataList")]
    public class WeaponDataList : ScriptableObject
    {
        [SerializeField] private List<WeaponData> _weapons = new List<WeaponData>();

        /// <summary>
        /// 무기 데이터 리스트
        /// </summary>
        public List<WeaponData> Weapons => _weapons;

        /// <summary>
        /// JSON Wrapper 클래스 (JsonUtility는 루트 배열을 지원하지 않으므로)
        /// </summary>
        [System.Serializable]
        private class WeaponListWrapper
        {
            public List<WeaponData> weapons;
        }

        /// <summary>
        /// JSON 텍스트로부터 무기 데이터 로드
        /// </summary>
        /// <param name="jsonText">JSON 문자열</param>
        public void LoadFromJson(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                Debug.LogError("[ERROR] WeaponDataList::LoadFromJson - JSON text is null or empty");
                return;
            }

            WeaponListWrapper wrapper = JsonUtility.FromJson<WeaponListWrapper>(jsonText);

            if (wrapper == null || wrapper.weapons == null)
            {
                Debug.LogError("[ERROR] WeaponDataList::LoadFromJson - Failed to parse JSON");
                return;
            }

            _weapons = wrapper.weapons;
            Debug.Log($"[INFO] WeaponDataList::LoadFromJson - Loaded {_weapons.Count} weapons from JSON");
        }

        /// <summary>
        /// Resources 경로로부터 JSON 파일 로드
        /// </summary>
        /// <param name="resourcePath">Resources 폴더 기준 경로 (확장자 제외)</param>
        /// <returns>로드된 WeaponDataList</returns>
        public static WeaponDataList LoadFromJsonFile(string resourcePath)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);

            if (jsonFile == null)
            {
                Debug.LogError($"[ERROR] WeaponDataList::LoadFromJsonFile - JSON file not found at {resourcePath}");
                return null;
            }

            WeaponDataList dataList = CreateInstance<WeaponDataList>();
            dataList.LoadFromJson(jsonFile.text);

            return dataList;
        }
    }
}

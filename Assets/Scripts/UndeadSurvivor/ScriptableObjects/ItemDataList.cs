using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 아이템 데이터 리스트 ScriptableObject
    /// JSON 파일로부터 동적으로 로드 가능
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDataList", menuName = "UndeadSurvivor/ItemDataList")]
    public class ItemDataList : ScriptableObject
    {
        [SerializeField] private List<ItemData> _items = new List<ItemData>();

        /// <summary>
        /// 아이템 데이터 리스트
        /// </summary>
        public List<ItemData> Items => _items;

        /// <summary>
        /// JSON Wrapper 클래스 (JsonUtility는 루트 배열을 지원하지 않으므로)
        /// </summary>
        [System.Serializable]
        private class ItemListWrapper
        {
            public List<ItemData> items;
        }

        /// <summary>
        /// JSON 텍스트로부터 아이템 데이터 로드
        /// </summary>
        /// <param name="jsonText">JSON 문자열</param>
        public void LoadFromJson(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                Debug.LogError("[ERROR] ItemDataList::LoadFromJson - JSON text is null or empty");
                return;
            }

            ItemListWrapper wrapper = JsonUtility.FromJson<ItemListWrapper>(jsonText);

            if (wrapper == null || wrapper.items == null)
            {
                Debug.LogError("[ERROR] ItemDataList::LoadFromJson - Failed to parse JSON");
                return;
            }

            _items = wrapper.items;
            Debug.Log($"[INFO] ItemDataList::LoadFromJson - Loaded {_items.Count} items from JSON");
        }

        /// <summary>
        /// Resources 경로로부터 JSON 파일 로드
        /// </summary>
        /// <param name="resourcePath">Resources 폴더 기준 경로 (확장자 제외)</param>
        /// <returns>로드된 ItemDataList</returns>
        public static ItemDataList LoadFromJsonFile(string resourcePath)
        {
            TextAsset jsonFile = Resources.Load<TextAsset>(resourcePath);

            if (jsonFile == null)
            {
                Debug.LogError($"[ERROR] ItemDataList::LoadFromJsonFile - JSON file not found at {resourcePath}");
                return null;
            }

            ItemDataList dataList = CreateInstance<ItemDataList>();
            dataList.LoadFromJson(jsonFile.text);

            return dataList;
        }
    }
}

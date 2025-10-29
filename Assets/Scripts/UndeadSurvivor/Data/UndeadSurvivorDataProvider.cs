using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// Undead Survivor 게임의 데이터 제공자
    /// ScriptableObject 기반 데이터 로딩 및 캐싱
    /// </summary>
    public class UndeadSurvivorDataProvider : IGameDataProvider
    {
        public string GameID => "UndeadSurvivor";
        public bool IsLoaded { get; private set; }

        // 데이터 딕셔너리
        private Dictionary<int, MonsterData> _monsterDict;
        private Dictionary<int, WeaponData> _weaponDict;
        private Dictionary<int, ItemData> _itemDict;
        private Dictionary<int, CharacterData> _characterDict;

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize()
        {
            _monsterDict = new Dictionary<int, MonsterData>();
            _weaponDict = new Dictionary<int, WeaponData>();
            _itemDict = new Dictionary<int, ItemData>();
            _characterDict = new Dictionary<int, CharacterData>();

            IsLoaded = false;
            Debug.Log("[INFO] UndeadSurvivor::DataProvider::Initialize - Data provider initialized");
        }

        /// <summary>
        /// 데이터 로드
        /// </summary>
        public void LoadData()
        {
            if (IsLoaded)
            {
                Debug.LogWarning("[WARNING] UndeadSurvivor::DataProvider::LoadData - Data already loaded");
                return;
            }

            Debug.Log("[INFO] UndeadSurvivor::DataProvider::LoadData - Loading data");

            LoadMonsterData();
            LoadWeaponData();
            LoadItemData();
            LoadCharacterData();

            IsLoaded = true;
            Debug.Log("[INFO] UndeadSurvivor::DataProvider::LoadData - Data loaded successfully");
        }

        /// <summary>
        /// 데이터 언로드
        /// </summary>
        public void UnloadData()
        {
            if (!IsLoaded)
            {
                Debug.LogWarning("[WARNING] UndeadSurvivor::DataProvider::UnloadData - Data not loaded");
                return;
            }

            Debug.Log("[INFO] UndeadSurvivor::DataProvider::UnloadData - Unloading data");

            _monsterDict.Clear();
            _weaponDict.Clear();
            _itemDict.Clear();
            _characterDict.Clear();

            IsLoaded = false;
            Debug.Log("[INFO] UndeadSurvivor::DataProvider::UnloadData - Data unloaded");
        }

        /// <summary>
        /// 데이터 조회 (제네릭)
        /// </summary>
        public T GetData<T>(string key) where T : class
        {
            Debug.LogWarning($"[WARNING] UndeadSurvivor::DataProvider::GetData - Generic GetData not implemented for key: {key}");
            return null;
        }

        /// <summary>
        /// 데이터 존재 여부 확인
        /// </summary>
        public bool HasData(string key)
        {
            return false;
        }

        #region 데이터 로딩

        /// <summary>
        /// 몬스터 데이터 로드
        /// </summary>
        private void LoadMonsterData()
        {
            MonsterDataList dataList = Resources.Load<MonsterDataList>("Data/UndeadSurvivor/ScriptableObjects/MonsterDataList");

            if (dataList == null)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::DataProvider::LoadMonsterData - MonsterDataList not found");
                return;
            }

            foreach (var data in dataList.Monsters)
            {
                if (_monsterDict.ContainsKey(data.Id))
                {
                    Debug.LogWarning($"[WARNING] UndeadSurvivor::DataProvider::LoadMonsterData - Duplicate monster ID: {data.Id}");
                    continue;
                }

                _monsterDict.Add(data.Id, data);
            }

            Debug.Log($"[INFO] UndeadSurvivor::DataProvider::LoadMonsterData - Loaded {_monsterDict.Count} monsters");
        }

        /// <summary>
        /// 무기 데이터 로드
        /// </summary>
        private void LoadWeaponData()
        {
            WeaponDataList dataList = Resources.Load<WeaponDataList>("Data/UndeadSurvivor/ScriptableObjects/WeaponDataList");

            if (dataList == null)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::DataProvider::LoadWeaponData - WeaponDataList not found");
                return;
            }

            foreach (var data in dataList.Weapons)
            {
                if (_weaponDict.ContainsKey(data.Id))
                {
                    Debug.LogWarning($"[WARNING] UndeadSurvivor::DataProvider::LoadWeaponData - Duplicate weapon ID: {data.Id}");
                    continue;
                }

                _weaponDict.Add(data.Id, data);
            }

            Debug.Log($"[INFO] UndeadSurvivor::DataProvider::LoadWeaponData - Loaded {_weaponDict.Count} weapons");
        }

        /// <summary>
        /// 아이템 데이터 로드
        /// </summary>
        private void LoadItemData()
        {
            ItemDataList dataList = Resources.Load<ItemDataList>("Data/UndeadSurvivor/ScriptableObjects/ItemDataList");

            if (dataList == null)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::DataProvider::LoadItemData - ItemDataList not found");
                return;
            }

            foreach (var data in dataList.Items)
            {
                if (_itemDict.ContainsKey(data.Id))
                {
                    Debug.LogWarning($"[WARNING] UndeadSurvivor::DataProvider::LoadItemData - Duplicate item ID: {data.Id}");
                    continue;
                }

                _itemDict.Add(data.Id, data);
            }

            Debug.Log($"[INFO] UndeadSurvivor::DataProvider::LoadItemData - Loaded {_itemDict.Count} items");
        }

        /// <summary>
        /// 캐릭터 데이터 로드
        /// </summary>
        private void LoadCharacterData()
        {
            CharacterDataList dataList = Resources.Load<CharacterDataList>("Data/UndeadSurvivor/ScriptableObjects/CharacterDataList");

            if (dataList == null)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::DataProvider::LoadCharacterData - CharacterDataList not found");
                return;
            }

            foreach (var data in dataList.Characters)
            {
                if (_characterDict.ContainsKey(data.Id))
                {
                    Debug.LogWarning($"[WARNING] UndeadSurvivor::DataProvider::LoadCharacterData - Duplicate character ID: {data.Id}");
                    continue;
                }

                _characterDict.Add(data.Id, data);
            }

            Debug.Log($"[INFO] UndeadSurvivor::DataProvider::LoadCharacterData - Loaded {_characterDict.Count} characters");
        }

        #endregion

        #region Public API

        /// <summary>
        /// 몬스터 데이터 조회
        /// </summary>
        public MonsterData GetMonsterData(int monsterId)
        {
            if (!IsLoaded)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::DataProvider::GetMonsterData - Data not loaded");
                return null;
            }

            if (_monsterDict.TryGetValue(monsterId, out MonsterData data))
            {
                return data;
            }

            Debug.LogError($"[ERROR] UndeadSurvivor::DataProvider::GetMonsterData - Monster ID {monsterId} not found");
            return null;
        }

        /// <summary>
        /// 레벨 스케일링이 적용된 몬스터 스탯 계산
        /// </summary>
        /// <param name="monsterId">몬스터 ID</param>
        /// <param name="level">레벨</param>
        /// <param name="isBoss">보스 여부</param>
        /// <returns>계산된 스탯</returns>
        public CharacterStat GetScaledMonsterStat(int monsterId, int level, bool isBoss = false)
        {
            MonsterData baseData = GetMonsterData(monsterId);
            if (baseData == null)
            {
                return new CharacterStat();
            }

            int bossMultiplier = isBoss ? 50 : 1;

            // 레벨 스케일링 공식 (원본 Undead Survivor 로직)
            float scaledMaxHp = baseData.MaxHp * ((100f + 10f * level) / 100f) * bossMultiplier;
            float scaledMoveSpeed = baseData.MoveSpeed * ((100f + level) / 100f);
            float scaledDamage = baseData.Damage * ((100f + level) / 100f);

            // ±10% 랜덤 변이
            scaledMaxHp = ApplyRandomVariation(scaledMaxHp);
            scaledDamage = ApplyRandomVariation(scaledDamage);

            CharacterStat stat = new CharacterStat(scaledMaxHp, scaledMoveSpeed, scaledDamage, baseData.Defense);

            Debug.Log($"[INFO] UndeadSurvivor::DataProvider::GetScaledMonsterStat - Monster {monsterId}, Level {level}, Boss {isBoss}: HP {scaledMaxHp:F0}, Speed {scaledMoveSpeed:F1}, Damage {scaledDamage:F0}");

            return stat;
        }

        /// <summary>
        /// 무기 데이터 조회
        /// </summary>
        public WeaponData GetWeaponData(int weaponId)
        {
            if (!IsLoaded)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::DataProvider::GetWeaponData - Data not loaded");
                return null;
            }

            if (_weaponDict.TryGetValue(weaponId, out WeaponData data))
            {
                return data;
            }

            Debug.LogError($"[ERROR] UndeadSurvivor::DataProvider::GetWeaponData - Weapon ID {weaponId} not found");
            return null;
        }

        /// <summary>
        /// 무기의 특정 레벨 스탯 조회
        /// </summary>
        public WeaponLevelStat GetWeaponLevelStat(int weaponId, int level)
        {
            WeaponData weaponData = GetWeaponData(weaponId);
            if (weaponData == null || level < 0 || level >= weaponData.LevelStats.Length)
            {
                Debug.LogError($"[ERROR] UndeadSurvivor::DataProvider::GetWeaponLevelStat - Invalid weapon ID {weaponId} or level {level}");
                return null;
            }

            return weaponData.LevelStats[level];
        }

        /// <summary>
        /// 아이템 데이터 조회
        /// </summary>
        public ItemData GetItemData(int itemId)
        {
            if (!IsLoaded)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::DataProvider::GetItemData - Data not loaded");
                return null;
            }

            if (_itemDict.TryGetValue(itemId, out ItemData data))
            {
                return data;
            }

            Debug.LogError($"[ERROR] UndeadSurvivor::DataProvider::GetItemData - Item ID {itemId} not found");
            return null;
        }

        /// <summary>
        /// 캐릭터 데이터 조회
        /// </summary>
        public CharacterData GetCharacterData(int characterId)
        {
            if (!IsLoaded)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::DataProvider::GetCharacterData - Data not loaded");
                return null;
            }

            if (_characterDict.TryGetValue(characterId, out CharacterData data))
            {
                return data;
            }

            Debug.LogError($"[ERROR] UndeadSurvivor::DataProvider::GetCharacterData - Character ID {characterId} not found");
            return null;
        }

        /// <summary>
        /// 모든 몬스터 ID 목록 반환
        /// </summary>
        public List<int> GetAllMonsterIds()
        {
            return new List<int>(_monsterDict.Keys);
        }

        /// <summary>
        /// 모든 무기 ID 목록 반환
        /// </summary>
        public List<int> GetAllWeaponIds()
        {
            return new List<int>(_weaponDict.Keys);
        }

        /// <summary>
        /// 모든 아이템 ID 목록 반환
        /// </summary>
        public List<int> GetAllItemIds()
        {
            return new List<int>(_itemDict.Keys);
        }

        /// <summary>
        /// 모든 캐릭터 ID 목록 반환
        /// </summary>
        public List<int> GetAllCharacterIds()
        {
            return new List<int>(_characterDict.Keys);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// ±10% 랜덤 변이 적용
        /// </summary>
        private float ApplyRandomVariation(float value)
        {
            return value * Random.Range(0.9f, 1.1f);
        }

        #endregion
    }
}

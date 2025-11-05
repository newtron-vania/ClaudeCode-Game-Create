using System;
using System.Collections.Generic;
using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// 플레이어 무기 장착 및 관리 시스템
    /// 무기 추가, 레벨업, 무기 슬롯 관리
    /// </summary>
    public class PlayerWeaponManager : MonoBehaviour
    {
        [Header("Weapon Slots")]
        [SerializeField] private int _maxWeaponSlots = 6; // 최대 무기 슬롯 개수
        private List<WeaponSlot> _equippedWeapons = new List<WeaponSlot>();

        [Header("Weapon Spawn Point")]
        [SerializeField] private Transform _weaponParent; // 무기 오브젝트 부모 Transform

        // 이벤트
        public event Action<int, string, int> OnWeaponAdded; // (weaponId, weaponName, currentLevel)
        public event Action<int, int> OnWeaponLevelUp; // (weaponId, newLevel)
        public event Action<int> OnWeaponSlotsFull; // (currentSlotCount)

        // 프로퍼티
        public int CurrentWeaponCount => _equippedWeapons.Count;
        public int MaxWeaponSlots => _maxWeaponSlots;
        public bool IsWeaponSlotsFull => _equippedWeapons.Count >= _maxWeaponSlots;

        private void Awake()
        {
            if (_weaponParent == null)
            {
                _weaponParent = transform;
            }
        }

        /// <summary>
        /// 무기 추가 (신규 무기 획득)
        /// </summary>
        public bool AddWeapon(WeaponData weaponData)
        {
            if (weaponData == null)
            {
                Debug.LogError("[ERROR] UndeadSurvivor::PlayerWeaponManager::AddWeapon - weaponData is null");
                return false;
            }

            // 무기 슬롯 포화 체크
            if (_equippedWeapons.Count >= _maxWeaponSlots)
            {
                Debug.LogWarning($"[WARNING] UndeadSurvivor::PlayerWeaponManager::AddWeapon - Weapon slots full ({_maxWeaponSlots})");
                OnWeaponSlotsFull?.Invoke(_equippedWeapons.Count);
                return false;
            }

            // 이미 장착한 무기인지 체크
            if (HasWeapon(weaponData.Id))
            {
                Debug.LogWarning($"[WARNING] UndeadSurvivor::PlayerWeaponManager::AddWeapon - Weapon {weaponData.Name} already equipped");
                return false;
            }

            // 무기 슬롯 생성
            WeaponSlot newSlot = new WeaponSlot
            {
                WeaponData = weaponData,
                CurrentLevel = 0, // 레벨 0 (표시는 1)
                WeaponObject = null // 실제 무기 오브젝트는 나중에 생성
            };

            _equippedWeapons.Add(newSlot);

            Debug.Log($"[INFO] UndeadSurvivor::PlayerWeaponManager::AddWeapon - Added weapon: {weaponData.Name} (ID: {weaponData.Id}), Slot: {_equippedWeapons.Count}/{_maxWeaponSlots}");

            OnWeaponAdded?.Invoke(weaponData.Id, weaponData.Name, newSlot.CurrentLevel + 1);

            return true;
        }

        /// <summary>
        /// 무기 레벨업
        /// </summary>
        public bool LevelUpWeapon(int weaponId)
        {
            WeaponSlot slot = GetWeaponSlot(weaponId);
            if (slot == null)
            {
                Debug.LogWarning($"[WARNING] UndeadSurvivor::PlayerWeaponManager::LevelUpWeapon - Weapon {weaponId} not found");
                return false;
            }

            // 최대 레벨 체크 (레벨 0-4, 표시 1-5)
            if (slot.CurrentLevel >= 4)
            {
                Debug.LogWarning($"[WARNING] UndeadSurvivor::PlayerWeaponManager::LevelUpWeapon - Weapon {slot.WeaponData.Name} already at max level (5)");
                return false;
            }

            slot.CurrentLevel++;

            Debug.Log($"[INFO] UndeadSurvivor::PlayerWeaponManager::LevelUpWeapon - {slot.WeaponData.Name} leveled up to {slot.CurrentLevel + 1}");

            OnWeaponLevelUp?.Invoke(weaponId, slot.CurrentLevel + 1);

            // 무기 스크립트에 레벨업 알림 (구현 예정)
            // if (slot.WeaponObject != null)
            // {
            //     var weapon = slot.WeaponObject.GetComponent<Weapon>();
            //     weapon?.LevelUp();
            // }

            return true;
        }

        /// <summary>
        /// 특정 무기를 보유하고 있는지 확인
        /// </summary>
        public bool HasWeapon(int weaponId)
        {
            return _equippedWeapons.Exists(slot => slot.WeaponData.Id == weaponId);
        }

        /// <summary>
        /// 무기 슬롯 조회
        /// </summary>
        public WeaponSlot GetWeaponSlot(int weaponId)
        {
            return _equippedWeapons.Find(slot => slot.WeaponData.Id == weaponId);
        }

        /// <summary>
        /// 모든 무기 슬롯 조회
        /// </summary>
        public List<WeaponSlot> GetAllWeaponSlots()
        {
            return new List<WeaponSlot>(_equippedWeapons);
        }

        /// <summary>
        /// 특정 무기의 현재 레벨 조회
        /// </summary>
        public int GetWeaponLevel(int weaponId)
        {
            WeaponSlot slot = GetWeaponSlot(weaponId);
            return slot != null ? slot.CurrentLevel : -1;
        }

        /// <summary>
        /// 특정 무기가 최대 레벨인지 확인
        /// </summary>
        public bool IsWeaponMaxLevel(int weaponId)
        {
            WeaponSlot slot = GetWeaponSlot(weaponId);
            return slot != null && slot.CurrentLevel >= 4;
        }

        /// <summary>
        /// 모든 무기 제거 (게임 종료 시)
        /// </summary>
        public void ClearAllWeapons()
        {
            // 무기 오브젝트 제거
            foreach (var slot in _equippedWeapons)
            {
                if (slot.WeaponObject != null)
                {
                    Destroy(slot.WeaponObject);
                }
            }

            _equippedWeapons.Clear();
            Debug.Log("[INFO] UndeadSurvivor::PlayerWeaponManager::ClearAllWeapons - All weapons cleared");
        }

        /// <summary>
        /// 초기화 (게임 시작 시)
        /// </summary>
        public void Initialize()
        {
            ClearAllWeapons();
            Debug.Log("[INFO] UndeadSurvivor::PlayerWeaponManager::Initialize - WeaponManager initialized");
        }

        /// <summary>
        /// 무기 오브젝트 등록 (무기 프리팹 인스턴스화 후 호출)
        /// </summary>
        public void RegisterWeaponObject(int weaponId, GameObject weaponObject)
        {
            WeaponSlot slot = GetWeaponSlot(weaponId);
            if (slot != null)
            {
                slot.WeaponObject = weaponObject;
                weaponObject.transform.SetParent(_weaponParent);
                Debug.Log($"[INFO] UndeadSurvivor::PlayerWeaponManager::RegisterWeaponObject - Weapon object registered for {slot.WeaponData.Name}");
            }
        }
    }

    /// <summary>
    /// 무기 슬롯 데이터
    /// </summary>
    [System.Serializable]
    public class WeaponSlot
    {
        public WeaponData WeaponData;       // 무기 데이터
        public int CurrentLevel;            // 현재 레벨 (0-4, 표시는 1-5)
        public GameObject WeaponObject;     // 실제 무기 오브젝트 인스턴스
    }
}

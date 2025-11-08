using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// MonsterData 로드 테스트 스크립트
    /// Unity 에디터에서 GameObject에 추가하여 Play 모드에서 실행
    /// </summary>
    public class TestMonsterDataLoader : MonoBehaviour
    {
        [SerializeField] private bool _testOnStart = true;

        private void Start()
        {
            if (_testOnStart)
            {
                TestMonsterDataLoad();
            }
        }

        /// <summary>
        /// MonsterData 로드 테스트
        /// </summary>
        [ContextMenu("Test Monster Data Load")]
        public void TestMonsterDataLoad()
        {
            Debug.Log("[TEST] ========== MonsterData Load Test Start ==========");

            // DataProvider 생성 및 초기화
            UndeadSurvivorDataProvider provider = new UndeadSurvivorDataProvider();
            provider.Initialize();

            // 데이터 로드
            provider.LoadData();

            if (!provider.IsLoaded)
            {
                Debug.LogError("[TEST] Failed to load data");
                return;
            }

            // 모든 몬스터 ID 가져오기
            var monsterIds = provider.GetAllMonsterIds();
            Debug.Log($"[TEST] Total monsters loaded: {monsterIds.Count}");

            // 각 몬스터 데이터 출력
            foreach (int id in monsterIds)
            {
                MonsterData monster = provider.GetMonsterData(id);
                if (monster != null)
                {
                    Debug.Log($"[TEST] Monster ID {id}:");
                    Debug.Log($"       Name: {monster.Name}");
                    Debug.Log($"       MaxHp: {monster.MaxHp}");
                    Debug.Log($"       Damage: {monster.Damage}");
                    Debug.Log($"       Defense: {monster.Defense}");
                    Debug.Log($"       MoveSpeed: {monster.MoveSpeed}");
                    Debug.Log($"       ExpMultiplier: {monster.ExpMultiplier}");
                }
            }

            // 레벨 스케일링 테스트
            Debug.Log("[TEST] ========== Level Scaling Test ==========");
            for (int level = 1; level <= 5; level++)
            {
                CharacterStat scaledStat = provider.GetScaledMonsterStat(1, level, false);
                Debug.Log($"[TEST] Zombie (ID 1) at Level {level}:");
                Debug.Log($"       MaxHp: {scaledStat.MaxHp:F1}");
                Debug.Log($"       Damage: {scaledStat.Damage:F1}");
                Debug.Log($"       MoveSpeed: {scaledStat.MoveSpeed:F2}");
            }

            // 보스 스케일링 테스트
            Debug.Log("[TEST] ========== Boss Scaling Test ==========");
            CharacterStat bossStat = provider.GetScaledMonsterStat(1, 1, true);
            Debug.Log($"[TEST] Zombie Boss (ID 1) at Level 1:");
            Debug.Log($"       MaxHp: {bossStat.MaxHp:F1}");
            Debug.Log($"       Damage: {bossStat.Damage:F1}");

            // 데이터 언로드
            provider.UnloadData();
            Debug.Log("[TEST] ========== MonsterData Load Test End ==========");
        }
    }
}

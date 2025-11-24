using UnityEngine;
using System.Collections.Generic;

namespace ThreeMatch.Data
{
    /// <summary>
    /// 난이도 설정 리스트 (ScriptableObject)
    /// 모든 난이도 설정을 관리하는 컨테이너
    /// </summary>
    [CreateAssetMenu(fileName = "DifficultyConfigList", menuName = "ThreeMatch/DifficultyConfigList")]
    public class DifficultyConfigList : ScriptableObject
    {
        [Header("Difficulty Configs")]
        [Tooltip("모든 난이도 설정 리스트 (Easy, Normal, Hard)")]
        public List<DifficultyConfig> Configs = new List<DifficultyConfig>();

        /// <summary>
        /// 난이도 레벨로 설정 조회
        /// </summary>
        public DifficultyConfig GetConfigByLevel(DifficultyLevel level)
        {
            return Configs.Find(c => c.Level == level);
        }
    }
}

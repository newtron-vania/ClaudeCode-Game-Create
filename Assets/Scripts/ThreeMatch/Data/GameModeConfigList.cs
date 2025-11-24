using UnityEngine;
using System.Collections.Generic;

namespace ThreeMatch.Data
{
    /// <summary>
    /// 게임 모드 설정 리스트 (ScriptableObject)
    /// 모든 게임 모드 설정을 관리하는 컨테이너
    /// </summary>
    [CreateAssetMenu(fileName = "GameModeConfigList", menuName = "ThreeMatch/GameModeConfigList")]
    public class GameModeConfigList : ScriptableObject
    {
        [Header("Game Mode Configs")]
        [Tooltip("모든 게임 모드 설정 리스트 (Classic, MovesLimited, Endless)")]
        public List<GameModeConfig> Modes = new List<GameModeConfig>();

        /// <summary>
        /// 게임 모드로 설정 조회
        /// </summary>
        public GameModeConfig GetConfigByMode(GameMode mode)
        {
            return Modes.Find(m => m.Mode == mode);
        }
    }
}

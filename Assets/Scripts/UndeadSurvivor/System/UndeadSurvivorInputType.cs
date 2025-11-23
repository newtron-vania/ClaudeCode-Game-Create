namespace UndeadSurvivor
{
    /// <summary>
    /// Undead Survivor 게임 전용 입력 타입
    /// InputManager의 InputType과 매핑하여 게임 내부에서 사용합니다.
    /// </summary>
    public enum UndeadSurvivorInputType
    {
        /// <summary>플레이어 이동 입력 (WASD)</summary>
        Move,

        /// <summary>일시정지 (ESC)</summary>
        Pause,

        /// <summary>레벨업 선택지 확인 (레벨업 UI용, 추후 구현)</summary>
        LevelUpConfirm,

        /// <summary>대시/회피 (Space, 추후 구현)</summary>
        Dash,

        /// <summary>특수 스킬 사용 (Q, 추후 구현)</summary>
        SpecialSkill
    }
}

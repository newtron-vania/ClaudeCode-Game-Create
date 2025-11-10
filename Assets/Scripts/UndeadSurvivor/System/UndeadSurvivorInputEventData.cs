using UnityEngine;

namespace UndeadSurvivor
{
    /// <summary>
    /// Undead Survivor 게임 전용 입력 이벤트 데이터
    /// 게임 내부에서 사용되는 입력 정보를 담습니다.
    /// </summary>
    [System.Serializable]
    public struct UndeadSurvivorInputEventData
    {
        /// <summary>
        /// 게임 전용 입력 타입
        /// </summary>
        public UndeadSurvivorInputType InputType;

        /// <summary>
        /// 이동 방향 벡터 (Move 타입에서 사용)
        /// </summary>
        public Vector2 MoveDirection;

        /// <summary>
        /// 입력이 눌렸는지 여부 (Pause, Dash 등에서 사용)
        /// </summary>
        public bool IsPressed;

        /// <summary>
        /// 이동 입력 이벤트 생성자
        /// </summary>
        public UndeadSurvivorInputEventData(Vector2 moveDirection)
        {
            InputType = UndeadSurvivorInputType.Move;
            MoveDirection = moveDirection;
            IsPressed = false;
        }

        /// <summary>
        /// 버튼 입력 이벤트 생성자
        /// </summary>
        public UndeadSurvivorInputEventData(UndeadSurvivorInputType inputType, bool isPressed)
        {
            InputType = inputType;
            MoveDirection = Vector2.zero;
            IsPressed = isPressed;
        }

        /// <summary>
        /// 디버깅용 문자열 변환
        /// </summary>
        public override string ToString()
        {
            if (InputType == UndeadSurvivorInputType.Move)
            {
                return $"UndeadSurvivor Input [{InputType}] MoveDirection: {MoveDirection}";
            }
            else
            {
                return $"UndeadSurvivor Input [{InputType}] IsPressed: {IsPressed}";
            }
        }
    }
}

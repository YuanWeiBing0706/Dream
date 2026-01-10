namespace Events
{
    public class GameEvents
    {
        /// 玩家移动被执行（输入中）
        public const string PLAYER_MOVE_PERFORMED = nameof(PLAYER_MOVE_PERFORMED);
        /// 玩家移动被取消（输入停止）
        public const string PLAYER_MOVE_CANCELED = nameof(PLAYER_MOVE_CANCELED);
        /// 玩家相机缩放
        public const string PLAYER_CAMERA_ZOOM = nameof(PLAYER_CAMERA_ZOOM);
        /// 设置输入数据包
        public const string SET_INPUTS = nameof(SET_INPUTS);
        /// 玩家跳跃被执行（按下）
        public const string PLAYER_JUMP_PERFROMED = nameof(PLAYER_JUMP_PERFROMED);
        /// 玩家跳跃被取消（松开）
        public const string PLAYER_JUMP_CANCELED = nameof(PLAYER_JUMP_CANCELED);
    }
}
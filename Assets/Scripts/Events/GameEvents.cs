namespace Events
{
    public class GameEvents
    {
        /// 玩家移动被执行
        public const string PLAYER_MOVE_PERFORMED = nameof(PLAYER_MOVE_PERFORMED);
        /// 玩家移动被取消
        public const string PLAYER_MOVE_CANCELED = nameof(PLAYER_MOVE_CANCELED);
        /// 玩家相机缩放
        public const string PLAYER_CAMERA_ZOOM = nameof(PLAYER_CAMERA_ZOOM);
        /// 玩家跳跃被执行
        public const string PLAYER_JUMP_PERFROMED = nameof(PLAYER_JUMP_PERFROMED);
        /// 玩家跳跃被取消
        public const string PLAYER_JUMP_CANCELED = nameof(PLAYER_JUMP_CANCELED);
        /// 玩家闪避被执行
        public const string PLAYER_DODGE_PERFORMED = nameof(PLAYER_DODGE_PERFORMED);
        /// 玩家闪避被取消
        public const string PLAYER_DODGE_CANCELED = nameof(PLAYER_DODGE_CANCELED);
        
        /// 设置输入数据包
        public const string SET_INPUTS = nameof(SET_INPUTS);
        
        /// 起跳动画
        public const string PLAYER_JUMP_ANIMATION = nameof(PLAYER_JUMP_ANIMATION);
        /// 冲刺动画
        public const string PLAYER_DASH_ANIMATION = nameof(PLAYER_DASH_ANIMATION);
        /// 翻滚动画
        public const string PLAYER_ROLL_ANIMATION = nameof(PLAYER_ROLL_ANIMATION);
        /// 下落动画
        public const string PLAYER_FALL_ANIMATION =  nameof(PLAYER_FALL_ANIMATION);

        
    }
}
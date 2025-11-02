namespace Events
{
    /// <summary>
    /// 游戏事件常量类，定义所有游戏事件的名称
    /// </summary>
    public class GameEvents
    {
        /// 延迟更新注册事件
        public const string LATE_UPDATE_REGISTER = nameof(LATE_UPDATE_REGISTER);
        
        /// 玩家移动执行事件
        public const string PLAYER_MOVE_PERFORMED = nameof(PLAYER_MOVE_PERFORMED);
        
        /// 玩家移动取消事件
        public const string PLAYER_MOVE_CANCELED = nameof(PLAYER_MOVE_CANCELED);
        
        public const string PLAYER_ANGLE_VIEW_PERFORMED = nameof(PLAYER_ANGLE_VIEW_PERFORMED);
        
        public const string PLAYER_ANGLE_VIEW_CANCELED = nameof(PLAYER_ANGLE_VIEW_CANCELED);
    }
}
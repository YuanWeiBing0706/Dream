namespace Events
{
    /// <summary>
    /// 游戏事件常量类，定义所有游戏事件的名称
    /// </summary>
    public class GameEvents
    {
        public const string PLAYER_MOVE_PERFORMED = nameof(PLAYER_MOVE_PERFORMED);
        public const string PLAYER_MOVE_CANCELED = nameof(PLAYER_MOVE_CANCELED);
        public const string PLAYER_CAMERA_ZOOM = nameof(PLAYER_CAMERA_ZOOM);
        public const string SET_INPUTS =  nameof(SET_INPUTS);
        public const string PLAYER_JUMP_PERFROMED =  nameof(PLAYER_JUMP_PERFROMED);
        public const string PLAYER_JUMP_CANCELED = nameof(PLAYER_JUMP_CANCELED);
    }
}
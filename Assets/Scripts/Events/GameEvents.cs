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
        /// 玩家闪避呗取消
        public const string PLAYER_DODGE_CANCELED = nameof(PLAYER_DODGE_CANCELED);
        /// 玩家下落攻击被执行
        public const string PLAYER_FALLATTACK_PERFROMED = nameof(PLAYER_FALLATTACK_PERFROMED);
        /// 玩家轻攻击被执行
        public const string PLAYER_LIGHTATTACK_PERFROMED = nameof(PLAYER_LIGHTATTACK_PERFROMED);
        /// 玩家轻攻击被取消
        public const string PLAYER_LIGHTATTACK_CANCELED = nameof(PLAYER_LIGHTATTACK_CANCELED);
        /// 玩家技能攻击被执行
        public const string PLAYER_SKILLATTACK_PERFROMED = nameof(PLAYER_SKILLATTACK_PERFROMED);

        /// 玩家重攻击被执行
        public const string PLAYER_HEAVYATTACK_PERFROMED = nameof(PLAYER_HEAVYATTACK_PERFROMED);
        /// 玩家重攻击被取消
        public const string PLAYER_HEAVYATTACK_CANCELED = nameof(PLAYER_HEAVYATTACK_CANCELED);

        /// 设置输入数据包
        public const string SET_INPUTS = nameof(SET_INPUTS);

        /// 起跳动画
        public const string PLAYER_JUMP_ANIMATION = nameof(PLAYER_JUMP_ANIMATION);
        /// 冲刺动画
        public const string PLAYER_DASH_ANIMATION = nameof(PLAYER_DASH_ANIMATION);
        /// 翻滚动画
        public const string PLAYER_ROLL_ANIMATION = nameof(PLAYER_ROLL_ANIMATION);
        /// 下落动画
        public const string PLAYER_FALL_ANIMATION = nameof(PLAYER_FALL_ANIMATION);
        /// 玩家下落攻击被执行
        public const string PLAYER_FALLATTACK_ANIMATION = nameof(PLAYER_FALLATTACK_ANIMATION);
        /// 玩家轻攻击被执行
        public const string PLAYER_LIGHTATTACK_ANIMATION = nameof(PLAYER_LIGHTATTACK_ANIMATION);
        /// 玩家重攻击被执行
        public const string PLAYER_SKILLATTACK_ANIMATION = nameof(PLAYER_SKILLATTACK_ANIMATION);
        /// 玩家技能攻击被执行
        public const string PLAYER_HEAVYATTACK_ANIMATION = nameof(PLAYER_HEAVYATTACK_ANIMATION);

        public const string PLAYER_ATTACK_OPEN_DETECTION = nameof(PLAYER_ATTACK_OPEN_DETECTION);
        public const string PLAYER_ATTACK_CLOSE_DETECTION = nameof(PLAYER_ATTACK_CLOSE_DETECTION);

        // 敌人事件
        public const string ENEMY_DAMAGED = nameof(ENEMY_DAMAGED);
        public const string ENEMY_DEATH = nameof(ENEMY_DEATH);        // 伤害管道事件
        /// 伤害请求（CombatSystem → DamageSystem）
        public const string DAMAGE_REQUEST = nameof(DAMAGE_REQUEST);
        /// 伤害结算结果（DamageSystem → 受击方）
        public const string DAMAGE_RESULT = nameof(DAMAGE_RESULT);


    }
}
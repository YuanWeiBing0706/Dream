namespace Const
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
        public const string ENEMY_DEATH = nameof(ENEMY_DEATH);
        /// 敌人从对象池生成，EnemyInjuriedSystem 订阅后完成注册
        public const string ENEMY_SPAWNED = nameof(ENEMY_SPAWNED);
        
        /// 伤害请求（CombatSystem → DamageSystem）
        public const string DAMAGE_REQUEST = nameof(DAMAGE_REQUEST);
        /// 伤害结算结果（DamageSystem → 受击方）
        public const string DAMAGE_RESULT = nameof(DAMAGE_RESULT);
        
        public const string PLAYER_DEAD = nameof(PLAYER_DEAD);
        
        /// 请求开始普通波次（LevelManager -> WaveManager，参数：string[] enemyPool）
        public const string START_WAVE_REQUEST = nameof(START_WAVE_REQUEST);
        /// 请求开始 Boss 波次（LevelManager -> WaveManager，参数：string[] bossPool）
        public const string START_BOSS_WAVE_REQUEST = nameof(START_BOSS_WAVE_REQUEST);
        /// 波次完成事件（WaveManager -> LevelManager）
        public const string WAVE_COMPLETED = nameof(WAVE_COMPLETED);
        
        /// 场景卸载通知（LevelManager -> Scene/UI 清理链路）
        public const string SCENE_UNLOADED = nameof(SCENE_UNLOADED);
        /// 请求加载大厅场景（LevelManager -> SceneFlowManager）
        public const string SCENE_LOAD_LOBBY_REQUEST = nameof(SCENE_LOAD_LOBBY_REQUEST);
        /// 请求显示常驻视图（LevelManager -> UIManager）
        public const string UI_SHOW_VIEW_REQUEST = nameof(UI_SHOW_VIEW_REQUEST);
        /// 请求显示窗口（LevelManager -> UIManager）
        public const string UI_SHOW_WINDOW_REQUEST = nameof(UI_SHOW_WINDOW_REQUEST);
        /// 关卡掉落计算请求（LevelManager -> DropSystem）
        public const string STAGE_DROP_ROLL_REQUEST = nameof(STAGE_DROP_ROLL_REQUEST);
        /// 掉落结果就绪（LevelManager -> ItemSelect）
        public const string ITEM_REWARDS_READY = nameof(ITEM_REWARDS_READY);
        /// 道具选择阶段开始
        public const string ITEM_SELECTION_BEGIN = nameof(ITEM_SELECTION_BEGIN);
        /// 海克斯选择阶段开始
        public const string HEX_SELECTION_BEGIN = nameof(HEX_SELECTION_BEGIN);
        /// 下一层倒计时 Tick（参数：剩余秒数）
        public const string LEVEL_COUNTDOWN_TICK = nameof(LEVEL_COUNTDOWN_TICK);
        /// 请求刷新会话信息（层数/金币等）
        public const string SESSION_INFO_REFRESH_REQUEST = nameof(SESSION_INFO_REFRESH_REQUEST);

        /// UI 选择界面打开时广播（LevelManager → PlayerInputSystem），锁定玩家输入
        public const string GAME_INPUT_LOCKED = nameof(GAME_INPUT_LOCKED);
        /// UI 选择界面关闭时广播（LevelManager → PlayerInputSystem），恢复玩家输入
        public const string GAME_INPUT_UNLOCKED = nameof(GAME_INPUT_UNLOCKED);
    }
}
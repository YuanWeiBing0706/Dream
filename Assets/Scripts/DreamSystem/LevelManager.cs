using Cysharp.Threading.Tasks;
using Const;
using Data;
using DreamConfig;
using DreamManager;
using DreamPool;
using DreamSystem.Damage;
using DreamSystem.UI.ViewModel;
using Enum.Buff;
using Model.Player;
using UnityEngine;
using VContainer;

namespace DreamSystem
{
    public class LevelManager : GameSystem
    {
        private readonly GameSessionData _sessionData;
        private readonly ResourcesManager _resources;
        private readonly UIManager _uiManager;
        private readonly SceneFlowManager _sceneFlow;
        private readonly EventManager _eventManager;
        private readonly HexSelectViewModel _hexSelectVm;
        private readonly PlayerStatusViewModel _playerStatusVm;
        private readonly ItemSelectViewModel _itemSelectVm;
        private readonly ShopViewModel _shopVm;
        private readonly GameResultViewModel _gameResultVm;
        private readonly DropSystem _dropSystem;
        private readonly PoolManager _poolManager;
        private readonly BuffSystem _buffSystem;
        private readonly PlayerModel _playerModel;

        private bool _isLevelRunning;
        private bool _playerDead;
        private bool _waveCompleted;
        private bool _isGameEnding;

        [Inject]
        public LevelManager(GameSessionData sessionData, ResourcesManager resources, UIManager uiManager, SceneFlowManager sceneFlow, EventManager eventManager, HexSelectViewModel hexSelectVm, PlayerStatusViewModel playerStatusVm, ItemSelectViewModel itemSelectVm, ShopViewModel shopVm, GameResultViewModel gameResultVm, DropSystem dropSystem, PoolManager poolManager, BuffSystem buffSystem, PlayerModel playerModel)
        {
            _sessionData = sessionData;
            _resources = resources;
            _uiManager = uiManager;
            _sceneFlow = sceneFlow;
            _eventManager = eventManager;
            _hexSelectVm = hexSelectVm;
            _playerStatusVm = playerStatusVm;
            _itemSelectVm = itemSelectVm;
            _shopVm = shopVm;
            _gameResultVm = gameResultVm;
            _dropSystem = dropSystem;
            _poolManager = poolManager;
            _buffSystem = buffSystem;
            _playerModel = playerModel;
        }

        public override void Start()
        {
            // 进入战斗场景时锁定并隐藏鼠标（大厅会设成可见）
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _eventManager.Publish(GameEvents.GAME_INPUT_UNLOCKED);
            // 清理上一个场景（大厅）遗留的 UI 缓存引用
            _eventManager.Publish(GameEvents.SCENE_UNLOADED);
            _uiManager.OnSceneUnloaded();
            // 清理上一局遗留的对象池（场景卸载时 GO 已被 Unity 销毁，池中引用全部失效）
            _poolManager.ClearScenePool();

            _eventManager.Subscribe<bool>(GameEvents.PLAYER_DEAD, OnPlayerDead);
            _eventManager.Subscribe(GameEvents.WAVE_COMPLETED, OnWaveCompleted);
            RunLevelFlow().Forget();
        }

        private void OnPlayerDead(bool _)
        {
            if (_playerDead) return;
            _playerDead = true;
            UnityEngine.Debug.Log("[LevelManager] 玩家死亡，准备进入结算窗口...");
            HandlePlayerDeath().Forget();
        }

        private async UniTaskVoid HandlePlayerDeath()
        {
            await UniTask.Delay(1000);
            await HandleGameEndFlow(false);
        }

        private async UniTaskVoid RunLevelFlow()
        {
            _isLevelRunning = true;

            // 等待 ResourcesManager 完成所有配置加载（异步启动，LevelManager 可能先于它 Start）
            await UniTask.WaitUntil(() => _resources.GetConfig<LevelConfig>() != null);

            UnityEngine.Debug.Log($"[LevelManager] 关卡开始！大关: {_sessionData.Chapter}, 小关: {_sessionData.Level}");

            // 拉起战斗 HUD
            _eventManager.Publish(GameEvents.UI_SHOW_VIEW_REQUEST, UIPanelIds.PLAYER_STATUS_VIEW);
            await _uiManager.ShowViewAsync(UIPanelIds.PLAYER_STATUS_VIEW, _playerStatusVm);

            var levelConfig = _resources.GetConfig<LevelConfig>();
            int flatIndex = (_sessionData.Chapter - 1) * 3 + _sessionData.Level;
            string levelId = $"level_{flatIndex:D2}";

            if (!levelConfig.TryGet(levelId, out var levelData))
            {
                UnityEngine.Debug.LogError($"[LevelManager] 找不到关卡配置: {levelId}");
                return;
            }

            UnityEngine.Debug.Log($"[LevelManager] 进入关卡：{levelData.levelName}（isBoss={levelData.isBoss}）");
            
            // WaveManager 内置"场上最多 6 只，总计 12 只自动补充"逻辑，只需启动一次
            if (levelData.isBoss)
            {
                await HandleBossWave(levelData.enemyPool);
            }
            else
            {
                _waveCompleted = false;
                _eventManager.Publish(GameEvents.START_WAVE_REQUEST, levelData.enemyPool);
                await UniTask.WaitUntil(() => _waveCompleted || _playerDead);
                if (_playerDead) return;
                UnityEngine.Debug.Log("[LevelManager] 本关波次全部消灭！");
            }

            // 1. 清怪后停顿 2 秒，直接进入奖励阶段（跳过宝箱交互）
            await UniTask.Delay(2000);
            if (_playerDead) return;

            // 2. 道具选择
            await HandleItemSelection(levelData);
            if (_playerDead) return;

            // 3. 海克斯选择
            await HandleHexSelection();
            if (_playerDead) return;
            
            // 3.5 领取完本轮全部奖励后：回复最大生命值的 50%
            HealPlayerAfterRewards();
            if (_playerDead) return;
            
            // 3.8 章节 Boss 结算后先进入商店，再推进到下一章
            if (levelData.isBoss && _sessionData.Chapter < 3)
            {
                await HandleShopSelection();
                if (_playerDead) return;
            }

            // 4. 进入下一关前的倒计时（非 Boss 关）
            if (!levelData.isBoss)
            {
                await HandleNextLevelCountdown();
                if (_playerDead) return;
            }

            AdvanceProgression(levelData.isBoss);
        }
        

        private async UniTask HandleBossWave(string[] bossPool)
        {
            UnityEngine.Debug.Log("[LevelManager] Boss 降临！");
            _waveCompleted = false;
            _eventManager.Publish(GameEvents.START_BOSS_WAVE_REQUEST, bossPool);
            await UniTask.WaitUntil(() => _waveCompleted || _playerDead);
            if (!_playerDead)
            {
                UnityEngine.Debug.Log("[LevelManager] Boss 已击败！");
            }
        }
        
        private void OnWaveCompleted()
        {
            _waveCompleted = true;
        }
        
        private async UniTask HandleItemSelection(LevelData levelData)
        {
            UnityEngine.Debug.Log("[LevelManager] 打开道具选择界面...");
            int baseGold = levelData.experienceReward / 5;
            _eventManager.Publish(GameEvents.STAGE_DROP_ROLL_REQUEST, levelData.dropGroupId, baseGold);
            var rewards = _dropSystem.RollStageDrop(levelData.dropGroupId, baseGold);
            _eventManager.Publish(GameEvents.ITEM_REWARDS_READY, rewards);
            _itemSelectVm.SetRewards(rewards);
            _eventManager.Publish(GameEvents.ITEM_SELECTION_BEGIN);

            EnterUIPhase();
            _eventManager.Publish(GameEvents.UI_SHOW_WINDOW_REQUEST, UIPanelIds.ITEM_SELECT_VIEW);
            var view = await _uiManager.ShowWindowAsync(UIPanelIds.ITEM_SELECT_VIEW, _itemSelectVm);
            if (view == null)
            {
                UnityEngine.Debug.LogWarning("[LevelManager] 道具选择界面加载失败，跳过。");
                ExitUIPhase();
                return;
            }
            await _itemSelectVm.WaitForConfirmation();
            ExitUIPhase();
            UnityEngine.Debug.Log("[LevelManager] 道具选择完毕。");
        }

        private async UniTask HandleHexSelection()
        {
            UnityEngine.Debug.Log("[LevelManager] 打开海克斯选择界面...");
            _eventManager.Publish(GameEvents.HEX_SELECTION_BEGIN);
            _hexSelectVm.RandomizeOptions();

            EnterUIPhase();
            _eventManager.Publish(GameEvents.UI_SHOW_WINDOW_REQUEST, UIPanelIds.HEX_SELECT_VIEW);
            var view = await _uiManager.ShowWindowAsync(UIPanelIds.HEX_SELECT_VIEW, _hexSelectVm);
            if (view == null)
            {
                UnityEngine.Debug.LogWarning("[LevelManager] 海克斯选择界面加载失败，跳过。");
                ExitUIPhase();
                return;
            }
            await _hexSelectVm.WaitForSelection();
            ExitUIPhase();
            UnityEngine.Debug.Log("[LevelManager] 海克斯选择完毕。");
        }

        private async UniTask HandleShopSelection()
        {
            UnityEngine.Debug.Log("[LevelManager] 打开章节商店界面...");
            _shopVm.OpenForChapter(_sessionData.Chapter);

            EnterUIPhase();
            _eventManager.Publish(GameEvents.UI_SHOW_WINDOW_REQUEST, UIPanelIds.SHOP_VIEW);
            var view = await _uiManager.ShowWindowAsync(UIPanelIds.SHOP_VIEW, _shopVm);
            if (view == null)
            {
                UnityEngine.Debug.LogWarning("[LevelManager] 商店界面未配置或加载失败，跳过商店阶段。");
                ExitUIPhase();
                return;
            }

            await _shopVm.WaitForClose();
            ExitUIPhase();
            UnityEngine.Debug.Log("[LevelManager] 商店阶段结束。");
        }
        
        /// <summary>
        /// 每次完成道具与海克斯领取后，回复最大生命值 50%。
        /// </summary>
        private void HealPlayerAfterRewards()
        {
            var stats = _playerModel?.Stats;
            if (stats == null) return;
            
            float maxHp = stats.GetStat(StatType.Health)?.FinalValue ?? 0f;
            if (maxHp <= 0f) return;

            float curHp = stats.GetCurrentStatValue(StatType.Health);
            float healAmount = maxHp * 0.5f;
            float targetHp = Mathf.Min(maxHp, curHp + healAmount);
            float actualHeal = targetHp - curHp;
            if (actualHeal <= 0f) return;

            stats.SetCurrentStatValue(StatType.Health, targetHp);
            UnityEngine.Debug.Log($"[LevelManager] 奖励结算回血: +{actualHeal:F1} ({curHp:F1} -> {targetHp:F1}/{maxHp:F1})");
        }

        /// <summary>
        /// 进入 UI 选择阶段：显示并解锁鼠标，禁用玩家输入。
        /// </summary>
        private void EnterUIPhase()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            _eventManager.Publish(GameEvents.GAME_INPUT_LOCKED);
        }

        /// <summary>
        /// 退出 UI 选择阶段：隐藏并锁定鼠标，恢复玩家输入。
        /// </summary>
        private void ExitUIPhase()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _eventManager.Publish(GameEvents.GAME_INPUT_UNLOCKED);
        }

        /// <summary>
        /// 海克斯选完后、进入下一关前的 10 秒倒计时（HUD 显示"距离下一层还有X秒"）。
        /// </summary>
        private async UniTask HandleNextLevelCountdown()
        {
            UnityEngine.Debug.Log("[LevelManager] 进入下一关倒计时...");
            for (int i = 10; i >= 1; i--)
            {
                if (_playerDead) return;
                _eventManager.Publish(GameEvents.LEVEL_COUNTDOWN_TICK, i);
                _playerStatusVm.SetCountdown(i, true);
                await UniTask.Delay(1000);
            }
            _eventManager.Publish(GameEvents.LEVEL_COUNTDOWN_TICK, 0);
            _playerStatusVm.SetCountdown(0, false);
        }

        // ───────────────────────────────────────────────
        // 进度推进
        // ───────────────────────────────────────────────

        private void AdvanceProgression(bool wasBoss)
        {
            if (wasBoss)
            {
                if (_sessionData.Chapter < 3)
                {
                    _sessionData.Chapter++;
                    _sessionData.Level = 1;
                    UnityEngine.Debug.Log($"[LevelManager] 大关通关！直接进入第 {_sessionData.Chapter} 大关第1层");
                    _eventManager.Publish(GameEvents.SESSION_INFO_REFRESH_REQUEST);
                    _playerStatusVm.RefreshSessionInfo();
                    StartNextChapterWithCountdown().Forget();
                    return;
                }
                else
                {
                    UnityEngine.Debug.Log("[LevelManager] 全游戏通关！进入结算窗口。");
                    HandleGameEndFlow(true).Forget();
                    return;
                }
            }

            _sessionData.Level++;
            UnityEngine.Debug.Log($"[LevelManager] 推进到小关: {_sessionData.Level}");
            // 立即刷新 HUD 层数显示，不等待下一帧重新绑定
            _eventManager.Publish(GameEvents.SESSION_INFO_REFRESH_REQUEST);
            _playerStatusVm.RefreshSessionInfo();
            RunLevelFlow().Forget();
        }

        private async UniTaskVoid StartNextChapterWithCountdown()
        {
            // Boss 后进入下一章第1层：先倒计时，再开刷怪（避免离开商店后立刻刷怪的突兀感）
            await HandleNextLevelCountdown();
            if (_playerDead) return;
            RunLevelFlow().Forget();
        }

        private async UniTask HandleGameEndFlow(bool isVictory)
        {
            if (_isGameEnding) return;
            _isGameEnding = true;

            EnterUIPhase();
            _gameResultVm.OpenResult(isVictory);

            _eventManager.Publish(GameEvents.UI_SHOW_WINDOW_REQUEST, UIPanelIds.GAME_RESULT_VIEW);
            var view = await _uiManager.ShowWindowAsync(UIPanelIds.GAME_RESULT_VIEW, _gameResultVm);
            if (view == null)
            {
                UnityEngine.Debug.LogError("[LevelManager] 结算窗口加载失败，等待修复后重试；当前不会自动返回大厅。");
                return;
            }

            await _gameResultVm.WaitForReturnLobby();

            _sessionData.ResetSession();
            _eventManager.Publish(GameEvents.SCENE_UNLOADED);
            _uiManager.OnSceneUnloaded();
            _eventManager.Publish(GameEvents.SCENE_LOAD_LOBBY_REQUEST);
            _sceneFlow.LoadLobbyScenes().Forget();
        }

        // ───────────────────────────────────────────────
        // ITickable / IDisposable
        // ───────────────────────────────────────────────

        public override void Tick()
        {
            _buffSystem.Tick(Time.deltaTime);
        }

        public override void Dispose()
        {
            _eventManager.Unsubscribe<bool>(GameEvents.PLAYER_DEAD, OnPlayerDead).Forget();
            _eventManager.Unsubscribe(GameEvents.WAVE_COMPLETED, OnWaveCompleted).Forget();
            _isLevelRunning = false;
        }
    }
}
using System;
using Events;
using Interface.IUntiy;
using UnityEngine;

namespace Dream
{
    [GameSystem(CollectType.Auto)]
    public class PlayerMoveSystem : GameSystem, ILateUpdate
    {


        /// <summary>
        /// 初始化玩家移动系统
        /// </summary>
        public override void Init()
        {

            //
            // // 订阅移动数据事件
            // EventManager.Instance.Subscribe<Vector2>(GameEvents.PLAYER_MOVE_PERFORMED, OnMovementPerformed);
            // EventManager.Instance.Subscribe<Vector2>(GameEvents.PLAYER_MOVE_CANCELED, OnMovementCanceled);
            //
            // // 订阅移动状态事件
            // EventManager.Instance.Subscribe(GameEvents.PLAYER_MOVE_STARTED, OnMovementStarted);
            // EventManager.Instance.Subscribe(GameEvents.PLAYER_MOVE_STOPPED, OnMovementStopped);
            
            Debug.Log("PlayerMoveSystem: 初始化完成");
        }




        /// <summary>
        /// 手动释放系统资源
        /// </summary>
        public override void ManualDispose()
        {
            // EventManager.Instance.Unsubscribe<Vector2>(GameEvents.PLAYER_MOVE_PERFORMED, OnMovementPerformed);
            // EventManager.Instance.Unsubscribe<Vector2>(GameEvents.PLAYER_MOVE_CANCELED, OnMovementCanceled);
            // EventManager.Instance.Unsubscribe(GameEvents.PLAYER_MOVE_STARTED, OnMovementStarted);
            // EventManager.Instance.Unsubscribe(GameEvents.PLAYER_MOVE_STOPPED, OnMovementStopped);
            Debug.Log("PlayerMoveSystem: 资源清理完成");
        }
        
        public void LateUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
using DreamSystem.Player;
using UnityEngine;
namespace StateMachine.Base
{
    public abstract class MovementState
    {
        protected readonly KccMoveController kccMoveController;

        protected MovementState(KccMoveController kccMoveController)
        {
            this.kccMoveController = kccMoveController;
        }
        
        /// <summary>
        /// 进入状态时调用一次 (例如：播放动画、重置重力)
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// 退出状态时调用一次 (例如：清理特效、恢复参数)
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// 对应 Unity 的 Update。用于处理输入检测、状态切换判断 (CheckTransitions)。
        /// </summary>
        public virtual void OnUpdate(float deltaTime) { }
        
        /// <summary>
        /// 对应 KCC 的 UpdateVelocity。
        /// 在这里编写具体的移动逻辑 (重力、摩擦力、阻力)。
        /// </summary>
        public virtual void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime) { }

        /// <summary>
        /// 对应 KCC 的 UpdateRotation。
        /// 在这里编写具体的旋转逻辑。
        /// </summary>
        public virtual void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime) { }
        
    }
}
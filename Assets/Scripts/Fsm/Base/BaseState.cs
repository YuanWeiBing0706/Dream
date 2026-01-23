using DreamManager;
using DreamSystem.Player;
using UnityEngine;
namespace Fsm.Base
{
    public abstract class BaseState
    {
        /// <summary>
        /// KCC 移动控制器引用，供子类访问
        /// </summary>
        protected readonly KccMoveController kccMoveController;
        protected readonly EventManager eventManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="kccMoveController">控制器实例</param>
        /// <param name="eventManager">事件控制器</param>
        protected BaseState(KccMoveController kccMoveController,EventManager eventManager)
        {
            this.kccMoveController = kccMoveController;
            this.eventManager = eventManager;
        }

        /// <summary>
        /// 进入状态时调用一次
        /// </summary>
        public virtual void OnEnter()
        {
            
        }

        /// <summary>
        /// 退出状态时调用一次
        /// </summary>
        public virtual void OnExit()
        {
            
        }

        /// <summary>
        /// 用于处理输入检测、状态切换判断。
        /// </summary>
        /// <param name="deltaTime">时间步长</param>
        public virtual void OnUpdate(float deltaTime)
        {
            
        }

        /// <summary>
        /// 对应 KCC 的 UpdateVelocity。
        /// 在这里编写具体的移动逻辑 (重力、摩擦力、阻力)。
        /// </summary>
        /// <param name="currentVelocity">当前速度向量（引用）</param>
        /// <param name="deltaTime">时间步长</param>
        public virtual void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
           
        }

        /// <summary>
        /// 对应 KCC 的 UpdateRotation。
        /// 在这里编写具体的旋转逻辑。
        /// </summary>
        /// <param name="currentRotation">当前旋转四元数（引用）</param>
        /// <param name="deltaTime">时间步长</param>
        public virtual void OnUpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
          
        }

    }
}
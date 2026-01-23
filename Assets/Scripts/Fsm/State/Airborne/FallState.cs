using DreamManager;
using DreamSystem.Player;
using Events;
using Fsm.Base;
using UnityEngine;
namespace Fsm.State.Airborne
{
    public class FallState : AirborneState
    {
        public FallState(KccMoveController kccMoveController,EventManager eventManager) : base(kccMoveController,eventManager) { }

        public override void OnEnter()
        {
            base.OnEnter();
            eventManager.Publish(GameEvents.PLAYER_FALL_ANIMATION);
        }
        
        public override void OnUpdate(float deltaTime)
        {
            // 显式工具方法调用
            if (CheckLanding()) return;   // 必须检测落地
            if (CheckAirDash()) return;   // 允许空中冲刺
        }

        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            base.OnUpdateVelocity(ref currentVelocity, deltaTime); // 应用重力和空气阻力
        }
        
        
    }
}
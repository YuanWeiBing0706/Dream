using DreamManager;
using DreamSystem.Player;
using Events;
using Fsm.Base;
using UnityEngine;

namespace Fsm.State.Grounded
{
    public class RollState : GroundedState
    {
        private float _timer;
        private Vector3 _rollDir;

        public RollState(KccMoveController kccMoveController, EventManager eventManager) : base(kccMoveController, eventManager) { }

        public override void OnEnter()
        {
            base.OnEnter();
            _timer = 0;

            _rollDir = kccMoveController.currentInputs.moveDirection.normalized;
            // 防呆：没按键就朝脸滚
            if (_rollDir == Vector3.zero)
            {
                _rollDir = kccMoveController.kinematicCharacterMotor.CharacterForward;
            }

            eventManager.Publish(GameEvents.PLAYER_ROLL_ANIMATION);
        }

        public override void OnUpdateVelocity(ref Vector3 velocity, float deltaTime)
        {
            // 物理层：还是要让父类帮忙算坡度，不然下坡会飞出去
            base.OnUpdateVelocity(ref velocity, deltaTime);

            // 强行覆盖速度，无视玩家输入
            velocity = _rollDir * kccMoveController.rollSpeed;
        }

        public override void OnUpdate(float deltaTime)
        {
            // 只检测物理跌落（禁止跳跃/闪避打断）
            if (CheckFalling()) return;

            // 计时器逻辑
            _timer += deltaTime;
            if (_timer > kccMoveController.rollDuration)
            {
                kccMoveController.StateMachine.TransitionTo(kccMoveController.moveState);
            }
        }
    }
}
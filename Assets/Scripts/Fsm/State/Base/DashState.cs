using DreamManager;
using DreamSystem.Player;
using Events;
using Fsm.Base;
using UnityEngine;
namespace Fsm.State.Base
{
    public class DashState : BaseState
    {
        private float _timer;
        private Vector3 _dashDir;
        private bool _startedOnGround;

        public DashState(KccMoveController kccMoveController,EventManager eventManager) : base(kccMoveController,eventManager) { }

        public override void OnEnter()
        {
            base.OnEnter();
            _timer = 0f;
            
            _startedOnGround = kccMoveController.kinematicCharacterMotor.GroundingStatus.IsStableOnGround;
            
            if (!_startedOnGround)
            {
                kccMoveController.hasUsedAirDash = true;
                kccMoveController.kinematicCharacterMotor.ForceUnground(); 
            }

            // 确定方向 (有输入往输入冲，没输入往脸冲)
            Vector3 inputDir = kccMoveController.currentInputs.moveDirection;
            if (inputDir.sqrMagnitude > 0.01f)
            {
                _dashDir = inputDir.normalized;
                kccMoveController.kinematicCharacterMotor.SetRotation(Quaternion.LookRotation(_dashDir, Vector3.up));
            }
            else
            {
                _dashDir = kccMoveController.kinematicCharacterMotor.CharacterForward;
            }
            
            // 播放动画
            eventManager.Publish(GameEvents.PLAYER_DODGE_CANCELED);
        }

        public override void OnUpdate(float deltaTime)
        {
            if (kccMoveController.currentInputs.jumpDown)
            {
                if (kccMoveController.kinematicCharacterMotor.GroundingStatus.IsStableOnGround || _startedOnGround)
                {
                    kccMoveController.StateMachine.TransitionTo(kccMoveController.jumpState);
                    return;
                }
            }
            
            _timer += deltaTime;
            if (_timer > kccMoveController.dashDuration)
            {
                if (kccMoveController.kinematicCharacterMotor.GroundingStatus.IsStableOnGround)
                {
                    kccMoveController.StateMachine.TransitionTo(kccMoveController.moveState);
                }
                else
                {
                    kccMoveController.StateMachine.TransitionTo(kccMoveController.fallState);
                }
            }
        }

        public override void OnUpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // 这一帧在地上，就贴地跑；下一帧飞出去了，就直线飞。
            // 不需要切换状态，只需要切换公式。
            if (kccMoveController.kinematicCharacterMotor.GroundingStatus.IsStableOnGround)
            {
                Vector3 slopeDir = kccMoveController.kinematicCharacterMotor.GetDirectionTangentToSurface(_dashDir, kccMoveController.kinematicCharacterMotor.GroundingStatus.GroundNormal);
                currentVelocity = slopeDir * kccMoveController.dashSpeed;
            }
            else
            {
                currentVelocity = _dashDir * kccMoveController.dashSpeed;
            }
        }
    }
}
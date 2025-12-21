using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;

namespace KinematicCharacterController.Walkthrough.ClimbingLadders
{
    public enum CharacterState
    {
        Default,
        Climbing,
    }

    public enum ClimbingState
    {
        Anchoring,
        Climbing,
        DeAnchoring
    }

    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool CrouchDown;
        public bool CrouchUp;
        public bool ClimbLadder;
    }

    public class MyCharacterController : MonoBehaviour, ICharacterController
    {
        public KinematicCharacterMotor Motor;

        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15;
        public float OrientationSharpness = 10;
        public float MaxStableDistanceFromLedge = 5f;
        [Range(0f, 180f)]
        public float MaxStableDenivelationAngle = 180f;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 10f;
        public float AirAccelerationSpeed = 5f;
        public float Drag = 0.1f;

        [Header("Jumping")]
        public bool AllowJumpingWhenSliding = false;
        public bool AllowDoubleJump = false;
        public bool AllowWallJump = false;
        public float JumpSpeed = 10f;
        public float JumpPreGroundingGraceTime = 0f;
        public float JumpPostGroundingGraceTime = 0f;

        [Header("Ladder Climbing")]
        public float ClimbingSpeed = 4f;
        public float AnchoringDuration = 0.25f;
        public LayerMask InteractionLayer;

        [Header("Misc")]
        public List<Collider> IgnoredColliders = new List<Collider>();
        public bool OrientTowardsGravity = false;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public Transform MeshRoot;

        public CharacterState CurrentCharacterState { get; private set; }

        private Collider[] _probedColliders = new Collider[8];
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;
        private bool _jumpRequested = false;
        private bool _jumpConsumed = false;
        private bool _doubleJumpConsumed = false;
        private bool _jumpedThisFrame = false;
        private bool _canWallJump = false;
        private Vector3 _wallJumpNormal;
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private float _timeSinceLastAbleToJump = 0f;
        private Vector3 _internalVelocityAdd = Vector3.zero;
        private bool _shouldBeCrouching = false;
        private bool _isCrouching = false;

        // 梯子变量
        private float _ladderUpDownInput;
        private MyLadder _activeLadder { get; set; }
        private ClimbingState _internalClimbingState;
        private ClimbingState _climbingState
        {
            get
            {
                return _internalClimbingState;
            }
            set
            {
                _internalClimbingState = value;
                _anchoringTimer = 0f;
                _anchoringStartPosition = Motor.TransientPosition;
                _anchoringStartRotation = Motor.TransientRotation;
            }
        }
        private Vector3 _ladderTargetPosition;
        private Quaternion _ladderTargetRotation;
        private float _onLadderSegmentState = 0;
        private float _anchoringTimer = 0f;
        private Vector3 _anchoringStartPosition = Vector3.zero;
        private Quaternion _anchoringStartRotation = Quaternion.identity;
        private Quaternion _rotationBeforeClimbing = Quaternion.identity;

        private void Start()
        {
            // 分配给电机
            Motor.CharacterController = this;

            // 处理初始状态
            TransitionToState(CharacterState.Default);
        }

        /// <summary>
        /// 处理移动状态转换和进入/退出回调
        /// </summary>
        public void TransitionToState(CharacterState newState)
        {
            CharacterState tmpInitialState = CurrentCharacterState;
            OnStateExit(tmpInitialState, newState);
            CurrentCharacterState = newState;
            OnStateEnter(newState, tmpInitialState);
        }

        /// <summary>
        /// 进入状态时的事件
        /// </summary>
        public void OnStateEnter(CharacterState state, CharacterState fromState)
        {
            switch (state)
            {
                case CharacterState.Default:
                    {
                        break;
                    }
                case CharacterState.Climbing:
                    {
                        _rotationBeforeClimbing = Motor.TransientRotation;

                        Motor.SetMovementCollisionsSolvingActivation(false);
                        Motor.SetGroundSolvingActivation(false);
                        _climbingState = ClimbingState.Anchoring;

                        // 存储要吸附到的目标位置和旋转
                        _ladderTargetPosition = _activeLadder.ClosestPointOnLadderSegment(Motor.TransientPosition, out _onLadderSegmentState);
                        _ladderTargetRotation = _activeLadder.transform.rotation;
                        break;
                    }
            }
        }

        /// <summary>
        /// 退出状态时的事件
        /// </summary>
        public void OnStateExit(CharacterState state, CharacterState toState)
        {
            switch (state)
            {
                case CharacterState.Default:
                    {
                        break;
                    }
                case CharacterState.Climbing:
                    {
                        Motor.SetMovementCollisionsSolvingActivation(true);
                        Motor.SetGroundSolvingActivation(true);
                        break;
                    }
            }
        }

        /// <summary>
        /// 由MyPlayer每帧调用，以告诉角色其输入是什么
        /// </summary>
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            // 处理梯子转换
            _ladderUpDownInput = inputs.MoveAxisForward;
            if (inputs.ClimbLadder)
            {
                if (Motor.CharacterOverlap(Motor.TransientPosition, Motor.TransientRotation, _probedColliders, InteractionLayer, QueryTriggerInteraction.Collide) > 0)
                {
                    if (_probedColliders[0] != null)
                    {
                        // 处理梯子
                        MyLadder ladder = _probedColliders[0].gameObject.GetComponent<MyLadder>();
                        if (ladder)
                        {
                            // 转换到梯子攀爬状态
                            if (CurrentCharacterState == CharacterState.Default)
                            {
                                _activeLadder = ladder;
                                TransitionToState(CharacterState.Climbing);
                            }
                            // 转换回默认移动状态
                            else if (CurrentCharacterState == CharacterState.Climbing)
                            {
                                _climbingState = ClimbingState.DeAnchoring;
                                _ladderTargetPosition = Motor.TransientPosition;
                                _ladderTargetRotation = _rotationBeforeClimbing;
                            }
                        }
                    }
                }
            }

            // 限制输入
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

            // 计算角色平面上的摄像机方向和旋转
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
            }
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        // 移动和视角输入
                        _moveInputVector = cameraPlanarRotation * moveInputVector;
                        _lookInputVector = cameraPlanarDirection;

                        // 跳跃输入
                        if (inputs.JumpDown)
                        {
                            _timeSinceJumpRequested = 0f;
                            _jumpRequested = true;
                        }

                        // 蹲下输入
                        if (inputs.CrouchDown)
                        {
                            _shouldBeCrouching = true;

                            if (!_isCrouching)
                            {
                                _isCrouching = true;
                                Motor.SetCapsuleDimensions(0.5f, 1f, 0.5f);
                                MeshRoot.localScale = new Vector3(1f, 0.5f, 1f);
                            }
                        }
                        else if (inputs.CrouchUp)
                        {
                            _shouldBeCrouching = false;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// （由KinematicCharacterMotor在其更新周期中调用）
        /// 在角色开始其移动更新之前调用
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// （由KinematicCharacterMotor在其更新周期中调用）
        /// 在这里告诉角色其旋转应该是什么。这是您应该设置角色旋转的唯一位置
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        if (_lookInputVector != Vector3.zero && OrientationSharpness > 0f)
                        {
                            // 从当前视角方向平滑插值到目标视角方向
                            Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                            // 设置当前旋转（将被KinematicCharacterMotor使用）
                            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                        }
                        if (OrientTowardsGravity)
                        {
                            // 从当前向上方向旋转以反转重力
                            currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -Gravity) * currentRotation;
                        }
                        break;
                    }
                case CharacterState.Climbing:
                    {
                        switch (_climbingState)
                        {
                            case ClimbingState.Climbing:
                                currentRotation = _activeLadder.transform.rotation;
                                break;
                            case ClimbingState.Anchoring:
                            case ClimbingState.DeAnchoring:
                                currentRotation = Quaternion.Slerp(_anchoringStartRotation, _ladderTargetRotation, (_anchoringTimer / AnchoringDuration));
                                break;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// （由KinematicCharacterMotor在其更新周期中调用）
        /// 在这里告诉角色其速度应该是什么。这是您可以设置角色速度的唯一位置
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        Vector3 targetMovementVelocity = Vector3.zero;
                        if (Motor.GroundingStatus.IsStableOnGround)
                        {
                            // 在斜坡上重新定向速度
                            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                            // 计算目标速度
                            Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                            Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                            targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                            // 平滑移动速度
                            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
                        }
                        else
                        {
                            // 添加移动输入
                            if (_moveInputVector.sqrMagnitude > 0f)
                            {
                                targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;

                                // 防止在非稳定斜坡上通过空中移动攀爬
                                if (Motor.GroundingStatus.FoundAnyGround)
                                {
                                    Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                                    targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                                }

                                Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                                currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
                            }

                            // 重力
                            currentVelocity += Gravity * deltaTime;

                            // 阻力
                            currentVelocity *= (1f / (1f + (Drag * deltaTime)));
                        }

                        // 处理跳跃
                        {
                            _jumpedThisFrame = false;
                            _timeSinceJumpRequested += deltaTime;
                            if (_jumpRequested)
                            {
                                // 处理二段跳
                                if (AllowDoubleJump)
                                {
                                    if (_jumpConsumed && !_doubleJumpConsumed && (AllowJumpingWhenSliding ? !Motor.GroundingStatus.FoundAnyGround : !Motor.GroundingStatus.IsStableOnGround))
                                    {
                                        Motor.ForceUnground(0.1f);

                                        // 添加到返回速度并重置跳跃状态
                                        currentVelocity += (Motor.CharacterUp * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                                        _jumpRequested = false;
                                        _doubleJumpConsumed = true;
                                        _jumpedThisFrame = true;
                                    }
                                }

                                // 查看我们是否真的被允许跳跃
                                if (_canWallJump ||
                                    (!_jumpConsumed && ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime)))
                                {
                                    // 在取消接地之前计算跳跃方向
                                    Vector3 jumpDirection = Motor.CharacterUp;
                                    if (_canWallJump)
                                    {
                                        jumpDirection = _wallJumpNormal;
                                    }
                                    else if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
                                    {
                                        jumpDirection = Motor.GroundingStatus.GroundNormal;
                                    }

                                    // 使角色在其下一次更新时跳过地面探测/吸附。
                                    // 如果这里没有这行代码，角色在尝试跳跃时会保持吸附在地面上。尝试注释掉这行代码看看。
                                    Motor.ForceUnground(0.1f);

                                    // 添加到返回速度并重置跳跃状态
                                    currentVelocity += (jumpDirection * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                                    _jumpRequested = false;
                                    _jumpConsumed = true;
                                    _jumpedThisFrame = true;
                                }
                            }

                            // 重置墙壁跳跃
                            _canWallJump = false;
                        }

                        // 考虑附加速度
                        if (_internalVelocityAdd.sqrMagnitude > 0f)
                        {
                            currentVelocity += _internalVelocityAdd;
                            _internalVelocityAdd = Vector3.zero;
                        }
                        break;
                    }
                case CharacterState.Climbing:
                    {
                        currentVelocity = Vector3.zero;

                        switch (_climbingState)
                        {
                            case ClimbingState.Climbing:
                                currentVelocity = (_ladderUpDownInput * _activeLadder.transform.up).normalized * ClimbingSpeed;
                                break;
                            case ClimbingState.Anchoring:
                            case ClimbingState.DeAnchoring:
                                Vector3 tmpPosition = Vector3.Lerp(_anchoringStartPosition, _ladderTargetPosition, (_anchoringTimer / AnchoringDuration));
                                currentVelocity = Motor.GetVelocityForMovePosition(Motor.TransientPosition, tmpPosition, deltaTime);
                                break;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// （由KinematicCharacterMotor在其更新周期中调用）
        /// 在角色完成其移动更新后调用
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        // 处理跳跃相关值
                        {
                            // 处理跳跃预地面宽限期
                            if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
                            {
                                _jumpRequested = false;
                            }

                            if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
                            {
                                // 如果我们在地面表面上，重置跳跃值
                                if (!_jumpedThisFrame)
                                {
                                    _doubleJumpConsumed = false;
                                    _jumpConsumed = false;
                                }
                                _timeSinceLastAbleToJump = 0f;
                            }
                            else
                            {
                                // 跟踪自我们上次能够跳跃以来的时间（用于宽限期）
                                _timeSinceLastAbleToJump += deltaTime;
                            }
                        }

                        // 处理取消蹲下
                        if (_isCrouching && !_shouldBeCrouching)
                        {
                            // 对角色站立高度进行重叠测试，查看是否有任何障碍物
                            Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                            if (Motor.CharacterOverlap(
                                Motor.TransientPosition,
                                Motor.TransientRotation,
                                _probedColliders,
                                Motor.CollidableLayers,
                                QueryTriggerInteraction.Ignore) > 0)
                            {
                                // 如果有障碍物，保持蹲下尺寸
                                Motor.SetCapsuleDimensions(0.5f, 1f, 0.5f);
                            }
                            else
                            {
                                // 如果没有障碍物，取消蹲下
                                MeshRoot.localScale = new Vector3(1f, 1f, 1f);
                                _isCrouching = false;
                            }
                        }
                        break;
                    }
                case CharacterState.Climbing:
                    {
                        switch (_climbingState)
                        {
                            case ClimbingState.Climbing:
                                // 检测在攀爬过程中离开梯子
                                _activeLadder.ClosestPointOnLadderSegment(Motor.TransientPosition, out _onLadderSegmentState);
                                if (Mathf.Abs(_onLadderSegmentState) > 0.05f)
                                {
                                    _climbingState = ClimbingState.DeAnchoring;

                                    // 如果我们高于梯子顶部点
                                    if (_onLadderSegmentState > 0)
                                    {
                                        _ladderTargetPosition = _activeLadder.TopReleasePoint.position;
                                        _ladderTargetRotation = _activeLadder.TopReleasePoint.rotation;
                                    }
                                    // 如果我们低于梯子底部点
                                    else if (_onLadderSegmentState < 0)
                                    {
                                        _ladderTargetPosition = _activeLadder.BottomReleasePoint.position;
                                        _ladderTargetRotation = _activeLadder.BottomReleasePoint.rotation;
                                    }
                                }
                                break;
                            case ClimbingState.Anchoring:
                            case ClimbingState.DeAnchoring:
                                // 检测从锚定状态转换出来
                                if (_anchoringTimer >= AnchoringDuration)
                                {
                                    if (_climbingState == ClimbingState.Anchoring)
                                    {
                                        _climbingState = ClimbingState.Climbing;
                                    }
                                    else if (_climbingState == ClimbingState.DeAnchoring)
                                    {
                                        TransitionToState(CharacterState.Default);
                                    }
                                }

                                // 跟踪自我们开始锚定以来的时间
                                _anchoringTimer += deltaTime;
                                break;
                        }
                        break;
                    }
            }
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (IgnoredColliders.Contains(coll))
            {
                return false;
            }
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        // 只有当我们不在地面上稳定且正在移动撞击障碍物时才能墙壁跳跃
                        if (AllowWallJump && !Motor.GroundingStatus.IsStableOnGround && !hitStabilityReport.IsStable)
                        {
                            _canWallJump = true;
                            _wallJumpNormal = hitNormal;
                        }
                        break;
                    }
            }
        }

        public void AddVelocity(Vector3 velocity)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    {
                        _internalVelocityAdd += velocity;
                        break;
                    }
            }
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }
    }
}
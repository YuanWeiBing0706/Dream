using KinematicCharacterController;
using Struct;
using UnityEngine;

namespace Interface
{
    public interface IPlayerMoveContext
    {
        MoveInputs MoveInputs { get; }

        KinematicCharacterMotor Motor { get; }

        float MaxStableMoveSpeed { get; }

        float StableMovementSharpness { get; }

        float MaxAirMoveSpeed { get; }

        float AirAccelerationSpeed { get; }

        float Drag { get; }

        float RotationSpeed { get; }

        float JumpSpeed { get; }

        float JumpPreGroundingGraceTime { get; }

        float JumpPostGroundingGraceTime { get; }

        Vector3 Gravity { get; }

        float RollSpeed { get; }

        float DashSpeed { get; }

        float DashDuration { get; }

        float RollDuration { get; }

        bool HasUsedAirDash { get; set; }

        bool HasUsedJump { get; set; }
    }
}
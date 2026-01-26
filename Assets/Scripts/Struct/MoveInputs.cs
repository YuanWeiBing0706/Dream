using UnityEngine;

namespace Struct
{
    public struct MoveInputs
    {
        public Vector3 moveDirection;
        public Quaternion cameraRotation;
        public bool jumpDown;
        public bool isDodge;
        public bool isLockedOn;
    }
}

using UnityEngine;
namespace Struct
{
    /// <summary>
    /// 输入数据包：包含所有控制角色需要的原始指令
    /// </summary>
    public struct KccInputs
    {
        // 世界坐标下的移动方向 (长度为1，或者长度为0)
        public Vector3 moveDirection; 
        
        // 相机的朝向 (用于计算相对方向)
        public Quaternion cameraRotation;
        
        // 是否按下了跳跃键 (这一帧)
        public bool jumpDown;
    }
}
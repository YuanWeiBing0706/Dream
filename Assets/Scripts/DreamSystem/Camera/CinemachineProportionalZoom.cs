using Cinemachine;
using Const;
using Cysharp.Threading.Tasks;
using DreamManager;
using UnityEngine;

namespace DreamSystem.Camera
{
    public class CinemachineProportionalZoom : GameSystem
    {
        // Settings
        private const float ZOOM_SPEED = 1f;
        private const float ZOOM_DAMPING = 5f;
        private const float MIN_SCALE = 0.5f;
        private const float MAX_SCALE = 3.0f; 
        private const float INITIAL_SCALE = 1.35f; // 初始镜头距离倍率（>1 更远）

        // State
        private float _targetScale = INITIAL_SCALE; 
        private float _currentScale = INITIAL_SCALE; 
        
        private OrbitSettings[] _originalOrbits = new OrbitSettings[3];

        // Dependencies
        private readonly CinemachineFreeLook _cinemachineFreeLook;
        private readonly EventManager _eventManager;

        // 保存 UI 阶段前的轴速度上限，以便解锁时恢复
        private float _savedXMaxSpeed;
        private float _savedYMaxSpeed;

        public CinemachineProportionalZoom(CinemachineFreeLook cinemachineFreeLook, EventManager eventManager)
        {
            _cinemachineFreeLook = cinemachineFreeLook;
            _eventManager = eventManager;
        }

        public override void Start()
        {
            if (_cinemachineFreeLook == null) return;

            //记录相机的初始数值，否则缩放基准是 0
            for (int i = 0; i < 3; i++)
            {
                _originalOrbits[i].height = _cinemachineFreeLook.m_Orbits[i].m_Height;
                _originalOrbits[i].radius = _cinemachineFreeLook.m_Orbits[i].m_Radius;
            }

            // 保存原始轴速度上限，解锁时恢复
            _savedXMaxSpeed = _cinemachineFreeLook.m_XAxis.m_MaxSpeed;
            _savedYMaxSpeed = _cinemachineFreeLook.m_YAxis.m_MaxSpeed;

            _eventManager.Subscribe<float>(GameEvents.PLAYER_CAMERA_ZOOM, OnCameraZoom);
            _eventManager.Subscribe(GameEvents.GAME_INPUT_LOCKED, OnInputLocked);
            _eventManager.Subscribe(GameEvents.GAME_INPUT_UNLOCKED, OnInputUnlocked);
        }

        /// <summary>
        /// 进入 UI 阶段：将轴速度上限清零。
        /// Cinemachine 内部 AxisState.Update() 每帧用 m_MaxSpeed 乘以输入量计算新值，
        /// 设为 0 后无论任何输入源（Legacy / New Input System / CinemachineInputProvider）
        /// 均无法改变轴值，不存在 LateTick 与 LateUpdate 执行顺序带来的一帧抖动。
        /// </summary>
        private void OnInputLocked()
        {
            if (_cinemachineFreeLook == null) return;
            _cinemachineFreeLook.m_XAxis.m_MaxSpeed = 0f;
            _cinemachineFreeLook.m_YAxis.m_MaxSpeed = 0f;
        }

        /// <summary>
        /// 离开 UI 阶段：恢复轴速度上限。
        /// </summary>
        private void OnInputUnlocked()
        {
            if (_cinemachineFreeLook == null) return;
            _cinemachineFreeLook.m_XAxis.m_MaxSpeed = _savedXMaxSpeed;
            _cinemachineFreeLook.m_YAxis.m_MaxSpeed = _savedYMaxSpeed;
        }
        
        private void OnCameraZoom(float scrollAmount)
        {
            if (Mathf.Abs(scrollAmount) > 0.001f)
            {
                // 注意：这里可能需要根据你的滚轮方向决定是 += 还是 -=
                // 通常向下滚动是负数，想拉远(Scale变大)，所以是 -=
                _targetScale -= scrollAmount * ZOOM_SPEED * 0.1f;
                _targetScale = Mathf.Clamp(_targetScale, MIN_SCALE, MAX_SCALE);
            }
        }

        public override void LateTick()
        {
            if (_cinemachineFreeLook == null) return;
            ZoomCameraView();
        }

        private void ZoomCameraView()
        {
            if (Mathf.Abs(_currentScale - _targetScale) > 0.0001f)
            {
                _currentScale = Mathf.Lerp(_currentScale, _targetScale, Time.deltaTime * ZOOM_DAMPING);

                for (int i = 0; i < 3; i++)
                {
                    _cinemachineFreeLook.m_Orbits[i].m_Height = _originalOrbits[i].height * _currentScale;
                    _cinemachineFreeLook.m_Orbits[i].m_Radius = _originalOrbits[i].radius * _currentScale;
                }
            }
        }

        public override void Dispose()
        {
            _eventManager.Unsubscribe<float>(GameEvents.PLAYER_CAMERA_ZOOM, OnCameraZoom).Forget();
            _eventManager.Unsubscribe(GameEvents.GAME_INPUT_LOCKED, OnInputLocked).Forget();
            _eventManager.Unsubscribe(GameEvents.GAME_INPUT_UNLOCKED, OnInputUnlocked).Forget();
        }
    }
    
    // Data Storage
    struct OrbitSettings
    {
        public float height; 
        public float radius;
    }
}
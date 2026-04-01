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

        // State
        private float _targetScale = 1f; 
        private float _currentScale = 1f; 
        
        private OrbitSettings[] _originalOrbits = new OrbitSettings[3];

        // Dependencies
        private readonly CinemachineFreeLook _cinemachineFreeLook;
        private readonly EventManager _eventManager;
        
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

            _eventManager.Subscribe<float>(GameEvents.PLAYER_CAMERA_ZOOM, OnCameraZoom);
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
        }
    }
    
    // Data Storage
    struct OrbitSettings
    {
        public float height; 
        public float radius;
    }
}
using Dream;
using Model.Player;
using UnityEngine;
namespace DreamSystem.Player
{
    public class PlayerMoveSystem : GameSystem
    {

        public float moveSpeed = 5f;
        private Vector2 _moveInput;
        private bool _isActive;
        
        readonly PlayerModel _player; 
        readonly EventManager _events; 
        
        public PlayerMoveSystem(PlayerModel player, EventManager events)
        {
            _player = player;
            _events = events;
        }

        public override void Start()
        {
            //Todo 移动事件订阅
            if (_player!=null)
            {
                UnityEngine.Debug.Log("PlayerMoveSystem 初始化成功");
            }
        }

        public void Activate()
        {
            _isActive = true;
        }

        public void LateTick()
        {
            if (!_isActive) return;
        }

        public override void Dispose()
        {
            // TODO: 取消订阅移动输入事件
        }
        
    }
}
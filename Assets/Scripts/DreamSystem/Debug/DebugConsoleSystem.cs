namespace DreamSystem.Debug
{
    public class DebugConsoleSystem : GameSystem
    {
        private bool _isActive;
        public DebugConsoleSystem()
        {

        }
        
        public void Activate()
        {
            _isActive = true;
        }
        
        public void LateTick()
        {
            if(!_isActive) return;
        }
    }
}
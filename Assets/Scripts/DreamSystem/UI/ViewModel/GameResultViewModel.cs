using Cysharp.Threading.Tasks;
namespace DreamSystem.UI.ViewModel
{
    public class GameResultViewModel : ViewModelBase
    {
        private UniTaskCompletionSource<bool> _returnLobbyTcs;

        // 【修改】移除所有数据属性（TitleText, DescText）

        // 【修改】简化该方法，它不再控制文本显示，仅作为面板打开通知
        // 外部系统（如LevelManager）应在需要打开此界面时调用此方法。
        public void OpenResult(bool isVictory)
        {
            // 此处可能需要添加：根据是胜利或失败播放不同的结算音效
            
            _returnLobbyTcs = new UniTaskCompletionSource<bool>();
            
            // 此处通知View刷新。由于没有数据payload，View可能不需要进行具体的数据绑定操作，只是完成Open流程。
            NotifyRefresh();
        }
        
        // 它会创建一个未完成的Task供外部await，直到玩家点击View上的按钮。
        public UniTask<bool> WaitForReturnLobby()
        {
            return _returnLobbyTcs?.Task ?? UniTask.FromResult(true);
        }
        
        // 此方法会解除 WaitForReturnLobby() 的阻塞，通知外部流程继续执行。
        public void ConfirmReturnLobby()
        {
            _returnLobbyTcs?.TrySetResult(true);
        }
    }
}
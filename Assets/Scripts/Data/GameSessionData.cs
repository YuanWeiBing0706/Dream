using Enum.Game;
namespace Data
{
    public class GameSessionData
    {
        /// 选择职业的ID
        public string SelectedCharacterId { get; set; } = "DoubleSword";
        
        /// 大关
        public int Chapter { get; set; } = 1;

        /// 小关
        public int Level { get; set; } = 1;

        /// 当前金币总量
        public int CurrentCoinCount { get; set; }
        
        // 当前波次
        public Wave CurrentWave { get; set; } =  Wave.One;
        
        public void AddCoin(int amount)
        {
            CurrentCoinCount += amount;
        }
        
        public void ResetSession()
        {
            CurrentCoinCount = 0;
            CurrentWave = Wave.One;
            Chapter = 1;
            Level = 1;
        }
        
    }
}

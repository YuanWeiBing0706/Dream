namespace Data
{
    public class GameSessionData
    {
        /// 选择职业的ID（与 CharacterStatsConfig 中的 characterId 对应）
        public string SelectedCharacterId { get; set; } = "player";
        
        /// 大关
        public int Chapter { get; set; } = 1;

        /// 小关（1-3，其中第3关为 Boss 关）
        public int Level { get; set; } = 1;

        /// 当前金币总量
        public int CurrentCoinCount { get; set; }
        
        /// 当前剩余血量（0 表示满血）
        public float CurrentHp { get; set; }
        
        /// 已解锁的海克斯 ID 列表
        public System.Collections.Generic.List<string> UnlockedHexIds { get; set; } = new();

        /// 已拥有的道具 ID 列表（被动道具永久生效，主动道具使用后移除）
        public System.Collections.Generic.List<string> OwnedItemIds { get; set; } = new();
        
        public void AddCoin(int amount)
        {
            CurrentCoinCount += amount;
        }
        
        public void ResetSession()
        {
            CurrentCoinCount = 0;
            Chapter = 1;
            Level = 1;
            CurrentHp = 0;
            UnlockedHexIds.Clear();
            OwnedItemIds.Clear();
        }
    }
}

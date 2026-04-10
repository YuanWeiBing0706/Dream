namespace Struct
{
    /// <summary>
    /// 小关结算时，单条掉落结果（由 DropSystem.RollStageDrop 生成，ItemSelectViewModel 消费）。
    /// </summary>
    public class ItemDropResult
    {
        public bool IsGold;
        public int GoldAmount;
        public string ItemId;
        public int Count;
        /// 玩家是否勾选领取（默认 true，金币条目始终为 true）
        public bool IsSelected = true;
    }
}

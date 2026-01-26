namespace Function.Damageable
{
    /// <summary>
    /// 敌人受伤事件数据。
    /// </summary>
    public struct EnemyDamageData
    {
        public EnemyDamageHandler Handler;
        public float Amount;

        public EnemyDamageData(EnemyDamageHandler handler, float amount)
        {
            Handler = handler;
            Amount = amount;
        }
    }
}

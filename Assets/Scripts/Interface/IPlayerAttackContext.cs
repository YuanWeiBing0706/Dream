using Struct;

namespace Interface
{
    public interface IPlayerAttackContext
    {
        /// 当前攻击输入状态
        AttackInputs AttackInputs { get; }

        /// <summary>
        /// 消费轻攻击输入（读取后立即重置为 false）。
        /// </summary>
        /// <returns>是否有轻攻击输入</returns>
        bool ConsumeLightAttack();

        /// <summary>
        /// 消费重攻击输入（读取后立即重置为 false）。
        /// </summary>
        /// <returns>是否有重攻击输入</returns>
        bool ConsumeHeavyAttack();


        bool ConsumeFallAttack();
    }
}
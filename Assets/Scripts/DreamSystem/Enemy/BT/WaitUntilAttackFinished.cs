using NodeCanvas.Framework;

namespace DreamSystem.Enemy.BT
{
    public class WaitUntilAttackFinished : ActionTask<EnemyCombatSystem>
    {
        /// <summary>
        /// 当节点开始执行的第一帧调用
        /// </summary>
        protected override void OnExecute()
        {
            // 如果节点刚开始执行时，敌人就已经不在攻击状态了，直接结束并返回成功
            if (!agent.IsAttacking())
            {
                EndAction(true);
            }
        }

        /// <summary>
        /// 当节点处于 Running 状态时，行为树每帧（Tick）都会调用此方法
        /// </summary>
        protected override void OnUpdate()
        {
            // 检测敌人战斗系统的攻击状态
            if (!agent.IsAttacking())
            {
                // IsAttacking 返回 false，说明动画播放完毕且状态重置
                // 调用 EndAction(true) 向父节点 Sequence 汇报 Success，结束当前节点的执行
                EndAction(true);
            }
            // 注意：只要不调用 EndAction()，NodeCanvas 的 Action 节点就会默认一直向父节点返回 Running 状态。
        }
    }
}
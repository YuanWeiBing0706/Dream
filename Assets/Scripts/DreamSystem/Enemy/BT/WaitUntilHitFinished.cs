using NodeCanvas.Framework;

namespace DreamSystem.Enemy.BT
{
    public class WaitUntilHitFinished : ActionTask<EnemyAnimationSystem>
    {
        protected override void OnExecute()
        {
            // 如果节点刚开始执行时，动画就已经不在播放了，直接成功
            if (!agent.IsActionPlaying)
            {
                EndAction(true);
            }
        }

        protected override void OnUpdate()
        {
            // 检测动画系统的高优先级动作状态
            if (!agent.IsActionPlaying)
            {
                // IsActionPlaying 返回 false，说明受击动画播放完毕并已重置为待机
                EndAction(true);
            }
        }
    }
}

using NodeCanvas.Framework;

namespace DreamSystem.Enemy.BT
{
    public class ResetHitState : ActionTask<EnemyStatusSystem>
    {
        protected override void OnExecute()
        {
            // 重置受击标志，使得行为树可以跳出受击分支
            agent.SetHit(false);
            EndAction(true);
        }
    }
}

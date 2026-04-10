using DreamAttribute;
using Enum.Buff;

namespace DreamSystem.Damage.Buff.Logic
{
    /// <summary>
    /// 标记挂载型 Buff 逻辑。
    /// <para>
    /// 生效时向 CharacterStats 添加 <c>stringParam</c> 指定的字符串标记；
    /// 移除时撤销该标记。外部系统（如 DropSystem、LevelManager）通过
    /// <c>CharacterStats.HasFlag()</c> 读取标记决定行为。
    /// </para>
    /// <para>对应 CSV 中 logicType = AddFlag 的行。</para>
    /// </summary>
    [BuffLogic(BuffLogicType.AddFlag)]
    public class AddFlagBuffLogic : BuffBaseLogic
    {
        private string _flag;

        public override void OnApply()
        {
            _flag = buffInstance?.data?.stringParam ?? string.Empty;
            if (string.IsNullOrEmpty(_flag) || _flag == "None")
            {
                UnityEngine.Debug.LogWarning("[AddFlagBuffLogic] stringParam 为空，标记未挂载");
                return;
            }

            owner?.Stats?.AddFlag(_flag);
        }

        public override void OnRemove()
        {
            if (!string.IsNullOrEmpty(_flag) && _flag != "None")
                owner?.Stats?.RemoveFlag(_flag);
        }
    }
}

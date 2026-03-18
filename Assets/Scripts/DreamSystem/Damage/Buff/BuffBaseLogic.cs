using System.Collections.Generic;
using Enum.Buff;
using Interface;
using UnityEngine;
namespace DreamSystem.Damage.Buff
{
    /// <summary>
    /// Buff 逻辑基类。
    /// <para>所有具体 Buff 逻辑继承此类，配合 BuffLogicAttribute 注册到工厂。</para>
    /// <para>子类重写 OnApply / OnUpdate / OnRemove / OnStackChanged 实现具体效果。</para>
    /// </summary>
    public class BuffBaseLogic
    {
        /// Buff 挂载的目标实体（可访问 GameObject 和 CharacterStats）
        protected IBuffOwner owner;

        /// 当前 Buff 运行时实例（可读取 Data、Stack、BuffContainer 等信息）
        protected BuffInstance buffInstance;

        /// <summary>
        /// 初始化 Buff 逻辑：设置 owner 和 instance，然后调用 OnApply。
        /// </summary>
        public void Initialize(IBuffOwner owner, BuffInstance instance)
        {
            this.owner = owner;
            buffInstance = instance;
            OnApply();
        }

        /// <summary>
        /// Buff 首次生效时调用（如添加属性修改器）。
        /// </summary>
        public virtual void OnApply() { }

        /// <summary>
        /// 每帧/每回合更新时调用（如持续伤害、持续回血）。
        /// </summary>
        public virtual void OnUpdate(float deltaTime) { }

        /// <summary>
        /// Buff 移除时调用（如撤销属性修改器）。
        /// </summary>
        public virtual void OnRemove() { }

        /// <summary>
        /// 层数变化时调用（如根据层数调整效果强度）。
        /// </summary>
        public virtual void OnStackChanged(int newStack) { }

        /// <summary>
        /// 从 BuffContainer 的黑名单中移除指定标签。
        /// <para>用于 Buff 逻辑在特定条件下解除黑名单限制。</para>
        /// </summary>
        protected void RemoveBlacklistTags(IReadOnlyList<BuffTag> tags)
        {
            if (buffInstance == null || buffInstance.buffSystem == null)
            {
                return;
            }

            buffInstance.buffSystem.RemoveTagsFromBlacklist(tags, buffInstance);
        }
    }
}
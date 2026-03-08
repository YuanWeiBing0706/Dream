using System.Collections.Generic;
using System.Linq;
using DreamManager;
using DreamSystem.Damage.Buff;
using Enum.Buff;
using Interface;
using UnityEngine;
namespace DreamSystem.Damage
{
    /// <summary>
    /// Buff 容器（纯逻辑类）。
    /// <para>管理角色身上所有 Buff 的生命周期，不依赖 MonoBehaviour。</para>
    /// <para>职责：Buff 的添加/移除/叠层/到期处理/标签查询/标签黑名单/控制锁定/优先级排序。</para>
    /// </summary>
    public class BuffSystem
    {
        private readonly List<BuffInstance> _buffInstanceList = new List<BuffInstance>();
        private readonly BuffManager _buffManager;
        private readonly GameObject _owner;
        private readonly ControlGate _controlGate;

        /// 标签黑名单引用计数（被黑名单的标签 → 引用计数）
        private readonly Dictionary<BuffTag, int> _tagBlacklistRefCounts = new Dictionary<BuffTag, int>();

        /// 每个 Buff 实例贡献的黑名单标签（用于 Buff 移除时准确回退）
        private readonly Dictionary<BuffInstance, Dictionary<BuffTag, int>> _blacklistContributionsByBuff = new Dictionary<BuffInstance, Dictionary<BuffTag, int>>();

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="owner">Buff 效果作用的目标 GameObject</param>
        /// <param name="controlGate">控制锁定门（可为 null，不使用控制锁定）</param>
        /// <param name="buffManager">Buff 工厂（可为 null，使用默认实例）</param>
        public BuffSystem(GameObject owner, ControlGate controlGate = null, BuffManager buffManager = null)
        {
            _owner = owner;
            _controlGate = controlGate;
            _buffManager = buffManager ?? new BuffManager();
            _buffManager.Init();
        }

        public void TickTurn()
        {
            TickAllBuff(1f);
        }

        public void Tick(float deltaTime)
        {
            TickAllBuff(deltaTime);
        }

        private void TickAllBuff(float deltaTime)
        {
            for (int i = 0; i < _buffInstanceList.Count;)
            {
                BuffInstance buff = _buffInstanceList[i];
                buff.Tick(deltaTime);

                if (buff.TryResolveExpiration())
                {
                    RemoveBuffAt(i);
                    continue;
                }

                i++;
            }
        }

        public void AddBuff(string buffGuid)
        {
            AddBuff(buffGuid, null);
        }

        public void AddBuff(string buffGuid, object source)
        {
            (BuffData data, BuffBase logic) = _buffManager.GetBuff(buffGuid);
            if (data == null || logic == null)
            {
                return;
            }

            AddBuff(data, logic, source);
        }

        private void AddBuff(BuffData data, BuffBase logic, object source)
        {
            // 检查标签黑名单
            if (IsBlockedByTagBlacklist(data))
            {
                return;
            }

            // 检查叠加
            BuffInstance existing = FindStackTarget(data, source);
            if (existing != null)
            {
                existing.AddStack();
                return;
            }

            // 新建实例
            BuffInstance newBuff = new BuffInstance(data, logic, source, this);
            InsertBuffByPriority(newBuff);
            RegisterBlacklistTagsFromData(newBuff);
            RegisterControlLocksFromData(newBuff);
            logic.Initialize(_owner, newBuff);
        }

        /// <summary>
        /// 检查 Buff 的标签是否在黑名单中。
        /// </summary>
        private bool IsBlockedByTagBlacklist(BuffData data)
        {
            if (data == null || data.tags == null)
            {
                return false;
            }

            for (int i = 0; i < data.tags.Count; i++)
            {
                if (IsTagBlacklisted(data.tags[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 按优先级插入 Buff（优先级高的排在前面）。
        /// </summary>
        private void InsertBuffByPriority(BuffInstance buff)
        {
            int insertIndex = _buffInstanceList.FindIndex(existing => existing.data.priority < buff.data.priority);
            if (insertIndex < 0)
            {
                _buffInstanceList.Add(buff);
                return;
            }

            _buffInstanceList.Insert(insertIndex, buff);
        }

        private BuffInstance FindStackTarget(BuffData data, object source)
        {
            if (data.stackType == StackType.None)
            {
                return null;
            }

            if (data.stackType == StackType.Aggregate)
            {
                return _buffInstanceList.Find(b => b.data.buffID == data.buffID);
            }

            if (data.stackType == StackType.AggregateBySource)
            {
                return _buffInstanceList.Find(b => b.data.buffID == data.buffID && IsSameSource(b.source, source));
            }

            return null;
        }

        private bool IsSameSource(object a, object b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null)
            {
                return false;
            }

            return a.Equals(b);
        }

        // ============================================
        // 标签查询
        // ============================================

        public bool HasBuffWithTag(BuffTag tag)
        {
            return _buffInstanceList.Any(buff => HasTag(buff.data, tag));
        }

        public List<BuffInstance> GetBuffsByTag(BuffTag tag)
        {
            return _buffInstanceList.Where(buff => HasTag(buff.data, tag)).ToList();
        }

        public int RemoveBuffsByTag(BuffTag tag)
        {
            int removedCount = 0;

            for (int i = _buffInstanceList.Count - 1; i >= 0; i--)
            {
                BuffInstance buff = _buffInstanceList[i];
                if (!HasTag(buff.data, tag))
                {
                    continue;
                }

                RemoveBuffAt(i);
                removedCount++;
            }

            return removedCount;
        }

        /// <summary>
        /// 驱散指定标签的 Buff（等同于 RemoveBuffsByTag）。
        /// </summary>
        public int DispelBuffsByTag(BuffTag tag)
        {
            return RemoveBuffsByTag(tag);
        }

        private bool HasTag(BuffData data, BuffTag tag)
        {
            if (data == null || data.tags == null)
            {
                return false;
            }

            for (int i = 0; i < data.tags.Count; i++)
            {
                if (data.tags[i] == tag)
                {
                    return true;
                }
            }

            return false;
        }

        // ============================================
        // 按 ID / 实例移除
        // ============================================

        public void RemoveBuff(string buffId)
        {
            int index = _buffInstanceList.FindIndex(x => x.data.buffID == buffId);
            if (index < 0)
            {
                return;
            }

            RemoveBuffAt(index);
        }

        public bool RemoveBuff(BuffInstance target)
        {
            if (target == null)
            {
                return false;
            }

            int index = _buffInstanceList.IndexOf(target);
            if (index < 0)
            {
                return false;
            }

            RemoveBuffAt(index);
            return true;
        }

        public void MinusStack(string buffId)
        {
            BuffInstance target = _buffInstanceList.FirstOrDefault(x => x.data.buffID == buffId);
            if (target == null)
            {
                return;
            }

            int remainStack = target.MinusStack();
            if (remainStack <= 0)
            {
                RemoveBuff(target);
            }
        }

        public void ClearAllBuff()
        {
            for (int i = _buffInstanceList.Count - 1; i >= 0; i--)
            {
                RemoveBuffAt(i);
            }
        }

        // ============================================
        // 标签黑名单
        // ============================================

        /// <summary>
        /// 查询指定标签是否在黑名单中。
        /// </summary>
        public bool IsTagBlacklisted(BuffTag tag)
        {
            return _tagBlacklistRefCounts.TryGetValue(tag, out int count) && count > 0;
        }

        /// <summary>
        /// 手动添加标签到黑名单。
        /// </summary>
        public void AddTagToBlacklist(BuffTag tag, BuffInstance owner)
        {
            if (owner == null)
            {
                return;
            }

            AddTagToBlacklistInternal(tag, owner);
        }

        /// <summary>
        /// 手动移除标签从黑名单。
        /// </summary>
        public void RemoveTagsFromBlacklist(IReadOnlyList<BuffTag> tags, BuffInstance owner)
        {
            if (owner == null || tags == null || tags.Count <= 0)
            {
                return;
            }

            for (int i = 0; i < tags.Count; i++)
            {
                RemoveTagFromBlacklistInternal(tags[i], owner);
            }
        }

        private void RegisterBlacklistTagsFromData(BuffInstance owner)
        {
            if (owner == null || !TryGetBlacklistTags(owner.data, out List<BuffTag> blacklistTags))
            {
                return;
            }

            for (int i = 0; i < blacklistTags.Count; i++)
            {
                AddTagToBlacklistInternal(blacklistTags[i], owner);
            }
        }

        private static bool TryGetBlacklistTags(BuffData data, out List<BuffTag> blacklistTags)
        {
            if (data is TagBlacklistBuffData blacklistData &&
                blacklistData.blacklistTags != null &&
                blacklistData.blacklistTags.Count > 0)
            {
                blacklistTags = blacklistData.blacklistTags;
                return true;
            }

            blacklistTags = null;
            return false;
        }

        private void AddTagToBlacklistInternal(BuffTag tag, BuffInstance owner)
        {
            if (!_blacklistContributionsByBuff.TryGetValue(owner, out Dictionary<BuffTag, int> contribution))
            {
                contribution = new Dictionary<BuffTag, int>();
                _blacklistContributionsByBuff[owner] = contribution;
            }

            if (contribution.TryGetValue(tag, out int ownCount))
            {
                contribution[tag] = ownCount + 1;
            }
            else
            {
                contribution[tag] = 1;
            }

            if (_tagBlacklistRefCounts.TryGetValue(tag, out int totalCount))
            {
                _tagBlacklistRefCounts[tag] = totalCount + 1;
            }
            else
            {
                _tagBlacklistRefCounts[tag] = 1;
            }
        }

        private void RemoveTagFromBlacklistInternal(BuffTag tag, BuffInstance owner)
        {
            if (!_blacklistContributionsByBuff.TryGetValue(owner, out Dictionary<BuffTag, int> contribution))
            {
                return;
            }

            if (!contribution.TryGetValue(tag, out int ownCount) || ownCount <= 0)
            {
                return;
            }

            if (ownCount == 1)
            {
                contribution.Remove(tag);
            }
            else
            {
                contribution[tag] = ownCount - 1;
            }

            if (contribution.Count == 0)
            {
                _blacklistContributionsByBuff.Remove(owner);
            }

            if (!_tagBlacklistRefCounts.TryGetValue(tag, out int totalCount))
            {
                return;
            }

            if (totalCount <= 1)
            {
                _tagBlacklistRefCounts.Remove(tag);
            }
            else
            {
                _tagBlacklistRefCounts[tag] = totalCount - 1;
            }
        }

        private void RemoveAllBlacklistContributions(BuffInstance owner)
        {
            if (owner == null)
            {
                return;
            }

            if (!_blacklistContributionsByBuff.TryGetValue(owner, out Dictionary<BuffTag, int> contribution))
            {
                return;
            }

            _blacklistContributionsByBuff.Remove(owner);
            foreach (KeyValuePair<BuffTag, int> pair in contribution)
            {
                if (!_tagBlacklistRefCounts.TryGetValue(pair.Key, out int totalCount))
                {
                    continue;
                }

                int remain = totalCount - pair.Value;
                if (remain <= 0)
                {
                    _tagBlacklistRefCounts.Remove(pair.Key);
                }
                else
                {
                    _tagBlacklistRefCounts[pair.Key] = remain;
                }
            }
        }

        // ============================================
        // 控制锁定
        // ============================================

        private void RegisterControlLocksFromData(BuffInstance owner)
        {
            if (_controlGate == null || owner == null)
            {
                return;
            }

            if (owner.data is IControlLockProvider provider)
            {
                _controlGate.AddLocks(provider.ControlLocks, owner);
            }
        }

        private void RemoveControlLocksFromData(BuffInstance owner)
        {
            if (_controlGate == null || owner == null)
            {
                return;
            }

            _controlGate.RemoveAllLocksFromSource(owner);
        }

        // ============================================
        // 统一移除入口
        // ============================================

        private void RemoveBuffAt(int index)
        {
            BuffInstance buff = _buffInstanceList[index];
            _buffInstanceList.RemoveAt(index);
            RemoveAllBlacklistContributions(buff);
            RemoveControlLocksFromData(buff);
            buff.logic?.OnRemove();
        }
    }
}

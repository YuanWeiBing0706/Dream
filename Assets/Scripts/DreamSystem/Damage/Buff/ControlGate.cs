using System;
using System.Collections.Generic;
using UnityEngine;

namespace DreamSystem.Damage.Buff
{
    /// <summary>
    /// 控制锁定标志位（支持位运算组合）。
    /// <para>用于标识角色可被锁定的行为类型。</para>
    /// </summary>
    [Flags]
    public enum ControlLock
    {
        None = 0,
        Move = 1 << 0,
        Attack = 1 << 1,
        Cast = 1 << 2
    }

    /// <summary>
    /// 控制锁定门（引用计数）。
    /// <para>Buff 可以锁定玩家的特定行为（Move / Attack / Cast），移除时自动解锁。</para>
    /// <para>多个 Buff 同时锁定同一行为时，需要全部移除才会解锁。</para>
    /// <para>按来源追踪每个锁定贡献，确保 Buff 移除时只解除自己添加的锁。</para>
    /// </summary>
    public class ControlGate : MonoBehaviour
    {
        /// 全局锁定引用计数（ControlLock → 总引用次数）
        private readonly Dictionary<ControlLock, int> _lockRefCounts = new Dictionary<ControlLock, int>();

        /// 每个来源贡献的锁定计数（来源 → { ControlLock → 该来源的引用次数 }）
        private readonly Dictionary<object, Dictionary<ControlLock, int>> _contributionsBySource =
            new Dictionary<object, Dictionary<ControlLock, int>>();

        /// 是否可以移动
        public bool CanMove => !IsLocked(ControlLock.Move);

        /// 是否可以攻击
        public bool CanAttack => !IsLocked(ControlLock.Attack);

        /// 是否可以施法
        public bool CanCast => !IsLocked(ControlLock.Cast);

        /// <summary>
        /// 检查指定的锁定标志是否被激活（支持组合标志，任一被锁定即返回 true）。
        /// </summary>
        /// <param name="locks">要检查的锁定标志（可组合，如 Move | Attack）</param>
        /// <returns>只要有一个标志被锁定就返回 true</returns>
        public bool IsLocked(ControlLock locks)
        {
            if (locks == ControlLock.None)
            {
                return false;
            }

            int bits = (int)locks;
            int flagValue = 1;
            while (bits != 0)
            {
                if ((bits & 1) != 0)
                {
                    ControlLock flag = (ControlLock)flagValue;
                    if (_lockRefCounts.TryGetValue(flag, out int count) && count > 0)
                    {
                        return true;
                    }
                }

                bits >>= 1;
                flagValue <<= 1;
            }

            return false;
        }

        /// <summary>
        /// 添加锁定（按来源追踪引用计数）。
        /// </summary>
        /// <param name="locks">要锁定的标志（可组合）</param>
        /// <param name="source">锁定来源（通常为 BuffInstance）</param>
        public void AddLocks(ControlLock locks, object source)
        {
            if (source == null || locks == ControlLock.None)
            {
                return;
            }

            if (!_contributionsBySource.TryGetValue(source, out Dictionary<ControlLock, int> contribution))
            {
                contribution = new Dictionary<ControlLock, int>();
                _contributionsBySource[source] = contribution;
            }

            // 逐位遍历，为每个被设置的标志位添加锁定
            int bits = (int)locks;
            int flagValue = 1;
            while (bits != 0)
            {
                if ((bits & 1) != 0)
                {
                    AddLockInternal((ControlLock)flagValue, contribution);
                }

                bits >>= 1;
                flagValue <<= 1;
            }
        }

        /// <summary>
        /// 移除锁定（按来源追踪引用计数）。
        /// </summary>
        /// <param name="locks">要解锁的标志（可组合）</param>
        /// <param name="source">锁定来源</param>
        public void RemoveLocks(ControlLock locks, object source)
        {
            if (source == null || locks == ControlLock.None)
            {
                return;
            }

            if (!_contributionsBySource.TryGetValue(source, out Dictionary<ControlLock, int> contribution))
            {
                return;
            }

            int bits = (int)locks;
            int flagValue = 1;
            while (bits != 0)
            {
                if ((bits & 1) != 0)
                {
                    RemoveLockInternal((ControlLock)flagValue, contribution);
                }

                bits >>= 1;
                flagValue <<= 1;
            }

            // 如果该来源已经没有任何锁定贡献，清理来源记录
            if (contribution.Count == 0)
            {
                _contributionsBySource.Remove(source);
            }
        }

        /// <summary>
        /// 移除指定来源的所有锁定（Buff 移除时调用，一次性清理该 Buff 贡献的所有锁定）。
        /// </summary>
        /// <param name="source">锁定来源</param>
        public void RemoveAllLocksFromSource(object source)
        {
            if (source == null)
            {
                return;
            }

            if (!_contributionsBySource.TryGetValue(source, out Dictionary<ControlLock, int> contribution))
            {
                return;
            }

            _contributionsBySource.Remove(source);

            // 从全局计数中减去该来源的所有贡献
            foreach (KeyValuePair<ControlLock, int> pair in contribution)
            {
                if (!_lockRefCounts.TryGetValue(pair.Key, out int totalCount))
                {
                    continue;
                }

                int remain = totalCount - pair.Value;
                if (remain <= 0)
                {
                    _lockRefCounts.Remove(pair.Key);
                }
                else
                {
                    _lockRefCounts[pair.Key] = remain;
                }
            }
        }

        /// <summary>
        /// 内部方法：为单个标志位增加一次引用（同时更新来源贡献和全局计数）。
        /// </summary>
        private void AddLockInternal(ControlLock lockType, Dictionary<ControlLock, int> contribution)
        {
            if (contribution.TryGetValue(lockType, out int ownCount))
            {
                contribution[lockType] = ownCount + 1;
            }
            else
            {
                contribution[lockType] = 1;
            }

            if (_lockRefCounts.TryGetValue(lockType, out int totalCount))
            {
                _lockRefCounts[lockType] = totalCount + 1;
            }
            else
            {
                _lockRefCounts[lockType] = 1;
            }
        }

        /// <summary>
        /// 内部方法：为单个标志位减少一次引用（同时更新来源贡献和全局计数）。
        /// </summary>
        private void RemoveLockInternal(ControlLock lockType, Dictionary<ControlLock, int> contribution)
        {
            if (!contribution.TryGetValue(lockType, out int ownCount) || ownCount <= 0)
            {
                return;
            }

            if (ownCount == 1)
            {
                contribution.Remove(lockType);
            }
            else
            {
                contribution[lockType] = ownCount - 1;
            }

            if (!_lockRefCounts.TryGetValue(lockType, out int totalCount))
            {
                return;
            }

            if (totalCount <= 1)
            {
                _lockRefCounts.Remove(lockType);
            }
            else
            {
                _lockRefCounts[lockType] = totalCount - 1;
            }
        }
    }
}

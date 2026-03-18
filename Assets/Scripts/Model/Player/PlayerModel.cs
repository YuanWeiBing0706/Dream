using DreamSystem.Damage.Stat;
using Interface;
using UnityEngine;
using VContainer;

namespace Model.Player
{
    /// <summary>
    /// 玩家模型（挂载在玩家 GameObject 上的 MonoBehaviour）。
    /// <para>实现 IBuffOwner 接口，使 BuffSystem 和 BuffBaseLogic 能够访问玩家的 GameObject 和属性。</para>
    /// </summary>
    public class PlayerModel : MonoBehaviour, IBuffOwner
    {
        public GameObject GameObject => gameObject;
        public CharacterStats Stats { get; private set; }

        [Inject]
        public void Construct(CharacterStats stats)
        {
            Stats = stats;
        }
    }
}
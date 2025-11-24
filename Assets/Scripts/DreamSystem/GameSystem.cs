using System;
using VContainer.Unity;
namespace DreamSystem
{
    public abstract class GameSystem : IStartable, ITickable, ILateTickable, IDisposable
    {
        public virtual void Start()
        {
        }
        public virtual void Tick()
        {
        }
        public virtual void LateTick()
        {
        }
        public virtual void Dispose()
        {
        }
    }
}
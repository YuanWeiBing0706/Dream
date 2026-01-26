using System.Collections.Generic;
using Function.Damageable;
using UnityEngine;
namespace DreamManager
{
    public class DamageManager 
    {
        public Dictionary<Collider, IDamageable> playGameAllDamageColliderDir = new();
    
        public void Register(Collider collider, IDamageable handler)
        {
            playGameAllDamageColliderDir[collider] = handler;
        }
        
        public void Unregister(Collider collider)
        {
            
            playGameAllDamageColliderDir.Remove(collider);
        }
        
        public bool TryGet(Collider collider, out IDamageable handler) => playGameAllDamageColliderDir.TryGetValue(collider, out handler);
    }
}
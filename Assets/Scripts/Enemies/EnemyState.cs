using UnityEngine;

namespace Game.Enemies
{
    public abstract class EnemyState
    {
        protected BaseEnemy enemy;
        public EnemyState(BaseEnemy enemy) { this.enemy = enemy; }

        public virtual void EnterState() { }
        public virtual void UpdateState() { }
        public virtual void FixedUpdateState() { }
        public virtual void ExitState() { }
    }
}


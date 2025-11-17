using UnityEngine;

namespace Game.Enemies
{
    public class BossPatrolState : EnemyState
    {
        private Boss boss;
        private float timer;
        private float restDuration;

        public BossPatrolState(Boss boss) : base(boss)
        {
            this.boss = boss;
        }

        public override void EnterState()
        {
            restDuration = Random.Range(boss.restMin, boss.restMax);
            timer = 0f;
            boss.StopMovement();
            boss.IdleAnimation();
            Debug.Log($"BossPatrolState.Enter - resting for {restDuration:F2}s");
        }

        public override void UpdateState()
        {
            if (boss.IsDead) return;
            timer += Time.deltaTime;

            if (timer >= 3f)
            {
                if (boss.IsPlayerInRangeFloat(boss.detectionRange))
                {
                    Debug.Log("Player detected!");
                    // 50% chance to enter dash attack vs chase
                    if (Random.value < boss.dashChance)
                    {
                        Debug.Log("BossPatrolState.Update - choosing DashAttack");
                        boss.ChangeState(new BossDashState(boss));
                    }
                    else
                    {
                        Debug.Log("BossPatrolState.Update - choosing Chase");
                        boss.ChangeState(new BossChaseState(boss));
                    }
                    return;
                }

                if (timer >= restDuration)
                {
                    Debug.Log("Rest ended, no player found, reset timer");
                    timer = 0f;
                    restDuration = Random.Range(boss.restMin, boss.restMax);
                }
            }

            if (boss.player != null)
            {
                boss.FacePlayer();
            }
        }

        public override void ExitState()
        {
            Debug.Log("BossPatrolState.Exit");
        }
    }
}

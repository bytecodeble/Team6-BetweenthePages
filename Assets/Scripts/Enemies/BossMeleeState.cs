using UnityEngine;

namespace Game.Enemies
{
    public class BossMeleeState : EnemyState
    {
        private Boss boss;
        private float timer;
        private float windup = 0.8f;
        private float attack = 0.3f;
        private float recovery = 1.2f;
        private float totalDuration;
        private bool attackSpawned = false;

        public BossMeleeState(Boss boss) : base(boss)
        {
            this.boss = boss;
            totalDuration = windup + attack + recovery;
        }

        public override void EnterState()
        {
            timer = 0f;
            attackSpawned = false;
            boss.StopMovement();
            Debug.Log("BossMeleeState.Enter");
        }

        public override void UpdateState()
        {
            timer += Time.deltaTime;
            boss.FacePlayer();

            // Windup -> then spawn attack in attack window
            if (!attackSpawned && timer >= windup && timer < windup + attack)
            {
                SpawnAttackHitbox();
            }

            if (timer >= totalDuration)
            {
                Debug.Log("BossMeleeAttackState.Update: finished attack, returning to Patrol");
                boss.ChangeState(new BossPatrolState(boss)); // return to patrol after attack
            }
        }

        private void SpawnAttackHitbox()
        {
            attackSpawned = true;
            if (boss.attackHitboxPrefab != null)
            {
                Vector3 spawnPosition = boss.transform.position;
                float xOffset = 2.5f * Mathf.Sign(boss.transform.localScale.x);
                float yOffset = 5f;
                spawnPosition += new Vector3(xOffset, yOffset, 0f);

                var go = GameObject.Instantiate(boss.attackHitboxPrefab, boss.transform.position, Quaternion.identity);
                go.transform.parent = boss.transform;

                Debug.Log($"BossMeleeAttackState.SpawnAttackHitbox: spawned hitbox at {spawnPosition}");
            }
            else
            {
                Debug.LogWarning("BossMeleeAttackState.SpawnAttackHitbox: attackHitboxPrefab is null");
            }
        }

        public override void ExitState()
        {
            Debug.Log("BossMeleeAttackState.Exit");
        }
    }
}

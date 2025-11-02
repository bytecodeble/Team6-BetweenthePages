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

        private Vector2 hitboxOffset = new Vector2(2f, 1.5f);
        
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
                Vector3 dirToPlayer = boss.player.position - boss.transform.position;
                float facingDir = Mathf.Sign(dirToPlayer.x);

                Vector3 spawnLocalOffset = new Vector3(hitboxOffset.x * facingDir, hitboxOffset.y, 0f);
                Vector3 spawnWorldPos = boss.transform.position + spawnLocalOffset;


                GameObject hitbox = Object.Instantiate(boss.attackHitboxPrefab, spawnWorldPos, Quaternion.identity, boss.transform);

                Debug.Log($"BossMeleeState.SpawnAttackHitbox: spawned hitbox at {spawnWorldPos}, facing {(facingDir > 0 ? "right" : "left")}");
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

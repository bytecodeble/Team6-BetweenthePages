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
        private bool chargeSpawned = false;

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
            chargeSpawned = false;
            boss.StopMovement();
            Debug.Log("BossMeleeState.Enter");
        }

        public override void UpdateState()
        {
            timer += Time.deltaTime;
            boss.FacePlayer();

            // charge effect during windup
            if (!chargeSpawned && timer < windup)
            {
                SpawnChargeEffect();
            }

            // attack window
            if (!attackSpawned && timer >= windup && timer < windup + attack)
            {
                SpawnAttackHitbox();
            }

            if (timer >= totalDuration)
            {
                if (boss.comboAttackCount >= boss.maxComboAttacks)
                {
                    Debug.Log("BossMeleeState.Update - max combo, jump to center");
                    boss.comboAttackCount = 0;
                    boss.ChangeState(new BossJumpState(boss, boss.roomCenterPos));
                }
                else
                {
                    Debug.Log("BossMeleeState.Update - attack finished, back to Patrol");
                    boss.ChangeState(new BossPatrolState(boss));
                }
            }
        }

        private void SpawnChargeEffect()
        {
            chargeSpawned = true;
            if (boss.chargeEffectPrefab != null)
            {
                // offset
                Vector3 effectPos = boss.transform.position + new Vector3(0, 2.5f, 0);
                GameObject fx = Object.Instantiate(boss.chargeEffectPrefab, effectPos, Quaternion.identity, boss.transform);
                Debug.Log("BossMeleeState.SpawnChargeEffect: charging effect spawned.");
            }
            else
            {
                Debug.LogWarning("BossMeleeState.SpawnChargeEffect: chargeEffectPrefab is null");
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
                boss.comboAttackCount++;
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

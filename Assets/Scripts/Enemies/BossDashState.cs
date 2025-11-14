using UnityEngine;

namespace Game.Enemies
{
    public class BossDashState : EnemyState
    {
        private enum DashPhase { Windup, Dash, Stun }
        private DashPhase phase;

        private Boss boss;
        private float timer;
        private Vector2 startPos;
        private Vector2 targetPos;

        private bool dashVelocityApplied = false;
        private bool hitboxSpawned = false;

        public BossDashState(Boss boss) : base(boss)
        {
            this.boss = boss;
        }

        public override void EnterState()
        {
            if (boss.IsDead) return;

            phase = DashPhase.Windup;
            timer = 0f;
            startPos = boss.transform.position;
            targetPos = boss.player != null ? (Vector2)boss.player.position : startPos; // record once
            boss.StopMovement();
            dashVelocityApplied = false;
            hitboxSpawned = false;

            Debug.Log($"BossDashState.Enter - recorded target {targetPos}, windup {boss.dashWindupTime}, dash {boss.dashAttackDuration}, stun {boss.dashStunDuration}");

            // optional charge effect during windup
            if (boss.chargeEffectPrefab != null)
            {
                Vector3 effectPos = boss.transform.position + new Vector3(0, 2.5f, 0);
                Object.Instantiate(boss.chargeEffectPrefab, effectPos, Quaternion.identity, boss.transform);
            }
        }

        public override void UpdateState()
        {
            if (boss.IsDead) return;

            timer += Time.deltaTime;

            switch (phase)
            {
                case DashPhase.Windup:
                    boss.FacePlayer(); // face current player position (visual only)
                    if (timer >= boss.dashWindupTime)
                    {
                        phase = DashPhase.Dash;
                        timer = 0f;
                        dashVelocityApplied = false;
                        Debug.Log("BossDashState.Update - starting dash");
                    }
                    break;

                case DashPhase.Dash:
                    if (!dashVelocityApplied)
                    {
                        // Calculate velocity to reach recorded target in dash duration
                        Vector2 displacement = targetPos - startPos;
                        Vector2 dashVelocity = displacement / Mathf.Max(0.0001f, boss.dashAttackDuration);
                        boss.rb.linearVelocity = dashVelocity;

                        // spawn dash hitbox once when dash starts (optional attack effect)
                        SpawnDashHitbox(displacement);

                        dashVelocityApplied = true;
                        // face toward dash direction (targetPos)
                        if (displacement.x != 0)
                        {
                            Vector3 scale = boss.transform.localScale;
                            scale.x = displacement.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                            boss.transform.localScale = scale;
                        }
                    }

                    if (timer >= boss.dashAttackDuration)
                    {
                        // snap to target to ensure accuracy
                        boss.transform.position = new Vector3(targetPos.x, targetPos.y, boss.transform.position.z);
                        boss.StopMovement();
                        phase = DashPhase.Stun;
                        timer = 0f;
                        Debug.Log("BossDashState.Update - dash complete, entering stun");
                    }
                    break;

                case DashPhase.Stun:
                    // stunned: no movement
                    if (timer >= boss.dashStunDuration)
                    {
                        Debug.Log("BossDashState.Update - stun ended, jumping to center");
                        boss.ChangeState(new BossJumpState(boss, boss.roomCenterPos));
                    }
                    break;
            }
        }

        private void SpawnDashHitbox(Vector2 displacement)
        {
            if (hitboxSpawned) return;
            hitboxSpawned = true;

            if (boss.attackHitboxPrefab == null || boss.player == null)
            {
                if (boss.attackHitboxPrefab == null)
                    Debug.LogWarning("BossDashState.SpawnDashHitbox: attackHitboxPrefab is null");
                return;
            }

            float facingDir = Mathf.Sign(displacement.x == 0 ? (boss.player.position.x - boss.transform.position.x) : displacement.x);

            // reuse melee offset concept, but place in front horizontally
            Vector3 spawnOffset = new Vector3(2.5f * facingDir, 1.5f, 0f);
            Vector3 spawnPos = boss.transform.position + spawnOffset;

            Object.Instantiate(boss.attackHitboxPrefab, spawnPos, Quaternion.identity, boss.transform);
            Debug.Log($"BossDashState.SpawnDashHitbox: spawned at {spawnPos}, facing {(facingDir > 0 ? "right" : "left")}");
        }

        public override void ExitState()
        {
            boss.rb.linearVelocity = Vector2.zero;
            Debug.Log("BossDashState.Exit");
        }
    }
}

using UnityEngine;

namespace Game.Enemies
{
    public class BossJumpState : EnemyState
    {
        private Boss boss;
        private Vector2 startPos;
        private Vector2 targetPos;
        private float timer;
        private float pause;
        private float duration;
        private float apex;
        private Vector2? specificTargetPos;

        public BossJumpState(Boss boss) : base(boss)
        {
            this.boss = boss;
        }

        public BossJumpState(Boss boss, Vector2 specificTarget) : base(boss)
        {
            this.boss = boss;
            this.specificTargetPos = specificTarget;
        }

        public override void EnterState()
        {
            startPos = boss.transform.position;
            targetPos = specificTargetPos ?? (boss.player != null ? (Vector2)boss.player.position : startPos);
            timer = 0f;
            pause = boss.jumpPause;
            duration = boss.jumpDuration;
            apex = boss.jumpApexHeight;
            boss.StopMovement();
            Debug.Log($"BossJumpState.Enter - target {targetPos}, pause {pause}, duration {duration}, apex {apex}");
        }

        public override void UpdateState()
        {
            if (boss.IsDead) return;
            timer += Time.deltaTime;
            boss.FacePlayer();

            if (timer < pause)
            {
                // small wind-up pause before jump
                return;
            }

            float t = (timer - pause) / duration;
            if (t > 1f) t = 1f;

            Vector2 basePos = Vector2.Lerp(startPos, targetPos, t);
            float arc = 4f * apex * t * (1f - t); // peaks at t=0.5 with height apex
            Vector2 newPos = basePos + Vector2.up * arc;

            // override transform to follow the curve
            boss.rb.linearVelocity = Vector2.zero;
            boss.transform.position = new Vector3(newPos.x, newPos.y, boss.transform.position.z);

            if (t >= 1f)
            {
                Debug.Log("BossJumpAttackState.Update landed, returning to Patrol");
                boss.ChangeState(new BossPatrolState(boss));
            }
        }

        public override void ExitState()
        {
            boss.rb.linearVelocity = Vector2.zero;
            Debug.Log("BossJumpAttackState.Exit");
        }
    }
}

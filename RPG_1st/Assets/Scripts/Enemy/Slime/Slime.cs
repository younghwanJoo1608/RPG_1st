using UnityEngine;

public class Slime : BaseMonster
{
    private Transform targetPlayer;

    protected override void Start()
    {
        // BaseMonster의 Start()를 먼저 실행하여 기본 세팅.
        base.Start();

        // "Player" 태그를 가진 오브젝트를 찾아서 목표물로 설정.
        GameObject playerobj = GameObject.FindGameObjectWithTag("Player");
        if (playerobj != null)
        {
            targetPlayer = playerobj.transform;
        }
    }

    // 물리 엔진으로 이동하므로 FixedUpdate 사용.
    private void FixedUpdate()
    {
        // 죽었거나 플레이어를 찾지 못했으면 멈춤.
        if (isDead || targetPlayer == null || isKnockbacked)
            return;

        // 추적 AI
        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);

        // 방향에 따라 스프라이트 뒤집기
        // transform.localScale을 뒤집어버리면 체력바, 네이밍 등 자식 오브젝트까지 뒤집혀버림.
        if (direction.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (direction.x > 0)
        {
            spriteRenderer.flipX = false;
        }
    }
}

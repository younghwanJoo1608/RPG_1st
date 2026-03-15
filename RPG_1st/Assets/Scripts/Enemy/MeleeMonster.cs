using UnityEngine;

public class MeleeMonster : BaseMonster
{
    private Transform targetPlayer;
    private bool isAggroed = false;       // 현재 플레이어를 발견해서 화가 났는가?

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
        float randomDirX = Random.value > 0.5f ? 1f : -1f;
        transform.localScale = new Vector3(randomDirX, 1, 1);
    }

    public override void TakeDamage(int damageAmount, Transform attacker)
    {
        // 맞으면 어그로가 끌립니다!
        isAggroed = true; 
        // 부모 클래스의 원래 피격, 넉백 로직을 그대로 실행.
        base.TakeDamage(damageAmount, attacker); 
    }

    protected override void OnCollisionStay2D(Collision2D collision)
    {
        // 비선공 몬스터는 처음에 맞아도 안 아픔.
        if (!isAggroed && monsterData.aggroType == AggroType.Peaceful) return;

        // 그 외의 경우(화났거나 선공형이거나)에는 부모 클래스의 데미지 주는 로직을 정상 실행.
        base.OnCollisionStay2D(collision);
    }

    // 물리 엔진으로 이동하므로 FixedUpdate 사용.
    private void FixedUpdate()
    {
        // 생성 중이거나, 죽었거나, 플레이어를 찾지 못했거나, 넉백(경직) 당해서 날아가는 중일 때는 멈춤.
        if (isDead || targetPlayer == null || isKnockbacked || isSpawning)
            return;

        // 추적 AI
        Vector2 directionToPlayer = targetPlayer.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // 평화
        if (!isAggroed)
        {
            // 선공 몬스터인가?
            if (monsterData.aggroType == AggroType.Aggressive)
            {
                // 플레이어가 시야 거리 안에 들어왔는가?
                if (distanceToPlayer <= monsterData.viewDistance)
                {
                    // 슬라임이 현재 바라보는 방향 (localScale.x가 양수면 오른쪽, 음수면 왼쪽)
                    Vector2 facingDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
                    
                    // 슬라임의 시선 방향과 플레이어가 있는 방향 사이의 각도를 잽니다.
                    float angleToPlayer = Vector2.Angle(facingDirection, directionToPlayer);

                    // 그 각도가 부채꼴(시야각)의 절반 이내인가?
                    if (angleToPlayer <= monsterData.viewAngle / 2f)
                    {
                        isAggroed = true; // 선공! (발견함)
                    }
                }
            }
        }
        else    // 플레이어 발견
        {
            // 플레이어가 없으면 어그로를 풉니다
            if (targetPlayer == null)
            {
                isAggroed = false;
                rb.linearVelocity = Vector2.zero; // 포기하고 제자리에 멈춤
                return;
            }

            // (기존의 추적 및 방향 전환 로직)
            Vector2 moveDirection = directionToPlayer.normalized;
            rb.MovePosition(rb.position + moveDirection * monsterData.moveSpeed * Time.fixedDeltaTime);
            
            if (moveDirection.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (moveDirection.x > 0)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (monsterData == null) return;
        
        // 씬(Scene) 뷰에서 슬라임을 클릭했을 때만 선이 보입니다.
        Vector3 facingDirection = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        
        // 1. 시야 거리 (노란색 원)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, monsterData.viewDistance);

        if (monsterData.aggroType == AggroType.Aggressive)   // 비선공 몬스터는 시야선 필요 없음
        {
            // 2. 시야각 부채꼴 (파란색 선)
            Gizmos.color = Color.blue;
            Vector3 upLimit = Quaternion.Euler(0, 0, monsterData.viewAngle / 2f) * facingDirection;
            Vector3 downLimit = Quaternion.Euler(0, 0, -monsterData.viewAngle / 2f) * facingDirection;
            
            Gizmos.DrawLine(transform.position, transform.position + upLimit * monsterData.viewDistance);
            Gizmos.DrawLine(transform.position, transform.position + downLimit * monsterData.viewDistance);
        }
    }
}

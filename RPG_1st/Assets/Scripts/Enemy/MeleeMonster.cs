using UnityEngine;
using UnityEngine.AI;

public class MeleeMonster : BaseMonster
{
    private Transform targetPlayer;
    private Vector2 aimOffset = new Vector2(0f, 1f);
    
    private bool isRoaming = false;   // 지금 걷는 중인가? (아니면 쉬는 중인가?)
    private float roamTimer = 0f;     // 상태가 바뀔 때까지 남은 시간
    private Vector2 roamDirection;    // 이번에 걸어갈 랜덤 방향
    private Vector2 roamTargetPos;
    
    // Pathfinding
    private NavMeshPath path;
    private int currentPathIndex = 0;
    private float pathUpdateTimer = 0f;
    private float pathUpdateInterval = 0.2f; // 0.2초마다 플레이어 위치를 향해 새로운 길을 찾습니다.
    
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

        // 타이머를 약간 다르게 줘서 몬스터들이 동시에 움직이는 것을 방지합니다.
        roamTimer = Random.Range(0f, monsterData.maxRoamWait);

        path = new NavMeshPath();
    }

    public override void TakeDamage(int damageAmount, Transform attacker)
    {
        // 맞으면 어그로가 끌립니다!
        isAggroed = true; 
        // 부모 클래스의 원래 피격, 넉백 로직을 그대로 실행.
        base.TakeDamage(damageAmount, attacker); 
    }

    protected override void OnTriggerStay2D(Collider2D collision)
    {
        // 비선공 몬스터는 처음에 맞아도 안 아픔.
        if (!isAggroed && monsterData.aggroType == AggroType.Peaceful) return;

        // 그 외의 경우(화났거나 선공형이거나)에는 부모 클래스의 데미지 주는 로직을 정상 실행.
        base.OnTriggerStay2D(collision);
    }

    // 물리 엔진으로 이동하므로 FixedUpdate 사용.
    private void FixedUpdate()
    {
        // 생성 중이거나, 죽었거나, 플레이어를 찾지 못했거나, 넉백(경직) 당해서 날아가는 중일 때는 멈춤.
        if (isDead || targetPlayer == null || isKnockbacked || isSpawning)
            return;

        // 추적 AI
        Vector2 targetCenterPos = (Vector2)targetPlayer.position + aimOffset;   // 플레이어 몸통 중앙을 향해 다가오도록.
        Vector2 directionToPlayer = targetCenterPos - (Vector2)transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // 평화
        if (!isAggroed)
        {
            // 시야 검사 : 선공 몬스터인가?
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
                        return; // 발견했으면 아래 배회 로직을 실행하지 않고 즉시 턴을 넘깁니다.
                    }
                }
            }

            // 자유 배회 (Roaming)
            roamTimer -= Time.fixedDeltaTime;

            if (roamTimer <= 0f)
            {
                isRoaming = !isRoaming; // 걷기 <-> 쉬기 상태 반전

                if (isRoaming)
                {
                    // 걷기 시작: 360도 중 무작위 방향을 하나 뽑습니다.
                    float randomAngle = Random.Range(0f, 360f);
                    Vector2 randomDir = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
                    Vector2 randomPoint = (Vector2)transform.position + randomDir * Random.Range(1f, monsterData.roamRadius);

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
                    {
                        roamTargetPos = hit.position;
                        roamDirection = (roamTargetPos - (Vector2)transform.position).normalized;
                        roamTimer = Random.Range(monsterData.minRoamWalk, monsterData.maxRoamWalk);
                    }
                    else
                    {
                        isRoaming = false;
                        rb.linearVelocity = Vector2.zero;
                        roamTimer = Random.Range(monsterData.minRoamWait, monsterData.maxRoamWait);
                    }
                }
                else
                {
                    // 쉬기 시작: 가속도를 죽여서 제자리에 멈춥니다.
                    rb.linearVelocity = Vector2.zero;
                    // 쉬는 시간 세팅
                    roamTimer = Random.Range(monsterData.minRoamWait, monsterData.maxRoamWait);
                }
            }

            // [3] 실제로 걸어가기
            if (isRoaming)
            {
                if (Vector2.Distance(transform.position, roamTargetPos) < 0.1f)
                {
                    isRoaming = false;
                    rb.linearVelocity = Vector2.zero;
                    roamTimer = Random.Range(monsterData.minRoamWait, monsterData.maxRoamWait);
                }
                else
                {
                    rb.MovePosition(rb.position + roamDirection * monsterData.roamSpeed * Time.fixedDeltaTime);
                    
                    if (roamDirection.x < 0) transform.localScale = new Vector3(-1, 1, 1);
                    else if (roamDirection.x > 0) transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
        else    // 플레이어 발견.
        {
            // 플레이어가 없으면 어그로를 풉니다
            if (targetPlayer == null)
            {
                isAggroed = false;
                rb.linearVelocity = Vector2.zero; // 포기하고 제자리에 멈춤
                roamTimer = monsterData.minRoamWait; // 포기하면 잠시 쉬도록 타이머 초기화
                return;
            }

            // 1. 일정 시간(0.2초)마다 플레이어까지의 최단 경로를 다시 계산합니다.
            pathUpdateTimer -= Time.fixedDeltaTime;
            if (pathUpdateTimer <= 0f)
            {
                // 몬스터나 플레이어가 물리 충돌로 인해 파란 영역(NavMesh)을 살짝 벗어났더라도,
                // SamplePosition을 사용해 '가장 가까운 파란 영역'으로 좌표를 보정해 길찾기 실패를 막습니다!
                Vector2 startPos = transform.position;
                Vector2 targetPos = targetPlayer.position; // 반드시 물리적 바닥인 targetPlayer.position 이어야 함.

                NavMeshHit hit;
                if (NavMesh.SamplePosition(startPos, out hit, 2f, NavMesh.AllAreas)) startPos = hit.position;
                if (NavMesh.SamplePosition(targetPos, out hit, 2f, NavMesh.AllAreas)) targetPos = hit.position;

                NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, path);
                
                // 핵심 2: corners[0]은 항상 '출발 지점'이므로, 그 다음 목표인 corners[1]부터 걷게 합니다.
                currentPathIndex = 1; 
                pathUpdateTimer = pathUpdateInterval;
            }

            // 2. 계산된 경로(path.corners)가 존재하고, 아직 목적지에 도착하지 않았다면 이동합니다.
            if (path != null && path.corners.Length > 0 && currentPathIndex < path.corners.Length)
            {
                Vector2 nextWaypoint = path.corners[currentPathIndex];
                Vector2 moveDirection = (nextWaypoint - (Vector2)transform.position).normalized;

                rb.MovePosition(rb.position + moveDirection * monsterData.moveSpeed * Time.fixedDeltaTime);

                // 3. 현재 목표 웨이포인트(모퉁이)에 거의 도착했다면, 다음 웨이포인트를 타겟으로 바꿉니다.
                if (Vector2.Distance(transform.position, nextWaypoint) < 0.8f)
                {
                    currentPathIndex++;
                }

                if (moveDirection.x < 0) transform.localScale = new Vector3(-1, 1, 1);
                else if (moveDirection.x > 0) transform.localScale = new Vector3(1, 1, 1);
            }
            else
            {
                // 경로를 찾지 못했거나 이미 도착한 경우의 예외 처리 (임시로 직선 이동)
                Vector2 moveDirection = directionToPlayer.normalized;
                rb.MovePosition(rb.position + moveDirection * monsterData.moveSpeed * Time.fixedDeltaTime);
                
                if (moveDirection.x < 0) transform.localScale = new Vector3(-1, 1, 1);
                else if (moveDirection.x > 0) transform.localScale = new Vector3(1, 1, 1);
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

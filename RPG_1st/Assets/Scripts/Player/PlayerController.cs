using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // public으로 선언하면 유니티 에디터 창에서 속도를 직접 조절.
    private float moveSpeed = 3f; 
    
    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 movement;

    private bool isAttacking = false;

    public bool isStunned = false;

    void Start()
    {
        // 게임이 시작될 때 Player에 붙어있는 Rigidbody2D를 찾아 변수에 저장.
        rb = GetComponent<Rigidbody2D>();
        // 캐릭터의 Animator 컴포넌트를 찾아 변수에 저장.
        anim = GetComponent<Animator>();
        // sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isStunned) return;

        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking && !isStunned)
        {
            isAttacking = true; // 공격 스위치 ON
            movement = Vector2.zero; // 공격을 시작하면 이동 방향을 0.

            anim.ResetTrigger("Attack");    // 남아 있는 트리거 지우고 새 공격 시작.
            anim.SetTrigger("Attack");
        }

        // 공격 중이 아닐 때만 이동 로직
        if (!isAttacking)
        {
            // 키보드 입력 받기 (WASD 또는 방향키)
            // GetAxisRaw를 사용하면 -1, 0, 1 값만 반환해서 미끄러짐 없이 즉각적으로 방향이 전환돼.
            // movement.x = Input.GetAxisRaw("Horizontal");
            // movement.y = Input.GetAxisRaw("Vertical");
            movement.x = 0f;
            movement.y = 0f;
            if (Input.GetKey(KeyCode.RightArrow)) movement.x = 1f;
            if (Input.GetKey(KeyCode.LeftArrow)) movement.x = -1f;
            if (Input.GetKey(KeyCode.UpArrow)) movement.y = 1f;
            if (Input.GetKey(KeyCode.DownArrow)) movement.y = -1f;

            // 💡 참고: 좌우/상하 키를 동시에 누르면 0이 되어 멈추게 하려면 아래 코드 추가 (선택사항)
            if (Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow)) movement.x = 0f;
            if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow)) movement.y = 0f;

            // 좌우반전 로직
            if (movement.x < 0)
            {
                // sr.flipX = true;
                transform.localScale = new Vector3(-1, 1, 1); // 자식 오브젝트까지 모두 뒤집음.
            }
            else if (movement.x > 0)
            {
                //sr.flipX = false;
                transform.localScale = new Vector3(1, 1, 1);
            }

            // 애니메이션 파라미터 업데이트
            // X축 입력(좌/우)이 있을 때만 캐릭터가 바라보는 방향을 갱신합니다.
            // 이렇게 하면 위/아래(Y축)로만 움직일 때 movement.x가 0이 되어도 이전 좌/우 방향을 유지합니다.
            if (movement.x != 0)
            {
                anim.SetFloat("LastHorizontal", movement.x);
            }
            
            // 이동 중인지 판별하기 위해 Speed 파라미터 세팅
            anim.SetFloat("Speed", movement.sqrMagnitude);

            // 대각선 이동 속도 보정 (정규화)
            // 이걸 안 하면 대각선으로 갈 때 피타고라스 정리에 의해 루트2(약 1.414)배 더 빨라져.
            movement = movement.normalized;
        }
        else
        {
            // 공격 중일 때는 Speed 파라미터도 강제로 0으로 만들어 걷기 모션이 섞이는 걸 막음
            anim.SetFloat("Speed", 0);
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            anim.SetTrigger("Attack"); // Animator의 Attack 방아쇠를 당김
        }
    }

    // 물리 엔진 업데이트 주기마다 호출되는 함수. (물리 연산은 무조건 여기서 처리!)
    void FixedUpdate()
    {
        if (isStunned) return;
        // 실제 이동 처리
        // 현재 위치 + (이동 방향 벡터 * 속도 * 고정 프레임 시간)
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void AttackComplete()
    {
        isAttacking = false; // 공격 스위치 OFF (다시 이동 가능)
        anim.ResetTrigger("Attack"); // 플레이어 연타에 따른 다음 공격 선입력 취소.
    }
}
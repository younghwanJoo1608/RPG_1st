using System.Collections;
using UnityEngine;

public abstract class BaseMonster : MonoBehaviour
{
    [Header("Attack Settings")]
    protected float lastAttackTime = 0f; // 마지막으로 공격한 시간 기억

    public MonsterData monsterData;
    public HealthBarUI healthBar;

    protected int currentHealth;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Collider2D coll;
    protected Color originalColor;

    protected bool isDead = false;
    protected bool isKnockbacked = false;
    protected bool isSpawning = true; // 리젠 중일 때.
    protected bool isAggroed = false;

    [Header("UI")]
    public GameObject damageTextPrefab;

    protected virtual void Start()
    {
        currentHealth = monsterData.maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        originalColor = spriteRenderer.color;

        if (healthBar != null)
        {
            healthBar.Setup(new Color32(0, 130, 255, 255)); // 파란색
            healthBar.UpdateHealth(currentHealth, monsterData.maxHealth);
        }

        StartCoroutine(FadeInRoutine());
    }

    // 외부(무기)에서 때렸을 때 호출될 함수
    public virtual void TakeDamage(int damageAmount, Transform attacker)
    {
        // 이미 죽었거나, 리젠 중이면 무시.
        if (isDead || isSpawning) return;

        currentHealth -= damageAmount;

        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, monsterData.maxHealth);
        }

        if (damageTextPrefab != null)
        {
            // 몬스터 머리 위(Y축 +0.5)에서 약간 무작위(X축)로 흩어지게 소환해서 타격감을 높입니다.
            Vector2 randomOffset = new Vector2(Random.Range(-0.5f, 0.5f), 0.5f);
            Vector2 spawnPosition = (Vector2)transform.position + randomOffset;

            // 프리팹 소환
            GameObject textObj = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity);
            
            // 텍스트 내용과 색상 세팅
            textObj.GetComponent<DamageText>().Setup(damageAmount, false);
        }
        
#region 1. 넉백
        // 내 위치 - 때린 사람 위치
        isKnockbacked = true;
        rb.linearVelocity = Vector2.zero; // 기존에 다가오던 가속도를 0으로 초기화.
        Vector2 knockbackDirection = (transform.position - attacker.position).normalized;

        // Impulse(순간적인 힘)로 밀어버림.
        rb.AddForce(knockbackDirection * monsterData.knockbackForce, ForceMode2D.Impulse);
#endregion

#region 2. 피격 체크
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            Debug.Log($"[{this.GetType().Name}] {damageAmount}의 데미지를 입었습니다! (남은 체력: {currentHealth})");
            // 코루틴이 이미 실행 중일 수 있으니 멈췄다가 다시 켭니다 (연속 타격 시 버그 방지)
            StopAllCoroutines();
            StartCoroutine(HitRoutine());
        }
#endregion

    }

    protected virtual void Die()
    {
        isDead = true;
        if (coll != null)
            coll.enabled = false;
        Debug.Log($"[{this.GetType().Name}] 몬스터 파괴됨!");

        StopAllCoroutines();
        StartCoroutine(FadeOutAndDestroy());
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        // 생성 중이거나, 죽었거나, 넉백(경직) 당해서 날아가는 중이거나, 플레이어를 발견하지 못했을 때는 공격 불가!
        if (isDead || isKnockbacked || isSpawning || !isAggroed) return;

        // 부딪힌 대상이 플레이어인지 확인
        if (collision.gameObject.CompareTag("Player"))
        {
            // 마지막으로 때린 시간에서 쿨타임이 지났는지 확인
            if (Time.time >= lastAttackTime + monsterData.attackCooldown)
            {
                PlayerHealth playerHealth = collision.GetComponentInParent<PlayerHealth>();
                if (playerHealth != null)
                {
                    // 플레이어의 TakeDamage 함수 실행
                    playerHealth.TakeDamage(monsterData.attackDamage, transform);
                    
                    // 마지막 공격 시간 갱신
                    lastAttackTime = Time.time; 
                }
            }
        }
    }

    private IEnumerator FadeInRoutine()
    {
        // 생성 중일 때는 콜라이더와 체력바를 꺼서 맞지도, 때리지도 못하게 합니다.
        if (coll != null) coll.enabled = false;
        if (healthBar != null) healthBar.gameObject.SetActive(false);

        float fadeDuration = 1f; // 1초 동안 서서히 나타남
        float elapsedTime = 0f;

        // 시작할 때 알파값을 0(완전 투명)으로 설정
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
            yield return null;
        }

        // 페이드인 완료 후 원래 색상으로 고정하고, 콜라이더 활성화 및 체력바 표시
        spriteRenderer.color = originalColor;
        if (coll != null) coll.enabled = true;
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(true);
            healthBar.Setup(Color.blue);
            healthBar.UpdateHealth(currentHealth, monsterData.maxHealth);
        }

        isSpawning = false; // 생성 완료.
    }

    // 일정 시간을 기다렸다가 실행되게 하는 코루틴.
    private IEnumerator HitRoutine()
    {
        // 피격 플래시
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = originalColor;

        // 추가 경직 시간
        yield return new WaitForSeconds(monsterData.stunTime - 0.15f);

        // 정신 차린 후 추적
        isKnockbacked = false;
    }

    private IEnumerator FadeOutAndDestroy()
    {
        float fadeDuration = 2f; // 사라지는데 걸리는 시간
        float elapsedTime = 0f;
        Color startColor = spriteRenderer.color;    // collider를 꺼서 무기가 통과하게 한다.

        // 설정 시간 동안 매 프레임마다 반복문 실행
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Mathf.Lerp를 사용해 투명도(Alpha)를 1에서 0으로 부드럽게 깎아냄
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
            yield return null; // 다음 프레임까지 대기
        }

        Destroy(gameObject); // 체력이 0이 되면 몬스터 삭제
    }
}

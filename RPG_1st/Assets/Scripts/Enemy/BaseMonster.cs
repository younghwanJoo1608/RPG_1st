using System.Collections;
using UnityEngine;

public abstract class BaseMonster : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHealth = 50;
    public float moveSpeed = 1f;
    public int attackDamage = 10;

    [Header("Hit Feedback")]
    public float knockbackForce = 10f;
    public float stunTime = 0.5f; // 총 경직 시간 (밀려나서 멍때리는 시간)

    protected int currentHealth;
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Collider2D coll;
    protected Color originalColor;

    protected bool isDead = false;
    protected bool isKnockbacked = false;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        coll = GetComponent<Collider2D>();
        originalColor = spriteRenderer.color;
    }

    // 외부(무기)에서 때렸을 때 호출될 함수
    public void TakeDamage(int damageAmount, Transform attacker)
    {
        // 이미 죽었으면 무시.
        if (isDead) return;

        currentHealth -= damageAmount;
        
#region 1. 넉백
        // 내 위치 - 때린 사람 위치
        isKnockbacked = true;
        rb.linearVelocity = Vector2.zero; // 기존에 다가오던 가속도를 0으로 초기화.
        Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
        float knockbackForce = 10f;

        // Impulse(순간적인 힘)로 밀어버림.
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
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

    // 일정 시간을 기다렸다가 실행되게 하는 코루틴.
    private IEnumerator HitRoutine()
    {
        // 피격 플래시
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = originalColor;

        // 추가 경직 시간
        yield return new WaitForSeconds(stunTime - 0.15f);

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

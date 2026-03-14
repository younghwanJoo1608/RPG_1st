using System.Collections;
using UnityEngine;

public class DummyHealth : MonoBehaviour
{
    public int currentHealth = 30; // 샌드백의 체력

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D coll;
    private Color originalColor;

    private bool isDead = false;

    void Start()
    {
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
        Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
        float knockbackForce = 10f;

        // Impulse(순간적인 힘)로 밀어버림.
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
#endregion

#region 2. 피격 체크
        if (currentHealth <= 0)
        {
            isDead = true;
            coll.enabled = false;
            Debug.Log("[DummyHealth] 샌드백 파괴됨!");
            StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            Debug.Log($"[DummyHealth] {damageAmount}의 데미지를 입었습니다! (남은 체력: {currentHealth})");
            StartCoroutine(HitFlashRoutine());
        }
#endregion

    }

    // 일정 시간을 기다렸다가 실행되게 하는 코루틴.
    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
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

        Destroy(gameObject); // 체력이 0이 되면 샌드백 삭제
    }
}
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBarUI healthBar;

    private Rigidbody2D rb;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private PlayerController playerController; // 조작 스크립트 조종 필요.

    [Header("Hit Feedback")]
    public float knockbackForce = 8f;
    public float stunTime = 0.3f;
    public float invincibilityDuration = 1f; // 총 무적 시간
    public float flashInterval = 0.1f;       // 깜빡이는 간격

    private bool isInvincible = false;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[spriteRenderers.Length];

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i].color;
        }

        if (healthBar)
        {
            healthBar.Setup(new Color32(255, 80, 80, 255)); // 빨간색
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }
    }

    public void TakeDamage(int damageAmount, Transform attacker)
    {
        if (currentHealth <= 0 ||isInvincible) return;

        currentHealth -= damageAmount;

        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, maxHealth);
        }
        
        Debug.Log($"[{this.GetType().Name}] 플레이어가 {damageAmount} 데미지를 입었습니다. (남은 체력: {currentHealth})");

        // 플레이어 넉백 (몬스터와 동일한 원리)
        rb.linearVelocity = Vector2.zero;
        Vector2 knockbackDirection = (transform.position - attacker.position).normalized;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 데미지를 입었으니 넉백 & 무적 시간 코루틴 시작!
            StartCoroutine(StunRoutine());
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator StunRoutine()
    {
        if (playerController != null) playerController.isStunned = true;
        
        yield return new WaitForSeconds(0.5f); // 조작이 먹히지 않으면서 밀려남
        
        if (playerController != null) playerController.isStunned = false;
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        float elapsedTime = 0f;
        bool isFlashed = false;

        // 총 무적 시간이 다 지날 때까지 무한 반복.
        while (elapsedTime < invincibilityDuration)
        {
            // 삼항 연산자: 반투명 상태면 알파값을 1(원래대로)로, 아니면 0.5(반투명)로 바꿉니다.
            float targetAlpha = isFlashed ? 1f : 0.5f;

            for (int j = 0; j < spriteRenderers.Length; j++)
            {
                spriteRenderers[j].color = new Color(originalColors[j].r, originalColors[j].g, originalColors[j].b, targetAlpha);
            }

            isFlashed = !isFlashed; // 상태 반전

            // 한 번 깜빡이고 대기. 
            // 단, 남은 무적 시간이 깜빡임 간격보다 짧으면 남은 시간만큼만 대기해서 정확히 싱크를 맞춥니다.
            float waitTime = Mathf.Min(flashInterval, invincibilityDuration - elapsedTime);
            yield return new WaitForSeconds(waitTime);
            
            elapsedTime += waitTime;
        }

        // 무적 시간이 끝나는 즉시, 스프라이트를 무조건 원래 색상으로 되돌립니다.
        for (int j = 0; j < spriteRenderers.Length; j++)
        {
            spriteRenderers[j].color = originalColors[j];
        }

        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        StopAllCoroutines();
        // 일단 조작하지 못하도록 스프라이트와 콜라이더만 꺼둡니다.
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.enabled = false;
        }
        GetComponent<Collider2D>().enabled = false;
        GetComponent<PlayerController>().enabled = false; // 조작 스크립트 끄기
    }
}

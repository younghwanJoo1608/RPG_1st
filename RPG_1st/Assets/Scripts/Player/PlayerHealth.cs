using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    private Rigidbody2D rb;
    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;
    private PlayerController playerController; // 조작 스크립트 조종 필요.

    [Header("Hit Feedback")]
    public float knockbackForce = 8f;
    public float stunTime = 0.3f;

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
    }

    public void TakeDamage(int damageAmount, Transform attacker)
    {
        if (currentHealth <= 0 ||isInvincible) return;

        currentHealth -= damageAmount;

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

        Color flashColor = new Color(1f, 1f, 1f, 0.5f);
        
        // 1초 동안 반투명해지며 깜빡거리는 연출 (5번 반복)
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < spriteRenderers.Length; j++)
            {
                spriteRenderers[j].color = new Color(originalColors[j].r, originalColors[j].g, originalColors[j].b, 0.5f);
            }
            yield return new WaitForSeconds(0.1f);
            for (int j = 0; j < spriteRenderers.Length; j++)
            {
                spriteRenderers[j].color = originalColors[j];
            }
            yield return new WaitForSeconds(0.1f);
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

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    [Header("Weapon Stats")]
    public int minDamage = 1;
    public int maxDamage = 5;
    public float critChance = 0.05f;

    [Header("Multi-Hit Settings")]
    public int maxHitsPerTarget = 1;
    public float hitInterval = 0.2f; // 다단히트 시 시간 간격

    private Dictionary<BaseMonster, int> hitCounts = new Dictionary<BaseMonster, int>();
    private Dictionary<BaseMonster, float> lastHitTimes = new Dictionary<BaseMonster, float>();
    
    private Collider2D hitCollider;
    private bool wasColliderEnabled = false;

    private void Start()
    {
        hitCollider = GetComponent<Collider2D>();
        if (hitCollider != null)
        {
            wasColliderEnabled = hitCollider.enabled;
        }
    }

    private void Update()
    {
        if (hitCollider == null) return;

        // 핵심: 이전 프레임에는 콜라이더가 꺼져있었는데, 지금 막 켜졌다면? (새로 휘둘렀다면!)
        if (hitCollider.enabled && !wasColliderEnabled)
        {
            // 타격 명부를 깨끗하게 초기화합니다!
            hitCounts.Clear();
            lastHitTimes.Clear();
        }

        // 현재 상태를 저장해둡니다.
        wasColliderEnabled = hitCollider.enabled;
    }

    private void OnEnable() 
    {
        hitCounts.Clear();
        lastHitTimes.Clear();
    }

    // Is Trigger가 체크된 콜라이더가 무언가와 겹쳤을 때 자동으로 실행되는 함수
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.isTrigger) return;

        // 겹친 대상의 태그가 "Enemy"인지 확인
        if (collision.CompareTag("Enemy"))
        {
            BaseMonster enemy = collision.GetComponent<BaseMonster>();

            if (enemy != null)
            {
                if (!hitCounts.ContainsKey(enemy))
                {
                    hitCounts[enemy] = 0;
                    lastHitTimes[enemy] = 0f;
                }

                if (hitCounts[enemy] >= maxHitsPerTarget) return;

                if (Time.time >= lastHitTimes[enemy] + hitInterval || hitCounts[enemy] == 0)
                {
                    hitCounts[enemy]++;
                    lastHitTimes[enemy] = Time.time;

                    // 1. 맞은 몬스터의 방어력을 가져옵니다. (안전하게 null 체크)
                    int enemyDefense = (enemy.monsterData != null) ? enemy.monsterData.defense : 0;

                    // 2. 데미지 계산기에 무기 스탯과 몬스터의 방어력을 넣고 굴립니다.
                    DamageResult result = DamageCalculator.Calculate(minDamage, maxDamage, critChance, enemyDefense);

                    // 3. 계산이 끝난 최종 결과(데미지 수치, 크리 여부)를 전달합니다.
                    enemy.TakeDamage(result, transform);
                }
            }
        }
    }
}
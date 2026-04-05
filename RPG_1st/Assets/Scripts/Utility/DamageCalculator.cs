using UnityEngine;

public struct DamageResult
{
    public int finalDamage;
    public bool isCritical;
}

public static class DamageCalculator
{
    public static DamageResult Calculate(int minAtk, int maxAtk, float critChance, float critDamage, int targetDef)
    {
        DamageResult result = new DamageResult();

        // 1. 기본 데미지 결정 (최소~최대 공격력 사이의 랜덤 값)
        float baseDamage = Random.Range(minAtk, maxAtk + 1);

        // 2. 크리티컬 판정
        result.isCritical = Random.value <= critChance;
        if (result.isCritical)
        {
            baseDamage *= critDamage;
        }

        // 3. 방어력 차감 및 최소 데미지(1) 보정
        // 방어력이 아무리 높아도 최소 1의 데미지는 들어가도록(Max) 처리합니다.
        result.finalDamage = Mathf.Max(1, Mathf.RoundToInt(baseDamage - targetDef));

        return result;
    }
}

using UnityEngine;
using System;

public class PlayerStats: MonoBehaviour
{
    [Header("Level & EXP")]
    public int currentLevel = 1;
    public int currentExp = 0;
    public int[] expToNextLevel = { 0, 15, (int)(15 * Math.Pow(1.2f, 1)), (int)(15 *  Math.Pow(1.2f, 2)), (int)(15 *  Math.Pow(1.2f, 3)), (int)(15 *  Math.Pow(1.2f, 4)) };

    [Header("Allocatable Stats")]
    public int statPoints = 0; // 레벨업 시 얻는 남은 스탯 포인트
    public int STR = 5;   // STR (근력)
    public int DEX = 5;  // DEX (민첩)
    public int INT = 5; // INT (지능)
    public int LUK = 5; // LUK (행운)

    [Header("Derived Combat Stats")]
    // 스탯에 의해 계산되는 최종 능력치들
    public int maxHealth = 100;
    public int minAttack = 1;
    public int maxAttack = 5;
    public float moveSpeed = 5.0f;

    // 무기 기본 공격력 (나중에 무기 장착 시스템이 생기면 무기 스탯을 여기로 받아옵니다)
    public int baseWeaponMinAtk = 1;
    public int baseWeaponMaxAtk = 3;

    // UI가 업데이트되어야 할 때 자동으로 신호를 보내는 이벤트 (매우 중요!)
    public event Action OnExpChanged;
    public event Action OnLevelUp;
    public event Action OnStatsChanged;

    public PlayerHealth playerHealth;

    private void Start()
    {
        // 게임 시작 시 초기 스탯 한 번 계산
        CalculateDerivedStats();
    }

    public void AddExp(int amount)
    {
        // 만약 만렙(설정해둔 배열의 끝)이라면 경험치를 더 이상 얻지 않음
        if (currentLevel >= expToNextLevel.Length) return;

        currentExp += amount;
        Debug.Log($"경험치 획득: {amount}");

        // 경험치가 꽉 찼는지 확인하고 레벨업 처리 (경험치 통을 초과해서 얻었을 경우 연속 레벨업 대비 while문 사용)
        while (currentLevel < expToNextLevel.Length && currentExp >= expToNextLevel[currentLevel])
        {
            currentExp -= expToNextLevel[currentLevel];
            LevelUp();
        }

        // 경험치가 변했으니 UI에게 화면을 그리라고 신호를 보냄
        OnExpChanged?.Invoke();
    }

    private void LevelUp()
    {
        currentLevel++;
        statPoints += 5; // 레벨업 당 스탯 포인트 3 지급 (기획에 맞게 수정)
        
        // 레벨업 시 체력 100% 회복 같은 로직을 여기에 추가할 수 있습니다.
        // GetComponent<PlayerHealth>().HealFull();

        Debug.Log($"레벨업! 현재 레벨: {currentLevel}, 남은 스탯 포인트: {statPoints}");
        
        OnLevelUp?.Invoke();    // 레벨 UI 업데이트 신호
        OnStatsChanged?.Invoke(); // 스탯 UI 업데이트 신호
    }

    // 추가됨: UI의 '+' 버튼을 누르면 호출될 함수
    public void IncreaseStat(string statType)
    {
        if (statPoints <= 0) return; // 포인트가 없으면 무시

        switch (statType)
        {
            case "STR": STR++; break;
            case "DEX": DEX++; break;
            case "INT": INT++; break;
            case "LUK": LUK++; break;
        }

        statPoints--;
        CalculateDerivedStats(); // 스탯이 올랐으니 최종 전투력 다시 계산!
        OnStatsChanged?.Invoke(); // UI 새로고침 신호
    }

    // 추가됨: STR, DEX 등에 따라 실제 전투력을 계산하는 공식
    private void CalculateDerivedStats()
    {
        minAttack = baseWeaponMinAtk + (STR * 1) + (DEX * 1);
        maxAttack = baseWeaponMaxAtk + (STR * 2) + (DEX * 1) + (LUK / 2);

        if (playerHealth != null)
        {
            playerHealth.UpdateMaxHealth(maxHealth);
        }
    }
}

using UnityEngine;

public enum AggroType 
{ 
    Aggressive, // 선공 (슬라임, 빨간콩)
    Peaceful    // 비선공 (초록콩)
}

// 이 줄을 넣으면 유니티 에디터에서 우클릭으로 이 데이터를 생성할 수 있게 됩니다!
[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Game Data/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterName = "Unknown";
    public AggroType aggroType = AggroType.Aggressive;

    [Header("Stats")]
    public int maxHealth = 50;
    public float moveSpeed = 2f;
    public int attackDamage = 10;
    public float attackCooldown = 3f;

    [Header("Hit Feedback")]
    public float knockbackForce = 10f;
    public float stunTime = 0.5f;

    [Header("AI & Vision")]
    public float viewDistance = 5f;
    public float viewAngle = 90f;

    // [Header("Drop Table")]
    // public ItemData[] dropItems; (나중에 아이템 시스템 만들 때 여기에 추가!)
}

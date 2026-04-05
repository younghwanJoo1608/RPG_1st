using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Game Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName; // 무기 이름 (예: "초보자의 검")
    // public Sprite weaponSprite; // 나중에 무기 이미지를 바꿀 때 사용할 변수

    [Header("Combat Stats")]
    public int pDamage = 5;         // 물리 공격력
    public int mDamage = 0;         // 마법 공격력
    public float critProbBonus = 0f;  // 추가 크리티컬 확률 (예: 0.05 = 5%)
    public float critDamageBonus = 0f; // 추가 크리티컬 데미지 (예: 0.2 = 20%)
}

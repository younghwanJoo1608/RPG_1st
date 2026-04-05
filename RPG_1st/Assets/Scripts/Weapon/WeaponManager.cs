using UnityEngine;
using System;

public class WeaponManager : MonoBehaviour
{
    [Header("Equipped Weapon")]
    public WeaponData currentWeapon; // 현재 장착 중인 무기 데이터

    // 무기를 바꿔 꼈을 때 PlayerStats에게 "나 무기 바꿨어! 스탯 다시 계산해!"라고 알려줄 이벤트
    public event Action OnWeaponChanged;

    // 나중에 인벤토리에서 무기를 더블클릭하면 실행될 함수
    public void EquipWeapon(WeaponData newWeapon)
    {
        currentWeapon = newWeapon;
        
        // 여기에 나중에 WeaponSprite.sprite = newWeapon.weaponSprite; 같은 코드를 넣어 외형도 바꿀 수 있습니다.

        // 무기가 바뀌었으니 스탯을 다시 계산하라고 신호를 쏩니다.
        OnWeaponChanged?.Invoke(); 
    }
}
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    public int weaponDamage = 10; // 국자의 공격력

    // Is Trigger가 체크된 콜라이더가 무언가와 겹쳤을 때 자동으로 실행되는 함수
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 겹친 대상의 태그가 "Enemy"인지 확인
        if (collision.CompareTag("Enemy"))
        {
            // 대상에게서 BaseMonster 스크립트를 찾아 데미지 전달
            BaseMonster enemy = collision.GetComponent<BaseMonster>();
            
            // null 체크는 이렇게 한 줄로 줄여도 됩니다.
            enemy?.TakeDamage(weaponDamage, transform);
        }
    }
}
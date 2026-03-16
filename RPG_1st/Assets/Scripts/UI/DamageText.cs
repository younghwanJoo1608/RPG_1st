using UnityEngine;
using TMPro;
using System.Collections;

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 0.5f;       // 위로 올라가는 속도
    public float fadeDuration = 1.5f;  // 사라지는 데 걸리는 시간

    private TextMeshPro textMesh;

    // 외부에서 텍스트를 생성할 때 호출할 초기화 함수
    public void Setup(int damageAmount, Color textColor)
    {
        textMesh = GetComponent<TextMeshPro>();
        textMesh.text = damageAmount.ToString();
        textMesh.color = textColor; // 플레이어는 빨간색, 몬스터는 흰색 등 색상을 다르게 받을 수 있습니다.

        StartCoroutine(FloatAndFade());
    }

    private IEnumerator FloatAndFade()
    {
        Color startColor = textMesh.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            // 1. 위로 이동
            transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);
            
            // 2. 투명도(Alpha) 서서히 감소
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 3. 완전히 사라지면 오브젝트 파괴
        Destroy(gameObject);
    }
}

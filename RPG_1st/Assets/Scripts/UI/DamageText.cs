using UnityEngine;
using TMPro;
using System.Collections;

public class DamageText : MonoBehaviour
{
    private float moveSpeed = 0.5f;       // 위로 올라가는 속도
    private float fadeDuration = 1.5f;  // 사라지는 데 걸리는 시간
    private float moveDuration = 0.7f; // 위로 올라가는 데 쓰는 시간

    [Header("Sprite Assets")]
    public TMP_SpriteAsset enemySpriteAsset;  
    public TMP_SpriteAsset playerSpriteAsset;
    public TMP_SpriteAsset criticalSpriteAsset;

    private TextMeshPro textMesh;

    // 외부에서 텍스트를 생성할 때 호출할 초기화 함수
    public void Setup(int damageAmount, bool isPlayer, bool isCritical)
    {
        textMesh = GetComponent<TextMeshPro>();

        int baseOrder = textMesh.sortingOrder; // 사실 없어도 됨. Instantiate 함수로 복제품이 만들어지기 때문.
        if (isCritical)
        {
            moveSpeed = 0.7f;
            textMesh.spriteAsset = criticalSpriteAsset;
            transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            textMesh.sortingOrder = baseOrder + 1; 
        }
        else
        {
            moveSpeed = 0.5f;
            textMesh.spriteAsset = isPlayer ? playerSpriteAsset : enemySpriteAsset;
            transform.localScale = Vector3.one;
            textMesh.sortingOrder = baseOrder; 
        }

        string damageString = damageAmount.ToString();
        string spriteText = "";
        
        for (int i = 0; i < damageString.Length; i++)
        {
            spriteText += $"<sprite name=\"{damageString[i]}\">";
        }

        textMesh.text = spriteText;
        textMesh.color = Color.white;

        StartCoroutine(FloatAndFade());
    }

    private IEnumerator FloatAndFade()
    {
        Color startColor = textMesh.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            // 1. 위로 이동
            if (elapsedTime < moveDuration)
            {
                transform.Translate(Vector2.up * moveSpeed * Time.deltaTime);
            }
            
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

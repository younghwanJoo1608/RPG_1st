using System.Collections;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [Header("Drop Settings")]
    public float dropRadius = 1.0f;       // 몬스터로부터 얼마나 멀리 떨어질지 (반경)
    public float dropDuration = 0.6f;     // 바닥에 닿을 때까지 걸리는 총 시간
    public int bounceCount = 3;           // 튕기는 횟수 (3번 튕김)
    public float maxBounceHeight = 0.4f;  // 처음 튕길 때의 최고 높이 (낮게 설정해서 구르는 느낌 강조)

    private void Start()
    {
        Drop();
    }

    public void Drop()
    {
        // 1. 몬스터 주변의 랜덤한 도착 지점(원 모양 안의 임의의 점)을 계산합니다.
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector2 targetPos = (Vector2)transform.position + randomDir * Random.Range(0.5f, dropRadius);

        StartCoroutine(BounceRoutine(transform.position, targetPos));
    }

    private IEnumerator BounceRoutine(Vector2 startPos, Vector2 targetPos)
    {
        float elapsedTime = 0f;

        while (elapsedTime < dropDuration)
        {
            // t는 0에서 시작해 1로 끝나는 진행률입니다. (예: 0.5면 절반 진행됨)
            float t = elapsedTime / dropDuration;

            // 1. 목적지를 향해 바닥을 스르륵 미끄러지며 이동하는 '기본 좌표' 계산
            Vector2 currentBasePos = Vector2.Lerp(startPos, targetPos, t);

            // 2. 통통 튀는 '높이(Y 오프셋)' 계산
            // Mathf.Sin을 이용해 포물선을 그리고, (1 - t)를 곱해서 튕길 때마다 높이가 점점 낮아지게 만듭니다!
            float bounceHeight = Mathf.Abs(Mathf.Sin(t * Mathf.PI * bounceCount)) * maxBounceHeight * (1f - t);

            // 3. 실제 동전의 위치 = 바닥 좌표 + 튀어오른 높이
            transform.position = currentBasePos + new Vector2(0f, bounceHeight);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 시간이 다 되면 정확히 목적지에 안착시킵니다.
        transform.position = targetPos;
    }
}

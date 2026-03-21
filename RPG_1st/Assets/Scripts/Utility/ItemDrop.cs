using System.Collections;
using UnityEngine;

public class ItemDrop : MonoBehaviour
{
    [Header("References")]
    public Transform visualTransform; // 동전 이미지 (자식 오브젝트)
    public Transform shadowTransform; // 그림자 이미지 (자식 오브젝트)
    private Vector3 shadowBaseScale;

    [Header("Bobbing Settings")]
    private float bobSpeed = 2f;       // 위아래로 움직이는 속도
    private float bobHeight = 0.05f;   // 위아래로 움직이는 높이
    private float shadowScaleMultiplier = 0.5f; // 동전이 높이 뜰 때 그림자가 작아지는 비율

    [Header("Drop Settings")]
    public float dropRadius = 1.0f;       // 몬스터로부터 얼마나 멀리 떨어질지 (반경)
    public float dropDuration = 0.6f;     // 바닥에 닿을 때까지 걸리는 총 시간
    public int bounceCount = 3;           // 튕기는 횟수 (3번 튕김)
    public float maxBounceHeight = 0.4f;  // 처음 튕길 때의 최고 높이 (낮게 설정해서 구르는 느낌 강조)

    [Header("Pickup Settings")]
    public float magnetRadius = 1.0f;   // 자석처럼 끌려가기 시작하는 거리
    public float magnetSpeed = 8f;      // 플레이어에게 날아가는 속도
    public float pickupDistance = 0.3f; // 이 거리보다 가까워지면 획득 판정

    private bool hasLanded = false;
    private bool isBeingPickedUp = false;
    private Vector3 visualBaseLocalPos;

    private Transform playerTransform;

    private void Start()
    {
        // 시작할 때 동전 이미지의 기본 위치를 기억
        if (visualTransform != null)
            visualBaseLocalPos = visualTransform.localPosition;
        if (shadowTransform != null)
            shadowBaseScale = shadowTransform.localScale;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

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
            transform.position = currentBasePos;

            // 2. 통통 튀는 '높이(Y 오프셋)' 계산
            // Mathf.Sin을 이용해 포물선을 그리고, (1 - t)를 곱해서 튕길 때마다 높이가 점점 낮아지게 만듭니다!
            float bounceHeight = Mathf.Abs(Mathf.Sin(t * Mathf.PI * bounceCount)) * maxBounceHeight * (1f - t);

            if (visualTransform != null)
                visualTransform.localPosition = visualBaseLocalPos + new Vector3(0f, bounceHeight, 0f);

            // 3. 실제 동전의 위치 = 바닥 좌표 + 튀어오른 높이
            if (shadowTransform != null)
            {
                float scaleRatio = 1f - (bounceHeight * shadowScaleMultiplier);
                shadowTransform.localScale = shadowBaseScale * scaleRatio;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 시간이 다 되면 정확히 목적지에 안착시킵니다.
        transform.position = targetPos;
        hasLanded = true;
    }

    private void Update()
    {
        if (!hasLanded) return;

        // 1. 자석 픽업 로직
        if (playerTransform != null)
        {
            // 플레이어와 동전 사이의 거리를 계산합니다.
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= magnetRadius && !isBeingPickedUp)
            {
                isBeingPickedUp = true;
                
                // 플레이어에게 날아가는 순간 동전의 '물리적인 몸집'을 아예 없애버립니다!
                Collider2D col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }

            // 끌려가는 중이라면?
            if (isBeingPickedUp)
            {
                // 플레이어의 명치(Center) 쪽으로 빠르게 날아갑니다.
                transform.position = Vector2.MoveTowards(transform.position, playerTransform.position + new Vector3(0f, 0.5f, 0f), magnetSpeed * Time.deltaTime);

                // 날아갈 때 그림자를 스르륵 작아지게 만들어서 공중에 뜬 느낌을 줍니다.
                if (shadowTransform != null)
                {
                    shadowTransform.localScale = Vector3.Lerp(shadowTransform.localScale, Vector3.zero, Time.deltaTime * 15f);
                }

                // 완전히 가까워지면 획득 처리!
                if (distanceToPlayer <= pickupDistance)
                {
                    CollectCoin();
                }
                
                return; // 끌려갈 때는 아래의 둥둥 떠다니는 로직을 무시합니다.
            }
        }

        // 2. 기존 둥둥 떠다니는 로직 (끌려가지 않고 가만히 있을 때만 실행)
        if (visualTransform != null)
        {
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            visualTransform.localPosition = visualBaseLocalPos + new Vector3(0f, bobOffset, 0f);

            if (shadowTransform != null)
            {
                float scaleRatio = 1f - (Mathf.Max(0, bobOffset) * shadowScaleMultiplier * 2f);
                shadowTransform.localScale = shadowBaseScale * scaleRatio;
            }
        }

    }

    private void CollectCoin()
    {
        // (나중에 여기에 플레이어의 소지금을 += 1 하는 코드를 추가하면 됩니다!)
        Debug.Log("동전 획득!");

        // 획득했으니 씬에서 동전 파괴
        Destroy(gameObject);
    }
}

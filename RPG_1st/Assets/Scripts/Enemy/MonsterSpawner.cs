using System.Collections;
using UnityEngine;

[System.Serializable]
public class MonsterSpawnInfo
{
    public GameObject monsterPrefab; // 소환할 몬스터 프리팹
    public int maxCount;             // 이 몬스터를 맵에 최대 몇 마리 유지할 것인가?
}

public class MonsterSpawner: MonoBehaviour
{
    [Header("Spawn Settings")]
    public MonsterSpawnInfo[] spawnInfos;    // 생성할 몬스터 프리팹
    public int maxMonsters = 15;         // 맵에 유지할 최대 몬스터 수
    public float checkInterval = 10f;    // 몇 초마다 몬스터 숫자를 검사할지

    //분산 소환 세팅
    [Header("Spread Settings")]
    public float minSpawnDistance = 2f; // 몬스터들끼리 최소한 이 거리(반경)만큼은 떨어져서 태어납니다.
    public int maxSpawnAttempts = 10;   // 겹쳤을 때 자리를 다시 뽑아볼 최대 횟수 (무한 루프 방지용)

    private BoxCollider2D spawnArea; // 몬스터 생성 구역. (씬에서 투명 박스로 조절)

    void Start()
    {
        spawnArea = GetComponent<BoxCollider2D>();

        // 게임이 시작되면 무한 반복되는 스폰 검사 코루틴 실행
        StartCoroutine(SpawnManagerRoutine());
    }

    private IEnumerator SpawnManagerRoutine()
    {
        while (true)
        {
            // 현재 씬에 살아있는 몬스터 수 계산
            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            foreach (MonsterSpawnInfo info in spawnInfos)
            {
                if (info.monsterPrefab == null) continue; // 프리팹이 안 비워져 있으면 패스

                int currentCount = 0;

                // 핵심: 현재 맵에 있는 적들 중, '이 몬스터'가 몇 마리인지 이름으로 검사해서 셉니다.
                // (유니티는 프리팹을 소환하면 이름 뒤에 "(Clone)"을 붙이므로 StartsWith로 검사합니다)
                foreach (GameObject enemy in allEnemies)
                {
                    if (enemy.name.StartsWith(info.monsterPrefab.name))
                    {
                        currentCount++;
                    }
                }

                // 최대치보다 적으면 생성
                while (currentCount < info.maxCount)
                {
                    bool spawned = SpawnMonster(info.monsterPrefab);

                    if (spawned)
                    {
                        currentCount++; // 한 마리 소환할 때마다 카운트 증가
                    }
                    else
                    {
                        // 무한 루프에 빠지지 않게 이번 턴의 소환은 포기
                        break;
                    }
                }
            }

            // checkInterval 동안 대기 후 다시 검사
            yield return new WaitForSeconds(checkInterval);
        }
    }

    private bool SpawnMonster(GameObject prefabToSpawn)
    {
        // 검사
        if (spawnArea == null) return false;

        // 1. 생성할 위치 결정
        // spawnArea의 크기(size) 안에서 랜덤한 x, y 좌표를 가져옴.
        Bounds bounds = spawnArea.bounds;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 spawnPosition = new Vector2(randomX, randomY);

            // 뽑힌 위치를 중심으로 가상의 원(minSpawnDistance)을 그려서 닿는 모든 것을 가져옵니다.
            Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPosition, minSpawnDistance);
            bool isPositionValid = true; // 일단 이 자리가 좋다고 가정

            // 원 안에 들어온 녀석들을 하나씩 검사합니다.
            foreach (Collider2D coll in colliders)
            {
                // 만약 원 안에 다른 몬스터나, 심지어 '플레이어'가 있다면? (플레이어 바로 옆에 태어나는 억까 방지)
                if (coll.CompareTag("Enemy")|| coll.CompareTag("Player"))
                {
                    isPositionValid = false; // "앗, 여기 누가 있네! 탈락!"
                    break; // 더 볼 것도 없이 foreach 탈출하고 다음 좌표를 뽑으러 갑니다.
                }
            }

            // 원 안에 몬스터/플레이어가 아무도 없어서 합격(true)했다면?
            if (isPositionValid)
            {
                // 드디어 소환하고 함수를 성공(true)으로 마칩니다.
                // 프리팹 생성 : Instantiate(생성할 것, 위치, 회전값)
                Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
                return true; 
            }
        }
        
        // 10번 시도했는데도 자리가 없으면 그냥 포기. 맵이 너무 좁거나 몬스터가 너무 많다.
        return false;
    }
}

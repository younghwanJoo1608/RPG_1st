using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(EdgeCollider2D))]
public class AutoMapBounds : MonoBehaviour
{
    [Header("Targert TileMap")]
    public Tilemap targetMap;

    [System.Serializable]
    public struct DirectionalInset
    {
        public float top;
        public float bottom;
        public float left;
        public float right;
    }

    [Header("Camera Inner Offset")]
    private DirectionalInset camInset = new DirectionalInset { top = 0.1f, bottom = 0.1f, left = 0.1f, right = 0.1f };

    [Header("Player Inner Offset")]
    private DirectionalInset playerInset = new DirectionalInset { top = 1.0f, bottom = 1.0f, left = 1.0f, right = 1.0f };

    void Awake()
    {
        if (targetMap == null)
        {
            Debug.LogError("Target TileMap is not assigned!");
            return;
        }

        // 1. 타일맵의 실제 렌더링 영역(경계) 가져오기
        targetMap.CompressBounds(); // 빈 공간을 정리해서 정확한 크기를 구함
        Bounds bounds = targetMap.localBounds;

        // 2. 여백(inset)을 적용한 4개의 꼭짓점 좌표 계산
        // bounds.min은 좌하단, bounds.max는 우상단을 의미해.
        Vector2 camBottomLeft = new Vector2(bounds.min.x + camInset.left, bounds.min.y + camInset.bottom);
        Vector2 camBottomRight = new Vector2(bounds.max.x - camInset.right, bounds.min.y + camInset.bottom);
        Vector2 camTopRight = new Vector2(bounds.max.x - camInset.right, bounds.max.y - camInset.top);
        Vector2 camTopLeft = new Vector2(bounds.min.x + camInset.left, bounds.max.y - camInset.top);

        Vector2 playerBottomLeft = new Vector2(bounds.min.x + playerInset.left, bounds.min.y + playerInset.bottom);
        Vector2 playerBottomRight = new Vector2(bounds.max.x - playerInset.right, bounds.min.y + playerInset.bottom);
        Vector2 playerTopRight = new Vector2(bounds.max.x - playerInset.right, bounds.max.y - playerInset.top);
        Vector2 playerTopLeft = new Vector2(bounds.min.x + playerInset.left, bounds.max.y - playerInset.top);

        // 3. 카메라 가두기용 PolygonCollider2D 자동 세팅
        PolygonCollider2D polyCollider = GetComponent<PolygonCollider2D>();
        polyCollider.isTrigger = true; // 카메라는 물리적 충돌을 하면 안 되므로 Trigger 켜기
        polyCollider.points = new Vector2[] { camBottomLeft, camBottomRight, camTopRight, camTopLeft };

        // 4. 플레이어 가두기용 EdgeCollider2D 자동 세팅 (투명 벽 4개 역할)
        EdgeCollider2D edgeCollider = GetComponent<EdgeCollider2D>();
        edgeCollider.isTrigger = false; // 플레이어는 부딪혀야 하므로 물리 충돌 켜기
        // 선을 이어주는 방식이므로, 마지막에 다시 시작점(bottomLeft)으로 돌아와야 사각형이 닫혀!
        edgeCollider.points = new Vector2[] { playerBottomLeft, playerBottomRight, playerTopRight, playerTopLeft, playerBottomLeft };

        CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        if (confiner != null)
        {
            confiner.InvalidateCache(); 
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

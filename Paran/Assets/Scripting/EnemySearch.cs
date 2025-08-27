using UnityEngine;

public class EnemySearch : MonoBehaviour
{
    public enum EnemyState
    {
        Warning,
        Search
    }

    [Header("시야 설정")]
    public float viewAngle = 90f;
    public float viewRange = 10f;

    [Header("플레이어 참조")]
    public Transform playerTransform;

    public EnemyState currentState = EnemyState.Warning;

    private float playerInSightTimer = 0f;
    private float timeToSwitchState = 5f;
    private float searchDuration = 15f;
    private float searchTimer = 0f;
    private PlayerMove playerState;

    public bool playerVisible = false;

    public LayerMask obstacleMask;
    private void Start()
    {
        if (playerTransform != null) playerState = playerTransform.GetComponent<PlayerMove>();
    }
    private void Update()
    {
        if (playerTransform == null) return;

        playerVisible = IsPlayerInFOV();

        switch (currentState)
        {
            case EnemyState.Warning:
                if (playerVisible)
                {
                    playerInSightTimer += Time.deltaTime;
                    if (playerInSightTimer >= timeToSwitchState)
                    {
                        Debug.Log("플레이어 감지");
                        currentState = EnemyState.Search;
                        searchTimer = 0f;
                    }
                }
                else
                {
                    playerInSightTimer = 0f;
                }
                break;

            case EnemyState.Search:
                if (playerVisible)
                {
                    Debug.Log("플레이어 재감지");
                    searchTimer = 0f;
                }
                else
                {
                    searchTimer += Time.deltaTime;
                    if (searchTimer >= searchDuration)
                    {
                        Debug.Log("경계 해제");
                        currentState = EnemyState.Warning;
                        playerInSightTimer = 0f;
                    }
                }
                break;
        }
    }

private bool IsPlayerInFOV()
{
    Vector3 toPlayer = playerTransform.position - transform.position;
    float flatDistance = new Vector2(toPlayer.x, toPlayer.z).magnitude;

    // 🛑 거리가 멀면 감지 자체 하지 않음 (Raycast 생략)
    if (flatDistance > viewRange) return false;

    // 시야각 안인지 검사
    Vector3 toPlayerFlat = new Vector3(toPlayer.x, 0f, toPlayer.z);
    float angle = Vector3.Angle(transform.forward, toPlayerFlat.normalized);
    if (angle > viewAngle * 0.5f) return false;

    // 거리가 충분히 가까우므로 Raycast 시작
    Vector3 rayOrigin = transform.position + Vector3.up * 1.5f;
    Vector3 rayTarget = playerTransform.position + Vector3.up * 1.0f;
    Vector3 rayDir = (rayTarget - rayOrigin).normalized;
    float rayDistance = Vector3.Distance(rayOrigin, rayTarget);

    if (Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, rayDistance))
    {
        GameObject hitObject = hit.collider.gameObject;
        int hitLayer = hitObject.layer;

        if (hitObject.transform == playerTransform)
        {
            return true; // Player가 가장 먼저 맞은 경우
        }

        if (hitLayer == LayerMask.NameToLayer("Wall"))
        {
            //Debug.Log("벽이 막고 있음");
            return false;
        }

        if (hitLayer == LayerMask.NameToLayer("Window"))
        {
            if (playerState.currentState == PlayerMove.PlayerState.Crawl)
            {
                //Debug.Log("창문 + 웅크림 → 감지 실패");
                return false;
            }
            else
            {
                //Debug.Log("창문 너머 플레이어 감지 성공");
                return true;
            }
        }

        return false; // 기타 물체가 막고 있으면 감지 실패
    }

    return false; // 아무것도 안 맞으면 감지 실패
}
    private void OnDrawGizmosSelected()
    {
        if (playerTransform == null) return;

        Gizmos.color = Color.yellow;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 forward = transform.forward;

        float halfAngle = viewAngle * 0.5f;
        int segments = 30;

        Vector3 prevPoint = origin + Quaternion.Euler(0, -halfAngle, 0) * forward * viewRange;

        // 부채꼴 시야각 곡선 그리기
        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + (viewAngle / segments) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 nextPoint = origin + dir * viewRange;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        // 시야 경계선
        Gizmos.DrawLine(origin, origin + Quaternion.Euler(0, -halfAngle, 0) * forward * viewRange);
        Gizmos.DrawLine(origin, origin + Quaternion.Euler(0, halfAngle, 0) * forward * viewRange);
        Gizmos.DrawLine(origin, origin + forward * viewRange);

        // Ray to player
        if (playerTransform != null)
        {
            Vector3 rayTarget = playerTransform.position + Vector3.up * 1.0f;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, rayTarget);
        }
    }
}

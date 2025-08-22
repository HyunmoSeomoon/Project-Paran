using UnityEngine;

public class EnemyManager : MonoBehaviour
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

    private void Update()
    {
        if (playerTransform == null) return;

        bool playerVisible = IsPlayerInFOV();

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
        toPlayer.y = 0;

        float distance = toPlayer.magnitude;
        if (distance > viewRange) return false;

        float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
        return angle <= viewAngle * 0.5f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        float halfAngle = viewAngle * 0.5f;
        int segments = 30;
        Vector3 prevPoint = origin + Quaternion.Euler(0, -halfAngle, 0) * forward * viewRange;

        for (int i = 1; i <= segments; i++)
        {
            float angle = -halfAngle + (viewAngle / segments) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 nextPoint = origin + dir * viewRange;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        Gizmos.DrawLine(origin, origin + Quaternion.Euler(0, -halfAngle, 0) * forward * viewRange);
        Gizmos.DrawLine(origin, origin + Quaternion.Euler(0, halfAngle, 0) * forward * viewRange);
    }
}

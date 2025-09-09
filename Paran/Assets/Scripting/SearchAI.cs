using UnityEngine;
using UnityEngine.AI;

public class SearchAI : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    private Transform currentTarget;

    private NavMeshAgent agent;
    private EnemySearch enemyManager;

    private bool released = false;
    private Vector3 searchTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyManager = GetComponent<EnemySearch>();

        if (pointA == null || pointB == null)
        {
            Debug.LogError("Point들 설정 필요");
            enabled = false;
            return;
        }

        currentTarget = pointA;
        agent.SetDestination(currentTarget.position);
    }

    void Update()
    {
        if (enemyManager == null || agent == null) return;

        switch (enemyManager.currentState)
        {
            case EnemySearch.EnemyState.Warning:
                if (released)
                {
                    if (Vector3.Distance(transform.position, pointA.position) < 0.5f)
                    {
                        released = false;
                        currentTarget = pointB;
                        agent.SetDestination(currentTarget.position);
                    }
                }
                else
                {
                    Patrol();
                }
                break;

            case EnemySearch.EnemyState.Chase:
                if (enemyManager.playerVisible)
                {
                    // 플레이어를 본 프레임의 플레이어 위치 저장
                    searchTarget = enemyManager.playerTransform.position;
                    agent.SetDestination(searchTarget);
                }

                // 도착 여부 판단 생략 가능 (원하는 동작 추가 가능)
                break;
        }

        // 다시 warning 상태 되면 A로 복귀 (고정): 나중에 변경해야할듯
        if (enemyManager.currentState == EnemySearch.EnemyState.Warning && 
            !released &&
            Vector3.Distance(agent.destination, searchTarget) < 1f)
        {
            released = true;
            agent.SetDestination(pointA.position);
        }
    }

    void Patrol()
    {
        // 지점 y좌표 무시
        Vector3 flatPosition = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector3 flatTarget = new Vector3(currentTarget.position.x, 0f, currentTarget.position.z);

        if (Vector3.Distance(flatPosition, flatTarget) < 0.5f)
        {
            currentTarget = (currentTarget == pointA) ? pointB : pointA;
            agent.SetDestination(currentTarget.position);
        }
    }
}

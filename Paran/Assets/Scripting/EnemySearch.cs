using UnityEngine;
using UnityEngine.AI;

public class EnemySearch : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Warning,
        Search,
        Died
    }
    public enum LosResult
    {
        None,       // Raycast 안 함 or 안 맞음
        Player,     // 플레이어가 첫 히트
        Wall,
        Window
    }

    [Header("시야 설정")]
    public float viewAngle = 90f;
    public float viewRange = 10f;
    [SerializeField] private LayerMask losMask = ~0;
    [SerializeField] private float probeDistance = 10f; // 이 거리 이내일 때만 Raycast 수행
    private RaycastHit sharedHit;

    [Header("플레이어 참조")]
    public Transform playerTransform;

    [Header("감지 속도 스케일")]
    [SerializeField] private float detectNear = 2.0f;      // 아주 가까움 판정 거리
    [SerializeField] private float detectFar = 12.0f;     // 멀다 판정 거리(이상에선 최소 가중)
    [SerializeField] private float detectMinMultiplier = 0.5f;  // 멀 때 최소 속도 배수
    [SerializeField] private float detectMaxMultiplier = 2.5f;  // 가까울 때 최대 속도 배수
    [SerializeField] private float visualBoost = 1.2f;     // 시야로 감지 중 가산(배수)
    [SerializeField] private float soundBoost = 0.8f;     // 청각으로 감지 중 가산(배수): 1 이상으로 설정하지 말 것!!!!
    public EnemyState currentState = EnemyState.Warning;
    private NavMeshAgent agent;
    private float playerInSightTimer = 0f;
    private float timeToSwitchState = 5f;
    private float searchDuration = 15f;
    private float searchTimer = 0f;
    private PlayerMove playerState;

    public bool playerVisible = false;
    public bool checkSound = false;

    public LayerMask obstacleMask;
    private LosResult lastLos;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (playerTransform != null) playerState = playerTransform.GetComponent<PlayerMove>();

        if (playerTransform != null)
        {
            int playerLayerBit = 1 << playerTransform.gameObject.layer;
            if ((losMask.value & playerLayerBit) == 0)
            {
                Debug.LogWarning("[EnemySearch] losMask에 Player 레이어가 포함되어 있지 않습니다. " +
                                 "Player, Wall, Window 레이어를 포함하도록 설정하세요.", this);
            }
        }
    }
private void Update()
{
    if (playerTransform == null) return;

    lastLos = UpdateSharedRaycast(playerTransform);

    playerVisible = IsPlayerInFOV(lastLos);
    checkSound    = AuditoryCheck(lastLos);

    switch (currentState)
    {
        case EnemyState.Warning:
        {
            if (playerVisible)
            {
                // Player를 계속 바라보게: 이동 정지 + 회전 수동 제어
                if (agent != null)
                {
                    agent.isStopped = true;
                    agent.updateRotation = false;
                }

                // XZ 평면 기준으로 플레이어 방향으로 천천히 회전
                Vector3 to = playerTransform.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(to, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation, targetRot, 120f * Time.deltaTime);
                }
            }
            else
            {
                // 시야에서 벗어나면 에이전트 회전 제어 복구
                if (agent != null)
                {
                    agent.updateRotation = true;
                    // agent.isStopped = false; // 필요 시 순찰 재개 지점에서 해제
                }
            }

            bool sensed = playerVisible || checkSound;
            if (sensed)
            {
                float detectRate = ComputeDetectRate(playerVisible, checkSound);
                playerInSightTimer += Time.deltaTime * detectRate;

                if (playerInSightTimer >= timeToSwitchState)
                {
                    Debug.Log($"플레이어 감지 (rate x{detectRate:F2})");
                    currentState = EnemyState.Search;
                    searchTimer = 0f;

                    // 수색 돌입 시 에이전트 제어 복구 및 이동 재개
                    if (agent != null)
                    {
                        agent.updateRotation = true;
                        agent.isStopped = false;
                    }
                }
            }
            else
            {
                playerInSightTimer = 0f;
            }
            break;
        }

        case EnemyState.Search:
        {
            if (playerVisible || checkSound)
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
}

    private LosResult UpdateSharedRaycast(Transform target)
    {
        if (target == null) return LosResult.None;

        // 2D(XZ) 제곱거리로 먼저 컷
        Vector2 A = new Vector2(transform.position.x, transform.position.z);
        Vector2 B = new Vector2(target.position.x, target.position.z);
        float sqrDist = (A - B).sqrMagnitude;
        float sqrGate = probeDistance * probeDistance;

        if (sqrDist > sqrGate)
        {
            // 게이트 밖 → Raycast 자체를 생략
            return LosResult.None;
        }

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 dest   = target.position   + Vector3.up * 1.0f;

        if (Physics.Linecast(origin, dest, out sharedHit, losMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"[EnemySearch] Linecast hit: {sharedHit.collider.name} (Layer: {LayerMask.LayerToName(sharedHit.collider.gameObject.layer)})");
            // Player가 첫 히트?
            if (sharedHit.collider.transform.root == target.root)
                return LosResult.Player;

            // 창문/벽 구분
            int hitLayer = sharedHit.collider.gameObject.layer;
            if (hitLayer == LayerMask.NameToLayer("Window")) return LosResult.Window;
            if (hitLayer == LayerMask.NameToLayer("Wall"))   return LosResult.Wall;

            // ✅ 그 외 어떤 레이어든 "장애물"로 간주 (안전 기본값)
            return LosResult.Wall;
        }
        return LosResult.None;
    }

    private bool IsPlayerInFOV(LosResult result)
    {
        if (playerTransform == null) return false;

        // 📌 1. 거리 컷
        Vector3 toPlayer = playerTransform.position - transform.position;
        float flatDistance = new Vector2(toPlayer.x, toPlayer.z).magnitude;
        if (flatDistance > viewRange) return false;

        // 📌 2. 시야각 컷
        Vector3 toPlayerFlat = new Vector3(toPlayer.x, 0, toPlayer.z);
        float angle = Vector3.Angle(transform.forward, toPlayerFlat.normalized);
        if (angle > viewAngle * 0.5f) return false;

        // 공유 result 사용
        if (result == LosResult.Player) return true;
        if (result == LosResult.Window)
            return playerState == null || playerState.currentState != PlayerMove.PlayerState.Crawl;
        return false;
}

    private bool AuditoryCheck(LosResult result)
    {
        if (playerTransform == null) return false;

        Vector3 toPlayer = playerTransform.position - transform.position;
        float flatDist = new Vector2(toPlayer.x, toPlayer.z).magnitude;

        if (flatDist > probeDistance) return false;
        if (result == LosResult.Window) return false; // 창문은 청각 차단

    // 플레이어가 'Run' 상태이고, LOS가 Player일 때만 청각 감지 + 회전
        if (result == LosResult.Player && playerState != null && playerState.currentState == PlayerMove.PlayerState.Run)
        {
            // XZ 평면 기준으로만 바라보도록 회전
            Vector3 look = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (look.sqrMagnitude > 0.0001f)
            {
                //Navmesh가 Rotation을 관리하지 못하도록 설정
                agent.updateRotation = false;

                Quaternion targetRot = Quaternion.LookRotation(look, Vector3.up);
                // 충분히 천천히 회전
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 90f * Time.deltaTime);
            }
            return true; // 청각으로 감지됨
        }
        return false;
    }

    private float ComputeDetectRate(bool viaVision, bool viaSound)
    {
        // 플레이어까지 평면거리
        Vector3 to = playerTransform.position - transform.position;
        float d = new Vector2(to.x, to.z).magnitude;

        // 0(멀다)~1(가깝다)로 정규화
        float closeness = Mathf.InverseLerp(detectFar, detectNear, d);
        // 거리 기반 속도 배수
        float rate = Mathf.Lerp(detectMinMultiplier, detectMaxMultiplier, closeness);

        // 둘 다 참이면 rate이 더욱 커지도록 설계
        float modality = 1f;
        if (viaVision) modality = visualBoost;
        if (viaSound) modality = soundBoost;
        if (viaVision && viaSound) modality = visualBoost / soundBoost;

        return rate * modality; // 최종 배수
    }

    public void SetState(EnemyState newState)
    {
        if (currentState == EnemyState.Died)
        {
            // 이미 사망한 경우, 상태 변경 무시
            return;
        }

        currentState = newState;

        if (newState == EnemyState.Died)
        {
            Debug.Log("적 사망 처리됨");
            // 필요한 경우 사망 애니메이션 재생이나 컴포넌트 비활성화 처리
            // 예: GetComponent<Animator>().SetTrigger("Die");
            // 또는 NavMeshAgent, AI 컴포넌트 비활성화 등
        }
    }

    // 외부에서 상태를 확인
    public EnemyState GetState()
    {
        return currentState;
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

        Gizmos.color = Color.black;
        int seg = 48;
        Vector3 center = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);
        Vector3 prev = center + new Vector3(probeDistance, 0f, 0f);
        for (int i = 1; i <= seg; i++)
        {
            float t = (float)i / seg * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(t) * probeDistance, 0f, Mathf.Sin(t) * probeDistance);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}

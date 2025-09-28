using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyMove : MonoBehaviour
{
    [Header("순찰 관련")]
    [SerializeField] private Transform[] patrolPoints;
    public float waypointTolerance = 0.5f;
    private int patrolIndex = 0;
    [SerializeField] float waitAtWaypoint = 5f; // 대기 시간(초)
    bool waiting;                                // 중복 코루틴 방지

    [Header("추적 관련")]
    private float chaseTimer;
    private Vector3 lastPlayerPos;
    public float chaseDuration = 5f;     // 5초 추적
    private bool isPausingAfterChase;
    [SerializeField] private float killRange = 5f;

    [Header("의심 관련")]
    private Vector3 lastKnownPlayerPos;
    public float postChasePause = 2f;    // 시야 잃으면 2초 정지
    private float pauseUntil;
    private bool isSearching;
    private float soundTimer = 0f; // 현재 타이머
    private bool soundRecentlyHeard = false;

    [Header("참조 관련")]
    private NavMeshAgent agent;
    private EnemySearch enemyManager;
    private EnemySearch.EnemyState prevState;

    [Header("Look Settings")]
    public float watchTurnSpeedVision = 360f;
    public float watchTurnSpeedSound  = 120f;

    [Header("기타")]
    bool deathApplied = false;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyManager = GetComponent<EnemySearch>();

        if (agent == null || enemyManager == null)
        {
            Debug.LogError("NavMeshAgent 또는 EnemySearch가 없습니다.");
            enabled = false; return;
        }
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetDestination(patrolPoints[0].position);
        }

        prevState = enemyManager.currentState;
        chaseTimer = 0f;
        isPausingAfterChase = false;
    }

    void Update()
    {
        if (enemyManager == null || agent == null) return;
        
        // 죽으면 멈추도록
        if (enemyManager.GetState() == EnemySearch.EnemyState.Died)
        {
            if (!agent.isStopped) agent.isStopped = true;
            return;
        }

        if (enemyManager.currentState != prevState)
        {
            OnStateChanged(prevState, enemyManager.currentState);
            prevState = enemyManager.currentState;
        }

        switch (enemyManager.currentState)
        {
            case EnemySearch.EnemyState.Idle:
                agent.updateRotation = true;
                agent.isStopped = false;
                Patrol();
                break;
            case EnemySearch.EnemyState.Warning:
                HandleWarning();
                break;

            case EnemySearch.EnemyState.Chase:
                // 만약 시체를 봤으면 (bool) Search() 호출
                HandleChase();
                break;
            case EnemySearch.EnemyState.Died:
                if (!deathApplied) ApplyDeathOnce();
                break;
        }
    }
    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        if (!agent.isOnNavMesh) return;

        Transform tgt = patrolPoints[patrolIndex];
        if (tgt == null) { patrolIndex = (patrolIndex + 1) % patrolPoints.Length; return; }

        // 필요 시 경로 보장
        if (!agent.hasPath && !agent.pathPending)
            agent.SetDestination(tgt.position);

        // --- XZ(높이 무시) 도착 판정 ---
        Vector2 curXZ = new Vector2(transform.position.x, transform.position.z);
        Vector2 tgtXZ = new Vector2(tgt.position.x, tgt.position.z);
        float arriveThreshold = Mathf.Max(agent.stoppingDistance, waypointTolerance);

        if (!waiting && Vector2.Distance(curXZ, tgtXZ) <= arriveThreshold)
        {
            StartCoroutine(WaitAndGotoNext());
        }
    }
    IEnumerator WaitAndGotoNext()
    {
        waiting = true;

        // 진행 반대 방향 바라보기 (velocity가 거의 0이면 현재 forward 기준)
        Vector3 moveDir = agent.velocity; moveDir.y = 0f;
        if (moveDir.sqrMagnitude < 1e-4f) moveDir = transform.forward;
        transform.rotation = Quaternion.LookRotation(-moveDir, Vector3.up);

        // 5초 대기
        agent.isStopped = true;
        agent.updateRotation = false;
        yield return new WaitForSeconds(waitAtWaypoint);

        // 다음 포인트로
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        agent.isStopped = false;
        agent.updateRotation = true;

        Transform next = patrolPoints[patrolIndex];
        if (next != null)
            agent.SetDestination(next.position);

        waiting = false;
    }

    int GetClosestPatrolIndex(Vector3 from)
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return 0;

        int closest = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float d = (patrolPoints[i].position - from).sqrMagnitude;
            if (d < bestDist) { bestDist = d; closest = i; }
        }
        return closest;
    }
    void OnStateChanged(EnemySearch.EnemyState from, EnemySearch.EnemyState to)
    {
        if (to == EnemySearch.EnemyState.Chase)
        {
            chaseTimer = 0f;
            isPausingAfterChase = false;
            // 추적 시작 시엔 이동/회전 권한을 에이전트로 복구
            agent.isStopped = false;
            agent.updateRotation = true;
        }
        else if (to == EnemySearch.EnemyState.Warning)
        {
            // 가장 가까운 순찰 지점으로 복귀
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                patrolIndex = GetClosestPatrolIndex(transform.position);
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
        }
    }
    void HandleWarning()
    {
        // 새로운 소리를 들었을 경우 타이머 리셋
        if (!enemyManager.playerVisible && enemyManager.checkSound && !soundRecentlyHeard)
        {
            soundTimer = Mathf.Max(0.5f, enemyManager.PlayerDetectRatio() * 2f);
            Debug.Log($"{soundTimer:F2}초 기다립니다");
            soundRecentlyHeard = true;
        }

        // 타이머 감소
        if (soundTimer > 0f)
        {
            soundTimer -= Time.deltaTime;
            if (soundTimer <= 0f)
            {
                soundRecentlyHeard = false;
            }
        }

        bool lookBySound = !enemyManager.playerVisible && soundRecentlyHeard;

        if (enemyManager.playerVisible || lookBySound)
        {
            // 고개만 돌릴 때: 멈춤
            agent.updateRotation = false;
            agent.isStopped = true;

            var target = enemyManager.playerTransform != null ? enemyManager.playerTransform.position : transform.position + transform.forward;
            float spd = enemyManager.playerVisible ? watchTurnSpeedVision : watchTurnSpeedSound;
            RotateTowards(target, spd);
        }
        else
        {
            // 순찰 재개
            agent.updateRotation = true;
            agent.isStopped = false;
            Patrol();
        }
    }
    void HandleChase()
    {
        if (enemyManager == null || enemyManager.playerTransform == null)
        {
            Debug.Log("enemyManager가 올바르지 않습니다.");
            return;
        }
        // 추적 시간 측정 (시야가 있든 없든 5초 카운트)
        chaseTimer += Time.deltaTime;

        if (enemyManager.playerVisible)
        {
            // 시야를 확보하면 계속 쫓기 + 마지막 위치 갱신
            lastPlayerPos = enemyManager.playerTransform.position;
            lastKnownPlayerPos = lastPlayerPos;

            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetDestination(lastPlayerPos);

            chaseTimer = 0f;    //보이면 다시 타이머 리셋
        }
        else if (lastPlayerPos != Vector3.zero && !isPausingAfterChase)
        {
            // 시야를 잃었으면 마지막 위치까지는 계속 감
            if (Vector3.Distance(transform.position, lastPlayerPos) > waypointTolerance)
            {
                agent.isStopped = false;
                agent.updateRotation = true;
                agent.SetDestination(lastPlayerPos);
            }
        }
        // Chase 상태에서 Player와의 사이에 장애물이 없고 
        Vector3 flat = enemyManager.playerTransform.position - transform.position;
        flat.y = 0f;
        if (flat.sqrMagnitude <= killRange * killRange)
        {
            if (enemyManager.UpdateSharedRaycast(enemyManager.playerTransform) == EnemySearch.LosResult.Player)
            {
                PlayerKill();
                return;
            }
        }

        // 5초 경과했고 아직 시야에 없으면 2초간 멈춰서 의심
        if (chaseTimer >= chaseDuration && !enemyManager.playerVisible && !isPausingAfterChase)
        {
            isPausingAfterChase = true;
            pauseUntil = Time.time + postChasePause;

            agent.isStopped = true;
            agent.updateRotation = false;
        }
        // 의심 연출 중: 마지막 본 방향을 계속 보게 함
        if (isPausingAfterChase)
        {
            if (lastKnownPlayerPos != Vector3.zero)
                RotateTowards(lastKnownPlayerPos, watchTurnSpeedSound);

            if (Time.time >= pauseUntil)
            {
                // 의심 종료 → Warning으로 복귀
                enemyManager.currentState = EnemySearch.EnemyState.Warning;
            }
        }
    }
    void Serch()
    {
        if (isSearching || agent == null || !agent.isOnNavMesh) return;
        StartCoroutine(CoSearch());
    }

    IEnumerator CoSearch()
    {
        isSearching = true;

        Vector3 origin = transform.position;
        float end = Time.time + 15f;              // 15초
        agent.isStopped = false;
        agent.updateRotation = true;

        while (Time.time < end)
        {
            // 중단 조건: 사망/플레이어 발견
            if (enemyManager == null || enemyManager.GetState() == EnemySearch.EnemyState.Died) break;
            if (enemyManager.playerVisible && enemyManager.playerTransform != null)
            {
                enemyManager.currentState = EnemySearch.EnemyState.Chase;
                agent.SetDestination(enemyManager.playerTransform.position);
                isSearching = false; yield break;
            }

            // 반경 30m 랜덤 지점으로 이동
            if (NavMesh.SamplePosition(origin + (Vector3)(Random.insideUnitCircle * 30f), out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);

                // 도착/중단까지 대기
                while (Time.time < end && (agent.pathPending || agent.remainingDistance > Mathf.Max(agent.stoppingDistance, waypointTolerance)))
                {
                    if (enemyManager.playerVisible && enemyManager.playerTransform != null)
                    {
                        enemyManager.currentState = EnemySearch.EnemyState.Chase;
                        agent.SetDestination(enemyManager.playerTransform.position);
                        isSearching = false; yield break;
                    }
                    if (enemyManager.GetState() == EnemySearch.EnemyState.Died) { isSearching = false; yield break; }
                    yield return null;
                }
            }
            else yield return null;
        }

        // 수색 종료 → 경계(Warning)로 복귀 & 근처 웨이포인트 이동
        if (enemyManager != null && enemyManager.currentState != EnemySearch.EnemyState.Chase)
        {
            enemyManager.currentState = EnemySearch.EnemyState.Warning;
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                patrolIndex = GetClosestPatrolIndex(transform.position);
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
        }

        isSearching = false;
    }
    void PlayerKill()
    {
        // GameController에 요청, GamePhase 전환: RetryScene 호출
    }
    void ApplyDeathOnce()
    {
        deathApplied = true;

        agent.updateRotation = false;
        agent.isStopped = true;

        int deadLayer = LayerMask.NameToLayer("DeadBody");
        if (deadLayer == -1)
            Debug.LogWarning("DeadBody 레이어를 먼저 Project Settings > Tags and Layers에 추가하세요.", this);
        else
            SetLayerRecursively(transform, deadLayer);
    }

    static void SetLayerRecursively(Transform root, int layer)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }
    void RotateTowards(Vector3 worldPos, float degPerSec)
    {
        Vector3 to = worldPos - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude < 1e-4f) return;

        Quaternion target = Quaternion.LookRotation(to, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, degPerSec * Time.deltaTime);
    }
}

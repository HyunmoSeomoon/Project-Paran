using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : MovableAI
{
    // ──────────────────────────────────────────────
    // 행동 정의 (enum 값 = 우선순위, 클수록 우선)
    // ──────────────────────────────────────────────
    private enum Behavior
    {
        None = 0,
        Patrol = 1,        // 순찰(포인트 2개 이상) / 제자리 두리번(0~1개)
        SoundReact = 2,    // 소리/시야 감지 → 바라보기
        Decoyed = 3,       // 유인 지점으로 이동
        CorpseMove = 4,    // 시체로 집결
        CorpseSearch = 5,  // K-Means 클러스터 수색
        CollabChase = 6,   // 공동 추격(10초)
        Chase = 7,         // 직접 추격 (최상위)
    }

    // ── 인스펙터 직렬화 필드 (이름 변경 금지) ──
    [Header("순찰 설정")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private int patrolIndex = 0;
    [SerializeField] private float idleRotSpeed = 60f;
    [SerializeField] private Animator enemyAnimator;

    // ── 튜닝 파라미터 (기존 리터럴을 기본값으로 보존) ──
    [Header("행동 파라미터 - 이동 속도")]
    [SerializeField] private float patrolSpeed = 2.5f;   // 순찰 / 수색 / 유인 이동 속도
    [SerializeField] private float chaseSpeed = 4f;      // 추격 / 공동 추격 이동 속도

    [Header("행동 파라미터 - 추격")]
    [SerializeField] private float chaseDuration = 5f;        // 시야 상실 후 추격 유지 시간
    [SerializeField] private float chaseSuspicionTime = 3f;   // 추격 종료 후 의심(두리번) 시간
    [SerializeField] private float collabChaseDuration = 10f; // 공동 추격 시간

    [Header("행동 파라미터 - 시체 집결")]
    [SerializeField] private float corpseArriveDistance = 3f; // 시체 도착 판정 거리
    [SerializeField] private float corpseWaitTimeout = 15f;   // 시체 옆 집결 대기 상한(교착 방지)

    [Header("행동 파라미터 - 대기 시간")]
    [SerializeField] private float patrolWaitTime = 5f;    // 순찰 포인트 도착 후 대기
    [SerializeField] private float idleTurnWaitTime = 5f;  // 제자리 회전 후 대기
    [SerializeField] private float soundLatchTime = 2f;    // 소리 반응 유지 시간
    [SerializeField] private float decoyPreDelay = 3f;     // 유인 반응 전 대기
    [SerializeField] private float decoyPostDelay = 3f;    // 유인 지점 도착 후 대기

    // ── 상태 ──
    private EnemySearch enemy;
    private Vector3 lastPlayerPos;
    private float soundTimer = 0f;
    private bool soundRecentlyHeard = false;

    // ── 코루틴 제어기 ──
    private Coroutine activeRoutine;
    private Behavior activeBehavior = Behavior.None;
    private int behaviorGeneration = 0;

    // ── 이동 대기 결과 (WaitForArrival의 반환 채널) ──
    private bool lastArrivalSucceeded = false;

    public static event Action<EnemyMove, Transform> OnCorpseArrived;

    // ──────────────────────────────────────────────
    // 라이프사이클
    // ──────────────────────────────────────────────
    private void EnsureRefs()
    {
        if (enemy == null) enemy = GetComponent<EnemySearch>();
        if (enemyAnimator == null) enemyAnimator = GetComponent<Animator>();
    }

    protected override void Start()
    {
        base.Start();
        EnsureRefs();

        // 초기 행동 진입 (EnemySearch.Start의 SetState 순서에 의존하지 않도록 보강)
        if (activeBehavior == Behavior.None &&
            enemy != null && enemy.currentState != EnemySearch.EnemyState.Died)
        {
            RequestBehavior(Behavior.Patrol, PatrolBehavior());
        }
    }

    private void OnEnable()
    {
        EnsureRefs();
        EnemySearch.OnCorpseSpotted += HandleCorpseAlert;
        EnemySearch.OnStateChanged += HandleStateChange;
        EnemyManager.OnChaseTogether += ChaseTogether;
    }

    private void OnDisable()
    {
        EnemySearch.OnCorpseSpotted -= HandleCorpseAlert;
        EnemySearch.OnStateChanged -= HandleStateChange;
        EnemyManager.OnChaseTogether -= ChaseTogether;
    }

    // ──────────────────────────────────────────────
    // Update : 상태 관찰 → 제어기에 행동 요청만 수행
    //          (이동 명령은 전부 코루틴 쪽에서만 나간다)
    // ──────────────────────────────────────────────
    protected override void Update()
    {
        if (enemy == null) return;

        EnemySearch.EnemyState state = enemy.currentState;
        if (state == EnemySearch.EnemyState.Died) return;

        UpdateSoundLatch();

        // 안전망 : 아무 행동도 실행되지 않는 상태를 방치하지 않는다
        if (activeBehavior == Behavior.None)
        {
            RequestBehavior(Behavior.Patrol, PatrolBehavior());
            return;
        }

        // Chase 상태의 이동은 Chase 코루틴이 전담한다
        if (state != EnemySearch.EnemyState.Warning) return;
        if (!IsPlayerSensed()) return;

        if (activeBehavior == Behavior.Decoyed)
        {
            // 유인 중 플레이어 감지 → 유인 중단, 고개만 돌림 (기존 의도 보존)
            Debug.Log($"[{name}] Decoy 중 플레이어 감지 → Decoy 중단");
            RequestBehavior(Behavior.SoundReact, SoundReactBehavior(), true);
            return;
        }

        // Patrol만 선점, 그 이상 우선순위 행동 중이면 자동 거부됨
        RequestBehavior(Behavior.SoundReact, SoundReactBehavior());
    }

    private void UpdateSoundLatch()
    {
        // 소리가 계속 들리는 동안 래치를 갱신하고, 끊긴 뒤에도 잠시 반응을 유지한다.
        if (!enemy.playerVisible && enemy.checkSound)
        {
            soundTimer = soundLatchTime;
            soundRecentlyHeard = true;
        }
        else if (soundTimer > 0f)
        {
            soundTimer -= Time.deltaTime;
            if (soundTimer <= 0f) soundRecentlyHeard = false;
        }
    }

    private bool IsPlayerSensed()
    {
        if (enemy == null) return false;
        return enemy.playerVisible || soundRecentlyHeard;
    }

    // ──────────────────────────────────────────────
    // 중앙 코루틴 제어기
    // ──────────────────────────────────────────────
    private bool RequestBehavior(Behavior newBehavior, IEnumerator routine, bool force = false)
    {
        if (routine == null) return false;
        if (newBehavior == Behavior.None) return false;
        if (enemy != null && enemy.currentState == EnemySearch.EnemyState.Died) return false;

        // 같거나 더 높은 우선순위 행동이 진행 중이면 거부 (force 예외)
        if (!force && (int)activeBehavior >= (int)newBehavior)
            return false;

        StopActiveBehavior();

        behaviorGeneration++;
        activeBehavior = newBehavior;
        activeRoutine = StartCoroutine(RunBehavior(newBehavior, routine, behaviorGeneration));
        return true;
    }

    private void StopActiveBehavior()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
        activeBehavior = Behavior.None;
        behaviorGeneration++; // 남아 있는 래퍼 코루틴 무효화
        ResetBehaviorState();
    }

    /// <summary>행동 전환 시 agent / 애니메이터를 일관된 기본 상태로 되돌린다.</summary>
    private void ResetBehaviorState()
    {
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh) agent.ResetPath();
            agent.isStopped = false;
            agent.updateRotation = true;
        }

        if (enemyAnimator != null)
            enemyAnimator.ResetTrigger("Turn");

        // 속도 파라미터는 정지 값으로 통일 → 새 행동이 첫 프레임에 자기 속도를 설정한다
        SetMoveSpeed(0f);
    }

    /// <summary>inner 행동이 자연 종료되면 제어기가 폴백(Patrol)으로 복귀시킨다.</summary>
    private IEnumerator RunBehavior(Behavior behavior, IEnumerator inner, int generation)
    {
        yield return inner;

        if (generation != behaviorGeneration) yield break; // 이미 다른 행동으로 교체됨
        if (activeBehavior != behavior) yield break;

        activeRoutine = null;
        activeBehavior = Behavior.None;

        OnBehaviorFinished(behavior);
    }

    private void OnBehaviorFinished(Behavior finished)
    {
        if (enemy == null) return;
        if (enemy.currentState == EnemySearch.EnemyState.Died) return;

        // Patrol은 무한 루프이므로 정상 종료가 없다. 예외적으로 끝났다면
        // 여기서 즉시 재시작하지 않고 Update의 안전망에 맡긴다(같은 프레임 재귀 방지).
        if (finished == Behavior.Patrol) return;

        RequestBehavior(Behavior.Patrol, PatrolBehavior());
    }

    /// <summary>agent.speed와 애니메이터 Enemy_Speed를 항상 쌍으로 설정한다(발 미끄러짐 방지).</summary>
    private void SetMoveSpeed(float speed)
    {
        if (agent != null) agent.speed = speed;
        if (enemyAnimator != null) enemyAnimator.SetFloat("Enemy_Speed", speed);
    }

    // ──────────────────────────────────────────────
    // 외부 요청 API (EnemyManager는 반드시 이 경로만 사용)
    // ──────────────────────────────────────────────
    public bool RequestDecoy(Vector3 pos)
    {
        return RequestBehavior(Behavior.Decoyed, DecoyBehavior(pos));
    }

    public bool RequestCorpseMove(Transform corpse)
    {
        if (corpse == null) return false;
        return RequestBehavior(Behavior.CorpseMove, CorpseMoveBehavior(corpse));
    }

    public bool RequestCorpseSearch(List<Transform> cluster)
    {
        if (cluster == null || cluster.Count == 0) return false;
        return RequestBehavior(Behavior.CorpseSearch, CorpseSearchBehavior(cluster));
    }

    public bool RequestCollabChase(Transform target)
    {
        if (target == null) return false;
        return RequestBehavior(Behavior.CollabChase, CollabChaseBehavior(target));
    }

    /// <summary>
    /// 순찰 복귀 요청. 추격류(Chase / CollabChase)가 진행 중이면 존중하고 거부된다.
    /// </summary>
    public bool ReturnToPatrol()
    {
        bool force = (int)activeBehavior <= (int)Behavior.CorpseSearch;
        return RequestBehavior(Behavior.Patrol, PatrolBehavior(), force);
    }

    // ──────────────────────────────────────────────
    // 이벤트 수신 → 제어기 요청
    // ──────────────────────────────────────────────
    private void HandleStateChange(EnemySearch spotter, EnemySearch.EnemyState newState)
    {
        if (enemy == null) enemy = GetComponent<EnemySearch>();
        if (spotter != enemy) return;

        switch (newState)
        {
            case EnemySearch.EnemyState.Died:
                OnDeath();
                break;

            case EnemySearch.EnemyState.Chase:
                // 최상위 우선순위 → 무조건 선점
                RequestBehavior(Behavior.Chase, ChaseBehavior(), true);
                break;

            case EnemySearch.EnemyState.Warning:
            case EnemySearch.EnemyState.Idle:
                // 진행 중인 상위 행동이 있으면 유지(요청이 거부됨)
                RequestBehavior(Behavior.Patrol, PatrolBehavior());
                break;
        }
    }

    private void HandleCorpseAlert(EnemySearch spotter, Transform corpse)
    {
        if (spotter == null || corpse == null) return;
        if (enemy == null) enemy = GetComponent<EnemySearch>();
        // 로그 스팸 방지 — 발견 당사자만 기록한다 (전 맵 브로드캐스트 로그 제거)
        if (spotter != enemy) return;
        Debug.Log($"[이벤트 송신] {spotter.name}이 {corpse.name}을 발견했다!");
    }

    // [B1] sender 필터 — 이벤트 대상 적만 반응 (전 맵 브로드캐스트 방지)
    private void ChaseTogether(EnemySearch sender, Transform target)
    {
        if (enemy == null) enemy = GetComponent<EnemySearch>();
        if (sender != enemy) return;
        RequestCollabChase(target);
    }

    /// <summary>
    /// 사망이 아닌 외부 사유(체포 연출 등)로 이 적의 모든 행동을 즉시 정지시킨다.
    /// enabled = false만으로는 실행 중 코루틴이 멈추지 않으므로 반드시 이 경로를 사용한다.
    /// </summary>
    public void StopAllBehaviors()
    {
        StopActiveBehavior();
        if (agent != null && agent.enabled)
        {
            if (agent.isOnNavMesh) agent.ResetPath();
            agent.isStopped = true;
        }
    }

    public void OnDeath()
    {
        StopActiveBehavior();

        if (agent != null)
        {
            if (agent.enabled && agent.isOnNavMesh) agent.ResetPath();
            agent.isStopped = true;
            agent.enabled = false;
        }

        soundTimer = 0f;
        soundRecentlyHeard = false;
        lastPlayerPos = Vector3.zero;

        if (enemyAnimator != null)
        {
            enemyAnimator.ResetTrigger("Turn");
            enemyAnimator.SetFloat("Enemy_Speed", 0f);
        }
    }

    // ──────────────────────────────────────────────
    // 공용 헬퍼
    // ──────────────────────────────────────────────
    private IEnumerator RotateTo(Quaternion targetRot, float speed, float tolerance = 0.5f)
    {
        while (Quaternion.Angle(transform.rotation, targetRot) > tolerance)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, speed * Time.deltaTime);
            yield return null;
        }
    }

    private void AdvancePatrolIndex()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    private bool AgentUsable()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }

    /// <summary>agent가 없을 때도 안전하게 쓸 수 있는 도착 판정 거리.</summary>
    private float ArriveStoppingDistance
    {
        get { return agent != null ? agent.stoppingDistance : 0.5f; }
    }

    /// <summary>
    /// 목적지 도착(성공) 또는 경로 실패/타임아웃(실패)까지 대기한다.
    /// 호출 전에 Tracking(...)으로 목적지를 지정해 두어야 한다.
    /// 결과는 <see cref="lastArrivalSucceeded"/>로 전달된다(코루틴이므로 반환값을 못 쓴다).
    /// </summary>
    /// <param name="arriveDistance">도착으로 볼 remainingDistance 임계값</param>
    /// <param name="timeout">이 시간이 지나면 실패로 종료</param>
    /// <param name="abortWhen">true가 되면 실패로 즉시 종료(선택)</param>
    private IEnumerator WaitForArrival(float arriveDistance, float timeout = 20f, Func<bool> abortWhen = null)
    {
        lastArrivalSucceeded = false;
        float elapsed = 0f;

        while (true)
        {
            // agent를 쓸 수 없으면 이동 자체가 불가능 → 실패로 종료
            if (!AgentUsable()) yield break;

            if (abortWhen != null && abortWhen()) yield break;

            if (!agent.pathPending)
            {
                // NavMesh 밖 목적지 / 끊긴 경로 → 영구 대기 대신 실패로 종료
                if (agent.pathStatus != NavMeshPathStatus.PathComplete)
                {
                    Debug.LogWarning($"[{name}] 경로 실패(pathStatus={agent.pathStatus}) → 이동 대기 중단");
                    yield break;
                }

                if (agent.remainingDistance <= arriveDistance)
                {
                    lastArrivalSucceeded = true;
                    yield break;
                }
            }

            elapsed += Time.deltaTime;
            if (elapsed >= timeout)
            {
                Debug.LogWarning($"[{name}] 이동 타임아웃({timeout:F1}s) → 이동 대기 중단");
                yield break;
            }

            yield return null;
        }
    }

    // ──────────────────────────────────────────────
    // 행동 코루틴 : Patrol (최하위, 무한 루프)
    // ──────────────────────────────────────────────
    private IEnumerator PatrolBehavior()
    {
        while (true)
        {
            if (!AgentUsable())
            {
                yield return null;
                continue;
            }

            // [B2] 포인트가 0~1개면 제자리 두리번(90도 회전 → 5초 대기)을 반복한다
            if (patrolPoints == null || patrolPoints.Length < 2)
                yield return IdleTurnStep();
            else
                yield return PatrolStep();
        }
    }

    private IEnumerator IdleTurnStep()
    {
        SetMoveSpeed(0f);
        agent.isStopped = true;
        agent.updateRotation = false;

        if (enemyAnimator != null) enemyAnimator.SetTrigger("Turn");

        Quaternion targetRot = Quaternion.Euler(0f, 90f, 0f) * transform.rotation;
        yield return RotateTo(targetRot, idleRotSpeed, 0.1f);

        yield return new WaitForSeconds(idleTurnWaitTime);
    }

    private IEnumerator PatrolStep()
    {
        // 인스펙터에 저장된 patrolIndex가 범위를 벗어나 있어도 안전하게
        if (patrolIndex < 0 || patrolIndex >= patrolPoints.Length)
            patrolIndex = 0;

        Transform target = patrolPoints[patrolIndex];
        if (target == null)
        {
            AdvancePatrolIndex();
            yield return null;
            yield break;
        }

        SetMoveSpeed(patrolSpeed);
        Tracking(target.position);
        yield return null; // 경로 계산 한 프레임 대기

        yield return WaitForArrival(ArriveStoppingDistance);

        if (!lastArrivalSucceeded)
        {
            // 경로 실패/타임아웃 → 이 포인트는 건너뛰고 다음 포인트로 진행
            Debug.LogWarning($"[{name}] 순찰 포인트 {patrolIndex}({target.name}) 도달 불가 → 다음 포인트로");
            SetMoveSpeed(0f);
            if (agent != null && agent.enabled) agent.isStopped = true;
            AdvancePatrolIndex();
            yield return null;
            yield break;
        }

        Debug.Log($"[{name}] 순찰 포인트 {patrolIndex} 도착");
        SetMoveSpeed(0f);
        agent.isStopped = true;
        agent.updateRotation = false;

        // 도착 후 다음 포인트 방향으로 회전
        int nextIndex = (patrolIndex + 1) % patrolPoints.Length;
        Transform next = patrolPoints[nextIndex];
        Vector3 dirToNext = next != null ? next.position - transform.position : Vector3.zero;
        dirToNext.y = 0f;

        if (dirToNext.sqrMagnitude > 0.001f)
        {
            if (enemyAnimator != null) enemyAnimator.SetTrigger("Turn");
            yield return RotateTo(Quaternion.LookRotation(dirToNext.normalized, Vector3.up), idleRotSpeed);
        }

        yield return new WaitForSeconds(patrolWaitTime);
        patrolIndex = nextIndex;
    }

    // ──────────────────────────────────────────────
    // 행동 코루틴 : SoundReact
    // ──────────────────────────────────────────────
    private IEnumerator SoundReactBehavior()
    {
        SetMoveSpeed(0f);
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
        }

        // 감지가 유지되는 동안 플레이어 쪽을 계속 바라본다
        while (IsPlayerSensed())
        {
            if (enemy != null && enemy.playerTransform != null)
                RotToward(enemy.playerTransform.position);
            yield return null;
        }

        // 감지 상실 → 두리번거린 뒤 Warning 복귀 (제어기가 Patrol로 폴백)
        yield return LookPlayerAndResume(2f);
    }

    /// <summary>정지 → 2초 대기 → 플레이어 방향 회전 → sec초 대기 → Warning 복귀 보장.</summary>
    private IEnumerator LookPlayerAndResume(float sec)
    {
        Debug.Log($"[{name}] LookPlayerAndResume 호출");

        SetMoveSpeed(0f);
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
        }

        yield return new WaitForSeconds(2f);

        // 마지막으로 플레이어가 있던 방향 계산
        if (enemy != null && enemy.playerTransform != null)
        {
            Vector3 lastDir = enemy.playerTransform.position - transform.position;
            lastDir.y = 0f;
            if (lastDir.sqrMagnitude > 0.01f)
                yield return RotateTo(Quaternion.LookRotation(lastDir.normalized, Vector3.up), idleRotSpeed);
        }

        yield return new WaitForSeconds(sec);

        // 상태만 Warning으로 정리 — 다음 행동 결정은 제어기가 한다
        if (enemy != null && enemy.currentState != EnemySearch.EnemyState.Warning)
            enemy.SetState(EnemySearch.EnemyState.Warning);
    }

    // ──────────────────────────────────────────────
    // 행동 코루틴 : Chase (최상위)
    // ──────────────────────────────────────────────
    private IEnumerator ChaseBehavior()
    {
        SetMoveSpeed(chaseSpeed);

        float chaseTimer = 0f;

        // 추격 : 시야에 있으면 타이머 리셋, 놓치면 마지막 위치로 이동하며 누적
        while (chaseTimer < chaseDuration)
        {
            if (!AgentUsable())
            {
                yield return null;
                continue;
            }

            if (enemy.playerVisible && enemy.playerTransform != null)
            {
                SetMoveSpeed(chaseSpeed);
                lastPlayerPos = enemy.playerTransform.position;
                if (!agent.pathPending) Tracking(lastPlayerPos);
                chaseTimer = 0f;
            }
            else
            {
                SetMoveSpeed(chaseSpeed);
                if (lastPlayerPos != Vector3.zero && !agent.pathPending)
                    Tracking(lastPlayerPos);
                chaseTimer += Time.deltaTime;
            }

            yield return null;
        }

        // 추격 종료(5초) → 의심 3초 → Warning 복귀
        Debug.Log($"[{name}] 의심중");
        SetMoveSpeed(0f);
        yield return LookPlayerAndResume(chaseSuspicionTime);
    }

    // ──────────────────────────────────────────────
    // 행동 코루틴 : CollabChase
    // ──────────────────────────────────────────────
    private IEnumerator CollabChaseBehavior(Transform target)
    {
        Debug.Log($"[{name}] → {collabChaseDuration}초간 플레이어 위치로 공동 추적");
        SetMoveSpeed(chaseSpeed);

        float timer = 0f;
        while (timer < collabChaseDuration)
        {
            if (target != null && AgentUsable() && !agent.pathPending)
                Tracking(target.position);

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"[{name}] → 공동 추적 종료");
    }

    // ──────────────────────────────────────────────
    // 행동 코루틴 : CorpseMove
    // ──────────────────────────────────────────────
    private IEnumerator CorpseMoveBehavior(Transform corpse)
    {
        SetMoveSpeed(patrolSpeed);
        Tracking(corpse.position);
        yield return null; // 경로 계산 한 프레임 대기

        yield return WaitForArrival(corpseArriveDistance);

        SetMoveSpeed(0f);
        if (agent != null && agent.enabled) agent.isStopped = true;

        if (corpse == null) yield break;

        if (!lastArrivalSucceeded)
        {
            // 시체까지 갈 수 없음 → 집결 포기. 그래도 도착 보고는 1회 발화한다.
            // (매니저의 집결 집계에서 "영구 미도착"으로 남아 수색이 시작되지 못하는 것을 막는 목적)
            Debug.LogWarning($"[{name}] 시체({corpse.name})까지 도달 불가 → 집결 포기(도착만 보고)");
            OnCorpseArrived?.Invoke(this, corpse);
            yield break; // 제어기가 Patrol로 폴백
        }

        Debug.Log($"[{name}] 시체 근처 {corpseArriveDistance}m 도착");

        // ⚠ 동기 호출 : 수신 측(EnemyManager)이 여기서 즉시 CorpseSearch를 요청해
        //    이 코루틴을 선점·중지시킬 수 있다. StopCoroutine은 실행 중인 코드를 즉시
        //    끊지 못하므로, 선점 여부를 generation으로 직접 확인하고 잔여 코드를 포기한다.
        //    (그러지 않으면 아래 정지 처리가 방금 시작된 CorpseSearch의 이동 설정을 덮어쓴다)
        int gen = behaviorGeneration;
        OnCorpseArrived?.Invoke(this, corpse);
        if (gen != behaviorGeneration || activeBehavior != Behavior.CorpseMove) yield break;

        // 전원 집결을 기다리는 동안 시체 옆에서 대기한다.
        // 폴백으로 Patrol에 들어가 시체를 떠나는 어색한 동작 방지.
        // 단, 수색이 영영 시작되지 않는 교착을 막기 위해 유한 타임아웃을 둔다.
        SetMoveSpeed(0f);
        if (agent != null && agent.enabled) agent.isStopped = true;

        float waitTimer = 0f;
        while (waitTimer < corpseWaitTimeout)
        {
            waitTimer += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"[{name}] 시체 집결 대기 시간 초과 → 순찰 복귀");
    }

    // ──────────────────────────────────────────────
    // 행동 코루틴 : CorpseSearch (K-Means 클러스터 순회)
    // ──────────────────────────────────────────────
    private IEnumerator CorpseSearchBehavior(List<Transform> clusterPoints)
    {
        for (int i = 0; i < clusterPoints.Count; i++)
        {
            Transform target = clusterPoints[i];
            if (target == null) continue;

            SetMoveSpeed(patrolSpeed);
            Tracking(target.position);
            Debug.Log($"[{name}] 수색 포인트 {i + 1}/{clusterPoints.Count}로 이동 중...");
            yield return null;

            yield return WaitForArrival(ArriveStoppingDistance);

            if (!lastArrivalSucceeded)
            {
                // 경로 실패/타임아웃 → 이 수색 포인트는 건너뛰고 다음 포인트로
                Debug.LogWarning($"[{name}] 수색 포인트({target.name}) 도달 불가 → 다음 포인트로");
                SetMoveSpeed(0f);
                if (agent != null && agent.enabled) agent.isStopped = true;
                yield return null;
                continue;
            }

            // 도착 시 회전
            SetMoveSpeed(0f);
            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
                agent.updateRotation = false;
            }

            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
                yield return RotateTo(Quaternion.LookRotation(dir, Vector3.up), rotSpeed);

            Debug.Log($"[{name}] 포인트({target.name}) 도착 → 주변 관찰 중...");
            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 4f)); // 2~4초 랜덤 대기
        }

        // 수색 완료 → 다음 순찰 포인트부터 재개 (제어기가 Patrol로 폴백)
        AdvancePatrolIndex();
    }

    // ──────────────────────────────────────────────
    // 행동 코루틴 : Decoyed
    // ──────────────────────────────────────────────
    private IEnumerator DecoyBehavior(Vector3 decoyPos)
    {
        Debug.Log($"[{name}] Decoy 시작");

        SetMoveSpeed(0f);
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.updateRotation = false;
        }

        yield return new WaitForSeconds(decoyPreDelay);

        SetMoveSpeed(patrolSpeed);
        Tracking(decoyPos);
        yield return null; // 경로 계산 한 프레임 대기

        // Warning 상태가 아니게 되면(기존 의도) 이동을 중단하고 종료 수순으로 넘어간다
        yield return WaitForArrival(
            ArriveStoppingDistance,
            20f,
            () => enemy != null && enemy.currentState != EnemySearch.EnemyState.Warning);

        if (!lastArrivalSucceeded)
            Debug.LogWarning($"[{name}] Decoy 지점 도달 불가/중단 → Decoy 종료 수순");

        SetMoveSpeed(0f);
        if (agent != null && agent.enabled) agent.isStopped = true;

        yield return new WaitForSeconds(decoyPostDelay);

        Debug.Log($"[{name}] Decoy 종료");
        AdvancePatrolIndex();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : MovableAI
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private int patrolIndex = 0;
    [SerializeField] private float idleRotSpeed = 60f;
    bool arriveFlag = false;
    private float soundTimer = 0f, chaseTimer = 0f, pauseUntil = 0f; //pauseUntilW = 0f;
    private float chaseDuration = 5f;
    private bool soundRecentlyHeard = false;
    private EnemySearch enemy;
    private Vector3 lastPlayerPos, lastKnownPlayerPos;
    private bool isPausingAfterChase;
    private Coroutine currentRoutine; // 진행 중인 코루틴
    private bool isCollaborating = false;
    private bool isDecoyed = false;
    public static event Action<EnemyMove, Transform> OnCorpseArrived;
    private bool isSearchingCorpse = false;
    protected override void Start()
    {
        base.Start();
        enemy = GetComponent<EnemySearch>();
        // 초기 위치로 이동
        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[patrolIndex].position);
    }
    protected override void Update()
    {
        //도착 판정은 update에서 상시 체크
        CheckArrive();
        switch (enemy.currentState)
        {
            case EnemySearch.EnemyState.Idle:
                break;
            case EnemySearch.EnemyState.Warning:
                HandleWarning();
                break;
            case EnemySearch.EnemyState.Chase:
                HandleChase();
                break;
            case EnemySearch.EnemyState.Died:
                break;
        }
    }
    private void HandleStateChange(EnemySearch spotter, EnemySearch.EnemyState newState)
    {
        if (spotter != enemy) return;

        StopAllCoroutines();
        agent.updateRotation = true;
        agent.isStopped = false;

        if (newState == EnemySearch.EnemyState.Warning)
        {
            arriveFlag = false;
            if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }
    private void HandleCorpseAlert(EnemySearch spotter, Transform corpse)
    {
        Debug.Log($"[이벤트 송신] {spotter.name}이 {corpse.name}을 발견했다!");
    }
    void CheckArrive()
    {
        if (isDecoyed)
        {
            return;
        }
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !arriveFlag)
        {
            Debug.Log("도착!");
            arriveFlag = true;
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            if (currentRoutine != null)
                StopCoroutine(currentRoutine);
            if (patrolPoints.Length == 1) currentRoutine = StartCoroutine(IdleTurn());
            else currentRoutine = StartCoroutine(Patrol());
        }
    }
    protected override void MoveDefault()
    {
        base.MoveDefault();
    }
    IEnumerator IdleTurn()
    {
        // NavMesh 정지 - 도착
        agent.isStopped = true;
        agent.updateRotation = false;

        while (true)
        {
            // 90도씩 회전
            Quaternion startRot = transform.rotation;
            Quaternion targetRot = Quaternion.Euler(0f, 90f, 0f) * startRot;

            while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, idleRotSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(5f);
        }
    }
    IEnumerator Patrol()
    {
        // NavMesh 정지 - 도착
        agent.isStopped = true;
        agent.updateRotation = false;
        // 다음 포인트 방향 계산
        Vector3 dirToNext = patrolPoints[patrolIndex].position - transform.position;
        dirToNext.y = 0f;
        if (dirToNext.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dirToNext.normalized, Vector3.up);

            while (Quaternion.Angle(transform.rotation, targetRot) > 0.5f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, idleRotSpeed * Time.deltaTime);
                yield return null;
            }
        }
        yield return new WaitForSeconds(3f); // 3초 대기

        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(patrolPoints[patrolIndex].position);
        arriveFlag = false;
    }
    IEnumerator LookPlayerAndResume(float sec)
    {
        agent.isStopped = true;
        agent.updateRotation = false;

        // 마지막으로 플레이어가 있던 방향 계산
        Vector3 lastDir = enemy.playerTransform.position - transform.position;
        lastDir.y = 0f;
        if (lastDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lastDir.normalized, Vector3.up);

            while (Quaternion.Angle(transform.rotation, targetRot) > 0.5f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, idleRotSpeed * Time.deltaTime);
                yield return null;
            }
        }

        // n초 대기
        yield return new WaitForSeconds(sec);

        if (enemy.GetState() != EnemySearch.EnemyState.Warning)
        {
            enemy.SetState(EnemySearch.EnemyState.Warning);
            yield break;
        } 

        // 순찰 복귀
        if(enemy.GetState() == EnemySearch.EnemyState.Warning)
        {
            if (patrolPoints.Length == 1)
                currentRoutine = StartCoroutine(IdleTurn());
            else
                currentRoutine = StartCoroutine(Patrol());   
        }
    }
    void HandleWarning()
    {
        // 새로운 소리를 들었을 경우 타이머 리셋
        if (!enemy.playerVisible && enemy.checkSound && !soundRecentlyHeard)
        {
            soundTimer = 2f;
            Debug.Log("2초 기다립니다");
            soundRecentlyHeard = true;
        }

        // 타이머 감소
        if (soundTimer > 0f)
        {
            soundTimer -= Time.deltaTime;
            if (soundTimer <= 0f)
                soundRecentlyHeard = false;
        }

        bool lookBySound = !enemy.playerVisible && soundRecentlyHeard;

        if (enemy.playerVisible || lookBySound)
        {
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
                currentRoutine = null;
            }

            RotToward(enemy.playerTransform.position);
        }
        else
        {
            if (currentRoutine == null)
            {
                currentRoutine = StartCoroutine(LookPlayerAndResume(2f));
            }
        }
    }
    void HandleChase()
    {
        // 추적 시간 누적 (시야 여부와 관계없이)
        chaseTimer += Time.deltaTime;

        // 시야에 있을 때
        if (enemy.playerVisible)
        {
            lastPlayerPos = enemy.playerTransform.position;
            lastKnownPlayerPos = lastPlayerPos;

            if (!agent.pathPending) // 중복 경로 계산 방지
                Tracking(lastPlayerPos);

            chaseTimer = 0f; // 타이머 리셋
            return;
        }
        //시야에서 놓쳤을 때
        if (!enemy.playerVisible && lastPlayerPos != Vector3.zero && !isPausingAfterChase)
        {
            float dist = Vector3.Distance(transform.position, lastPlayerPos);

            // 마지막 본 위치까지 이동
            if (!agent.pathPending)
                Tracking(lastKnownPlayerPos);
        }
        // 추적 종료(5초) 후 의심 중
        if (chaseTimer >= chaseDuration && !enemy.playerVisible && !isPausingAfterChase)
        {
            Debug.Log("의심중");
            isPausingAfterChase = true;
            pauseUntil = Time.time + 3f;
            chaseTimer = 0f;
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
                currentRoutine = null;   
            }
            StartCoroutine(LookPlayerAndResume(3f));
        }
    }
    void ChaseTogether(EnemySearch sender, Transform target)
    {
        if (isCollaborating) return;
        StartCoroutine(ChaseTemporarily(target));
    }
    private IEnumerator ChaseTemporarily(Transform target)
    {
        isCollaborating = true;
        Debug.Log($"{name} → 10초간 플레이어 추적: 보이든안보이든 걍 위치로");

        float timer = 0f;

        while (timer < 10f)
        {
            if (target != null)
            {
                Tracking(target.position);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log($"{name} → 공동 추적 종료");
        isCollaborating = false;
    }
    public void MoveToCorpse(Transform corpse)
    {
        if (isSearchingCorpse) return;
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }
        StartCoroutine(MoveToCorpseRoutine(corpse));
    }
    private IEnumerator MoveToCorpseRoutine(Transform corpse)
    {
        isSearchingCorpse = true;
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(corpse.position);

        while (agent.pathPending || agent.remainingDistance > 3f)
            yield return null;

        Debug.Log($"[{name}] 시체({corpse.name}) 근처 3m 도착");
        agent.isStopped = true;
        OnCorpseArrived?.Invoke(this, corpse);
        isSearchingCorpse = false;
    }
    public IEnumerator Search(List<Transform> clusterPoints)
    {
        // 수색 포인트 순회
        for (int i = 0; i < clusterPoints.Count; i++)
        {
            Transform target = clusterPoints[i];
            if (target == null) continue;

            // 이동 시작
            agent.isStopped = false;
            agent.updateRotation = true;
            agent.SetDestination(target.position);
            Debug.Log($"[{name}] 수색 포인트 {i + 1}/{clusterPoints.Count}로 이동 중...");

            // 도착 대기
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
                yield return null;

            // 도착 시 회전 
            agent.isStopped = true;
            agent.updateRotation = false;

            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                while (Quaternion.Angle(transform.rotation, targetRot) > 0.5f)
                {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotSpeed * Time.deltaTime);
                    yield return null;
                }
            }

            Debug.Log($"[{name}] 포인트({target.name}) 도착 → 주변 관찰 중...");
            yield return new WaitForSeconds(UnityEngine.Random.Range(2f, 4f)); // 2~4초 랜덤 대기
        }
        // NavMeshAgent 정상화
        agent.isStopped = false;
        agent.updateRotation = true;

        // Warning으로 복귀
        EnemySearch search = GetComponent<EnemySearch>();
        if (search != null)
        {
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[patrolIndex].position);
            }
            else
            {
                StartCoroutine(IdleTurn());
            }
        }
    }
    public IEnumerator Decoyed(Vector3 decoyPos)
    {
        isDecoyed = true;
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        Tracking(decoyPos);
        yield return null;

        while (agent.pathPending)
            yield return null;
        Debug.Log($"[Decoyed] path ready. distance = {agent.remainingDistance:F2}");

        float timer = 0f;
        while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
        {
            agent.speed = 4f;
            if (enemy.GetState() != EnemySearch.EnemyState.Warning)
                yield break; // 상태가 바뀌면 즉시 중단
            if (timer % 1f < Time.deltaTime) // 1초마다 로그
            {
                Debug.Log($@"
                [Decoyed Debug]
                agent.enabled={agent.enabled}
                isOnNavMesh={agent.isOnNavMesh}
                hasPath={agent.hasPath}
                pathPending={agent.pathPending}
                pathStatus={agent.pathStatus}
                remainingDistance={agent.remainingDistance:F2}
                isStopped={agent.isStopped}
                SetDestination={agent.SetDestination(decoyPos)}
                ");
            }
            yield return null;
        }

        yield return new WaitForSeconds(3f);
        isDecoyed = false;

        if (enemy.GetState() != EnemySearch.EnemyState.Warning)
        yield break;

        if(enemy.GetState() == EnemySearch.EnemyState.Warning)
        {
            if (patrolPoints.Length == 1)
                currentRoutine = StartCoroutine(IdleTurn());
            else
                currentRoutine = StartCoroutine(Patrol());   
        }
    }
    public void OnDeath()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        StopAllCoroutines();
        enabled = false; // Update() 자체 비활성화
    }
    private void OnEnable()
    {
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
}

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : MovableAI
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private int patrolIndex = 0;
    bool arriveFlag = false;
    private float soundTimer = 0f, chaseTimer = 0f, pauseUntil = 0f;
    private float chaseDuration = 5f;
    private bool soundRecentlyHeard = false;
    private EnemySearch enemy;
    private Vector3 lastPlayerPos, lastKnownPlayerPos;
    private bool isPausingAfterChase;
    private bool isCollaborating = false;
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
        Debug.Log($"[매니저] {spotter.name}이 {corpse.name}을 발견했다!");
    }
    void CheckArrive()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !arriveFlag)
        {
            Debug.Log("도착!");
            arriveFlag = true;
            if (patrolPoints.Length == 1) StartCoroutine(IdleTurn());
            else StartCoroutine(Patrol());
        }
    }
    protected override void MoveDefault()
    {
        base.MoveDefault();
    }
    IEnumerator IdleTurn()
    {
        // NavMesh 정지 - 돛착
        agent.isStopped = true;
        agent.updateRotation = false;

        while (true)
        {
            // 90도씩 회전
            Quaternion startRot = transform.rotation;
            Quaternion targetRot = Quaternion.Euler(0f, 90f, 0f) * startRot;

            while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotSpeed * Time.deltaTime);
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

        // 180도 회전
        Quaternion startRot = transform.rotation;
        Quaternion targetRot = Quaternion.Euler(0f, 180f, 0f) * startRot;

        while (Quaternion.Angle(transform.rotation, targetRot) > 0.5f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotSpeed * Time.deltaTime);
            yield return null;
        }
            
        yield return new WaitForSeconds(5f); // 5초 대기

        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(patrolPoints[patrolIndex].position);
        arriveFlag = false;
        
    }
    void HandleWarning()
    {
        // 새로운 소리를 들었을 경우 타이머 리셋
        if (!enemy.playerVisible && enemy.checkSound && !soundRecentlyHeard)
        {
            soundTimer = Mathf.Max(0.5f, enemy.PlayerDetectRatio() * 2f);
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

        bool lookBySound = !enemy.playerVisible && soundRecentlyHeard;

        if (enemy.playerVisible || lookBySound)
        {
            var target = enemy.playerTransform.position;
            RotToward(target);
        }
        else
        {
            // 순찰 재개
            agent.isStopped = false;
            agent.updateRotation = true;
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
            pauseUntil = Time.time + 2f;
        }

        // 복귀
        if (isPausingAfterChase)
        {
            if (lastKnownPlayerPos != Vector3.zero)
                RotToward(lastKnownPlayerPos);

            if (Time.time >= pauseUntil)
            {
                Debug.Log("의심끝");
                // 의심 종료 → Warning 복귀
                isPausingAfterChase = false;
                enemy.SetState(EnemySearch.EnemyState.Warning);
            }
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
        if (enemy.currentState != EnemySearch.EnemyState.Chase) enemy.SetState(EnemySearch.EnemyState.Chase);
        isCollaborating = false;
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

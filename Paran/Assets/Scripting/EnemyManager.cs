using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    public static Action<EnemySearch, Transform> OnChaseTogether;
    public Transform playerTransform;
    private PlayerMove playerState;
    private List<EnemySearch> enemies = new List<EnemySearch>();
    private List<Transform> patrolPoints = new List<Transform>();
    private float[,] distTable;   //지점 간 adjacency matrix(사전 계산)

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
    private void OnEnable()
    {
        // ✅ EnemySearch 이벤트 구독
        EnemySearch.OnCorpseSpotted += HandleCorpseAlert;
        EnemySearch.OnStateChanged += HandlePlayerDetected;
        EnemySearch.OnEnemyDied += HandleEnemyDeath;
    }

    private void OnDisable()
    {
        // ✅ 구독 해제 (중복 방지)
        EnemySearch.OnCorpseSpotted -= HandleCorpseAlert;
        EnemySearch.OnStateChanged -= HandlePlayerDetected;
        EnemySearch.OnEnemyDied -= HandleEnemyDeath;
    }

    private void Start()
    {
        RegisterAllEnemies();
        RegisterAllPatrolPoints();
        ComputePatPointDist();
        if (playerTransform != null) playerState = playerTransform.GetComponent<PlayerMove>();
    }

    private void RegisterAllEnemies()
    {
        enemies.Clear();
        enemies.AddRange(FindObjectsByType<EnemySearch>(FindObjectsSortMode.None));
    }

    private void RegisterAllPatrolPoints()
    {
        patrolPoints.Clear();
        foreach (var point in GameObject.FindGameObjectsWithTag("PatrolPoint"))
            patrolPoints.Add(point.transform);
    }
    private void ComputePatPointDist()
    {
        int n = patrolPoints.Count;
        distTable = new float[n, n];
        
        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                float dist = Vector3.Distance(patrolPoints[i].position, patrolPoints[j].position);
                distTable[i, j] = dist;
                distTable[j, i] = dist; // 대칭
            }
        }
        Debug.Log($"[EnemyManager] PatrolPoint 거리 행렬 계산 완료 ({n}x{n})");
    }
    public Transform GetRandomPatrolPoint()
    {
        if (patrolPoints.Count == 0) return null;
        return patrolPoints[UnityEngine.Random.Range(0, patrolPoints.Count)];
    }

    // ──────────────────────────────────────────────
    // 🧩 이벤트 처리부
    // ──────────────────────────────────────────────
    private void HandleCorpseAlert(EnemySearch spotter, Transform corpse)
    {
        Debug.Log($"[EnemyManager] {spotter.name}이(가) 시체 {corpse.name}을 발견했습니다.");

        // 20미터 내 포인트, 10미터 내 적들로 각 리스트 생성
        var nearbyCorpse = patrolPoints.Where(p => Vector3.Distance(p.position, corpse.position) <= 20f).ToList();
        var enemyCorpse = enemies.Where(q => Vector3.Distance(q.transform.position, corpse.position) <= 10f).ToList();

        foreach (var e in enemyCorpse)
        {
            if (e == null || e == spotter || e.GetState() == EnemySearch.EnemyState.Died)
                continue;
            e.SetState(EnemySearch.EnemyState.Warning);
        }
    }
    private void HandlePlayerDetected(EnemySearch sender, EnemySearch.EnemyState state)
    {
        if (state == EnemySearch.EnemyState.Chase)
        {
            Debug.Log($"[EnemyManager] {sender.name}이 Chase 상태로 전환됨");

            foreach (var enemy in enemies)
            {
                if (enemy == sender) continue; // 자기 자신 제외
                if (enemy.currentState == EnemySearch.EnemyState.Died) continue;

                float dist = Vector3.Distance(sender.transform.position, enemy.transform.position);

                if (dist <= 15f)
                {
                    Debug.Log($"[Manager] {enemy.name}은 {dist:F1}m 거리 → 공동 추적");
                    OnChaseTogether?.Invoke(enemy, playerTransform);
                }
            }
        }
    }
    private void HandleEnemyDeath(EnemySearch deadEnemy)
    {
        if (enemies.Contains(deadEnemy))
        {
            enemies.Remove(deadEnemy);
            Debug.Log($"[EnemyManager] {deadEnemy.name}이 사망하여 리스트에서 제거됨.");
        }
    }
}

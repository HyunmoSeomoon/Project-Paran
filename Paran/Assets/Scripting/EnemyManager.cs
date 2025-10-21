using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    public static Action<EnemySearch, Transform> OnChaseTogether;
    public Transform playerTransform;
    private PlayerMove playerState;
    private List<EnemySearch> enemies = new List<EnemySearch>();
    private List<Transform> patrolPoints = new List<Transform>();

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

    public Transform GetRandomPatrolPoint()
    {
        if (patrolPoints.Count == 0) return null;
        return patrolPoints[UnityEngine.Random.Range(0, patrolPoints.Count)];
    }

    // ──────────────────────────────────────────────
    // 🧩 이벤트 처리부
    // ──────────────────────────────────────────────

    /// <summary>
    /// (1) 시체 발견 시 호출됨
    /// </summary>
    private void HandleCorpseAlert(EnemySearch spotter, Transform corpse)
    {
        Debug.Log($"[EnemyManager] {spotter.name}이(가) 시체 {corpse.name}을 발견했습니다.");

        // 주변 적들에게 Warning 전파
        BroadcastWarning(spotter, corpse.position, 15f);
    }

    /// <summary>
    /// 플레이어 발견 공동 추적 이벤트
    private void HandlePlayerDetected(EnemySearch sender, EnemySearch.EnemyState state)
    {
        if (state == EnemySearch.EnemyState.Chase)
        {
            Debug.Log($"[Manager] {sender.name}이 Chase 상태로 전환됨");

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

    /// <summary>
    /// (3) 적 사망 시 호출됨
    /// </summary>
    private void HandleEnemyDeath(EnemySearch deadEnemy)
    {
        if (enemies.Contains(deadEnemy))
        {
            enemies.Remove(deadEnemy);
            Debug.Log($"[EnemyManager] {deadEnemy.name}이 사망하여 리스트에서 제거됨.");
        }
    }

    // ──────────────────────────────────────────────
    // 🧩 상태 전파 로직
    // ──────────────────────────────────────────────

    private void BroadcastWarning(EnemySearch sender, Vector3 sourcePos, float radius)
    {
        foreach (var e in enemies)
        {
            if (e == null || e == sender || e.GetState() == EnemySearch.EnemyState.Died)
                continue;

            float dist = Vector3.Distance(e.transform.position, sourcePos);
            if (dist <= radius)
            {
                Debug.Log($"[EnemyManager] {e.name}이(가) {sender.name}의 경보를 듣고 경계 상태로 전환");
                e.SetState(EnemySearch.EnemyState.Warning);
            }
        }
    }

    private void BroadcastAlert(EnemySearch sender, Vector3 playerPos, float radius)
    {
        foreach (var e in enemies)
        {
            if (e == null || e == sender || e.GetState() == EnemySearch.EnemyState.Died)
                continue;

            float dist = Vector3.Distance(e.transform.position, playerPos);
            if (dist <= radius)
            {
                Debug.Log($"[EnemyManager] {e.name}이(가) {sender.name}의 경보를 듣고 추적 상태로 전환");
                e.SetState(EnemySearch.EnemyState.Chase);
            }
        }
    }
}

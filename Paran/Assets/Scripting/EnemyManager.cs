using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    public static Action<EnemySearch, Transform> OnChaseTogether;
    public Transform playerTransform;

    [Header("반경 설정")]
    [SerializeField] private float corpseSearchPointRadius = 20f; // 시체 주변 수색 포인트 수집 반경
    [SerializeField] private float corpseGatherRadius = 10f;      // 시체로 집결시킬 적 수집 반경
    [SerializeField] private float collabChaseRadius = 15f;       // 공동 추격 전파 반경
    [SerializeField] private float decoyRadius = 30f;             // 유인 전파 반경

    private List<EnemySearch> enemies = new List<EnemySearch>();
    private List<Transform> patrolPoints = new List<Transform>();
    private List<Transform> corpsePositions = new List<Transform>();
    private List<Transform> nearbyPoints = new List<Transform>();
    private List<EnemySearch> nearbyEnemies = new List<EnemySearch>();
    private float[,] distTable;   //지점 간 adjacency matrix(사전 계산)
    private bool corpseSearchStarted = false;
    // ✅ [B5] 도착 판정을 EnemyMove가 보고하는 방식으로 통일 (직선거리 기준 불일치 제거)
    private readonly HashSet<EnemyMove> arrivedAtCorpse = new HashSet<EnemyMove>();
    // ✅ [F3] 집결 요청을 실제로 "수락"한 적만 추적한다.
    //         (우선순위로 거부된 적을 기다리다 수색이 영영 시작되지 않는 문제 방지)
    private readonly HashSet<EnemyMove> gatheringMovers = new HashSet<EnemyMove>();
    // ✅ [F3] 대응(집결→수색 배정)이 끝난 시체. 이 시체의 신규 알림만 무시하고,
    //         다른 새 시체는 정상 처리된다.
    private readonly HashSet<Transform> resolvedCorpses = new HashSet<Transform>();
    private Transform activeCorpse;

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
        EnemyMove.OnCorpseArrived += HandleCorpseSearch;
        PlayerMove.OnDecoyEnemies += HandleDecoy;
    }

    private void OnDisable()
    {
        // ✅ 구독 해제 (중복 방지)
        EnemySearch.OnCorpseSpotted -= HandleCorpseAlert;
        EnemySearch.OnStateChanged -= HandlePlayerDetected;
        EnemySearch.OnEnemyDied -= HandleEnemyDeath;
        EnemyMove.OnCorpseArrived -= HandleCorpseSearch;
        PlayerMove.OnDecoyEnemies -= HandleDecoy;
    }

    private void Start()
    {
        RegisterAllEnemies();
        RegisterAllPatrolPoints();
        ComputePatPointDist();
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
    private List<List<Transform>> KMeansClusterWithCache(List<Transform> subset, int k, int maxIter = 20)
    {
        if (subset == null || distTable == null) return new List<List<Transform>>();

        // ✅ [B5] patrolPoints에 없는 포인트는 distTable 인덱스가 -1이 되므로 제외
        subset = subset.Where(p => p != null && patrolPoints.IndexOf(p) >= 0).ToList();

        int n = subset.Count;
        if (n == 0 || k <= 0) return new List<List<Transform>>();

        // ✅ [B5] 클러스터 수를 포인트 수로 클램프 → centers[c] 인덱스 예외 방지
        k = Mathf.Min(k, n);

        // (1) subset을 patrolPoints의 인덱스로 매핑
        int[] subsetIdx = subset.Select(p => patrolPoints.IndexOf(p)).ToArray();

        // (2) 무작위 초기 중심 선택
        System.Random rand = new System.Random();
        List<int> centers = subsetIdx.OrderBy(x => rand.Next()).Take(k).ToList();

        int[] assignment = new int[n];
        bool changed = true;
        int iter = 0;

        while (changed && iter < maxIter)
        {
            changed = false;
            iter++;

            // (3) 각 포인트를 가장 가까운 중심에 할당 (거리 행렬 이용)
            for (int i = 0; i < n; i++)
            {
                float bestDist = float.MaxValue;
                // ✅ [B5] centers에 없는 값이 남아 IndexOf가 -1이 되는 일을 막는다
                int bestCenter = centers[0];

                foreach (int cIdx in centers)
                {
                    float d = distTable[subsetIdx[i], cIdx];
                    if (d < bestDist)
                    {
                        bestDist = d;
                        bestCenter = cIdx;
                    }
                }

                int newCluster = centers.IndexOf(bestCenter);
                if (assignment[i] != newCluster)
                {
                    assignment[i] = newCluster;
                    changed = true;
                }
            }

            // (4) 중심 재계산 → 각 클러스터의 중심에 가장 가까운 실제 포인트 선택
            for (int c = 0; c < k; c++)
            {
                var clusterPoints = subsetIdx.Where((_, i) => assignment[i] == c).ToList();
                if (clusterPoints.Count == 0) continue;

                float bestSum = float.MaxValue;
                int bestPoint = clusterPoints[0];

                foreach (int candidate in clusterPoints)
                {
                    float sum = 0f;
                    foreach (int other in clusterPoints)
                        sum += distTable[candidate, other];
                    if (sum < bestSum)
                    {
                        bestSum = sum;
                        bestPoint = candidate;
                    }
                }
                centers[c] = bestPoint;
            }
        }

        // (5) 결과 클러스터 구성
        List<List<Transform>> result = new List<List<Transform>>();
        for (int c = 0; c < k; c++)
            result.Add(new List<Transform>());

        for (int i = 0; i < n; i++)
            result[assignment[i]].Add(subset[i]);

        Debug.Log($"[KMeans] 최종 결과 (총 클러스터 {k}개, 반복 {iter}회)");
        for (int c = 0; c < k; c++)
        {
            string clusterInfo = $"Cluster {c + 1}: ";
            if (result[c].Count == 0)
            {
                clusterInfo += "(비어있음)";
            }
            else
            {
                clusterInfo += string.Join(", ",
                    result[c].Select(p => p.name));
            }
            Debug.Log(clusterInfo);
        }
        return result;
    }
    // ──────────────────────────────────────────────
    // 🧩 이벤트 처리부
    // ──────────────────────────────────────────────
    private void HandleCorpseAlert(EnemySearch spotter, Transform corpse)
    {
        if (spotter == null || corpse == null) return;

        // ✅ [F3] 이미 대응(수색 배정)이 끝난 시체는 무시한다.
        //         다른(새) 시체 알림은 아래 로직으로 정상 처리된다.
        if (resolvedCorpses.Contains(corpse))
        {
            Debug.Log($"[EnemyManager] {corpse.name}은 이미 대응이 끝난 시체 → 무시");
            return;
        }

        // ✅ [B4/F3] 같은 시체에 대한 집결/수색이 이미 진행 중이면 초기화하지 않는다
        //            (집결 상태를 리셋하면 이미 집결 중인 적이 재요청을 거부당해 교착된다)
        if (activeCorpse == corpse && (corpseSearchStarted || gatheringMovers.Count > 0))
        {
            Debug.Log($"[EnemyManager] {corpse.name} 대응이 이미 진행 중 → 재집결 무시");
            return;
        }

        Debug.Log($"[EnemyManager] {spotter.name}이(가) 시체 {corpse.name}을 발견했습니다.");

        corpseSearchStarted = false;
        activeCorpse = corpse;
        arrivedAtCorpse.Clear();
        gatheringMovers.Clear(); // 시체가 바뀌면 집결 추적을 초기화

        // corpseSearchPointRadius 내 포인트, corpseGatherRadius 내 적들로 각 리스트 생성
        nearbyPoints = patrolPoints.Where(p => p != null && Vector3.Distance(p.position, corpse.position) <= corpseSearchPointRadius).ToList();
        nearbyEnemies = enemies.Where(e => e != null && e.currentState != EnemySearch.EnemyState.Died && Vector3.Distance(e.transform.position, corpse.position) <= corpseGatherRadius).ToList();

        foreach (var e in nearbyEnemies)
        {
            if (e.currentState == EnemySearch.EnemyState.Idle) e.SetState(EnemySearch.EnemyState.Warning);
            EnemyMove mover = e.GetComponent<EnemyMove>();
            if (mover == null) continue;

            // ✅ [F3] 집결을 수락한(우선순위 통과) 적만 집계 대상에 넣는다
            if (mover.RequestCorpseMove(corpse)) // ✅ 공개 요청 API 경유
                gatheringMovers.Add(mover);
            else
                Debug.Log($"[EnemyManager] {e.name}은 상위 행동 중 → 집결 대상 제외");
        }

        if (gatheringMovers.Count == 0)
        {
            // 집결 가능한 적이 없다 → 이 시체는 미처리로 남겨 두고(resolved 아님)
            // activeCorpse를 비워 이후 다른 적의 발견 알림이 정상 처리되게 한다.
            Debug.Log($"[EnemyManager] {corpse.name}에 집결 가능한 적이 없음 → 대응 보류");
            activeCorpse = null;
        }
    }
    private void HandleCorpseSearch(EnemyMove mover, Transform corpse)
    {
        if (mover == null || corpse == null) return;
        if (activeCorpse == null || corpse != activeCorpse) return;
        if (corpseSearchStarted) return;

        // ✅ [B5] EnemyMove가 보고한 도착만 기준으로 삼는다 (판정 기준 통일)
        arrivedAtCorpse.Add(mover);

        // ✅ [F4] 도착 시점에 집결 조건 재평가
        TryStartCorpseSearch();
    }

    /// <summary>
    /// ✅ [F3/F4] 집결 조건을 평가해 조건이 충족되면 수색을 배정한다.
    /// 도착(HandleCorpseSearch) / 사망(HandleEnemyDeath) / Chase 전환(HandlePlayerDetected)에서
    /// 각각 호출되어, 마지막 도착 이후 집결자가 이탈해도 교착되지 않는다.
    /// </summary>
    private void TryStartCorpseSearch()
    {
        if (corpseSearchStarted) return;
        if (activeCorpse == null) return;

        // 사망 / 추격으로 이탈한 집결자를 정리한다 (기존 Died/Chase 제외 필터 유지)
        PruneGatheringMovers();

        List<EnemyMove> expected = gatheringMovers.ToList();
        if (expected.Count == 0) return;
        if (!expected.All(m => arrivedAtCorpse.Contains(m))) return;

        Transform corpse = activeCorpse;
        corpseSearchStarted = true;

        // ✅ [B5] 수색 포인트가 없으면 수색 단계를 건너뛰고 순찰 복귀
        if (nearbyPoints == null || nearbyPoints.Count == 0)
        {
            Debug.LogWarning("[EnemyManager] 시체 주변 수색 포인트가 없음 → 수색 생략, 순찰 복귀");
            foreach (var m in expected) m.ReturnToPatrol();
            FinishCorpseResponse(corpse);
            return;
        }

        // ✅ [B5] 적 수가 포인트 수보다 많으면 k를 클램프
        int k = Mathf.Min(expected.Count, nearbyPoints.Count);
        List<List<Transform>> clusters = KMeansClusterWithCache(nearbyPoints, k);

        for (int i = 0; i < expected.Count; i++)
        {
            EnemyMove m = expected[i];
            EnemySearch search = m.GetComponent<EnemySearch>();

            // 추격 중인 적의 상태는 건드리지 않는다 (Chase 최우선 원칙)
            if (search != null && search.currentState != EnemySearch.EnemyState.Chase)
                search.SetState(EnemySearch.EnemyState.Warning);

            if (i < clusters.Count && clusters[i] != null && clusters[i].Count > 0)
            {
                if (m.RequestCorpseSearch(clusters[i])) // ✅ 공개 요청 API 경유
                    Debug.Log($"[EnemyManager] {m.name}이 {clusters[i].Count}개 포인트 수색 시작");
            }
            else
            {
                // 클러스터가 없는 남는 적은 순찰 복귀
                m.ReturnToPatrol();
            }
        }

        FinishCorpseResponse(corpse);
    }

    /// <summary>
    /// ✅ [F3] 이 시체에 대한 대응을 종료 처리한다.
    /// corpseSearchStarted를 영구 true로 남기지 않고 resolvedCorpses로 옮겨,
    /// 수색이 진행 중이어도 "다른 시체" 알림은 정상 처리되게 한다.
    /// </summary>
    private void FinishCorpseResponse(Transform corpse)
    {
        if (corpse != null) resolvedCorpses.Add(corpse);
        if (activeCorpse == corpse) activeCorpse = null;

        corpseSearchStarted = false;
        gatheringMovers.Clear();
        arrivedAtCorpse.Clear();
    }

    /// <summary>✅ [F4] 사망 / 추격으로 이탈한 집결자를 집결 집합에서 제거한다.</summary>
    private void PruneGatheringMovers()
    {
        gatheringMovers.RemoveWhere(m =>
        {
            if (m == null) return true;
            EnemySearch s = m.GetComponent<EnemySearch>();
            if (s == null) return true;
            return s.currentState == EnemySearch.EnemyState.Died
                || s.currentState == EnemySearch.EnemyState.Chase;
        });
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

                if (dist <= collabChaseRadius)
                {
                    Debug.Log($"[Manager] {enemy.name}은 {dist:F1}m 거리 → 공동 추적");
                    OnChaseTogether?.Invoke(enemy, playerTransform);
                }
            }

            // ✅ [F4] 집결자가 Chase로 이탈했을 수 있으므로 집결 조건을 재평가한다
            TryStartCorpseSearch();
        }
    }
    private void HandleEnemyDeath(EnemySearch deadEnemy)
    {
        if (deadEnemy == null) return;

        if (enemies.Contains(deadEnemy))
        {
            enemies.Remove(deadEnemy);
            corpsePositions.Add(deadEnemy.transform);
            Debug.Log($"[EnemyManager] {deadEnemy.name}이 사망하여 리스트에서 제거됨.");
        }

        // ✅ [B5] 사망한 적을 집결 대기 목록에서 제거 → allArrived 영구 대기 방지
        nearbyEnemies.Remove(deadEnemy);
        EnemyMove deadMover = deadEnemy.GetComponent<EnemyMove>();
        if (deadMover != null)
        {
            arrivedAtCorpse.Remove(deadMover);
            gatheringMovers.Remove(deadMover); // ✅ [F4]
        }

        // ✅ [F4] 마지막 도착 이후 집결자가 사망해도 재평가되도록 한다
        TryStartCorpseSearch();
    }
    private void HandleDecoy(Vector3 decoyPos)
    {
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy.currentState != EnemySearch.EnemyState.Warning) continue;
            float dist = Vector3.Distance(decoyPos, enemy.transform.position);
            if (dist <= decoyRadius)
            {
                EnemyMove mover = enemy.GetComponent<EnemyMove>();
                if (mover != null) mover.RequestDecoy(decoyPos); // ✅ 공개 요청 API 경유
            }
        }
    }
    public List<Transform> GetCorpsePositions()
    {
        return corpsePositions;
    }
}

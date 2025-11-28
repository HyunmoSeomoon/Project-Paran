using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    public static Action<EnemySearch, Transform> OnChaseTogether;
    public Transform playerTransform;
    private PlayerMove playerState;
    private List<EnemySearch> enemies = new List<EnemySearch>();
    private List<Transform> patrolPoints = new List<Transform>();
    private List<Transform> corpsePositions = new List<Transform>();
    private List<Transform> nearbyPoints = new List<Transform>();
    private List<EnemySearch> nearbyEnemies = new List<EnemySearch>();
    private float[,] distTable;   //지점 간 adjacency matrix(사전 계산)
    private bool corpseSearchStarted = false;

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
    private void TestKMeansClustering()
    {
        int n = patrolPoints.Count;
        if (n == 0)
        {
            Debug.LogWarning("[KMeans Test] PatrolPoint가 없습니다!");
            return;
        }

        // 예시: 3개 그룹으로 나누기
        int k = 3;
        List<List<Transform>> clusters = KMeansClusterWithCache(patrolPoints, k);

        // 클러스터 결과 콘솔 출력
        Debug.Log($"[KMeans Test] 총 {k}개 클러스터로 분류 완료.");
        for (int i = 0; i < clusters.Count; i++)
        {
            string clusterInfo = $"Cluster {i + 1}: " +
                string.Join(", ", clusters[i].Select(p => p.name));
            Debug.Log(clusterInfo);
        }
    }
    private List<List<Transform>> KMeansClusterWithCache(List<Transform> subset, int k, int maxIter = 20)
    {
        int n = subset.Count;
        if (n == 0 || k <= 0) return new List<List<Transform>>();

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
                int bestCenter = 0;

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
        corpseSearchStarted = false;
        Debug.Log($"[EnemyManager] {spotter.name}이(가) 시체 {corpse.name}을 발견했습니다.");

        // 20미터 내 포인트, 10미터 내 적들로 각 리스트 생성
        nearbyPoints = patrolPoints.Where(p => Vector3.Distance(p.position, corpse.position) <= 20f).ToList();
        nearbyEnemies = enemies.Where(e => e != null && e.GetState() != EnemySearch.EnemyState.Died).Where(q => Vector3.Distance(q.transform.position, corpse.position) <= 10f).ToList();

        foreach (var e in nearbyEnemies)
        {
            if (e.GetState() == EnemySearch.EnemyState.Idle) e.SetState(EnemySearch.EnemyState.Warning);
            EnemyMove mover = e.GetComponent<EnemyMove>();
            if (mover != null) mover.MoveToCorpse(corpse);
        } 
    }
    private void HandleCorpseSearch(EnemyMove mover, Transform corpse)
    {
        if (corpseSearchStarted) return;

        bool allArrived = nearbyEnemies.All(e => Vector3.Distance(e.transform.position, corpse.position) <= 3f);
        if (!allArrived) return;
        corpseSearchStarted = true;
        
        List<List<Transform>> clusters = KMeansClusterWithCache(nearbyPoints, nearbyEnemies.Count);

        for (int i = 0; i < nearbyEnemies.Count; i++)
        {
            var cluster = clusters[i];
            var enemy = nearbyEnemies[i];

            enemy.SetState(EnemySearch.EnemyState.Warning);
            mover = enemy.GetComponent<EnemyMove>();
            if (mover != null && cluster.Count > 0)
            {
                mover.StopAllCoroutines();
                mover.StartCoroutine(mover.Search(cluster));
                Debug.Log($"[EnemyManager] {enemy.name}이 {cluster.Count}개 포인트 수색 시작");
            }
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
            corpsePositions.Add(deadEnemy.transform);
            Debug.Log($"[EnemyManager] {deadEnemy.name}이 사망하여 리스트에서 제거됨.");
        }
    }
    private void HandleDecoy(Vector3 decoyPos)
    {
        foreach (var enemy in enemies)
        {
            if (enemy.GetState() != EnemySearch.EnemyState.Warning) continue;
            float dist = Vector3.Distance(decoyPos, enemy.transform.position);
            if (dist <= 15f)
            {
                EnemyMove mover = enemy.GetComponent<EnemyMove>();
                mover.StartCoroutine(mover.Decoyed(decoyPos));
            }
        }
    }
    public List<Transform> GetCorpsePositions()
    {
        return corpsePositions;
    }
}

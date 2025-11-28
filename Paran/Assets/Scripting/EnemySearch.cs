using System;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EnemySearch : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Warning,
        Chase,
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
    [SerializeField] private float visualBoost = 2f;     // 시야로 감지 중 가산(배수)
    private float playerInSightTimer = 0f;
    private float timeToSwitchState = 10f;
    private float searchTimer = 0f;
    private PlayerMove playerState;

    public bool playerVisible = false;
    public bool checkSound = false;
    private LosResult lastLos;
    // ✅ 시체 감지 관련 변수
    private float corpseCheckInterval = 0.5f;
    private float corpseCheckTimer = 0f;
    public Vector3 corpseVec;
    public static event Action<EnemySearch, Transform> OnCorpseSpotted;
    public static event Action<EnemySearch, Transform> OnPlayerDetected;
    public static event Action<EnemySearch> OnEnemyDied;
    public static event Action<EnemySearch, EnemyState> OnStateChanged;
    // 상태 바뀔 때 이벤트 발생
    private EnemyState _currentState;
    public EnemyState currentState => _currentState;
    private void Start()
    {
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
        SetState(EnemyState.Warning);
    }
    private void Update()
    {
        if (playerTransform == null) return;
        if (currentState == EnemyState.Died) return;

        lastLos = UpdateSharedRaycast(playerTransform);
        playerVisible = IsPlayerInFOV(lastLos);
        checkSound = AuditoryCheck(lastLos);

        // ✅ 시체 감지는 모든 상태(Dead 제외)에서 수행
        corpseCheckTimer += Time.deltaTime;
        if (corpseCheckTimer >= corpseCheckInterval)
        {
            corpseCheckTimer = 0f;
            TryDetectDeadBody();
        }

        switch (currentState)
        {
            case EnemyState.Warning:
                {
                    bool sensed = playerVisible || checkSound;
                    if (sensed)
                    {
                        float detectRate = ComputeDetectRate(playerVisible);
                        playerInSightTimer += Time.deltaTime * detectRate;

                        if (playerInSightTimer >= timeToSwitchState)
                        {
                            Debug.Log($"플레이어 감지 (rate x{detectRate:F2})");
                            SetState(EnemyState.Chase);
                            OnPlayerDetected?.Invoke(this, transform);
                            searchTimer = 0f;
                        }
                    }
                    else
                    {
                        playerInSightTimer -= 2f * Time.deltaTime;
                        if (playerInSightTimer < 0f) playerInSightTimer = 0f;
                    }
                    break;
                }

            case EnemyState.Chase:
                {
                    if (playerVisible || checkSound)
                    {
                        searchTimer = 0f;
                    }
                    else
                    {
                        searchTimer += Time.deltaTime;
                    }
                    TryCatchPlayer();
                    break;
                }
        }
    }
    public LosResult UpdateSharedRaycast(Transform target)
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
        
        // ✅ 자기 레이어 제외
        int selfMask = 1 << gameObject.layer;
        int effectiveMask = losMask & ~selfMask;

        if (Physics.Linecast(origin, dest, out sharedHit, effectiveMask, QueryTriggerInteraction.Ignore))
        {
            // Player가 첫 히트?
            if (sharedHit.collider.transform.root == target.root) return LosResult.Player;
            // 창문/벽 구분
            int hitLayer = sharedHit.collider.gameObject.layer;
            if (hitLayer == LayerMask.NameToLayer("Window")) return LosResult.Window;
            if (hitLayer == LayerMask.NameToLayer("Wall")) return LosResult.Wall;
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
        if (result == LosResult.Player && playerState != null && playerState.currentState == PlayerMove.PlayerState.Run) return true; // 청각으로 감지됨

        return false;
    }
    // ✅ 다른 적 중 Died 상태를 감지
    private void TryDetectDeadBody()
    {
        var corpses = EnemyManager.Instance?.GetCorpsePositions();
        if (corpses == null || corpses.Count == 0) return;

        foreach (var corpsePos in corpses)
        {
            Vector3 toCorpse = corpsePos.position - transform.position;
            float dist = new Vector2(toCorpse.x, toCorpse.z).magnitude;
            if (dist > viewRange) continue;

            float angle = Vector3.Angle(transform.forward, new Vector3(toCorpse.x, 0, toCorpse.z).normalized);
            if (angle > viewAngle * 0.5f) continue;

            // Raycast로 가림 여부만 확인
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 dest = corpsePos.position + Vector3.up * 0.5f;
            if (!Physics.Linecast(origin, dest, out var hit, ~0, QueryTriggerInteraction.Ignore))
            {
                HandleDeadBodySpotted(corpsePos);
                return;
            }

            int hitLayer = hit.collider.gameObject.layer;
            if (hitLayer == LayerMask.NameToLayer("Wall")) continue;

            HandleDeadBodySpotted(corpsePos);
            return;
        }
    }

    public void HandleDeadBodySpotted(Transform corpse)
    {
        Debug.Log($"{name}이(가) 시체({corpse.name})를 발견함!");
        SetState(EnemyState.Warning); // 시체를 보면 경계 상태로
        corpseVec = corpse.position;

        OnCorpseSpotted?.Invoke(this, corpse);
    }

    private float ComputeDetectRate(bool viaVision)
    {
        if (!viaVision) return 0f; // 시야에 안 보이면 감지 안 함

        // 거리(XZ 기준)
        Vector3 to = playerTransform.position - transform.position;
        float distance = new Vector2(to.x, to.z).magnitude;

        // 파라미터
        float maxRate = 10f;      // 최대 감지율
        float minRate = 2f;       // 최소 감지율
        float falloffDistance = 10f; // 감지율이 떨어지기 시작하는 기준 거리

        // 거리 기반 감쇠 (선형 or 곡선적 감쇠 가능)
        float t = Mathf.Clamp01(distance / falloffDistance);
        float rate = Mathf.Lerp(maxRate, minRate, t); // 가까울수록 maxRate, 멀수록 minRate

        // 시각 배수 적용
        rate *= visualBoost;

        if (playerState.currentState == PlayerMove.PlayerState.Crawl) rate *= 0.3f;
        if (playerState.currentState == PlayerMove.PlayerState.Run) rate *= 1.3f;

        return rate;
    }

    public float PlayerDetectRatio()
    {
        return Mathf.Clamp01(playerInSightTimer / timeToSwitchState);
    }

    private void TryCatchPlayer()
    {
        if (currentState != EnemyState.Chase) return;
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist <= 5f && lastLos == LosResult.Player)
        {
            OnCatched();
        }
    }

    private void OnCatched()
    {
        Debug.Log($"[EnemySearch] 플레이어를 잡음! → Game Over");

        // 1) 플레이어 동작 정지
        if (playerState != null)
        {
            playerState.enabled = false;
            var cc = playerState.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
        }

        // 2) 적 동작 정지
        var move = GetComponent<EnemyMove>();
        if (move != null)
        {
            move.enabled = false;
            if (move.GetComponent<NavMeshAgent>() != null)
                move.GetComponent<NavMeshAgent>().isStopped = true;
        }

        // 3) 게임 Phase 변경
        GameController.Instance.gamePhase = GameController.GamePhase.Retry;
    }


    public void SetState(EnemyState newState)
    {
        // 상태 변경 불필요하거나 이미 죽은 경우 리턴
        if (_currentState == newState || _currentState == EnemyState.Died) 
            return;

        // 사망 상태 처리
        if (newState == EnemyState.Died)
        {
            OnEnemyDied?.Invoke(this);
            _currentState = newState;

            enabled = false;
            playerVisible = false;
            checkSound = false;

            GetComponent<EnemyMove>()?.OnDeath();
            return; // 🔹 사망은 여기서 완전히 종료
        }

        // 일반 상태 변경
        _currentState = newState;
        OnStateChanged?.Invoke(this, _currentState);
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

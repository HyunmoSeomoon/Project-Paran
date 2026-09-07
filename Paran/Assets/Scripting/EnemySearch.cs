using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
        None,
        Player,
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

    [Header("감지 파라미터")]
    [SerializeField] private float maxDetectRate = 10f;          // 최대 감지율(근거리)
    [SerializeField] private float minDetectRate = 2f;           // 최소 감지율(원거리)
    [SerializeField] private float detectFalloffDistance = 10f;  // 감지율 감쇠 기준 거리
    [SerializeField] private float crawlRateMultiplier = 0.3f;   // 플레이어 포복 시 감지율 배율
    [SerializeField] private float runRateMultiplier = 1.3f;     // 플레이어 달리기 시 감지율 배율
    [SerializeField] private float detectDecayRate = 2f;         // 미감지 시 감지 게이지 감소율(초당)
    [SerializeField] private float catchDistance = 5f;           // 추격 중 체포 판정 거리

    [Header("청각 파라미터")]
    [SerializeField] private float wallMuffleFactor = 0.5f;  // 벽 너머 발소리 가청 거리 배율
    [SerializeField] private float hearingDetectRate = 3f;   // 소리만 들렸을 때의 감지율(시각보다 낮게)

    private float playerInSightTimer = 0f;
    private float timeToSwitchState = 10f;
    private PlayerMove playerState;
    private bool caught = false; // 체포 처리 1회성 가드

    public bool playerVisible = false;
    public bool checkSound = false;
    private LosResult lastLos;
    // ✅ 시체 감지 관련 변수
    private float corpseCheckInterval = 0.5f;
    private float corpseCheckTimer = 0f;
    public Vector3 corpseVec;
    // ✅ [B4] 이미 발견한 시체는 재발화하지 않는다 (0.5초마다 이벤트 반복 방지)
    private readonly HashSet<Transform> spottedCorpses = new HashSet<Transform>();
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
                        }
                    }
                    else
                    {
                        playerInSightTimer -= detectDecayRate * Time.deltaTime;
                        if (playerInSightTimer < 0f) playerInSightTimer = 0f;
                    }
                    break;
                }

            case EnemyState.Chase:
                {
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


        int selfMask = 1 << gameObject.layer;
        int effectiveMask = losMask & ~selfMask;

        if (Physics.Linecast(origin, dest, out sharedHit, effectiveMask, QueryTriggerInteraction.Ignore))
        {

            if (sharedHit.collider.transform.root == target.root) return LosResult.Player;

            int hitLayer = sharedHit.collider.gameObject.layer;
            if (hitLayer == LayerMask.NameToLayer("Window")) return LosResult.Window;
            if (hitLayer == LayerMask.NameToLayer("Wall")) return LosResult.Wall;

            return LosResult.Wall;
        }
        return LosResult.None;
    }

    private static float FlatDistance(Vector3 a, Vector3 b)
    {
        return new Vector2(a.x - b.x, a.z - b.z).magnitude;
    }

    private bool IsInViewCone(Vector3 targetPos)
    {
        if (FlatDistance(transform.position, targetPos) > viewRange) return false;
        Vector3 flat = targetPos - transform.position;
        flat.y = 0f;
        return Vector3.Angle(transform.forward, flat.normalized) <= viewAngle * 0.5f;
    }

    private bool IsPlayerInFOV(LosResult result)
    {
        if (playerTransform == null) return false;

        // 📌 1. 거리 컷 + 2. 시야각 컷
        if (!IsInViewCone(playerTransform.position)) return false;

        // 공유 result 사용
        if (result == LosResult.Player) return true;
        if (result == LosResult.Window)
            return playerState == null || playerState.currentState != PlayerMove.PlayerState.Crawl;
        return false;
    }

    private bool AuditoryCheck(LosResult result)
    {
        if (playerTransform == null) return false;

        // 발소리는 'Run' 상태에서만 발생한다
        if (playerState == null || playerState.currentState != PlayerMove.PlayerState.Run) return false;

        if (result == LosResult.Window) return false; // 창문은 청각 차단 (기존 유지)

        // LOS를 요구하지 않는다 — 벽 뒤 발소리도 들리되, 벽 너머는 가청 거리를 절반으로 감쇠
        float hearRange = (result == LosResult.Wall) ? probeDistance * wallMuffleFactor : probeDistance;

        return FlatDistance(transform.position, playerTransform.position) <= hearRange;
    }
    // ✅ 다른 적 중 Died 상태를 감지
    private void TryDetectDeadBody()
    {
        var corpses = EnemyManager.Instance?.GetCorpsePositions();
        if (corpses == null || corpses.Count == 0) return;

        // ✅ [B4] UpdateSharedRaycast와 동일하게 자기 레이어 제외 + losMask 적용
        //         (기존 ~0 마스크는 시체/자기 콜라이더에 먼저 맞아 벽 차폐가 무력화됐다)
        int selfMask = 1 << gameObject.layer;
        int effectiveMask = losMask & ~selfMask;

        // ✅ 플레이어 몸이 시체 앞을 지나면 차폐로 오판되던 문제 → Player 레이어 제외
        if (playerTransform != null)
            effectiveMask &= ~(1 << playerTransform.gameObject.layer);

        foreach (var corpse in corpses)
        {
            if (corpse == null) continue;
            if (spottedCorpses.Contains(corpse)) continue; // 이미 발견한 시체
            if (!IsInViewCone(corpse.position)) continue;

            // Raycast로 가림 여부만 확인
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 dest = corpse.position + Vector3.up * 0.5f;

            if (Physics.Linecast(origin, dest, out var hit, effectiveMask, QueryTriggerInteraction.Ignore))
            {
                // 시체 자신이 첫 히트라면 가려진 것이 아니다
                if (hit.collider.transform.root != corpse.root)
                {
                    int hitLayer = hit.collider.gameObject.layer;
                    // 창문을 통해서는 보이고, 벽 및 그 외 장애물은 차폐로 간주 (안전 기본값)
                    if (hitLayer != LayerMask.NameToLayer("Window")) continue;
                }
            }

            HandleDeadBodySpotted(corpse);
            return;
        }
    }

    public void HandleDeadBodySpotted(Transform corpse)
    {
        if (corpse == null) return;

        if (!spottedCorpses.Add(corpse)) return;

        Debug.Log($"{name}이(가) 시체({corpse.name})를 발견함!");
        SetState(EnemyState.Warning);
        corpseVec = corpse.position;

        OnCorpseSpotted?.Invoke(this, corpse);
    }

    private float ComputeDetectRate(bool viaVision)
    {
        // 시야 미확보 — 이번 프레임에 소리를 들었다면 청각 전용(낮은) 감지율을 적용한다.
        // 시야 + 소리가 동시면 아래 시각 rate가 우선한다(중복 합산 없음).
        if (!viaVision) return checkSound ? hearingDetectRate : 0f;

        // 거리(XZ 기준)
        float distance = FlatDistance(transform.position, playerTransform.position);

        // 거리 기반 감쇠 (선형 or 곡선적 감쇠 가능)
        float t = Mathf.Clamp01(distance / detectFalloffDistance);
        float rate = Mathf.Lerp(maxDetectRate, minDetectRate, t); // 가까울수록 maxDetectRate, 멀수록 minDetectRate

        // 시각 배수 적용
        rate *= visualBoost;

        if (playerState != null)
        {
            if (playerState.currentState == PlayerMove.PlayerState.Crawl) rate *= crawlRateMultiplier;
            if (playerState.currentState == PlayerMove.PlayerState.Run) rate *= runRateMultiplier;
        }

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

        if (dist <= catchDistance && lastLos == LosResult.Player)
        {
            OnCatched();
        }
    }

    private void OnCatched()
    {
        // ✅ 1회성 가드 — 매 프레임 재실행되어 previousPhase가 Retry로 오염되던 문제 차단
        if (caught) return;
        caught = true;

        Debug.Log($"[EnemySearch] 플레이어를 잡음! → Game Over");

        // 1) 플레이어 동작 정지
        if (playerState != null)
        {
            playerState.enabled = false;
            var cc = playerState.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
        }

        // 2) 적 동작 정지
        //    enabled = false만으로는 실행 중 코루틴(Chase의 Tracking)이 멈추지 않는다.
        //    반드시 StopAllBehaviors()로 코루틴과 agent를 함께 정지시킨 뒤 비활성화한다.
        var move = GetComponent<EnemyMove>();
        if (move != null)
        {
            move.StopAllBehaviors();
            move.enabled = false;
        }

        // 3) 게임 Phase 변경
        //    caught 가드는 인스턴스 단위이므로, 다른 적이 뒤이어 잡는 경우에도
        //    previousPhase가 Retry로 덮이지 않도록 한 번 더 확인한다.
        GameController gc = GameController.Instance;
        if (gc != null)
        {
            if (gc.gamePhase != GameController.GamePhase.Retry)
                gc.previousPhase = gc.gamePhase;
            gc.gamePhase = GameController.GamePhase.Retry;
        }
    }


    public void SetState(EnemyState newState)
    {
        // 상태 변경 불필요하거나 이미 죽은 경우 리턴
        if (_currentState == newState || _currentState == EnemyState.Died) 
            return;

        // 사망 상태 처리
        if (newState == EnemyState.Died)
        {
            // ✅ 상태를 먼저 확정한 뒤 이벤트를 발화한다 —
            //    수신 측(EnemyManager)이 currentState를 Died로 정확히 관측할 수 있어야 한다
            _currentState = newState;

            playerVisible = false;
            checkSound = false;

            OnEnemyDied?.Invoke(this);

            enabled = false;

            GetComponent<EnemyMove>()?.OnDeath();
            return; // 🔹 사망은 여기서 완전히 종료
        }

        // 일반 상태 변경
        _currentState = newState;
        OnStateChanged?.Invoke(this, _currentState);
    }

    // 기존 호출부(예: VisualDetection)와의 호환성을 유지한다.
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

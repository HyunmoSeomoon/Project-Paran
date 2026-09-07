using UnityEngine;

/// <summary>
/// 적 뒤쪽에 붙은 암살 트리거.
/// - OnTriggerEnter/Stay/Exit : inRange 갱신 + 프롬프트 UI 표시/숨김만 담당
/// - Update                   : 암살 키 입력 감지 + 암살 실행
/// 암살 발동 시 플레이어를 적 뒤쪽 고정 거리로 워프시켜 애니메이션 정렬을 보장한다.
/// </summary>
public class EnemyAssasinManager : MonoBehaviour
{
    // ── 인스펙터 직렬화 필드 (기존 이름 유지) ──
    [Header("참조")]
    [SerializeField] private GameObject AssassinUI;
    [SerializeField] private GameObject enemy;          // 트리거의 부모(암살 대상) 적 오브젝트
    [SerializeField] private Transform Camera;          // 프롬프트 UI 빌보드용 카메라
    [SerializeField] private EnemySearch enemyState;    // 대상 적의 EnemySearch (비우면 enemy에서 자동 취득)

    [Header("암살 설정")]
    [SerializeField] private KeyCode assassinKey = KeyCode.F;
    // 적 뒤쪽으로 플레이어를 붙일 거리 (트리거 위치 관계 기준 기본값)
    [SerializeField] private float assassinDistance = 1.2f;
    // true면 적의 y(NavMesh 지면 기준)를 사용, false면 플레이어의 현재 y 유지
    [SerializeField] private bool useEnemyGroundHeight = true;

    // ── 런타임 상태 ──
    private Animator enemyAnimator;
    private bool inRange;
    private bool assassinated;
    private PlayerMove playerMove;
    private CharacterController playerController;

    private void Start()
    {
        ShowPrompt(false);

        if (enemy == null)
        {
            Debug.LogWarning("[EnemyAssasinManager] enemy 참조가 비어 있습니다 → 암살 트리거 비활성화", this);
            gameObject.SetActive(false);
            return;
        }

        enemyAnimator = enemy.GetComponent<Animator>();

        if (enemyState == null)
            enemyState = enemy.GetComponent<EnemySearch>();

        if (enemyState == null)
            Debug.LogWarning("[EnemyAssasinManager] 대상 적에 EnemySearch가 없습니다.", this);
    }

    private void Update()
    {
        if (assassinated) return;
        if (!inRange) return;

        // 적이 추격/사망 상태가 되면 프롬프트를 내리고 암살 불가 처리 (UI 잔존 방지)
        if (!CanAssassinate())
        {
            ShowPrompt(false);
            return;
        }

        ShowPrompt(true);
        FacePromptToCamera();

        if (Input.GetKeyDown(assassinKey))
            ExecuteAssassination();
    }

    // ──────────────────────────────────────────────
    // 트리거 : 범위 판정 + 프롬프트 UI만 담당
    // ──────────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        TryRegisterPlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // Enter를 놓친 경우(오브젝트가 나중에 활성화되는 등)에도 범위를 복구한다
        TryRegisterPlayer(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        inRange = false;
        playerMove = null;
        playerController = null;
        ShowPrompt(false);
    }

    private void TryRegisterPlayer(Collider other)
    {
        if (assassinated) return;
        if (!other.CompareTag("Player")) return;

        if (playerMove == null)
        {
            playerMove = other.GetComponentInParent<PlayerMove>();
            playerController = playerMove != null
                ? playerMove.GetComponent<CharacterController>()
                : other.GetComponentInParent<CharacterController>();
        }

        inRange = true;
    }

    // ──────────────────────────────────────────────
    // 암살
    // ──────────────────────────────────────────────
    private bool CanAssassinate()
    {
        if (enemy == null || enemyState == null) return false;
        if (enemyState.currentState == EnemySearch.EnemyState.Chase) return false;
        if (enemyState.currentState == EnemySearch.EnemyState.Died) return false;
        return true;
    }

    private void ExecuteAssassination()
    {
        if (playerMove == null) return;

        assassinated = true;

        // 1) 플레이어를 적 뒤쪽 고정 거리로 워프 (거리·회전 정렬 보장)
        WarpPlayerBehindEnemy();

        // 2) 플레이어 암살(공격) 상태 진입 — 기존 방식 유지
        playerMove.currentState = PlayerMove.PlayerState.Attack;

        // 3) 적 암살 애니메이션 → 사망 처리
        if (enemyAnimator != null) enemyAnimator.SetTrigger("Assassin");
        enemyState.SetState(EnemySearch.EnemyState.Died);

        // 4) 프롬프트 정리 + 재발동 방지
        ShowPrompt(false);
        inRange = false;
        gameObject.SetActive(false);
    }

    private void WarpPlayerBehindEnemy()
    {
        Transform enemyTf = enemy.transform;
        Transform playerTf = playerMove.transform;

        // 적 forward를 수평으로 평탄화 (모델이 기울어져 있어도 지면 정렬 유지)
        Vector3 flatForward = enemyTf.forward;
        flatForward.y = 0f;
        flatForward = flatForward.sqrMagnitude > 1e-4f ? flatForward.normalized : enemyTf.forward;

        Vector3 targetPos = enemyTf.position - flatForward * assassinDistance;
        // NavMesh 지면 기준이므로 기본값은 적의 y를 사용한다
        targetPos.y = useEnemyGroundHeight ? enemyTf.position.y : playerTf.position.y;

        // ⚠ CharacterController가 켜진 상태로 transform.position을 대입하면
        //   다음 Move에서 되돌려진다 → 반드시 끄고 옮기고 다시 켠다.
        if (playerController != null) playerController.enabled = false;

        playerTf.position = targetPos;
        playerTf.rotation = Quaternion.LookRotation(flatForward, Vector3.up);

        if (playerController != null) playerController.enabled = true;
    }

    // ──────────────────────────────────────────────
    // 프롬프트 UI
    // ──────────────────────────────────────────────
    private void ShowPrompt(bool show)
    {
        if (AssassinUI == null) return;
        if (AssassinUI.activeSelf != show) AssassinUI.SetActive(show);
    }

    private void FacePromptToCamera()
    {
        if (AssassinUI == null || Camera == null) return;
        AssassinUI.transform.forward = Camera.forward;
    }
}

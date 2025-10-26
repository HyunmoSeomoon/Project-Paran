using UnityEngine;
using UnityEngine.AI;

public abstract class MovableAI : MonoBehaviour
{
    protected NavMeshAgent agent;
    [Header("이동 속도")]
    public float moveSpeed;
    public float rotSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        if (agent == null) Debug.Log("Agent가 없음");
        agent.speed = moveSpeed;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }

    protected virtual void MoveDefault()
    {
        if (agent.isStopped == true) agent.isStopped = false;
        if (agent.updateRotation == false) agent.updateRotation = true;
        // 이후 NPC, Enemy, Boss에 따라 기본 이동 세팅
    }

    protected virtual void Tracking(Vector3 target)
    {
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.ResetPath();
        agent.SetDestination(target);
    }

    protected virtual void RotToward(Vector3 targetPos)
    {
        agent.isStopped = true;
        agent.updateRotation = false;
        agent.ResetPath();

        Vector3 to = (targetPos - transform.position).normalized;
        to.y = 0f;
        if (to.sqrMagnitude < 1e-4f) return;

        Quaternion rotation = Quaternion.LookRotation(to);
        Quaternion rotValue = Quaternion.RotateTowards(transform.rotation, rotation, rotSpeed * Time.deltaTime);
        //Quaternion 대신 SingedAngle 사용 - 회전방향 직접 조정
        // float angle = Vector3.SignedAngle(transform.forward, to.normalized, Vector3.up);

        // // 각도의 절댓값이 작으면 회전 완료
        // if (Mathf.Abs(angle) < 0.5f)
        // {
        //     agent.updateRotation = true;
        //     agent.isStopped = false;
        //     return;
        // }

        //float step = Mathf.Sign(angle) * rotSpeed * Time.deltaTime;

        transform.rotation = rotValue;
    }
}

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
        //agent.ResetPath();
        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(target);
    }

    protected virtual void RotToward(Vector3 targetPos)
    {
        agent.isStopped = true;
        agent.updateRotation = false;

        Vector3 to = (targetPos - transform.position).normalized;
        to.y = 0f;
        if (to.sqrMagnitude < 1e-4f) return;

        Quaternion rotation = Quaternion.LookRotation(to);
        Quaternion rotValue = Quaternion.RotateTowards(transform.rotation, rotation, rotSpeed * Time.deltaTime);

        transform.rotation = rotValue;
    }
}

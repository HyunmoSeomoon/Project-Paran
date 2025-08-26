using UnityEditor;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public enum PlayerState
    {
        Stand,
        Crawl,
        Run
    }

    [Header("속도 설정")]
    [SerializeField] float standSpeed = 7f;
    [SerializeField] float crawlSpeed = 5f;
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float gravity = -9.8f;
    [SerializeField] float rotationSpeed = 20f;

    private CharacterController cc;
    private Vector3 velocity;
    
    [SerializeField] Animator animator;

    public PlayerState currentState = PlayerState.Stand;
    public float moveSpeed; // 현재 속도

    void Start()
    {
        cc = GetComponent<CharacterController>();
        moveSpeed = standSpeed;
    }

    void Update()
    {
        // 상태 전환 처리
        if (Input.GetKey(KeyCode.LeftControl))
        {
            currentState = PlayerState.Crawl;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            currentState = PlayerState.Run;
        }
        else
        {
            currentState = PlayerState.Stand;
        }

        // 상태에 따라 속도 설정
        switch (currentState)
        {
            case PlayerState.Stand:
                moveSpeed = standSpeed;
                break;
            case PlayerState.Crawl:
                moveSpeed = crawlSpeed;
                break;
            case PlayerState.Run:
                moveSpeed = runSpeed;
                break;
        }

        // 입력 처리
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(x, 0, z).normalized;

        if (inputDir != Vector3.zero)
        {
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = (camForward * z + camRight * x).normalized;

            if (currentState == PlayerState.Crawl)
            {
                float camY = Camera.main.transform.eulerAngles.y;
                Quaternion crawlRot = Quaternion.Euler(0, camY, 0);
                transform.rotation = crawlRot;
            }
            else
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);  
            }
            cc.Move(moveDir * moveSpeed * Time.deltaTime);
        }
        animator.SetFloat("Speed", new Vector3(x,0,z).magnitude);
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, -transform.up, out hit, 1))
            velocity.y += gravity * Time.deltaTime;
        else velocity.y = 0;
        cc.Move(velocity * Time.deltaTime);
    }
}

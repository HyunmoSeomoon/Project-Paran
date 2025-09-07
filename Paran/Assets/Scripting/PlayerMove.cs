using System;
using UnityEditor;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public enum PlayerState
    {
        Stand,
        Crawl,
        Walk,
        Run
    }

    [Header("속도 설정")]
    [SerializeField] float walkSpeed = 7f;
    [SerializeField] float crawlSpeed = 5f;
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float gravity = -9.8f;
    [SerializeField] float rotationSpeed = 20f;

    private CharacterController cc;
    private Vector3 velocity;
    
    [SerializeField] Animator animator;

    public PlayerState currentState = PlayerState.Stand;
    public float moveSpeed; // 현재 속도
    public float targetSpeed; // 가속 감속을 위한 목표 속도

    void Start()
    {
        cc = GetComponent<CharacterController>();
        moveSpeed = 0f;
    }

    void Update()
    {
        
        // 입력 처리
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(x, 0, z).normalized;

        // 상태 전환 처리
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (currentState != PlayerState.Crawl)
                currentState = PlayerState.Crawl;
            else currentState = PlayerState.Stand;
        }

        if (currentState != PlayerState.Crawl)
        {
            if (inputDir != Vector3.zero)
            {
                currentState = PlayerState.Walk;
                if (Input.GetKey(KeyCode.LeftShift))
                    currentState = PlayerState.Run;
            }
            else
            {
                currentState = PlayerState.Stand;
            }
        }
        else
        {
            if (inputDir != Vector3.zero && Input.GetKey(KeyCode.LeftShift))
                currentState = PlayerState.Run;
        }

        // 상태에 따라 속도 설정
        switch (currentState)
        {
            case PlayerState.Stand:
                targetSpeed = 0;
                animator.SetBool("Running", false);
                animator.SetBool("Crawling", false);
                break;
            case PlayerState.Walk:
                targetSpeed = walkSpeed;
                animator.SetBool("Running", false);
                animator.SetBool("Crawling", false);
                break;
            case PlayerState.Crawl:
                targetSpeed = crawlSpeed;
                animator.SetBool("Running", false);
                animator.SetBool("Crawling", true);
                break;
            case PlayerState.Run:
                targetSpeed = runSpeed;
                animator.SetBool("Running", true);
                animator.SetBool("Crawling", false);
                break;
        }
        
        if (Mathf.Abs(moveSpeed - targetSpeed) < 0.01f)
            moveSpeed = targetSpeed;
        else
            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, Time.deltaTime * 5f); // 현재속도를 초속 5의 가속도에 맞춰 목표 속도까지 증가

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

        bool isGrounded = cc.isGrounded;
        
        if (isGrounded && velocity.y < 0) velocity.y = 0;

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}

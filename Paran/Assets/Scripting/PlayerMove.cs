using UnityEditor;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float gravity = -9.8f;
    [SerializeField] float rotationSpeed = 3f;

    private CharacterController cc;
    private Vector3 velocity;
    [SerializeField] Animator animator;
    
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
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

            Vector3 forward = transform.forward;
            float angle = Vector3.SignedAngle(forward, moveDir, Vector3.up);
            if (Mathf.Abs(angle) <= 90f)
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

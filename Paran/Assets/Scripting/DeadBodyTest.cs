using UnityEngine;

public class DeadBodyTest : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform attachTransform;
    [SerializeField] private PlayerMove playerMove;
    private InteractableObject currentHeldObject = null;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Test");
            // 1. 현재 무언가를 들고 있다면, 내려놓는다.
            if (currentHeldObject != null)
            {
                currentHeldObject.Drop();
                currentHeldObject = null;
                //playerAnimator.SetBool("Carrying", false);
                playerMove.currentState = PlayerMove.PlayerState.Stand;
            }
            // 2. 아무것도 들고 있지 않다면, 주변을 탐색해서 집는다.
            else
            {
                PickupObject();
            }
        }
    }

    private void PickupObject()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2, interactableLayer);
        float closestDistance = 10f;
        InteractableObject targetObject = null;

        foreach (var col in colliders)
        {
            // 콜라이더에서 RagdollInteractable 스크립트를 찾아본다.
            if (col.TryGetComponent<InteractableObject>(out InteractableObject interactable))
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    targetObject = interactable;
                }
            }
        }
        Debug.Log(colliders.Length);

        if (targetObject != null)
        {
            // 해당 오브젝트를 집어들고, 현재 들고 있는 오브젝트로 기록
            targetObject.PickUp(attachTransform);
            currentHeldObject = targetObject;
            //playerAnimator.SetBool("Carrying", true);
            playerMove.currentState = PlayerMove.PlayerState.Carry;
        }
    }
}

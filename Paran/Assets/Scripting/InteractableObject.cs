using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Rigidbody hipsbody;
    [SerializeField] private Transform originParent;
    private FixedJoint attachedJoint;
    private Rigidbody[] allRigidbodies;
    void Start()
    {
        allRigidbodies = GetComponentsInChildren<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PickUp(Transform attachPoint)
    {
        foreach (Rigidbody rigidbody in allRigidbodies)
        {
            rigidbody.isKinematic = true;
        }
        hipsbody.transform.SetParent(attachPoint);
        hipsbody.transform.localPosition = Vector3.zero;
        hipsbody.transform.rotation = attachPoint.rotation;
    }

    public void Drop()
    {
        hipsbody.transform.SetParent(originParent);
        foreach (Rigidbody rigidbody in allRigidbodies)
        {
            rigidbody.isKinematic = false;
        }
    }
}

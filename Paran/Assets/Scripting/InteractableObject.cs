using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Rigidbody hipsbody;
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

    public virtual void PickUp(Transform attachPoint)
    {

    }

    public virtual void Drop()
    {

    }
}

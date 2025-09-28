using UnityEngine;

public class EnemyRagDollManager : MonoBehaviour
{
    public Rigidbody[] rigidbodies;

    public void ControlKinematic(bool b)
    {
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = b;
        }
    }
}

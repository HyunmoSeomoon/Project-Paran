using UnityEngine;

public class EnemyAssasinManager : MonoBehaviour
{
    [SerializeField] private Collider deathCollider;
    [SerializeField] private GameObject AssassinUI;
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private Transform Camera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AssassinUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AssassinUI.SetActive(true);
            AssassinUI.transform.forward = Camera.forward;
            if (Input.GetKeyDown(KeyCode.F))
            {
                other.transform.position = transform.position;
                other.transform.rotation = transform.rotation;
                other.gameObject.GetComponent<PlayerMove>().currentState = PlayerMove.PlayerState.Attack;
                enemyAnimator.SetTrigger("Assassin");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            AssassinUI.SetActive(false);
    }
}

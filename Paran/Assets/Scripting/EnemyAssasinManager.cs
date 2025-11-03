using UnityEngine;

public class EnemyAssasinManager : MonoBehaviour
{
    [SerializeField] private Collider deathCollider;
    [SerializeField] private GameObject AssassinUI;
    [SerializeField] private GameObject enemy;
    private Animator enemyAnimator;
    private EnemySearch enemySearch;
    private bool inRange;
    private GameObject playerObject;
    [SerializeField] private Transform Camera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AssassinUI.SetActive(false);
        if (enemy == null)
        {
            Debug.Log("enemy assassin code error");
            gameObject.SetActive(false);
            return;
        }
        enemyAnimator = enemy.GetComponent<Animator>();
        enemySearch = enemy.GetComponent<EnemySearch>();
    }

    void Update()
    {
        if (inRange)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                playerObject.GetComponent<CharacterController>().enabled = false;
                playerObject.transform.position = transform.position;
                playerObject.transform.rotation = transform.rotation;
                enemySearch.SetState(EnemySearch.EnemyState.Died);
                playerObject.GetComponent<PlayerMove>().currentState = PlayerMove.PlayerState.Attack;
                enemyAnimator.SetTrigger("Assassin");
                playerObject.GetComponent<CharacterController>().enabled = true;
                AssassinUI.SetActive(false);
                gameObject.SetActive(false);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AssassinUI.SetActive(true);
            AssassinUI.transform.forward = Camera.forward;
            inRange = true;
            playerObject = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AssassinUI.SetActive(false);
            inRange = false;
        }
            
    }
}

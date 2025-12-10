using System;
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
    [SerializeField] private EnemySearch enemyState;
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

        if (enemyState == null)
        enemyState = enemy.GetComponent<EnemySearch>();
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
                playerObject.GetComponent<PlayerMove>().currentState = PlayerMove.PlayerState.Attack;
                enemyAnimator.SetTrigger("Assassin");
                playerObject.GetComponent<CharacterController>().enabled = true;
                AssassinUI.SetActive(false);
                gameObject.SetActive(false);
                enemySearch.SetState(EnemySearch.EnemyState.Died);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (enemyState.GetState() == EnemySearch.EnemyState.Chase || enemyState.GetState() == EnemySearch.EnemyState.Died) return;
        if (other.CompareTag("Player"))
        {
            AssassinUI.SetActive(true);
            if(Camera!=null)
                AssassinUI.transform.forward = Camera.forward;
            if (Input.GetKeyDown(KeyCode.F))
            {
                other.transform.position = transform.position;
                other.transform.rotation = transform.rotation;
                enemyState.SetState(EnemySearch.EnemyState.Died);
                other.gameObject.GetComponent<PlayerMove>().currentState = PlayerMove.PlayerState.Attack;
                enemyAnimator.SetTrigger("Assassin");
            }
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

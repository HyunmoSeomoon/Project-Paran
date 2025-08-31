using UnityEngine;
using System.Collections.Generic;

public class AssasinManager : MonoBehaviour
{
    private List<GameObject> enemiesInRange = new List<GameObject>();

    [SerializeField] private Transform playerTransform;
    [SerializeField] private float alignDistance = 1.5f; // 암살 애니메이션 정렬 거리
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (enemiesInRange.Count == 1)
            {
                GameObject enemy = enemiesInRange[0];

                // 거리 정렬 및 위치 맞춤
                Vector3 directionToEnemy = (enemy.transform.position - playerTransform.position).normalized;
                playerTransform.position = enemy.transform.position - directionToEnemy * alignDistance + positionOffset;
                playerTransform.LookAt(enemy.transform); // 정렬

                // 상태 변경
                EnemySearch stateHandler = enemy.GetComponent<EnemySearch>();
                if (stateHandler != null)
                {
                    stateHandler.SetState(EnemySearch.EnemyState.Died);
                }
                else
                {
                    Debug.LogWarning("EnemySearch 스크립트가 없습니다.");
                }
            }
            else if (enemiesInRange.Count > 1)
            {
                Debug.LogError("암살 대상이 둘 이상입니다!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !enemiesInRange.Contains(other.gameObject))
        {
            enemiesInRange.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy") && enemiesInRange.Contains(other.gameObject))
        {
            enemiesInRange.Remove(other.gameObject);
        }
    }
}

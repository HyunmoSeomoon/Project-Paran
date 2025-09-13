using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

public class PlayerInteract : MonoBehaviour
{

    [SerializeField] private Transform playerTransform;
    [SerializeField] private float alignDistance = 0.5f; // 암살 애니메이션 정렬 거리
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Interaction();
        }
    }

    void Interaction()
    {
        Vector3 sphereCenter = transform.position + transform.forward * alignDistance;
        Collider[] hits = Physics.OverlapSphere(sphereCenter, alignDistance);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log("1단계 통과");
                EnemySearch stateHandler = hit.GetComponent<EnemySearch>();
                if (stateHandler == null) return;

                // 후면 체크
                Vector3 toEnemy = (hit.transform.position - transform.position).normalized;
                float dot = Vector3.Dot(hit.transform.forward, toEnemy);

                Debug.Log("Dot value: " + dot);
                Debug.Log("Enemy State: " + stateHandler.GetState());

                if (dot > 0.5f && stateHandler.GetState() != EnemySearch.EnemyState.Chase)
                {
                    Debug.Log("2단계 통과");
                    Vector3 directionToEnemy = (hit.transform.position - transform.position).normalized;
                    Vector3 newPosition = hit.transform.position - directionToEnemy * alignDistance + positionOffset;
                    newPosition.y = playerTransform.position.y; // Y값 고정
                    playerTransform.position = newPosition;

                    Vector3 lookDirection = new Vector3(hit.transform.position.x, playerTransform.position.y, hit.transform.position.z);
                    playerTransform.LookAt(lookDirection);

                    //애니메이션 여기 추가
                    stateHandler.SetState(EnemySearch.EnemyState.Died);
                    break; // 하나만 암살
                }
            }
            else if (hit.CompareTag("InteractableObject"))
            {

            }
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 sphereCenter = transform.position + transform.forward * alignDistance;
        Gizmos.DrawWireSphere(sphereCenter, alignDistance);
    }
}

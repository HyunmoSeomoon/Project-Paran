using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VisualDetection : MonoBehaviour
{
    [SerializeField] private EnemySearch enemySearch; 
    [SerializeField] private Slider detectionSlider;
    [SerializeField] private Image firstImage;

    private EnemySearch.EnemyState lastState;

    private void Start()
    {
        if (firstImage != null)
            firstImage.gameObject.SetActive(false); 

        if (enemySearch != null)
            lastState = enemySearch.currentState;
    }

    private void Update()
    {
        if (enemySearch == null || detectionSlider == null || firstImage == null) return;

        // 죽었거나 Idle 상태면 UI 전체 비활성화
        if (enemySearch.GetState() == EnemySearch.EnemyState.Died || enemySearch.GetState() == EnemySearch.EnemyState.Idle)
        {
            if (detectionSlider.gameObject.activeSelf) detectionSlider.gameObject.SetActive(false);
            if (firstImage.gameObject.activeSelf) firstImage.gameObject.SetActive(false);
            return;
        }

        // 나머지 상태(Warning, Chase)에서는 다시 UI 활성화
        if (!detectionSlider.gameObject.activeSelf) detectionSlider.gameObject.SetActive(true);

        // 감지 비율 슬라이더 업데이트
        float ratio = enemySearch.PlayerDetectRatio();
        detectionSlider.value = ratio;

        // 상태 전환 감지 (Warning -> Chase)
        if (lastState != enemySearch.currentState)
        {
            if (enemySearch.currentState == EnemySearch.EnemyState.Chase)
            {
                StartCoroutine(ShowImageTemporarily());
            }

            lastState = enemySearch.currentState;
        }
    }

    private IEnumerator ShowImageTemporarily()
    {
        firstImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        firstImage.gameObject.SetActive(false);
    }
}
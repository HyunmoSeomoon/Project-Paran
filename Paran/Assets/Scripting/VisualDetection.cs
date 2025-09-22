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
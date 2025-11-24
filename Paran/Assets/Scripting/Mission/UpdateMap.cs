using UnityEngine;

public class UpdateMap : MonoBehaviour
{
    public GameObject[] targetPoint;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach(GameObject go in targetPoint)
        {
            go.SetActive(false);
        }
    }

    public void UpdateTargetPoint(int i)
    {
        if(i>=targetPoint.Length || i<0) return;
        targetPoint[i].SetActive(true);
    }

    public void DestroyTargetPoint(int i)
    {
        if(i>=targetPoint.Length || i<0) return;
        targetPoint[i].SetActive(false);
    }
}

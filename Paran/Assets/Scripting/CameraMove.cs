using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform player;
    
    [SerializeField] float yaw = 0f, pitch = 20f;
    [SerializeField] float sensitivityX = 2f, sensitivityY = 2f;
    [SerializeField] float pitchMin = -30f, pitchMax = 60f;

    [SerializeField] float distance = 5f;
    [SerializeField] float minDistance = 2f, maxDistance = 10f;
    [SerializeField] float zoomSpeed = 2f;

    [SerializeField] float collisionBuffer = 0.9f;

    void Update()
    {
        //회전 마우스로
        yaw += Input.GetAxis("Mouse X") * sensitivityX;
        pitch -= Input.GetAxis("Mouse Y") * sensitivityY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        // 줌기능...
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void LateUpdate()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetOffset = rotation * new Vector3(0, 0, -distance);
        Vector3 targetPos = player.position + targetOffset;

        // 장애물 충돌 체크
        Vector3 origin = player.position + Vector3.up * 1.5f;
        Vector3 dir = targetPos - origin;
        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dir.magnitude, LayerMask.GetMask("Wall")))
        {
            targetPos = origin + dir.normalized * (hit.distance * collisionBuffer);
        }

        // 카메라 위치 설정
        transform.position = targetPos;

        // 카메라 방향 설정
        transform.LookAt(origin);
    }
}
using System;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform player;
    private PlayerMove playerMove;

    [SerializeField] float yaw = 0f, pitch = 20f;
    [SerializeField] float sensitivityX = 2f, sensitivityY = 2f;
    [SerializeField] float pitchMin = -30f, pitchMax = 60f;

    [SerializeField] float distance = 5f;
    [SerializeField] float minDistance = 2f, maxDistance = 10f;
    [SerializeField] float zoomSpeed = 2f;
    [SerializeField] private float shoulderOffset = 0.5f; // 오버 더 숄더 오프셋(양수 = 카메라를 오른쪽으로 이동, 화면상 플레이어는 좌측에 위치)
    private float currentCamHeight; 
    [SerializeField] private float standHeight = 1.5f;
    [SerializeField] private float crawlHeight = 0.9f;
    [SerializeField] private float heightLerpSpeed = 20f;
    [SerializeField] float collisionBuffer = 0.9f;
    public bool isLocked = false;
    private int wallMask;

    void Start()
    {
        playerMove = player.GetComponent<PlayerMove>();
        currentCamHeight = standHeight;
        wallMask = LayerMask.GetMask("Wall");
    }

    void Update()
    {
        if (isLocked) return;
        yaw += Input.GetAxis("Mouse X") * sensitivityX;
        pitch -= Input.GetAxis("Mouse Y") * sensitivityY;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            distance -= scroll * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void LateUpdate()
    {
        if (isLocked) return;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        Vector3 targetPos = player.position + offset;

        // Crawl 시 카메라 높이
        float targetHeight = (playerMove.currentState == PlayerMove.PlayerState.Crawl)
            ? crawlHeight
            : standHeight;

        currentCamHeight = Mathf.Lerp(currentCamHeight, targetHeight, Time.deltaTime * heightLerpSpeed);

        Vector3 lookOrigin = player.position + Vector3.up * currentCamHeight;

        // 오버 더 숄더 오프셋: 카메라 yaw 기준 수평 right 방향으로 카메라 위치와 시선 목표(lookOrigin)를
        // 동일하게 평행이동. 같은 벡터를 양쪽에 더하므로 시선 방향(targetPos - lookOrigin)은 그대로 유지되고,
        // 카메라만 옆으로 이동하여 화면상 플레이어가 반대쪽(좌측)으로 치우쳐 보이게 된다.
        Vector3 shoulderRight = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
        Vector3 shoulderVector = shoulderRight * shoulderOffset;
        targetPos += shoulderVector;
        lookOrigin += shoulderVector;

        // 충돌 처리
        Vector3 origin = lookOrigin;
        Vector3 dir = targetPos - origin;
        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dir.magnitude, wallMask))
        {
            targetPos = origin + dir.normalized * (hit.distance * collisionBuffer);
        }

        transform.position = targetPos;
        transform.LookAt(lookOrigin);
    }
}

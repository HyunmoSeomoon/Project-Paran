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
    private float currentCamHeight; 
    [SerializeField] private float standHeight = 1.5f;
    [SerializeField] private float crawlHeight = 0.9f;
    [SerializeField] private float heightLerpSpeed = 20f;
    [SerializeField] float collisionBuffer = 0.9f;
    public bool isLocked = false;

    void Start()
    {
        playerMove = player.GetComponent<PlayerMove>();
        currentCamHeight = standHeight;
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

        // 충돌 처리
        Vector3 origin = lookOrigin;
        Vector3 dir = targetPos - origin;
        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dir.magnitude, LayerMask.GetMask("Wall")))
        {
            targetPos = origin + dir.normalized * (hit.distance * collisionBuffer);
        }

        transform.position = targetPos;
        transform.LookAt(lookOrigin);
    }
}

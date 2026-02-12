using UnityEngine;

public class FollowHead : MonoBehaviour
{
    public Transform targetCamera;     // 一般是 Main Camera
    public float distance = 0.6f;      // 菜单离眼睛多远（米）
    public float heightOffset = -0.1f; // 低一点更舒服
    public float followSpeed = 6f;     // 跟随平滑
    public bool yawOnly = true;        // 只跟随水平旋转，避免上下抖

    void Start()
    {
        if (targetCamera == null && Camera.main != null)
            targetCamera = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 forward = targetCamera.forward;
        if (yawOnly)
        {
            forward.y = 0f;
            forward.Normalize();
        }

        Vector3 targetPos = targetCamera.position + forward * distance;
        targetPos.y += heightOffset;

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

        // 面向相机（只绕Y轴）
        Vector3 lookDir = transform.position - targetCamera.position;
        if (yawOnly) lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * followSpeed);
        }
    }
}

using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class BindCanvasEventCamera : MonoBehaviour
{
    private void OnEnable()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas.renderMode != RenderMode.WorldSpace) return;

        // 1) 优先用 MainCamera tag
        Camera cam = Camera.main;

        // 2) 找不到就找场景里任意启用的 Camera
        if (cam == null)
        {
            cam = FindObjectOfType<Camera>();
        }

        if (cam != null)
        {
            canvas.worldCamera = cam;
            // Debug.Log($"[BindCanvasEventCamera] Bound to {cam.name}");
        }
        else
        {
            Debug.LogWarning("[BindCanvasEventCamera] No camera found to bind.");
        }
    }
}

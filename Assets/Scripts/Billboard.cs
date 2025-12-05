using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Tooltip("Leave empty to use Camera.main")]
    public Camera targetCamera;

    [Tooltip("If true, only rotate around Y (good for health bars above enemies).")]
    public bool lockY = true;

    private void Start()
    {
        // If no camera assigned, use main camera
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        // Direction from this object to the camera
        Vector3 dir = targetCamera.transform.position - transform.position;

        if (lockY)
        {
            dir.y = 0f; // ignore vertical difference
        }

        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(-dir); // face *towards* camera
            transform.rotation = lookRot;
        }
    }
}

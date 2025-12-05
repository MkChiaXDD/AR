using UnityEngine;
using UnityEngine.UI;

public class Waypoint : MonoBehaviour
{
    public Image img;          // icon on UI
    public Transform target;   // balloon to follow
    public Camera arCamera;    // assign in code or use Camera.main
    public Vector2 screenOffset = new Vector2(0, 2f);

    private void Awake()
    {
        if (img == null)
            img = GetComponent<Image>();
    }

    private void Start()
    {
        if (arCamera == null)
            arCamera = Camera.main;
    }

    private void Update()
    {
        if (target == null || img == null || arCamera == null)
        {
            // balloon died or camera missing -> remove waypoint
            Destroy(gameObject);
            return;
        }

        float minX = img.GetPixelAdjustedRect().width / 2;
        float maxX = Screen.width - minX;

        float minY = img.GetPixelAdjustedRect().height / 2;
        float maxY = Screen.height - minY;

        // world -> screen
        Vector3 screenPos = arCamera.WorldToScreenPoint(target.position);

        // if target is behind camera, flip position horizontally
        if (Vector3.Dot((target.position - arCamera.transform.position), arCamera.transform.forward) < 0)
        {
            if (screenPos.x < Screen.width / 2f)
                screenPos.x = maxX;
            else
                screenPos.x = minX;
        }

        screenPos.x = Mathf.Clamp(screenPos.x, minX, maxX);
        screenPos.y = Mathf.Clamp(screenPos.y, minY, maxY);

        Vector2 finalPos = new Vector2(screenPos.x, screenPos.y) + screenOffset;

        img.transform.position = finalPos;
    }
}

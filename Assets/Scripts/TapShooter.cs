using UnityEngine;
using TMPro;

public class TapShooter : MonoBehaviour
{
    public TMP_Text tapCountText;
    public Camera arCamera;
    private int tapCount;
    private void Start()
    {
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
    }
    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        tapCount++;
        if (tapCountText != null)
            tapCountText.text = "Tap Count: " + tapCount;

        Ray ray = arCamera.ScreenPointToRay(touch.position);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Balloon balloon = hit.collider.GetComponent<Balloon>();
            if (balloon != null)
            {
                balloon.OnHit();
            }
        }
        else
        {
            Debug.Log("Raycast hit nothing");
        }
    }
}

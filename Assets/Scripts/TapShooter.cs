using UnityEngine;
using TMPro;

public class TapShooter : MonoBehaviour
{
    public TMP_Text tapCountText;
    private int tapCount;
    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        tapCount++;
        tapCountText.text = "Tap Count: " + tapCount;

        Ray ray = Camera.main.ScreenPointToRay(touch.position);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Balloon balloon = hit.collider.GetComponent<Balloon>();
            if (balloon != null)
            {
                // Tell GameManager a balloon was popped
                GameManager.Instance.OnBalloonPopped();

                Destroy(balloon.gameObject);
            }
        }
    }
}

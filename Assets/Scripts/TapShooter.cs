using UnityEngine;

public class TapShooter : MonoBehaviour
{
    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

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

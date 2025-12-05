using UnityEngine;
using TMPro;

public class TapShooter : MonoBehaviour
{
    public TMP_Text tapCountText;
    public Camera arCamera;
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;

    private int tapCount;

    private void Start()
    {
        if (arCamera == null)
            arCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        tapCount++;
        if (tapCountText != null)
            tapCountText.text = "Tap Count: " + tapCount;

        ShootBullet();
    }

    private void ShootBullet()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("No bullet prefab assigned!");
            return;
        }

        // spawn bullet at camera position & rotation
        GameObject bullet = Instantiate(
            bulletPrefab,
            arCamera.transform.position,
            arCamera.transform.rotation
        );

        // add forward velocity
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = arCamera.transform.forward * bulletSpeed;
        }
    }
}

using UnityEngine;
using TMPro;

public class TapShooter : MonoBehaviour
{
    public TMP_Text tapCountText;
    public Camera arCamera;
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;

    [Header("Fire Settings")]
    public float fireCooldownMax = 0.5f; // time between shots
    private float fireCooldown = 0f;

    private int tapCount;

    private void Start()
    {
        if (arCamera == null)
            arCamera = Camera.main;
    }

    void Update()
    {
        // decrease cooldown timer
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        // if still cooling down, DO NOT shoot
        if (fireCooldown > 0f)
            return;

        // reset cooldown
        fireCooldown = fireCooldownMax;

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

        // spawn bullet at camera
        GameObject bullet = Instantiate(
            bulletPrefab,
            arCamera.transform.position,
            arCamera.transform.rotation
        );

        // push bullet forward
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = arCamera.transform.forward * bulletSpeed;
        }
    }
}
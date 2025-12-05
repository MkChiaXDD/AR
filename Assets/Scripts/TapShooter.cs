using UnityEngine;
using TMPro;
using Unity.IO.LowLevel.Unsafe;

public class TapShooter : MonoBehaviour
{
    public TMP_Text tapCountText;
    public Camera arCamera;
    public GameObject bulletPrefab;
    public float bulletSpeed = 5f;

    [Header("Fire Settings")]
    public int fireRate = 1;      // bullets per second
    private float fireCooldown = 0f;
    public TMP_Text fireRateText;
    public int fireRateUpgradeCost = 5;

    private int tapCount;
    public GameManager gameMgr;

    private void Start()
    {
        if (arCamera == null)
            arCamera = Camera.main;

        if (fireRate < 1)
            fireRate = 1; // safety
    }

    void Update()
    {
        // countdown cooldown timer
        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        // if no touches, don't shoot
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        // we only care if the finger is held OR just touched
        bool isHolding =
            touch.phase == TouchPhase.Began ||
            touch.phase == TouchPhase.Stationary ||
            touch.phase == TouchPhase.Moved;

        if (!isHolding)
            return;

        // still cooling down ? cannot shoot yet
        if (fireCooldown > 0f)
            return;

        // reset cooldown BEFORE shooting
        fireCooldown = 1f / fireRate;

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

        GameObject bullet = Instantiate(
            bulletPrefab,
            arCamera.transform.position,
            arCamera.transform.rotation
        );

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = arCamera.transform.forward * bulletSpeed;
        }
    }

    public void UpgradeFireRate()
    {
        if (gameMgr.GetGoldAmount() < fireRateUpgradeCost) return;

        fireRate++;
        gameMgr.UseGold(fireRateUpgradeCost);
        UpdateFireRateText(fireRate);
    }

    private void UpdateFireRateText(int fireRate)
    {
        fireRateText.text = "Firerate: " + fireRate;
    }
}

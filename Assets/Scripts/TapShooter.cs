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
    public int fireRate = 1;
    private float fireCooldown = 0f;
    public TMP_Text fireRateText;
    public int fireRateUpgradeCost = 5;

    private int tapCount;
    public GameManager gameMgr;
    public LayerMask coinLayerMask;

    private void Start()
    {
        if (arCamera == null)
            arCamera = Camera.main;

        if (fireRate < 1)
            fireRate = 1; // safety
    }

    void Update()
    {
        if (gameMgr.gameEnd == true) return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (TryCollectCoin(Input.GetTouch(0).position))
                return;
        }

        if (fireCooldown > 0f)
            fireCooldown -= Time.deltaTime;

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);

        bool isHolding =
            touch.phase == TouchPhase.Began ||
            touch.phase == TouchPhase.Stationary ||
            touch.phase == TouchPhase.Moved;

        if (!isHolding)
            return;

        if (fireCooldown > 0f)
            return;

        fireCooldown = 1f / fireRate;

        tapCount++;
        if (tapCountText != null)
            tapCountText.text = "Tap Count: " + tapCount;

        ShootBullet();
    }


    private void ShootBullet()
    {
        if (BulletPool.Instance == null)
        {
            Debug.LogWarning("No BulletPool in scene!");
            return;
        }

        AudioManager.Instance.PlaySFX("DartShoot");

        Vector3 spawnPos = arCamera.transform.position;
        Quaternion spawnRot = arCamera.transform.rotation;

        GameObject bullet = BulletPool.Instance.GetBullet(spawnPos, spawnRot);

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

    private bool TryCollectCoin(Vector2 screenPos)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f, coinLayerMask))
        {
            gameMgr.AddGold(1);
            Destroy(hit.collider.gameObject);
            AudioManager.Instance.PlaySFX("CoinCollect", 0.1f);
            return true;
        }

        return false;
    }



}

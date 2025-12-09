using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Targeting")]
    public string enemyTag = "Enemy";
    public float range = 3f;
    public float rotationSpeed = 10f;
    public Transform headToRotate;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 1f;
    public float bulletSpeed = 6f;

    private float fireCooldown = 0f;

    private void Awake()
    {
        if (headToRotate == null)
            headToRotate = transform;
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        Transform target = GetClosestTarget();

        if (target != null)
        {
            AimAt(target);

            if (fireCooldown <= 0f)
            {
                Shoot(target);
                fireCooldown = 1f / fireRate;
            }
        }
    }

    private Transform GetClosestTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        Transform closest = null;
        float closestDistSqr = Mathf.Infinity;

        Vector3 myPos = transform.position;

        foreach (GameObject e in enemies)
        {
            if (e == null) continue;

            float distSqr = (e.transform.position - myPos).sqrMagnitude;
            if (distSqr < closestDistSqr && distSqr <= range * range)
            {
                closestDistSqr = distSqr;
                closest = e.transform;
            }
        }

        return closest;
    }

    private void AimAt(Transform target)
    {
        Vector3 dir = (target.position - headToRotate.position).normalized;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion lookRot = Quaternion.LookRotation(dir);
        headToRotate.rotation = Quaternion.Lerp(
            headToRotate.rotation,
            lookRot,
            rotationSpeed * Time.deltaTime
        );
    }

    private void Shoot(Transform target)
    {
        if (BulletPool.Instance == null || firePoint == null)
        {
            Debug.LogWarning("No BulletPool or firePoint missing!");
            return;
        }

        AudioManager.Instance.PlaySFX("TurretShoot", 0.5f);

        Vector3 dir = (target.position - firePoint.position).normalized;

        GameObject bulletGO = BulletPool.Instance.GetBullet(
            firePoint.position,
            Quaternion.LookRotation(dir)
        );

        Rigidbody rb = bulletGO.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = dir * bulletSpeed;
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

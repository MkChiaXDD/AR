using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Targeting")]
    public string enemyTag = "Enemy";   // balloons must use this tag
    public float range = 3f;            // how far the turret can see
    public float rotationSpeed = 10f;   // how fast it turns to face target
    public Transform headToRotate;      // optional: part that rotates (can be same as this.transform)

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;         // where bullets spawn
    public float fireRate = 1f;         // shots per second
    public float bulletSpeed = 6f;      // bullet velocity

    private float fireCooldown = 0f;

    private void Awake()
    {
        if (headToRotate == null)
            headToRotate = transform;
    }

    private void Update()
    {
        fireCooldown -= Time.deltaTime;

        // Find closest target in range
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
        dir.y = 0f; // only rotate on Y so it doesn't tilt weirdly

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
        if (bulletPrefab == null || firePoint == null) return;

        // direction to target
        Vector3 dir = (target.position - firePoint.position).normalized;

        // spawn bullet
        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(dir));

        // apply velocity
        Rigidbody rb = bulletGO.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = dir * bulletSpeed;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // just to see range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}

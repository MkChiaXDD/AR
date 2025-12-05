using UnityEngine;

public class Balloon : MonoBehaviour
{
    private Transform target;
    private float speed;

    // Called by spawner AFTER instantiating the balloon
    public void Init(Transform baseTransform, float speed)
    {
        target = baseTransform;
        this.speed = speed;
    }

    void Update()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            // balloon reached base
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnBalloonLeak();
            }

            Destroy(gameObject);
        }
    }
}

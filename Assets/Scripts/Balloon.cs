using UnityEngine;

public class Balloon : MonoBehaviour
{
    private Transform target;
    public float speed = 0.1f;

    // Called by spawner AFTER instantiating the balloon
    public void Init(Transform baseTransform)
    {
        target = baseTransform;
    }

    void Update()
    {
        if (target == null) return;

        Vector3 dir = (target.position - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            // here you call your game manager to subtract lives etc.
            // GameManager.Instance.OnBalloonLeak();

            Destroy(gameObject);
        }
    }
}

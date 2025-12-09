using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float autoDestroyTimer = 3f;
    private float lifeTimer;

    private void OnEnable()
    {
        lifeTimer = autoDestroyTimer;
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f)
        {
            if (BulletPool.Instance != null)
                BulletPool.Instance.ReturnBullet(gameObject);
            else
                gameObject.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Balloon balloon = collision.gameObject.GetComponent<Balloon>();
            if (balloon != null)
            {
                balloon.OnHit(1);
            }
        }

        if (BulletPool.Instance != null)
            BulletPool.Instance.ReturnBullet(gameObject);
        else
            gameObject.SetActive(false);
    }
}

using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float autoDestroyTimer;
    private void Start()
    {
        Destroy(gameObject, autoDestroyTimer);
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
            Destroy(gameObject);
        }
    }
}

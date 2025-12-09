using UnityEngine;
using UnityEngine.UI;

public class Balloon : MonoBehaviour
{
    private Transform target;
    private float speed;
    private int maxHp;
    private int currHp;
    public Image hpImage;
    public int gold = 1;

    public void Init(Transform baseTransform, float speed, int health)
    {
        target = baseTransform;
        this.speed = speed;
        maxHp = health;
        currHp = health;

        UpdateUI();
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

    public void OnHit(int damage)
    {
        currHp -= damage;
        UpdateUI();

        if (currHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        ParticlePool.Instance.GetParticle(transform.position, Quaternion.identity);
        GameManager.Instance.OnBalloonPopped(gold);
        AudioManager.Instance.PlaySFX("BalloonPop");
        Destroy(gameObject);
    }

    private void UpdateUI()
    {
        if (hpImage != null)
        {
            hpImage.fillAmount = (float)currHp / maxHp;
        }
    }
}

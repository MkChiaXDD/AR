using UnityEngine;

public class ParticleAutoReturn : MonoBehaviour
{
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        ps.Clear();
        ps.Play();
        StartCoroutine(ReturnWhenDone());
    }

    private System.Collections.IEnumerator ReturnWhenDone()
    {
        yield return new WaitUntil(() => !ps.IsAlive(true));

        ParticlePool.Instance.ReturnParticle(gameObject);
    }
}

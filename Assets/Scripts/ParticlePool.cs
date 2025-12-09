using System.Collections.Generic;
using UnityEngine;

public class ParticlePool : MonoBehaviour
{
    public static ParticlePool Instance { get; private set; }

    public GameObject particlePrefab;
    public int initialPoolSize = 20;

    private readonly List<GameObject> pool = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject p = Instantiate(particlePrefab, transform);
            p.SetActive(false);
            pool.Add(p);
        }
    }

    public GameObject GetParticle(Vector3 position, Quaternion rotation)
    {
        foreach (GameObject p in pool)
        {
            if (!p.activeInHierarchy)
            {
                p.transform.SetPositionAndRotation(position, rotation);
                p.SetActive(true);

                var ps = p.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    ps.Clear();
                    ps.Play();
                }

                return p;
            }
        }

        GameObject newP = Instantiate(particlePrefab, position, rotation, transform);
        pool.Add(newP);

        var newPs = newP.GetComponent<ParticleSystem>();
        if (newPs != null)
        {
            newPs.Clear();
            newPs.Play();
        }

        return newP;
    }

    public void ReturnParticle(GameObject particle)
    {
        particle.SetActive(false);
    }
}

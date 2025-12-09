using UnityEngine;

public class Farm : MonoBehaviour
{
    [Header("Coin Settings")]
    public GameObject coinPrefab;
    public float timeBetweenDrops = 3f;
    public int coinsPerDrop = 1;

    [Header("Drop Offset")]
    public float dropRadius = 0.1f;

    private float timer;
    private float baseY; // store farm Y height

    private void Start()
    {
        baseY = transform.position.y; // save Y level once
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= timeBetweenDrops)
        {
            DropCoins();
            timer = 0f;
        }
    }

    void DropCoins()
    {
        for (int i = 0; i < coinsPerDrop; i++)
        {
            Vector2 offset = Random.insideUnitCircle * dropRadius;

            Vector3 spawnPos = new Vector3(
                transform.position.x + offset.x,
                baseY,                                   // ALWAYS same Y height
                transform.position.z + offset.y
            );

            Instantiate(coinPrefab, spawnPos, Quaternion.identity);

            AudioManager.Instance.PlaySFX("DropCoin");
        }
    }
}

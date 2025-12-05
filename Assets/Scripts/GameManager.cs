using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Transform DefenseBase { get; private set; }

    public GameObject BalloonPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // If you want this to persist across scenes, uncomment:
        // DontDestroyOnLoad(gameObject);
    }

    // Called by ObjectSpawner AFTER the base is spawned
    public void SetDefenseBase(Transform baseTransform)
    {
        DefenseBase = baseTransform;
        // Here you can also trigger: enable UI, allow waves, etc.
        Debug.Log("Defense base set: " + baseTransform.name);
        StartGame();
    }

    public bool HasDefenseBase()
    {
        return DefenseBase != null;
    }

    private void StartGame()
    {
        var newBalloon = Instantiate(BalloonPrefab);
        newBalloon.GetComponent<Balloon>().Init(DefenseBase);
    }
}

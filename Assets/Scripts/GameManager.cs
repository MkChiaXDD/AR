using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

[System.Serializable]
public class Round
{
    public int round;

    [Header("Normal Balloons")]
    public int normalBalloonCount;
    public float normalBalloonSpeed;

    [Header("Special Balloons")]
    public int specialBalloonCount;   // set > 0 from round 3 onwards
    public float specialBalloonSpeed;
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Transform DefenseBase { get; private set; }

    [Header("Pre-Game")]
    public GameObject introPanel;
    public ARPlaneManager planeManager;
    public PlacementMarker placementMarker;
    public ObjectSpawner objectSpawner;

    [Header("UI")]
    public GameObject placeBaseText;
    public GameObject tapToShootText;
    public GameObject winText;
    public GameObject loseText;
    public GameObject restartBtn;
    public GameObject placeTurretText;

    [Header("Balloon Settings")]
    public GameObject BalloonPrefab;          // normal
    public GameObject specialBalloonPrefab;   // special type
    public float spawnRadius = 0.7f;
    public float timeBetweenSpawns = 0.8f;
    public float timeBetweenRounds = 3f;
    public int balloonHealth = 3;
    public int specialBalloonHealth = 5;
    public GameObject waypointPrefab;
    public Canvas waypointCanvas;

    [Header("Rounds")]
    public List<Round> rounds = new List<Round>();

    [Header("Gameplay")]
    public int maxLives = 5;

    private int goldCount;
    private int currentRoundIndex = -1;
    private int lives;
    private int balloonsAlive = 0;

    public TMP_Text roundText;
    public TMP_Text balloonCountText;
    public TMP_Text goldText;

    public bool gameEnd;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        gameEnd = false;
        lives = maxLives;

        introPanel?.SetActive(true);
        placeTurretText.SetActive(false);
        placeBaseText?.SetActive(false);

        tapToShootText.SetActive(false);
        winText.SetActive(false);
        loseText.SetActive(false);
        restartBtn.SetActive(false);

        if (planeManager) planeManager.enabled = false;
        if (placementMarker) placementMarker.enabled = false;
        if (objectSpawner) objectSpawner.enabled = false;
    }


    // Called from UI button
    public void OnStartScanning()
    {
        introPanel?.SetActive(false);
        placeBaseText?.SetActive(true);

        if (planeManager) planeManager.enabled = true;
        if (placementMarker) placementMarker.enabled = true;
        if (objectSpawner) objectSpawner.enabled = true;
    }


    // Called when the base is spawned
    public void SetDefenseBase(Transform baseTransform, Base baseScript)
    {
        DefenseBase = baseTransform;
        baseScript.Init(maxLives);

        placeBaseText?.SetActive(false);
        tapToShootText.SetActive(true);

        StartGame();
    }

    public bool HasDefenseBase()
    {
        return DefenseBase != null;
    }

    private void StartGame()
    {
        if (!HasDefenseBase()) return;
        if (rounds == null || rounds.Count == 0) return;

        AddGold(100);
        currentRoundIndex = -1;
        StartNextRound();
    }

    private void StartNextRound()
    {
        currentRoundIndex++;

        if (currentRoundIndex >= rounds.Count)
        {
            WinGame();
            return;
        }

        UpdateRoundText(rounds[currentRoundIndex].round);
        StartCoroutine(SpawnRoundRoutine(rounds[currentRoundIndex]));
    }

    private IEnumerator SpawnRoundRoutine(Round roundData)
    {
        if (!HasDefenseBase() || BalloonPrefab == null)
            yield break;

        int totalThisRound = roundData.normalBalloonCount + roundData.specialBalloonCount;
        balloonsAlive = totalThisRound;
        UpdateBalloonCountText(balloonsAlive);

        // Spawn normal balloons
        for (int i = 0; i < roundData.normalBalloonCount; i++)
        {
            SpawnOneBalloon(BalloonPrefab, roundData.normalBalloonSpeed, balloonHealth);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        // Spawn special balloons
        for (int i = 0; i < roundData.specialBalloonCount; i++)
        {
            SpawnOneBalloon(specialBalloonPrefab, roundData.specialBalloonSpeed, specialBalloonHealth);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        // Wait for all balloons to die/leak
        while (balloonsAlive > 0)
            yield return null;

        yield return new WaitForSeconds(timeBetweenRounds);
        StartNextRound();
    }

    private void SpawnOneBalloon(GameObject prefab, float moveSpeed, int health)
    {
        Vector3 basePos = DefenseBase.position;
        Vector2 offset2D = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos = basePos + new Vector3(offset2D.x, 0.2f, offset2D.y);

        GameObject b = Instantiate(prefab, spawnPos, Quaternion.identity);
        Balloon balloon = b.GetComponent<Balloon>();

        if (balloon != null)
            balloon.Init(DefenseBase, moveSpeed, health);

        if (waypointPrefab != null && waypointCanvas != null)
        {
            GameObject wpObj = Instantiate(waypointPrefab, waypointCanvas.transform);
            Waypoint wp = wpObj.GetComponent<Waypoint>();

            wp.target = b.transform;
            wp.arCamera = Camera.main;
        }
    }

    // Balloon reached the base
    public void OnBalloonLeak()
    {
        balloonsAlive--;
        lives--;

        UpdateBalloonCountText(balloonsAlive);
        FindAnyObjectByType<Base>()?.TakeDamage(1);

        if (lives <= 0)
            LoseGame();
    }

    // Balloon popped by player
    public void OnBalloonPopped(int gold)
    {
        balloonsAlive--;
        goldCount+=gold;

        UpdateGoldCountText(goldCount);
        UpdateBalloonCountText(balloonsAlive);
    }

    private void WinGame()
    {
        winText.SetActive(true);
        restartBtn.SetActive(true);
        gameEnd = true;
    }

    private void LoseGame()
    {
        loseText.SetActive(true);
        restartBtn.SetActive(true);
        gameEnd = true;
    }

    private void UpdateRoundText(int round)
    {
        roundText.text = "Round: " + round + "/5";
    }

    private void UpdateBalloonCountText(int count)
    {
        balloonCountText.text = "Enemies: " + count;
    }

    private void UpdateGoldCountText(int count)
    {
        goldText.text = "" + count;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public int GetGoldAmount()
    {
        return goldCount;
    }

    public void UseGold(int price)
    {
        goldCount -= price;
        UpdateGoldCountText(goldCount);
    }

    public void AddGold(int amount)
    {
        goldCount += amount;
        UpdateGoldCountText(goldCount);
    }
}

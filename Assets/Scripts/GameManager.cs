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
    public int specialBalloonCount;
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

    private Coroutine roundRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResetState();
    }

    /// <summary>
    /// Full reset of game state & UI. Called on startup and after scene reload.
    /// </summary>
    private void ResetState()
    {
        // core state
        gameEnd = false;
        goldCount = 0;
        lives = maxLives;
        balloonsAlive = 0;
        currentRoundIndex = -1;
        DefenseBase = null;

        // UI
        if (introPanel) introPanel.SetActive(true);
        if (placeBaseText) placeBaseText.SetActive(false);
        if (tapToShootText) tapToShootText.SetActive(false);
        if (placeTurretText) placeTurretText.SetActive(false);
        if (winText) winText.SetActive(false);
        if (loseText) loseText.SetActive(false);
        if (restartBtn) restartBtn.SetActive(false);

        // AR / placement
        if (planeManager) planeManager.enabled = false;
        if (placementMarker) placementMarker.enabled = false;
        if (objectSpawner) objectSpawner.enabled = false;

        // text values
        if (goldText) UpdateGoldCountText(goldCount);
        if (balloonCountText) UpdateBalloonCountText(0);
        if (roundText) UpdateRoundText(1);
    }

    // Called from UI button
    public void OnStartScanning()
    {
        if (introPanel) introPanel.SetActive(false);
        if (placeBaseText) placeBaseText.SetActive(true);

        if (planeManager) planeManager.enabled = true;
        if (placementMarker) placementMarker.enabled = true;
        if (objectSpawner) objectSpawner.enabled = true;
    }

    // Called when the base is spawned
    public void SetDefenseBase(Transform baseTransform, Base baseScript)
    {
        DefenseBase = baseTransform;
        baseScript.Init(maxLives);

        if (placeBaseText) placeBaseText.SetActive(false);
        if (tapToShootText) tapToShootText.SetActive(true);

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

        if (roundRoutine != null)
            StopCoroutine(roundRoutine);

        roundRoutine = StartCoroutine(SpawnRoundRoutine(rounds[currentRoundIndex]));
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
        if (gameEnd) return;

        balloonsAlive = Mathf.Max(0, balloonsAlive - 1);
        lives--;

        UpdateBalloonCountText(balloonsAlive);
        FindAnyObjectByType<Base>()?.TakeDamage(1);

        if (lives <= 0)
            LoseGame();
    }

    // Balloon popped by player
    public void OnBalloonPopped(int gold)
    {
        if (gameEnd) return;

        balloonsAlive = Mathf.Max(0, balloonsAlive - 1);
        goldCount += gold;

        UpdateGoldCountText(goldCount);
        UpdateBalloonCountText(balloonsAlive);
    }

    private void WinGame()
    {
        gameEnd = true;
        if (winText) winText.SetActive(true);
        if (restartBtn) restartBtn.SetActive(true);
    }

    private void LoseGame()
    {
        gameEnd = true;
        if (loseText) loseText.SetActive(true);
        if (restartBtn) restartBtn.SetActive(true);
    }

    private void UpdateRoundText(int round)
    {
        if (roundText)
            roundText.text = "Round: " + round + "/5";
    }

    private void UpdateBalloonCountText(int count)
    {
        if (balloonCountText)
            balloonCountText.text = "Enemies: " + count;
    }

    private void UpdateGoldCountText(int count)
    {
        if (goldText)
            goldText.text = "" + count;
    }

    public void RestartGame()
    {
        // safest: reload current scene so everything (including AR stuff) is rebuilt
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

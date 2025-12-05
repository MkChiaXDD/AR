using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;   // 👈 for ARPlaneManager

[System.Serializable]
public class Round
{
    public int round;
    public int balloonCount;
    public float balloonSpeed;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Transform DefenseBase { get; private set; }

    [Header("Pre-Game")]
    public GameObject introPanel;          // big panel explaining the game
    public GameObject tapToPlaceText;      // "Move phone & tap to place base"
    public ARPlaneManager planeManager;    // from XR Origin
    public PlacementMarker placementMarker;
    public ObjectSpawner objectSpawner;    // your existing spawner

    [Header("UI")]
    public GameObject preStartText;        // was already here - can reuse or hide later
    public GameObject tapToShootText;
    public GameObject winText;
    public GameObject loseText;
    public GameObject restartBtn;

    [Header("Balloon Settings")]
    public GameObject BalloonPrefab;
    public float spawnRadius = 0.7f;         // how far from base to spawn
    public float timeBetweenSpawns = 0.8f;   // delay between balloons
    public float timeBetweenRounds = 3f;     // delay between waves
    public int balloonHealth = 3;
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
    private bool isSpawning = false;

    public TMP_Text roundText;
    public TMP_Text balloonCountText;
    public TMP_Text goldText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        lives = maxLives;

        // ===== PRE-GAME STATE =====
        // Show intro panel, hide tap-to-place until Start Scanning
        if (introPanel != null) introPanel.SetActive(true);
        if (tapToPlaceText != null) tapToPlaceText.SetActive(true);

        // This was your old hint - you can keep it off until later or just not use it
        if (preStartText != null) preStartText.SetActive(false);

        // Gameplay UI off at start
        tapToShootText.SetActive(false);
        winText.SetActive(false);
        loseText.SetActive(false);
        restartBtn.SetActive(false);

        // AR components OFF at start – only ON after Start Scanning button
        if (planeManager != null) planeManager.enabled = false;
        if (placementMarker != null) placementMarker.enabled = false;
        if (objectSpawner != null) objectSpawner.enabled = false;
    }

    // =========================
    //   PRE-GAME BUTTON HOOK
    // =========================

    // Call this from your "Start Scanning" button
    public void OnStartScanning()
    {
        // Hide intro explanation
        if (introPanel != null) introPanel.SetActive(false);

        // Show text telling player to move phone & place base
        if (tapToPlaceText != null) tapToPlaceText.SetActive(false);

        // If you want to reuse preStartText as extra hint, you can:
        if (preStartText != null) preStartText.SetActive(true);

        // Enable plane detection + marker + spawner
        if (planeManager != null) planeManager.enabled = true;
        if (placementMarker != null) placementMarker.enabled = true;
        if (objectSpawner != null) objectSpawner.enabled = true;

        Debug.Log("Start Scanning pressed: AR plane detection enabled, placement active.");
    }

    // Called by ObjectSpawner AFTER the base is spawned
    public void SetDefenseBase(Transform baseTransform, Base baseScript)
    {
        DefenseBase = baseTransform;
        baseScript.Init(maxLives);
        Debug.Log("Defense base set: " + baseTransform.name);

        // Once base is placed: stop showing "tap to place" hints
        if (preStartText != null) preStartText.SetActive(false);

        // Show "Tap to shoot" text
        tapToShootText.SetActive(true);

        StartGame();
    }

    public bool HasDefenseBase()
    {
        return DefenseBase != null;
    }

    private void StartGame()
    {
        if (!HasDefenseBase())
        {
            Debug.LogWarning("No defense base, cannot start game.");
            return;
        }

        if (rounds == null || rounds.Count == 0)
        {
            Debug.LogWarning("No rounds configured in GameManager.");
            return;
        }

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

        Debug.Log($"Starting round {rounds[currentRoundIndex].round}");
        StartCoroutine(SpawnRoundRoutine(rounds[currentRoundIndex]));
    }

    private IEnumerator SpawnRoundRoutine(Round roundData)
    {
        if (!HasDefenseBase() || BalloonPrefab == null)
        {
            yield break;
        }

        isSpawning = true;
        balloonsAlive = roundData.balloonCount;

        UpdateBalloonCountText(balloonsAlive);

        for (int i = 0; i < roundData.balloonCount; i++)
        {
            SpawnOneBalloon(roundData);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        isSpawning = false;

        // Wait until all balloons in this round are gone, then go to next round
        while (balloonsAlive > 0)
        {
            yield return null;
        }

        yield return new WaitForSeconds(timeBetweenRounds);
        StartNextRound();
    }

    private void SpawnOneBalloon(Round roundData)
    {
        Vector3 basePos = DefenseBase.position;

        Vector2 offset2D = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos = basePos + new Vector3(offset2D.x, 0.2f, offset2D.y);

        GameObject b = Instantiate(BalloonPrefab, spawnPos, Quaternion.identity);

        Balloon balloon = b.GetComponent<Balloon>();
        if (balloon != null)
        {
            balloon.Init(DefenseBase, roundData.balloonSpeed, balloonHealth);
        }
        else
        {
            Debug.LogWarning("BalloonPrefab has no Balloon script attached.");
        }

        // 🔹 Create waypoint for this balloon
        if (waypointPrefab != null && waypointCanvas != null)
        {
            GameObject wpObj = Instantiate(waypointPrefab, waypointCanvas.transform);
            Waypoint wp = wpObj.GetComponent<Waypoint>();

            wp.target = b.transform;         // follow this balloon
            wp.arCamera = Camera.main;       // or your AR camera reference
        }
    }

    // Called when a balloon reaches the base
    public void OnBalloonLeak()
    {
        balloonsAlive--;
        lives--;

        UpdateBalloonCountText(balloonsAlive);

        FindAnyObjectByType<Base>()?.TakeDamage(1);

        if (lives <= 0)
        {
            LoseGame();
        }
    }

    // Call this later from your tap shooter when you pop a balloon
    public void OnBalloonPopped()
    {
        balloonsAlive--;
        goldCount++;
        UpdateGoldCountText(goldCount);
        UpdateBalloonCountText(balloonsAlive);
    }

    private void WinGame()
    {
        Debug.Log("YOU WIN! All rounds cleared.");
        winText.SetActive(true);
        restartBtn.SetActive(true);
    }

    private void LoseGame()
    {
        Debug.Log("GAME OVER!");
        loseText.SetActive(true);
        restartBtn.SetActive(true);
    }

    private void UpdateRoundText(int round)
    {
        roundText.text = "Round: " + round + "/5";
    }

    private void UpdateBalloonCountText(int count)
    {
        balloonCountText.text = "Balloons Left: " + count;
    }

    private void UpdateGoldCountText(int count)
    {
        goldText.text = "Gold: " + count;
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
}

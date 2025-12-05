using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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

    [Header("Balloon Settings")]
    public GameObject BalloonPrefab;
    public float spawnRadius = 0.7f;         // how far from base to spawn
    public float timeBetweenSpawns = 0.8f;   // delay between balloons
    public float timeBetweenRounds = 3f;     // delay between waves
    public GameObject waypointPrefab;
    public Canvas waypointCanvas;

    [Header("Rounds")]
    public List<Round> rounds = new List<Round>();

    [Header("Gameplay")]
    public int maxLives = 3;

    private int currentRoundIndex = -1;
    private int lives;
    private int balloonsAlive = 0;
    private bool isSpawning = false;

    public TMP_Text roundText;
    public TMP_Text balloonCountText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        lives = maxLives;
    }

    // Called by ObjectSpawner AFTER the base is spawned
    public void SetDefenseBase(Transform baseTransform, Base baseScript)
    {
        DefenseBase = baseTransform;
        baseScript.Init(maxLives);
        Debug.Log("Defense base set: " + baseTransform.name);
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
            balloon.Init(DefenseBase, roundData.balloonSpeed);
        }
        else
        {
            Debug.LogWarning("BalloonPrefab has no Balloon script attached.");
        }

        // ?? Create waypoint for this balloon
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

        UpdateBalloonCountText(balloonsAlive);
    }

    private void WinGame()
    {
        Debug.Log("YOU WIN! All rounds cleared.");
        // TODO: show win UI, stop game, etc.
    }

    private void LoseGame()
    {
        Debug.Log("GAME OVER!");
        // TODO: show lose UI, stop game, etc.
        
    }

    private void UpdateRoundText(int round)
    {
        roundText.text = "Round: " + round;
    }

    private void UpdateBalloonCountText(int count)
    {
        balloonCountText.text = "Balloons Left: " + count;
    }
}

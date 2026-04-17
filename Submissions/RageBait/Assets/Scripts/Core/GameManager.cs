using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Menu,
    Playing,
    Dead,
    Won
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action<GameState> OnGameStateChanged;
    public event Action<int> OnDeathCountChanged;

    [Header("References")]
    public Transform playerSpawnPoint;
    public GameObject player;

    [Header("Settings")]
    public float respawnDelay = 1.5f;

    public GameState CurrentState { get; private set; } = GameState.Menu;
    public int DeathCount { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        AutoFindReferences();
        SetState(GameState.Menu);
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.DisableControl();
        }
    }

    void Update()
    {
        if (CurrentState == GameState.Menu && Input.GetKeyDown(KeyCode.Space))
            StartGame();
    }

    void AutoFindReferences()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p;
        }
        if (playerSpawnPoint == null)
        {
            GameObject sp = GameObject.Find("SpawnPoint");
            if (sp != null) playerSpawnPoint = sp.transform;
        }
    }

    public void StartGame()
    {
        DeathCount = 0;
        OnDeathCountChanged?.Invoke(DeathCount);
        // Apply level-specific rules (not just reset — levels have different rules)
        LevelManager.ApplyLevelRules(LevelManager.CurrentLevel);
        SetState(GameState.Playing);
        RespawnPlayer();
    }

    public void OnPlayerDeath(HazardType? causeOfDeath)
    {
        if (CurrentState != GameState.Playing) return;

        DeathCount++;
        OnDeathCountChanged?.Invoke(DeathCount);
        SetState(GameState.Dead);

        // Re-apply level base rules to keep the betrayal consistent
        // Then layer on death-based mutations for extra chaos
        LevelManager.ApplyLevelRules(LevelManager.CurrentLevel);

        // In Level 3 (chaos), also mutate a random rule on top for unpredictability
        if (LevelManager.CurrentLevel >= 3 && RuleEngine.Instance != null)
        {
            if (causeOfDeath.HasValue)
                RuleEngine.Instance.MutateRule(causeOfDeath.Value);
        }

        StartCoroutine(RespawnAfterDelay());
    }

    public void OnPlayerWin()
    {
        if (CurrentState != GameState.Playing) return;
        Time.timeScale = 1f;
        SetState(GameState.Won);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        LevelManager.ResetToLevel1();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SetState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSecondsRealtime(respawnDelay);
        RespawnPlayer();
        SetState(GameState.Playing);
    }

    private void RespawnPlayer()
    {
        if (player == null) { AutoFindReferences(); if (player == null) return; }

        // ALWAYS reset timeScale first
        Time.timeScale = 1f;

        // Reset all player state
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null) health.ResetHealth();

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null) controller.EnableControl();

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = true; // Reset gravity in case GravityZone broke it
        }

        // Reset control reverser
        if (ControlReverser.Instance != null)
            ControlReverser.Instance.ForceReset();

        // Restore player speed in case SpeedTrap left it modified
        if (controller != null)
            controller.moveSpeed = 8f;

        // Use level zone spawn point if available, fallback to default
        Vector3 spawnPos = new Vector3(0, 2, 0);
        if (LevelManager.Instance != null)
        {
            foreach (var zone in LevelManager.Instance.zones)
            {
                if (zone != null && zone.levelNumber == LevelManager.CurrentLevel && zone.spawnPoint != null)
                {
                    spawnPos = zone.spawnPoint.position;
                    break;
                }
            }
        }
        else if (playerSpawnPoint != null)
        {
            spawnPos = playerSpawnPoint.position;
        }
        player.transform.position = spawnPos;
        player.transform.rotation = Quaternion.identity;
        player.SetActive(true);

        // Snap camera
        CameraFollow cam = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
        if (cam != null) cam.SnapToTarget();
    }
}

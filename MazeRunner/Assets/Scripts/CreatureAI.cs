using UnityEngine;

public class CreatureAI : MonoBehaviour
{
    [Header("Behavior")]
    public float wanderSpeed = 2.2f;
    public float chaseSpeed = 6f;
    public float detectionRange = 7f;
    public float killRange = 1.1f;
    public float spawnDelay = 30f;

    [Header("Hearing")]
    public float hearingRange = 18f;
    public float investigationCooldown = 1.5f;

    [Header("Audio")]
    public AudioClip breathingClip;
    public AudioClip chaseClip;

    Transform player;
    AudioSource audioSource;
    MazeGenerator maze;
    bool isActive;
    bool isChasing;
    float activateTime;
    float repathTimer;
    Vector3 currentTarget;

    bool hasInvestigation;
    Vector3 investigationPos;
    float investigationExpire;

    void OnEnable()
    {
        NoiseSystem.OnNoise += HandleNoise;
    }

    void OnDisable()
    {
        NoiseSystem.OnNoise -= HandleNoise;
    }

    void Start()
    {
        var pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
        maze = FindFirstObjectByType<MazeGenerator>();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 1.5f;
        audioSource.maxDistance = 22f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.loop = true;

        activateTime = Time.time + spawnDelay;
        SetVisible(false);
    }

    void SetVisible(bool v)
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = v;
        foreach (var l in GetComponentsInChildren<Light>(true)) l.enabled = v;
    }

    void HandleNoise(Vector3 pos, float radius)
    {
        if (!isActive) return;
        if (Vector3.Distance(transform.position, pos) > Mathf.Min(hearingRange, radius * 2f)) return;
        // Don't override an active chase.
        if (isChasing) return;
        hasInvestigation = true;
        investigationPos = pos;
        investigationExpire = Time.time + 8f;
    }

    void Update()
    {
        if (player == null) return;

        if (!isActive)
        {
            if (Time.time >= activateTime) Activate();
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.gameIsOver)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        bool sees = false;
        if (dist < detectionRange)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer, out RaycastHit hit, detectionRange))
            {
                if (hit.transform == player || hit.transform.IsChildOf(player))
                    sees = true;
            }
        }

        isChasing = sees;
        UpdateAudio();

        if (sees)
        {
            // Direct chase.
            Vector3 dir = (player.position - transform.position);
            dir.y = 0; dir.Normalize();
            transform.position += dir * chaseSpeed * Time.deltaTime;
            FaceMovement(dir);
            hasInvestigation = false;
        }
        else if (hasInvestigation && Time.time < investigationExpire)
        {
            FollowMazePathTo(investigationPos);
            if (Vector3.Distance(transform.position, investigationPos) < 1.2f)
                hasInvestigation = false;
        }
        else
        {
            FollowMazePathTo(player.position);
        }

        if (dist < killRange)
        {
            GameManager.Instance?.Lose("Something found you.");
        }
    }

    void Activate()
    {
        isActive = true;
        SetVisible(true);

        if (maze != null)
        {
            int x = maze.mazeWidth - 1;
            int z = maze.mazeHeight - 1;
            transform.position = maze.CellToWorld(new Vector2Int(x, z), 1f);
            currentTarget = transform.position;
        }
        Hud.Toast("<color=#FF6E6E>Something stirs in the maze.</color>", 3f);
    }

    void FollowMazePathTo(Vector3 worldTarget)
    {
        if (maze == null || !maze.IsBuilt)
        {
            Vector3 d = (worldTarget - transform.position); d.y = 0;
            if (d.sqrMagnitude > 0.01f)
                transform.position += d.normalized * wanderSpeed * Time.deltaTime;
            return;
        }

        repathTimer -= Time.deltaTime;
        Vector3 toTarget = currentTarget - transform.position;
        toTarget.y = 0;

        if (repathTimer <= 0f || toTarget.sqrMagnitude < 0.25f)
        {
            repathTimer = 0.5f;
            var fromCell = maze.WorldToCell(transform.position);
            var toCell = maze.WorldToCell(worldTarget);
            var next = maze.NextStepToward(fromCell, toCell);
            if (next.x >= 0)
                currentTarget = maze.CellToWorld(next, transform.position.y);
            else
                currentTarget = worldTarget;
        }

        Vector3 dir = currentTarget - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();
            transform.position += dir * wanderSpeed * Time.deltaTime;
            FaceMovement(dir);
        }
    }

    void FaceMovement(Vector3 dir)
    {
        if (dir.sqrMagnitude < 0.001f) return;
        var look = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * 6f);
    }

    void UpdateAudio()
    {
        var wantClip = isChasing ? chaseClip : breathingClip;
        if (wantClip == null) return;
        if (audioSource.clip != wantClip)
        {
            audioSource.clip = wantClip;
            audioSource.Play();
        }
        else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public bool IsActive => isActive;
    public bool IsChasing => isChasing;
    public float DistanceToPlayer => player == null ? float.MaxValue :
        Vector3.Distance(transform.position, player.position);
}

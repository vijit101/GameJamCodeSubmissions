using UnityEngine;

public class DifficultyEscalator : MonoBehaviour
{
    public static DifficultyEscalator Instance { get; private set; }

    public float enemySpeedMultiplier = 1f;
    public float hazardSizeMultiplier = 1f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnDeathCountChanged += OnDeathCountChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnDeathCountChanged -= OnDeathCountChanged;
    }

    void OnDeathCountChanged(int deaths)
    {
        enemySpeedMultiplier = 1f + deaths * 0.08f;
        hazardSizeMultiplier = 1f + deaths * 0.03f;

        MovingEnemy[] enemies = FindObjectsOfType<MovingEnemy>();
        foreach (MovingEnemy me in enemies)
        {
            me.moveSpeed = me.moveSpeed > 0 ?
                Mathf.Abs(me.moveSpeed) / (1f + (deaths - 1) * 0.08f) * enemySpeedMultiplier :
                2.5f * enemySpeedMultiplier;
        }

        if (deaths == 5 && RageBaitMessages.Instance != null)
            RageBaitMessages.Instance.ShowMessage(
                "difficulty++;\n// enemies are getting faster btw",
                new Color(1f, 0.5f, 0f), 2f
            );

        if (deaths == 10 && RageBaitMessages.Instance != null)
            RageBaitMessages.Instance.ShowMessage(
                "if (deaths >= 10) {\n  difficulty = NIGHTMARE;\n}",
                new Color(1f, 0f, 0f), 3f
            );

        if (deaths == 15 && RageBaitMessages.Instance != null)
            RageBaitMessages.Instance.ShowMessage(
                "// at this point we're just impressed\n// you haven't ragequit",
                new Color(1f, 0.84f, 0f), 3f
            );
    }
}

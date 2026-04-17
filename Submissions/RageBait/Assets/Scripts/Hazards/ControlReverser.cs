using UnityEngine;
using System.Collections;

public class ControlReverser : MonoBehaviour
{
    public static ControlReverser Instance { get; private set; }
    public bool IsReversed { get; private set; } = false;

    public float reverseDuration = 5f;

    private int reverseCount = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
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

    void OnDeathCountChanged(int count)
    {
        // Every 4th death, reverse controls for a bit
        if (count > 0 && count % 4 == 0)
        {
            StartCoroutine(ReverseControls());
        }
    }

    public void ForceReset()
    {
        StopAllCoroutines();
        IsReversed = false;
    }

    IEnumerator ReverseControls()
    {
        IsReversed = true;
        reverseCount++;

        if (RageBaitMessages.Instance != null)
            RageBaitMessages.Instance.ShowMessage("CONTROLS REVERSED\nif (left) { goRight(); }", new Color(1f, 0.5f, 0f), 2f);

        yield return new WaitForSeconds(reverseDuration);

        IsReversed = false;

        if (RageBaitMessages.Instance != null)
            RageBaitMessages.Instance.ShowMessage("controls restored... for now", new Color(0.5f, 1f, 0.5f), 1.5f);
    }
}

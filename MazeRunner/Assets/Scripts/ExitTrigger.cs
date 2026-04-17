using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ExitTrigger : MonoBehaviour
{
    // Short arm-delay so we ignore any initial overlap caused by the maze
    // generator moving the trigger through the player's spawn point on Start.
    public float armDelaySeconds = 0.5f;
    private float armedAt;

    void OnEnable()
    {
        armedAt = Time.time + armDelaySeconds;
    }

    void Reset()
    {
        var c = GetComponent<Collider>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time < armedAt) return;
        if (other.GetComponentInParent<PlayerController>() == null) return;
        if (GameManager.Instance != null) GameManager.Instance.Win();
    }
}

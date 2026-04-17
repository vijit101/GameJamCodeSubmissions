using UnityEngine;

public enum PickupType { Page, Battery }

[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    public PickupType type;
    public float bobAmplitude = 0.18f;
    public float bobFrequency = 1.6f;
    public float spinSpeed = 90f;

    Vector3 basePos;
    Light glow;

    void Start()
    {
        basePos = transform.position;
        glow = GetComponentInChildren<Light>();
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        var p = basePos;
        p.y = basePos.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = p;

        if (glow != null)
        {
            float pulse = 0.6f + 0.25f * Mathf.Sin(Time.time * 2.5f);
            glow.intensity = pulse * (type == PickupType.Battery ? 1.4f : 1.0f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerController>() == null) return;
        Collect();
    }

    void Collect()
    {
        if (type == PickupType.Page)
        {
            ScoreSystem.pagesCollected++;
            Hud.Toast($"Page  {ScoreSystem.pagesCollected}/{ScoreSystem.totalPages}", 1.6f);
        }
        else
        {
            var fl = Object.FindFirstObjectByType<Flashlight>();
            if (fl != null)
            {
                fl.currentBattery = Mathf.Min(fl.maxBattery,
                    fl.currentBattery + fl.maxBattery * 0.5f);
            }
            ScoreSystem.batteriesCollected++;
            Hud.Toast("Battery +50%", 1.6f);
        }

        Hud.PingPickup();
        Destroy(gameObject);
    }
}

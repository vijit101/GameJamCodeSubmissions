using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("Battery")]
    public float maxBattery = 240f;  // seconds (4 minutes)
    public float currentBattery;
    public float offDrainMultiplier = 0.1f;  // drains 10% speed when off
    public float sprintDrainMultiplier = 2f;  // drains 2x when sprinting

    [Header("Light Settings")]
    public float fullAngle = 45f;       // spot angle at 100% battery
    public float emptyAngle = 20f;      // spot angle when nearly dead
    public float fullIntensity = 3f;
    public float emptyIntensity = 0.5f;
    public float fullRange = 20f;
    public float emptyRange = 8f;

    [Header("Flicker")]
    public float flickerStartPercent = 0.2f; // start flickering at 20%
    public float strobeStartPercent = 0.05f; // heavy strobe at 5%

    [Header("Audio")]
    public AudioSource flashlightAudio;
    public AudioClip toggleSound;
    public AudioClip flickerSound;

    private Light spotLight;
    private bool isOn = true;
    private PlayerController playerController;

    void Start()
    {
        currentBattery = maxBattery;

        // Prefer a Light on this GameObject, then search children, then parents.
        spotLight = GetComponent<Light>();
        if (spotLight == null) spotLight = GetComponentInChildren<Light>(true);
        if (spotLight == null) spotLight = GetComponentInParent<Light>();

        if (spotLight == null)
        {
            Debug.LogError(
                "[Flashlight] No Light component found on '" + name +
                "' or its children/parents. Attach this script to the Spot Light " +
                "(or a GameObject that has one as a child). Disabling script.",
                this);
            enabled = false;
            return;
        }

        playerController = GetComponentInParent<PlayerController>();
    }

    void Update()
    {
        if (spotLight == null) return;

        HandleToggle();
        DrainBattery();
        UpdateLightProperties();
        HandleFlicker();
    }

    void HandleToggle()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            isOn = !isOn;
            spotLight.enabled = isOn;

            if (flashlightAudio != null && toggleSound != null)
                flashlightAudio.PlayOneShot(toggleSound);

            // The click is loud enough to make a curious thing curious.
            NoiseSystem.Emit(transform.position, 6f);
        }
    }

    void DrainBattery()
    {
        float drainRate = 1f;

        if (!isOn)
            drainRate = offDrainMultiplier;
        else if (playerController != null && playerController.IsSprinting())
            drainRate = sprintDrainMultiplier;

        currentBattery -= drainRate * Time.deltaTime;
        currentBattery = Mathf.Max(0f, currentBattery);
    }

    void UpdateLightProperties()
    {
        if (!isOn) return;

        float percent = currentBattery / maxBattery; // 1.0 = full, 0.0 = dead

        // Gradually reduce light quality
        spotLight.spotAngle = Mathf.Lerp(emptyAngle, fullAngle, percent);
        spotLight.intensity = Mathf.Lerp(emptyIntensity, fullIntensity, percent);
        spotLight.range = Mathf.Lerp(emptyRange, fullRange, percent);
    }

    void HandleFlicker()
    {
        if (!isOn) return;

        float percent = currentBattery / maxBattery;

        if (percent <= strobeStartPercent)
        {
            // Heavy strobing
            spotLight.enabled = Random.value > 0.4f; // 40% chance of being off each frame
        }
        else if (percent <= flickerStartPercent)
        {
            // Occasional flicker
            if (Random.value < 0.03f) // 3% chance per frame
            {
                spotLight.enabled = false;
                Invoke(nameof(TurnBackOn), Random.Range(0.05f, 0.15f));
            }
        }
    }

    void TurnBackOn()
    {
        if (isOn) spotLight.enabled = true;
    }

    // Public getters for UI and game manager
    public float GetBatteryPercent() => currentBattery / maxBattery;
    public bool IsFlashlightOn() => isOn;
    public bool IsDead() => currentBattery <= 0f;
}
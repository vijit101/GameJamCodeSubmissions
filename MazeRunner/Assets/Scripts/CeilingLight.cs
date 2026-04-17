using System.Collections;
using UnityEngine;

// Hanging bulb with occasional flicker bursts. Some bulbs start dead.
public class CeilingLight : MonoBehaviour
{
    Light l;
    bool dead;
    float nextFlickerTime;

    void Start()
    {
        l = GetComponent<Light>();
        dead = Random.value < 0.25f;
        if (dead && l != null) l.enabled = false;
        else nextFlickerTime = Time.time + Random.Range(6f, 20f);
    }

    void Update()
    {
        if (dead || l == null) return;
        if (Time.time < nextFlickerTime) return;
        StartCoroutine(FlickerBurst());
        nextFlickerTime = Time.time + Random.Range(10f, 30f);
    }

    IEnumerator FlickerBurst()
    {
        int n = Random.Range(3, 7);
        for (int i = 0; i < n; i++)
        {
            l.enabled = !l.enabled;
            yield return new WaitForSeconds(Random.Range(0.03f, 0.14f));
        }
        l.enabled = true;
    }
}

using UnityEngine;

// Blood streak on a wall that slowly grows downward, pauses, shrinks, repeats.
// Give it a vertical quad child whose Y-scale it animates.
public class DrippingBlood : MonoBehaviour
{
    public Transform streak;        // child quad, pivot at top
    public float maxLength = 0.9f;
    public float growSpeed = 0.12f;
    public float shrinkSpeed = 0.3f;
    public float holdAtFull = 2.5f;

    enum Phase { Grow, Hold, Shrink, Rest }
    Phase phase;
    float length;
    float holdTimer;

    void Start()
    {
        if (streak == null) return;
        // Offset the pivot by parenting under an empty wrapper. If the user passed the quad
        // directly, assume its parent anchors the top and we scale Y downward.
        phase = Phase.Grow;
        length = Random.Range(0f, 0.2f);
        ApplyLength();
    }

    void Update()
    {
        if (streak == null) return;

        switch (phase)
        {
            case Phase.Grow:
                length += growSpeed * Time.deltaTime;
                if (length >= maxLength) { length = maxLength; phase = Phase.Hold; holdTimer = 0f; }
                break;
            case Phase.Hold:
                holdTimer += Time.deltaTime;
                if (holdTimer >= holdAtFull) phase = Phase.Shrink;
                break;
            case Phase.Shrink:
                length -= shrinkSpeed * Time.deltaTime;
                if (length <= 0f)
                {
                    length = 0f;
                    phase = Phase.Rest;
                    holdTimer = 0f;
                }
                break;
            case Phase.Rest:
                holdTimer += Time.deltaTime;
                if (holdTimer >= Random.Range(4f, 10f)) phase = Phase.Grow;
                break;
        }

        ApplyLength();
    }

    void ApplyLength()
    {
        var s = streak.localScale;
        s.y = length;
        streak.localScale = s;
        // streak's pivot is top (y=0.5 on quad), so we offset down by length/2 to
        // keep the top glued to the starting point.
        var lp = streak.localPosition;
        lp.y = -length * 0.5f;
        streak.localPosition = lp;
    }
}

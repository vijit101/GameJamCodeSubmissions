using UnityEngine;

public class MovingSawBlade : MonoBehaviour
{
    public float moveSpeed = 4f;
    public float moveRange = 5f;
    public float rotateSpeed = 360f;
    public bool vertical = false;

    private Vector3 startPos;
    private float direction = 1f;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);

        Vector3 pos = transform.position;
        if (vertical)
        {
            pos.y += moveSpeed * direction * Time.deltaTime;
            if (pos.y > startPos.y + moveRange || pos.y < startPos.y - moveRange)
                direction *= -1f;
        }
        else
        {
            pos.x += moveSpeed * direction * Time.deltaTime;
            if (pos.x > startPos.x + moveRange || pos.x < startPos.x - moveRange)
                direction *= -1f;
        }
        transform.position = pos;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            if (RageBaitMessages.Instance != null)
            {
                string[] msgs = {
                    "SAW BLADE GO BRRRRR",
                    "if (blade.isTouching(player)) { slice(); }",
                    "you walked into a spinning blade.\nthink about that.",
                    "buzz buzz, you're dead"
                };
                RageBaitMessages.Instance.ShowMessage(
                    msgs[Random.Range(0, msgs.Length)],
                    new Color(1f, 0.2f, 0f), 1.5f
                );
            }
            health.TakeDamage(3, null);
        }
    }
}

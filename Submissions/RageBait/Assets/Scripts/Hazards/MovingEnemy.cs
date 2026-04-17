using UnityEngine;

public class MovingEnemy : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float moveRange = 3f;
    public bool vertical = false;

    private Vector3 startPos;
    private float direction = 1f;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
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
}

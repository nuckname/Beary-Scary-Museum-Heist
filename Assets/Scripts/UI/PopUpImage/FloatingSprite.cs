using UnityEngine;

public class FloatingSprite : MonoBehaviour
{
    public float speed = 2.0f;
    public float height = 0.5f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * speed) * height;

        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
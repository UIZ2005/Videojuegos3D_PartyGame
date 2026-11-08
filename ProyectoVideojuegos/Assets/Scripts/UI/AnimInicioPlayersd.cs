using UnityEngine;

public class AnimInicioPlayersd : MonoBehaviour
{
    public float speed = 2f;
    public float distance = 24f; // cuánto camina antes de destruirse

    Vector3 startPos;

    void Start() => startPos = transform.position;

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime, Space.World);

        if (transform.position.x - startPos.x > distance)
            Destroy(gameObject);
    }
}
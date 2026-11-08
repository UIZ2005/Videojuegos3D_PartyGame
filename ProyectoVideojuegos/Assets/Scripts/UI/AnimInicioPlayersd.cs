using UnityEngine;

public class AnimInicioPlayersd : MonoBehaviour
{
    public float speed = 2f;
    [HideInInspector] public Vector3 target;

    void Start()
    {
        speed *= Random.Range(0.8f, 1.2f);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (transform.position == target)
            Destroy(gameObject);
    }
}
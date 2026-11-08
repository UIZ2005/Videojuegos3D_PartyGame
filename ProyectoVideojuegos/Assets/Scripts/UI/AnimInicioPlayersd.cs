using UnityEngine;

public class AnimInicioPlayersd : MonoBehaviour
{
    [Header("Movimiento")]
    public float minSpeed = 60f;
    public float maxSpeed = 140f;

    public bool syncAnimation = true;
    public float referenceSpeed = 100f;

    [Header("Efecto al hacer clic")]
    public GameObject explosionPrefab;   // prefab de partículas
    public AudioClip clickSound;         // sonido de este personaje
    [Range(0f, 1f)] public float volume = 1f;

    [HideInInspector] public Vector3 target;

    float speed;
    bool exploded;

    void Start()
    {
        speed = Random.Range(minSpeed, maxSpeed);

        if (syncAnimation)
        {
            Animator anim = GetComponentInChildren<Animator>();
            if (anim != null)
                anim.speed = speed / referenceSpeed;
        }
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (transform.position == target)
            Destroy(gameObject);
    }

    // Lo llama el Spawner cuando clicas al personaje
    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        // centro visual del modelo
        Vector3 center = transform.position;
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                b.Encapsulate(renderers[i].bounds);
            center = b.center;
        }

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, center, Quaternion.identity);

        // se reproduce en la posición de la cámara para que siempre se escuche
        if (clickSound != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position, volume);

        Destroy(gameObject);
    }
}
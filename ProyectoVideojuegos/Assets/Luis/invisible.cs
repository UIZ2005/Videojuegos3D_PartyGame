using UnityEngine;
using System.Collections;

public class invisible : PowerUp
{
    [Header("Disolver")]
    public float disolverDuration = 2f;
    public float duracionInvisibilidad = 5f;
    public float disolverStrength;
    public Material[] disolveMaterials;

    private Material disolveMaterial;

    private AudioSource audioitem;
    public AudioClip clip;

    [Header("Visual")]
    public GameObject objvisual;
    [SerializeField] float escalaPequena = 0.5f;

    private bool estaSostenido = false;
    private bool yaUsado = false;

    private Rigidbody rb;
    private Collider col;
    private Vector3 escalaOriginal;

    private GameObject player;

    // Renderer incluye:
    // MeshRenderer
    // SkinnedMeshRenderer
    private Renderer[] renderers;

    private Material[][] materialesOriginales;

    private audiomanager Audiomanager;

    void Start()
    {
        audioitem = GetComponent<AudioSource>();
        Audiomanager = FindAnyObjectByType<audiomanager>();

        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        escalaOriginal = transform.localScale;
    }

    public override bool PuedeSerRecogido()
    {
        return !estaSostenido && !yaUsado;
    }

    public override void SerRecogido(Transform puntoAgarre)
    {
        if (Audiomanager != null)
            Audiomanager.seleccionAudio(0);

        rb.isKinematic = true;
        col.isTrigger = true;

        player = puntoAgarre.root.gameObject;

        movePlayer jugador = player.GetComponent<movePlayer>();

        if (jugador == null)
        {
            Debug.LogError("No se encontró movePlayer en el jugador.");
            return;
        }

        // Seleccionar material de disolución según el jugador.
        disolveMaterial = disolveMaterials[jugador.numplayer];

        // Obtener TODOS los Renderer del jugador.
        // Esto incluye MeshRenderer y SkinnedMeshRenderer.
        renderers = player.GetComponentsInChildren<Renderer>();

        materialesOriginales = new Material[renderers.Length][];

        // Guardar los materiales originales.
        for (int i = 0; i < renderers.Length; i++)
        {
            materialesOriginales[i] = renderers[i].materials;
        }

        transform.position = puntoAgarre.position;

        transform.SetParent(puntoAgarre);

        transform.localScale = escalaOriginal * escalaPequena;
    }

    public override void Usar(Vector3 direccionUso)
    {
        if (yaUsado)
            return;

        audioitem.PlayOneShot(clip);

        yaUsado = true;

        objvisual.SetActive(false);

        // Aplicar material de disolución a todos
        // los Renderer del jugador.
        foreach (Renderer renderer in renderers)
        {
            renderer.materials = CrearMaterialesDisolver(renderer);
        }

        StartDisolve();
    }

    private Material[] CrearMaterialesDisolver(Renderer renderer)
    {
        Material[] materiales = new Material[renderer.materials.Length];

        for (int i = 0; i < materiales.Length; i++)
        {
            materiales[i] = disolveMaterial;
        }

        return materiales;
    }

    private void StartDisolve()
    {
        StartCoroutine(Dissolve());
    }

    private void StartApper()
    {
        StartCoroutine(Apper());
    }

    IEnumerator Dissolve()
    {
        float elapsedTime = 0f;

        while (elapsedTime < disolverDuration)
        {
            elapsedTime += Time.deltaTime;

            disolverStrength = Mathf.Lerp(
                0f,
                1f,
                elapsedTime / disolverDuration
            );

            disolveMaterial.SetFloat(
                "_Escala_Disolver",
                disolverStrength
            );

            yield return null;
        }

        yield return new WaitForSeconds(duracionInvisibilidad);

        StartApper();
    }

    IEnumerator Apper()
    {
        float elapsedTime = 0f;

        while (elapsedTime < disolverDuration)
        {
            elapsedTime += Time.deltaTime;

            disolverStrength = Mathf.Lerp(
                1f,
                0f,
                elapsedTime / disolverDuration
            );

            disolveMaterial.SetFloat(
                "_Escala_Disolver",
                disolverStrength
            );

            yield return null;
        }

        // Restaurar todos los materiales originales.
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].materials = materialesOriginales[i];
        }

        Destroy(gameObject);
    }
}
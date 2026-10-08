using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class invisible : PowerUp
{

    [Header("Disolver")]
    public float disolverDuration = 2f;
    public float duracionInvisibilidad=5f;
    public float disolverStrength;
    public Material[] disolveMaterials;
    private Material disolveMaterial;

    [Header("visual")]
    public GameObject objvisual;
    [SerializeField] float escalaPequena = 0.5f;

    private bool estaSostenido = false;
    private bool yaUsado = false;

    private Rigidbody rb;
    private Collider col;
    private Vector3 escalaOriginal;
    private GameObject player;
    private MeshRenderer[] meshRenderers;
    private Material[][] materialesOriginales;
    private audiomanager Audiomanager;
    void Start()
    {
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
        disolveMaterial = disolveMaterials[player.GetComponent<movePlayer>().numplayer];
        meshRenderers = player.GetComponentsInChildren<MeshRenderer>();
        materialesOriginales = new Material[meshRenderers.Length][];

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            materialesOriginales[i] = meshRenderers[i].materials;
        }

        transform.position = puntoAgarre.position;
        transform.SetParent(puntoAgarre);
        transform.localScale = escalaOriginal;
        transform.localScale = escalaOriginal * escalaPequena;
    }
    public override void Usar(Vector3 direccionUso)
    {
        if (yaUsado)
            return;
        yaUsado = true;
        objvisual.SetActive(false);

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material = disolveMaterial;
        }
        StartDisolve();
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

            disolverStrength = Mathf.Lerp(0f, 1f, elapsedTime / disolverDuration);
            disolveMaterial.SetFloat("_Escala_Disolver", disolverStrength);

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

            disolverStrength = Mathf.Lerp(1f, 0f, elapsedTime / disolverDuration);
            disolveMaterial.SetFloat("_Escala_Disolver", disolverStrength);

            yield return null;
        }
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            meshRenderers[i].materials = materialesOriginales[i];
        }

        Destroy(gameObject);
    }
}

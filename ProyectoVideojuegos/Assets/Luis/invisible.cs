using UnityEngine;
using System.Collections;

public class invisible : PowerUp
{

    [Header("Disolver")]
    public float disolverDuration = 2f;
    public float duracionInvisibilidad=5f;
    public float disolverStrength;
    public Material disolveMaterial;

    [Header("visual")]
    public GameObject objvisual;


    private bool estaSostenido = false;
    private bool yaUsado = false;

    private Rigidbody rb;
    private Collider col;
    private Vector3 escalaOriginal;
    private GameObject player;
    private Material actualMaterial;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        escalaOriginal = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public override bool PuedeSerRecogido()
    {
        return !estaSostenido && !yaUsado;
    }
    public override void SerRecogido(Transform puntoAgarre)
    {
        rb.isKinematic = true;
        col.isTrigger = true;
        player = puntoAgarre.root.gameObject;
        actualMaterial = puntoAgarre.root.GetComponent<MeshRenderer>().material;
        transform.position = puntoAgarre.position;
        transform.SetParent(puntoAgarre);
        transform.localScale = escalaOriginal;
    }
    public override void Usar(Vector3 direccionUso)
    {
        if (yaUsado)
            return;
        yaUsado = true;
        objvisual.SetActive(false);
        player.GetComponent<Renderer>().material = disolveMaterial;
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
        player.GetComponent<Renderer>().material = actualMaterial;

        Destroy(gameObject);
    }
}

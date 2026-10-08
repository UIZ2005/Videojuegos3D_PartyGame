using System.Collections;
using UnityEngine;

public class Diamante : MonoBehaviour
{
    public float relentizacion=2f;
    public float fuerzaSalida=10f;
    public float fuerzaSalidaup = 5f;
    public float tiempodeespera;
    private Vector3 escalaOriginal;
    private bool inplayer=false;
    private Transform playertransfomr;
    private Rigidbody rb;
    private BoxCollider col;

    private void Awake()
    {
        escalaOriginal = transform.localScale;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<BoxCollider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !inplayer)
        {
            other.GetComponent<movePlayer>().canvaObj.SetActive(true);
            col.isTrigger = true;
            rb.isKinematic = true;
            playertransfomr = other.transform;
            inplayer = true;
            Transform agarrediamante = other.GetComponent<movePlayer>().diamante;
            playertransfomr.gameObject.GetComponent<movePlayer>().speed -= relentizacion;
            playertransfomr.gameObject.GetComponent<movePlayer>().condiamante = true;
            transform.position = agarrediamante.position;
            transform.SetParent(agarrediamante);
            transform.localScale = escalaOriginal;
        }
    }

    //Modificacion aqui para que suelte correctamente el diamante con la mecanica de groundpound -suarez
    public void soltardiamante()
    {
        if (!inplayer || playertransfomr == null)
            return;
        playertransfomr.GetComponent<movePlayer>().canvaObj.SetActive(false);
        StartCoroutine(hitdiamon());
    }

    IEnumerator hitdiamon()
    {
        playertransfomr.gameObject.GetComponent<movePlayer>().condiamante = false;
        playertransfomr.gameObject.GetComponent<movePlayer>().speed += relentizacion;
        rb.isKinematic = false;
        col.isTrigger = false;
        Vector3 direccionsoltar =
               (-playertransfomr.forward * fuerzaSalida) +
               (Vector3.up * fuerzaSalidaup);
        transform.SetParent(null);
        rb.AddForce(direccionsoltar, ForceMode.Impulse);

        yield return new WaitForSeconds(tiempodeespera);

        inplayer = false;
        yield return null;
    }
}

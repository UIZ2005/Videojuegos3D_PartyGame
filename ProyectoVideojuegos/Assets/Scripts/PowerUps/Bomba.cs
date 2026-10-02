using UnityEngine;
using System.Collections;

public class Bomba : PowerUp
{
    [SerializeField] GameObject modelo, particulasExplosion;
    [SerializeField] float fuerzaExplosion;
    [SerializeField] float radioExplosion;
    [SerializeField] LayerMask capasExplosion;
    [SerializeField] float escalaPequena = 0.5f; 

    private Rigidbody rb;
    private Collider col;
    private bool estaSostenida = false;
    private bool fueLanzada = false;
    private bool yaExplotando = false;
    private Vector3 escalaOriginal;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        escalaOriginal = transform.localScale;
        transform.localScale = escalaOriginal * escalaPequena;
    }

    public override bool PuedeSerRecogido()
    {
        return !estaSostenida && !fueLanzada && !yaExplotando;
    }

    public override void SerRecogido(Transform puntoAgarre)
    {
        estaSostenida = true;
        rb.isKinematic = true; 
        col.isTrigger = true;  

        transform.position = puntoAgarre.position;
        transform.SetParent(puntoAgarre);
        transform.localScale = escalaOriginal; 
    }

    public override void Usar(Vector3 fuerzaParabola)
    {
        estaSostenida = false;
        fueLanzada = true;
        
        transform.SetParent(null);
        rb.isKinematic = false;
        col.isTrigger = false;
        
        rb.AddForce(fuerzaParabola, ForceMode.Impulse);
    }

    public void Explosion()
    {
        if (yaExplotando) return; 
        yaExplotando = true;

        if (particulasExplosion != null)
        {
            particulasExplosion.transform.SetParent(null);
            particulasExplosion.transform.localScale = Vector3.one; 
            particulasExplosion.SetActive(true);
            Destroy(particulasExplosion, 0.75f);
        }

        Collider[] objetosColisionados = Physics.OverlapSphere(
            transform.position, radioExplosion, capasExplosion
        );

        foreach (var item in objetosColisionados)
        {
            if (item.TryGetComponent(out Rigidbody rigidColisionado))
            {
                rigidColisionado.AddExplosionForce(
                    fuerzaExplosion, transform.position, radioExplosion, 2, ForceMode.Impulse
                );
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (fueLanzada && !yaExplotando)
        {
            Explosion(); 
        }
    }
}
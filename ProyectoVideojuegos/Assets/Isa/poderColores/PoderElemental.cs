using UnityEngine;
using System.Collections;

public abstract class PoderElemental : MonoBehaviour
{
    [SerializeField] protected string nombrePoder;
    [SerializeField] protected float cooldown = 4f;

    protected movePlayer jugador;
    protected Rigidbody rb;
    private bool disponible = true;

    public bool Disponible => disponible;
    public string ObtenerNombre() => nombrePoder;

    protected virtual void Awake()
    {
        jugador = GetComponentInParent<movePlayer>();
        rb = jugador.GetComponent<Rigidbody>();
    }

    public void IntentarActivar()
    {
        if (!disponible || !jugador.PuedeUsarPoder) return;
        StartCoroutine(Ciclo());
    }

    private IEnumerator Ciclo()
    {
        disponible = false;
        yield return Ejecutar();
        yield return new WaitForSeconds(cooldown);
        disponible = true;
    }

    // Lo que hace el poder (el cooldown empieza cuando termina)
    protected abstract IEnumerator Ejecutar();

    // Si el objeto se desactiva a mitad del poder, deja todo como estaba
    protected virtual void AlInterrumpir() { }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        disponible = true;
        AlInterrumpir();
    }
}

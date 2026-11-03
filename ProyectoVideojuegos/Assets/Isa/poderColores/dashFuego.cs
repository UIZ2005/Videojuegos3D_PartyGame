using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dashFuego : PoderElemental
{
    [Header("Dash")]
    [SerializeField] private float fuerzaDash = 30f;     // velocidad
    [SerializeField] private float duracionDash = 0.35f; 

    [Header("Golpe")]
    [SerializeField] private float radioGolpe = 1.2f;
    [SerializeField] private float fuerzaEmpuje = 14f;
    [SerializeField] private float fuerzaArriba = 4f;
    [SerializeField] private float tiempoAturdido = 0.6f;
    [SerializeField] private LayerMask capasGolpe;       

    [Header("Efectos")]
    [SerializeField] private GameObject particulasFuegoPrefab;
    [SerializeField] private GameObject impactoFuegoPrefab;
    [SerializeField] private AudioClip sonidoDash;

    private Diamante diamante;
    private GameObject llamas;
    private bool gravedadOriginal = true;
    private bool enDash = false;

    protected override void Awake()
    {
        base.Awake();
        diamante = FindAnyObjectByType<Diamante>();
    }

    protected override IEnumerator Ejecutar()
    {
        enDash = true;
        jugador.BloquearMovimiento(true);
        jugador.AnimacionTrigger("dash");

        if (sonidoDash != null)
            AudioSource.PlayClipAtPoint(sonidoDash, jugador.transform.position);

        if (particulasFuegoPrefab != null)
            llamas = Instantiate(particulasFuegoPrefab, jugador.transform.position,
                                 jugador.transform.rotation, jugador.transform);

        Vector3 direccion = jugador.DireccionInput();
        rb.MoveRotation(Quaternion.LookRotation(direccion, Vector3.up));

        // Vuela en línea recta: sin gravedad durante el dash
        gravedadOriginal = rb.useGravity;
        rb.useGravity = false;

        var golpeados = new HashSet<Rigidbody>(); // cada rival recibe un solo golpe
        float t = 0f;
        while (t < duracionDash)
        {
            rb.linearVelocity = direccion * fuerzaDash;
            Golpear(direccion, golpeados);

            yield return new WaitForFixedUpdate();
            t += Time.fixedDeltaTime;
        }

        rb.linearVelocity = direccion * jugador.speed; // sale sin frenazo
        Terminar();
    }

    private void Golpear(Vector3 direccion, HashSet<Rigidbody> golpeados)
    {
        Collider[] tocados = Physics.OverlapSphere(
            jugador.transform.position + Vector3.up * 0.5f, radioGolpe, capasGolpe);

        foreach (Collider col in tocados)
        {
            Rigidbody otro = col.attachedRigidbody;
            if (otro == null || otro == rb || golpeados.Contains(otro)) continue;
            golpeados.Add(otro);

            Vector3 lateral = otro.position - jugador.transform.position;
            lateral.y = 0f;
            Vector3 dirEmpuje = (direccion + lateral.normalized * 0.5f).normalized;
            Vector3 fuerza = dirEmpuje * fuerzaEmpuje + Vector3.up * fuerzaArriba;

            if (otro.TryGetComponent(out movePlayer victima))
            {
                if (victima.condiamante && diamante != null)
                    diamante.soltardiamante();

                victima.RecibirEmpuje(fuerza, tiempoAturdido);
            }
            else
            {
                otro.AddForce(fuerza, ForceMode.VelocityChange);
            }

            if (impactoFuegoPrefab != null)
            {
                GameObject impacto = Instantiate(impactoFuegoPrefab,
                    col.ClosestPoint(jugador.transform.position), Quaternion.identity);
                Destroy(impacto, 1.5f);
            }
        }
    }

    private void Terminar()
    {
        if (!enDash) return;
        enDash = false;

        rb.useGravity = gravedadOriginal;
        jugador.BloquearMovimiento(false);

        // Deja de emitir y deja que las llamas que ya salieron terminen solas
        if (llamas != null)
        {
            llamas.transform.SetParent(null);
            foreach (ParticleSystem ps in llamas.GetComponentsInChildren<ParticleSystem>())
            {
                var emission = ps.emission;
                emission.enabled = false;
            }
            Destroy(llamas, 2f);
            llamas = null;
        }
    }

    protected override void AlInterrumpir() => Terminar();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, radioGolpe);
    }
}

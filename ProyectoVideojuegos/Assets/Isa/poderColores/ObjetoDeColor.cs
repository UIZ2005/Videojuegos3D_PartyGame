using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ObjetoDeColor : MonoBehaviour
{
    [SerializeField] private ColorJuego colorAsignado;

    [Header("Aviso antes de reaparecer")]
    [SerializeField] private float tiempoParpadeo = 1f;
    [SerializeField] private float velocidadParpadeo = 0.1f;

    [Header("No reaparecer encima de un jugador (opcional)")]
    [SerializeField] private LayerMask capaJugadores;

    private static readonly List<ObjetoDeColor> todos = new List<ObjetoDeColor>();

    private Renderer[] renderers;
    private Collider[] colliders;
    private Bounds[] boundsGuardados;
    private Coroutine rutina;

    public ColorJuego ColorAsignado => colorAsignado;
    public bool EstaOculto { get; private set; }

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);
        boundsGuardados = new Bounds[colliders.Length];
    }

    void OnEnable()  => todos.Add(this);

    void OnDisable()
    {
        todos.Remove(this);
        if (EstaOculto) Restaurar();
    }

    // ===== Punto único de entrada: lo llama el power-up =====
    public static void OcultarColor(ColorJuego color, float duracion)
    {
        foreach (var obj in todos)
            if (obj.colorAsignado == color)
                obj.Ocultar(duracion);
    }

    public void Ocultar(float duracion)
    {
        // Si ya estaba oculto y alguien vuelve a usar el mismo color, se reinicia el tiempo
        if (rutina != null) StopCoroutine(rutina);
        rutina = StartCoroutine(RutinaOcultar(duracion));
    }

    private IEnumerator RutinaOcultar(float duracion)
    {
        for (int i = 0; i < colliders.Length; i++)
            if (colliders[i].enabled) boundsGuardados[i] = colliders[i].bounds;

        EstaOculto = true;
        SetVisible(false);
        SetColliders(false);

        yield return new WaitForSeconds(Mathf.Max(0f, duracion - tiempoParpadeo));

        // Parpadeo de aviso: se ve, pero todavía no es sólido
        float t = 0f;
        bool visible = false;
        while (t < tiempoParpadeo)
        {
            visible = !visible;
            SetVisible(visible);
            yield return new WaitForSeconds(velocidadParpadeo);
            t += velocidadParpadeo;
        }

        // Espera a que ningún jugador esté dentro del espacio del objeto
        SetVisible(true);
        while (HayJugadorDentro())
            yield return new WaitForSeconds(0.1f);

        Restaurar();
    }

    private void Restaurar()
    {
        SetVisible(true);
        SetColliders(true);
        EstaOculto = false;
        rutina = null;
    }

    private bool HayJugadorDentro()
    {
        if (capaJugadores.value == 0) return false;
        foreach (var b in boundsGuardados)
        {
            if (b.size == Vector3.zero) continue;
            if (Physics.CheckBox(b.center, b.extents, Quaternion.identity,
                                 capaJugadores, QueryTriggerInteraction.Ignore))
                return true;
        }
        return false;
    }

    private void SetVisible(bool v)   { foreach (var r in renderers) r.enabled = v; }
    private void SetColliders(bool v) { foreach (var c in colliders) c.enabled = v; }
}

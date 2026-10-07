using UnityEngine;
using UnityEngine.InputSystem;

public class PowerUpColor : PowerUp
{
    [SerializeField] private GameObject modelo;
    [SerializeField] private float duracionDesaparicion = 5f;

    private Rigidbody rb;
    private Collider col;
    private RuedaColoresUI rueda;

    private bool recogido = false;
    private bool ruedaAbierta = false;
    private bool usado = false;


    public AudioSource fuenteAudio;   // AudioSource en este mismo objeto
     public AudioClip sonidoRecoger;

    public override bool SigueEnManoTrasUsar() => !usado;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public override bool PuedeSerRecogido()
    {
        return !recogido && !usado;
    }

    public override void SerRecogido(Transform puntoAgarre)
    {
        recogido = true;


        if (rb != null) rb.isKinematic = true;
        if (col != null) col.enabled = false;
        if (modelo != null) modelo.SetActive(true); // no se sostiene en la mano como la bomba

        transform.SetParent(puntoAgarre);
        transform.localPosition = Vector3.zero;

        fuenteAudio.PlayOneShot(sonidoRecoger);


        // Busca la rueda del jugador que lo recogió
        var playerInput = puntoAgarre.GetComponentInParent<PlayerInput>();
        if (playerInput != null)
            rueda = playerInput.GetComponentInChildren<RuedaColoresUI>(true);

        if (rueda == null)
            Debug.LogWarning("PowerUpColor: el jugador no tiene una RuedaColoresUI en su prefab.");
    }

    //usar es para abrir la rueda
    public override void Usar(Vector3 direccionUso)
    {
        if (!recogido || ruedaAbierta || usado || rueda == null) return;

        ruedaAbierta = true;
        if (modelo != null) modelo.SetActive(false); // no se sostiene en la mano como la bomba
        rueda.Abrir(AlConfirmarColor, AlCancelar);
    }

    private void AlConfirmarColor(ColorJuego color)
    {
        ruedaAbierta = false;
        usado = true;

        ObjetoDeColor.OcultarColor(color, duracionDesaparicion);
        Destroy(gameObject);
    }

    private void AlCancelar()
    {
        ruedaAbierta = false; // conserva el poder para usarlo después
    }

    void OnDestroy()
    {
        if (ruedaAbierta && rueda != null) rueda.Cerrar();
    }
}

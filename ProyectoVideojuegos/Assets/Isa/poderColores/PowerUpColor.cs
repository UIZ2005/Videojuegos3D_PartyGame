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
    private Vector3 escalaOriginal;

    public AudioSource fuenteAudio;   
    public AudioClip sonidoRecoger;

    public override bool SigueEnManoTrasUsar() => !usado;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        escalaOriginal = transform.localScale;
    }

    protected override void Start()
    {
        // Inicia la animación de respirar
        base.Start();
    }

    public override bool PuedeSerRecogido()
    {
        return !recogido && !usado;
    }

    public override void SerRecogido(Transform puntoAgarre)
    {
        recogido = true;
        apareciendo = false; // APAGAMOS LA ANIMACIÓN

        if (rb != null) rb.isKinematic = true;
        if (col != null) col.enabled = false;
        if (modelo != null) modelo.SetActive(true); 

        transform.SetParent(puntoAgarre);
        transform.localPosition = Vector3.zero;
        transform.localScale = escalaOriginal; // Restauramos escala estable

        fuenteAudio.PlayOneShot(sonidoRecoger);

        var playerInput = puntoAgarre.GetComponentInParent<PlayerInput>();
        if (playerInput != null)
            rueda = playerInput.GetComponentInChildren<RuedaColoresUI>(true);

        if (rueda == null)
            Debug.LogWarning("PowerUpColor: el jugador no tiene una RuedaColoresUI en su prefab.");
    }

    public override void Usar(Vector3 direccionUso)
    {
        if (!recogido || ruedaAbierta || usado || rueda == null) return;

        ruedaAbierta = true;
        if (modelo != null) modelo.SetActive(false); 
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
        ruedaAbierta = false; 
    }

    void OnDestroy()
    {
        if (ruedaAbierta && rueda != null) rueda.Cerrar();
    }
}
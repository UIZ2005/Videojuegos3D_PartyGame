using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Va en el objeto "RuedaColores" dentro del Canvas del jugador (SIEMPRE activo).
// La rueda es un anillo que rodea al jugador en su propia pantalla.
public class RuedaColoresUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private RectTransform contenedorRueda;
    [SerializeField] private RectTransform indicador;   // opcional: flecha que apunta al color
    [SerializeField] private Sprite spriteAnillo;       // opcional: si está vacío se genera solo

    [Header("Seguir al jugador")]
    [Tooltip("Punto del cuerpo que queda en el centro del anillo. Si está vacío usa el objeto del PlayerInput.")]
    [SerializeField] private Transform objetivo;
    [Tooltip("Desplazamiento desde el objetivo (ej: Y = 1 para el pecho si el pivote está en los pies).")]
    [SerializeField] private Vector3 desplazamiento = new Vector3(0f, 1f, 0f);

    [Header("Apariencia")]
    [SerializeField] private float radio = 180f;
    [SerializeField, Range(0.1f, 0.9f)] private float grosorAnillo = 0.3f;
    [SerializeField, Range(0f, 10f)] private float separacionGrados = 4f;
    [SerializeField] private float escalaSeleccionado = 1.1f;
    [SerializeField, Range(0f, 1f)] private float alfaNoSeleccionado = 0.5f;
    [SerializeField] private float suavizado = 15f;

    [Header("Nombres de las acciones (mismo Action Map del jugador)")]
    [SerializeField] private string accionApuntar = "ApuntarRueda";
    [SerializeField] private string accionGirar = "GirarRueda";
    [SerializeField] private string accionConfirmar = "ConfirmarColor";
    [SerializeField] private string accionCancelar = "CancelarRueda";
    [SerializeField] private string accionCamara = "Look"; // se bloquea mientras la rueda está abierta
    [SerializeField] private float zonaMuertaStick = 0.5f;
    [SerializeField] private float pausaEntrePasosScroll = 0.08f;

    private InputAction apuntar, girar, confirmar, cancelar, camara;
    private Canvas canvas;
    private Camera camaraJugador;
    private Image[] segmentos;
    private Image imagenIndicador;
    private int indice = 0;
    private bool abierta = false;
    private float siguientePasoScroll;
    private int frameApertura; // evita que el mismo clic abra y confirme
    private Action<ColorJuego> onConfirmar;
    private Action onCancelar;

    public bool EstaAbierta => abierta;

    void Awake()
    {
        if (playerInput == null) playerInput = GetComponentInParent<PlayerInput>();
        if (objetivo == null && playerInput != null) objetivo = playerInput.transform;

        var acciones = playerInput.actions;
        apuntar = acciones.FindAction(accionApuntar);
        girar = acciones.FindAction(accionGirar);
        confirmar = acciones.FindAction(accionConfirmar);
        cancelar = acciones.FindAction(accionCancelar);
        camara = string.IsNullOrEmpty(accionCamara) ? null : acciones.FindAction(accionCamara);

        ConfigurarCanvas();

        if (indicador != null) imagenIndicador = indicador.GetComponentInChildren<Image>(true);

        if (spriteAnillo == null) spriteAnillo = CrearSpriteAnillo(512, grosorAnillo);
        ConstruirSegmentos();
        contenedorRueda.gameObject.SetActive(false);
    }

    // Busca la cámara de ESTE jugador y se la asigna al Canvas
    private void ConfigurarCanvas()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("RuedaColoresUI debe estar dentro de un Canvas.");
            return;
        }

        camaraJugador = canvas.worldCamera;
        if (camaraJugador == null && playerInput != null) camaraJugador = playerInput.camera;
        if (camaraJugador == null)
        {
            var mp = GetComponentInParent<movePlayer>();
            if (mp != null) camaraJugador = mp.camara;
        }

        if (camaraJugador == null)
        {
            Debug.LogError("RuedaColoresUI: no se encontró la cámara del jugador. Asígnala en el Canvas (Render Camera).");
            return;
        }

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camaraJugador;
    }

    private void ConstruirSegmentos()
    {
        int n = ColorJuegoUtil.Cantidad;
        float anguloSeg = 360f / n;
        segmentos = new Image[n];

        for (int i = 0; i < n; i++)
        {
            var go = new GameObject("Segmento_" + (ColorJuego)i, typeof(RectTransform), typeof(Image));
            var rt = (RectTransform)go.transform;
            rt.SetParent(contenedorRueda, false);
            rt.sizeDelta = Vector2.one * radio * 2f;
            rt.anchoredPosition = Vector2.zero;
            // Segmento i ocupa de i*anguloSeg a (i+1)*anguloSeg, en sentido horario desde arriba
            rt.localEulerAngles = new Vector3(0f, 0f, -(i * anguloSeg + separacionGrados * 0.5f));

            var img = go.GetComponent<Image>();
            img.sprite = spriteAnillo;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Radial360;
            img.fillOrigin = (int)Image.Origin360.Top;
            img.fillClockwise = true;
            img.fillAmount = (anguloSeg - separacionGrados) / 360f;
            img.color = ColorJuegoUtil.AColor((ColorJuego)i);
            img.raycastTarget = false;

            segmentos[i] = img;
        }

        if (indicador != null) indicador.SetAsLastSibling();
    }

    // Genera un anillo blanco con bordes suaves (el centro queda transparente)
    private static Sprite CrearSpriteAnillo(int tam, float grosor)
    {
        var tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float r = tam * 0.5f;
        float rInterior = r * (1f - grosor);
        var pixeles = new Color32[tam * tam];

        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                float dx = x + 0.5f - r;
                float dy = y + 0.5f - r;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(r - d) * Mathf.Clamp01(d - rInterior);
                pixeles[y * tam + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
        }

        tex.SetPixels32(pixeles);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tam, tam), new Vector2(0.5f, 0.5f), 100f);
    }

    public void Abrir(Action<ColorJuego> alConfirmar, Action alCancelar)
    {
        frameApertura = Time.frameCount;
        onConfirmar = alConfirmar;
        onCancelar = alCancelar;
        abierta = true;
        contenedorRueda.gameObject.SetActive(true);
        camara?.Disable();
        PosicionarSobreJugador();
        ActualizarVisual(true);
    }

    public void Cerrar()
    {
        abierta = false;
        contenedorRueda.gameObject.SetActive(false);
        camara?.Enable();
        onConfirmar = null;
        onCancelar = null;
    }

    void Update()
    {
        if (!abierta) return;
        int n = segmentos.Length;

        // MANDO: el stick derecho apunta directamente al color
        if (apuntar != null)
        {
            Vector2 v = apuntar.ReadValue<Vector2>();
            if (v.sqrMagnitude > zonaMuertaStick * zonaMuertaStick)
            {
                float ang = Mathf.Atan2(v.x, v.y) * Mathf.Rad2Deg; // 0° = arriba, sentido horario
                if (ang < 0f) ang += 360f;
                indice = Mathf.FloorToInt(ang / (360f / n)) % n;
            }
        }

        // MOUSE: cada movimiento de la rueda avanza o retrocede un color
        if (girar != null && Time.unscaledTime >= siguientePasoScroll)
        {
            float s = girar.ReadValue<float>();
            if (Mathf.Abs(s) > 0.01f)
            {
                indice = s > 0f ? (indice - 1 + n) % n : (indice + 1) % n;
                siguientePasoScroll = Time.unscaledTime + pausaEntrePasosScroll;
            }
        }

        if (confirmar != null && Time.frameCount > frameApertura && confirmar.WasPressedThisFrame())
        {
            var callback = onConfirmar;
            var elegido = (ColorJuego)indice;
            Cerrar();
            callback?.Invoke(elegido);
            return;
        }

        if (cancelar != null && cancelar.WasPressedThisFrame())
        {
            var callback = onCancelar;
            Cerrar();
            callback?.Invoke();
            return;
        }

        ActualizarVisual(false);
    }

    // LateUpdate: después de que el jugador y la cámara se movieron
    void LateUpdate()
    {
        if (abierta) PosicionarSobreJugador();
    }

    private void PosicionarSobreJugador()
    {
        if (objetivo == null || camaraJugador == null) return;

        Vector3 pantalla = camaraJugador.WorldToScreenPoint(objetivo.position + desplazamiento);
        if (pantalla.z < 0f) return; // el jugador quedó detrás de la cámara

        var padre = (RectTransform)contenedorRueda.parent;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(padre, pantalla, camaraJugador, out Vector2 local))
            contenedorRueda.anchoredPosition = local;
    }

    private void ActualizarVisual(bool instantaneo)
    {
        int n = segmentos.Length;
        float k = instantaneo ? 1f : 1f - Mathf.Exp(-suavizado * Time.unscaledDeltaTime);
        Color colorElegido = ColorJuegoUtil.AColor((ColorJuego)indice);

        for (int i = 0; i < n; i++)
        {
            bool sel = i == indice;
            var rt = segmentos[i].rectTransform;
            rt.localScale = Vector3.Lerp(rt.localScale, Vector3.one * (sel ? escalaSeleccionado : 1f), k);

            Color c = ColorJuegoUtil.AColor((ColorJuego)i);
            c.a = sel ? 1f : alfaNoSeleccionado;
            segmentos[i].color = Color.Lerp(segmentos[i].color, c, k);
        }

        if (indicador != null)
        {
            float objetivoZ = -(indice + 0.5f) * (360f / n);
            float z = Mathf.LerpAngle(indicador.localEulerAngles.z, objetivoZ, k);
            indicador.localEulerAngles = new Vector3(0f, 0f, z);

            if (imagenIndicador != null)
                imagenIndicador.color = Color.Lerp(imagenIndicador.color, colorElegido, k);
        }
    }
}
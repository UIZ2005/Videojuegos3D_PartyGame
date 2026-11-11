using UnityEngine;

public abstract class PowerUp : MonoBehaviour
{
    [SerializeField] protected string nombrePoder;

    [Header("Animación de Aparición")]
    public float duracionAnimacion = 2f;
    
    protected bool apareciendo = true;
    protected Vector3 escalaFinal;  // Guardará la escala exacta del prefab

    private float tiempoActual = 0f;

    protected virtual void Start()
    {
        // Guardamos la escala actual que tenga el objeto (su tamaño final)
        escalaFinal = transform.localScale;
        
        // Lo hacemos invisible (escala 0) para que empiece a crecer
        transform.localScale = Vector3.zero;
        apareciendo = true;
        tiempoActual = 0f;
    }

    protected virtual void Update()
    {
        if (apareciendo)
        {
            tiempoActual += Time.deltaTime;
            
            // Calculamos el progreso del 0 al 1
            float progreso = tiempoActual / duracionAnimacion;
            
            // Hacemos que el crecimiento sea suave (empieza rápido y frena al final)
            float progresoSuave = Mathf.SmoothStep(0f, 1f, progreso);
            
            // Interpolar de 0 a su escala final original
            transform.localScale = Vector3.Lerp(Vector3.zero, escalaFinal, progresoSuave);

            // Si ya llegó al 100%, detenemos la animación
            if (progreso >= 1f)
            {
                transform.localScale = escalaFinal;
                apareciendo = false;
            }
        }
    }

    public string ObtenerNombre()
    {
        return nombrePoder;
    }

    // Métodos obligatorios para los hijos
    public abstract bool PuedeSerRecogido();
    public abstract void SerRecogido(Transform puntoAgarre);
    public abstract void Usar(Vector3 direccionUso);
    public virtual bool SigueEnManoTrasUsar() => false;
}
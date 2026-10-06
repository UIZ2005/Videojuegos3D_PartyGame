using UnityEngine;

public abstract class PowerUp : MonoBehaviour
{
    [SerializeField] protected string nombrePoder;

    public string ObtenerNombre()
    {
        return nombrePoder;
    }

    // Métodos de cada power Up
    public abstract bool PuedeSerRecogido();
    public abstract void SerRecogido(Transform puntoAgarre);
    
    // Recibe un Vector3 porque la bomba lo usa como fuerza, el gancho como dirección.
    public abstract void Usar(Vector3 direccionUso);


    // para los colroes, 
    public virtual bool SigueEnManoTrasUsar() => false;
}


using System.Collections;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    public Transform[] spawns;

    private void OnTriggerEnter(Collider other)
    {
        movePlayer jugador = other.GetComponentInParent<movePlayer>();

        if (jugador == null)
            return;

        reAparecer(jugador);
    }

    public void reAparecer(movePlayer jugador)
    {
        jugador.activarquieto();
        if (spawns == null || spawns.Length == 0)
        {
            Debug.LogWarning("No hay puntos de respawn asignados.");
            return;
        }
        int indiceAleatorio = Random.Range(0, spawns.Length);
        Transform spawnSeleccionado = spawns[indiceAleatorio];

        if (spawnSeleccionado == null)
            return;

        Rigidbody rb = jugador.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            jugador.transform.SetPositionAndRotation(
                spawnSeleccionado.position,
                spawnSeleccionado.rotation
            );
        }
        StartCoroutine(apareciendo(jugador, spawnSeleccionado));
    }
    IEnumerator apareciendo(movePlayer jugador, Transform spawnSeleccionado)
    {
        jugador.transform.SetPositionAndRotation(
                spawnSeleccionado.position,
                spawnSeleccionado.rotation
            );
        yield return new WaitForSeconds(2f);
        jugador.desactivarquieto();

        Rigidbody rb = jugador.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
        }
        yield return null;
    }
}

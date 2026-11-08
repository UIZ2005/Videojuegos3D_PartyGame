using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WalkerSpawner : MonoBehaviour
{
    public GameObject[] walkerPrefabs;   // prefabs desde Project
    public Transform startCube;          // donde nacen
    public Transform endCube;            // donde desaparecen
    public float minDelay = 1f;          // pausa entre un NPC y el siguiente
    public float maxDelay = 3f;

    GameObject current;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnWalker();

            // espera hasta que el NPC actual desaparezca (llegó al final o lo clicaste)
            while (current != null)
                yield return null;

            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        }
    }

    void SpawnWalker()
    {
        if (walkerPrefabs == null || walkerPrefabs.Length == 0) return;
        if (startCube == null || endCube == null) return;

        GameObject prefab = walkerPrefabs[Random.Range(0, walkerPrefabs.Length)];
        if (prefab == null) return;

        current = Instantiate(prefab, startCube.position, prefab.transform.rotation);

        AnimInicioPlayersd walker = current.GetComponent<AnimInicioPlayersd>();
        if (walker != null)
            walker.target = endCube.position;
    }

    void Update()
    {
        if (current == null || cam == null) return;
        if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame) return;

        Vector2 click = Pointer.current.position.ReadValue();
        if (ClickedOn(current, click))
        {
            AnimInicioPlayersd walker = current.GetComponent<AnimInicioPlayersd>();
            if (walker != null)
                walker.Explode();
            else
                Destroy(current);
        }

    // true si el clic cae dentro del área que ocupa el modelo en pantalla
    bool ClickedOn(GameObject npc, Vector2 screenPos)
    {
        Renderer[] renderers = npc.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return false;

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            b.Encapsulate(renderers[i].bounds);

        Vector3 min = b.min, max = b.max;
        Vector2 sMin = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 sMax = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new Vector3(
                (i & 1) == 0 ? min.x : max.x,
                (i & 2) == 0 ? min.y : max.y,
                (i & 4) == 0 ? min.z : max.z);

            Vector3 sp = cam.WorldToScreenPoint(corner);
            if (sp.z < 0) continue; // detrás de la cámara

            sMin = Vector2.Min(sMin, sp);
            sMax = Vector2.Max(sMax, sp);
        }

        return screenPos.x >= sMin.x && screenPos.x <= sMax.x &&
               screenPos.y >= sMin.y && screenPos.y <= sMax.y;
    }
}
}
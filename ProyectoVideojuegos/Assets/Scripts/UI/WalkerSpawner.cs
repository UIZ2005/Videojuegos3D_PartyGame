using System.Collections;
using UnityEngine;

public class WalkerSpawner : MonoBehaviour
{
    public GameObject[] walkerPrefabs;   // prefabs desde Project
    public Transform startCube;          // donde nacen
    public Transform endCube;            // donde desaparecen
    public float minInterval = 2f;
    public float maxInterval = 5f;

    IEnumerator Start()
    {
        while (true)
        {
            SpawnWalker();
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        }
    }

    void SpawnWalker()
    {
        if (walkerPrefabs == null || walkerPrefabs.Length == 0) return;
        if (startCube == null || endCube == null) return;

        GameObject prefab = walkerPrefabs[Random.Range(0, walkerPrefabs.Length)];
        if (prefab == null) return;

        GameObject npc = Instantiate(prefab, startCube.position, prefab.transform.rotation);

        AnimInicioPlayersd walker = npc.GetComponent<AnimInicioPlayersd>();
        if (walker != null)
            walker.target = endCube.position;
    }
}
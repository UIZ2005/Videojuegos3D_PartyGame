using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingStarSpawner : MonoBehaviour
{
    public GameObject[] stars;      // los 3 objetos PADRE de las estrellas
    public float minDelay = 2f;
    public float maxDelay = 8f;

    List<int> bag = new List<int>();
    int lastIndex = -1;

    IEnumerator Start()
    {
        if (stars == null || stars.Length == 0) yield break;

        for (int i = 0; i < stars.Length; i++)
        {
            Debug.Log("Espacio " + i + ": " + (stars[i] != null ? stars[i].name : "VACIO"));
            if (stars[i] != null) stars[i].SetActive(false);
        }

        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            int index = NextIndex();
            if (index < 0) continue;

            GameObject star = stars[index];
            Debug.Log("Lanzando espacio " + index + ": " + star.name);

            star.SetActive(true);
            ParticleSystem[] systems = star.GetComponentsInChildren<ParticleSystem>(true);

            foreach (var ps in systems)
            {
                ps.Clear(true);
                ps.Play(false);
            }
            yield return null;

            // espera a que terminen el meteorito y el aura
            bool alive = true;
            while (alive)
            {
                alive = false;
                foreach (var ps in systems)
                    if (ps.IsAlive(false)) { alive = true; break; }
                yield return null;
            }

            star.SetActive(false);
        }
    }

    int NextIndex()
    {
        if (bag.Count == 0)
        {
            for (int i = 0; i < stars.Length; i++)
                if (stars[i] != null) bag.Add(i);

            for (int i = 0; i < bag.Count; i++)
            {
                int j = Random.Range(i, bag.Count);
                int tmp = bag[i]; bag[i] = bag[j]; bag[j] = tmp;
            }

            if (bag.Count > 1 && bag[0] == lastIndex)
            {
                int tmp = bag[0]; bag[0] = bag[bag.Count - 1]; bag[bag.Count - 1] = tmp;
            }
        }

        if (bag.Count == 0) return -1;

        int next = bag[0];
        bag.RemoveAt(0);
        lastIndex = next;
        return next;
    }
}
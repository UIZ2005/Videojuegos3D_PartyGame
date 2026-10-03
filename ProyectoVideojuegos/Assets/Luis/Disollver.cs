using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Disollver : MonoBehaviour
{
    public float disolverDuration = 5f;
    public float disolverStrength;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartDisolve();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            StartApper();
        }
    }

    private void StartDisolve()
    {
        StartCoroutine(Dissolve());
    }

    private void StartApper()
    {
        StartCoroutine(Apper());
    }

    IEnumerator Dissolve()
    {
        float elapsedTime = 0f;

        Material disolveMaterial = GetComponent<Renderer>().material;

        while (elapsedTime < disolverDuration)
        {
            elapsedTime += Time.deltaTime;

            disolverStrength = Mathf.Lerp(0f, 1f, elapsedTime / disolverDuration);
            disolveMaterial.SetFloat("_Escala_Disolver", disolverStrength);

            yield return null;
        }

       
    }
    IEnumerator Apper()
    {
        float elapsedTime = 0f;

        Material disolveMaterial = GetComponent<Renderer>().material;

        while (elapsedTime < disolverDuration)
        {
            elapsedTime += Time.deltaTime;

            disolverStrength = Mathf.Lerp(1f, 0f, elapsedTime / disolverDuration);
            disolveMaterial.SetFloat("_Escala_Disolver", disolverStrength);

            yield return null;
        }



        
    }
}

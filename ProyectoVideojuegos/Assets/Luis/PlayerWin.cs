using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerWin : MonoBehaviour
{
    public GameObject canvaswin;
    void Start()
    {
        
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<movePlayer>().condiamante)
            {
                canvaswin.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                StartCoroutine(empiezaVictoria());
                
            }
        }
    }

    IEnumerator empiezaVictoria()
    {

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("MenuInteractivoInicio");

        yield return null;
    }
}

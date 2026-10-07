using UnityEngine;

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
            }
        }
    }
}

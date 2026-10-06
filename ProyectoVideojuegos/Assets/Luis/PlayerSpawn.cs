using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerSpawn : MonoBehaviour
{
    public Transform[] SpawnPoints;
    public int m_playerCount;
    //private audiomanager audiomanager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //audiomanager = FindAnyObjectByType<audiomanager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.transform.position = SpawnPoints[m_playerCount].transform.position;

        playerInput.GetComponentInChildren<CinemachineInputAxisController>().PlayerIndex = m_playerCount;
        m_playerCount++;
        //audiomanager.seleccionAudio(3);
        //playerInput.GetComponent<movePlayer>().skinplayer(m_playerCount);
    }
}

using UnityEngine;

public class audiomanager : MonoBehaviour
{
    [SerializeField] private AudioClip[] audios;
    private AudioSource AudioSource;
    void Start()
    {
        AudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    public void seleccionAudio(int Indice)
    {
        AudioSource.PlayOneShot(audios[Indice]);
    }
    public void pausaAudio()
    {
        AudioSource.Pause();
    }
}
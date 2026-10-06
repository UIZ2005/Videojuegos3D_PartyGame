using Unity.Cinemachine;
using UnityEngine;

public class camerafocus : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CinemachineBrain brain;
    public ICinemachineCamera CamA;
    public ICinemachineCamera CamB;
    void Start()
    {
        CamA = GetComponent<CinemachineCamera>();
        CamB = GetComponent<CinemachineCamera>();

        int layer = 1;
        int priority = 1;
        float weight = 1f;
        float blendTime = 0f;

        brain.SetCameraOverride(layer, priority, CamA, CamB, weight, blendTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

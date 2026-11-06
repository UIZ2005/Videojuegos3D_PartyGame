using UnityEngine;

public class transform_ObjectDistance : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Shader.SetGlobalVector("_ObjectPosition", new Vector4(this.transform.position.x, this.transform.position.y, this.transform.position.z,this.transform.localScale.x));  
    }
}

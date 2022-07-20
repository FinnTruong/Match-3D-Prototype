using UnityEngine;

public class ShaderInteractor : MonoBehaviour
{
    public Renderer renderer;
    // Update is called once per frame
    void Update()
    {
        renderer.material.SetVector("_PositionMoving", transform.position);
        //Shader.SetGlobalVector("_PositionMoving", transform.position);
        Debug.Log("Set Pos");
    }
}

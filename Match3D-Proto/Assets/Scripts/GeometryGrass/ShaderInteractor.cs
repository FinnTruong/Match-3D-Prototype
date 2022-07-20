using UnityEngine;

public class ShaderInteractor : MonoBehaviour
{
    public Renderer renderer;
    public Camera mainCam;
    public float mousePosOffset;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            var mousePos = Input.mousePosition;
            mousePos.z = mainCam.nearClipPlane + mousePosOffset;
            var worldPos = mainCam.ScreenToWorldPoint(mousePos);
            worldPos.y = -2.5f;
            renderer.material.SetVector("_PositionMoving", worldPos);
        }
    }
}

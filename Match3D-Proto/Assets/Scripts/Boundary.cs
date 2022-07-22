using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Anchor
{
    Top,
    Bottom,
    Left,
    Right
}

public class Boundary : MonoBehaviour
{
    public Anchor anchor;
    public float yPos;
    // Start is called before the first frame update

    void Start()
    {

    }
    private void OnValidate()
    {
        SetPosition();
    }
    // Update is called once per frame
    void Update()
    {

    }

    void SetPosition()
    {
        var anchorPos = GetAnchor();
        transform.position = anchorPos;
    }


    Vector3 GetAnchor()
    {
        Vector3 anchorPos = Camera.main.ScreenToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane));
        switch (anchor)
        {
            case Anchor.Top:
                anchorPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width/2f, 0f, Camera.main.nearClipPlane));
                break;
            case Anchor.Bottom:
                anchorPos = Camera.main.ScreenToWorldPoint(new Vector3(0.5f, 0, Camera.main.nearClipPlane));
                break;
            case Anchor.Left:
                anchorPos = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height/2f, Camera.main.nearClipPlane));
                Debug.Log(anchorPos);
                break;
            case Anchor.Right:
                anchorPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height/2f, Camera.main.nearClipPlane));
                break;
            default:
                break;
        }

        return anchorPos;
    }
}

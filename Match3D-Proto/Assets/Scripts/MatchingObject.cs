using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class MatchingObject : MonoBehaviour
{
    private Rigidbody rigidbody;
    public Renderer renderer;
    private Vector3 oldMousePos;

    [Space]
    [Header("Movement Variables")]
    public float moveSpeed = 200f;
    public float force = 1.5f;
    public float rotateForce = 1f;
    public float height = 1f;


    public bool isInRange;
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        renderer = GetComponent<Renderer>();
        //renderer.material.DisableKeyword("_EMISSION");
    }

    #region MOVEMENT LOGIC
    void OnMouseDown()
    {
        //renderer.material.EnableKeyword("_EMISSION");
        rigidbody.isKinematic = true;
    }


    void OnMouseDrag()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane + height;
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(mousePos);
        transform.position = Vector3.MoveTowards(transform.position, curPosition, moveSpeed * Time.deltaTime);
        oldMousePos = Input.mousePosition;
    }

    void OnMouseUp()
    {
        OnRelease();
    }
    #endregion

    void OnRelease()
    {
        if(isInRange)
        {
            //DO STUFF
        }
        else
        {
            //renderer.material.DisableKeyword("_EMISSION");
            var mousePos = Input.mousePosition;
            rigidbody.isKinematic = false;
            rigidbody.velocity = Vector3.zero;
            var delta = mousePos - oldMousePos;
            var throwDir = new Vector3(delta.x, 0, delta.y);
            var rotateDir = new Vector3(delta.y, 0, -delta.x);
            rigidbody.AddForce(throwDir * force, ForceMode.Impulse);
            rigidbody.AddTorque(rotateDir * rotateForce);
            //var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //var dir = mousePos - transform.position;
            //dir.Normalize();
            //rigidbody.velocity = dir * speed;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("MatchChecker"))
        {
            isInRange = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("MatchChecker"))
        {
            isInRange = false;
        }
    }

    IEnumerator Squish()
    {
        Vector3 startScale = transform.localScale;
        float percent = 0f;
        while(percent<1f)
        {
            //transform.localScale = Vector3.Lerp(startScale, )
            yield return null;
            
        }
    }
}

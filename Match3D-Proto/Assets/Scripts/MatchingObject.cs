using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class MatchingObject : MonoBehaviour
{
    private Rigidbody rigidbody;
    private Vector3 oldMousePos;

    public float moveSpeed = 200f;
    public float force = 1.5f;
    public float rotateForce = 1f;
    public float height = 1f;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void OnMouseDown()
    {
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
        var mousePos = Input.mousePosition;
        rigidbody.isKinematic = false;
        rigidbody.velocity = Vector3.zero;
        var delta = mousePos - oldMousePos;
        var throwDir = new Vector3(delta.x, 0, delta.y);
        var rotateDir = new Vector3(delta.y, 0, -delta.x);
        rigidbody.AddForce(throwDir * force, ForceMode.Impulse);
        rigidbody.AddTorque(rotateDir * rotateForce, ForceMode.Impulse);
        //var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //var dir = mousePos - transform.position;
        //dir.Normalize();
        //rigidbody.velocity = dir * speed;
    }
}

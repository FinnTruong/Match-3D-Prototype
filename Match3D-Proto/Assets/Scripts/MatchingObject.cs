using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class MatchingObject : MonoBehaviour
{
    private Rigidbody rigidbody;
    public Renderer renderer;
    private Vector3 oldMousePos;
    private MatchingManager matchingManager;

    [Space]
    [Header("Movement Variables")]
    public float moveSpeed = 200f;
    public float throwForce = 1.5f;
    public float maxThrowDirLength = 2f;
    public float rotateForce = 1f;
    public float expelForce = 50f;
    public float height = 1f;


    public bool isInRange;
    public bool isChecking;
    public bool triggerThrow;
    public bool triggerExpel;

    private Vector3 throwDir;
    private Vector3 rotateDir;
    void Start()
    {
        matchingManager = MatchingManager.Instance;
        rigidbody = GetComponent<Rigidbody>();
        renderer = GetComponent<Renderer>();
        //renderer.material.DisableKeyword("_EMISSION");
    }

    private void FixedUpdate()
    {
        if(triggerThrow)
        {
            triggerThrow = false;
            rigidbody.AddForce(throwDir * throwForce, ForceMode.Impulse);
            rigidbody.AddTorque(rotateDir * rotateForce);
        }

        if(triggerExpel)
        {
            triggerExpel = false;
            rigidbody.AddForce(Vector3.forward * expelForce + Vector3.up, ForceMode.Impulse);
            rigidbody.AddTorque(Vector3.forward * rotateForce);
        }
    }

    #region MOVEMENT LOGIC
    void OnMouseDown()
    {        
        //renderer.material.EnableKeyword("_EMISSION");
        rigidbody.isKinematic = true;
        if (isChecking)
        {
            switch (matchingManager.state)
            {
                case MatchingState.Empty:
                    break;
                case MatchingState.Half:
                    isChecking = false;
                    matchingManager.state = MatchingState.Empty;
                    matchingManager.leftObject = null;
                    break;
                case MatchingState.Full:
                    isChecking = false;
                    matchingManager.state = MatchingState.Half;
                    matchingManager.rightObject = null;
                    break;
                default:
                    break;
            }
        }
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
        if(matchingManager.IsInRange(transform.position) && !isChecking)
        {
            switch (matchingManager.state)
            {
                case MatchingState.Empty:
                    transform.DOMove(matchingManager.leftPos.position, 0.3f);
                    transform.DORotate(Vector3.zero, 0.3f);
                    matchingManager.state = MatchingState.Half;
                    matchingManager.leftObject = this;
                    isChecking = true;
                    break;
                case MatchingState.Half:
                    if(matchingManager.IsMatch(this.gameObject.name))
                    {
                        transform.DOMove(matchingManager.rightPos.position, 0.3f);
                        transform.DORotate(Vector3.zero, 0.3f).OnComplete(() => matchingManager.OnMatch());
                        matchingManager.state = MatchingState.Full;
                        matchingManager.rightObject = this;
                    }
                    else
                    {
                        matchingManager.leftObject.transform.DOScale(matchingManager.leftObject.transform.localScale - Vector3.one * 1.2f, 0.12f).SetLoops(2, LoopType.Yoyo);
                        rigidbody.isKinematic = false;
                        rigidbody.velocity = Vector3.zero;
                        triggerExpel = true;
                    }
                    break;
                case MatchingState.Full:

                    break;
                default:
                    break;
            }
        }
        else
        {
            isChecking = false;
            var mousePos = Input.mousePosition;
            rigidbody.isKinematic = false;
            rigidbody.velocity = Vector3.zero;
            var delta = mousePos - oldMousePos;
            throwDir = new Vector3(delta.x, 0, delta.y);
            throwDir = Vector3.ClampMagnitude(throwDir, maxThrowDirLength);
            Debug.Log(throwDir);
            rotateDir = new Vector3(delta.y, 0, -delta.x);
            rotateDir = Vector3.ClampMagnitude(rotateDir, maxThrowDirLength);
            triggerThrow = true;
        }
    }
    
    public void SetData(float _moveSpeed, float _maxDirLength, float _throwForce, float _rotateForce, float _expelForce, float _height)
    {
        moveSpeed = _moveSpeed;
        maxThrowDirLength = _maxDirLength;
        throwForce = _throwForce;
        rotateForce = _rotateForce;
        expelForce = _expelForce;
        height = _height;
    }
}

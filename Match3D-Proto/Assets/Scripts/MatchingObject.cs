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
    private int pairId = -1;

    [Space]
    [Header("Movement Variables")]
    public float moveSpeed = 200f;
    public float force = 1.5f;
    public float rotateForce = 1f;
    public float expelForce = 50f;
    public float height = 1f;


    public bool isInRange;
    public bool isChecking;
    void Start()
    {
        matchingManager = MatchingManager.Instance;
        rigidbody = GetComponent<Rigidbody>();
        renderer = GetComponent<Renderer>();
        //renderer.material.DisableKeyword("_EMISSION");
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
                        rigidbody.AddForce(Vector3.forward * expelForce + Vector3.up, ForceMode.Impulse);
                        rigidbody.AddTorque(Vector3.forward * rotateForce);
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
            var throwDir = new Vector3(delta.x, 0, delta.y);
            var rotateDir = new Vector3(delta.y, 0, -delta.x);
            rigidbody.AddForce(throwDir * force, ForceMode.Impulse);
            rigidbody.AddTorque(rotateDir * rotateForce);
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

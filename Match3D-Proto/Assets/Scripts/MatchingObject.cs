using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class MatchingObject : MonoBehaviour
{
    private Rigidbody rb;
    public List<Renderer> renderers = new List<Renderer>();
    private Vector3 oldMousePos;
    private MatchingManager matchingManager;
    public MatchingObject pairedObject;
    public Outline outlineEffect;

    [Space]
    [Header("Movement Variables")]
    public float moveSpeed = 200f;
    public float throwForce = 1.5f;
    public float maxThrowDirLength = 2f;
    public float rotateForce = 1f;
    public float expelForce = 50f;
    public float height = 1f;

    public bool isHolding;
    public bool isInRange;
    public bool isChecking;
    public bool triggerThrow;
    public bool triggerExpel;

    private Vector3 throwDir;
    private Vector3 rotateDir;

    private bool triggerHint;
    void Start()
    {
        matchingManager = MatchingManager.Instance;
        rb = GetComponent<Rigidbody>();

    }

    private void OnValidate()
    {
        outlineEffect = GetComponent<Outline>();
    }

    private void Update()
    {
        if(!isHolding && matchingManager.IsInRange(transform.position))
        {
            //
        }
    }
    private void FixedUpdate()
    {
        if(triggerThrow)
        {
            triggerThrow = false;
            rb.AddForce(throwDir * throwForce, ForceMode.Impulse);
            rb.AddTorque(rotateDir * rotateForce);
        }

        if(triggerExpel)
        {
            triggerExpel = false;
            rb.AddForce(Vector3.forward * expelForce + Vector3.up, ForceMode.Impulse);
            rb.AddTorque(Vector3.forward * rotateForce);
        }
    }

    #region MOVEMENT LOGIC
    void OnMouseDown()
    {
        //AudioManager.instance.PlaySFX("PickUp", 1f);
        //renderer.material.EnableKeyword("_EMISSION");

        rb.isKinematic = true;
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
            OnInRange();
        }
        else
        {
            isChecking = false;
            var mousePos = Input.mousePosition;
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            var delta = mousePos - oldMousePos;
            throwDir = new Vector3(delta.x, 0, delta.y);
            throwDir = Vector3.ClampMagnitude(throwDir, maxThrowDirLength);
            rotateDir = new Vector3(delta.y, 0, -delta.x);
            rotateDir = Vector3.ClampMagnitude(rotateDir, maxThrowDirLength);
            triggerThrow = true;
        }
    }
    
    public void SetData(float _moveSpeed, float _maxDirLength, float _throwForce, float _rotateForce, float _expelForce, float _height, Color outlineColor, float outlineWidth)
    {
        moveSpeed = _moveSpeed;
        maxThrowDirLength = _maxDirLength;
        throwForce = _throwForce;
        rotateForce = _rotateForce;
        expelForce = _expelForce;
        height = _height;
        SetOutline(outlineColor, outlineWidth);
    }
    public void SetOutline(Color color, float width)
    {
        if (outlineEffect == null) return;
        outlineEffect.enabled = false;
        outlineEffect.OutlineColor = color;
        outlineEffect.OutlineWidth = width;
    }
    public void SetHint(bool flag)
    {
        Debug.Log("Set Hint: " + flag);
        for (int i = 0; i < renderers.Count; i++)
        {
            outlineEffect.enabled = flag;
            //renderers[i].material.SetInt("_Hint", flag ? 1 : 0);
        }
    }    

    private void OnInRange()
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
                if (matchingManager.IsMatch(this.gameObject.name))
                {
                    transform.DOMove(matchingManager.rightPos.position, 0.3f);
                    transform.DORotate(Vector3.zero, 0.3f).OnComplete(() => matchingManager.OnMatch());
                    matchingManager.state = MatchingState.Full;
                    matchingManager.rightObject = this;
                }
                else
                {
                    AudioManager.instance.PlaySFX("Error", 0.7f);
                    matchingManager.leftObject.transform.DOScale(matchingManager.leftObject.transform.localScale - Vector3.one * 1.2f, 0.12f).SetLoops(2, LoopType.Yoyo);
                    rb.isKinematic = false;
                    rb.velocity = Vector3.zero;
                    triggerExpel = true;
                }
                break;
            case MatchingState.Full:

                break;
            default:
                break;
        }
    }
}

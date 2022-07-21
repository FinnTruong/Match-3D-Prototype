using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

public enum CustomTransitionType
{
    None,
    Color,
    SpriteSwap,
}

public class AnimatedUIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler {
    public UnityEvent onClick;
    public Image image;
    public string sfxName = "Click";
    public float highlightScale = 0.9f;
    [SerializeField] Color highlightedColor = new Color(200 / 255f, 200 / 255f, 200 / 255f, 1);
    [SerializeField] Color disabledColor = new Color(200 / 255f, 200 / 255f, 200 / 255f, 0.5f);
    public Sprite highlightSprite;

    private Color baseColor;
    private Sprite baseSprite;

    Vector3 onPointerDownScale = Vector3.one;
    float duration = 0.1f;
    private bool interactebleValue = true;
    public bool interactable {
        get => interactebleValue;
        set {
            interactebleValue = value;
        }
    }

    public float shrinkTime = 0.3f;

    Vector3 startScale;

    public CustomTransitionType transition = CustomTransitionType.Color;

    public bool playSound = true;

    public bool isIgnoreAnimate;
    [SerializeField] private bool oneTimeClick = false;

    private void Awake() 
    {
        image = GetComponent<Image>();
        if (image != null) startScale = image.transform.localScale;
        onPointerDownScale = new Vector3(highlightScale * startScale.x, highlightScale * startScale.y, highlightScale * startScale.z);
        if (image != null) {
            baseColor = image.color;
            baseSprite = image.sprite;
        }
    }

    private void OnEnable() {
        if (oneTimeClick) {
            interactable = true;
            image.color = baseColor;
        }
    }
    public void DisableButton() {
        interactable = false;
        image.color = disabledColor;
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (!interactable) return;
        HighlightButton();

        if (isIgnoreAnimate) return;
        transform.DOScale(onPointerDownScale, duration).SetEase(Ease.InBack);
        //LeanTween.scale(this.gameObject, onPointerDownScale, duration)
        //    .setIgnoreTimeScale(true)
        //    .setEaseInBack();
    }
    public void OnPointerUp(PointerEventData eventData) {
        if (!interactable || isIgnoreAnimate) return;
        transform.DOScale(startScale, duration)
            .SetEase(Ease.InBack)
            .OnComplete(() => ResetButton());
        //LeanTween.scale(this.gameObject, startScale, duration)
        //            .setIgnoreTimeScale(true)
        //            .setEaseInBack()
        //            .setOnComplete(_ => ResetButton());
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (!interactable || isIgnoreAnimate) return;
        transform.DOScale(startScale, duration)
            .SetEase(Ease.InBack)
            .OnComplete(() => ResetButton());
        //LeanTween.scale(this.gameObject, startScale, duration)
        //    .setIgnoreTimeScale(true)
        //    .setEaseInBack()
        //    .setOnComplete(_ => ResetButton());
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (!interactable)
            return;
        Vibration.Vibrate(50);
        AudioManager.instance.PlaySFX(sfxName, 1f);
        //if (playSound) {
        //    if (sfx == null)
        //        UISoundManager.Instance.PlayClickAccept();
        //    else
        //        UISoundManager.Instance.PlaySound(sfx);
        //}
        onClick?.Invoke();
        if (oneTimeClick) DisableButton();
    }

    public void HighlightButton() 
{
        if (image != null) {
            switch (transition) {
                case CustomTransitionType.None:
                    break;
                case CustomTransitionType.Color:
                    image.color = highlightedColor;
                    break;
                case CustomTransitionType.SpriteSwap:
                    if (highlightSprite != null)
                        image.sprite = highlightSprite;
                    break;
                default:
                    break;
            }
        }
    }

    public void ResetButton() {
        if (image == null) return;

        switch (transition) {
            case CustomTransitionType.None:
                break;
            case CustomTransitionType.Color:
                image.color = baseColor;
                break;
            case CustomTransitionType.SpriteSwap:
                image.sprite = baseSprite;
                break;
            default:
                break;
        }
    }

    public void Shrink()
    {
        transform.DOScale(Vector3.zero, shrinkTime).SetEase(Ease.InBack);
        //LeanTween.scale(gameObject, Vector3.zero, shrinkTime).setEaseInBack().setIgnoreTimeScale(true);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum MatchingState
{
    Empty,
    Half,
    Full    
}
public class MatchingManager : MonoBehaviour
{
    public static MatchingManager Instance;
    public MatchingState state = MatchingState.Empty;

    public Spawner spawner;
    public Transform leftPos, rightPos, centerPos;
    public MatchingObject leftObject, rightObject;

    public float distanceThreshold = 2f;
    public float animationDuration = 0.2f;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    public void OnMatch()
    {
        if(leftObject == null || rightObject == null)
        {
            Debug.LogError("Match Error");
            return;
        }

        StartCoroutine(StartMatchingAnimation());
    }
    public bool IsMatch(string objectName)
    {
        if (leftObject == null)
            return false;
        if (leftObject.name == objectName)
            return true;
        return false;
    }
    public bool IsInRange(Vector3 position)
    {
        var distance = transform.position - position;
        distance.y = 0;
        return distance.magnitude < distanceThreshold;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, distanceThreshold);
    }

    IEnumerator StartMatchingAnimation()
    {
        var matchingObject_01 = leftObject;
        var matchingObject_02 = rightObject;
        var object01StartScale = leftObject.transform.localScale;
        var object02StartScale = rightObject.transform.localScale;

        state = MatchingState.Empty;
        leftObject = null;
        rightObject = null;

        float percent = 0f;
        while (percent < 1f)
        {
            percent += Time.deltaTime / animationDuration;
            matchingObject_01.transform.localPosition = Vector3.Lerp(leftPos.position, centerPos.position, percent);           
            matchingObject_02.transform.localPosition = Vector3.Lerp(rightPos.position, centerPos.position, percent);
            matchingObject_01.transform.localScale = Vector3.Lerp(object01StartScale, object01StartScale + Vector3.one * 1.5f, percent);
            matchingObject_02.transform.localScale = Vector3.Lerp(object02StartScale, object02StartScale + Vector3.one * 1.5f, percent);
            yield return null;
        }
        yield return new WaitForSecondsRealtime(0.2f);

        percent = 0f;
        object01StartScale = matchingObject_01.transform.localScale;
        object02StartScale = matchingObject_02.transform.localScale;
        while(percent<1f)
        {
            percent += Time.deltaTime / 0.1f;
            matchingObject_01.transform.localScale = Vector3.Lerp(object01StartScale, Vector3.zero, percent);
            matchingObject_02.transform.localScale = Vector3.Lerp(object02StartScale, Vector3.zero, percent);
            yield return null;

        }
        matchingObject_01.gameObject.SetActive(false);
        matchingObject_02.gameObject.SetActive(false);
        spawner.CheckCompleteLevel();
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spawner : MonoBehaviour
{
    public MatchingObject[] objectPrefabs;
    [Header("Level Variables")]
    public int objectCount = 25;
    public int pairCount = 5;
    public float minX, maxX, minY, maxY, minZ, maxZ;
    private Transform objectHolder;
    public List<MatchingObject> activeObjects = new List<MatchingObject>();
    private List<int> selectedIndex = new List<int>();
    public MatchingManager matchingManager;

    [Space]
    [Header("Matching Object Attributes")]
    public float moveSpeed = 200f;
    public float throwForce = 1.5f;
    public float maxThrowDirLength = 2f;
    public float rotateForce = 1f;
    public float expelForce = 50f;
    public float height = 1f;

    public MatchingObject highlightObject_01;
    public MatchingObject highlightObject_02;
    public float hintTimer;
    public float hintTime = 5f;

    public UnityEvent onCompleteEvent;
    private void Start()
    {
        SpawnObjects();
    }

    private void Update()
    {
        if (hintTimer > 0)
        {
            hintTimer -= Time.deltaTime;
            if (hintTimer <= 0) 
            {
                HideHint();
            }
        }
    }
    private void Init()
    {
        string holderName = "Generated Objects";
        if (transform.Find(holderName))
            DestroyImmediate(transform.Find(holderName).gameObject);
        selectedIndex.Clear();
        activeObjects = new List<MatchingObject>();
        objectHolder = new GameObject(holderName).transform;
        objectHolder.parent = transform;
    }

    public void SpawnObjects()
    {
        Init();
        matchingManager.ResetState();
        highlightObject_01 = null;
        highlightObject_02 = null;
        for (int i = 0; i < objectPrefabs.Length; i++)
        {
            if (i > objectCount)
                return;
            for (int j = 0; j < Random.Range(1, pairCount + 1); j++) 
            {
                int randomIndex;

                do
                {
                    randomIndex = Random.Range(0, objectPrefabs.Length - 1);
                }
                while (selectedIndex.Contains(randomIndex));
                selectedIndex.Add(randomIndex);

                var matchingObject_01 = Instantiate(objectPrefabs[randomIndex], GetRandomPos(), Random.rotation, objectHolder);
                var matchingObject_02 = Instantiate(objectPrefabs[randomIndex], GetRandomPos(), Random.rotation, objectHolder);
                matchingObject_01.SetData(moveSpeed, maxThrowDirLength,throwForce, rotateForce, expelForce, height);
                matchingObject_02.SetData(moveSpeed, maxThrowDirLength,throwForce, rotateForce, expelForce, height);
                matchingObject_01.pairedObject = matchingObject_02;
                matchingObject_02.pairedObject = matchingObject_01;
                activeObjects.Add(matchingObject_01);
                activeObjects.Add(matchingObject_02);

            }           
        }


    }

    private Vector3 GetRandomPos()
    {
        return new Vector3((int)Random.Range(minX, maxX), Random.Range(minY, maxY), (int)Random.Range(minZ, maxZ));
    }

    public void CheckCompleteLevel()
    {
        if (activeObjects.Count > 0)
            return;
        Debug.Log("Complete");
        AudioManager.instance.PlaySFX("Complete", 0.8f);
        onCompleteEvent?.Invoke();
    }
    
    public void ShowHint()
    {
        if (hintTimer > 0)
            return;
        GetRandomActivePair();
        hintTimer = hintTime;
    }

    public void HideHint()
    {
        if (highlightObject_01 == null || highlightObject_02 == null)
            return;
        highlightObject_01.SetHint(false);
        highlightObject_02.SetHint(false);
        highlightObject_01 = null;
        highlightObject_02 = null;
    }

    private void GetRandomActivePair()
    {
        highlightObject_01 = null;
        highlightObject_02 = null;
        int randIndex = Random.Range(0, activeObjects.Count - 1);
        highlightObject_01 = activeObjects[randIndex];
        highlightObject_02 = highlightObject_01.pairedObject;
        highlightObject_01.SetHint(true);
        highlightObject_02.SetHint(true);
    }
}

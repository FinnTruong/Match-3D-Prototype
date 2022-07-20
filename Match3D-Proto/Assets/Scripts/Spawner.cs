using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public MatchingObject[] objectPrefabs;
    public int objectCount = 25;
    public int pairCount = 5;
    public float minX, maxX, minY, maxY, minZ, maxZ;
    private Transform objectHolder;
    public List<MatchingObject> activeObjects = new List<MatchingObject>();
    private List<int> selectedIndex = new List<int>();


    private void Start()
    {
        SpawnObjects();
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
        for (int i = 0; i < activeObjects.Count; i++)
        {
            if (activeObjects[i].gameObject.activeSelf)
                return;
        }
        Debug.Log("Complete");
        SpawnObjects();
    }

}

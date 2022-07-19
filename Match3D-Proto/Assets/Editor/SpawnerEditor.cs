using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Spawner))]
public class SpawnerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Spawner spawner = target as Spawner;
        if(DrawDefaultInspector())
        {
            spawner.SpawnObjects();
        }

        if(GUILayout.Button("Spawn Objects"))
        {
            spawner.SpawnObjects();
        }
    }
}

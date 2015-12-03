using UnityEngine;
using System.Collections;
using UnityEditor;

public class SpawnColliders : ScriptableWizard
{
    [MenuItem("Custom tools/CreateColliders")]
    static void Create()
    {
        ScriptableWizard.DisplayWizard<SpawnColliders>("Colliders", "Create");
    }

    public Transform parent;
    public GameObject prefab;

    void OnWizardCreate()
    {
        for(int i = 0; i < 10; i++)
            for (int j = 0; j < 10; j++)
            {
                var col = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                col.transform.parent = parent;
                col.transform.position = new Vector3(i * 640f - 3200f + 320f, 0f, j * 640f - 3200f + 320f);
            }

    }
}

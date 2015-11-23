using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class TerrainGenerator : MonoBehaviour
{
    public Texture2D heightMap;
    public Transform player;
    public Material level1Material;
    public Material level2Material;

    IEnumerator Start()
    {
        var colors = heightMap.GetPixels32();
        int width = 100;
        int depth = 100;

        GameObject[] children = new GameObject[width * depth];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < depth; j++)
            {
                var elevationLevel = GetElevation((int) (colors[i * heightMap.width + j].r));

                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.isStatic = true;
                cube.transform.parent = this.transform;
                cube.GetComponent<MeshRenderer>().material = elevationLevel == 1 ? level1Material : level2Material;
                //cube.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0f, 1f), 0f, 0f, 1f);

                cube.transform.localScale = new Vector3(1f, elevationLevel, 1f);
                cube.transform.position = new Vector3(i, elevationLevel / 2f , j);

                children[i*width + j] = cube;
            }
        }
        var meshBaker = GetComponent<MB3_MultiMeshBaker>();
        meshBaker.AddDeleteGameObjects(children, null, true);
        foreach (var mesh in GetComponentsInChildren<MeshRenderer>())
        {
            Destroy(mesh);
            Destroy(mesh.GetComponent<MeshFilter>());
        }
        meshBaker.Apply();

        //StaticBatchingUtility.Combine(children, this.gameObject);

        yield return new WaitForEndOfFrame();
    }

    private int GetElevation(int heightMapValue)
    {
        switch (heightMapValue)
        {
            case 255:
                return 1;
            case 229:
                return 2;
            case 204:
                return 3;
            case 179:
                return 4;
            case 153:
                return 5;
            default:
                Debug.LogError("Invalid elevation level");
                return 1;
        }
    }
}

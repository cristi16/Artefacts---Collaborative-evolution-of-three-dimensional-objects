using UnityEngine;
using System.Collections;

public class ProceduralMesh : MonoBehaviour
{

    void Start()
    {

    }

    void OnDrawGizmosSelected()
    {
        var vertices = GetComponent<MeshFilter>().mesh.vertices;
        var normals = GetComponent<MeshFilter>().mesh.normals;

        Gizmos.color = Color.green;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawLine(vertices[i], vertices[i] + normals[i] * 2f);
        }
    }
}

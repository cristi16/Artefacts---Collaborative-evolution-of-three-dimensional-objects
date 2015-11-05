using UnityEngine;
using System.Collections;

public class ProceduralMesh : MonoBehaviour
{
    public bool showGizmos = true; 

    void Start()
    {

    }

    void OnDrawGizmosSelected()
    {
        if(!showGizmos) return;

        var vertices = GetComponent<MeshFilter>().mesh.vertices;
        var normals = GetComponent<MeshFilter>().mesh.normals;

        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawLine(vertices[i], vertices[i] + normals[i] * 2f);
        }
    }
}

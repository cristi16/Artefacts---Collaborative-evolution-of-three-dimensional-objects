using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using SharpNeat.Core;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Must;

[Serializable]
public class VoxelVolume
{
    public int width = 16;
    public int height = 16;
    public int length = 16;
}

public class MeshEvolver : MonoBehaviour
{
    public enum TestType { DistanceToCenter, Sphere, Box, Combined}

    public float RotationSpeed = 30f;
    public VoxelVolume m_voxelVolume;
    public TestType testType;
    public bool showGizmos = false;
    public bool showNeatOutput;

    private const int k_numberOfInputs = 4;
    private const int k_numberOfOutputs = 4;

    private MeshEvolutionExperiment m_experiment;
    private NeatInteractiveEvolutionAlgorithm<NeatGenome> m_evolutionaryAlgorithm;

    private GameObject m_meshGameObject;
    private Color[,,] colorOutput;
    private float[,,] meshFillOutput;
    private float minFill;
    private float maxFill;

    void Start () 
	{
	    m_experiment = new MeshEvolutionExperiment();
        
	    XmlDocument xmlConfig = new XmlDocument();
	    TextAsset textAsset = (TextAsset)Resources.Load("MeshEvolutionExperiment.config");
	    xmlConfig.LoadXml(textAsset.text);
	    m_experiment.Initialize("Mesh Evolution", xmlConfig.DocumentElement, k_numberOfInputs, k_numberOfOutputs);

	    m_evolutionaryAlgorithm = m_experiment.CreateEvolutionAlgorithm(1);
	    m_evolutionaryAlgorithm.UpdateEvent = InteractiveEvolutionListener;

        //Winding order of triangles use 2,1,0 or 0,1,2
        MarchingCubes.SetWindingOrder(0, 1, 2);

        //Set the mode used to create the mesh
        //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface
        MarchingCubes.SetModeToCubes();

        m_meshGameObject = new GameObject("Mesh");
        m_meshGameObject.AddComponent<MeshFilter>();
        m_meshGameObject.AddComponent<MeshRenderer>();
        m_meshGameObject.AddComponent<ProceduralMesh>();
        m_meshGameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
        Camera.main.GetComponent<CameraMouseOrbit>().target = m_meshGameObject.transform;

	    StartCoroutine(RotateMesh());
	}

    void InteractiveEvolutionListener()
    {
        Debug.Log("Current generation: " + m_evolutionaryAlgorithm.CurrentGeneration);
    }

	void Update () 
	{
	    if (Input.GetKey(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
	    {
	        m_evolutionaryAlgorithm.EvolveOneStep();
	        var phenom = m_experiment.GenomeDecoder.Decode(m_evolutionaryAlgorithm.GenomeList[0]);

            var voxels = new float[m_voxelVolume.width, m_voxelVolume.height, m_voxelVolume.length];
            colorOutput = new Color[m_voxelVolume.width, m_voxelVolume.height, m_voxelVolume.length];
            meshFillOutput = new float[m_voxelVolume.width, m_voxelVolume.height, m_voxelVolume.length];

            for (int x = 0; x < m_voxelVolume.width; x++)
            {
                for (int y = 0; y < m_voxelVolume.height; y++)
                {
                    for (int z = 0; z < m_voxelVolume.length; z++)
                    {
                        ISignalArray inputArr = phenom.InputSignalArray;
                        inputArr[0] = (float)x /m_voxelVolume.width * 2 - 1;
                        inputArr[1] = (float)y /m_voxelVolume.height * 2 - 1;
                        inputArr[2] = (float)z /m_voxelVolume.length * 2 - 1;

                        var sphereDistance = DistanceFunctions.SphereDistance(x, y, z, m_voxelVolume, m_voxelVolume.width / 3f);

                        var boxSize = new Vector3(6, m_voxelVolume.height / 6f, m_voxelVolume.length / 5f);
                        var boxDistance = DistanceFunctions.BoxDistance(x, y, z, m_voxelVolume, boxSize);

                        switch (testType)
                        {
                            case TestType.Sphere:
                                inputArr[3] = sphereDistance;
                                break;
                            case TestType.Box:
                                inputArr[3] = boxDistance;
                                break;
                            case TestType.Combined:
                                inputArr[3] = Mathf.Min(sphereDistance, boxDistance);
                                break;
                            case TestType.DistanceToCenter:
                                inputArr[3] = DistanceFunctions.DistanceToCenter(x, y, z, m_voxelVolume);
                                break;
                        }

                        //inputArr[3] = DistanceFunctions.DistanceToCenter(x, y, z, m_voxelVolume);

                        phenom.Activate();

                        ISignalArray outputArr = phenom.OutputSignalArray;

                        // 0 - means the voxel will be filled, any other value means it won't be filled
                        voxels[x, y, z] = Mathf.Abs((float) outputArr[0]) > 0.3f ? 0f : 1f;
                        meshFillOutput[x, y, z] = (float) outputArr[0];

                        float r = Mathf.Max(0, (float)outputArr[1]);
                        float g = Mathf.Max(0, (float)outputArr[2]);
                        float b = Mathf.Max(0, (float)outputArr[3]);
                        colorOutput[x, y, z] = new Color(r * 2, g * 3, b * 4);
                    }
                }
            }
	        minFill = float.MaxValue;
	        maxFill = float.MinValue;
	        foreach (var fill in meshFillOutput)
	        {
	            if (fill < minFill)
	                minFill = fill;
	            if (fill > maxFill)
	                maxFill = fill;
	        }
            Debug.Log("Max fill: " + maxFill);
            Debug.Log("Min fill: " + minFill);

            Mesh mesh = MarchingCubes.CreateMesh(voxels);

            List<Color> vertexColors = new List<Color>();
	        foreach (var vertex in mesh.vertices)
            {
                var zeroBasedVertex = vertex + new Vector3(m_voxelVolume.width, m_voxelVolume.height, m_voxelVolume.length);
                vertexColors.Add(colorOutput[GetIndex(zeroBasedVertex.x), GetIndex(zeroBasedVertex.y), GetIndex(zeroBasedVertex.z)]);
            }
	        mesh.colors = vertexColors.ToArray();

	        mesh.RecalculateNormals();
	        if (mesh.vertices.Length > 0)
	        {
	            Vector3 furthestZVertex = mesh.vertices[0];
	            foreach (var vertex in mesh.vertices)
	            {
	                if (vertex.z > furthestZVertex.z)
	                    furthestZVertex = vertex;
	            }
	            Debug.Log(mesh.normals[mesh.vertices.ToList().IndexOf(furthestZVertex)].z > 0);
	        }

	        //The diffuse shader wants uvs so just fill with a empty array, they're not actually used
	        mesh.uv = new Vector2[mesh.vertices.Length];
	        mesh.Optimize();

            m_meshGameObject.GetComponent<MeshFilter>().mesh = mesh;
	        
	    }
	}

    [ContextMenu("Flip winding order")]
    void InvertWindingOrder()
    {
        Mesh mesh = m_meshGameObject.GetComponent<MeshFilter>().mesh;
        int[] triangles = mesh.triangles;
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int temp = triangles[i + 1];
            triangles[i + 1] = triangles[i + 2];
            triangles[i + 2] = temp;
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private int GetIndex(float value)
    {
        return Mathf.Abs(Mathf.RoundToInt(value))%(m_voxelVolume.width - 1);
    }

    private IEnumerator RotateMesh()
    {
        while (true)
        {
            m_meshGameObject.transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime, Space.World);
            yield return null;
        }    
    }

    private void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(m_voxelVolume.width, m_voxelVolume.height, m_voxelVolume.length));

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(7 * 2, m_voxelVolume.height / 3f, m_voxelVolume.length / 2.5f));

            var color = Color.cyan;
            ;
            color.a = 0.5f;
            Gizmos.color = color;
            Gizmos.DrawSphere(Vector3.zero, m_voxelVolume.width / 3f);
        }

        if (showNeatOutput && meshFillOutput != null)
        {
            for (int i0 = 0; i0 < meshFillOutput.GetLength(0); i0++)
                for (int i1 = 0; i1 < meshFillOutput.GetLength(1); i1++)
                    for (int i2 = 0; i2 < meshFillOutput.GetLength(2); i2++)
                    {
                        var fill = meshFillOutput[i0, i1, i2];
                        Gizmos.color = new Color(fill - minFill, fill - minFill, fill - minFill)/(maxFill - minFill);
                        Gizmos.DrawWireCube(new Vector3(i0 - m_voxelVolume.width / 2f, i1 - m_voxelVolume.height / 2f, i2 - m_voxelVolume.length / 2f ), Vector3.one);
                    }
        }
    }
}

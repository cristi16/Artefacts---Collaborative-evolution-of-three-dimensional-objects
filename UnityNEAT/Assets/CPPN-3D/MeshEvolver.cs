using UnityEngine;
using System.Collections;
using System.Xml;
using SharpNeat.Core;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using UnityEngine.Assertions;
using UnityEngine.Assertions.Must;

public class MeshEvolver : MonoBehaviour
{

    private const int k_numberOfInputs = 4;
    private const int k_numberOfOutputs = 1;

    private MeshEvolutionExperiment m_experiment;
    private NeatInteractiveEvolutionAlgorithm<NeatGenome> m_evolutionaryAlgorithm;

    private GameObject m_mesh;
	
	void Start () 
	{
	    m_experiment = new MeshEvolutionExperiment();

	    XmlDocument xmlConfig = new XmlDocument();
	    TextAsset textAsset = (TextAsset)Resources.Load("MeshEvolutionExperiment.config");
	    xmlConfig.LoadXml(textAsset.text);
	    m_experiment.Initialize("Mesh Evolution", xmlConfig.DocumentElement, k_numberOfInputs, k_numberOfOutputs);

	    m_evolutionaryAlgorithm = m_experiment.CreateEvolutionAlgorithm(1);
	    m_evolutionaryAlgorithm.UpdateEvent = GenerationalEvolutionListener;

        //Winding order of triangles use 2,1,0 or 0,1,2
        MarchingCubes.SetWindingOrder(0, 1, 2);

        //Set the mode used to create the mesh
        //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface
        MarchingCubes.SetModeToCubes();

        m_mesh = new GameObject("Mesh");
        m_mesh.AddComponent<MeshFilter>();
        m_mesh.AddComponent<MeshRenderer>();
        m_mesh.GetComponent<Renderer>().material = new Material(Shader.Find("Legacy Shaders/Diffuse"));
    }

    void GenerationalEvolutionListener()
    {
        Debug.Log(m_evolutionaryAlgorithm.CurrentGeneration);
    }

	void Update () 
	{
	    if (Input.GetKey(KeyCode.Space))
	    {
	        m_evolutionaryAlgorithm.EvolveOneStep();
	        var phenom = m_experiment.GenomeDecoder.Decode(m_evolutionaryAlgorithm.GenomeList[0]);

            var width = 16;
            var height = 16;
            var length = 16;

            var voxels = new float[width, height, length];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < length; z++)
                    {
                        ISignalArray inputArr = phenom.InputSignalArray;
                        inputArr[0] = (float)x / width * 2 - 1;
                        inputArr[1] = (float)y / height * 2 - 1;
                        inputArr[2] = (float)z / length * 2 - 1;
                        
                        var point = new Vector3(Mathf.Abs(x - width/2f), Mathf.Abs(y - height/2f), Mathf.Abs(z - length/2f));
                        var sphere = point.sqrMagnitude / (width / 3f * width / 3f);
                        if (sphere > 1)
                            sphere *= 100f;

                        var bounds = new Vector3(width/4f, height/6f, length/5f);
                        var distance = bounds - point;
                        var max = Mathf.Max(distance.x, distance.y, distance.z);
                        if (max == distance.x)
                            max /= bounds.x;
                        if (max == distance.y)
                            max /= bounds.y;
                        if (max == distance.z)
                            max /= bounds.z;

                        if (point.x > bounds.x || point.y > bounds.y || point.z > bounds.z)
                            inputArr[3] = 100f;
                        else
                            inputArr[3] = max;

                        inputArr[3] = Mathf.Min((float)inputArr[3], sphere);
                        phenom.Activate();

                        ISignalArray outputArr = phenom.OutputSignalArray;

                        // 0 - means the voxel will be filled, any other value means it won't be filled
                        voxels[x, y, z] = (float) outputArr[0] > 0.3f ? 0f : 1f;
                    }
                }
            }

            Mesh mesh = MarchingCubes.CreateMesh(voxels);

            //The diffuse shader wants uvs so just fill with a empty array, there not actually used
            mesh.uv = new Vector2[mesh.vertices.Length];
            mesh.RecalculateNormals();

            
            m_mesh.GetComponent<MeshFilter>().mesh = mesh;
        }
	}
}

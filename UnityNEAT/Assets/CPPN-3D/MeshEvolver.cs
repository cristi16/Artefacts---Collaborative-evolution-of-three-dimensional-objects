using System;
using System.IO;
using UnityEngine;
using System.Xml;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;

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

    public VoxelVolume m_voxelVolume;
    public TestType testType;
    public bool showGizmos = false;
    public bool showNeatOutput;

    private const int k_numberOfInputs = 4;
    private const int k_numberOfOutputs = 1;

    private MeshEvolutionExperiment m_experiment;
    private NeatInteractiveEvolutionAlgorithm<NeatGenome> m_evolutionaryAlgorithm;

    private GameObject m_meshGameObject;
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
        MarchingCubes.SetTarget(0.1f);

        //Set the mode used to create the mesh
        //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface
        MarchingCubes.SetModeToCubes();

        m_meshGameObject = new GameObject("Mesh");
        m_meshGameObject.AddComponent<MeshFilter>();
        m_meshGameObject.AddComponent<MeshRenderer>();
        m_meshGameObject.AddComponent<ProceduralMesh>();
        m_meshGameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
        Camera.main.GetComponent<CameraMouseOrbit>().target = m_meshGameObject.transform;
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

            //var doc = NeatGenomeXmlIO.Save(m_evolutionaryAlgorithm.GenomeList[0], true);
            //var byteCount = System.Text.ASCIIEncoding.ASCII.GetByteCount(doc.OuterXml);
            //Debug.LogWarning("Byte count: " + byteCount);

            //NeatGenomeXmlIO.ReadGenome(XmlReader.Create(new StringReader(doc.OuterXml)), true);         

            var phenom = m_experiment.GenomeDecoder.Decode(m_evolutionaryAlgorithm.GenomeList[0]);

            var voxels = new float[m_voxelVolume.width, m_voxelVolume.height, m_voxelVolume.length];
            meshFillOutput = new float[m_voxelVolume.width, m_voxelVolume.height, m_voxelVolume.length];

            for (int x = 0; x < m_voxelVolume.width; x++)
            {
                for (int y = 0; y < m_voxelVolume.height; y++)
                {
                    for (int z = 0; z < m_voxelVolume.length; z++)
                    {
                        ISignalArray inputArr = phenom.InputSignalArray;
                        inputArr[0] = Mathf.Abs((float)x / (m_voxelVolume.width - 1) * 2 - 1);
                        inputArr[1] = Mathf.Abs((float)y / (m_voxelVolume.height - 1) * 2 - 1);
                        inputArr[2] = Mathf.Abs((float)z / (m_voxelVolume.length - 1) * 2 - 1);

                        var sphereDistance = DistanceFunctions.SphereDistance(x, y, z, m_voxelVolume, m_voxelVolume.width / 3f, 
                            new Vector3(m_voxelVolume.width / 2f, m_voxelVolume.height / 2f, m_voxelVolume.length / 2f));

                        var boxSize = new Vector3(6, m_voxelVolume.height / 6f, m_voxelVolume.length / 5f);
                        var boxDistance = DistanceFunctions.BoxDistance(x, y, z, m_voxelVolume, boxSize,
                            new Vector3(m_voxelVolume.width / 2f, m_voxelVolume.height / 2f, m_voxelVolume.length / 2f));

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

                        phenom.Activate();

                        ISignalArray outputArr = phenom.OutputSignalArray;

                        // for smoother surfaces, don't modify output

                        voxels[x, y, z] = (float)outputArr[0];// > 0.3f ? 0f : 1f;

                        meshFillOutput[x, y, z] = (float)outputArr[0];

                        if(x == 0 || x == m_voxelVolume.width-1  || y == 0 || y == m_voxelVolume.height-1 || z == 0 || z == m_voxelVolume.length-1)
                                voxels[x, y, z] = -1f;
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

            if(Mathf.Approximately(minFill, maxFill)) return;

            //MarchingCubes.SetTarget(minFill + (maxFill - minFill) /2f);

            for (int index00 = 1; index00 < voxels.GetLength(0) - 1; index00++)
                for (int index01 = 1; index01 < voxels.GetLength(1) - 1; index01++)
                    for (int index02 = 1; index02 < voxels.GetLength(2) - 1; index02++)
                    {
                        if (Mathf.Approximately(meshFillOutput[0, 0, 0], maxFill))
                            voxels[index00, index01, index02] = minFill + (maxFill - voxels[index00, index01, index02]);

                        voxels[index00, index01, index02] = voxels[index00, index01, index02] < minFill + (maxFill - minFill) / 2f ? 0f : 1f;
                    }

            Mesh mesh = MarchingCubes.CreateMesh(voxels);

	        mesh.RecalculateNormals();

	        //The diffuse shader wants uvs so just fill with a empty array, they're not actually used
	        mesh.uv = new Vector2[mesh.vertices.Length];
            // optimize mesh to it renders faster
	        mesh.Optimize();
            // destroy mesh object to free up memory
            GameObject.DestroyImmediate(m_meshGameObject.GetComponent<MeshFilter>().mesh);

            m_meshGameObject.GetComponent<MeshFilter>().mesh = mesh;        
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

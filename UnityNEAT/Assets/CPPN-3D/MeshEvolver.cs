using System;
using System.IO;
using UnityEngine;
using System.Xml;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;

public class MeshEvolver : MonoBehaviour
{

    public ArtefactEvaluator.VoxelVolume m_voxelVolume;
    public ArtefactEvaluator.InputType InputType;
    public bool showGizmos = false;
    public bool showNeatOutput;

    private const int k_numberOfInputs = 4;
    private const int k_numberOfOutputs = 1;

    private MeshEvolutionExperiment m_experiment;
    private NeatInteractiveEvolutionAlgorithm<NeatGenome> m_evolutionaryAlgorithm;

    private GameObject m_meshGameObject;
    ArtefactEvaluator.EvaluationInfo evaluationInfo;

    void Start () 
	{
	    m_experiment = new MeshEvolutionExperiment();
        
	    XmlDocument xmlConfig = new XmlDocument();
	    TextAsset textAsset = (TextAsset)Resources.Load("MeshEvolutionExperiment.config");
	    xmlConfig.LoadXml(textAsset.text);
	    m_experiment.Initialize("Mesh Evolution", xmlConfig.DocumentElement, k_numberOfInputs, k_numberOfOutputs);

	    m_evolutionaryAlgorithm = m_experiment.CreateEvolutionAlgorithm(1);
	    m_evolutionaryAlgorithm.UpdateEvent = InteractiveEvolutionListener;


        m_meshGameObject = new GameObject("Mesh");
        m_meshGameObject.AddComponent<MeshFilter>();
        m_meshGameObject.AddComponent<MeshRenderer>();
        m_meshGameObject.AddComponent<ProceduralMesh>();
        m_meshGameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
        Camera.main.GetComponent<CameraMouseOrbit>().target = m_meshGameObject.transform;
	}

    void OnValidate()
    {
        ArtefactEvaluator.DefaultInputType = InputType;
    }

    void InteractiveEvolutionListener()
    {
        Debug.Log("Current generation: " + m_evolutionaryAlgorithm.CurrentGeneration);
        Debug.Log("Connections: " + (m_evolutionaryAlgorithm.GenomeList[0].ConnectionGeneList.Count));
        Debug.Log("Neurons: " + (m_evolutionaryAlgorithm.GenomeList[0].NeuronGeneList.Count));
    }

    void Update () 
	{
	    if (Input.GetKey(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
	    {
	        m_evolutionaryAlgorithm.EvolveOneStep();

            var phenome = m_experiment.GenomeDecoder.Decode(m_evolutionaryAlgorithm.GenomeList[0]);

	        
	        Mesh mesh = ArtefactEvaluator.Evaluate(phenome, m_voxelVolume, out evaluationInfo);

	        mesh.RecalculateNormals();
	        //The diffuse shader wants uvs so just fill with a empty array, they're not actually used
	        mesh.uv = new Vector2[mesh.vertices.Length];
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
            color.a = 0.5f;
            Gizmos.color = color;
            Gizmos.DrawSphere(Vector3.zero, m_voxelVolume.width / 3f);
        }

        if (showNeatOutput && evaluationInfo.cleanOutput != null)
        {
            var minFill = evaluationInfo.minOutputValue;
            var maxFill = evaluationInfo.maxOutputValue;

            for (int i0 = 0; i0 < evaluationInfo.cleanOutput.GetLength(0); i0++)
                for (int i1 = 0; i1 < evaluationInfo.cleanOutput.GetLength(1); i1++)
                    for (int i2 = 0; i2 < evaluationInfo.cleanOutput.GetLength(2); i2++)
                    {
                        var fill = evaluationInfo.cleanOutput[i0, i1, i2];
                        Gizmos.color = new Color(fill - minFill, fill - minFill, fill - minFill)/(maxFill - minFill);
                        Gizmos.DrawWireCube(new Vector3(i0 - m_voxelVolume.width / 2f, i1 - m_voxelVolume.height / 2f, i2 - m_voxelVolume.length / 2f ), Vector3.one);
                    }
        }
    }
}

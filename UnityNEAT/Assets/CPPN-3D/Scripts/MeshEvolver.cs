using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;
using SharpNeat.Network;

public class MeshEvolver : MonoBehaviour
{
    public ArtefactEvaluator.VoxelVolume m_voxelVolume;
    public ArtefactEvaluator.InputType InputType;
    public bool showGizmos = false;
    public bool showNeatOutput;
    public bool showDistanceToCenter;

    public Material standardMaterial;
    public Material triplanarTexturingMaterial;
    public Material triplanarColoringMaterial;

    public AnimationCurve sineCurve;

    private const int k_numberOfInputs = 4;
    private const int k_numberOfOutputs = 1;

    private GameObject m_meshGameObject;
    private EvolutionHelper evolutionHelper;
    private NeatGenome currentGenome;
    private NeatGenomeDecoder genomeDecoder;
    private ArtefactEvaluator.EvaluationInfo evaluationInfo;

    void Start ()
	{ 
        evolutionHelper = new EvolutionHelper(k_numberOfInputs, k_numberOfOutputs);
        currentGenome = evolutionHelper.CreateInitialGenome();
        genomeDecoder = new NeatGenomeDecoder(NetworkActivationScheme.CreateAcyclicScheme());
        ArtefactEvaluator.DefaultInputType = InputType;

        //SaveGenome();
        Sine.__DefaultInstance.Curve = sineCurve;

        m_meshGameObject = new GameObject("Mesh");
        m_meshGameObject.AddComponent<MeshFilter>();
        m_meshGameObject.AddComponent<MeshRenderer>();
        m_meshGameObject.AddComponent<ProceduralMesh>();
        m_meshGameObject.GetComponent<Renderer>().material = standardMaterial;
        Camera.main.GetComponent<CameraMouseOrbit>().target = m_meshGameObject.transform;
	}

    void Update () 
	{
	    if (Input.GetKey(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
	    {
	        currentGenome = evolutionHelper.MutateGenome(currentGenome);
            Debug.Log("Current generation: " + currentGenome.BirthGeneration);
            //var byteCount = System.Text.ASCIIEncoding.ASCII.GetByteCount(NeatGenomeXmlIO.Save(currentGenome, true).OuterXml);
            //Debug.LogWarning("Byte count: " + byteCount);

            var phenome = genomeDecoder.Decode(currentGenome);
            
	        Mesh mesh = ArtefactEvaluator.Evaluate(phenome, m_voxelVolume, out evaluationInfo);

	        mesh.RecalculateNormals();
	        //The diffuse shader wants uvs so just fill with a empty array, they're not actually used
	        mesh.uv = new Vector2[mesh.vertices.Length];
	        // destroy mesh object to free up memory
	        GameObject.DestroyImmediate(m_meshGameObject.GetComponent<MeshFilter>().mesh);

	        m_meshGameObject.GetComponent<MeshFilter>().mesh = mesh;
	        //m_meshGameObject.GetComponent<Renderer>().material.color = ArtefactEvaluator.artefactColor;

            //SaveGenome();
	    }
	}

    private void SaveGenome()
    {
        XmlWriterSettings _xwSettings = new XmlWriterSettings();
        _xwSettings.Indent = true;
        using (XmlWriter xw = XmlWriter.Create(Application.persistentDataPath + "/genome.gnm.xml", _xwSettings))
        {
            NeatGenomeXmlIO.WriteComplete(xw, currentGenome, true);
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

        if (showDistanceToCenter && evaluationInfo.distanceToCenter != null)
        {
            var minFill = 0f;
            var maxFill = 1f;

            for (int i0 = 0; i0 < evaluationInfo.distanceToCenter.GetLength(0); i0++)
                for (int i1 = 0; i1 < evaluationInfo.distanceToCenter.GetLength(1); i1++)
                    for (int i2 = 0; i2 < evaluationInfo.distanceToCenter.GetLength(2); i2++)
                    {
                        var fill = evaluationInfo.distanceToCenter[i0, i1, i2];
                        Gizmos.color = new Color(fill - minFill, fill - minFill, fill - minFill) / (maxFill - minFill);
                        Gizmos.DrawWireCube(new Vector3(i0 - m_voxelVolume.width / 2f, i1 - m_voxelVolume.height / 2f, i2 - m_voxelVolume.length / 2f), Vector3.one);
                    }
        }
    }
}

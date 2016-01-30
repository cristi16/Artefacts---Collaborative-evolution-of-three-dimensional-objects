using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;

public class ArtefactGhost : MonoBehaviour
{
    public Material artefactMaterial;

    [HideInInspector]
    public string SerializedGenome;

    public const float k_artefactScale = 0.1326183f;
    public const float k_seedScale = 0.05f;
    private static ArtefactEvaluator.VoxelVolume m_voxelVolume = new ArtefactEvaluator.VoxelVolume() { width = 16, height = 16, length = 16 };

    void Start()
    {
        if (SerializedGenome == string.Empty)
        {
            Debug.LogError("Spawned artefact without genome!");
        }

        // Deserialize genome
        Profiler.BeginSample("Deserialize");
        var genome = NeatGenomeXmlIO.ReadGenome(XmlReader.Create(new StringReader(SerializedGenome)), true);
        Profiler.EndSample();

        // we need to assign genome factory we used for creating the genome
        Profiler.BeginSample("Create genome factory");
        genome.GenomeFactory = EvolutionHelper.Instance.GenomeFactory;
        Profiler.EndSample();

        // Decode phenome from genome
        Profiler.BeginSample("Decode");
        var genomeDecoder = new NeatGenomeDecoder(NetworkActivationScheme.CreateAcyclicScheme());
        var phenome = genomeDecoder.Decode(genome);
        Profiler.EndSample();

        // Evaluate phenome using voxel grid and generate mesh using Marching Cubes algorithm
        Profiler.BeginSample("Evaluation");
        ArtefactEvaluator.EvaluationInfo evaluationInfo;
        var mesh = ArtefactEvaluator.Evaluate(phenome, m_voxelVolume, out evaluationInfo);
        Profiler.EndSample();

        // Add required components in order to render mesh
        Profiler.BeginSample("Display");
        DisplayMesh(mesh);
        Profiler.EndSample();
    }

    void DisplayMesh(Mesh mesh)
    {
        mesh.RecalculateNormals();
        mesh.uv = new Vector2[mesh.vertices.Length];

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<Renderer>().material.color = ArtefactEvaluator.artefactColor;

        //gameObject.AddComponent<MeshCollider>().convex = true;
    }
}

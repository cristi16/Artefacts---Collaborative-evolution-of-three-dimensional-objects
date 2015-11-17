using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;
using UnityEngine.Networking;

public class Artefact : NetworkBehaviour
{
    public Material artefactMaterial;

    [HideInInspector, SyncVar] public string SerializedGenome;

    private static ArtefactEvaluator.VoxelVolume m_voxelVolume = new ArtefactEvaluator.VoxelVolume() { width = 16, height = 16, length = 16 };

    public override void OnStartClient()
    {
        base.OnStartClient();

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

        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>().material = artefactMaterial;
        gameObject.GetComponent<Renderer>().material.color = ArtefactEvaluator.artefactColor;
        gameObject.AddComponent<ProceduralMesh>();

        // Concave collider generation is cool but it is incredibly slow at runtime, especially when generating them for multiple meshes at the same time. 
        // This is also probably slow because our meshes have lots of vertices
        //var concaveCollider = gameObject.AddComponent<ConcaveCollider>();
        //concaveCollider.Algorithm = ConcaveCollider.EAlgorithm.Legacy;
        //concaveCollider.ComputeHulls(null, null);
        gameObject.AddComponent<MeshCollider>().convex = true;

        gameObject.AddComponent<Rigidbody>().mass = 100f;
    }
}

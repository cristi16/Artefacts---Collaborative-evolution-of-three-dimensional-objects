using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using UnityEngine.Networking;

public class Artefact : NetworkBehaviour
{ 
    [HideInInspector, SyncVar] public string SerializedGenome;

    private static ArtefactEvaluator.VoxelVolume m_voxelVolume = new ArtefactEvaluator.VoxelVolume() { width = 16, height = 16, length = 16 };

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (SerializedGenome == String.Empty)
        {
            Debug.LogError("Spawned artefact without genome!");
        }

        // Deserialize genome
        var genome = NeatGenomeXmlIO.ReadGenome(XmlReader.Create(new StringReader(SerializedGenome)), true);
        // we need to assign genome factory we used for creating the genome
        genome.GenomeFactory = EvolutionHelper.CreateGenomeFactory();

        // Decode phenome from genome
        var genomeDecoder = new NeatGenomeDecoder(NetworkActivationScheme.CreateAcyclicScheme());
        var phenome = genomeDecoder.Decode(genome);

        // Evaluate phenome using voxel grid and generate mesh using Marching Cubes algorithm
        ArtefactEvaluator.EvaluationInfo evaluationInfo;
        var mesh = ArtefactEvaluator.Evaluate(phenome, m_voxelVolume, out evaluationInfo);

        // Add required components in order to render mesh
        DisplayMesh(mesh);
    }

    void DisplayMesh(Mesh mesh)
    {
        mesh.RecalculateNormals();
        mesh.uv = new Vector2[mesh.vertices.Length];

        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>();
        gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
        gameObject.AddComponent<ProceduralMesh>();

        var concaveCollider = gameObject.AddComponent<ConcaveCollider>();
        concaveCollider.Algorithm = ConcaveCollider.EAlgorithm.Legacy;
        concaveCollider.ComputeHulls(null, null);

        gameObject.AddComponent<Rigidbody>();
        gameObject.transform.localScale = Vector3.one*0.2f;
        gameObject.transform.position = Vector3.up*10f;
    }
}

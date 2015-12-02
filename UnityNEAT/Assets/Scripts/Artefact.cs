using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml;
using HighlightingSystem;
using SharpNeat.Core;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;
using UnityEngine.Networking;

public class Artefact : NetworkBehaviour
{
    public Material artefactMaterial;

    [HideInInspector, SyncVar] public string SerializedGenome;
    [HideInInspector, SyncVar] public uint GenomeId;

    public const float k_artefactScale = 0.1326183f;
    public const float k_seedScale = 0.05f;
    public const float k_growthMove = 4f;
    public const float k_growthTime = 1.5f;

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

        var localClient = ClientScene.localPlayers[0].gameObject.GetComponent<PlayerNetworkSetup>();
        localClient.CmdSaveArtefactColor(GenomeId, ArtefactEvaluator.artefactColor.r, ArtefactEvaluator.artefactColor.g, ArtefactEvaluator.artefactColor.b);

        if (this.GetType() == typeof (Artefact))
        {
            //StartCoroutine(Grow());
            StartCoroutine(Glow());
        }
    }

    void DisplayMesh(Mesh mesh)
    {
        mesh.RecalculateNormals();
        mesh.uv = new Vector2[mesh.vertices.Length];

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        gameObject.GetComponent<Renderer>().material.color = ArtefactEvaluator.artefactColor;

        // Concave collider generation is cool but it is incredibly slow at runtime, especially when generating them for multiple meshes at the same time. 
        // This is also probably slow because our meshes have lots of vertices
        //var concaveCollider = gameObject.AddComponent<ConcaveCollider>();
        //concaveCollider.Algorithm = ConcaveCollider.EAlgorithm.Legacy;
        //concaveCollider.ComputeHulls(null, null);
        gameObject.AddComponent<MeshCollider>().convex = true;
    }

    IEnumerator Glow()
    {
        GetComponent<Highlighter>().FlashingOn();
        yield return new WaitForSeconds(k_growthTime);
        GetComponent<Highlighter>().FlashingOff();
    }

    IEnumerator Grow()
    {
        var rb = GetComponent<Rigidbody>();

        float timer = 0f;
        var initialScale = transform.localScale;
        while (timer < k_growthTime)
        {
            transform.localScale = Vector3.Lerp(initialScale, Vector3.one*k_artefactScale, timer / k_growthTime);
            rb.MovePosition(transform.position + Vector3.up * Time.deltaTime * k_growthMove);
            timer += Time.deltaTime;
            yield return null;
        }
        rb.MovePosition(transform.position + Vector3.up * Time.deltaTime * k_growthMove);
        transform.localScale = Vector3.one * k_artefactScale;
    }
}
